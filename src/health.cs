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
		talk(%obj.attackCount SPC %obj.attackRegion[%obj.attackCount] SPC %obj.attackType[%obj.attackCount] SPC %obj.attacker[%obj.attackCount].GetPlayerName());

		%obj.setDamageFlash(getMax(0.25, %damage / %obj.maxHealth));
		%obj.playPain();

		%obj.setHealth(%obj.health - %damage);

		if (%obj.health <= 0)
		{
			Parent::damage(%this, %obj, %src, %position, %this.maxDamage * 4, %type);
		}
	}

	function serverCmdSuicide(%this, %bypass)
	{
		if (%bypass)
			return parent::serverCmdSuicide(%this);
		%message = "<h2>Are you SURE you want to commit suicide?</h2>You will be dead for the rest of the round!";
		%message = parseCustomTML(%message);
		messageBoxYesNo("", %message, "serverCmdSuicide(" @ %this @ ", 1);");
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSHealthPackage");