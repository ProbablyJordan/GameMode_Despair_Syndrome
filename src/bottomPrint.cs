function GameConnection::updateBottomPrint(%this)
{
	if (!isObject(%character = %this.character)) return;

	%mode = (%mini = %this.minigame).gamemode;
	if (isObject(%mode))
		%text = %mode.getBottomPrintText(%mini, %this);
	%this.bottomPrint(%text, 5, 1);
}

function DSGamemode::getBottomPrintText(%mode, %mini, %cl)
{
	%character = %cl.character;
	%nameTextColor = "ffff00";
	if (%character.gender $= "female")
		%nameTextColor = "ff11cc";
	else if (%character.gender $= "male")
		%nameTextColor = "22ccff";

	%health = 0;
	%maxhealth = 0;
	%stamina = 0;
	%maxstamina = 0;
	if (isObject(%cl.player))
	{
		%health = mFloor(%cl.player.getHealth());
		%maxhealth = %cl.player.getMaxHealth();
		%stamina = mFloor(%cl.player.getEnergyLevel());
		%maxstamina = mFloor(%cl.player.energyLimit);
	}

	%roleColor = "\c7";
	%role = "Undecided";
	if (%cl.inDefaultGame() && isObject(%cl.miniGame.gameMode.killer))
	{
		%roleColor = "\c2";
		%role = "Innocent";
		if (%cl.miniGame.gameMode.killer == %cl)
		{
			%roleColor = "\c0";
			%role = "Killer";
		}
	}

	%text = "\c6Health: \c0"@%health@"/"@%maxhealth@"<just:right>\c6Name: <color:"@%nameTextColor@">"@%character.name@"\c6 (\c3Room #"@%character.room@"\c6)";
	%text = %text @ "<just:left><br>\c6Stamina: \c5"@%stamina@"/"@%maxstamina @ "<just:right>\c6Role:" SPC %roleColor @ %role;
	%isNight = $DefaultMinigame.currTime $= "Night";
	%segmentLength = %isNight ? $DS::Time::NightLength : $DS::Time::DayLength;
	%time = (%segmentLength * 1000 - getTimeRemaining($DefaultMinigame.dayTimeSchedule)) | 0;
	%cycleLength = $DS::Time::DayLength + $DS::Time::NightLength;
	%time *= 86.4 / %cycleLength;
	%dayRatio = $DS::Time::DayLength / %cycleLength;
	%time += %isNight ? (%dayRatio * 86400) + 21600 : 21600; //21600 is 6 AM
	%time -= mFloor(%time / 86400) * 86400;
	%hour = mFloor(%time / 3600);
	%minute = mFloor((%time - %hour * 3600) / 60);
	if (%minute < 10)
		%minute = "0" @ %minute;
	%pastNoon = %hour >= 12 ? "PM" : "AM";
	%hour = %hour % 12;
	if (%hour == 0)
		%hour = 12;
	%time = %hour @ ":" @ %minute SPC %pastNoon;
	//%text = %text @ "<just:left><br>\c6Exhaustion: \c5" @ %bar @ "<just:right>\c6Time: \c3" @ %time;
	%text = %text @ "<br><just:right>\c6Time: \c3" @ %time;
	return %text;
}

function GameConnection::sendActiveCharacter(%this)
{
	%control = %this.getControlObject();

	if (%control.getType() & $TypeMasks::PlayerObjectType)
	{
		%target = %control.client;
	}
	else if (%control.getType() & $TypeMasks::CameraObjectType)
	{
		%target = %control.getOrbitObject().client;
	}

	if (isObject(%target))
	{
		if (isObject(%target.character))
			%name = %target.character.name;
		%rName = %target.name;
	}

	%text = "\c3Spectating:" SPC %name @ (%this.isAdmin ? " (" @ %rName @ ")" : "");
	%this.bottomPrint(%text);
}

package DSSpectatePackage
{
	function GameConnection::setControlObject(%this, %control)
	{
		Parent::setControlObject(%this, %control);
		%this.sendActiveCharacter();
	}

	function Camera::setOrbitMode(%this, %obj, %mat, %minDist, %maxDist, %curDist, %ownObj)
	{
		Parent::setOrbitMode(%this, %obj, %mat, %minDist, %maxDist, %curDist, %ownObj);
		%client = %this.getControllingClient();

		if (isObject(%client))
		{
			%client.sendActiveCharacter();
		}
	}

	function Camera::setMode(%this, %mode, %orbit)
	{
		Parent::setMode(%this, %mode, %orbit);
		%client = %this.getControllingClient();

		if (isObject(%client))
		{
			%client.sendActiveCharacter();
		}
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSSpectatePackage");