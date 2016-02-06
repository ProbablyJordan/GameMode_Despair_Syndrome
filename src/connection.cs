function pingScoreTick()
{
	cancel($pingScoreTick);
	if (!isObject(ClientGroup) || ClientGroup.getCount() <= 0)
		return;

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

$FullException::Mode = 3;
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
	function GameConnection::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15)
	{
		%ret = parent::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
		return %ret;
	}
	function GameConnection::startLoad(%this, %a, %b, %c)
	{
		if(ClientGroup.getCount() >= $Pref::Server::MaxPlayers)
		{
			if(getNumKeyId() $= %this.bl_id && $FullException::Mode >= 1)
				%isAdmin = 1;
			
			if(!%isAdmin && $FullException::Mode >= 3)
			{
				for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++)
				{
					if(getWord($Pref::Server::AutoAdminList, %i) $= %this.bl_id)
					{
						%isAdmin = 1;
						break;
					}
				}
			}
			
			if(!%isAdmin && $FullException::Mode >= 2)
			{
				for(%i = 0; %i < getWordCount($Pref::Server::AutoSuperAdminList); %i++)
				{
					if(getWord($Pref::Server::AutoSuperAdminList, %i) $= %this.bl_id)
					{
						%isAdmin = 1;
						break;
					}
				}
			}
		}
		%count = ClientGroup.getCount();
		for (%i=0;%i<ClientGroup.getCount();%i++)
		{
			%member = ClientGroup.getObject(%i);
			if (%member.isAdmin)
			{
				%adminOn = true;
				if(!isObject(%member.miniGame) || isObject(DSAdminQueue) && DSAdminQueue.isMember(%member))
				{
					%count--; //Admins outside minigame or admins in the admin queue
				}
			}
		}
		if (%count > $DS::MaxPlayers) //Max non-admin player limit reached.
		{
			%this.delete("Server is full ("@$DS::MaxPlayers@" max players).\nOpen player slots you might see are reserved for admins due to the server's heavy reliance on proper administration.\n<a:forum.blockland.us/index.php?topic=292001.45Forum Topic</a>");
			return;
		}
		if ($Pref::Server::DespairSyndrome::RequireAdmins && !%isAdmin)
		{
			if (!%adminOn)
			{
				%this.delete("There are no admins present on the server.\nServer prefs dictate that nobody can join unless admins are on!\nIf you want to play please notify us via the <a:forum.blockland.us/index.php?topic=292001.45Forum Topic</a>");
				return;
			}
			setServerName($Pref::Server::Name);
		}
		parent::startLoad(%this, %a, %b, %c);
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage(DSConnectionPackage);