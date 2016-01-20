// Default Killer GameMode.
$DS::GameMode::Trial::InvestigationPeriod = 300; //5 mins
$DS::GameMode::Trial::TrialPeriod = 180; //3 mins
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
	%this.deathCount = 0;
	%this.vote = false;
	%this.bodyDiscoveries = 0;
	%this.killer = "";
	%this.announcements = 0;
	cancel(%this.trialSchedule);
	activatepackage(DSTrialPackge);
	%miniGame.messageAll('', '\c5The culprit will be decided on by night. The first day doesn\'t have a killer, so use this time to study each other\'s behaviours.');
	%miniGame.messageAll('', '\c5The lore is that everyone was dragged into a murder game by a psycho mastermind. Nobody knows who the mastermind is...');
	%miniGame.messageAll('', '\c5Rules are: The culprit kills someone and has to get away with it. Once bodies are found, investigation period starts, and after that, the vote.');
	%miniGame.messageAll('', '\c5If you guys vote the CORRECT CULPRIT, everyone survives and culprit dies. HOWEVER, if you guys are WRONG, the culprit lives and EVERYONE ELSE DIES.');
	%miniGame.messageAll('', '<font:impact:30>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE CULPRIT!!');
}
function DSGameMode_Trial::onEnd(%this, %miniGame, %winner)
{
	if (isEventPending(%miniGame.resetSchedule))
		return;
	cancel(%this.trialSchedule);
	deactivatepackage(DSTrialPackge);
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
		%this.killer.play2d(KillerJingleSound);
		messageClient(%this.killer, '', '<font:impact:30>You ended up killing someone. Self defence? Murder? Doesn\'t matter. Now you have to get away with it. Nobody must know.');
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

	%this.deathCount++;
	if (%this.deathCount >= 3) //This is one kill more than the voting check.
		%miniGame.DisableWeapons();
}
function DSGameMode_Trial::onDay(%this, %miniGame)
{
	parent::onDay(%this, %miniGame);
	%this.checkInvestigationStart(%miniGame);
}
function DSGameMode_Trial::onNight(%this, %miniGame)
{
	parent::onNight(%this, %miniGame);
	%this.checkInvestigationStart(%miniGame);
	if (!isObject(%this.killer))
	{
		%count = 0;
		for (%i = 0; %i < %miniGame.numMembers; %i++)
		{
			%member = %miniGame.member[%i];

			if (isObject(%member.player))
				%alivePlayers[%count++] = %member;
		}
		%this.killer = $DS::GameMode::ForceKiller !$= "" ? $DS::GameMode::ForceKiller : %alivePlayers[getRandom(1, %count)];
		%this.killer.player.regenStaminaDefault *= 2;
		%this.killer.play2d(KillerJingleSound);
		%msg = "<color:FF0000>You are plotting murder against someone! Kill them and do it in such a way that nobody finds out it\'s you!";
		messageClient(%this.killer, '', "<font:impact:30>" @ %msg);
		commandToClient(%this.killer, 'messageBoxOK', "MURDER TIME!", %msg);
	}
}
function DSGameMode_Trial::onBodyExamine(%this, %miniGame, %client, %body)
{
	for (%i=1; %i <= %body.bodyDiscoveries; %i++) //Check if the client had already discovered the body
	{
		if (%body.Discovered[%i] $= %client)
			return;
	}
	for (%i=1;%i<=%body.attackCount;%i++) //Parse attack logs for info
	{
		if (%body.attackType[%i] $= "Suicide")
			%suicide = true;
	}
	if (!%body.ignore && !%suicide && !%body.unconscious)
	{
		%body.Discovered[%body.bodyDiscoveries++] = %client;
		%client.play2d(bodyDiscoveryNoise);
		messageClient(%client, '', "<font:impact:22>You have discovered a body!");
		if (%body.bodyDiscoveries >= 2 && !%body.announced)
		{
			%body.announced = true;
			%this.checkInvestigationStart(%miniGame, 1);
			%this.makeBodyAnnouncement(%miniGame);
		}
	}
}
function DSGameMode_Trial::makeBodyAnnouncement(%this, %miniGame, %multiple)
{
	serverPlay2d(AnnouncementJingleSound);
	if (!%multiple)
		%this.announcements++;
	%miniGame.messageAll('', '\c0%2 on school premises! \c5You guys have %1 minutes to investigate them before the trial starts.',
		MCeil((%this.investigationStart - $Sim::Time)/60), %multiple ? "There are corpses to be found" : (%this.announcements > 1 ? "Another body has been discovered" : "A body has been discovered"));
}
function DSGameMode_Trial::checkInvestigationStart(%this, %miniGame, %no_announce)
{
	if (%this.deathCount >= 2) //Only disable weapons when there are 2 murders max
		%miniGame.DisableWeapons();
	if (%this.deathCount > 0 && !isEventPending(%this.trialSchedule) && !%this.vote)
	{
		%this.investigationStart = $Sim::Time + $DS::GameMode::Trial::InvestigationPeriod;
		if (!%no_announce)
			%this.makeBodyAnnouncement(%miniGame, %this.deathCount > 0);
		%this.trialSchedule = %this.schedule($DS::GameMode::Trial::InvestigationPeriod*1000, "trialStart", %miniGame);
	}
}
function DSGameMode_Trial::trialStart(%this, %miniGame)
{
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];
		%player = %member.player;
		if (!isObject(%member.character))
			continue;

		%character = %member.character;
		%stand = BrickGroup_888888.NTObject["_courtstand_" @ %character.room, 0];
		%center = BrickGroup_888888.NTObject["_courtroom_center", 0];
		if (!isObject(%player))
		{
			%sign = new Item() {
				dataBlock = hammerItem;
				position = vectorAdd(getWords(%stand.getTransform(), 0, 2), "0 0 2");
				static = true;
			};
			missionCleanUp.add(%sign);
			GameRoundCleanup.add(%sign);
			%sign.setShapeName(%character.name SPC "(DEAD)");
			%sign.setShapeNameColor("0.5 0.5 0.5");
			continue;
		}
		%player.setTransform(vectorAdd(getWords(%stand.getTransform(), 0, 2), "0 0 0.3"));
		%player.setVelocity("0 0 0"); //Prevents sliding around
		%player.setShapeNameDistance(120);
		%player.wakeUp();
		%player.changeDataBlock(PlayerDSFrozenArmor);
	}
	%miniGame.DisableWeapons();
	%miniGame.messageAll('', '\c5Investigation period is now OVER! Everyone will be teleported to the courtroom.');
	%miniGame.messageAll('', '\c5You guys have %1 minutes until the voting period starts. This is a good time to discuss who the killer is with everyone present!', $DS::GameMode::Trial::TrialPeriod/60);
	%miniGame.messageAll('', '\c5Picking the right murderer will mean that you guys win! However, if you guys pick the WRONG culprit... \c0EVERYONE DIES!');
	loadEnvironment($DS::Path @ "data/env/day.txt");
	cancel(%miniGame.DayTimeSchedule);
	cancel(%this.trialSchedule);
	%this.trialSchedule = %this.schedule($DS::GameMode::Trial::TrialPeriod*1000, "voteStart", %miniGame);
}

function DSGameMode_Trial::voteStart(%this, %miniGame)
{
	cancel(%this.trialSchedule);
	serverPlay2d(votingTimeSound);
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
		for (%i = 1; %i <= %miniGame.gameMode.voteCount; %i++)
		{
			if (%miniGame.gameMode.voters[%i] == %client)
			{
				messageClient(%client, '', '\c6You already voted!');
				return;
			}
		}
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

	function serverCmdForceTrial(%client)
	{
		if (!%client.isAdmin)
			return;
		%gameMode = $defaultMiniGame.gameMode;
		if (!isObject(%gameMode))
			return;
		if (%gameMode.deathCount > 0 && !%gameMode.vote)
			%gameMode.trialStart($defaultMiniGame);
	}

	function serverCmdForceVote(%client)
	{
		if (!%client.isAdmin)
			return;
		%gameMode = $defaultMiniGame.gameMode;
		if (!isObject(%gameMode))
			return;
		if (%gameMode.deathCount > 0 && !%gameMode.vote)
			%gameMode.voteStart($defaultMiniGame);
	}
};