//NOTE TO SELF: TSShapeConstructor has to be done BEFORE player datablock.
datablock TSShapeConstructor(mDespairsyndromeDts) {
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
	maxTools = 4;
	isDSPlayer = 1;

	//minImpactSpeed = 15;
	// minImpactSpeed = 18;
	// speedDamageScale = 2.3;
	// speedDamageScale = 1.6;

	jumpEnergyDrain = 30;
	minJumpEnergy = 30;
	ShowEnergyBar = true;
	jumpForce = 1200;
	regenStamina = 0.5;
	rechargeRate = 0;
	jumpSound = "";
	
	convertRatio0 = 0.01;  // stamina to exhaustion ratio
};

datablock PlayerData(PlayerDSFrozenArmor : PlayerStandardArmor)
{
	shapeFile = "base/data/shapes/player/m_despairsyndrome.dts";
	uiName = "Frozen Player";
	canJet = 0;
	jumpForce = 0;
	runForce = 0;
	maxTools = 4;
	jumpSound = "";
};

datablock PlayerData(PlayerCorpseArmor : PlayerStandardArmor)
{
	shapeFile = "base/data/shapes/player/m_despairsyndrome.dts";
	uiName = "Corpse Player";
	canJet = 0;
	boundingBox = "5 5 4";
	crouchBoundingBox = "5 5 4";
	firstPersonOnly = 1;
	maxTools = 4;
	jumpSound = "";
};


function PlayerDSArmor::onNewDataBlock(%this, %obj)
{
	Parent::onNewDataBlock(%this, %obj);
	%obj.regenStaminaDefault = %this.regenStamina;
	%obj.regenStamina = %this.regenStamina;
	%obj.energyLimit = %this.maxEnergy;
	%obj.convertRatio0 = %this.convertRatio0;
	%obj.monitorEnergyLevel(getSimTime());
	if (%obj.currSleepLevel $= "")
	{
		%obj.currSleepLevel = 25 * 0.583333333;
		%obj.speedMult = 1;
	}
	%obj.exhaustionImmune = false;
}

function serverCmdSleep(%this, %bypass)
{
	if(!isObject(%pl = %this.player) || !%this.inDefaultGame() || %this.miniGame.gameMode.trial)
		return;
	if (%pl.currSleepLevel < 25)
	{
		%this.chatMessage("\c6You can't sleep yet - you don't feel tired!");
		return;
	}
	if (%bypass)
	{
		if ((%pl.unconscious && !%pl.currResting) || %pl.currSleeping)
			return;
		%pl.currSleeping = 1;
		%pl.KnockOut(-1); //1.30 minute KO w/ 2 bars of exhaustion recovery
		%this.updateBottomPrint();
		return;
	}
	%message = "\c2Are you sure you want to sleep?\nYou will be unconscious for a minute and a half!";
	commandToClient(%this, 'messageBoxYesNo', "Sleep Prompt", %message, 'SleepAccept');
}

function servercmdRest(%cl)
{
	if (!isObject(%pl = %cl.player) || !%cl.inDefaultGame() || %cl.miniGame.gameMode.trial)
		return;
	if (%pl.currSleeping || %pl.currResting || %pl.unconscious)
		return;
	%cl.setControlObject(%cam = %cl.camera);
	%cam.setMode("CORPSE", %pl);
	%cl.centerPrint("\c6You are resting. Left-click to wake up.", 3);
	%pl.changeDatablock(PlayerCorpseArmor);
	%pl.setArmThread(land);
	%pl.playThread(0, "root");
	%pl.playThread(1, "root");
	%pl.playThread(2, "root");
	%pl.playThread(3, "death1");
	%pl.setActionThread("root");
	%pl.currResting = 1;
	%pl.unconscious = 1;
	%pl.setShapeNameDistance(0);
	%pl.isBody = true;
}

function serverCmdSleepAccept(%this)
{
	serverCmdSleep(%this, 1);
}

function Player::monitorEnergyLevel(%this, %last)
{
	cancel(%this.monitorEnergyLevel);

	if (%this.getState() $= "Dead" || !isObject(%this.client))
		return;

	%carryingCorpse = isObject(%this.carryObject) && %this.carryObject.getType() & $TypeMasks::CorpseObjectType;
	if (%this.getMountedImage(0) || %carryingCorpse) //Weapon out or carrying body
	{
		%this.running = false;
		%this.regenStamina = %this.regenStaminaDefault;
		%this.setMaxForwardSpeed(%this.getDataBlock().maxForwardSpeed * %this.speedMult);
		%this.setMaxSideSpeed(%this.getDataBlock().maxSideSpeed * %this.speedMult);
		%this.setMaxBackwardSpeed(%this.getDataBlock().maxBackwardSpeed * %this.speedMult);
	}

	if (%this.running && vectorLen(%this.getVelocity()) > %this.getDataBlock().maxForwardSpeed)
	{
		%this.setEnergyLevel(%this.getEnergyLevel() - 1);

		if (%this.getEnergyLevel() < 1)
		{
			%this.setMaxForwardSpeed(%this.getDataBlock().maxForwardSpeed * %this.speedMult * 0.5);
			%this.setMaxSideSpeed(%this.getDataBlock().maxSideSpeed * %this.speedMult * 0.5);
			%this.setMaxBackwardSpeed(%this.getDataBlock().maxBackwardSpeed * %this.speedMult * 0.5);
		}
	}
	else //idle
	{
		%restoreStamina = %this.regenStamina;
		if (getSimTime() - %this.haltStaminaReg < 1300)
			%restoreStamina = %restoreStamina * ((getSimTime() - %this.haltStaminaReg)/1300);
		%currStamina = %this.getEnergyLevel();
		%newStamina = getMin(%currStamina + %restoreStamina, %this.energyLimit);
		%deltaStamina = %newStamina - %currStamina;
		//if (%deltaStamina > 0)
		//	%this.currSleepLevel += %deltaStamina * %this.convertRatio0;
		%this.setEnergyLevel(%newStamina); //Energy Limit used for exhaustion stuffs
	}
	%this.client.updateBottomPrint();
	// %show = %this.getEnergyLevel() < %this.getDataBlock().maxEnergy;

	// if (%show != %last)
	// 	commandToClient(%this.client, 'ShowEnergyBar', %show);

	%this.monitorEnergyLevel = %this.schedule(32, monitorEnergyLevel, %now);
}

function PlayerDSArmor::onCollision(%this, %obj, %col, %vec, %speed)
{
	if (%col.getClassName() $= "Item")
	{
		if ($defaultMiniGame.noWeapons)
			return;
		if (!isObject(%col.carryPlayer) && isObject(%col.lastTosser) && %col.lastTosser != %obj) //We shouldn't be able to hurt our tosser
		{
			if (vectorLen(%col.getVelocity()) > 5) //Enough force to register as a hit
			{
				// echo(vectorLen(%col.getVelocity()));
				if (%col.getDataBlock().image.isWeapon)
				{
					%obj.lastItemImpact = %col;
					%col.getDataBlock().image.onRaycastCollision(%col.lastTosser, %obj, %col.getPosition(), vectorNormalize(vectorAdd(%obj.getHackPosition(), %col.getPosition()))); //haaaaaax
					%obj.lastItemImpact = "";
					%col.lastTosser = ""; //nullify last tosser
				}
				else
				{
					// %this.damage(%obj, %col.lastTosser, "", %col.getPosition(), vectorLen(%col.getVelocity()) * 3, $DamageType::Stamina); //Deal stamina damage -- no bucket bonking yet :(
				}
			}
		}
		return;
	}
	Parent::onCollision(%this, %obj, %col, %col, %vec, %speed);
}

function PlayerCorpseArmor::onCollision(%this, %obj, %col, %vec, %speed)
{
	PlayerDSArmor::onCollision(%this, %obj, %col, %col, %vec, %speed);
}

function PlayerDSArmor::onTrigger(%this, %obj, %slot, %state)
{
	Parent::onTrigger(%this, %obj, %slot, %state);
	//Item carrying/picking up
	if(%slot == 0)
	{
		%item = %obj.carryObject;
		if (isObject(%item) && isEventPending(%item.carrySchedule) && %item.carryPlayer $= %obj)
		{
			%time = $Sim::Time - %item.carryStart;
			cancel(%item.carrySchedule);
			%item.lastTosser = %obj;
			%item.carryEnd = $Sim::Time;
			%item.carryPlayer = 0;
			%obj.carryObject = 0;
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
				$TypeMasks::CorpseObjectType |
				$TypeMasks::ItemObjectType;

			%ray = containerRayCast(%a, %b, %mask, %obj);

			if (%ray && %ray.getClassName() $= "Item" && !%item.static)// && !isEventPending(%ray.carrySchedule))
			{
				if (isEventPending(%ray.carrySchedule) && isObject(%ray.carryPlayer))
					%ray.carryPlayer.playThread(2, "root");

				%obj.carryObject = getWord(%ray, 0);
				%ray.carryPlayer.carryObject = 0;
				%ray.carryPlayer = %obj;
				%ray.carryStart = $Sim::Time;
				// %ray.static = false;
				%ray.carryTick();
				%obj.playThread(2, "armReadyBoth");
			}
			else //Body carrying
			{
				if (isObject(firstWord(%ray)))
					%pos = getWords( %ray, 1, 3 );
				else
					%pos = %b;
				initContainerRadiusSearch(%pos, 0.2,
					$TypeMasks::playerObjectType | $TypeMasks::CorpseObjectType);

				while (isObject(%col = containerSearchNext()))
				{
					if (%col.isBody)
					{
						%found = %col;
						break;
					}
				}
				if (isObject(%found) && vectorDist(%found.getPosition(), %pos) < 2)
				{
					if ($Sim::Time - %obj.lastBodyClick < 0.3) //Double-click to carry bodies
					{
						if (isEventPending(%col.carrySchedule) && isObject(%col.carryPlayer))
							%col.carryPlayer.playThread(2, "root");
						%obj.carryObject = %col;
						%col.carryPlayer.carryObject = 0;
						%col.carryPlayer = %obj;
						%col.carryStart = $Sim::Time;
						%col.carryTick();
						if (%col.bloody["chest_front"] || %col.bloody["chest_back"])
						{
							%obj.bloody["rhand"] = true;
							%obj.bloody["lhand"] = true;
						}
						if (isObject(%obj.client))
						{
							%obj.client.applyBodyParts();
							%obj.client.applyBodyColors();
						}
						%obj.playThread(2, "armReadyBoth");
					}
					else
					{
						if (isObject(%found.client))
							%character = %found.client.character;
						else
							%character = %found.character;
						if(%found.charred)
							%text = "\c6The body is charred beyond recognition.";
						else
							%text = "\c6This is" SPC (isObject(%character) ? %character.name : "Unknown") @ "'s body.";
						if (%found.currResting)
							%text = %text @ "\n\c3They appear to be resting.";
						else if (%found.unconscious)
							%text = %text @ "\n\c3They are unconscious.";
						//Unfinished body examination flavortext below
						//"Their head has 2 cuts and 1 bruises from behind, 1 cut from the side and 1 cut from the front." -- Intended results
						// %affectedLimbs = "";
						for (%i=1;%i<=%found.attackCount;%i++) //Parse attack logs for info
						{
							// if (strPos(%affectedLimbs, %found.attackRegion[%i]) == -1)
							// 	%affectedLimbs = getWordCount(%affectedLimbs) > 0 ? %affectedLimbs SPC %found.attackRegion[%i] : %found.attackRegion[%i];
							// %limbDamageCount[%found.attackRegion[%i]]++;
							if (%found.attackType[%i] $= "Suicide")
							{
								%suicide = true;
								continue;
							}
							%damageCount[(%found.attackDot[%i] > 0 ? "back" : "front") SPC (%found.attackType[%i] $= "Sharp" ? "cut" : "bruise")]++;
						}
						// for (%i=0;%i<getWordCount(%affectedLimbs);%i++) //Parse affected limbs
						// {
						// 	%limb = getWord(%affectedLimbs, %i);
						// 	%text = %text @ "\n\c6Their" SPC %limb SPC "has";
						// 	%text = %text SPC %limbDamageCount[%limb] SPC "wounds.";
						// }
						if (%found.suicide)
							%text = %text @ "\n\c5It appears to be suicide...";
						if (%damageCount["back cut"] > 0 || %damageCount["back bruise"] > 0)
						{
							%bruise = %damageCount["back bruise"];
							%cut = %damageCount["back cut"];
							%text = %text @ "\n\c6They have" SPC (%bruise > 0 ? (%bruise SPC "bruises") : "") @ (%cut > 0 ? ((%bruise > 0 ? " and" : "") SPC %cut SPC "cuts") : "") SPC "from behind.";
						}
						if (%damageCount["front cut"] > 0 || %damageCount["front bruise"] > 0)
						{
							%bruise = %damageCount["front bruise"];
							%cut = %damageCount["front cut"];
							%text = %text @ "\n\c6They have" SPC (%bruise > 0 ? (%bruise SPC "bruises") : "") @ (%cut > 0 ? ((%bruise > 0 ? " and" : "") SPC %cut SPC "cuts") : "") SPC "from the front.";
						}
						%text = %text @ "\n\n\c3Click twice to carry.";
						if (isObject(%obj.client))
							%obj.client.centerPrint(%text, 6);

						//Hardcode central below
						if (isObject(%obj.client) && isObject(%obj.client.miniGame.gameMode))
							%obj.client.miniGame.gameMode.onBodyExamine(%obj.client.miniGame, %obj.client, %found);
					}
					%obj.lastBodyClick = $Sim::Time;
				}
			}
		}
		else if (isObject(%item) && (%item.getType() & $TypeMasks::ItemObjectType) && %time < 0.15 && %item.canPickUp)
		{
			if (%obj.addTool(%item.GetDatablock(), %item.itemProps, 1) != -1)
			{
				%item.itemProps = "";
				%item.delete();
			}
		}
	}
	//Sprinting/tossing carried objects
	if (%obj.getMountedImage(0) || (isObject(%obj.carryObject) && %obj.carryObject.getClassName() $= "Player"))
		return;
	if (%slot == 4)
	{
		%item = %obj.carryObject;
		if (isObject(%item) && isEventPending(%item.carrySchedule) && %item.carryPlayer $= %obj)
		{
			if (%obj.getEnergyLevel() >= 30)
			{
				%time = $Sim::Time - %item.carryStart;
				cancel(%item.carrySchedule);
				%item.carryPlayer = 0;
				%obj.carryObject = 0;
				%obj.playThread(2, "shiftUp");
				%item.lastTosser = %obj;
				%obj.setEnergyLevel(%obj.getEnergyLevel() - 30);
				%item.setVelocity(vectorAdd(%item.getVelocity(), vectorScale(%obj.getEyeVector(), 30)));
			}
			return;
		}
		if (%state && %obj.getEnergyLevel() >= 10)
		{
			%obj.setEnergyLevel(%obj.getEnergyLevel() - 10);
			%obj.running = true;
			%obj.regenStamina = 0;
			%obj.setMaxForwardSpeed(%this.maxForwardSpeed * %obj.speedMult * 1.75);
			%obj.monitorEnergyLevel(getSimTime());
		}
		else
		{
			%obj.running = false;
			%obj.regenStamina = %obj.regenStaminaDefault;
			%obj.setMaxForwardSpeed(%obj.getDataBlock().maxForwardSpeed * %obj.speedMult);
			%obj.setMaxSideSpeed(%obj.getDataBlock().maxSideSpeed * %obj.speedMult);
			%obj.setMaxBackwardSpeed(%obj.getDataBlock().maxBackwardSpeed * %obj.speedMult);
			%obj.monitorEnergyLevel(getSimTime());
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
		%this.lastTosser = %player;
		%this.carryEnd = $Sim::Time;
		%this.carryPlayer = 0;
		%player.carryObject = 0;
		return;
	}
	%eyePoint = %player.getEyePoint();
	%eyeVector = %player.getEyeVector();

	%center = %this.getWorldBoxCenter();
	%target = vectorAdd(%eyePoint, vectorScale(%eyeVector, 3));

	if (vectorDist(%center, %target) > 5)
	{
		%this.lastTosser = %player;
		%this.carryEnd = $Sim::Time;
		%this.carryPlayer = 0;
		%player.carryObject = 0;
		%player.playThread(2, "root");
		return;
	}

	%this.setVelocity(vectorScale(vectorSub(%target, %center), 8 / %this.GetDatablock().mass));
	%this.carrySchedule = %this.schedule(1, "carryTick");
}
function Player::carryTick(%this)
{
	cancel(%this.carrySchedule);
	if (!%this.isBody)
	{
		%player = %this.carryPlayer;
		%this.lastTosser = %player;
		%this.carryEnd = $Sim::Time;
		%this.carryPlayer = 0;
		%player.carryObject = 0;
		%player.playThread(2, "root");
		return;
	}
	%player = %this.carryPlayer;

	if (!isObject(%player) || %player.getState() $= "Dead")
	{
		%this.lastTosser = %player;
		%this.carryEnd = $Sim::Time;
		%this.carryPlayer = 0;
		%player.carryObject = 0;
		return;
	}
	if (%player.getMountedImage(0))
	{
		%player = %this.carryPlayer;
		%this.lastTosser = %player;
		%this.carryPlayer = 0;
		%player.carryObject = 0;
		return;
	}
	%eyePoint = %player.getEyePoint();
	%eyeVector = %player.getEyeVector();

	%center = %this.getPosition();
	%target = vectorAdd(%eyePoint, vectorScale(%eyeVector, 3));

	if (vectorDist(%center, %target) > 5)
	{
		%this.carryPlayer = 0;
		%player.carryObject = 0;
		%player.playThread(2, "root");
		return;
	}

	%this.setVelocity(vectorScale(vectorSub(%target, %center), 4));
	%this.carrySchedule = %this.schedule(1, "carryTick");
}
