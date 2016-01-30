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
	if (isObject(%this.player)) //Check for soundproof zones and other things
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
		if (%this.player.unconscious && !%description.is2d)
		{
			%dist = vectorDist(%this.player.getEyePoint(), %position);
			if (%dist > 24) //Unconscious players can only hear sounds right next to them
				return;
			//Below we adjust the position in a proper way so it's heard relative to camera. This is done due to unconsciousness setting player's camera somewhere else.
			//First, we make sound pos relative to player (vectorsub). Second, we make it relative to camera position (vectoradd).
			%position = vectorAdd(vectorSub(%position, %this.player.getEyePoint()), getWords(%this.camera.getTransform(), 0, 2));
			//The next problem is the rotation issue - play3d doesn't take transform, so we have to rotate the %position ourselves.
			%rotation = axisToEuler(getWords(%this.player.getTransform(), 3, 6));
			%zrot = getWord(%rotation, 2);
			%camrot = getWord(axisToEuler(getWords(%this.camera.getTransform(), 3, 6)), 2); //Get Z rotation of camera
			%position = RotatePointAroundPivot(%position, getWords(%this.camera.getTransform(), 0, 2), mClampF(%zrot-%camrot, -180, 180));
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