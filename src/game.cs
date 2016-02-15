$DS::RoomCount = 16;
$DS::MaxPlayers = $DS::RoomCount;
$DS::Time::DayLength = 300; //5 mins
$DS::Time::NightLength = 150; //2.5 mins
$DS::GameVoteRounds = 3;

if (!isObject(GameRoundCleanup))
	new SimSet(GameRoundCleanup);

if (!isObject(GameCharacters))
	new SimSet(GameCharacters);

function MiniGameSO::DayTimeSchedule(%this)
{
	cancel(%this.DayTimeSchedule);
	if (isEventPending(%this.scheduleReset))
		return;
	%this.currtime = %this.currTime $= "Day" ? "Night" : "Day";
	%time = %this.currTime $= "Day" ? $DS::Time::DayLength : $DS::Time::NightLength;
	%this.messageAll('', '\c5It is now \c3%1\c5.', %this.currTime);
	if (%this.currTime $= "Day")
		%this.gameMode.onDay(%this);
	else
		%this.gameMode.onNight(%this);
	%this.DayTimeSchedule = %this.schedule(%time * 1000, "DayTimeSchedule");
}
function MiniGameSO::DisableWeapons(%this)
{
	%this.noWeapons = true;
	for (%i = 0; %i < %this.numMembers; %i++)
	{
		%member = %this.member[%i];
		%player = %member.player;
		if (!isObject(%player))
			continue;
		if (%player.getMountedImage(0) && %player.getMountedImage(0).isWeapon)
			%player.unMountImage(0);
	}
}

package DespairSyndromePackage
{
	function GameConnection::inDefaultGame(%this)
	{
		return isObject($DefaultMiniGame) && %this.miniGame == $DefaultMiniGame;
	}

	function Armor::onNewDataBlock(%this, %obj)
	{
		parent::onNewDataBlock(%this, %obj);
		%obj.maxTools = %obj.getDataBlock().maxTools;
	}

	function Armor::onDisabled(%this, %obj)
	{
		if (isObject(%obj.client))
		{
			%obj.client.stopViewingInventory();
			if (%obj.client.inDefaultGame() && isObject(GameRoundCleanup))
			{
				%obj.inhibitRemoveBody = true;
				GameRoundCleanup.add(%obj);
			}
		}

		Parent::onDisabled(%this, %obj);
	}

	function Player::removeBody(%this)
	{
		if (!%this.inhibitRemoveBody)
			Parent::removeBody(%this);
	}

	function Player::mountImage(%this, %image, %slot, %loaded, %skinTag)
	{
		if (isObject(%this.client) && %this.client.inDefaultGame() && (%image.isWeapon && $DefaultMiniGame.noWeapons))
			return;
		parent::mountImage(%this, %image, %slot, %loaded, %skinTag);
	}

	function Armor::onUnMount(%this, %obj, %slot)
	{
		Parent::onUnMount(%data, %obj, %slot);

		if (%obj.isBody)
			%obj.playThread(3, "death1");
	}
	function MiniGameSO::addMember(%this, %member)
	{
		%empty = %this.numMembers < 1;
		Parent::addMember(%this, %member);
		if (%empty)
		{
			%this.reset(0);
		}
		if (isObject(%this.gameMode))
			%this.gameMode.onMiniGameJoin(%this, %member);
		if (!isObject(DSAdminQueue))
			new SimSet(DSAdminQueue);
		if (%this.numMembers - DSAdminQueue.getCount() >= $DS::RoomCount && %member.isAdmin)
		{
			DSAdminQueue.add(%member);
			messageClient(%member, '', "\c2You have been added to the admin queue due to max playerlimit being reached.");
		}
		messageClient(%member, '', '<font:arial bold:30>\c3Current mode is - \c2%1\c3! %2', %this.gameMode.name, %this.gameMode.desc);
	}
	function MiniGameSO::removeMember(%this, %member)
	{
		Parent::removeMember(%this, %member);
		if (isObject(%this.gameMode))
			%this.gameMode.onMiniGameLeave(%this, %member);
		if (%member.isAdmin)
			DSAdminQueue.remove(%member);
		%this.checkLastManStanding();
	}
	function MiniGameSO::Reset(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::reset(%this, %client);

		// Play nice with the default rate limiting.
		if (getSimTime() - %this.lastResetTime < 5000)
			return;

		//Doing this before initialising any of the gamemode stuffs might be a bit bad but w/e
		cancel(%this.voteSchedule);
		%this.rounds++;
		if (%this.rounds % ($DS::GameVoteRounds + 1) == $DS::GameVoteRounds) //Every $DS::GameVoteRounds rounds
		{
			cancel(%this.DayTimeSchedule);
			%this.voteCount = 0;
			%this.gameModeVote = true;
			announce("\c3It's time to vote for the next gametype! Available gametypes:");
			for (%i = 0; %i < DSGameModeGroup.GetCount(); %i++)
			{
				%obj = DSGameModeGroup.getObject(%i);
				if (!isObject(%obj) || %obj.omit)
					continue;
				announce("\c6 - \c2" @ %obj.name @ "\c6: " @ %obj.desc);
			}
			announce("\c3Cast your votes via /vote *gametype name*! You have \c660\c3 seconds to cast your votes.");
			%this.voteSchedule = %this.schedule(60000, "checkVotes");
			return;
		}
		//Voting check above^
		%this.gameModeVote = false;
		if (isObject(GameRoundCleanup))
			GameRoundCleanup.deleteAll();
		if (isObject(GameCharacters))
			GameCharacters.deleteAll();
		if (isObject(DecalGroup))
			DecalGroup.deleteAll();

		if (!isObject($DS::GameMode))
			$DS::GameMode = DSGameMode_Default;
		%this.gameMode = $DS::GameMode;
		Parent::reset(%this, %client);
		%this.gameMode.onStart(%this);
		%this.currTime = "Day";
		%this.days = 0;
		%this.gameMode.onDay(%this);
		cancel(%this.DayTimeSchedule);
		%this.DayTimeSchedule = %this.schedule(($DS::Time::DayLength/2) * 1000, "DayTimeSchedule");
	}
	//Gamemode voting, woo!
	function MiniGameSO::checkVotes(%this)
	{
		cancel(%this.voteSchedule);
		%miniGame.messageAll('', '\c3 There were \c2%1\c3 votes in total!', %this.voteCount);
		for (%i = 1; %i <= %this.voteCount; %i++)
		{
			%votesfor[%this.votes[%i]]++;
		}
		%a = 0;
		for (%i = 0; %i < DSGameModeGroup.GetCount(); %i++)
		{
			%obj = DSGameModeGroup.getObject(%i);
			if (!isObject(%obj) || %obj.omit)
				continue;
			if (%votesfor[%obj] > %majority)
				%majority = %votesfor[%obj];
			%contestants[%a++] = %obj;
			announce((%votesfor[%obj] $= "" ? "\c60" : ("\c2" @ %votesfor[%obj])) @ "\c6 votes for\c3" SPC %obj.name @ "!");
		}
		%c = 0;
		for (%i = 1; %i <= %a; %i++)
		{
			if (%votesfor[%contestants[%i]] >= %majority)
			{
				%choices[%c++] = %contestants[%i];
			}
		}
		%winner = %choices[getRandom(1, %c)];
		announce("\c3The winner is... \c6" @ %winner.name @ "!");
		$DS::GameMode = %winner;
		%this.scheduleReset(3000);
	}

	function serverCmdVote(%client, %a, %b, %c, %d, %e, %f)
	{
		if (!$DefaultMiniGame.gameModeVote)
		{
			parent::serverCmdVote(%client, %a, %b, %c, %d, %e, %f);
			return;
		}
		if (!%client.inDefaultGame())
			return;
		%miniGame = %client.miniGame;
		%search = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f);
		for (%i = 1; %i <= %miniGame.voteCount; %i++)
		{
			if (%miniGame.voters[%i] == %client)
			{
				messageClient(%client, '', '\c6You already voted!');
				return;
			}
		}
		%a = 0;
		for (%i = 0; %i < DSGameModeGroup.GetCount(); %i++)
		{
			%obj = DSGameModeGroup.getObject(%i);

			if (!isObject(%obj) || %obj.omit)
				continue;

			if (striPos(%obj.name, %search) != -1)
			{
				%pick[%a++] = %obj;
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
		%miniGame.voters[%miniGame.voteCount++] = %client;
		%miniGame.votes[%miniGame.voteCount] = %pick[1];
		messageClient(%client, '', '\c6You have cast your vote for %1.', %pick[1].name);
	}
	//{end of gamemode voting}
	function GameConnection::spawnPlayer(%this)
	{
		if (!%this.inDefaultGame())
			return Parent::spawnPlayer(%this);
		if (%this.isAdmin && DSAdminQueue.isMember(%this))
		{
			if (%this.miniGame.numMembers - DSAdminQueue.getCount() >= $DS::MaxPlayers)
			{
				CenterPrint(%this, "\c2You won't be able to spawn this round due to max players reached for minigame.");
				return;
			}
			DSAdminQueue.remove(%member);
			messageClient(%this, '', "\c2You have been removed from the admin queue.");
		}
		Parent::spawnPlayer(%this);
	}

	function MiniGameSO::checkLastManStanding(%this)
	{
		if (%this != $DefaultMiniGame)
			return Parent::checkLastManStanding(%this);
		if (%this.numMembers < 1 || isEventPending(%this.scheduleReset))
			return 0;
		if (%this.gameModeVote)
			return 0;
		%this.gameMode.checkLastManStanding(%this);
		return 0;
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		if (%client.miniGame != $DefaultMiniGame)
			return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);

		if (%sourceObject.sourceObject.isBot)
		{
			%sourceClientIsBot = 1;
			%sourceClient = %sourceObject.sourceObject;
		}

		%player = %client.player;

		if (isObject(%player))
		{
			%player.changeDatablock(PlayerCorpseArmor); //Doing this before client is nullified is important for appearance stuff
			%player.getDataBlock().onDisabled(%player); //...still call onDisabled because datablock switch fucks with anims
			%player.setShapeName("", 8564862);
			%player.unconscious = false;
			if (isObject(%player.tempBrick))
			{
				%player.tempBrick.delete();
				%player.tempBrick = 0;
			}
			%player.isBody = true;
			%player.character = %client.character;
			%player.client = 0;
		}
		else
			warn("WARNING: No player object in GameConnection::onDeath() for client '" @ %client @ "'");

		if (isObject(%client.camera) || isObject(%player))
		{ // this part of the code isn't accurate
			if (%client.getControlObject() != %client.camera)
			{
				%client.camera.setMode("Corpse", %player);
				%client.camera.setControlObject(0);
				%client.setControlObject(%client.camera);
			}
		}

		%client.player = 0;
		%client.corpse = %player;

		if ($Damage::Direct[$damageType] && getSimTime() - %player.lastDirectDamageTime < 100 && %player.lastDirectDamageType !$= "")
			%damageType = %player.lastDirectDamageType;

		if (%damageType == $DamageType::Impact && isObject(%player.lastPusher) && getSimTime() - %player.lastPushTime <= 1000)
			%sourceClient = %player.lastPusher;

		%message = '%2 killed %1';

		if (%sourceClient == %client || %sourceClient == 0)
		{
			%message = $DeathMessage_Suicide[%damageType];
			%client.corpse.suicide = true;
		}
		else
			%message = $DeathMessage_Murder[%damageType];

		// removed mid-air kills code here
		// removed mini-game kill points here

		%clientName = %client.getPlayerName();

		if (isObject(%sourceClient))
			%sourceClientName = %sourceClient.getPlayerName();
		else if (isObject(%sourceObject.sourceObject) && %sourceObject.sourceObject.getClassName() $= "AIPlayer")
			%sourceClientName = %sourceObject.sourceObject.name;
		else
			%sourceClientName = "";

		echo("\c4" SPC %sourceClientName SPC "killed" SPC %clientName);
		// removed mini-game checks here
		// removed death message print here
		// removed %message and %sourceClientName arguments
		messageClient(%client, 'MsgYourDeath', '', %clientName, '', '');//%client.miniGame.respawnTime);

		commandToClient(%client, 'CenterPrint', '', 1);
		%client.miniGame.checkLastManStanding();
		%client.miniGame.gameMode.onDeath(%client.miniGame, %client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	function serverCmdSuicide(%this)
	{
		if (!%this.inDefaultGame())
			return parent::serverCmdSuicide(%this);
		messageClient(%this, '', "Suicide is disabled.");
	}

	function serverCmdAlarm(%this)
	{
		%obj = %this.player;
		if (!%this.inDefaultGame() || !isObject(%this.player) || %this.player.getState() $= "Dead")
			return parent::serverCmdAlarm(%this);

		if (getSimTime() - %this.lastScream < 3000) //A limit so it's not spammable
			return;

		%this.lastScream = getSimTime();
		%this.haltStaminaReg = getSimTime();
		%obj.setEnergyLevel(%obj.getEnergyLevel() - 20); //Screaming is riskee y'know
		serverCmdMessageSent(%this, "!AAAAAAAAAAH!!");
	}

	function serverCmdLight(%this) //Another "interact" key for inventory stuff and other things
	{
		%player = %this.player;
		if (!%this.inDefaultGame() || !isObject(%this.player) || %this.player.getState() $= "Dead")
			return parent::serverCmdLight(%this);

		if (getSimTime() - %this.lastLightClick < 100) //a limit so server cannot be lagged out
			return;

		%this.lastLightClick = getSimTime();

		if (%player.unconscious) //Can't do stuff while unconscious bro
			return;

		%start = %player.getEyePoint();
		%end = vectorAdd(%start, vectorScale(%player.getEyeVector(), 6));

		%mask = $TypeMasks::All ^ ($TypeMasks::FxBrickAlwaysObjectType | $TypeMasks::StaticShapeObjectType);
		%ray = containerRayCast(%start, %end, %mask, %player);

		if (%ray && %ray.getType() & $TypeMasks::fxBrickObjectType)
		{
			%data = %ray.getDataBlock();

			if (%ray.storageBrick)
			{
				if (%ray.allowStoringItems && isObject(%player.tool[%player.currTool]))
				{
					if (%player.tool[%player.currTool].w_class > %ray.w_class_max)
					{
						commandToClient(%this, 'CenterPrint', "\c3" @ %player.tool[%player.currTool].uiName SPC "\c6is too big for this storage!", 1);
						return;
					}
					if (%ray.storeItem(%player.tool[%player.currTool], %player.getItemProps(%player.currTool), 1) != -1)
					{
						%player.itemProps[%player.currTool] = "";
						%player.removeToolSlot(%player.currTool, 1);
						%player.playThread(2, "shiftAway");
						if (%player.isViewingInventory)
							%this.updateInventoryView();
					}
				}
				else
				{
					%this.startViewingInventory(%ray);
					%player.playThread(2, "activate2");
				}
				return;
			}
			if (%data.isDoor) //Knock knock
			{
				%player.playThread(2, "shiftAway");
				serverPlay3d(DoorKnockSound, %ray.getWorldBoxCenter(), 1);
			}
		}

		if (%ray && %ray.getType() & $TypeMasks::playerObjectType)
		{
			// %this.startViewingInventory(%ray);
			// return;
		}
		//Corpse looting/planting
		if (%ray)
			%pos = getWords(%ray, 1, 3);
		else
			%pos = %b;
		initContainerRadiusSearch(%pos, 0.2,
			$TypeMasks::playerObjectType | $TypeMasks::CorpseObjectType);

		while (isObject(%col = containerSearchNext()))
		{
			if (%col.isBody)
			{
				%corpse = %col;
				break;
			}
		}
		if (isObject(%corpse) && vectorDist(%corpse.getPosition(), %pos) < 2)
		{
			if (isObject(%player.tool[%player.currTool]) && %corpse.addTool(%player.tool[%player.currTool], %player.getItemProps(%player.currTool), 1) != -1) //Tool selected, plant on body
			{
				%player.itemProps[%player.currTool] = "";
				%player.removeToolSlot(%player.currTool, 1);
				%player.playThread(2, "shiftAway");
				if (%player.isViewingInventory)
					%this.updateInventoryView();
			}
			else
			{
				%this.startViewingInventory(%corpse);
				%player.playThread(2, "activate2");
			}
		}
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DespairSyndromePackage");
