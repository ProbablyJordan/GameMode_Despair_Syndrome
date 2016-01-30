function pingScoreTick()
{
	cancel($pingScoreTick);

	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%client.setScore(%client.getPing());
	}

	$pingScoreTick = schedule(500, 0, "pingScoreTick");
}

if (!isEventPending($pingScoreTick))
	pingScoreTick();

function setServerName(%name)
{
	$Server::Name = %name;
	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%client.sendPlayerListUpdate();
	}
}

package DSConnectionPackage
{
	function GameConnection::onClientLeaveGame(%this, %a, %b, %c)
	{
		for (%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%client = ClientGroup.getObject(%i);
			if (%client.isAdmin)
			{
				%adminOn = true;
				break;
			}
		}
		if ($Pref::Server::DespairSyndrome::RequireAdmins && !%adminOn)
			setServerName("(NO ADMINS)" @ $Pref::Server::Name);
		parent::onClientLeaveGame(%this, %a, %b, %c);
	}
	function GameConnection::startLoad(%this, %a, %b, %c)
	{
		//WHY IS BL_ID NOT AVAILABLE AAAAAAAAAAA
		//Gotta check through three lists:
		//$Pref::Server::AutoAdminList
		//$Pref::Server::AutoAdminServerOwner
		//$Pref::Server::AutoSuperAdminList
		// for (%i=0;%i<getWordCount($Pref::Server::AutoAdminList);%i++)
		// {
		// 	%bl_id = getWord($Pref::Server::AutoAdminList, %i);
		// 	if (%bl_id == %this.bl_id)
		// 	{
		// 		%isAdmin = true;
		// 		break;
		// 	}
		// }
		// for (%i=0;%i<getWordCount($Pref::Server::AutoAdminServerOwner);%i++)
		// {
		// 	%bl_id = getWord($Pref::Server::AutoAdminList, %i);
		// 	if (%bl_id == %this.bl_id)
		// 	{
		// 		%isAdmin = true;
		// 		break;
		// 	}
		// }
		// for (%i=0;%i<getWordCount($Pref::Server::AutoSuperAdminList);%i++)
		// {
		// 	%bl_id = getWord($Pref::Server::AutoAdminList, %i);
		// 	if (%bl_id == %this.bl_id)
		// 	{
		// 		%isAdmin = true;
		// 		break;
		// 	}
		// }
		parent::startLoad(%this, %a, %b, %c);
		// talk(%this.getPlayerName() SPC "attempting connection" SPC %this.bl_id SPC "ADMIN:" SPC %this.isAdmin);
		if ($Pref::Server::DespairSyndrome::RequireAdmins && !%this.isAdmin)
		{
			for (%i = 0; %i < ClientGroup.getCount(); %i++)
			{
				%client = ClientGroup.getObject(%i);
				if (%client.isAdmin)
				{
					%adminOn = true;
					break;
				}
			}
			if (!%adminOn)
			{
				%this.delete("There are no admins present on the server.\nServer prefs dictate that nobody can join unless admins are on!\nIf you want to play please notify us via the <a:forum.blockland.us/index.php?topic=292001.45Forum Topic</a>");
				return;
			}
			setServerName($Pref::Server::Name);
		}
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage(DSConnectionPackage);