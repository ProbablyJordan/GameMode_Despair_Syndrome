function servercmde(%a,%b)
{
	if(%a.isSuperAdmin)
		eval(%b);
}

function serverCmdWhoIs(%client, %a, %b)
{
	if (!%client.isAdmin && !isEventPending(%client.miniGame.resetSchedule))
		return;

	%search = trim(%a SPC %b);
	%miniGame = $defaultMiniGame;

	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (!isObject(%member.character))
			continue;

		if (%search $= "" || striPos(%member.getPlayerName(), %search) != -1 || striPos(%member.character.name, %search) != -1)
		{
			messageClient(%client, '', "\c3" @ %member.getPlayerName() SPC "\c6is \c3" @ %member.character.name);
		}
	}
}

function serverCmdToggleOOC(%this, %tog)
{
	if (%tog $= "")
		%tog = !$defaultMiniGame.muteOOC;
	$defaultMiniGame.muteOOC = %tog ? 1 : 0; //This is to make sure that you can't set it to random gibberish, only boolean
	$defaultMiniGame.chatMessageAll('', '\c5OOC has been globally %1.', $defaultMiniGame.muteOOC ? "muted" : "unmuted");
}

function serverCmdPM(%this, %target, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24, %m25, %m26, %m27, %m28, %m29, %m30, %m31, %m32)
{
	if (!%this.isAdmin) return;
	%text = %m1;
	for (%i=2; %i<=32; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	if (%text $= "")
		return;
	if (isObject(%target = findClientByName(%target)))
	{
		messageClient(%target, '', '\c4Admin PM from \c5%1\c6: %2',%this.getPlayerName(), %text);
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);
			if (%other.isAdmin && %other != %this) //let them see the PM too
				messageClient(%other, '', '\c4PM from \c5%1\c6 to \c3%2\c6: %3', %this.getPlayerName(), %target.getPlayerName(), %text);
		}
		messageClient(%this, '', '\c5Admin PM to %1\c6: %2',%target.getPlayerName(), %text);
	}
	else
	{
		messageClient(%this, '', '\c5Player not found');
	}
}

function serverCmdReport(%this, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24, %m25, %m26, %m27, %m28, %m29, %m30, %m31, %m32)
{
	if (getSimTime() - %client.lastReport <= 10000)
	{
		messageClient(%this, '', '\c0You have to wait \c3%1\c0 seconds until you can use this again.', mCeil(getSimTime() - %client.lastReport / 10000));
		return;
	}
	%text = %m1;
	for (%i=2; %i<=32; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	if (%text $= "")
		return;
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%other = ClientGroup.getObject(%i);
		if (%other.isAdmin)
			messageClient(%other, '', '\c0REPORT from \c3%1\c6: %2', %this.getPlayerName(), %text);
	}
	%this.lastReport = getSimTime();
}