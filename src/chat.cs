$MaxLinkLength = 32;

function ParseLinks(%msg)
{
	%words = getWordCount(%msg);
	for(%i=0;%i<%words;%i++)
	{
		%word = getWord(%msg, %i);
		if(getSubStr(%word, 0, 7) $= "http://")
			%msg = setWord(%msg, %i, "<a:"@(%word=strReplace(%word, "http://", ""))@">"@%word@"</a>");
		else if(getSubStr(%word, 0, 8) $= "https://")
			%msg = setWord(%msg, %i, "<a:"@(%word=strReplace(%word, "https://", ""))@">"@%word@"</a>");
		else if(getSubStr(%word, 0, 6) $= "ftp://")
			%msg = setWord(%msg, %i, "<a:"@(%word=strReplace(%word, "ftp://", ""))@">"@%word@"</a>");
	}
	return trim(%msg);
}

function ParseLinks(%text)
{
	%count = getWordCount(%text);

	for (%i = 0; %i < %count; %i++)
	{
		%word = getWord(%text, %i);

		if (getSubStr(%word, 0, 7) $= "http://")
			%link = getSubStr(%word, 7, strlen(%word));
		else if (getSubStr(%word, 0, 8) $= "https://")
			%link = getSubStr(%word, 8, strlen(%word));
		else
			continue;

		if (%link $= "" || strpos(%link, ":") != -1)
			continue; // this is illegal you know

		if (strlen(%link) > $MaxLinkLength)
			%show = getSubStr(%link, 0, $MaxLinkLength - 3) @ "...";
		else
			%show = %link;

		%text = setWord(%text, %i, "<a:" @ %link @ ">" @ %show @ "</a>");
	}

	return %text;
}


function serverCmdMe(%client, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24)
{
	if (!isObject(%pl = %client.player) || (%pl.unconscious && !%pl.currResting))
		return;
	if(despair_isMuted(%client.getBLID(), 0))
	{
		messageClient(%client, '', "You have been muted!");
		return;
	}
	
	%text = %m1;
	for (%i=2; %i<=24; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	if (%text $= "")
		return;
	%name = %client.getPlayerName();
	if (isObject(%client.character))
		%name = %client.character.name;
	
	generalRecordLine(%client, "ACTION: {0} {1}", %text);

	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%other = ClientGroup.getObject(%i);
		if (%other.inDefaultGame() && isObject(%other.player))
		{
			%a = %pl.getEyePoint();
			%b = %other.player.getEyePoint();
			%mask = $TypeMasks::All ^ $TypeMasks::FxBrickAlwaysObjectType;
			%ray = containerRayCast(%a, %b, %mask, %client.player);
			if (%ray && %ray.getClassName() !$= "Player") //Can't see emote
				continue;
			if (vectorDist(%a, %b) > 24) //Out of range
				continue;
		}

		messageClient(%other, '', '<color:ffaa44>%1<color:ffcc66> %2',
							%name, %text);
	}
}

package ChatPackage
{
	function serverCmdStartTalking(%client)
	{
		if (!%client.inDefaultGame())
			Parent::serverCmdStartTalking(%client);
	}

	function serverCmdStartTalking(%client)
	{
		if (!%client.inDefaultGame())
			Parent::serverCmdStopTalking(%client);
	}

	function serverCmdMessageSent(%client, %text)
	{
		if ((%client.isAdmin || %client.isSuperAdmin || %client.bl_id == getNumKeyID()) && %text !$= "|" && getSubStr(%text, 0, 1) $= "|")
		{
			if ((%client.inDefaultGame() || !%client.hasSpawnedOnce) && !isEventPending(%client.minigame.resetSchedule))
			{
				servercmdTeamMessageSent(%client, %text);
				return;
			}
			else
			{
				adminRecordLine(%client, "ANNOUNCE: {0} {1}", %text);
				adminAnnounce(getSubStr(%text, 1, strLen(%text) - 1));
			}
		}
		
		if ((!%client.inDefaultGame() && %client.hasSpawnedOnce) || isEventPending(%client.miniGame.resetSchedule))
		{
			generalRecordLine(%client, "CHAT: {0} {1}", %text);
			return Parent::serverCmdMessageSent(%client, %text);
		}

		if (isObject(%pl = %client.player) && (%pl.unconscious && !%pl.currResting))
			return;
		
		if(despair_isMuted(%client.getBLID(), 0))
		{
			messageClient(%client, '', "You have been muted!");
			return;
		}
		
		if (%text $= "")
			return;
		
		generalRecordLine(%client, "CHAT: {0}{1}: {2}", isObject(%char = %client.character) ? " [Char: " @ %char.name @ "]" : "", %text);
		(%mini = %client.minigame).gamemode.messageSent(%mini, %client, %text);
	}

	function serverCmdTeamMessageSent(%client, %text) //OOC
	{
		if ((%client.isAdmin || %client.isSuperAdmin || %client.bl_id == getNumKeyID()) && %text !$= "|" && getSubStr(%text, 0, 1) $= "|")
		{
			if ((!%client.inDefaultGame() && %client.hasSpawnedOnce) || isEventPending(%client.minigame.resetSchedule))
			{
				servercmdMessageSent(%client, %text);
				return;
			}
			else
			{
				adminRecordLine(%client, "ANNOUNCE: {0} {1}", %text);
				adminAnnounce(getSubStr(%text, 1, strLen(%text) - 1));
			}
		}
		
		if (!%client.inDefaultGame() && %client.hasSpawnedOnce)
		{
			generalRecordLine(%client, "CHAT: {0} {1}", %text);
			return Parent::serverCmdMessageSent(%client, %text);
		}
		if (isEventPending(%client.miniGame.resetSchedule))
		{
			generalRecordLine(%client, "CHAT: {0} {1}", %text);
			return serverCmdMessageSent(%client, %text);
		}
		
		if(despair_isMuted(%client.getBLID(), 1))
		{
			messageClient(%client, '', "You have been muted!");
			return;
		}
		
		if ($defaultMiniGame.muteOOC && !%client.isAdmin)
		{
			messageClient(%client, '', "OOC is muted!");
			return;
		}
		%text = trim(stripMLControlChars(%text));
		%text = ParseLinks(%text);
		if (%text $= "")
			return;
		generalRecordLine(%client, "OOC CHAT: {0} {1}", %text);
		(%mini = %client.minigame).gamemode.teamMessageSent(%mini, %client, %text);
	}
};

function DSGamemode::messageSent(%gamemode, %minigame, %client, %text)
{
	%text = trim(stripMLControlChars(%text));
	%player = %client.player;
	
	%name = %client.getPlayerName();
	if (isObject(%client.character))
		%name = %client.character.name;
	
	%structure = '<color:ffaa44>%1<color:ffffff> %3, %4\"%2\"';
	%does = "says";
	%range = 24;
	%zrange = 16;
	if (getSubStr(%text, 0, 1) $= "!") //shouting
	{
		%text = getSubStr(%text, 1, strLen(%text));
		%does = "shouts";
		%font = "<font:Verdana:28>";
		%range = 100;
		%zrange = 32;
	}
	else if(getSubStr(%text, 0, 1) $= "@") //Whispering
	{
		%text = getSubStr(%text, 1, strLen(%text));
		%does = "whispers";
		%font = "<font:segoe ui light:24>";
		%range = 4;
		%zrange = 4;
	}
	
	if (%text $= "")
		return;
	
	if (isObject(%client.player))
	{
		%client.player.playThread(0, "talk");
		%client.player.schedule(strLen(%text) * 35, "playThread", 0, "root");
	}
	%count = ClientGroup.getCount();
	if (!isObject(%client.player) || !%client.inDefaultGame())
		deadGamesParse(%client, %text);
	for (%i = 0; %i < %count; %i++)
	{
		%other = ClientGroup.getObject(%i);
		%msg = %text;
		if (!isObject(%client.player) || !%client.inDefaultGame()) //dead chat
		{
			%structure = '\c7[DEAD] %1<color:aaaaaa>: %2';
			if (!%client.hasSpawnedOnce)
				%structure = '\c7[SPEC] %1<color:aaaaaa>: %2';
			if(isObject(%other.player)) //Listener's player is alive. Don't transmit the message to them.
				continue;
		}
		else if (%other.inDefaultGame() && isObject(%other.player))
		{
			%playerZone = getZoneFromPos(%other.player.getEyePoint());
			%otherZone = getZoneFromPos(%client.player.getEyePoint());
			if (isObject(%playerZone) && %playerZone.isSoundProof && (!isObject(%otherZone) || %otherZone != %playerZone))
			{
				// talk("The sound was made in soundproof zone. Player" SPC %client.getPlayerName() SPC "didn't hear it!");
				continue;
			}
			if (isObject(%otherZone) && %otherZone.isSoundProof && (!isObject(%playerZone) || %playerZone != %otherZone))
			{
				// talk("The player is in a soundproof zone. Player" SPC %client.getPlayerName() SPC "didn't hear it!");
				continue;
			}
			
			if (vectorDist(%client.player.getEyePoint(), %other.player.getEyePoint()) > %range) //Out of range
				continue;
			if (mAbs(getWord(%client.player.getEyePoint(), 2) - getWord(%other.player.getEyePoint(), 2) > %zrange)) //Check if it's out of Z range too
				continue;
			if ((%other.player.unconscious && !%other.player.currResting) && vectorDist(%client.player.getEyePoint(), %other.player.getEyePoint()) > 4)
			{
				%msg = muffleText(%text, 35);
			}
		}
		
		messageClient(%other, '', %structure,
							%name, %msg, %does, %font);
	}
}

function DSGamemode::TeamMessageSent(%gamemode, %minigame, %client, %text)
{
	%structure = '<color:4444FF>[OOC] %1<color:aaaaFF>: %2';
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%other = ClientGroup.getObject(%i);
		messageClient(%other, '', %structure,
							%client.getPlayerName(), %text);
	}
}

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("ChatPackage");
