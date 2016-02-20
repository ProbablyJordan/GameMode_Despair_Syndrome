function servercmdAPI_Whois(%cl, %query)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	%mini = $DefaultMinigame;
	%count = %mini.numMembers;
	for (%i = 0; %i < %count; %i++)
	{
		%obj = %mini.member[%i];
		if (!isObject(%obj.character))
			continue;
		if (%query $= "" || %query $= %obj.getBLID()
			|| striPos(%member.getPlayerName(), %query) != -1
			|| striPos(%member.character.name, %query) != -1)
			commandToClient(%cl, 'API_WhoisReply', %member, %member.getPlayerName(), %member.character.name);
	}
}

function servercmdAPI_PM(%cl, %query, %msg)
{
	if (!(%cl.isAdmin || %cl.isSuperAdmin || %cl.hasAPIAccess))
		return;
	if (isObject(%query) && %query.getClassName() $= "GameConnection")
	{
		messageClient(%query, '', '\c4Admin PM from\c5 %1\c6: %2', %cl.getPlayerName(), %msg);
		%query.play2D(AdminBwoinkSound);    //srsly?
		messageAdmins("\c4PM from \c5" @ %this.getPlayerName() @ "\c6 to \c5" @ %query.getPlayerName() @ "\c6: " @ %msg);
	}
	else if (isObject(%query = findClientByName(%query)))
	{
		messageClient(%query, '', '\c4Admin PM from\c5 %1\c6: %2', %cl.getPlayerName(), %msg);
		%query.play2D(AdminBwoinkSound);   //srsly?
		messageAdmins("\c4PM from \c5" @ %this.getPlayerName() @ "\c6 to \c5" @ %query.getPlayerName() @ "\c6: " @ %msg);
	}
	else
		commandToClient(%cl, 'API_Error', "PM: Player not found");
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