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

		%text = trim(stripMLControlChars(%text));

		%name = %client.getPlayerName();
		if (isObject(%client.character))
			%name = %client.character.name;

		%structure = '<color:ffaa44>%1<color:ffffff> %3, \"%2\"';
		%does = "says";
		%range = 32;
		if (getSubStr(%text, 0, 1) $= "!") //shouting
		{
			%text = getSubStr(%text, 1, strLen(%text));
			%does = "shouts";
			%range = 64;
		}
		else if(getSubStr(%text, 0, 1) $= "@") //Whispering
		{
			%text = getSubStr(%text, 1, strLen(%text));
			%does = "whispers";
			%range = 8;
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

			if (!isObject(%client.player)) //dead chat
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
				if (!%ignoreSrcSZ && isObject(%playerZone) && %playerZone.isSoundProof && (!isObject(%otherZone) || %otherZone != %playerZone))
				{
					// talk("The sound was made in soundproof zone. Player" SPC %client.getPlayerName() SPC "didn't hear it!");
					continue;
				}
				if (!%ignorePlayerSZ && isObject(%otherZone) && %otherZone.isSoundProof && (!isObject(%playerZone) || %playerZone != %otherZone))
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

activatePackage("ChatPackage");