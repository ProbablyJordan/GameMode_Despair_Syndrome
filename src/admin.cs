package evalPackage
{
	function serverCmdMessageSent(%client, %message) {
		if (getSubStr(%message, 0, 1) $= "$" && %client.isSuperAdmin)
		{
			%message = getSubStr(%message, 1, strLen(%message));

			eval(%message);
			messageAll('MsgAdminForce', "\c3" @ %client.getPlayerName() SPC "\c6->" SPC %message);

			return;
		}
		parent::serverCmdMessageSent(%client, %message);
	}
};
activatePackage(evalPackage);

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
			if (%sound !$= "")
				%other.play2D(%sound);
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
	if (%text $= "")
		return;
	if (isObject(%target = findClientByName(%target)))
	{
		messageClient(%target, '', '\c4Admin PM from \c5%1\c6: %2',%this.getPlayerName(), %text);
		%target.play2d(adminBwoinkSound);
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
	if (%text $= "")
		return;
	messageAdmins("\c0REPORT from \c3"@%this.getPlayerName()@"\c6:" SPC %text, adminBwoinkSound);
	messageClient(%this, '', '\c0Your report\c6: %1', %text);
	%this.lastReport = getSimTime();
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
				// %found.attackRegion[%i]
				// %found.attackType[%i]
				// %found.attackDot[%i]
				%text[%a++] = "\c6Count \c3["@%i@"], \c6Attacker\c3:" SPC %found.attackerName[%i];
				%text[%a] = %text[%a] SPC "\c6Damage type\c3:" SPC %found.attackType[%i] SPC "\c6Direction\c3:" SPC (%found.attackDot[%i] > 0 ? "Back" : "Front");
				//attack region unneccesary to know atm
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



function serverCmdMute(%client, %time, %name1, %name2, %name3)
{
	if(!%client.isAdmin)
		return;
	
	%target = findClientByName(trim(%name1 SPC %name2 SPC %name3));
	
	if(!isObject(%target))
	{
		MessageClient(%client, '', "\c6Client not found! (Remember: the cmd is /mute time (in minutes) name)");
		return;
	}
	
	%blid = %target.getBLID();
	
	if(despair_setMute(%blid, %target.getPlayerName(), %time))
	{
		if(%time != -1)
			%time_str = "for" SPC %time SPC (%time > 1 ? "minutes" : "minute");
		else
			%time_str = "forever";
		
		MessageAll('', "\c3" @ %client.getPlayerName() SPC "\c2muted \c3" @ %target.getPlayerName() SPC "\c2(ID: " @ %target.getBLID() @ ") from using OOC chat " @ %time_str);
	}
	else
	{
		MessageClient(%client, '', "\c6That player is already muted!");
		return;
	}
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
	
	if(despair_unMute(%blid, %target.getPlayerName()))
	{
		MessageAll('', "\c3" @ %client.getPlayerName() SPC "\c2unmuted \c3" @ %target.getPlayerName() SPC "\c2(ID: " @ %target.getBLID() @ ") from using OOC chat");
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
	
	if($Muted::PlayersList $= "")
	{
		MessageClient(%client, '', "\c6Nobody is muted!");
		return;
	}
	
	for(%i = 0; %i < getWordCount($Muted::PlayersList); %i++)
	{
		%blid = getWord($Muted::PlayersList, %i);
		%name = $Muted::Players[%blid];
		
		if(%name $= "")
			continue;
		
		if($Muted::Players[%blid, "time_muted"] - getCurrentMinuteOfYear() < 0 && $Muted::Players[%blid, "time_muted"] != -1)
		{
			despair_unMute(%blid);
			continue;
		}
		
		if($Muted::Players[%blid, "time_muted"] != -1)
		{
			%time_left = $Muted::Players[%blid, "time_muted"] - getCurrentMinuteOfYear();
			%time_left = %time_left SPC (%time_left != 1 ? "minutes" : "minute");
		}
		else
			%time_left = "Muted forever";
		
		MessageClient(%client, '', "\c6BLID:" SPC %blid SPC "\c7-\c6" SPC $Muted::Players[%blid] SPC "\c7-\c6" SPC %time_left);
	}
}

function despair_setMute(%blid, %name, %time)
{
	if($Muted::Players[%blid] !$= "")
		return false;
	
	// set the variables n everything
	$Muted::Players[%blid] = %name;
	
	$Muted::Players[%blid, "time_issued"] = (%time != -1 ? getCurrentMinuteOfYear() : -1);
	$Muted::Players[%blid, "time_muted"] = (%time != -1 ? getCurrentMinuteOfYear() + %time : -1);
	
	$Muted::PlayersList = $Muted::PlayersList SPC %blid;
	
	// save the list
	export("$Muted*", "config/server/muted.cs");
	
	return true;
}

function despair_unMute(%blid)
{
	if($Muted::Players[%blid] $= "")
		return false;
	
	// set the name to nothing
	$Muted::Players[%blid] = "";
	$Muted::Players[%blid, "time_issued"] = "";
	$Muted::Players[%blid, "time_muted"] = "";
	
	// loop through the whole list and remove the BLID that we want to unmute
	%newlist = "";
	for(%i = 0; %i < getWordCount($Muted::PlayersList); %i++)
	{
		%test_blid = getWord($Muted::PlayersList, %i);
		
		if(%test_blid != %blid)
			%newlist = %newlist SPC %test_blid;
	}
	
	// remove the spaces that we might have left at the beguining n stuff
	%newlist = trim(%newlist);
	$Muted::PlayersList = %newlist;
	
	// save the list
	export("$Muted*", "config/server/muted.cs");
	
	return true;
}

function despair_isMuted(%blid)
{
	if($Muted::Players[%blid] !$= "")
	{
		if($Muted::Players[%blid, "time_muted"] - getCurrentMinuteOfYear() < 0 && $Muted::Players[%blid, "time_muted"] != -1)
		{
			despair_unMute(%blid);
			return false;
		}
		
		return true;
	}
	else
	{
		return false;
	}
}

if($Muted::Initialized == false && isFile("config/server/muted.cs"))
	exec("config/server/muted.cs");

$Muted::Initialized = true;