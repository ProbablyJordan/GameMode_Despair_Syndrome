$MaxLinkLength = 32;

function linkify(%text)
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
	if (!isObject(%client.player) || %client.player.unconscious)
		return;
	%text = %m1;
	for (%i=2; %i<=24; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	if (%text $= "")
		return;
	%name = %client.getPlayerName();
	if (isObject(%client.character))
		%name = %client.character.name;

	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%other = ClientGroup.getObject(%i);
		if (%other.inDefaultGame() && isObject(%other.player))
		{
			%a = %client.player.getEyePoint();
			%b = %other.player.getEyePoint();
			%mask = $TypeMasks::All ^ $TypeMasks::FxBrickAlwaysObjectType;
			%ray = containerRayCast(%a, %b, %mask, %client.player);
			if (%ray && %ray.getClassName() !$= "Player") //Can't see emote
				continue;
			if (vectorDist(%client.player.getEyePoint(), %other.player.getEyePoint()) > 24) //Out of range
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
		if ((!%client.inDefaultGame() && %client.hasSpawnedOnce) || isEventPending(%client.miniGame.resetSchedule))
			return Parent::serverCmdMessageSent(%client, %text);

		if (isObject(%client.player) && %client.player.unconscious)
			return;

		%text = trim(stripMLControlChars(%text));

		%name = %client.getPlayerName();
		if (isObject(%client.character))
			%name = %client.character.name;

		%structure = '<color:ffaa44>%1<color:ffffff> %3, \"%2\"';
		%does = "says";
		%range = 24;
		if (getSubStr(%text, 0, 1) $= "!") //shouting
		{
			%text = getSubStr(%text, 1, strLen(%text));
			%does = "shouts";
			%range = 100;
		}
		else if(getSubStr(%text, 0, 1) $= "@") //Whispering
		{
			%text = getSubStr(%text, 1, strLen(%text));
			%does = "whispers";
			%range = 4;
		}

		if (%text $= "")
			return;

		if (isObject(%client.player))
		{
			%client.player.playThread(0, "talk");
			%client.player.schedule(strLen(%text) * 35, "playThread", 0, "root");
		}
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);

			if (!isObject(%client.player) || !%client.inDefaultGame()) //dead chat
			{
				%structure = '<color:444444>[DEAD] %1<color:aaaaaa>: %2';
				if (!%client.hasSpawnedOnce)
					%structure = '<color:444444>[SPEC] %1<color:aaaaaa>: %2';
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
			}

			messageClient(%other, '', %structure,
								%name, %text, %does);
		}
	}

	function serverCmdTeamMessageSent(%client, %text) //OOC
	{
		if (!%client.inDefaultGame())
			return Parent::serverCmdMessageSent(%client, %text);
		if (isEventPending(%client.miniGame.resetSchedule))
			return serverCmdMessageSent(%client, %text);
		if ($defaultMiniGame.muteOOC)
		{
			messageClient(%client, '', "OOC is muted!");
			return;
		}
		%text = trim(stripMLControlChars(%text));
		%text = linkify(%text);
		if (%text $= "")
			return;
		%structure = '<color:4444FF>[OOC] %1<color:aaaaFF>: %2';
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);
			messageClient(%other, '', %structure,
								%client.getPlayerName(), %text);
		}
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("ChatPackage");
