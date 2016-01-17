// Default Killer GameMode.
if (!isObject(DSGameMode_Trial))
{
	new ScriptObject(DSGameMode_Trial)
	{
		name = "Despair Trial";
		desc = "Someone's plotting murder against one of the students... Figure out who it is in a courtroom trial!";
		class = DSGameMode;
	};
}

function DSGameMode_Trial::onStart(%this, %miniGame)
{
	parent::onStart(%this, %miniGame);
	activatepackage(DSTrialPackge);
	%miniGame.messageAll('', '<font:impact:20>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE KILLER!!');
	%this.killer = %miniGame.member[getRandom(0, %miniGame.numMembers - 1)];
	%this.killer.player.regenStaminaDefault *= 2;
	%this.deathCount = 0;
	%msg = '<font:impact:20>You are plotting murder against someone! Kill them and do it in such a way that nobody finds out it\'s you!!';
	messageClient(%this.killer, '', %msg);
	%this.killer.bottomPrint(%msg, 10);
}
function DSGameMode_Trial::onEnd(%this, %miniGame, %winner)
{
	if (isEventPending(%miniGame.resetSchedule))
		return;
	cancel(%this.trialSchedule);
	deactivatepackage(DSTrialPackge);
	if (isObject(%this.ballot))
		%this.ballot.delete();
	%this.vote = false;
	%endtext = isObject(%winner) && %winner == %this.killer ? "\c3The killer wins!" : "\c3The killer loses!";
	%endtext = %endtext SPC %this.killer.getPlayerName() @ (isObject(%this.killer.character) ? " (" @ %this.killer.character.name @ ")" : "") SPC "was the killer this round.";
	%miniGame.messageAll('', %endtext SPC "\c5A new game will begin in 15 sceonds.");
	%miniGame.scheduleReset(10000);
}
function DSGameMode_Trial::onDeath(%this, %miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
	if (%sourceClient == %client || %sourceClient == 0)
		return;
	if (%this.killer $= %client)
	{
		%this.killer = %sourceClient;
		messageClient(%this.killer, '', '<font:impact:20>You ended up killing someone. Self defence? Murder? Doesn\'t matter. Now you have to get away with it. Nobody must know.');
	}
	else if (%this.killer != %sourceClient) //Freekill?
	{
		//Maybe penalize?
		echo("\c2" SPC %sourceClient.getPlayerName() SPC "just killed" SPC %client.getPlayerName() SPC "as a non-killer.");
		return;
	}

	%this.deathCount++;
}
function DSGameMode_Trial::onDay(%this, %miniGame)
{
	parent::onDay(%this, %miniGame);
	if (%this.deathCount > 0 && !isEventPending(%this.trialSchedule) && !%this.vote)
	{
		%miniGame.messageAll('', '\c0One or more bodies have been discovered on school premises! \c5You guys have 5 minutes to investigate them before voting period starts.');
		%miniGame.messageAll('', '\c5Picking the right murderer will mean that you guys win! However, if you guys pick the WRONG culprit... \c0EVERYONE DIES!');
		%this.trialSchedule = %this.schedule(300000, "trialStart", %miniGame);
		%miniGame.DisableWeapons();
	}
}
function DSGameMode_Trial::onNight(%this, %miniGame)
{
	parent::onNight(%this, %miniGame);
	if (%this.deathCount > 0 && !isEventPending(%this.trialSchedule) && !%this.vote)
	{
		%miniGame.messageAll('', '\c0One or more bodies have been discovered on school premises! \c5You guys have 5 minutes to investigate them before voting period starts.');
		%miniGame.messageAll('', '\c5Picking the right murderer will mean that you guys win! However, if you guys pick the WRONG culprit... \c0EVERYONE DIES!');
		%this.trialSchedule = %this.schedule(300000, "trialStart", %miniGame);
		%miniGame.DisableWeapons();
	}
}
function DSGameMode_Trial::trialStart(%this, %miniGame)
{
	%this.vote = true;
	%miniGame.messageAll('', '\c5Time\'s up! Cast your vote via /vote *firstname* *lastname*!');
	%miniGame.messageAll('', '\c5You have \c360\c5 seconds to cast your votes.');
	%this.voteCount = 0;
	%this.trialSchedule = %this.schedule(60000, "checkVotes", %miniGame);
}
function DSGameMode_Trial::checkVotes(%this, %miniGame)
{
	%correct = 0;
	%wrong = 0;
	%miniGame.messageAll('', '\c5 There were \c3%1 votes in total!', %this.voteCount);
	for (%i = 1; %i <= %this.voteCount; %i++)
	{
		if (%this.votes[%i] $= %this.killer)
			%correct++;
		else
			%wrong++;
		%miniGame.messageAll('', '\c3%1\c5 voted %3%2.',
				%this.voters[%i].character.name SPC "("@%this.voters[%i].getPlayerName()@")",
				%this.votes[%i].character.name SPC "("@%this.votes[%i].getPlayerName()@")",
				%this.votes[%i] $= %this.killer ? "\c0" : "\c1");
	}
	if (%correct > %wrong)
	{
		%miniGame.messageAll('', '\c5You guys are right!');
		%win = true;
	}
	else
	{
		%miniGame.messageAll('', '\c5You guys are oh so very wrong.');
		%win = false;
	}
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];
		%player = %member.player;
		if (!isObject(%player))
			continue;
		if ((%member $= %this.killer) == %win)
			%player.kill();
	}

	%this.onEnd(%miniGame, !%win ? %this.killer : "");
}
package DSTrialPackge
{
	function serverCmdVote(%client, %a, %b)
	{
		if (!%client.inDefaultGame() || !isObject(%client.player)) return;
		%search = trim(%a SPC %b);
		%miniGame = %client.miniGame;
		if (!%miniGame.gameMode.vote) return;
		%a = 0;
		for (%i = 0; %i < %miniGame.numMembers; %i++)
		{
			%member = %miniGame.member[%i];

			if (!isObject(%member.character))
				continue;

			if (striPos(%member.character.name, %search) != -1)
			{
				%pick[%a++] = %member;
			}
		}
		if (%a > 1)
		{
			messageClient(%client, '', '\c6Please input a more specific name. There were %1 matches for the one you gave!', %a);
			return;
		}
		if (%a <= 0)
		{
			messageClient(%client, '', '\c6There were no matches for given name!');
			return;
		}
		%miniGame.GameMode.voters[%miniGame.GameMode.voteCount++] = %client;
		%miniGame.GameMode.votes[%miniGame.GameMode.voteCount] = %pick[1];
		messageClient(%client, '', '\c6You have cast your vote for %1.', %pick[1].character.name);
	}
};