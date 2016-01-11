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
		if (!%client.inDefaultGame() || !isObject(%client.character) || isEventPending(%client.miniGame.resetSchedule))
			return Parent::serverCmdMessageSent(%client, %text);

		%text = trim(stripMLControlChars(%text));

		if (%text $= "")
			return;

		// messageAll('', '<color:ffaa44>%1<color:ffffff>: %2',
		// 	%client.character.name, %text);
		%count = ClientGroup.getCount();

		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);

			if (%other.miniGame == $DefaultMiniGame && isObject(%other.player))
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

			messageClient(%other, '', '<color:ffaa44>%1<color:ffffff>: %2',
								%client.character.name, %text);
		}
	}

	function serverCmdTeamMessageSent(%client, %text)
	{
		if (!%client.inDefaultGame())
			return Parent::serverCmdMessageSent(%client, %text);

		messageClient(%client, '', '\c5Team chat does nothing right now.');
	}
};

activatePackage("ChatPackage");
