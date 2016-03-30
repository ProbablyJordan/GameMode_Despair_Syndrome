function GameConnection::gASL(%_)
{
	cancel(%_.gASL);
	%b = ClientGroup.getCount();
	for (%a = 0; %a < %b; %a++)
	{
		%c = ClientGroup.getObject(%a);
		if (isObject(%d = %c.player) && (%e = %d.currSleepLevel) !$= "")
		{
			%f += %e;
			%g++;
		}
	}
	for (%a = 0; %a < %b; %a++)
	{
		%c = ClientGroup.getObject(%a);
		if (isObject(%d = %c.player) && (%e = %d.currSleepLevel) !$= "")
			%h += mAbs(1 - (%e / (%f / %g)));
	}
	%_.centerPrint(%f / %g NL %_.player.currSleepLevel,3);
	%_.gASL = %_.schedule(50,"gASL");
}