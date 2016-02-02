AddDamageType("Stamina",	'knocked out %1',	'%2 knocked out %1',0.75,1);
AddDamageType("Blunt",		'bashed %1',	'%2 bashed %1',0.75,1);
AddDamageType("Sharp",		'stabbed %1',	'%2 stabbed %1',0.75,1);

//Health
function Player::updateHealth(%this)
{
	%maxHealth = %this.getDataBlock().maxDamage;

	if (%this.maxHealth $= "")
	{
		%this.maxHealth = %maxHealth;
		%this.setHealth(%maxHealth);
	}
	else
	{
		%delta = %maxHealth - %this.maxHealth;
		%this.maxHealth = %maxHealth;

		if (%delta >= 0)
			%this.setHealth(%this.health + %delta);
		else if (%this.health > %this.maxHealth)
			%this.setHealth(%this.maxHealth);
	}
}

function Player::setHealth(%this, %health)
{
	%this.health = mClampF(%health, 0, %this.maxHealth);

	if (isObject(%this.client))
		%this.client.updateBottomPrint();
}

function Player::addHealth(%this, %health)
{
	//%health = mClampF(%health, 0, %this.maxHealth - %this.health);
	%health = getMax(0, %health);
	%this.setHealth(%this.health + %health);
}

function Player::getHealth(%this)
{
	if (%this.health !$= "")
		return %this.health;

	return %this.getDataBlock().maxDamage - %this.getDamageLevel();
}

function Player::getMaxHealth(%this)
{
	if (%this.maxHealth !$= "")
		return %this.maxHealth;

	return %this.getDataBlock().maxDamage;
}

function Player::KnockOut(%this, %duration, %exRestore)
{
	%this.changeDataBlock(PlayerCorpseArmor);
	%client = %this.client;
	if (isObject(%client) && isObject(%client.camera))
	{
		messageClient(%client, '', 'You will be unconscious for %1 seconds.', %duration / 1000);
		if (%client.getControlObject() != %client.camera)
		{
			// %client.camera.setMode("Corpse", %this);
			// %client.setControlObject(%client.camera);
			if (!isObject($KOScreenShape))
			{
				$KOScreenShape = new StaticShape()
				{
					datablock = PlaneShapeGlowData;
					scale = "1 1 1";
					position = "0 0 -300"; //units below ground level, woo
				};
				$KOScreenShape.setNodeColor("ALL", "0 0 0 1");
			}
			%camera = %client.camera;
			//aim the camera at the target
			%pos = vectorAdd($KOScreenShape.position, "0.2 0 0");
			%delta = vectorSub($KOScreenShape.position, %pos);
			%deltaX = getWord(%delta, 0);
			%deltaY = getWord(%delta, 1);
			%deltaZ = getWord(%delta, 2);
			%deltaXYHyp = vectorLen(%deltaX SPC %deltaY SPC 0);

			%rotZ = mAtan(%deltaX, %deltaY) * -1; 
			%rotX = mAtan(%deltaZ, %deltaXYHyp);

			%aa = eulerRadToMatrix(%rotX SPC 0 SPC %rotZ); //this function should be called eulerToAngleAxis...

			%camera.setTransform(%pos SPC %aa);
			%camera.setFlyMode();
			%camera.mode = "Observer";
			%client.setControlObject(%camera);
			%camera.setControlObject(%client.dummyCamera);
		}
	}
	if (%exRestore !$= "")
		%this.setExhaustion(%this.exhaustion + %exRestore);
	%this.setArmThread(land);
	%this.playThread(0, "root");
	%this.playThread(1, "root");
	%this.playThread(2, "root");
	%this.playThread(3, "death1");
	%this.setActionThread("root");
	%this.unconscious = true;
	%this.setShapeNameDistance(0);
	%this.isBody = true;
	%this.wakeUpSchedule = %this.schedule(%duration, WakeUp);
}

function Player::WakeUp(%this)
{
	cancel(%this.wakeUpSchedule);
	if (%this.getState() $= "Dead")
		return;
	%client = %this.client;
	if (isObject(%client) && isObject(%client.camera))
	{
		%client.camera.setMode("Player", %this);
		%client.camera.setControlObject(%client);
		%client.setControlObject(%this);
	}
	%this.setArmThread(look);
	%this.unconscious = false;
	%this.isBody = false;
	%this.setShapeNameDistance($defaultMinigame.shapeNameDistance);
	%this.changeDataBlock(PlayerDSArmor);
}

package DSHealthPackage
{
	function Player::playThread(%this, %slot, %sequenceName)
	{
		if (%this.unconscious && %slot == 3) //Don't allow to "overwrite" the sleep animation playthread
			return;
		Parent::playThread(%this, %slot, %sequenceName);
	}
	function Observer::onTrigger(%this, %obj, %slot, %state)
	{
		%client = %obj.getControllingClient();
		if (isObject(%client.player) && %client.player.unconscious)
			return;
		Parent::onTrigger(%this, %obj, %slot, %state);
	}
	function serverCmdUseTool(%client, %slot)
	{
		if (isObject(%client.player) && %client.player.unconscious)
			return;
		parent::serverCmdUseTool(%client, %slot);
	}
	function serverCmdUnUseTool(%client, %slot)
	{
		if (isObject(%client.player) && %client.player.unconscious)
			return;
		parent::serverCmdUnUseTool(%client, %slot);
	}
	function Player::playPain(%this)
	{
		//parent::playPain(%this);
		//serverPlay3d(painSound, %this.getHackPosition());
	}
	function Player::playDeathCry(%this)
	{
		//parent::playDeathCry(%this);
	}
	function Armor::onNewDataBlock(%this, %obj)
	{
		Parent::onNewDataBlock(%this, %obj);

		if (%obj.maxHealth $= "" || %obj.health $= "")
			%obj.updateHealth();
	}

	function Armor::onDisabled(%this, %obj, %state)
	{
		parent::onDisabled(%this, %obj, %state);
		%obj.playThread(0, "root");
		%obj.playThread(1, "root");
		%obj.playThread(2, "root");
		%obj.playThread(3, "death1");
		%obj.setActionThread("root");
		if (%obj.attackDot[%obj.attackCount] > 0)
		{
			%obj.playThread(2, "jump");
			%obj.playThread(3, "crouch");
			%obj.schedule(100, playThread, 2, "plant");
		}
	}

	function Armor::damage(%this, %obj, %src, %pos, %damage, %type)
	{
		if (%damage == 0)
			return;
		if (%obj.getState() $= "Dead" || getSimTime() - %obj.spawnTime < $Game::PlayerInvulnerabilityTime)
			return;

		%source = %src;

		if (isObject(%src.sourceObject))
			%source = %src.sourceObject;

		if (%damage < 0)
		{
			%obj.addHealth(-%damage);
			return;
		}

		if(%src.getType() & $TypeMasks::PlayerObjectType)
		{
			%vector = %src.getForwardVector();
		}
		else
		{
			%vector = vectorScale(%src.normal, -1);
		}

		%dot = vectorDot(%obj.getForwardVector(),%vector);
		%obj.attackCount++;
		%obj.attackRegion[%obj.attackCount] = %obj.getRegion(%pos);
		%obj.attackType[%obj.attackCount] = $DamageType_Array[%type];
		%obj.attackDot[%obj.attackCount] = %dot;
		%obj.attacker[%obj.attackCount] = %source.getClassName() $= "GameConnection" ? %source : %source.client;
		%obj.attackTime[%obj.attackCount] = getSimTime();
		%obj.attackerName[%obj.attackCount] = isObject(%obj.attacker[%obj.attackCount]) ? %obj.attacker[%obj.attackCount].GetPlayerName() : "";
		// echo("HARM:" SPC %obj.attackCount SPC %obj.attackRegion[%obj.attackCount] SPC %obj.attackType[%obj.attackCount] SPC %obj.attacker[%obj.attackCount].GetPlayerName());
		%obj.setDamageFlash(getMax(0.25, %damage / %obj.maxHealth));

		%randMax = %type == $DamageType::Sharp ? 2 : 3; //Sharp weapons have higher chance to cause blood
		%blood = %type != $DamageType::Suicide && %type != $DamageType::Stamina;
		%obj.playPain();
		if (%blood)
			%obj.doSplatterBlood(%randMax, %pos, %vector, %type == $DamageType::Sharp ? 45 : 180);
		if (%source.getClassName() $= "Player") //rather good chance of getting blood on yourself
		{
			%image = %source.getMountedImage(0);
			%props = %source.getItemProps();
			if (%props.class $= "MeleeProps") //Can bloodify!
				%props.bloody = true; //Always bloodify
			if (getRandom(1, %randMax) == 1 && %blood && isObject(%image))
			{
				%source.bloody["rhand"] = true;
				// %source.bloody["lhand"] = true; //Idea: bloodify the other hand if player tries to carry the body!
				%source.bloody["chest_front"] = true; //you filthy murderer, get blood on your chest.
				if (isObject(%source.client))
					%source.client.applyBodyParts();
			}
			if (getRandom(1, %randMax) == 1 && %blood)
			{
				%obj.bloody["chest_" @ (%dot > 0 ? "back" : "front")] = true; //TODO: take sides into account, too. Maybe.
				if (isObject(%obj.client))
					%obj.client.applyBodyParts();
			}
			if (%dot > 0) //Backstab
			{
				%damage *= getMin(1, %image.backstabMult) + %dot;
			}
		}
		if (%type == $DamageType::Stamina)
		{
			%obj.setEnergyLevel(%obj.getEnergyLevel() - %damage);
			if (%obj.getEnergyLevel() <= 10 && !%obj.unconscious)
			{
				%obj.KnockOut(30000, 1); //KO'd for 30 seconds with 1 bar of exhaustion recovery
			}
			return;
		}
		else
			%obj.setHealth(%obj.health - %damage);
		if (%obj.health <= 0)
		{
			if (%blood)
				%obj.doSplatterBlood(6, %pos, %vector, %type == $DamageType::Sharp ? 45 : 180);
			if (%obj.unconscious)
				%obj.WakeUp();
			Parent::damage(%this, %obj, %src, %position, %this.maxDamage * 4, %type);
		}
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSHealthPackage");
