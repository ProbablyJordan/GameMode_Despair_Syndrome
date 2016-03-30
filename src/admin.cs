function serverCmdWhoIs(%client, %a, %b)
{
	if (!%client.isAdmin && !isEventPending(%client.miniGame.resetSchedule))
		return;

	%search = trim(%a SPC %b);
	%miniGame = $defaultMiniGame;

	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (!isObject(%member.character))
			continue;

		if (%search $= "" || striPos(%member.getPlayerName(), %search) != -1 || striPos(%member.character.name, %search) != -1)
		{
			messageClient(%client, '', "\c3" @ %member.getPlayerName() SPC "\c6is \c3" @ %member.character.name);
		}
	}
}

function serverCmdToggleOOC(%this, %tog)
{
	if (!%this.isAdmin) return;
	if (%tog $= "")
		%tog = !$defaultMiniGame.muteOOC;
	$defaultMiniGame.muteOOC = %tog ? 1 : 0; //This is to make sure that you can't set it to random gibberish, only boolean
	$defaultMiniGame.chatMessageAll('', '\c5OOC has been globally %1 by %2.', $defaultMiniGame.muteOOC ? "muted" : "unmuted", %this.getPlayerName());
}

function serverCmdReset(%this, %do)
{
	if (!%this.isAdmin) return;
	if (%do)
	{
		$defaultMiniGame.reset(0);
		return;
	}
	%message = "\c2Are you sure you want to reset?";
	commandToClient(%this, 'messageBoxYesNo', "Reset", %message, 'resetAccept');
}
function serverCmdResetAccept(%this)
{
	if (!%this.isAdmin) return;
	$defaultMiniGame.reset(0);
}

function messageAdmins(%msg, %sound)
{
	%count = ClientGroup.getCount();
	for (%i = 0; %i < %count; %i++)
	{
		%other = ClientGroup.getObject(%i);
		if (%other.isAdmin)
		{
			messageClient(%other, '', %msg);
			if (isObject(%sound))
				%other.play2d(%sound);
		}
	}
}

function serverCmdPM(%this, %target, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24, %m25, %m26, %m27, %m28, %m29, %m30, %m31, %m32)
{
	if (!%this.isAdmin)
	{
		serverCmdReport(%this, %target, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24, %m25, %m26, %m27, %m28, %m29, %m30, %m31, %m32);
		return;
	}
	%text = %m1;
	for (%i=2; %i<=32; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	%text = ParseLinks(%text);
	if (%text $= "")
		return;
	if (isObject(%target = findClientByName(%target)))
	{
		messageClient(%target, '', '\c4Admin PM from \c5%1\c6: %2',%this.getPlayerName(), %text);
		%target.play2d(AdminBwoinkSound);
		messageAdmins("\c4PM from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %target.getPlayerName() @"\c6: "@%text);
	}
	else
	{
		messageClient(%this, '', '\c5Player not found');
	}
}

function serverCmdPMCC(%this, %targ0, %targ1, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24, %m25, %m26, %m27, %m28, %m29, %m30, %m31, %m32)
{
	if (!%this.isAdmin)
		return;
	%text = %m1;
	for (%i=2; %i<=32; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	%text = ParseLinks(%text);
	if (%text $= "")
		return;
	if (isObject(%targ0 = findClientByName(%targ0)))
	{
		messageClient(%targ0, '', '\c4Admin PM from \c5%1\c6: %2',%this.getPlayerName(), %text);
		%targ0.play2d(AdminBwoinkSound);
		if (isObject(%targ1 = findClientByName(%targ1)))
		{
			messageClient(%targ1, '', "\c4Admin PM from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %targ0.getPlayerName() @"\c6: "@%text);
			messageAdmins("\c4PM from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %targ0.getPlayerName() @ "\c6 and \c3" @ %targ1.getPlayerName() @"\c6: "@%text);
		}
		else
			messageAdmins("\c4PM from \c5"@ %this.getPlayerName() @"\c6 to \c3"@ %target.getPlayerName() @"\c6: "@%text);
	}
	else
	{
		messageClient(%this, '', '\c5Player not found');
	}
}

function serverCmdReport(%this, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19, %m20, %m20, %m22, %m23, %m24, %m25, %m26, %m27, %m28, %m29, %m30, %m31, %m32)
{
	if (getSimTime() - %client.lastReport <= 10000)
	{
		messageClient(%this, '', '\c0You have to wait \c3%1\c0 seconds until you can use this again.', mCeil(getSimTime() - %client.lastReport / 10000));
		return;
	}
	%text = %m1;
	for (%i=2; %i<=32; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	%text = ParseLinks(%text);
	if (%text $= "")
		return;
	messageAdmins("\c0REPORT from \c3"@%this.getPlayerName()@"\c6:" SPC %text, AdminBwoinkSound);
	messageClient(%this, '', '\c0Your report\c6: %1', %text);
	%this.lastReport = getSimTime();
}

function servercmdViewInv(%this, %t0, %t1, %t2, %t3, %t4)
{
	if (!(%this.isAdmin || %this.isSuperAdmin))
		return;
	for (%i = 0; %i < 5; %i++)
		%targ = %targ SPC %t[%i];
	%targ = trim(%targ);
	%vict = findClientByName(%targ);
	if (!isObject(%vict))
	{
		%this.chatMessage("\c6Unable to find " @ strReplace(%targ, "%", "%%") @ "!");
		return;
	}
	if (!isObject(%pl = %vict.player))
	{
		%this.chatMessage("\c6" @ strReplace(%vict.name, "%", "%%") @ " does not have a player!");
		return;
	}
	%this.chatMessage("\c6Viewing " @ strReplace(%vict.name, "%", "%%") @ "'s inventory:");
	for (%i = 0; %i < %pl.getDatablock().maxTools; %i++)
	{
		%tool = %pl.tool[%i];
		if (isObject(%tool))
			%this.chatMessage("\c6Slot " @ %i @ ": " @ %tool.uiName);
		else
			%this.chatMessage("\c6Slot " @ %i @ ": Empty");
	}
}

function serverCmdA(%this, %m0, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19)
{
	serverCmdAC(%this, %m0, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19);
}

function serverCmdAC(%this, %m0, %m1, %m2, %m3, %m4, %m5, %m6, %m7, %m8, %m9, %m10, %m11, %m12, %m13, %m14, %m15, %m16, %m17, %m18, %m19)
{
	if (!(%this.isAdmin || %this.isSuperAdmin))
		return;
	for (%i = 0; %i < 20; %i++)
		%text = %text SPC %m[%i];
	%text = trim(stripMLControlChars(%text));
	%text = ParseLinks(%text);
	if (%text $= "")
		return;
	messageAdmins("\c2" @ %this.name @ "\c3: " @ %text);
}

function serverCmdGetKiller(%client)
{
	if (!%client.isAdmin && !isEventPending(%client.miniGame.resetSchedule))
		return;

	%miniGame = $defaultMiniGame;
	if (isObject(%miniGame.gameMode) && isObject(%miniGame.gameMode.killer))
	{
		messageAdmins("\c5" @ %client.getPlayerName() SPC "used /getkiller.");
		messageClient(%client, '', '\c6The killer is \c0%1 (%2)', %miniGame.gameMode.killer.character.name, %miniGame.gameMode.killer.getPlayerName());
	}
}
function serverCmdDamageLogs(%client, %target)
{
	if (!%client.isAdmin) return;
	if (isObject(%target = findClientByName(%target)))
	{
		%found = isObject(%target.player) ? %target.player : (isObject(%target.corpse) ? %target.corpse : "");
		if (%found)
		{
			messageClient(%client, '', '\c5Damage logs for client \c3%1\c5:', %target.getPlayerName());
			for (%i=1;%i<=%found.attackCount;%i++) //Parse attack logs for info
			{
				%text[%a++] = "\c3["@ (%found.attackTime[%i] - $defaultMiniGame.lastResetTime) / 1000 @ " seconds after roundstart], \c6Attacker\c3:" SPC %found.attackerName[%i];
				%text[%a] = %text[%a] SPC "\c6Damage weapon\c3:" SPC %found.attackSource[%i].getName() SPC "\c6Direction\c3:" SPC (%found.attackDot[%i] > 0 ? "Back" : "Front");
				%text[%a] = %text[%a] SPC "\c6" @ (%found.attackStamina[%i] ? "Stamina" : "Health") @ " from " @ %found.attackHealthS[%i] @ " to " @ %found.attackHealthE[%i];
			}
			for (%i=1; %i<=%a; %i++)
				messageClient(%client, '', %text[%i]);
		}
		else
		{
			messageClient(%client, '', '\c5Player object for provided client not found.');
		}
	}
	else
	{
		messageClient(%client, '', '\c5Player not found');
	}
}
function serverCmdViewQueue(%this)
{
	if (!%this.isAdmin)
		return;
	if (!isObject(DSTrialGameMode_Queue))
	{
		messageClient(%this, '', '\c5Killer queue is not initialized.');
		return;
	}
	messageAdmins("\c5" @ %this.getPlayerName() SPC "is viewing the killer queue.");
	for (%i=0;%i<DSTrialGameMode_Queue.getCount();%i++)
	{
		%entry = DSTrialGameMode_Queue.getObject(%i);
		if (isObject(%entry))
			messageClient(%this, '', '\c6 - \c2%1', %entry.getPlayerName());
	}
	messageClient(%this, '', '\c5Found %1 entries in queue!', %i);
}
function serverCmdAddToQueue(%this, %target)
{
	if (!%this.isAdmin)
		return;
	if (!isObject(DSTrialGameMode_Queue))
	{
		messageClient(%this, '', '\c5Killer queue is not initialized.');
		return;
	}
	if (isObject(%target = findClientByName(%target)))
	{
		if (DSTrialGameMode_Queue.isMember(%target)) //found target in queue already
		{
			messageClient(%this, '', '\c5Target already in queue!');
			return;
		}
		DSTrialGameMode_Queue.add(%target);
		messageAdmins("\c5" @ %this.getPlayerName() SPC "has added\c6" SPC %target.getPlayerName() SPC "\c5to the killer queue.");
		return;
	}
	messageClient(%this, '', '\c5Target not found!');
}
function serverCmdRemoveFromQueue(%this, %target)
{
	if (!%this.isAdmin)
		return;
	if (!isObject(DSTrialGameMode_Queue))
	{
		messageClient(%this, '', '\c5Killer queue is not initialized.');
		return;
	}
	if (isObject(%target = findClientByName(%target)))
	{
		if (!DSTrialGameMode_Queue.isMember(%target))
		{
			messageClient(%this, '', '\c5Target not found in queue!');
			return;
		}
		DSTrialGameMode_Queue.add(%target);
		messageAdmins("\c5" @ %this.getPlayerName() SPC "has removed\c6" SPC %target.getPlayerName() SPC "\c5from the killer queue.");
		return;
	}
	messageClient(%this, '', '\c5Target not found!');
}
function serverCmdIgnoreQueue(%this, %target)
{
	%target = findClientByName(%target);
	if (!%this.isAdmin)
		return;//%target = %this;
	if (!isObject(DSTrialGameMode_Queue))
	{
		messageClient(%this, '', '\c5Killer queue is not initialized.');
		return;
	}
	if (isObject(%target))
	{
		%target.ignoreFromQueue = !%target.ignoreFromQueue;
		if (%this.isAdmin)
			messageAdmins("\c5" @ %this.getPlayerName() SPC "has" SPC %target.ignoreFromQueue ? "ignored" : "unignored" SPC"\c6" SPC %target.getPlayerName() SPC "\c5from the killer queue.");
		if (%target == %this)
			messageClient(%this, '', '\c5You will now be %1 from the killer queue.', %target.ignoreFromQueue ? "ignored" : "unignored");
		return;
	}
	messageClient(%this, '', '\c5Target not found!');
}

function serverCmdMute(%client, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9)
{
	if(!%client.isAdmin)
		return;
	
	for (%i = 0; %i < 10; %i++)
	{
		if (%a[%i] !$= "")
			continue;
		if (%i == 0 || %i == 1)
		{
			%client.chatMessage("\c6You have to put in someone to mute and the length of the mute!");
			return;
		}
		if(%a[%i - 1] $= (%a[%i - 1] | 0))
		{
			%time = %a[%i - 1];
			if (%time !$= (%time | 0))
			{
				%client.chatMessage("\c6You have to put in someone to mute and the length of the mute!");
				return;
			}
			for (%j = 0; %j < %i - 1; %j++)
				%name = %name SPC %a[%j];
			%name = getSubStr(%name, 1, strLen(%name) - 1);
			break;
		}
		else
		{
			%time = %a0;
			if (%time !$= (%time | 0))
			{
				%client.chatMessage("\c6You have to put in someone to mute and the length of the mute!");
				return;
			}
			for(%j = 1; %j < %i; %j++)
				%name = %name SPC %a[%j];
			%name = getSubStr(%name, 1, strLen(%name) - 1);
			break;
		}
	}
	
	%target = findClientByName(%name);
	if(!isObject(%target))
	{
		MessageClient(%client, '', "\c6Could not find " @ %name @ "!");
		return;
	}
	
	%blid = %target.getBLID();
	
	despair_setMute(%blid, %target.getPlayerName(), %time, 1);
	if(%time != -1)
		%time_str = "for" SPC %time SPC (%time > 1 ? "minutes" : "minute");
	else
		%time_str = "forever";
	
	MessageAll('', "\c3" @ %client.getPlayerName() SPC "\c2muted \c3" @ %target.getPlayerName() SPC "\c2(ID: " @ %target.getBLID() @ ") from using OOC chat " @ %time_str);
}

function serverCmdUnMute(%client, %name1, %name2, %name3)
{
	if(!%client.isAdmin)
		return;
	
	%target = findClientByName(trim(%name1 SPC %name2 SPC %name3));
	
	if(!isObject(%target))
	{
		MessageClient(%client, '', "\c6Client not found! (Remember: the cmd is /unmute name)");
		return;
	}
	
	%blid = %target.getBLID();
	
	if(despair_unMute(%blid, 1))
	{
		MessageAll('', "\c3" @ %client.getPlayerName() SPC "\c2unmuted \c3" @ %target.getPlayerName() SPC "\c2(ID: " @ %target.getBLID() @ ") from using OOC chat");
	}
	else
	{
		MessageClient(%client, '', "\c6That player is not muted!");
		return;
	}
}

function serverCmdICMute(%client, %a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9)
{
	if(!%client.isAdmin)
		return;
	
	for (%i = 0; %i < 10; %i++)
	{
		if (%a[%i] !$= "")
			continue;
		if (%i == 0 || %i == 1)
		{
			%client.chatMessage("\c6You have to put in someone to mute and the length of the mute!");
			return;
		}
		if(%a[%i - 1] $= (%a[%i - 1] | 0))
		{
			%time = %a[%i - 1];
			if (%time !$= (%time | 0))
			{
				%client.chatMessage("\c6You have to put in someone to mute and the length of the mute!");
				return;
			}
			for (%j = 0; %j < %i - 1; %j++)
				%name = %name SPC %a[%j];
			%name = getSubStr(%name, 1, strLen(%name) - 1);
			break;
		}
		else
		{
			%time = %a0;
			if (%time !$= (%time | 0))
			{
				%client.chatMessage("\c6You have to put in someone to mute and the length of the mute!");
				return;
			}
			for(%j = 1; %j < %i; %j++)
				%name = %name SPC %a[%j];
			%name = getSubStr(%name, 1, strLen(%name) - 1);
			break;
		}
	}
	
	%target = findClientByName(%name);
	if(!isObject(%target))
	{
		MessageClient(%client, '', "\c6Could not find " @ %name @ "!");
		return;
	}
	
	%blid = %target.getBLID();
	
	despair_setMute(%blid, %target.getPlayerName(), %time, 0);
	if(%time != -1)
		%time_str = "for" SPC %time SPC (%time > 1 ? "minutes" : "minute");
	else
		%time_str = "forever";
	
	MessageAll('', "\c3" @ %client.getPlayerName() SPC "\c2muted \c3" @ %target.getPlayerName() SPC "\c2(ID: " @ %target.getBLID() @ ") from using in-character chat " @ %time_str);
}

function serverCmdICUnmute(%client, %name1, %name2, %name3)
{
	if(!%client.isAdmin)
		return;
	
	%target = findClientByName(trim(%name1 SPC %name2 SPC %name3));
	
	if(!isObject(%target))
	{
		MessageClient(%client, '', "\c6Client not found! (Remember: the cmd is /icUnmute name)");
		return;
	}
	
	%blid = %target.getBLID();
	
	if(despair_unMute(%blid, 0))
	{
		MessageAll('', "\c3" @ %client.getPlayerName() SPC "\c2unmuted \c3" @ %target.getPlayerName() SPC "\c2(ID: " @ %target.getBLID() @ ") from using in-character chat");
	}
	else
	{
		MessageClient(%client, '', "\c6That player is not muted!");
		return;
	}
}

function serverCmdViewMute(%client)
{
	if(!%client.isAdmin)
		return;
	
	if($Muted::OOCPlayersList $= "" && $Muted::ICPlayersList $= "")
	{
		MessageClient(%client, '', "\c6Nobody is muted!");
		return;
	}
	
	if($Muted::OOCPlayersList !$= "")
	{
		%client.chatMessage("\c6OOC mutes:");
		for(%i = 0; %i < getWordCount($Muted::OOCPlayersList); %i++)
		{
			%blid = getWord($Muted::OOCPlayersList, %i);
			%name = $Muted::OOCPlayers[%blid];
			
			if(%name $= "")
				continue;
			
			if($Muted::OOCPlayers[%blid, "time_muted"] - getCurrentMinuteOfYear() < 0 && $Muted::OOCPlayers[%blid, "time_muted"] != -1)
			{
				despair_unMute(%blid);
				continue;
			}
			
			if($Muted::OOCPlayers[%blid, "time_muted"] != -1)
			{
				%time_left = $Muted::OOCPlayers[%blid, "time_muted"] - getCurrentMinuteOfYear();
				%time_left = %time_left SPC (%time_left != 1 ? "minutes" : "minute");
			}
			else
				%time_left = "Muted forever";
			
			MessageClient(%client, '', "\c6BLID:" SPC %blid SPC "\c7-\c6" SPC %name SPC "\c7-\c6" SPC %time_left);
		}
	}
	
	if($Muted::ICPlayersList !$= "")
	{
		%client.chatMessage("\c6OOC mutes:");
		for(%i = 0; %i < getWordCount($Muted::ICPlayersList); %i++)
		{
			%blid = getWord($Muted::ICPlayersList, %i);
			%name = $Muted::ICPlayers[%blid];
			
			if(%name $= "")
				continue;
			
			if($Muted::ICPlayers[%blid, "time_muted"] - getCurrentMinuteOfYear() < 0 && $Muted::ICPlayers[%blid, "time_muted"] != -1)
			{
				despair_unMute(%blid);
				continue;
			}
			
			if($Muted::ICPlayers[%blid, "time_muted"] != -1)
			{
				%time_left = $Muted::ICPlayers[%blid, "time_muted"] - getCurrentMinuteOfYear();
				%time_left = %time_left SPC (%time_left != 1 ? "minutes" : "minute");
			}
			else
				%time_left = "Muted forever";
			
			MessageClient(%client, '', "\c6BLID:" SPC %blid SPC "\c7-\c6" SPC %name SPC "\c7-\c6" SPC %time_left);
		}
	}
}

function despair_setMute(%blid, %name, %time, %ooc)
{
	if(%ooc)
	{
		$Muted::OOCPlayers[%blid] = %name;
		$Muted::OOCPlayers[%blid, "time_issued"] = (%time != -1 ? getCurrentMinuteOfYear() : -1);
		$Muted::OOCPlayers[%blid, "time_muted"] = (%time != -1 ? getCurrentMinuteOfYear() + %time : -1);
	
		$Muted::OOCPlayersList = $Muted::OOCPlayersList SPC %blid;
	}
	else	
	{
		$Muted::ICPlayers[%blid] = %name;
		$Muted::ICPlayers[%blid, "time_issued"] = (%time != -1 ? getCurrentMinuteOfYear() : -1);
		$Muted::ICPlayers[%blid, "time_muted"] = (%time != -1 ? getCurrentMinuteOfYear() + %time : -1);
	
		$Muted::ICPlayersList = $Muted::ICPlayersList SPC %blid;
	}
	
	// save the list
	export("$Muted*", "config/server/muted.cs");
	
	return true;
}

function despair_unMute(%blid, %ooc)
{
	if(%ooc)
	{
		if($Muted::OOCPlayers[%blid] $= "")
			return false;
		
		// set the name to nothing
		$Muted::OOCPlayers[%blid] = "";
		$Muted::OOCPlayers[%blid, "time_issued"] = "";
		$Muted::OOCPlayers[%blid, "time_muted"] = "";
		
		// loop through the whole list and remove the BLID that we want to unmute
		%newlist = "";
		for(%i = 0; %i < getWordCount($Muted::OOCPlayersList); %i++)
		{
			%test_blid = getWord($Muted::OOCPlayersList, %i);
			
			if(%test_blid != %blid)
				%newlist = %newlist SPC %test_blid;
		}
		
		// remove the spaces that we might have left at the beguining n stuff
		%newlist = trim(%newlist);
		$Muted::OOCPlayersList = %newlist;
	}
	else	
	{
		if($Muted::ICPlayers[%blid] $= "")
			return false;
		
		// set the name to nothing
		$Muted::ICPlayers[%blid] = "";
		$Muted::ICPlayers[%blid, "time_issued"] = "";
		$Muted::ICPlayers[%blid, "time_muted"] = "";
		
		// loop through the whole list and remove the BLID that we want to unmute
		%newlist = "";
		for(%i = 0; %i < getWordCount($Muted::ICPlayersList); %i++)
		{
			%test_blid = getWord($Muted::ICPlayersList, %i);
			
			if(%test_blid != %blid)
				%newlist = %newlist SPC %test_blid;
		}
		
		// remove the spaces that we might have left at the beguining n stuff
		%newlist = trim(%newlist);
		$Muted::ICPlayersList = %newlist;
	}
	
	// save the list
	export("$Muted*", "config/server/muted.cs");
	
	return true;
}

function despair_isMuted(%blid, %ooc)
{
	if(%ooc)
	{
		if($Muted::OOCPlayers[%blid] !$= "")
		{
			if($Muted::OOCPlayers[%blid, "time_muted"] - getCurrentMinuteOfYear() < 0 && $Muted::OOCPlayers[%blid, "time_muted"] != -1)
			{
				despair_unMute(%blid);
				return false;
			}
			
			return true;
		}
	}
	else
	{
		if($Muted::ICPlayers[%blid] !$= "")
		{
			if($Muted::ICPlayers[%blid, "time_muted"] - getCurrentMinuteOfYear() < 0 && $Muted::ICPlayers[%blid, "time_muted"] != -1)
			{
				despair_unMute(%blid);
				return false;
			}
			
			return true;
		}
	}
	return false;
}

if($Muted::Initialized == false && isFile("config/server/muted.cs"))
	exec("config/server/muted.cs");

$Muted::Initialized = true;