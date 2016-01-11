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
	if (%health < %this.health)
		%this.lastDamageTime = $Sim::Time;

	%this.health = mClampF(%health, 0, %this.maxHealth);

	// if (isObject(%this.client))
	// 	%this.client.updateBottomPrint();
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

package DSHealthPackage
{
	function Armor::onNewDataBlock(%this, %obj)
	{
		Parent::onNewDataBlock(%this, %obj);

		if (%obj.maxHealth $= "" || %obj.health $= "")
			%obj.updateHealth();
	}

	function Armor::onDisabled(%this, %obj, %state)
	{
		parent::onDisabled(%this, %obj, %state); //TODO: replace this function with custom one so we can manipulate the bodies better
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

		%obj.attackCount++;
		%obj.attackRegion[%obj.attackCount] = %obj.getRegion(%pos);
		%obj.attackType[%obj.attackCount] = %type;
		%obj.attacker[%obj.attackCount] = %source.getClassName() $= "GameConnection" ? %source : %source.client;
		// echo("HARM:" SPC %obj.attackCount SPC %obj.attackRegion[%obj.attackCount] SPC %obj.attackType[%obj.attackCount] SPC %obj.attacker[%obj.attackCount].GetPlayerName());

		%obj.setDamageFlash(getMax(0.25, %damage / %obj.maxHealth));
		%obj.playPain();

		%obj.setHealth(%obj.health - %damage);
		%obj.doSplatterBlood(3);
		if (%source.getClassName() $= "Player") //rather good chance of getting blood on yourself
		{
			%dot = vectorDot(%obj.getForwardVector(),%source.getForwardVector());
			if (getRandom(1, 3) == 1)
			{
				%source.bloody["rhand"] = true; //Both hands get bloodified atm
				%source.bloody["lhand"] = true;
				%source.bloody["chest_front"] = true; //you filthy murderer, get blood on your chest.
				if (isObject(%source.client))
					%source.client.applyBodyParts();
			}
			if (getRandom(1, 2) == 1)
			{
				%obj.bloody["chest_" @ (%dot > 0 ? "back" : "front")] = true; //TODO: take sides into account, too
				if (isObject(%obj.client))
					%obj.client.applyBodyParts();
			}
			if (%dot > 0) //Backstab
				%damage *= 2 + %dot; //Double it (or triple, potentially)
		}
		if (%obj.health <= 0)
		{
			%obj.doSplatterBlood(10);
			Parent::damage(%this, %obj, %src, %position, %this.maxDamage * 4, %type);
		}
	}

	function serverCmdSuicide(%this, %bypass)
	{
		if (%bypass)
			return parent::serverCmdSuicide(%this);
		%message = "<h2>Are you SURE you want to commit suicide?</h2>You will be dead for the rest of the round!";
		%message = parseCustomTML(%message);
		commandToClient(%this, 'messageBoxYesNo', "", %message, 'suicideAccept');
	}
};

function serverCmdSuicideAccept(%this)
{
	serverCmdSuicide(%this, 1);
}

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSHealthPackage");
