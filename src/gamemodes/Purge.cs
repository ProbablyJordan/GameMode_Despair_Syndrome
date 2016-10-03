if (!isObject(DSGamemode_Purge))
{
	new ScriptObject(DSGamemode_Purge)
	{
		name = "The Purge";
		desc = "The first night is the Purge. Whoever murders on that night becomes a killer for the rest of the round!";
		class = DSGameMode;
		omit = true;
		minPlayers = 10;
	};
}

function DSGamemode_Purge::getBottomPrintText(%this, %minigame, %cl)
{
	%msg = Parent::getBottomPrintText(%this, %minigame, %cl);
	if ((%cam = %cl.getControlObject()).getType() & $Typemasks::CameraObjectType
		&& isObject(%targ = %cam.getOrbitObject().client))
	{
		%cl = %targ;
		%isSpectate = 1;
	}
	%role = "\c7Undecided";
	if (%isSpectate)
		%role = "\c7Unknown";
	else if (!isObject(%cl.player))
		%role = "\c7Corpse";
	else if (DSPurgeKillers.isMember(%cl))
		%role = "\c0Killer";
	else if(%this.madeKiller)
		%role = "\c2Innocent";
	%field = getFirstField(%msg, "\c6Role");
	%msg = setField(%msg, %field, "\c6Role: " @ %role);
	return %msg;
}

function DSGameMode_Purge::onMiniGameLeave(%this, %miniGame, %client)
{
	parent::onMiniGameLeave(%this, %miniGame, %client);
	DSTrialGameMode_Queue.remove(%client);
}

function DSGameMode_Purge::onStart(%this, %miniGame)
{
	parent::onStart(%this, %miniGame);
	%this.slaughterNotify = false;
	%this.madeKiller = false;
	%this.trial = false;
	cancel(%this.trialSchedule);
	%miniGame.messageAll('', '\c5The school\'s occult club has prophesized that tonight will be the Purge. Tonight, many murderers shall rise and attempt to slay everyone in their path.');
	%miniGame.messageAll('', '\c5The objective of the innocents is to lynch all the murderers from the night of the Purge.');
	%miniGame.messageAll('', '\c5The objective of the killers is to kill the remaining survivors before they are all killed off.');
	%miniGame.messageAll('', '\c5Note that while you cannot kill during the day, killer role stealing from Despair Trial is still in play.');
	//%miniGame.messageAll('', '<font:impact:20>\c3DO NOT MURDER WITHOUT REASON THIS ROUND UNLESS YOU\'RE THE KILLER!!');
	if (!isObject(DSPurgeKillers))
		new SimSet(DSPurgeKillers);
	else
		DSPurgeKillers.clear();
	if (!isObject(DSPurgeTrialItems))
		new SimSet(DSPurgeTrialItems);
}

function DSGameMode_Purge::onEnd(%this, %miniGame, %winner, %ignore)
{
	if (isEventPending(%miniGame.resetSchedule))
		return;
	%count = %minigame.numMembers;
	for (%i = 0; %i < %count; %i++)
	{
		%member = %minigame.member[%i];
		if (DSPurgeKillers.isMember(%member))
			%killers++;
		else
			%victims++;
	}
	if ((%killers == 0) == (%victims == 0))
	{
		if (%ignore)
			return;
		else
			%endText = "\c3Nobody wins!" @ (%killers == 0 ? "" : " \c4(This shouldn't actually happen, yell at Xalos if you see this)");
	}
	else if (%killers == 0)
		%endText = "\c3The innocents win!";
	else
		%endText = "\c3The killers win!";
	%this.trial = 0;
	%miniGame.messageAll('', %endtext SPC "\c5A new game will begin in 15 sceonds.");
	%miniGame.scheduleReset(10000);
}

function DSGameMode_Purge::onDeath(%this, %miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc)
{
	if (%this.madeKiller)
		DSGameMode_Purge.onEnd(%miniGame, "", 1);
	if (%this.trial && isObject(%pl = %client.player))
		DSPurgeTrialItems.add(%pl);
	if (%sourceClient == %client || %sourceClient == 0)
	{
		if (DSPurgeKillers.isMember(%client))
			DSPurgeKillers.remove(%client);
		return;
	}
	if (DSPurgeKillers.isMember(%client))
	{
		DSPurgeKillers.remove(%client);
		if (DSPurgeKillers.isMember(%sourceClient))
			return;
		DSPurgeKillers.add(%sourceClient);
		%sourceClient.play2d(KillerJingleSound);
		messageClient(%sourceClient, '', '<font:impact:30>YOU BECAME A KILLER! Self defence? Murder? Doesn\'t matter. Now you have to get away with it. Nobody must know.');
		//messageClient(%sourceClient, '', '\c5Put $ before your message to use killer chat, but not too close to innocent players! (like so: "$killer")');
		//messageClient(%sourceClient, '', '\c5Put % before your message to shout in killer chat, but be careful - it\'s even riskier than $! (like so: "%killer")');
		%sourceClient.centerPrint("<font:impact:30>YOU BECAME A KILLER!", 3);
		//%sourceClient.player.regenStaminaDefault *= 2;
		//%sourceClient.player.exhaustionImmune = true;
	}
	else if (!DSPurgeKillers.isMember(%sourceClient)) //Freekill?
	{
		if (%minigame.currTime $= "Night" && !%minigame.madeKiller)
		{
			DSPurgeKillers.add(%sourceClient);
			%sourceClient.play2d(KillerJingleSound);
			messageClient(%sourceClient, '', '<font:impact:30>YOU BECAME A KILLER! Now you have to get away with it. Nobody must know.');
			//messageClient(%sourceClient, '', '\c5Put $ before your message to use killer chat, but not too close to innocent players! (like so: "$killer")');
			//messageClient(%sourceClient, '', '\c5Put % before your message to shout in killer chat, but be careful - it\'s even riskier than $! (like so: "%killer")');
			%sourceClient.centerPrint("<font:impact:30>YOU BECAME A KILLER!", 3);
			//%sourceClient.player.regenStaminaDefault *= 2;
			//%sourceClient.player.exhaustionImmune = true;
			return;
		}
		//Maybe penalize?
		%log = %sourceClient.getPlayerName() SPC "just killed" SPC %client.getPlayerName() SPC "as a non-killer.";
		echo("\c2" SPC %log);
		freekillRecordLine(%sourceClient, "FREEKILL: {0} killed {1} (BL_ID: {2})!", %client.name, %client.bl_id);
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);
			if (%other.getModLevel() != 0)
			{
				messageClient(%other, '', "FREEKILL:" SPC %log);
				commandToClient(%other, 'API_Freekill', %sourceClient, %client);
			}
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
	if (%count <= 1 && %this.madeKiller)
	{
		%this.onEnd(%miniGame, %alivePlayers[%count]);
	}
	else if (DSPurgeKillers.getCount() == 0 && %this.madekiller) //Killer left the game?
		%this.onEnd(%miniGame);
}
function DSGameMode_Purge::onDay(%this, %miniGame)
{
	Parent::onDay(%this, %miniGame);
	%count = %minigame.numMembers;
	for (%i = 0; %i < %count; %i++)
		if (isObject(%minigame.member[%i].player))
			%alive++;
	if (%alive > 4)
		%minigame.DisableWeapons();
	if (!%this.madeKiller && DSPurgeKillers.getCount() != 0)
	{
		%this.madeKiller = true;
		%miniGame.messageAll('', '<font:impact:20>\c3The Purge has passed and now the innocents must survive long enough to vote off all the killers! There will be a trial at 2 PM!');
	}
	else if (%this.madeKiller)
		%miniGame.messageAll('', '<font:impact:20>\c3The innocent players have survived another night, and so there will be another trial at 2 PM!');
	if (%alive <= 4 && !%this.slaughterNotify)
	{
		%miniGame.messageAll('', '<font:impact:20>\c3Four or fewer players remain alive - weapons will not be disabled today!');
		%this.slaughterNotify = true;
	}
	if (%this.madeKiller)
	{
		%time = $DS::Time::DayLength * 500;
		%this.trialSchedule = %this.schedule(%time, "trialStart", %miniGame);
	}
}
function DSGamemode_Purge::trialStart(%this, %miniGame)
{
	setEnvironment("skyColor", "0 0 0"); //da void
	for (%i = 0; %i < GameCharacters.getCount(); %i++)
	{
		%character = GameCharacters.getObject(%i);
		%player = %character.player;

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
			DSPurgeTrialItems.add(%sign);
			%sign.PointAt(getWords(%center.getTransform(), 0, 2));
			%status = "(LEFT)";
			if (isObject(%player) && !%player.clientLeft)
				%status = %player.lynched ? "(LYNCHED)" : (%player.ignore ? "(FREEKILL)" : (%player.suicide ? "(SUICIDE)" : "(DEAD)"));
			%sign.setShapeName(%character.name SPC %status);
			%sign.setShapeNameColor("0.5 0.5 0.5");
			continue;
		}
		%player.restoreSleep = %player.currSleepLevel;
		%player.prevTransform = %player.getTransform();
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
	%miniGame.messageAll('', '\c5You guys have two minutes to discuss evidence and cast your votes. This is a good time to discuss who the killers may be with everyone present!');
	%miniGame.messageAll('', '\c5The person who the most innocent players vote for will DIE! Once voting is over, everyone will be returned to the school until the next day.');
	%miniGame.messageAll('', '\c4USE /VOTE NAME TO VOTE FOR SOMEONE!');
	//loadEnvironment($DS::Path @ "data/env/day.txt");
	cancel(%this.trialSchedule);
	%this.voteCount = 0;
	%this.trialSchedule = %this.schedule(120000, "checkVotes", %miniGame);
}
function DSGamemode_Purge::checkVotes(%this, %miniGame)
{
	cancel(%this.trialSchedule);
	%max = 0;
	%target = -1;
	%miniGame.messageAll('', '\c5 There were \c3%1 votes in total!', %this.voteCount);
	for (%i = 1; %i <= %this.voteCount; %i++)
	{
		//if (DSPurgeKillers.isMember(%this.voters[%i]))
		//	continue;
		%for[%this.votes[%i]]++;
		if (%for[%this.votes[%i]] == %max)
			%target = "";
		else if (%for[%this.votes[%i]] > %max)
		{
			%target = %this.votes[%i];
			%max = %for[%this.votes[%i]];
		}
	}
	if (!isObject(%target))
	{
		%max = 0;
		%target = -1;
		if (DSPurgeKillers.isMember(%this.voters[%i]))
			continue;
		%for[%this.votes[%i]]++;
		if (%for[%this.votes[%i]] == %max)
			%target = "";
		else if (%for[%this.votes[%i]] > %max)
		{
			%target = %this.votes[%i];
			%max = %for[%this.votes[%i]];
		}
	}
	if (isObject(%target))
	{
		%minigame.messageAll('', '\c5%1 (%2\c5) has been lynched by the innocents!', %target.character.name, DSPurgeKillers.isMember(%target) ? "\c0Killer" : "\c2Innocent");
		DSPurgeKillers.remove(%target);
		if (isObject(%pl = %target.player))
		{
			%pl.kill();
			%pl.lynched = 1;
			DSPurgeTrialItems.add(%pl);
		}
		%this.onEnd(%minigame, "", 1);
		if (isEventPending(%minigame.resetSchedule))
			return;
	}
	
	%this.schedule(5000, "returnPort", %minigame);
}

function DSGamemode_Purge::returnPort(%this, %minigame)
{
	%this.trial = 0;
	for (%i = 0; %i < GameCharacters.getCount(); %i++)
	{
		%character = GameCharacters.getObject(%i);
		%player = %character.player;
		if (!isObject(%player) || %player.getState() $= "Dead")
			continue;
		%player.itemOffset = 0;
		%player.setTransform(%player.prevTransform);
		%player.prevTransform = "";
		%player.currSleepLevel = %player.restoreSleep;
		%player.changeDatablock(PlayerDSArmor);
	}
	for (%i = DSPurgeTrialItems.getCount() - 1; %i >= 0; %i--)
	{
		%item = DSPurgeTrialItems.getObject(0);
		if (isObject(%pl = %item.returnPlayer) && %pl.getState() !$= "DEAD")
		{
			%pl.tool[%item.returnSlot] = %item.getDatablock().getID();
			%pl.itemProps[%item.returnSlot] = %item.itemProps;
			%item.itemProps = "";
			if (isObject(%cl = %pl.client))
				messageClient(%pl.client, 'MsgItemPickup', '', %item.returnSlot, %item.getDatablock().getID(), 1);
		}
		if (%item.getType() & $Typemasks::CorpseObjectType)
		{
			%item.setTransform(%item.prevTransform);
			%item.prevTransform = "";
			DSPurgeTrialItems.remove(%item);
		}
		else
			%item.delete();
	}
	if (%this.slaughterNotify)
		%minigame.EnableWeapons();
}
function DSGameMode_Purge::onNight(%this, %miniGame)
{
	if (!%this.madeKiller)
		%miniGame.messageAll('', '<font:impact:20>\c3Tonight is the night of the Purge... if you kill someone tonight, you will become a killer for the rest of the round!');
	else
		%miniGame.messageAll('', '<font:impact:20>\c3Everyone who killed on the night of the Purge should now continue trying to kill off the innocents!');
	%minigame.EnableWeapons();
	Parent::onNight(%this, %miniGame);
}
function DSGamemode_Purge::isKiller(%this, %minigame, %cl)
{
	return %cl.minigame == %minigame && DSPurgeKillers.isMember(%cl);
}

package DSPurgePackage
{
	function serverCmdVote(%client, %a, %b, %c, %d, %e, %f)
	{
		if (!%client.inDefaultGame() || !isObject(%client.player))
		{
			Parent::servercmdVote(%client, %a, %b, %c, %d, %e, %f);
			return;
		}
		%search = trim(%a SPC %b);
		%miniGame = %client.miniGame;
		if (%minigame.gamemode.getID() != DSGamemode_Purge.getID() || !%miniGame.gameMode.trial)
		{
			Parent::servercmdVote(%client, %a, %b, %c, %d, %e, %f);
			return;
		}
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

			if (!isObject(%member.character) || !isObject(%pl = %member.player) || %pl.getState() $= "DEAD")
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
	
	function servercmdDropTool(%cl, %slot)
	{
		if (!%cl.inDefaultGame() || !(%mode = %cl.minigame.gamemode).trial
			|| %mode.getID() != DSGamemode_Purge.getID() || !isObject(%pl = %cl.player))
			Parent::servercmdDropTool(%cl, %slot);
		else
		{
			if (!isObject(%pl.tool[%slot]))
				return;
			%tool = %pl.tool[%slot];
			%pl.tool[%slot] = "";
			messageClient(%cl, 'MsgItemPickup', '', %slot, -1, 1);
			%value = 3.825 + %pl.itemOffset;
			if (isObject(%props = %pl.itemProps[%slot]))
				%pl.itemProps[%slot] = "";
			%item = new Item()
			{
				position = "0 0 0";
				datablock = %tool;
				itemProps = %props;
				minigame = %cl;
				static = true;
				rotate = true;
			};
			%value -= getWord(%box = %item.getWorldBox(), 2);
			%height = getWord(%box, 5) - getWord(%box, 2);
			%pl.itemOffset += %height + 0.2;
			%offset = setWord("0 0 0", 2, %value);
			%item.setTransform(vectorAdd(%pl.getPosition(), %offset));
			if (%tool.getName() $= "KeyItem")
				%item.setShapeName(%item.itemProps.name);
			else if (%tool.getName() $= "KeyJanitorItem")
				%item.setShapeName(strReplace(%item.itemProps.name, "Key", "Janitor Key"));
			else
				%item.setShapeName(%tool.uiName);
			MissionCleanup.add(%item);
			GameRoundCleanup.add(%item);
			DSPurgeTrialItems.add(%item);
			%item.returnPlayer = %pl;
			%item.returnSlot = %slot;
		}
	}
};
activatePackage("DSPurgePackage");