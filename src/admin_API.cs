function servercmdAPI_Whois(%cl, %query)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	%mini = $DefaultMinigame;
	%count = %mini.numMembers;
	for (%i = 0; %i < %count; %i++)
	{
		%member = %mini.member[%i];
		if (!isObject(%member.character))
			continue;
		if (%query $= "" || %query $= %member.getBLID()
			|| striPos(%member.getPlayerName(), %query) != -1
			|| striPos(%member.character.name, %query) != -1)
			commandToClient(%cl, 'API_WhoisReply', %member, %member.getPlayerName(), %member.character.name);
	}
}

function servercmdAPI_PM(%cl, %query, %msg)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	if ((isObject(%query) && %query.getClassName() $= "GameConnection") || isObject(%query = findClientByName(%query)))
	{
		messageClient(%query, '', '\c4Admin PM from\c5 %1\c6: %2', %cl.getPlayerName(), %msg);
		%query.play2D(AdminBwoinkSound);	//srsly?
		messageAdmins("\c4PM from \c5" @ %cl.getPlayerName() @ "\c6 to \c5" @ %query.getPlayerName() @ "\c6: " @ %msg);
		adminRecordLine(%this, "PM: {0} to {1}: {2}", %target.name @ " (" @ %target.bl_id @ ")", %text);
	}
	else
		commandToClient(%cl, 'API_Error', "PM: Player not found");
}

function serverCmdAPI_PMCC(%cl, %query0, %query1, %msg)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	%msg = ParseLinks(%msg);
	if (%msg $= "")
		return;
	if ((isObject(%query0) && %query0.getClassName() $= "GameConnection") || isObject(%query0 = findClientByName(%query0)))
	{
		%targ0.play2d(AdminBwoinkSound);
		if (isObject(%targ1 = findClientByName(%targ1)))
		{
			messageClient(%targ0, '', "\c4Admin PM from \c5"@ %this.getPlayerName() @"\c6 to \c3you\c6 and \c3"@ %targ1.getPlayerName() @"\c6: "@%text);
			messageClient(%targ1, '', "\c4Admin PM from \c5"@ %this.getPlayerName() @"\c6 to \c3you\c6 and \c3"@ %targ0.getPlayerName() @"\c6: "@%text);
			messageAdmins("\c4PM CC from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %targ0.getPlayerName() @ "\c6 and \c3" @ %targ1.getPlayerName() @"\c6: "@%text);
			adminRecordLine(%this, "PMCC: {0} to {1} and {2}: {3}", %targ0.name @ " (" @ %targ0.bl_id @ ")",
			%targ1.name @ " (" @ %targ1.bl_id @ ")", %text);
		}
		else
		{
			messageClient(%targ0, '', '\c4Admin PM from \c5%1\c6: %2',%this.getPlayerName(), %text);
			messageAdmins("\c4PM from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %target.getPlayerName() @"\c6: "@%text);
			adminRecordLine(%this, "PM: {0} to {1}: {2}", %target.name @ " (" @ %target.bl_id @ ")", %text);
		}
	}
	else
		commandToClient(%cl, 'API_Error', "PMCC: Player not found");
}

function serverCmdAPI_PMBCC(%cl, %query0, %query1, %msg)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	%msg = ParseLinks(%msg);
	if (%msg $= "")
		return;
	if ((isObject(%query0) && %query0.getClassName() $= "GameConnection") || isObject(%query0 = findClientByName(%query0)))
	{
		messageClient(%query0, '', '\c4Admin PM from \c5%1\c6: %2',%cl.getPlayerName(), %msg);
		%query0.play2d(AdminBwoinkSound);
		if ((isObject(%query1) && %query1.getClassName() $= "GameConnection") || isObject(%query1 = findClientByName(%targ1)))
		{
			if (isObject(%char = %query0.character))
				%name = %char.name;
			else
				%name = %query0.getPlayerName();
			messageClient(%query1, '', "\c4Admin PM from \c5"@ %cl.getPlayerName() @"\c6 to \c3"@ %name @"\c6: "@%msg);
			%query1.play2d(AdminBwoinkSound);
			messageAdmins("\c4PM from \c5"@ %cl.getPlayerName() @"\c6 to \c3"@ %query0.getPlayerName() @ "\c6 and \c3" @ %query1.getPlayerName() @"\c6: "@%msg);
			adminRecordLine(%this, "PM: {0} to {1} and {2}: {3}", %targ0.name @ " (" @ %targ0.bl_id @ ")",
			%targ1.name @ " (" @ %targ1.bl_id @ ")", %text);
		}
		else
		{
			messageAdmins("\c4PM from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %target.getPlayerName() @"\c6: "@%msg);
			adminRecordLine(%this, "PM: {0} to {1}: {2}", %target.name @ " (" @ %target.bl_id @ ")", %text);
		}
	}
	else
		commandToClient(%cl, 'API_Error', "PMCC: Player not found");
}

function servercmdAPI_GetKiller(%cl)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	%mini = $DefaultMinigame;
	if (isObject(%mini.gamemode) && isObject(%killer = %mini.gamemode.killer))
	{
		messageAdmins("\c5" @ %cl.getPlayerName() @ " used /API_GetKiller.");
		commandToClient(%cl, 'API_KillerReply', %killer, %killer.getPlayerName(), %killer.character.name);
	}
}

function API_sendDamageUpdate(%victim, %attacker, %time, %image, %direction)
{
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
			continue;
		commandToClient(%cl, 'API_DamageUpdate', %victim, %attacker, %time, %image, %direction);
	}
}