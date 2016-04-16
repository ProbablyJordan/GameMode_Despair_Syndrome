package moderators
{
	function GameConnection::AutoAdminCheck(%cl, %a, %b, %c)
	{
		%ret = Parent::AutoAdminCheck(%cl, %a, %b, %c);
		if (%cl.getModLevel() > 0)
			return %ret;
		if (strPos(" " @ $Pref::Server::AutoModList @ " ", " " @ %cl.bl_id @ " ") != -1)
		{
			messageAll('MsgAdminForce', formatString("\c2{0} has become Moderator (Auto)", %cl.name));
			commandToClient(%cl, 'setAdminLevel', 1);
			%cl.isModerator = 1;
		}
		return %ret;
	}
	
	function servercmdBan(%cl, %targ, %targBL_ID, %time, %reason)
	{
		%mod = %cl.getModLevel();
		if (%mod > $ModLvl::Moderator)
		{
			adminRecordLine(%cl, "{0} banned {1} for {2} minutes: \"{3}\"", isObject(%targ) ? %targ.name @ " (BL_ID: " @ %targBL_ID @ ")" : "BL_ID: " @ %targBL_ID, %time, %reason);
			Parent::servercmdBan(%cl, %targ, %targBL_ID, %time, %reason);
		}
		else if (%mod == $ModLvl::Moderator)
		{
			if (%time < 0 || %time > 1440)
				%time = 1440;
			%cl.isAdmin = 1;
			adminRecordLine(%cl, "{0} banned {1} for {2} minutes: \"{3}\"", isObject(%targ) ? %targ.name @ " (BL_ID: " @ %targBL_ID @ ")" : "BL_ID: " @ %targBL_ID, %time, %reason);
			Parent::servercmdBan(%cl, %targ, %targBL_ID, %time, %reason);
			%cl.isAdmin = 0;
		}
	}
	
	function servercmdUnban(%cl, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j)
	{
		%mod = %cl.getModLevel();
		if (%mod >= 2)
			Parent::servercmdUnban(%cl, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j);
	}
};
activatePackage("moderators");

function GameConnection::getModLevel(%cl)
{
	%blid = %cl.bl_id;
	if (%blid == getNumKeyID())
		return 5;
	else if (%cl.isSuperAdmin)
		return 4;
	else if (%cl.isAdmin)
		return 3;
	else if (%cl.isModerator)
		return 2;
	return 0;
}

$ModLvl::Moderator = 2;
$ModLvl::Admin = 3;

function generalRecordLine(%cl, %line, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8)
{
	if (!isObject($GeneralRecorder))
	{
		$GeneralRecorder = new FileObject();
		%dt = strReplace(strReplace(getDateTime(), "/", " "), ":", "-");
		%dt = formatString("{2}-{0}-{1} {3}", getWord(%dt, 0), getWord(%dt, 1), getWord(%dt, 2), getWord(%dt, 3));
		$GeneralRecorder.currFile = "config/server/records/general_" @ %dt @ ".txt";
	}
	$GeneralRecorder.openForAppend($GeneralRecorder.currFile);
	$GeneralRecorder.writeLine("[" @ getDateTime() @ "]: " @ formatString(%line, %cl.name @ " (BL_ID: " @ %cl.bl_id @ ")", %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8));
	$GeneralRecorder.close();
}

function adminRecordLine(%cl, %line, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8)
{
	if (!isObject($GeneralRecorder))
	{
		$GeneralRecorder = new FileObject();
		%dt = strReplace(strReplace(getDateTime(), "/", " "), ":", "-");
		%dt = formatString("{2}-{0}-{1} {3}", getWord(%dt, 0), getWord(%dt, 1), getWord(%dt, 2), getWord(%dt, 3));
		$GeneralRecorder.currFile = "config/server/records/general_" @ %dt @ ".txt";
	}
	if (!isObject($AdminRecorder))
		$AdminRecorder = new FileObject();
	$AdminRecorder.openForAppend("config/server/records/admin_" @ %cl.bl_id @ ".txt");
	$AdminRecorder.writeLine("[" @ (%timestamp = getDateTime()) @ "]: " @ formatString(%line, %cl.name @ " (BL_ID: " @ %cl.bl_id @ ")", %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8));
	$AdminRecorder.close();
	$GeneralRecorder.openForAppend($GeneralRecorder.currFile);
	$GeneralRecorder.writeLine("[" @ %timestamp @ "]: " @ formatString(%line, %cl.name @ " (BL_ID: " @ %cl.bl_id @ ")", %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8));
	$GeneralRecorder.close();
}

function freekillRecordLine(%cl, %line, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8)
{
	if (!isObject($GeneralRecorder))
	{
		$GeneralRecorder = new FileObject();
		%dt = strReplace(strReplace(getDateTime(), "/", " "), ":", "-");
		%dt = formatString("{2}-{0}-{1} {3}", getWord(%dt, 0), getWord(%dt, 1), getWord(%dt, 2), getWord(%dt, 3));
		$GeneralRecorder.currFile = "config/server/records/general_" @ %dt @ ".txt";
	}
	if (!isObject($FreekillRecorder))
	{
		$FreekillRecorder = new FileObject();
		$FreekillRecorder.currFile = strReplace($GeneralRecorder.currFile, "general", "freekill");
	}
	$FreekillRecorder.openForAppend($FreekillRecorder.currFile);
	$FreekillRecorder.writeLine("[" @ (%timestamp = getDateTime()) @ "]: " @ formatString(%line, %cl.name @ " (BL_ID: " @ %cl.bl_id @ ")", %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8));
	$FreekillRecorder.close();
	$GeneralRecorder.openForAppend($GeneralRecorder.currFile);
	$GeneralRecorder.writeLine("[" @ %timestamp @ "]: " @ formatString(%line, %cl.name @ " (BL_ID: " @ %cl.bl_id @ ")", %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8));
	$GeneralRecorder.close();
}