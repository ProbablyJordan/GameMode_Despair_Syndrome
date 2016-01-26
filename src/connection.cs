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


package DSConnectionPackage
{
	function GameConnection::startLoad(%this)
	{
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
				%this.delete("There are no admins present on the server.\nServer prefs dictate that nobody can join unless admins are on!");
				return;
			}
		}
		parent::startLoad(%this);
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage(DSConnectionPackage);