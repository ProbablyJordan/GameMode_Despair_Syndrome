function GameConnection::updateBottomPrint(%this)
{
	if (!isObject(%character = %this.character)) return;

	%nameTextColor = "ffff00";
	if (%character.gender $= "female")
		%nameTextColor = "ff11cc";
	else if (%character.gender $= "male")
		%nameTextColor = "22ccff";

	for (%i=1; %i<=4; %i++)
	{
		%color = %i <= %this.player.exhaustion ? "\c6" : "\c7";
		%bar = %bar @ %color;
		if (%i > 1)
			%bar = %bar @ "|";
		%bar = %bar @ "---"; //Exhaustion bars
	}
	%health = 0;
	%maxhealth = 0;
	%stamina = 0;
	%maxstamina = 0;
	if (isObject(%this.player))
	{
		%health = MFloor(%this.player.getHealth());
		%maxhealth = %this.player.getMaxHealth();
		%stamina = MFloor(%this.player.getEnergyLevel());
		%maxstamina = %this.player.energyLimit;
	}

	%roleColor = "\c7";
	%role = "Undecided";
	if (%this.inDefaultGame() && isObject(%this.miniGame.gameMode.killer))
	{
		%roleColor = "\c2";
		%role = "Innocent";
		if (%this.miniGame.gameMode.killer == %this)
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
	%text = %text @ "<just:left><br>\c6Exhaustion: \c5" @ %bar @ "<just:right>\c6Time: \c3" @ %hour @ ":" @ %minute SPC %pastNoon;
	%this.bottomPrint(%text, 5, 0);
}