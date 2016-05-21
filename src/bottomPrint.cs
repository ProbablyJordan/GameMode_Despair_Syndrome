function GameConnection::updateBottomPrint(%this)
{
	%mode = (%mini = %this.minigame).gamemode;
	if (isObject(%mode))
		%text = %mode.getBottomPrintText(%mini, %this);
	// compile text
	%fields = getFieldCount(%text);
	for (%i = 0; %i < %fields; %i += 2)
		%msg = %msg @ (%i != 0 ? "<just:left><br>" : "") @ getField(%text, %i) @ "<just:right>" @ getField(%text, %i + 1);
	if (%this.infoMsg !$= %msg)
	{
		%this.bottomPrint(%msg, -1, 1);
		%this.infoMsg = %msg;
	}
}

function DSGamemode::getBottomPrintText(%mode, %mini, %cl)
{
	if (isObject(%cam = %cl.getControlObject()) && %cam.getType() & $Typemasks::CameraObjectType && isObject(%targ = %cam.getOrbitObject().client))
	{
		%cl = %targ;
		%isSpectate = 1;
	}
	
	if (!isObject(%character = %cl.character))
	{
		return "\c6Health: \c00/0\t\c7Waiting for spawn\t\c6Stamina: \c50/0\t\c6Time: \c3" @ getTimeText();
	}
	
	%character = %cl.character;
	%nameTextColor = "ffff00";
	if (%character.gender $= "female")
		%nameTextColor = "ff11cc";
	else if (%character.gender $= "male")
		%nameTextColor = "22ccff";

	%health = 0;
	%maxhealth = 0;
	%stamina = 0;
	%maxstamina = 0;
	%heldItem = "\c7No Item";
	if (isObject(%pl = %cl.player))
	{
		%health = mFloor(%cl.player.getHealth());
		%maxhealth = %cl.player.getMaxHealth();
		%stamina = mFloor(%cl.player.getEnergyLevel());
		%maxstamina = mFloor(%cl.player.energyLimit);
		if (isObject(%img = %pl.getMountedImage(0)))
		{
			%item = %img.item;
			if (%item.getName() $= "KeyItem" || %item.getName() $= "KeyJanitorItem")
			{
				%heldItem = %pl.itemProps[%pl.currTool].name;
			}
			else
			{
				%heldItem = %item.uiName;
				%dType = %img.directDamageType;
				if (%dType == $DamageType::Blunt)
					%heldItem = %heldItem @ " (\c5Blunt, Lethal\c6)";
				else if (%dType == $DamageType::Sharp)
					%heldItem = %heldItem @ " (\c5Sharp, Lethal\c6)";
				else if (%dType == $DamageType::Stamina)
					%heldItem = %heldItem @ " (\c5Blunt, Nonlethal\c6)";
			}
		}
	}
	else
	{
		%health = 0;
		%maxHealth = 100;
		%stamina = 0;
		%maxStamina = 100;
		%heldItem = "";
	}

	%roleColor = "\c7";
	%role = "Undecided";
	if (%isSpectate)
	{
		%roleColor = "\c7";
		%role = "Unknown";
	}
	else if (!isObject(%cl.player))
	{
		%roleColor = "\c7";
		%role = "Corpse";
	}
	else if (%cl.inDefaultGame() && %cl.minigame.gamemode.madeKiller)
	{
		%roleColor = "\c2";
		%role = "Innocent";
		if (%cl.minigame.gamemode.isKiller(%cl.minigame, %cl))
		{
			%roleColor = "\c0";
			%role = "Killer";
		}
	}

	%text = "\c6Health: \c0"@%health@"/"@%maxhealth@"\t\c6Name: <color:"@%nameTextColor@">"@%character.name@"\c6 (\c3Room #"@%character.room@"\c6)";
	%text = %text @ "\t\c6Stamina: \c5"@%stamina@"/"@%maxstamina @ "\t\c6Role:" SPC %roleColor @ %role;
	//%text = %text @ "<just:left><br>\c6Exhaustion: \c5" @ %bar @ "<just:right>\c6Time: \c3" @ getTimeText();
	%text = %text @ "\t\c6" @ %heldItem @ "\t\c6Time: \c3" @ getTimeText();
	return %text;
}

function getFirstField(%text, %search)
{
	%fields = getFieldCount(%text);
	for (%i = 0; %i < %fields; %i++)
	{
		%field = getField(%text, %i);
		if (striPos(%field, %search) == 0)
			return %i;
	}
	return -1;
}

function getTimeText()
{
	%isNight = $DefaultMinigame.currTime $= "Night";
	%segmentLength = %isNight ? $DS::Time::NightLength : $DS::Time::DayLength;
	%time = (%segmentLength * 1000 - getTimeRemaining($DefaultMinigame.dayTimeSchedule)) | 0;
	%cycleLength = $DS::Time::DayLength + $DS::Time::NightLength;
	%time *= 86.4 / %cycleLength;
	%dayRatio = $DS::Time::DayLength / %cycleLength;
	%time += %isNight ? (%dayRatio * 86400) + 21600 : 21600; //21600 is 6 AM
	%time -= mFloor(%time / 86400) * 86400;
	%hour = mFloor(%time / 3600);
	%minute = mFloor((%time - %hour * 3600) / 60);
	if (%minute < 10)
		%minute = "0" @ %minute;
	%pastNoon = %hour >= 12 ? "PM" : "AM";
	%hour = %hour % 12;
	if (%hour == 0)
		%hour = 12;
	return %hour @ ":" @ %minute SPC %pastNoon;
}

function GameConnection::sendActiveCharacter(%this)
{
	%control = %this.getControlObject();

	if (%control.getType() & $TypeMasks::PlayerObjectType)
	{
		%target = %control.client;
	}
	else if (%control.getType() & $TypeMasks::CameraObjectType)
	{
		%target = %control.getOrbitObject().client;
	}

	if (isObject(%target))
	{
		if (isObject(%target.character))
			%name = %target.character.name;
		%rName = %target.name;
	}

	%text = "\c3Spectating:" SPC %name @ (%this.isAdmin ? " (" @ %rName @ ")" : "");
	%this.bottomPrint(%text);
}

package DSSpectatePackage
{
	function GameConnection::setControlObject(%this, %control)
	{
		Parent::setControlObject(%this, %control);
		//%this.sendActiveCharacter();
		%this.updateBottomprint();
	}

	function Camera::setOrbitMode(%this, %obj, %mat, %minDist, %maxDist, %curDist, %ownObj)
	{
		Parent::setOrbitMode(%this, %obj, %mat, %minDist, %maxDist, %curDist, %ownObj);
		%client = %this.getControllingClient();

		if (isObject(%client))
		{
			//%client.sendActiveCharacter();
			%client.updateBottomprint();
		}
	}

	function Camera::setMode(%this, %mode, %orbit)
	{
		Parent::setMode(%this, %mode, %orbit);
		%client = %this.getControllingClient();

		if (isObject(%client))
		{
			%client.sendActiveCharacter();
		}
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSSpectatePackage");