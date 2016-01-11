//This is a sound system designed to allow us to have "soundproof" objects! Woo.
//Variables:
//profile - audio profile
//position - source of sound
//IgnoreSrcSZ - Source's soundZone won't be taken into account. Means the sound will be heard outside the soundzone containing sound.
//IgnorePlayerSZ - Player's soundzone won't be taken into account. Means the sound will be heard by players in different soundzones (?)
function GameConnection::playGameSound(%this, %profile, %position, %ignoreSrcSZ, %ignorePlayerSZ)
{
	if (%position $= "") //No position provided? Skip the pleasantries and play it as 2d no matter what.
	{
		%this.play2d(%profile);
		return;
	}
	%description = %profile.description;
	if (isObject(%this.player)) //Check for soundproof zones
	{
		%foundZone = getZoneFromPos(%position);
		%playerZone = getZoneFromPos(%this.player.getEyePoint());
		if (!%ignoreSrcSZ && isObject(%foundZone) && %foundZone.isSoundProof && (!isObject(%playerZone) || %playerZone != %foundZone))
		{
			// talk("The sound was made in soundproof zone. Player" SPC %this.getPlayerName() SPC "didn't hear it!");
			return;
		}
		if (!%ignorePlayerSZ && isObject(%playerZone) && %playerZone.isSoundProof && (!isObject(%foundZone) || %foundZone != %playerZone))
		{
			// talk("The player is in a soundproof zone. Player" SPC %this.getPlayerName() SPC "didn't hear it!");
			return;
		}
	}
	if (%description.is2d)
		%this.play2d(%profile);
	else
		%this.play3d(%profile, %position);
}

function getZoneFromPos(%position)
{
	%x = getWord(%position, 0);
	%y = getWord(%position, 1);
	%z = getWord(%position, 2);
	
	%count = ZoneGroup.getCount();
	
	for (%i = 0; %i < %count; %i++)
	{
		%zone = ZoneGroup.getObject(%i);
		%minX = getWord(%zone.center, 0) - getWord(%zone.bounds, 0) / 2;
		%minY = getWord(%zone.center, 1) - getWord(%zone.bounds, 1) / 2;
		%minZ = getWord(%zone.center, 2) - getWord(%zone.bounds, 2) / 2;
		%maxX = getWord(%zone.center, 0) + getWord(%zone.bounds, 0) / 2;
		%maxY = getWord(%zone.center, 1) + getWord(%zone.bounds, 1) / 2;
		%maxZ = getWord(%zone.center, 2) + getWord(%zone.bounds, 2) / 2;
		if (%x >= %minX && %x <= %maxX && %y >= %minY && %y <= %maxY && %z >= %minZ && %z <= %maxZ)
			return %zone;
	}
	
	return 0;
}


package DSSoundReplacePackage
{
	function serverPlay3d(%profile, %position, %ignoreSrcSZ, %ignorePlayerSZ) //Replace serverPlay3d
	{
		%count = ClientGroup.getCount();

		for (%i = 0; %i < %count; %i++)
			ClientGroup.getObject(%i).playGameSound(%profile, %position, %ignoreSrcSZ, %ignorePlayerSZ);
	}
};
activatePackage("DSSoundReplacePackage");