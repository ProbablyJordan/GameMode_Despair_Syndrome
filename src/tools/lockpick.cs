datablock AudioProfile(DoorPickSound1) {
	fileName = $DS::Path @ "res/sounds/tools/picklock-01.wav";
	description = audioSilent3d;
	preload = true;
};
datablock AudioProfile(DoorPickSound2) {
	fileName = $DS::Path @ "res/sounds/tools/picklock-02.wav";
	description = audioSilent3d;
	preload = true;
};
datablock AudioProfile(DoorPickSound3) {
	fileName = $DS::Path @ "res/sounds/tools/picklock-03.wav";
	description = audioSilent3d;
	preload = true;
};

datablock ItemData(LockpickItem)
{
	shapeFile = $DS::Path @ "res/shapes/tools/satchel.dts";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;

	uiName = "Lockpick";
	// iconName = "Add-Ons/Item_Lockpick/Icon_Lockpick";
	doColorShift = true;
	colorShiftColor = "0.4 0.4 0.4 1";

	itemPropsAlways = false;

	customPickupAlways = false;
	customPickupMultiple = false;

	image = LockpickImage;

	canDrop = true;

	// collisionSFX = LockpickImpactSFX;
	// collisionThreshold = 3;
};

datablock ShapeBaseImageData(LockpickImage)
{
	className = "WeaponImage";
	shapeFile = $DS::Path @ "res/shapes/tools/satchel.dts";

	rotation = eulerToMatrix("0 0 90");

	item = LockpickItem;
	armReady = true;

	doColorShift = LockpickItem.doColorShift;
	colorShiftColor = LockpickItem.colorShiftColor;
};

function LockpickImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this, %obj, %slot);
}

function Player::lockpickDoAfter(%this, %time, %target, %ticks, %prevPosition, %done)
{
	cancel(%this.lockpickDoAfter);
	if (%ticks $= "")
		%ticks = 1;

	if (%prevPosition !$= "" && %this.getPosition() !$= %prevPosition || %this.getMountedImage(0) != LockpickImage.getID() || !isObject(%target))
	{
		if (isObject(%this.client))
			%this.client.centerPrint("\c6Lockpick action interrupted!", 2);
		return -1; //Fail
	}
	if (%done >= %ticks)
	{
		%target.lockState = false;
		serverPlay3d(DoorUnlockSound, %target.getWorldBoxCenter(), 1);
		%this.playThread(2, "rotCW");
		if (isObject(%this.client))
			%this.client.centerPrint("\c6You unlock the door.", 2);
		return 1; //Success
	}
	%this.playThread(2, "shiftRight");
	serverPlay3d(DoorPickSound @ getRandom(1,3), %target.getWorldBoxCenter(), 1);
	for (%i = 0; %i < %done % 4; %i++)
	{
		%dots = %dots @ ".";
	}
	%this.client.centerPrint("\c6Unlocking the door" @ %dots, 2);
	%prevPosition = %this.getPosition();
	%timefraction = mFloor(%time/%ticks);
	%this.lockpickDoAfter = %this.schedule(%timefraction, lockpickDoAfter, %time, %target, %ticks, %prevPosition, %done++);
}

package LockpickPackage
{
	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);

		if (!%state || %obj.getMountedImage(0) != LockpickImage.getID() || %slot != 0)
			return;

		%a = %obj.getEyePoint();
		%b = vectorAdd(%a, vectorScale(%obj.getEyeVector(), 4));

		%mask =
			$TypeMasks::FxBrickObjectType |
			$TypeMasks::PlayerObjectType;

		%ray = containerRayCast(%a, %b, %mask, %obj);

		if (!%ray || %ray.lockId $= "")
			return;

		%props = %obj.getItemProps();

		%data = %ray.getDataBlock();

		if (%data.isDoor && %data.isOpen || isEventPending(%obj.lockpickDoAfter))
			return;

		if (%slot == 0) //do the thang
		{
			if (%ray.lockState)
			{
				serverPlay3d(DoorJiggleSound, %ray.getWorldBoxCenter(), 1);
				%obj.playThread(2, "activate");
				%obj.lockpickDoAfter(10000, %ray, 10);
			}
			else
			{
				serverPlay3d(DoorJiggleSound, %ray.getWorldBoxCenter(), 1);
				%obj.playThread(2, "shiftRight");
				if (isObject(%obj.client))
					%obj.client.centerPrint("\c6The door is already unlocked.", 2);
			}
		}
	}
};

activatePackage("LockpickPackage");
