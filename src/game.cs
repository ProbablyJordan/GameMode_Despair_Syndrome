$DS::RoomCount = 16;

if (!isObject(GameRoundCleanup))
	new SimSet(GameRoundCleanup);

if (!isObject(GameCharacters))
	new SimSet(GameCharacters);

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

		Parent::reset(%this, %client);

		%this.messageAll('', '\c5A new round is starting.');

		// Close *all* doors
		%count = BrickGroup_888888.getCount();

		for (%i = 0; %i < %count; %i++)
		{
			%brick = BrickGroup_888888.getObject(%i);
			%brick.respawn();

			%data = %brick.getDataBlock();

			if (%data.isDoor)
			{
				%brick.doorHits = 0;
				%brick.setDataBlock(%brick.isCCW ? %data.closedCCW : %data.closedCW);
			}
		}

		%freeCount = $DS::RoomCount;

		for (%i = 0; %i < %freeCount; %i++)
		{
			%room = %i + 1;
			%freeRoom[%i] = %room;
			%roomDoor = BrickGroup_888888.NTObject["_door_r" @ %room, 0];
			%roomDoor.lockId = "R"@%room;
			%roomDoor.lockState = true;
			%roomSpawn = BrickGroup_888888.NTObject["_" @ %room, 0];
		}

		// Give everyone rooms, names, appearances, roles, etc
		for (%i = 0; %i < %this.numMembers && %freeCount; %i++)
		{
			%member = %this.member[%i];
			%player = %member.player;

			if (!isObject(%player))
				continue;

			%freeCount--;
			%freeIndex = getRandom(%freeCount);
			%room = %freeRoom[%freeIndex];
			for (%j = %freeIndex; %j < %freeCount; %j++)
				%freeRoom[%j] = %freeRoom[%j + 1];

			%freeRoom[%freeCount] = "";

			%character = new ScriptObject()
			{
				client = %member;
				clientName = %member.getPlayerName();
				player = %player;
				gender = getRandomGender();
				room = %room;
			};

			GameCharacters.add(%character);
			%member.character = %character;

			%character.name = getRandomName(%character.gender);
			%character.appearance = getRandomAppearance(%character.gender);

			%member.applyBodyParts();
			%member.applyBodyColors();

			%roomDoor = BrickGroup_888888.NTObject["_door_r" @ %room, 0];
			%roomSpawn = BrickGroup_888888.NTObject["_" @ %room, 0];

			%player.setTransform(%roomSpawn.getTransform());
			%player.setShapeName(%character.name, 8564862);

			if (%character.gender $= "female")
			{
				%nameTextColor = "ff11cc";
				// %player.setShapeNameColor("1 0.1 0.9");
				%player.setShapeNameColor("1 0.15 0.8");
			}
			else if (%character.gender $= "male")
			{
				%nameTextColor = "22ccff";
				%player.setShapeNameColor("0.1 0.8 1");
			}
			else
				%nameTextColor = "ffff00";

			// Give them a key to their room
			%props = KeyItem.newItemProps(%player, 0);
			%props.name = "Room #" @ %room @ " Key";
			%props.id = "R" @ %room;

			%player.addTool(KeyItem, %props);

			messageClient(%member, '', '\c6You are <color:%1>%2\c6, and you have been assigned to \c3Room #%3\c6.',
				%nameTextColor,
				%character.name,
				%room);
		}
	}

	function MiniGameSO::checkLastManStanding(%this)
	{
		if (%this != $DefaultMiniGame)
			return Parent::checkLastManStanding(%this);
		if (%this.numMembers < 1 || isEventPending(%this.scheduleReset))
			return 0;
		//Do game end checks if needed
		return Parent::checkLastManStanding(%this);
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

		// removed mini-game checks here
		// removed death message print here
		// removed %message and %sourceClientName arguments
		messageClient(%client, 'MsgYourDeath', '', %clientName, '', '');//%client.miniGame.respawnTime);

		commandToClient(%client, 'CenterPrint', '', 1);
		%client.miniGame.checkLastManStanding();
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
	activatePackage("DespairSyndromePackage");
