function sleepyTime(%last)
{
	cancel($SleepyTime);
	%time = vectorDist(%last, %now = getSimTime()) / 1000;
	%dcLength = $DS::Time::DayLength + $DS::Time::NightLength;
	%sleepPeriod = %dcLength * 0.80;
	%increase = (%time * 25) / %sleepPeriod;
	%count = ClientGroup.getCount();
	%dreamProb = 1 - mPow(0.9, %time);
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (!%cl.inDefaultGame() || !isObject(%pl = %cl.player))
			continue;
		if (%cl.minigame.gameMode.trial)
		{
			if (%pl.knockoutStart !$= "" || %pl.currSleeping || %pl.currResting || %pl.unconscious)
				%pl.WakeUp();
			%pl.currSleeping = 0;
			%pl.currSleepLevel = 0;
			%pl.knockoutStart = "";
			continue;
		}
		if (%pl.knockoutStart !$= "")
		{
			doDream(%cl, %dreamProb, 0.15);
			%pl.currSleepLevel += %increase;
			if (%pl.currSleepLevel >= 100)
				%pl.currSleepLevel = 100;
			if (vectorDist(%pl.knockoutStart, %now) >= %pl.knockoutLength)
			{
				%pl.currSleeping = 0;
				%pl.knockoutStart = "";
				%pl.WakeUp();
			}
			else
			{
				%time = mCeil((%pl.knockoutLength - vectorDist(%pl.knockoutStart, %now)) / 1000);
				if (%cl.lastSleepMsgTime != %time)
				{
					%cl.centerPrint("\c6" @ %time @ " second" @ (%time == 1 ? "" : "s") @ " left until you wake up.", 3);
					%cl.lastSleepMsgTime = %time;
				}
			}
		}
		if (%pl.currSleeping)
		{
			doDream(%cl, %dreamProb, 0.15);
			if (%pl.currSleepLevel > 25)
			{
				%time = ((%pl.currSleepLevel - 25) / 300) * %sleepPeriod;
				%time = mCeil(%time + %sleepPeriod / 4);
				if (%cl.lastSleepMsgTime != %time)
				{
					%cl.centerPrint("\c6" @ %time @ " second" @ (%time == 1 ? "" : "s") @ " left until you wake up.", 3);
					%cl.lastSleepMsgTime = %time;
				}
				%decrease = %increase * 12;
				if(%decrease + 25 > %pl.currSleepLevel)
				{
					%decrease = (%decrease - (%pl.currSleepLevel - 25)) / 3;
					%pl.currSleepLevel = 25 - %decrease;
				}
				else
					%pl.currSleepLevel -= %decrease;
			}
			else
			{
				%time = mCeil((%pl.currSleepLevel / 100) * %sleepPeriod);
				if (%cl.lastSleepMsgTime != %time)
				{
					%cl.centerPrint("\c6" @ %time @ " second" @ (%time == 1 ? "" : "s") @ " left until you wake up.", 3);
					%cl.lastSleepMsgTime = %time;
				}
				%pl.currSleepLevel -= %increase * 4;
			}
			if (%pl.currSleepLevel <= 0)
			{
				commandToClient(%cl, 'clearCenterPrint');
				%pl.currSleepLevel = 0;
				%pl.currSleeping = 0;
				%pl.WakeUp();
			}
		}
		else
		{
			%pl.currSleepLevel += %increase;
			if (%cl.minigame.gamemode.killer == %cl)
				%pl.currSleepLevel = 0;
			if (%pl.currSleepLevel >= 100)
			{
				%pl.currSleeping = 1;
				%pl.KnockOut(-1);
			}
			if (%pl.currSleepLevel > 25)
			{
				%data = %pl.getDatablock();
				%pl.speedMult = 1 - (%pl.currSleepLevel - 25) / 75 * 0.3;
				%pl.setMaxForwardSpeed(%data.maxForwardSpeed * %pl.speedMult);
				%pl.setMaxSideSpeed(%data.maxSideSpeed * %pl.speedMult);
				%pl.setMaxBackwardSpeed(%data.maxBackwardSpeed * %pl.speedMult);
				if (%pl.running)
					if (%pl.getEnergyLevel() < 1)
					{
						%pl.setMaxForwardSpeed(%data.maxForwardSpeed * %pl.speedMult * 0.5);
						%pl.setMaxSideSpeed(%data.maxSideSpeed * %pl.speedMult * 0.5);
						%pl.setMaxBackwardSpeed(%data.maxBackwardSpeed * %pl.speedMult * 0.5);
					}
					else
						%pl.setMaxForwardSpeed(%data.maxForwardSpeed * %pl.speedMult * 1.75);
			}
			else if(%pl.speedMult != 1)
			{
				%pl.speedMult = 1;
				%data = %pl.getDatablock();
				%pl.setMaxForwardSpeed(%data.maxForwardSpeed);
				%pl.setMaxSideSpeed(%data.maxSideSpeed);
				%pl.setMaxBackwardSpeed(%data.maxBackwardSpeed);
			}
			if (%pl.currSleepLevel >= 75)
			{
				if (%pl.currSleepNotify < 3)
					%cl.chatMessage("You feel completely worn out. You should get some sleep with \c3/sleep\c0 before you pass out.");
				%pl.currSleepNotify = 3;
			}
			else if (%pl.currSleepLevel >= 50)
			{
				if (%pl.currSleepNotify < 2)
					%cl.chatMessage("You feel exhausted. You should really get some sleep with \c3/sleep\c0.");
				%pl.currSleepNotify = 2;
			}
			else if (%pl.currSleepLevel >= 25)
			{
				if (%pl.currSleepNotify < 1)
					%cl.chatMessage("You feel tired. You should probably get some sleep with \c3/sleep\c0.");
				%pl.currSleepNotify = 1;
			}
			else
				%pl.currSleepNotify = 0;
		}
		%stamina = getMin(125 - %pl.currSleepLevel, 100) / 100;
		%pl.energyLimit = %pl.getDataBlock().maxEnergy * %stamina;
	}
	$SleepyTime = schedule(16, 0, "sleepyTime", %now);
}
if(!isEventPending($SleepyTime))
	sleepyTime(getSimTime());

function doDream(%cl, %dreamProb, %charProb)
{
	if (getRandom() < %dreamProb)
	{
		%dream = getDreamText();
		if (getRandom() < %charProb)
		{
			%character = GameCharacters.getObject(getRandom(0, GameCharacters.getCount() - 1));
			if (isObject(%character))
				%dream = %character.name;
		}
		messageClient(%cl, '', '   \c1... %1 ...', %dream);
	}
}