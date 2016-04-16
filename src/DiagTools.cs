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

function servercmdSetDoomData(%cl, %data)
{
	if (isObject(%pl = %cl.player))
		%pl.doomLightData = %data;
}

function Player::lightOfDoom(%pl)
{
	cancel(%pl.doomLightSched);
	%light = %pl.doomLight;
	if (!isObject(%lData = %pl.doomLightData))
	{
		%lData = PlayerLight;
		%pl.doomLightData = %lData;
	}
	if (%pl.trigger0)
	{
		if (!isObject(%light))
		{
			%light = new fxLight()
			{
				datablock = %lData;
			};
			%pl.doomLight = %light;
		}
		%eye = %pl.getEyePoint();
		%dir = %pl.getEyeVector();
		%ray = containerRaycast(%eye, %rayEnd = vectorAdd(%eye, vectorScale(%dir, 192)), $Typemasks::fxBrickObjectType);
		if (isObject(firstWord(%ray)))
			%pos = vectorAdd(getWords(%ray, 1, 3), getWords(%ray, 4, 6));
		else
			%pos = %rayEnd;
		%light.setTransform(%pos @ " 1 0 0 0");
		%light.reset();
	}
	else if (isObject(%light))
		%light.delete();
	%pl.doomLightSched = %pl.schedule(16, "lightOfDoom");
}

package Triggers
{
	function Armor::onTrigger(%data, %this, %trig, %tog)
	{
		%this.trigger[%trig] = %tog;
		Parent::onTrigger(%data, %this, %trig, %tog);
	}
};
activatePackage("Triggers");