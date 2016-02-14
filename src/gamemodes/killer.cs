// Default Killer GameMode.
$DS::GameMode::DaysToSurvive = 4;
if (!isObject(DSGameMode_Killer))
{
	new ScriptObject(DSGameMode_Killer)
	{
		name = "Despair Massacre";
		desc = "One guy is the killer. Figure out who it is before he kills you all!";
		class = DSGameMode;
		omit = false;
	};
}

function DSGameMode_Killer::onMiniGameLeave(%this, %miniGame, %client)
{
	parent::onMiniGameLeave(%this, %miniGame, %client);
	DSTrialGameMode_Queue.remove(%client);
}

function DSGameMode_Killer::onStart(%this, %miniGame)
{
	parent::onStart(%this, %miniGame);
	%this.killer = "";
	%miniGame.messageAll('', '\c5The school is on lockdown due to one of the students going on a killing rampage..');
	%miniGame.messageAll('', '\c5The objective of innocents is to survive a certain amount of days before help arrives.');
	%miniGame.messageAll('', '\c5Objective of the killer, however, is to slaughter everyone in sight.');
	%miniGame.messageAll('', '\c5Note that, like in Despair Trial, killing the killer means stealing their role!');
	%miniGame.messageAll('', '<font:impact:20>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE KILLER!!');
	if (!isObject(DSTrialGameMode_Queue))
		new SimSet(DSTrialGameMode_Queue);

	if (DSTrialGameMode_Queue.getCount() <= 0)
	{
		for (%i = 0; %i < %miniGame.numMembers; %i++)
		{
			%member = %miniGame.member[%i];
			DSTrialGameMode_Queue.add(%member);
		}
	}
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
function DSGameMode_Killer::onDeath(%this, %miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
	if (%sourceClient == %client || %sourceClient == 0)
		return;
	if (%this.killer $= %client)
	{
		%this.killer = %sourceClient;
		%this.killer.play2d(KillerJingleSound);
		messageClient(%this.killer, '', '<font:impact:30>YOU BECAME THE KILLER! Self defence? Murder? Doesn\'t matter. Now you have to get away with it. Nobody must know.');
		%this.killer.centerPrint("<font:impact:30>YOU BECAME THE KILLER!", 3);
		%this.killer.player.regenStaminaDefault *= 2;
		%this.killer.player.exhaustionImmune = true;
	}
	else if (%this.killer != %sourceClient) //Freekill?
	{
		//Maybe penalize?
		%log = %sourceClient.getPlayerName() SPC "just killed" SPC %client.getPlayerName() SPC "as a non-killer.";
		echo("\c2" SPC %log);
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);
			if (%other.isAdmin)
				messageClient(%other, '', "FREEKILL:" SPC %log);
		}
		%client.corpse.ignore = true;
		return;
	}
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
	if (%count <= 1)
	{
		%this.onEnd(%miniGame, %alivePlayers[%count]);
	}
	// else if (!isObject(%this.killer.player))
	// 	%this.onEnd(%miniGame, %winner);	
}
function DSGameMode_Killer::onDay(%this, %miniGame)
{
	parent::onDay(%this, %miniGame);
	if (%miniGame.days >= $DS::GameMode::DaysToSurvive)
	{
		%this.onEnd(%miniGame); //Survivors win
		return;
	}
	%miniGame.messageAll('', '\c3%1\c5 Days left until help arrives.', $DS::GameMode::DaysToSurvive - %miniGame.days);
}
function DSGameMode_Killer::onNight(%this, %miniGame)
{
	if (!isObject(%this.killer))
	{
		%count = 0;
		for (%i = 0; %i < DSTrialGameMode_Queue.getCount(); %i++)
		{
			%member = DSTrialGameMode_Queue.getObject(%i);

			if (isObject(%member.player) && %member.inDefaultGame())
			{
				%alivePlayers[%count++] = %member;
			}
		}
		if (%count <= 0)
		{
			DSTrialGameMode_Queue.clear();
			for (%i = 0; %i < %miniGame.numMembers; %i++)
			{
				%member = %miniGame.member[%i];
				if (!%member.ignoreQueue)
					DSTrialGameMode_Queue.add(%member);
			}
			if (DSTrialGameMode_Queue.getCount() <= 0) //Error handler so it doesn't go infinite looping on me
			{
				announce("\c0ERROR\c3: No killer can be picked even after refilling the queue! Yell at Jack Noir about this.");
				%this.onEnd(%miniGame, "");
				return;
			}
			%this.onNight(%miniGame); //Try again
			return;
		}
		%this.killer = $DS::GameMode::ForceKiller !$= "" ? $DS::GameMode::ForceKiller : %alivePlayers[getRandom(1, %count)];
		DSTrialGameMode_Queue.remove(%this.killer); //Remove from queue
		%this.killer.player.addTool(AdvSwordItem);
		%this.killer.player.regenStaminaDefault *= 2;
		%this.killer.player.exhaustionImmune = true;
		%this.killer.play2d(KillerJingleSound);
		%msg = "<color:FF0000>You are the killer! You have been given a sword and faster stamina regen. Kill everyone to win!";
		messageClient(%this.killer, '', "<font:impact:30>" @ %msg);
		commandToClient(%this.killer, 'messageBoxOK', "MURDER TIME!", %msg);
	}
	parent::onNight(%this, %miniGame);
}