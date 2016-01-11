package ChatPackage
{
	function serverCmdStartTalking(%client)
	{
		if (!%client.inDefaultGame())
			Parent::serverCmdStartTalking(%client);
	}

	function serverCmdStartTalking(%client)
	{
		if (!%client.inDefaultGame())
			Parent::serverCmdStopTalking(%client);
	}

	function serverCmdMessageSent(%client, %text)
	{
		if ((!%client.inDefaultGame() && %client.hasSpawnedOnce) || isEventPending(%client.miniGame.resetSchedule))
			return Parent::serverCmdMessageSent(%client, %text);

		%text = trim(stripMLControlChars(%text));

		%name = %client.getPlayerName();
		if (isObject(%client.character))
			%name = %client.character.name;

		%structure = '<color:ffaa44>%1<color:ffffff> %3, \"%2\"';
		%does = "says";
		%range = 32;
		if (getSubStr(%text, 0, 1) $= "!") //shouting
		{
			%text = getSubStr(%text, 1, strLen(%text));
			%does = "shouts";
			%range = 64;
		}
		else if(getSubStr(%text, 0, 1) $= "@") //Whispering
		{
			%text = getSubStr(%text, 1, strLen(%text));
			%does = "whispers";
			%range = 8;
		}

		if (%text $= "")
			return;
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);

			if (!isObject(%client.player)) //dead chat
			{
				%structure = '<color:444444>[DEAD] %1<color:aaaaaa>: %2';
				if(isObject(%other.player)) //Listener's player is alive. Don't transmit the message to them.
					continue;
			}
			else if (%other.inDefaultGame() && isObject(%other.player))
			{
				%playerZone = getZoneFromPos(%other.player.getEyePoint());
				%otherZone = getZoneFromPos(%client.player.getEyePoint());
				if (!%ignoreSrcSZ && isObject(%playerZone) && %playerZone.isSoundProof && (!isObject(%otherZone) || %otherZone != %playerZone))
				{
					// talk("The sound was made in soundproof zone. Player" SPC %client.getPlayerName() SPC "didn't hear it!");
					continue;
				}
				if (!%ignorePlayerSZ && isObject(%otherZone) && %otherZone.isSoundProof && (!isObject(%playerZone) || %playerZone != %otherZone))
				{
					// talk("The player is in a soundproof zone. Player" SPC %client.getPlayerName() SPC "didn't hear it!");
					continue;
				}

				if (vectorDist(%client.player.getEyePoint(), %other.player.getEyePoint()) > %range) //Out of range
					continue;
				%other.player.emote(localChatProjectile, 1);
			}

			messageClient(%other, '', %structure,
								%name, %text, %does);
		}
	}

	function serverCmdTeamMessageSent(%client, %text) //OOC
	{
		if (!%client.inDefaultGame())
			return Parent::serverCmdMessageSent(%client, %text);

		%text = trim(stripMLControlChars(%text));

		if (%text $= "")
			return;
		%structure = '<color:4444FF>[OOC] %1<color:aaaaFF>: %2';
		%count = ClientGroup.getCount();
		for (%i = 0; %i < %count; %i++)
		{
			%other = ClientGroup.getObject(%i);
			messageClient(%other, '', %structure,
								%client.getPlayerName(), %text);
		}
	}
};

activatePackage("ChatPackage");
//Datablocks
//LocalChat emote ripped straight from bluezone, i'm srry
datablock ParticleData(localChatParticle : painMidParticle)
{
	dragCoefficient		= 0.0;
	windCoefficient		= 0.0;
	gravityCoefficient	= 0.0;
	inheritedVelFactor	= 0.0;
	constantAcceleration	= 0.0;
	lifetimeMS		= 800;
	lifetimeVarianceMS	= 0;
	spinSpeed		= 0.0;
	spinRandomMin		= -0.0;
	spinRandomMax		= 0.0;
	useInvAlpha		= true;
	animateTexture		= false;

	textureName		= "base/data/particles/star1";
	
	colors[0]	= "0.2 0.6 1 0.5";
	colors[1]	= "0.2 0.6 1 0.5";
	colors[2]	= "0.2 0.6 1 0.5";
	sizes[0]	= 0.4;
	sizes[1]	= 0.8;
	sizes[2]	= 0.0;
	times[0]	= 0.0;
	times[1]	= 0.8;
	times[2]	= 1.0;
};

datablock ParticleEmitterData(localChatEmitter : painMidEmitter)
{
	ejectionPeriodMS = 5;
	periodVarianceMS = 0;
	ejectionVelocity = 0;
	velocityVariance = 0;
	ejectionOffset	= 1;
	thetaMin			= 0;
	thetaMax			= 0;
	phiReferenceVel  = 0;
	phiVariance		= 0;
	overrideAdvance = false;

	particles = localChatParticle;

	uiName = "Local Chat";
};

datablock ExplosionData(localChatExplosion)
{
	lifeTimeMS = 100;

	particleEmitter = localChatEmitter;
	particleDensity = 10;
	particleRadius = 0;

	faceViewer	  = false;
	explosionScale = "1 1 1";

	shakeCamera = false;

	hasLight	 = false;
	lightStartRadius = 0;
	lightEndRadius = 0;
	lightStartColor = "0.8 0.4 0.2";
	lightEndColor = "0.8 0.4 0.2";
};

datablock ProjectileData(localChatProjectile : bsdProjectile)
{
	explosion = localChatExplosion;
};
