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

		if (%text $= "")
			return;

		%name = %client.getPlayerName();
		if (isObject(%client.character))
			%name = %client.character.name;
		%structure = '<color:ffaa44>%1<color:ffffff> says, \"%2\"';
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);

			if (!isObject(%client.player)) //dead chat
			{
				%structure = '<color:444444>[DEAD] %1<color:aaaaaa>: %2';
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

				if (vectorDist(%client.player.getEyePoint(), %other.player.getEyePoint()) > 48) //Out of range
					continue;
			}

			messageClient(%other, '', %structure,
								%name, %text);
		}
	}

	function serverCmdTeamMessageSent(%client, %text) //OOC
	{
		if (!%client.inDefaultGame())
			return Parent::serverCmdMessageSent(%client, %text);

		%text = trim(stripMLControlChars(%text));

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
