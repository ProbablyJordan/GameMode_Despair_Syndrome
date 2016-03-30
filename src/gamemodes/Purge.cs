if (!isObject(DSGameMode_Purge))
{
	new ScriptObject(DSGameMode_Purge)
	{
		name = "The Purge";
		desc = "The first night is the Purge. Whoever murders on that night becomes a killer for the rest of the round!";
		class = DSGameMode;
		omit = false;
		minPlayers = 10;
	};
}

function DSGameMode_Purge::onMiniGameLeave(%this, %miniGame, %client)
{
	parent::onMiniGameLeave(%this, %miniGame, %client);
	DSTrialGameMode_Queue.remove(%client);
}

function DSGameMode_Purge::onStart(%this, %miniGame)
{
	parent::onStart(%this, %miniGame);
	%this.madekiller = false;
	%miniGame.messageAll('', '\c5The school\'s occult club has prophesized that tonight will be the Purge. Tonight, many murderers shall rise and attempt to slay everyone in their path.');
	%miniGame.messageAll('', '\c5The objective of the innocents is to lynch all the murderers from the night of the Purge.');
	%miniGame.messageAll('', '\c5The objective of the killers is to kill the remaining survivors before they are all killed off.');
	%miniGame.messageAll('', '\c5Note that while you cannot kill during the day, killer role stealing from Despair Trial is still in play.');
	%miniGame.messageAll('', '<font:impact:20>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE KILLER!!');
	if (!isObject(DSPurgeKillers))
		new SimSet(DSPurgeKillers);
}
function DSGameMode_Purge::onEnd(%this, %miniGame, %winner)
{
	if (isEventPending(%miniGame.resetSchedule))
		return;
	%endtext = isObject(%winner) && %winner == %this.killer ? "\c3The killer wins!" : "\c3The killer loses!";
	if (isObject(%killer = %this.killer))
		%endtext = %endtext SPC %killer.getPlayerName() @ (isObject(%killer.character) ? " (" @ %killer.character.name @ ")" : "") SPC "was the killer this round.";
	else %endtext = %endtext SPC "Nobody was the killer this round.";
	%miniGame.messageAll('', %endtext SPC "\c5A new game will begin in 15 sceonds.");
	%miniGame.scheduleReset(10000);
}
function DSGameMode_Purge::onDeath(%this, %miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
	if (%sourceClient == %client || %sourceClient == 0)
		return;
	if (DSPurgeKillers.isMember(%client))
	{
		%this.killer = %sourceClient;
		%this.killer.play2d(KillerJingleSound);
		messageClient(%this.killer, '', '<font:impact:30>YOU BECAME A KILLER! Self defence? Murder? Doesn\'t matter. Now you have to get away with it. Nobody must know.');
		%this.killer.centerPrint("<font:impact:30>YOU BECAME A KILLER!", 3);
		%this.killer.player.regenStaminaDefault *= 2;
		%this.killer.player.exhaustionImmune = true;
	}
	else if (!DSPurgeKillers.isMember(%sourceClient)) //Freekill?
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
function DSGameMode_Purge::checkLastManStanding(%this, %miniGame)
{
	%count = 0;
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (isObject(%member.player))
			%alivePlayers[%count++] = %member;
	}
	if (%count <= 1)
	{
		%this.onEnd(%miniGame, %alivePlayers[%count]);
	}
	else if (DSPurgeKillers.getCount() == 0 && %this.madekiller) //Killer left the game?
		%this.onEnd(%miniGame);
}
function DSGameMode_Purge::onDay(%this, %miniGame)
{
	parent::onDay(%this, %miniGame);
	if (%miniGame.days >= $DS::GameMode::DaysToSurvive)
	{
		%this.onEnd(%miniGame); //Survivors win
		return;
	}
	%miniGame.messageAll('', '\c3%1\c5 Days left until help arrives.', $DS::GameMode::DaysToSurvive - %miniGame.days);
}
function DSGameMode_Purge::onNight(%this, %miniGame)
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
		%this.madekiller = true;
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