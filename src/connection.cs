// function pingScoreTick()
// {
// 	cancel($pingScoreTick);
// 	if (!isObject(ClientGroup) || ClientGroup.getCount() <= 0)
// 		return;

// 	for (%i = 0; %i < ClientGroup.getCount(); %i++)
// 	{
// 		%client = ClientGroup.getObject(%i);
// 		%client.setScore(%client.getPing());
// 	}

// 	$pingScoreTick = schedule(1000, 0, "pingScoreTick");
// }

// if (!isEventPending($pingScoreTick))
// 	pingScoreTick();

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
		if (%this.inDefaultGame() && isObject(%pl = %this.player))
		{
			%pl.clientLeft = 1;
			%pl.carryPlayer = "";
			%pl.lastTosser = "";
			%pl.kill();
		}
		Parent::onClientLeaveGame(%this, %a, %b, %c);
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
	}
	function GameConnection::startLoad(%this, %a, %b, %c)
	{
		if(getNumKeyId() $= %this.bl_id)
			%isAdmin = 1;
		
		%blid = " " @ %this.bl_id @ " ";
		if(!%isAdmin)
		{
			if (strPos(" " @ $Pref::Server::AutoModList @ " ", %blid) != -1)
				%isMod = 1;
			if (strPos(" " @ $Pref::Server::AutoAdminList @ " ", %blid) != -1)
				%isAdmin = 1;
			else if (strPos(" " @ $Pref::Server::AutoSuperAdminList @ " ", %blid) != -1)
				%isAdmin = 1;
		}
		%count = ClientGroup.getCount();
		for (%i=0;%i<ClientGroup.getCount();%i++)
		{
			%member = ClientGroup.getObject(%i);
			if (%member.getModLevel() >= 1)
			{
				%adminOn = true;
				if(!isObject(%member.miniGame) || isObject(DSAdminQueue) && DSAdminQueue.isMember(%member))
				{
					%count--; //Admins outside minigame or admins in the admin queue
				}
			}
		}
		if (%count > $DS::MaxPlayers && !%isAdmin) //Max non-admin player limit reached.
		{
			%this.delete("Server is full ("@$DS::MaxPlayers@" max players).\nOpen player slots you might see are reserved for admins due to the server's heavy reliance on proper administration.\n<a:forum.blockland.us/index.php?topic=292001.45>Forum Topic</a>");
			return;
		}
		if ($Pref::Server::DespairSyndrome::RequireAdmins && !(%isAdmin || %isMod))
		{
			if (!%adminOn)
			{
				%this.delete("There are no admins present on the server.\nServer prefs dictate that nobody can join unless admins are on!\nIf you want to play please notify us via the <a:forum.blockland.us/index.php?topic=292001.45>Forum Topic</a>");
				return;
			}
			setServerName($Pref::Server::Name);
		}
		parent::startLoad(%this, %a, %b, %c);
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage(DSConnectionPackage);