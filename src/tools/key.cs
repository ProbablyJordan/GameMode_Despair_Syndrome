datablock ItemData(KeyItem)
{
	shapeFile = "Add-Ons/Item_Key/keya.dts";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;

	uiName = "Key";
	iconName = "Add-Ons/Item_Key/Icon_KeyA";
	doColorShift = true;
	colorShiftColor = "1 1 0 1";

	itemPropsClass = "KeyProps";
	itemPropsAlways = true;

	customPickupAlways = true;
	customPickupMultiple = true;

	image = KeyImage;
};

function KeyProps::onAdd(%this)
{
	%this.name = "Key";
	%this.id = ""; // Can't open anything
}

datablock ShapeBaseImageData(KeyImage)
{
	className = "WeaponImage";
	shapeFile = "Add-Ons/Item_Key/keya.dts";

	item = KeyItem;
	armReady = true;

	doColorShift = KeyItem.doColorShift;
	colorShiftColor = KeyItem.colorShiftColor;
};

function KeyImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this, %obj, %slot);
	%props = %obj.getItemProps();

	if (isObject(%obj.client))
		%obj.client.centerPrint("\c3" @ %props.name @ "\n", 2.5);
}

package KeyPackage
{
	function FxDTSBrick::door(%this, %state, %client)
	{
		if (%this.lockId !$= "" && %this.lockState)
			%client.centerPrint("\c2The door is locked.", 2);
		else
			Parent::door(%this, %state, %client);
	}

	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);

		if (!%state || %obj.getMountedImage(0) != KeyImage.getID() || (%slot != 0 && %slot != 4))
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

		if (%ray.lockId !$= %props.id)
		{
			if (isObject(%obj.client))
				%obj.client.centerPrint("\c6The key doesn't fit.", 2);

			return;
		}

		%data = %ray.getDataBlock();

		if (%data.isDoor && "is_open_somehow")
			return;

		if (%slot == 0) // Unlock
		{
			if (%ray.lockState)
			{
				%ray.lockState = false;

				if (isObject(%obj.client))
					%obj.client.centerPrint("\c6You unlock the door.", 2);
			}
			else if (isObject(%obj.client))
				%obj.client.centerPrint("\c6The key refuses to turn. The door is already unlocked.", 2);
		}
		else if (%slot == 4) // Lock
		{
			if (!%ray.lockState)
			{
				%ray.lockState = true;

				if (isObject(%obj.client))
					%obj.client.centerPrint("\c6You lock the door.", 2);
			}
			else if (isObject(%obj.client))
				%obj.client.centerPrint("\c6The key refuses to turn. The door is already locked.", 2);
		}
	}
};

activatePackage("KeyPackage");

function mopImage::onFire(%this, %obj, %slot)
{
	%obj.playThread(2, shiftAway);
	%point = %obj.getMuzzlePoint(%slot);
	%vector = %obj.getMuzzleVector(%slot);
	%stop = vectorAdd(%point, vectorScale(%vector, 7));

	%ray = containerRayCast(%point, %stop,
		$TypeMasks::FxBrickObjectType |
		$TypeMasks::ShapeBaseObjectType |
		$TypeMasks::TerrainObjectType,
		%obj
	);

	if (isObject(firstWord(%ray))) {
		%pos = getWords( %ray, 1, 3 );
	}
	else {
		%pos = %stop;
	}

	initContainerRadiusSearch(
		%pos, 0.75,
		$TypeMasks::ShapeBaseObjectType
	);

	while (isObject( %col = containerSearchNext())) {
		if (%col.isBlood || %col.isPaint) {
			%col.freshness -= 0.9;
			%col.color = getWords(%col.color, 0, 2) SPC getWord(%col.color, 3) * 0.5;
			%col.setNodeColor("ALL", %col.color);
			%col.setScale(vectorScale(%col.getScale(), 0.8));
			if (%col.freshness <= 0)
				%col.delete();
			continue;
		}
	}
}

function mopImage::onStopFire(%this, %obj, %slot)
{
	%obj.playThread(2, root);
}
