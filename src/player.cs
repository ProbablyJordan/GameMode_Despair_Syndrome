//NOTE TO SELF: TSShapeConstructor has to be done BEFORE player datablock.
datablock TSShapeConstructor(m_despairsyndromeDts) {
	baseShape = "base/data/shapes/player/m_despairsyndrome.dts";
	sequence0 = "base/data/shapes/player/m_root.dsq root";
	sequence1 = "base/data/shapes/player/m_run.dsq run";
	sequence2 = "base/data/shapes/player/m_run.dsq walk";
	sequence3 = "base/data/shapes/player/m_back.dsq back";
	sequence4 = "base/data/shapes/player/m_side.dsq side";
	sequence5 = "base/data/shapes/player/m_crouch.dsq crouch";
	sequence6 = "base/data/shapes/player/m_crouchRun.dsq crouchRun";
	sequence7 = "base/data/shapes/player/m_crouchBack.dsq crouchBack";
	sequence8 = "base/data/shapes/player/m_crouchSide.dsq crouchSide";
	sequence9 = "base/data/shapes/player/m_look.dsq look";
	sequence10 = "base/data/shapes/player/m_headSide.dsq headside";
	sequence11 = "base/data/shapes/player/m_headup.dsq headUp";
	sequence12 = "base/data/shapes/player/m_standjump.dsq jump";
	sequence13 = "base/data/shapes/player/m_standjump.dsq standjump";
	sequence14 = "base/data/shapes/player/m_fall.dsq fall";
	sequence15 = "base/data/shapes/player/m_root.dsq land";
	sequence16 = "base/data/shapes/player/m_armAttack.dsq armAttack";
	sequence17 = "base/data/shapes/player/m_armReadyLeft.dsq armReadyLeft";
	sequence18 = "base/data/shapes/player/m_armReadyRight.dsq armReadyRight";
	sequence19 = "base/data/shapes/player/m_armReadyBoth.dsq armReadyBoth";
	sequence20 = "base/data/shapes/player/m_spearReady.dsq spearready";
	sequence21 = "base/data/shapes/player/m_spearThrow.dsq spearThrow";
	sequence22 = "base/data/shapes/player/m_talk.dsq talk";
	sequence23 = "base/data/shapes/player/m_death1.dsq death1";
	sequence24 = "base/data/shapes/player/m_shiftUp.dsq shiftUp";
	sequence25 = "base/data/shapes/player/m_shiftDown.dsq shiftDown";
	sequence26 = "base/data/shapes/player/m_shiftAway.dsq shiftAway";
	sequence27 = "base/data/shapes/player/m_shiftTo.dsq shiftTo";
	sequence28 = "base/data/shapes/player/m_shiftLeft.dsq shiftLeft";
	sequence29 = "base/data/shapes/player/m_shiftRight.dsq shiftRight";
	sequence30 = "base/data/shapes/player/m_rotCW.dsq rotCW";
	sequence31 = "base/data/shapes/player/m_rotCCW.dsq rotCCW";
	sequence32 = "base/data/shapes/player/m_undo.dsq undo";
	sequence33 = "base/data/shapes/player/m_plant.dsq plant";
	sequence34 = "base/data/shapes/player/m_sit.dsq sit";
	sequence35 = "base/data/shapes/player/m_wrench.dsq wrench";
	sequence36 = "base/data/shapes/player/m_activate.dsq activate";
	sequence37 = "base/data/shapes/player/m_activate2.dsq activate2";
	sequence38 = "base/data/shapes/player/m_leftrecoil.dsq leftrecoil";
};

datablock PlayerData(PlayerDSArmor : PlayerStandardArmor)
{
	shapeFile = "base/data/shapes/player/m_despairsyndrome.dts";
	uiName = "Despair Syndrome Player";

	cameraMaxDist = 2;
	cameraVerticalOffset = 1.25;
	maxFreelookAngle = 2;

	canJet = 0;
	mass = 120;

	isDSPlayer = 1;

	//minImpactSpeed = 15;
	// minImpactSpeed = 18;
	// speedDamageScale = 2.3;
	// speedDamageScale = 1.6;

	jumpEnergyDrain = 30;
	minJumpEnergy = 30;
	ShowEnergyBar = true;
	jumpForce = 1200;

	rechargeRate = 0;
};

function Player::monitorEnergyLevel(%this)
{
	cancel(%this.monitorEnergyLevel);

	if (%this.getState() $= "Dead" || !isObject(%this.client))
		return;

	if (%this.getMountedImage(0))
	{
		%this.running = false;
		%this.setMaxForwardSpeed(%this.getDataBlock().maxForwardSpeed);
	}

	if (%this.running && vectorLen(%this.getVelocity()) > %this.getDataBlock().maxForwardSpeed)
	{
		%this.setEnergyLevel(%this.getEnergyLevel() - 1);

		if (%this.getEnergyLevel() < 1)
			%this.setMaxForwardSpeed(%this.getDataBlock().maxForwardSpeed * 0.5);
	}
	else //idle
	{
		%this.setEnergyLevel(%this.getEnergyLevel() + 0.5);
	}

	// %show = %this.getEnergyLevel() < %this.getDataBlock().maxEnergy;

	// if (%show != %last)
	// 	commandToClient(%this.client, 'ShowEnergyBar', %show);

	%this.monitorEnergyLevel = %this.schedule(32, monitorEnergyLevel);
}

function PlayerDSArmor::onCollision(%this, %obj, %col, %vec, %speed)
{
}

function PlayerDSArmor::onTrigger(%this, %obj, %slot, %state)
{
	Parent::onTrigger(%this, %obj, %slot, %state);
	//Item carrying/picking up
	if(%slot == 0)
	{
		%item = %obj.carryItem;

		if (isObject(%item) && isEventPending(%item.carrySchedule) && %item.carryPlayer $= %obj)
		{
			%time = $Sim::Time - %item.carryStart;
			cancel(%item.carrySchedule);
			%item.carryPlayer = 0;
			%obj.carryItem = 0;
			%obj.playThread(2, "root");
		}
		if (%obj.getMountedImage(0))
			return;
		if (%state)
		{
			%a = %obj.getEyePoint();
			%b = vectorAdd(%a, vectorScale(%obj.getEyeVector(), 6));

			%mask =
				$TypeMasks::FxBrickObjectType |
				$TypeMasks::PlayerObjectType |
				$TypeMasks::ItemObjectType;

			%ray = containerRayCast(%a, %b, %mask, %obj);

			if (%ray && %ray.getClassName() $= "Item")// && !isEventPending(%ray.carrySchedule))
			{
				if (isEventPending(%ray.carrySchedule) && isObject(%ray.carryPlayer))
					%ray.carryPlayer.playThread(2, "root");

				%obj.carryItem = getWord(%ray, 0);
				%ray.carryPlayer.carryItem = 0;
				%ray.carryPlayer = %obj;
				%ray.carryStart = $Sim::Time;
				%ray.static = false;
				%ray.carryTick();
				%obj.playThread(2, "armReadyBoth");
			}
		}
		else if (isObject(%item) && %time < 0.15 && %item.canPickUp)
		{
			if (%obj.addTool(%item.GetDatablock(), %item.itemProps) != -1)
			{
				%item.itemProps = "";
				%item.delete();
			}
		}
	}
	//Sprinting
	if (%slot == 4)
	{
		if (%state && %obj.getEnergyLevel() >= 2)
		{
			%obj.running = true;
			%obj.setMaxForwardSpeed(%this.maxForwardSpeed * 1.75);
			%obj.monitorEnergyLevel();
		}
		else
		{
			%obj.running = false;
			%obj.setMaxForwardSpeed(%this.maxForwardSpeed);
			%obj.monitorEnergyLevel();
		}
	}
}

function Item::carryTick(%this)
{
	cancel(%this.carrySchedule);

	%player = %this.carryPlayer;

	if (!isObject(%player) || %player.getState() $= "Dead")
	{
		%this.carryPlayer = 0;
		return;
	}
	if (%player.getMountedImage(0))
	{
		%this.carryPlayer = 0;
		%player.carryItem = 0;
		return;
	}
	%eyePoint = %player.getEyePoint();
	%eyeVector = %player.getEyeVector();

	%center = %this.getWorldBoxCenter();
	%target = vectorAdd(%eyePoint, vectorScale(%eyeVector, 3));

	if (vectorDist(%center, %target) > 5)
	{
		%this.carryPlayer = 0;
		%player.carryItem = 0;
		%player.playThread(2, "root");
		return;
	}

	%this.setVelocity(vectorScale(vectorSub(%target, %center), 8 / %this.GetDatablock().mass));
	%this.carrySchedule = %this.schedule(1, "carryTick");
}
