// Default Killer GameMode.
$DS::GameMode::Trial::InvestigationPeriod = 300; //5 mins
$DS::GameMode::Trial::TrialPeriod = 600; //10 mins
if (!isObject(DSGameMode_Trial))
{
	new ScriptObject(DSGameMode_Trial)
	{
		name = "Despair Trial";
		desc = "Someone's plotting murder against one of the students... Figure out who it is in a courtroom trial!";
		class = DSGameMode;
	};
}

datablock ItemData(MemorialItem)
{
	canPickUp = false;
	doColorShift = true;
	colorShiftColor = "0.7 0.7 0.2 1";
	shapeFile = $DS::Path @ "res/shapes/props/memorial.dts";

	uiName = "Memorial";
	canDrop = false;

	mass = 2;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

function DSGameMode_Trial::onMiniGameJoin(%this, %miniGame, %client)
{
	parent::onMiniGameJoin(%this, %miniGame, %client);
	// Abusable (potentially)
	// if (!isObject(DSTrialGameMode_Queue))
	// 	new SimSet(DSTrialGameMode_Queue);
	// DSTrialGameMode_Queue.add(%client);
}

function DSGameMode_Trial::onMiniGameLeave(%this, %miniGame, %client)
{
	parent::onMiniGameLeave(%this, %miniGame, %client);
	DSTrialGameMode_Queue.remove(%client);
}

function DSGameMode_Trial::onStart(%this, %miniGame)
{
	parent::onStart(%this, %miniGame);
	%this.deathCount = 0;
	%this.vote = false;
	%this.trial = false;
	%this.madekiller = false;
	%this.killer = "";
	%this.announcements = 0;
	%this.forceVoteCount = 0;
	%this.forceTrialCount = 0;
	%this.killerRevealed = false;
	cancel(%this.trialSchedule);
	activatepackage(DSTrialPackge);
	%miniGame.messageAll('', '\c5The culprit will be decided on by night. The first day doesn\'t have a killer, so use this time to study each other\'s behaviours.');
	%miniGame.messageAll('', '\c5The lore is that everyone was dragged into a murder game by a psycho mastermind. Nobody knows who the mastermind is...');
	%miniGame.messageAll('', '\c5Rules are: The culprit kills someone and has to get away with it. Once bodies are found, investigation period starts, and after that, the vote.');
	%miniGame.messageAll('', '\c5If you guys vote the CORRECT CULPRIT, everyone survives and culprit dies. HOWEVER, if you guys are WRONG, the culprit lives and EVERYONE ELSE DIES.');
	%miniGame.messageAll('', '<font:impact:30>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE CULPRIT!!');
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
		messageClient(%this.killer, '', '<font:impact:30>YOU BECAME THE KILLER! Self defence? Murder? Doesn\'t matter. Now you have to get away with it. Nobody must know.');
		%this.killer.centerPrint("<font:impact:30>YOU BECAME THE KILLER!", 3);
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
function DSGameMode_Trial::checkLastManStanding(%this, %miniGame)
{
	parent::checkLastManStanding(%this, %miniGame);
	if (!isObject(%this.killer) && %this.madekiller) //KILLER LEFT FUCK HIM
	{
		%miniGame.messageAll('', "\c5THE KILLER LEFT THE FUCKING GAME");
		%this.onEnd(%miniGame, ""); //It's in place so I don't have to reset round every time killer leaves myself.
	}
}
function DSGameMode_Trial::onDay(%this, %miniGame)
{
	parent::onDay(%this, %miniGame);
	%this.checkInvestigationStart(%miniGame);
	if (%this.deathCount <= 0 && %miniGame.days >= 3 && !%this.killerRevealed) //at 3rd day, someone will be tipped off about the murderer
	{
		for (%i = 0; %i < %miniGame.numMembers; %i++)
		{
			%member = %miniGame.member[%i];
			%player = %member.player;
			if (!isObject(%player) || %this.killer == %member)
				continue;
			%alivePlayers[%count++] = %member;
		}
		if (%count <= 0)
			return;
		%tipoff = %alivePlayers[getRandom(1, %count)];
		messageClient(%tipoff, '', '<font:impact:30>You suddenly realise the true culprit: %1! Your objective is to kill them to steal their role. Do not reveal this information to anyone.', %this.killer);
		messageAdmins(%tipoff.getPlayerName() SPC "was told who the killer is!");
		%this.killerRevealed = true;
	}
}
function DSGameMode_Trial::onNight(%this, %miniGame)
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
		%this.killer.player.regenStaminaDefault *= 2;
		%this.killer.player.exhaustionImmune = true;
		%this.killer.play2d(KillerJingleSound);
		%msg = "<color:FF0000>You are plotting murder against someone! Kill them and do it in such a way that nobody finds out it\'s you!";
		messageClient(%this.killer, '', "<font:impact:30>" @ %msg);
		commandToClient(%this.killer, 'messageBoxOK', "MURDER TIME!", %msg);
	}
	parent::onNight(%this, %miniGame);
	//%this.checkInvestigationStart(%miniGame);
}
function DSGameMode_Trial::onBodyExamine(%this, %miniGame, %client, %body)
{
	for (%i=1; %i <= %body.bodyDiscoveries; %i++) //Check if the client had already discovered the body
	{
		if (%body.Discovered[%i] $= %client)
			return;
	}
	if (!%body.ignore && !%body.suicide && !%body.unconscious)
	{
		%body.Discovered[%body.bodyDiscoveries++] = %client;
		%client.play2d(bodyDiscoveryNoise @ getRandom(1,2));
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
	setEnvironment("skyColor", "0 0 0"); //da void
	for (%i = 0; %i < GameCharacters.getCount(); %i++)
	{
		%character = GameCharacters.getObject(%i);
		%player = %character.player;
		if (!isObject(%character))
			continue;

		%stand = BrickGroup_888888.NTObject["_courtstand_" @ %character.room, 0];
		%center = BrickGroup_888888.NTObject["_courtroom_center", 0];
		if (!isObject(%player) || %player.getState() $= "Dead")
		{
			%sign = new Item() {
				dataBlock = MemorialItem;
				position = vectorAdd(getWords(%stand.getTransform(), 0, 2), "0 0 1.9");
				static = true;
			};
			missionCleanUp.add(%sign);
			GameRoundCleanup.add(%sign);
			%sign.PointAt(getWords(%center.getTransform(), 0, 2));
			%status = "(LEFT)";
			if (isObject(%player))
				%status = %player.ignore ? "(FREEKILL)" : (%player.suicide ? "(SUICIDE)" : "(DEAD)");
			%sign.setShapeName(%character.name SPC %status);
			%sign.setShapeNameColor("0.5 0.5 0.5");
			continue;
		}
		%player.setTransform(vectorAdd(getWords(%stand.getTransform(), 0, 2), "0 0 0.3"));
		%player.PointAt(getWords(%center.getTransform(), 0, 2));
		%player.setVelocity("0 0 0"); //Prevents sliding around
		%player.wakeUp();
		%player.setShapeNameDistance(120);
		%player.changeDataBlock(PlayerDSFrozenArmor);
	}
	%this.trial = true;
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
	cancel(%this.trialSchedule);
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
		if (%pick[1] == %client)
		{
			messageClient(%client, '', '\c6You cannot vote for yourself!');
			return;
		}
		%miniGame.GameMode.voters[%miniGame.GameMode.voteCount++] = %client;
		%miniGame.GameMode.votes[%miniGame.GameMode.voteCount] = %pick[1];
		messageClient(%client, '', '\c6You have cast your vote for %1.', %pick[1].character.name);
	}

	function serverCmdForceTrial(%client)
	{
		%gameMode = $defaultMiniGame.gameMode;
		if (!isObject(%gameMode))
			return;
		if (%gameMode.deathCount > 0 && !%gameMode.vote && !%gameMode.trial)
		{
			if (%client.inDefaultGame())
			{
				if (!isObject(%client.player) || !isObject(%client.character))
					return;
				for (%i = 0; %i < $defaultMiniGame.numMembers; %i++)
				{
					%member = $defaultMiniGame.member[%i];
					%player = %member.player;
					if (!isObject(%player))
						continue;
					%alivePlayers++;
				}
				for (%i = 1; %i <= %gameMode.forceTrialCount; %i++)
				{
					%member = %gameMode.forceTrials[%i];

					if (!isObject(%member) || !isObject(%member.character))
						continue;
					if (%member == %client)
					{
						messageClient(%client, '', '\c6You already voted!');
						return;
					}
					%validvotes++;
				}
				%gameMode.forceTrials[%gameMode.forceTrialCount++] = %client;
				%validvotes++;
				if (%validvotes >= (MFloor(%alivePlayers * 0.9))) // if at least 90% of alive players voted
				{
					$defaultMiniGame.messageAll('', '\c3%1 has voted to start the trial early!\c6 There are enough votes to force the trial period.',
						%client.character.name);
					%start = true;
				}
				else
				{
					echo("ForceTrial Votes left:" SPC MFloor(%alivePlayers * 0.9) - %validvotes);
					$defaultMiniGame.messageAll('', '\c3%1 has voted to start the trial early!\c6 Do /forcetrial to concur.', //No votes revealed due to meta possibilities
						%client.character.name);
				}
			}
			else if (%client.isAdmin) //"Admin" forcetrial only works outside minigame
				%start = true;
			if (%start)
				%gameMode.trialStart($defaultMiniGame);
		}
	}

	function serverCmdForceVote(%client)
	{
		%gameMode = $defaultMiniGame.gameMode;
		if (!isObject(%gameMode))
			return;
		if (%gameMode.deathCount > 0 && !%gameMode.vote && %gamemode.trial)
		{
			if (%client.inDefaultGame())
			{
				if (!isObject(%client.player) || !isObject(%client.character))
					return;
				for (%i = 0; %i < $defaultMiniGame.numMembers; %i++)
				{
					%member = $defaultMiniGame.member[%i];
					%player = %member.player;
					if (!isObject(%player))
						continue;
					%alivePlayers++;
				}
				for (%i = 1; %i <= %gameMode.forceVoteCount; %i++)
				{
					%member = %gameMode.forceVotes[%i];

					if (!isObject(%member) || !isObject(%member.character))
						continue;
					if (%member == %client)
					{
						messageClient(%client, '', '\c6You already voted!');
						return;
					}
					%validVotes++;
				}
				%gameMode.forceVotes[%gameMode.forceVoteCount++] = %client;
				%validVotes++;
				if (%validVotes >= (MFloor(%alivePlayers * 0.8))) // if at least 90% of alive players voted
				{
					$defaultMiniGame.messageAll('', '\c3%1 has voted to start the vote early!\c6 There are enough votes to force the voting period.',
						%client.character.name);
					%start = true;
				}
				else
				{
					$defaultMiniGame.messageAll('', '\c3%1 has voted to start the vote early!\c6 Do /forcevote to concur. %2 votes left.',
						%client.character.name, MFloor(%alivePlayers * 0.8) - %validVotes);
				}
			}
			else if (%client.isAdmin) //"Admin" forcevote only works outside minigame
				%start = true;
			if (%start)
				%gameMode.voteStart($defaultMiniGame);
		}
	}
};