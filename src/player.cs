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

	jumpEnergyDrain = 50;
	minJumpEnergy = 50;

	jumpForce = 1200;
};

function Player::monitorEnergyLevel(%this, %last)
{
	cancel(%this.monitorEnergyLevel);

	if (%this.getState() $= "Dead" || !isObject(%this.client))
	{
		return;
	}

	if (%this.running)
	{
		%this.setEnergyLevel(%this.getEnergyLevel() - 2);
		if (%this.getEnergyLevel() < 2)
			%this.setMaxForwardSpeed(%this.getDataBlock().maxForwardSpeed * 0.75);
	}

	%show = %this.getEnergyLevel() < %this.getDataBlock().maxEnergy;

	if (%show != %last)
	{
		commandToClient(%this.client, 'ShowEnergyBar', %show);
	}

	%this.monitorEnergyLevel = %this.schedule(32, monitorEnergyLevel, %show);
}

function PlayerDSArmor::onTrigger(%this, %obj, %slot, %state)
{
	Parent::onTrigger(%this, %obj, %slot, %state);

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