if (!isObject(DSGameModeGroup))
{
	new ScriptGroup(DSGameModeGroup);
}
function DSGameMode::onAdd(%this)
{
	DSGameModeGroup.add(%this);
}
function DSGameMode::onStart(%this, %miniGame)
{
	%miniGame.messageAll('', '\c5A new round has started. Current mode is: %1! %2', %this.name, %this.desc);

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
	for (%i = 0; %i < %miniGame.numMembers && %freeCount; %i++)
	{
		%member = %miniGame.member[%i];
		%player = %member.player;

		if (!isObject(%player))
			return;

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
function DSGameMode::onEnd(%this, %miniGame, %winner)
{
	if (isEventPending(%miniGame.resetSchedule))
		return;
	%endtext = "\c3Nobody won.";
	if (isObject(%winner))
	{
		%endtext = "\c3" @ %winner.getPlayerName() @ (isObject(%winner.character) ? " (" @ %winner.character.name @ ")" : "") SPC "is the only survivor!";
	}
	%miniGame.messageAll('', %endtext SPC "\c5A new game will begin in 10 sceonds.");
	%miniGame.scheduleReset(10000);
}
function DSGameMode::checkLastManStanding(%this, %miniGame)
{
	%count = 0;
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (isObject(%member.player))
			%alivePlayers[%count++] = %member;
	}
	if (%count <= 1)
	{
		%winner = %alivePlayers[%count];
		%this.onEnd(%miniGame, %winner);
	}
}