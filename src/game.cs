$DS::RoomCount = 0;

if (!isObject(GameRoundCleanup))
	new SimSet(GameRoundCleanup);

if (!isObject(GameCharacters))
	new SimSet(GameCharacters);

package DespairSyndromePackage
{
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
		Parent::addMember(%this, %member);
	}

	function MiniGameSO::removeMember(%this, %member)
	{
		Parent::removeMember(%this, %member);
	}

	function MiniGameSO::Reset(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::reset(%this, %client);

		if (isObject(GameRoundCleanup))
			GameRoundCleanup.deleteAll();
		if (isObject(GameCharacters))
			GameCharacters.deleteAll();
		if (isObject(DecalGroup))
			DecalGroup.deleteAll();

		Parent::reset(%this, %client);

		%this.allowPickup = false;
		schedule(60000,0,allowpickup);

		// Close *all* doors
		%count = BrickGroup_888888.getCount();

		for (%i = 0; %i < %count; %i++)
		{
			%brick = BrickGroup_888888.getObject(%i);
			%data = %brick.getDataBlock();

			if (%data.isDoor)
				%brick.setDataBlock(%brick.isCCW ? %data.closedCCW : %data.closedCW);
		}

		%freeCount = $DS::RoomCount;

		for (%i = 0; %i < %freeCount; %i++)
		{
			%room = %i + 1;
			%freeRoom[%i] = %room;
			%roomDoor = BrickGroup_888888.NTObject["_door_r" @ %room, 0];
			%roomDoor.lockId = %room;
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

			%roomDoor = BrickGroup_888888.NTObject["_door_r" @ %room, 0];
			%roomSpawn = BrickGroup_888888.NTObject["_" @ %room, 0];

			%player.setTransform(%roomSpawn.getTransform());
			%player.setShapeName(%character.name);

			if (%character.gender $= "female")
				%player.setShapeNameColor("1 0.1 0.9");
			else if (%character.gender $= "male")
				%player.setShapeNameColor("0.1 0.8 1");

			// Give them a key to their room
			%props = KeyItem.newItemProps(%player, 0);
			%props.name = "Room #" @ %room @ " Key";
			%props.id = "R" @ %room;

			%player.addTool(KeyItem, %props);
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
		messageClient(%client, 'MsgYourDeath', '', %clientName, '', %client.miniGame.respawnTime);

		commandToClient(%client, 'CenterPrint', '', 1);
		%client.miniGame.checkLastManStanding();
	}

	function Armor::onCollision(%this, %obj, %col, %vec, %speed)
	{
	}

	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);

		if (%slot != 0)
			return;

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
		else if (isObject(%item) && %time < 0.15 && $DefaultMinigame.allowPickup && %item.canPickUp)
		{
			%obj.addItem(%item.GetDatablock());
			%item.delete();
		}
	}
};

function allowpickup()
{
	$defaultminigame.allowpickup=true;
	messageall('',"\c6The weapon can now be picked up with a quick left click!");
}

function Item::carryTick(%this)
{
	cancel(%this.carrySchedule);

	%player = %this.carryPlayer;

	if (!isObject(%player) || %player.getState() $= "Dead" || %player.getMountedImage(0))
		return;

	%eyePoint = %player.getEyePoint();
	%eyeVector = %player.getEyeVector();

	%center = %this.getWorldBoxCenter();
	%target = vectorAdd(%eyePoint, vectorScale(%eyeVector, 3));

	if (vectorDist(%center, %target) > 6)
	{
		%player.playThread(2, "root");
		return;
	}

	%this.setVelocity(vectorScale(vectorSub(%target, %center), 8 / %this.GetDatablock().mass));
	%this.carrySchedule = %this.schedule(1, "carryTick");
}

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DespairSyndromePackage");
