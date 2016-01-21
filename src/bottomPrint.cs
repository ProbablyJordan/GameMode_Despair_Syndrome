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

	%text = "\c6Health: \c0"@%health@"/"@%maxhealth@"<just:right>\c6Name: <color:"@%nameTextColor@">"@%character.name@"\c6 (\c3Room #"@%character.room@"\c6)";
	%text = %text @ "<just:left><br>\c6Stamina: \c5"@%stamina@"/"@%maxstamina;
	%text = %text @ "<br>\c6Exhaustion: \c5" @ %bar;
	%this.bottomPrint(%text, 5, 0);
}