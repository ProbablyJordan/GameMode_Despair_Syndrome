if (!isObject(DSGameModeGroup))
{
	new ScriptGroup(DSGameModeGroup);
}
function DSGameMode::onAdd(%this)
{
	DSGameModeGroup.add(%this);
}
function DSGameMode::onMiniGameJoin(%this, %miniGame, %client)
{	
}
function DSGameMode::onMiniGameLeave(%this, %miniGame, %client)
{
}
function DSGameMode::onBodyExamine(%this, %miniGame, %client, %body)
{
}
function DSGameMode::onStart(%this, %miniGame)
{
	$DefaultMiniGame.lastStageStarted = getSimTime();
	$DefaultMiniGame.noWeapons = false;
	$DefaultMiniGame.shapeNameDistance = 13.5;
	%miniGame.messageAll('', '\c5A new round has started. Current mode is: %1! %2', %this.name, %this.desc);

	// Close *all* doors
	%count = BrickGroup_888888.getCount();

	for (%i = 0; %i < %count; %i++)
	{
		%brick = BrickGroup_888888.getObject(%i);

		%data = %brick.getDataBlock();

		if (%data.isDoor)
		{
			%brick.doorHits = 0;
			%brick.broken = false;
			%brick.setDataBlock(%brick.isCCW ? %data.closedCCW : %data.closedCW);
		}
		
		%name = %brick.getName();
		if((%len = strLen(%name)) > 5 && getSubStr(%name, 0, 5) $= "_room"
			&& (%num = getSubStr(%name, 5, %len - 5)) $= (%num | 0))
		{
			%brick.createZoneUp(4.5, 0.3);
			%brick.setZoneSoundproof(1);
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
		%roomDoor.doorMaxHits = 6;
		%roomSpawn = BrickGroup_888888.NTObject["_" @ %room, 0];
	}
	if(isObject(%door = BrickGroup_888888.NTObject["_door_janitor", 0]))
	{
		%door.lockID = "JCloset";
		%door.lockState = true;
		%door.doorMaxHits = 12;
		%closetDoor = %door;
	}
	if(isObject(%door = BrickGroup_888888.NTObject["_door_incinerator", 0]))
	{
		%door.lockID = "JFurnace";
		%door.lockState = true;
		%door.doorMaxHits = 12;
		%furnaceDoor = %door;
	}
	
	// Random items!
	%name = "_randomlootspawn";
	//%choice[%choices++-1] = "CaneItem 1";
	//%choice[%choices++-1] = "UmbrellaItem 1.25";
	//%choice[%choices++-1] = "MonkeyWrenchItem 1";
	//%choice[%choices++-1] = "PanItem 1.25";
	//%choice[%choices++-1] = "KnifeItem 1";
	//%choice[%choices++-1] = "MopItem 0.75";
	//%choice[%choices++-1] = "LockpickItem 0.4";
	//%choice[%choices++-1] = "WoodBatItem 1";
	%choice[%choices++-1] = "0.8 CaneItem MonkeyWrenchItem KnifeItem";
	
	//%choice[%choices++-1] = "0.2 WoodBatItem MetalBatItem AluminumBatItem";
	%choice[%choices++-1] = "0.2 CaneItem MonkeyWrenchItem KnifeItem";
	
	%choice[%choices++-1] = "1.5 UmbrellaItem PanItem";
	//%choice[%choices++-1] = "0.75 MopItem";
	%choice[%choices++-1] = "0.4 LockpickItem";
	
	for (%i = 0; %i < %choices; %i++)
		%chance += getWord(%choice[%i], 0);

	%count = BrickGroup_888888.NTObjectCount[%name];

	%maxItems = mCeil(%miniGame.numMembers * 2); //Adjust this number when we add more non-weapon items
	echo(%maxItems);
	for (%i = 0; %i < %count; %i++)
	{
		%brick = BrickGroup_888888.NTObject[%name, %i];
		%choose = getRandom() * %chance;
		for (%j = 0; %j < %choices; %j++)
		{
			%choose -= getWord(%choice[%j], 0);
			if(%choose <= 0)
				break;
		}
		%words = getWordCount(%choice[%j]) - 1;
		%pick = getWord(%choice[%j], getRandom(1, %words));
		if (%i >= %maxItems)
			break;
		%brick.setItem(%pick);
	}
	//Give everyone rooms, names, appearances, roles, etc
	for (%i = 0; %i < %miniGame.numMembers && %freeCount; %i++)
	{
		%member = %miniGame.member[%i];
		%player = %member.player;

		if (%member.isAdmin)
			commandToClient(%member, 'API_RoundStart');

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

		%specialCharChance = 0.01;
		if (getRandom() < %specialCharChance)
			getRandomSpecialChar(%character);
		else	
		{
			%character.name = getRandomName(%character.gender);
			%character.appearance = getRandomAppearance(%character.gender);
		}

		%member.applyBodyParts();
		%member.applyBodyColors();

		%roomDoor = BrickGroup_888888.NTObject["_door_r" @ %room, 0];
		%roomSpawn = BrickGroup_888888.NTObject["_" @ %room, 0];
		%point = %roomSpawn.getTransform();
		%player.setTransform(%point);
		%player.setShapeName(%character.name, 8564862);
		%player.setShapeNameDistance($DefaultMiniGame.shapeNameDistance);
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

		// Give them a good look at their brand new character
		%camera = %member.camera;
		//aim the camera at the target
		%pos = vectorAdd(%player.getHackPosition(), vectorScale(%player.getForwardVector(), 2));
		%delta = vectorSub(%player.getHackPosition(), %pos);
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
		%member.setControlObject(%camera);
		%camera.setControlObject(%member.dummyCamera);
		%camera.schedule(5000, "setControlObject", %member);
		%member.schedule(5000, "setControlObject", %player); //5 seconds of looking at urself
		schedule(1000, 0, messageClient, %member, '', '\c6You are <color:%1>%2\c6, and you have been assigned to \c3Room #%3\c6.',
			%nameTextColor,
			%character.name,
			%room);
		%member.updateBottomPrint();
	}
	
	%char = GameCharacters.getObject(getRandom(0, GameCharacters.getCount() - 1));
	(%props = %char.player.itemProps0).sourceItemData = KeyJanitorItem.getID();
	%char.player.tool0 = KeyJanitorItem.getID();
	messageClient(%char.client, 'MsgItemPickup', '', 0, KeyJanitorItem.getID(), 1);
	if (isObject(%closetDoor))
		%closetDoor.lockID = %props.id;
	if (isObject(%furnaceDoor))
		%furnaceDoor.lockID = %props.id;
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
function DSGamemode::isAvailable(%this, %minigame)
{
	if (%this.omit)
		return 0;
	if (%this.maxPlayers !$= "" && %minigame.numMembers > %this.maxPlayers)
		return 0;
	if (%this.minPlayers !$= "" && %minigame.numMembers < %this.minPlayers)
		return 0;
	return 1;
}
function DSGamemode::unavailableReason(%this, %minigame)
{
	if (%this.omit)
		return "Disabled";
	if (%this.maxPlayers !$= "" && %minigame.numMembers > %this.maxPlayers)
		return "Too many players";
	if (%this.minPlayers !$= "" && %minigame.numMembers < %this.minPlayers)
		return "Too few players";
	return "";
}
function DSGameMode::onDay(%this, %miniGame)
{
	$DefaultMiniGame.lastStageStarted = getSimTime();
	$DefaultMiniGame.shapeNameDistance = 13.5;
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];
		%player = %member.player;

		if (!isObject(%player))
			continue;
		if (%player.unconscious) continue;
		%player.setShapeNameDistance($DefaultMiniGame.shapeNameDistance); //Update shapenames
	}
	loadEnvironment($DS::Path @ "data/env/day.txt");
	%miniGame.messageAll('', '\c5All water in the building has resumed function. Cafeteria has been unlocked.');
	%name = "_sink";
	%count = BrickGroup_888888.NTObjectCount[%name];
	for (%i = 0; %i < %count; %i++)
	{
		%brick = BrickGroup_888888.NTObject[%name, %i];
		%brick.eventEnabled0 = true;
		%brick.eventEnabled1 = true;
		%brick.eventEnabled2 = false;
	}
	%miniGame.days++;
}
function DSGameMode::onNight(%this, %miniGame)
{
	$DefaultMiniGame.lastStageStarted = getSimTime();
	$DefaultMiniGame.shapeNameDistance = 5;
	//Exhaust all players
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];
		%player = %member.player;

		if (!isObject(%player))
			continue;

		if (%player.unconscious) continue;
		%player.setShapeNameDistance($DefaultMiniGame.shapeNameDistance); //Update shapenames
	}
	loadEnvironment($DS::Path @ "data/env/night.txt");
	%miniGame.messageAll('', '\c5All water in the building has been disabled for the night. Cafeteria will be off-limits in 30 seconds.');
	%name = "_sink";
	%count = BrickGroup_888888.NTObjectCount[%name];
	for (%i = 0; %i < %count; %i++)
	{
		%brick = BrickGroup_888888.NTObject[%name, %i];
		%brick.eventEnabled0 = false;
		%brick.eventEnabled1 = false;
		%brick.eventEnabled2 = true;
	}
}
function DSGameMode::onDeath(%this, %miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
}