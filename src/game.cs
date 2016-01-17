$DS::RoomCount = 16;
$DS::Time::DayLength = 300; //5 mins
$DS::Time::NightLength = 150; //2.5 mins

if (!isObject(GameRoundCleanup))
	new SimSet(GameRoundCleanup);

if (!isObject(GameCharacters))
	new SimSet(GameCharacters);

function MiniGameSO::DayTimeSchedule(%this)
{
	cancel(%this.DayTimeSchedule);
	if (isEventPending(%this.scheduleReset))
		return;
	%this.currtime = %this.currTime $= "Day" ? "Night" : "Day";
	%time = %this.currTime $= "Day" ? $DS::Time::DayLength : $DS::Time::NightLength;
	%this.messageAll('', '\c5It is now \c3%1\c5.', %this.currTime);
	if (%this.currTime $= "Day")
		%this.gameMode.onDay(%this);
	else
		%this.gameMode.onNight(%this);
	%this.DayTimeSchedule = %this.schedule(%time * 1000, "DayTimeSchedule");
}
function MiniGameSO::DisableWeapons(%this)
{
	%this.noWeapons = true;
	for (%i = 0; %i < %this.numMembers; %i++)
	{
		%member = %this.member[%i];
		%player = %member.player;
		if (!isObject(%player))
			continue;
		if (%player.getMountedImage(0) && %player.getMountedImage(0).isWeapon)
			%player.unMountImage(0);
	}
}

package DespairSyndromePackage
{
	function GameConnection::inDefaultGame(%this)
	{
		return isObject($DefaultMiniGame) && %this.miniGame == $DefaultMiniGame;
	}

	function Armor::onDisabled(%this, %obj)
	{
		if (isObject(%obj.client) && %obj.client.miniGame == $DefaultMiniGame && isObject(GameRoundCleanup))
		{
			%obj.inhibitRemoveBody = true;
			GameRoundCleanup.add(%obj);
		}

		Parent::onDisabled(%this, %obj);
	}

	function Player::removeBody(%this)
	{
		if (!%this.inhibitRemoveBody)
			Parent::removeBody(%this);
	}

	function Player::mountImage(%this, %image, %slot, %loaded, %skinTag)
	{
		if (isObject(%this.client) && %this.client.inDefaultGame() && (%image.isWeapon && $DefaultMiniGame.noWeapons))
			return;
		parent::mountImage(%this, %image, %slot, %loaded, %skinTag);
	}

	function Armor::onUnMount(%this, %obj, %slot)
	{
		Parent::onUnMount(%data, %obj, %slot);

		if (%obj.isBody)
			%obj.playThread(3, "death1");
	}
	function MiniGameSO::addMember(%this, %member)
	{
		%empty = %this.numMembers < 1;
		Parent::addMember(%this, %member);
		if (%empty)
		{
			%this.reset(0);
		}
	}
	function MiniGameSO::removeMember(%this, %member)
	{
		Parent::removeMember(%this, %member);
	}
	function MiniGameSO::Reset(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::reset(%this, %client);

		// Play nice with the default rate limiting.
		if (getSimTime() - %this.lastResetTime < 5000)
			return;

		if (isObject(GameRoundCleanup))
			GameRoundCleanup.deleteAll();
		if (isObject(GameCharacters))
			GameCharacters.deleteAll();
		if (isObject(DecalGroup))
			DecalGroup.deleteAll();

		if (!isObject($DS::GameMode))
			$DS::GameMode = DSGameMode_Default;
		%this.gameMode = $DS::GameMode;
		Parent::reset(%this, %client);
		%this.gameMode.onStart(%this);
		%this.currTime = "";
		%this.DayTimeSchedule();
	}

	function MiniGameSO::checkLastManStanding(%this)
	{
		if (%this != $DefaultMiniGame)
			return Parent::checkLastManStanding(%this);
		if (%this.numMembers < 1 || isEventPending(%this.scheduleReset))
			return 0;
		%this.gameMode.checkLastManStanding(%this);
		return 0;
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if (%client.miniGame != $DefaultMiniGame)
			return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);

		if (%sourceObject.sourceObject.isBot)
		{
			%sourceClientIsBot = 1;
			%sourceClient = %sourceObject.sourceObject;
		}

		%player = %client.player;

		if (isObject(%player))
		{
			%player.setShapeName("", 8564862);

			if (isObject(%player.tempBrick))
			{
				%player.tempBrick.delete();
				%player.tempBrick = 0;
			}
			%player.isBody = true;
			%player.character = %client.character;
			%player.client = 0;
		}
		else
			warn("WARNING: No player object in GameConnection::onDeath() for client '" @ %client @ "'");

		if (isObject(%client.camera) || isObject(%player))
		{ // this part of the code isn't accurate
			if (%client.getControlObject() != %client.camera)
			{
				%client.camera.setMode("Corpse", %player);
				%client.camera.setControlObject(0);
				%client.setControlObject(%client.camera);
			}
		}

		%client.player = 0;
		%client.corpse = %player;

		if ($Damage::Direct[$damageType] && getSimTime() - %player.lastDirectDamageTime < 100 && %player.lastDirectDamageType !$= "")
			%damageType = %player.lastDirectDamageType;

		if (%damageType == $DamageType::Impact && isObject(%player.lastPusher) && getSimTime() - %player.lastPushTime <= 1000)
			%sourceClient = %player.lastPusher;

		%message = '%2 killed %1';

		if (%sourceClient == %client || %sourceClient == 0)
			%message = $DeathMessage_Suicide[%damageType];
		else
			%message = $DeathMessage_Murder[%damageType];

		// removed mid-air kills code here
		// removed mini-game kill points here

		%clientName = %client.getPlayerName();

		if (isObject(%sourceClient))
			%sourceClientName = %sourceClient.getPlayerName();
		else if (isObject(%sourceObject.sourceObject) && %sourceObject.sourceObject.getClassName() $= "AIPlayer")
			%sourceClientName = %sourceObject.sourceObject.name;
		else
			%sourceClientName = "";

		echo("\c4" SPC %sourceClientName SPC "killed" SPC %clientName);
		// removed mini-game checks here
		// removed death message print here
		// removed %message and %sourceClientName arguments
		messageClient(%client, 'MsgYourDeath', '', %clientName, '', '');//%client.miniGame.respawnTime);

		commandToClient(%client, 'CenterPrint', '', 1);
		%client.miniGame.checkLastManStanding();
		%client.miniGame.gameMode.onDeath(%miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	function serverCmdSuicide(%this, %bypass)
	{
		if (%bypass)
			return parent::serverCmdSuicide(%this);
		%message = "<h2>Are you SURE you want to commit suicide?</h2>You will be dead for the rest of the round!";
		%message = parseCustomTML(%message);
		commandToClient(%this, 'messageBoxYesNo', "", %message, 'suicideAccept');
	}

	function serverCmdAlarm(%this)
	{
		//Todo: make this "scream" hotkey
	}

	function serverCmdLight(%this)
	{
		//Todo: make this something else
	}
};

function serverCmdSuicideAccept(%this)
{
	serverCmdSuicide(%this, 1);
}

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DespairSyndromePackage");
