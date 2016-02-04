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
	//===============================
	// Title: Full Exception
	// Author: Jincux
	//===============================
	function GameConnection::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15)
	{
		%actualMax = $Pref::Server::MaxPlayers;
		if(ClientGroup.getCount() >= $Pref::Server::MaxPlayers)
		{
			if(getNumKeyId() $= %client.bl_id && $FullException::Mode >= 1)
				%isAdmin = 1;
			
			if(!%isAdmin && $FullException::Mode >= 3)
			{
				for(%i = 0; %i < getWordCount($Pref::Server::AutoAdminList); %i++)
				{
					if(getWord($Pref::Server::AutoAdminList, %i) $= %client.bl_id)
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
					if(getWord($Pref::Server::AutoSuperAdminList, %i) $= %client.bl_id)
					{
						%isAdmin = 1;
						break;
					}
				}
			}
			
			if(%isAdmin && $Pref::Server::MaxPlayers < 99)
				$Pref::Server::MaxPlayers = ClientGroup.getCount() + 1;
		}
		if ($Pref::Server::DespairSyndrome::RequireAdmins && !%isAdmin)
		{
			for (%i = 0; %i < ClientGroup.getCount(); %i++)
			{
				%other = ClientGroup.getObject(%i);
				if (%other.isAdmin)
				{
					%adminOn = true;
					break;
				}
			}
			if (!%adminOn)
			{
				%client.delete("There are no admins present on the server.\nServer prefs dictate that nobody can join unless admins are on!\nIf you want to play please notify us via the <a:forum.blockland.us/index.php?topic=292001.45Forum Topic</a>");
				return;
			}
			setServerName($Pref::Server::Name);
		}
		
		%ret = parent::onConnectRequest(%client, %ip, %lan, %net, %prefix, %suffix, %arg5, %rtb, %arg7, %arg8, %arg9, %arg10, %arg11, %arg12, %arg13, %arg14, %arg15);
		
		$Pref::Server::MaxPlayers = %actualMax;
		return %ret;
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage(DSConnectionPackage);