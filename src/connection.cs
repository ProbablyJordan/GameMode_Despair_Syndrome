function pingScoreTick()
{
	cancel($pingScoreTick);

	for (%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%client = ClientGroup.getObject(%i);
		%client.setScore(%client.getPing());
	}

	$pingScoreTick = schedule(1000, 0, "pingScoreTick");
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
		if (%this.isAdmin) return;
		%count = ClientGroup.getCount();
		for (%i=0;%i<ClientGroup.getCount();%i++)
		{
			%member = ClientGroup.getObject(%i);
			if (%member.isAdmin)
			{
				if(!isObject(%member.miniGame) || isObject(DSAdminQueue) && DSAdminQueue.isMember(%member))
				{
					%count--; //Admins outside minigame or admins in the admin queue
				}
			}
		}
		if (%count > $DS::MaxPlayers) //Max non-admin player limit reached.
		{
			%this.delete("Max server playercount is actually "@$DS::MaxPlayers@".\nThe reason why that is the case is to open up some space for admins.\nThis server heavily relies on proper administration. <a:forum.blockland.us/index.php?topic=292001.45Forum Topic</a>");
			return;
		}
		if ($Pref::Server::DespairSyndrome::RequireAdmins)
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