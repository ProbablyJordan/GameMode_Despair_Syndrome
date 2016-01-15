// Default Killer GameMode.
if (!isObject(DSGameMode_Killer))
{
	new ScriptObject(DSGameMode_Killer)
	{
		name = "Killer";
		desc = "One guy is the killer. Figure out who it is before he kills you all!";
		class = DSGameMode;
	};
}

function DSGameMode_Killer::onStart(%this, %miniGame)
{
	parent::onStart(%this, %miniGame);
	%miniGame.messageAll('', '<font:impact:20>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE KILLER!!');
	%this.killer = %miniGame.member[getRandom(0, %miniGame.numMembers - 1)];
	echo(%this.killer.getPlayerName());
	%this.killer.player.addTool(AdvSwordItem);
	%this.killer.player.regenStaminaDefault *= 2;
	messageClient(%this.killer, '', '<font:impact:30>You are the killer! You have been given a sword and faster stamina regen. Kill everyone to win!');
}
function DSGameMode_Killer::onEnd(%this, %miniGame, %winner)
{
	if (isEventPending(%miniGame.resetSchedule))
		return;
	%endtext = isObject(%winner) && %winner == %this.killer ? "\c3The killer wins!" : "\c3The killer loses!";
	%endtext = %endtext SPC %this.killer.getPlayerName() @ (isObject(%this.killer.character) ? " (" @ %this.killer.character.name @ ")" : "") SPC "was the killer this round.";
	%miniGame.messageAll('', %endtext SPC "\c5A new game will begin in 15 sceonds.");
	%miniGame.scheduleReset(10000);
}
function DSGameMode_Killer::checkLastManStanding(%this, %miniGame)
{
	%count = 0;
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (isObject(%member.player))
			%alivePlayers[%count++] = %member;
	}
	%winner = "";
	if (%count <= 1 && %alivePlayers[%count] $= %this.killer)
	{
		%this.onEnd(%miniGame, %alivePlayers[%count]);
	}
	else if (!isObject(%this.killer.player))
		%this.onEnd(%miniGame, %winner);	
}