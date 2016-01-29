$FOOTSTEPS_INTERVAL = 300;
$FOOTSTEPS_MIN_LANDING = -1.5;
$FOOTSTEPS_WALKING_FACTOR = 0.5;
$FOOTSTEPS_BLOODYFOOTPRINTS = 1;
$FOOTSTEPS_MAXBLOODSTEPS = 30;

function Player::updateFootsteps(%this, %lastVert)
{
	cancel(%this.updateFootsteps);

	if (%this.getState() $= "Dead" || %this.isCorpse)
	{
		return;
	}

	%velocity = %this.getVelocity();

	%vert = getWord(%velocity, 2);
	%horiz = vectorLen(setWord(%velocity, 2, 0));

	if (%lastVert < $FOOTSTEPS_MIN_LANDING && %vert >= 0)
	{
		%this.getDataBlock().onLand(%this);
	}

	if (%horiz >= %this.getDataBlock().maxForwardSpeed * $FOOTSTEPS_WALKING_FACTOR && !%this.isCrouched() && (!%this.getDataBlock().canJet || !%this.triggerState[4]))
	{
		if (!isEventPending(%this.playFootsteps))
		{
			%this.playFootsteps(1);
		}
	}
	else if (isEventPending(%this.playFootsteps))
	{
		cancel(%this.playFootsteps);
	}

	%this.updateFootsteps = %this.schedule(32, "updateFootsteps", %vert);
}

function Player::playFootsteps(%this, %foot)
{
	cancel(%this.playFootsteps);

	if (%this.getState() $= "Dead" || %this.isCorpse)
	{
		return;
	}

	%this.getDataBlock().onFootstep(%this, %foot);
	// 290?
	%this.playFootsteps = %this.schedule($FOOTSTEPS_INTERVAL * (%this.running ? 0.75 : 1), "playFootsteps", !%foot);
}

function Player::getFootPosition(%this, %foot)
{
	%base = %this.getPosition();
	%side = vectorCross(%this.getUpVector(), %this.getForwardVector());

	if (!%foot)
	{
		%side = vectorScale(%side, -1);
	}

	//return vectorAdd(%base, vectorScale(%side, 0.3));
	return vectorAdd(%base, vectorScale(%side, 0.4));
}

function Player::getFootObject(%this, %foot)
{
	%pos = %this.getFootPosition(%foot);

	return containerRayCast(
		vectorAdd(%pos, "0 0 0.1"),
		vectorSub(%pos, "0 0 1.1"),
		$TypeMasks::FxBrickObjectType | $TypeMasks::terrainObjectType, %this
	);
}

function Armor::onLand(%this, %obj)
{
	for (%i = 0; %i < 2; %i++)
	{
		%ray = %obj.getFootObject(%i);

		if (!%ray)
		{
			continue;
		}

		if (%obj.bloodyFootprints > 0)
		{
			%obj.doBloodyFootprint(%ray, %i, %obj.bloodyFootprints / %obj.bloodyFootprintsLast);
		}

		%color = -1;
		%material = "flat";
		if (%ray.getType() & $TypeMasks::FxBrickObjectType)
		{
			%color = %ray.getColorID();
			%prefix = "_fsm_";
			%strpos = strpos(%ray.getName(), %prefix);
			if(%strpos >= 0) //We have detected "fsm_" in the brick's name! That means someone wants us to use the brick as a footstep material.
			{
				%strpos += strlen(%prefix); //search after "fsm_", even if someone decides to name their brick "foobarfsm_"
				%material = getSubStr(%ray.getName(), %strpos, strlen(%ray.getName())); //Find the material name from after "fsm_" to string length.
				if((%strpos = strpos(%material, "_")) >= 0) //Oh crap, we have found an underscore in our new %material! Let's strip out everything after it!
				{
					%material = getSubStr(%material, 0, %strpos); //Boom.
				}
			}
			else
			{
				%material = $m $= "" ? getMaterial(%color) : $m;
			}
		}
		if (%ray.material !$= "")
		{
			%material = %ray.material;
		}

		%sound = nameToID("footstepSound_" @ %material @ getRandom(1, $FS::SoundCount[%material]));

		if (isObject(%sound))
		{
			serverPlay3D(%sound, getWords(%ray, 1, 3));
		}
	}
}

function Armor::onFootstep(%this, %obj, %foot)
{
	%ray = %obj.getFootObject(%foot);

	if (!%ray)
	{
		return;
	}

	if($FOOTSTEPS_BLOODYFOOTPRINTS)
	{
		initContainerRadiusSearch(getWords(%ray, 1, 3), 0.4, $TypeMasks::ShapeBaseObjectType);
		while (%col = containerSearchNext())
		{
			if (%col.isBlood && %col.freshness >= 1)// && %col.sourceClient != %obj.client)
			{
				%obj.setBloodyFootprints(getMin(%obj.bloodyFootprints + %col.freshness * 3, $FOOTSTEPS_MAXBLOODSTEPS), %col.sourceClient); //Default freshness for splatter blood is 3, so 15 footsteps for fresh step.
				%col.freshness--; //Decrease blood freshness
				createBloodExplosion(getWords(%ray, 1, 3), %obj.getVelocity(), %col.getScale());
				serverPlay3d(bloodSpillSound, getWords(%ray, 1, 3));
				break;
			}
		}
		//if (!%obj.isCloaked && 0)
		if (%obj.bloodyFootprints > 0)
		{
			%obj.doBloodyFootprint(%ray, %foot, %obj.bloodyFootprints / %obj.bloodyFootprintsLast);
			%obj.bloodyFootprints--;
		}
	}

	%color = -1;
	%material = "flat";
	if (%ray.getType() & $TypeMasks::FxBrickObjectType)
	{
		%color = %ray.getColorID();
		%prefix = "_fsm_";
		%strpos = strpos(%ray.getName(), %prefix);
		if(%strpos >= 0) //We have detected "fsm_" in the brick's name! That means someone wants us to use the brick as a footstep material.
		{
			%strpos += strlen(%prefix); //search after "fsm_", even if someone decides to name their brick "foobarfsm_"
			%material = getSubStr(%ray.getName(), %strpos, strlen(%ray.getName())); //Find the material name from after "fsm_" to string length.
			if((%strpos = strpos(%material, "_")) >= 0) //Oh crap, we have found an underscore in our new %material! Let's strip out everything after it!
			{
				%material = getSubStr(%material, 0, %strpos); //Boom.
			}
		}
		else
		{
			%material = $m $= "" ? getMaterial(%color) : $m;
		}
	}

	//%p = new Projectile()
	//{
	//  datablock = PongProjectile;
	//  initialPosition = %obj.getFootPosition(%foot);
	//  initialVelocity = "0 0 0";
	//};

	//%p.setScale("0.5 0.01 0.5");

	if (%ray.material !$= "")
	{
		%material = %ray.material;
	}

	%sound = "footstepSound_" @ %material @ getRandom(1, $FS::SoundCount[%material]);
	%sound = nameToID(%sound);

	if (isObject(%sound))
	{
		serverPlay3D(%sound, getWords(%ray, 1, 3));
	}
}

package FootstepsPackage
{
	function Armor::onNewDataBlock(%this, %obj)
	{
		Parent::onNewDataBlock(%this, %obj);
		if (%obj.isCorpse) return;
		if (!isEventPending(%obj.updateFootsteps))
		{
			%obj.updateFootsteps = %obj.schedule(0, "updateFootsteps");
		}
	}

	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);
		%obj.triggerState[%slot] = %state ? 1 : 0;
	}
};

activatePackage("FootstepsPackage");
JumpSound.fileName = "";

function getNumberStart( %str )
{
	%best = -1;

	for ( %i = 0 ; %i < 10 ; %i++ )
	{
		%pos = strPos( %str, %i );

		if ( %pos < 0 )
		{
			continue;
		}

		if ( %best == -1 || %pos < %best )
		{
			%best = %pos;
		}
	}

	return %best;
}

function loadFootstepSounds()
{
	%pattern = $DS::Path @ "res/sounds/footsteps/*.wav";
	%list = "generic 0";

	deleteVariables( "$FS::Sound*" );
	$FS::SoundNum = 0;

	for ( %file = findFirstFile( %pattern ) ; %file !$= "" ; %file = findNextFile( %pattern ) )
	{
		%base = fileBase( %file );
		%name = "footstepSound_" @ %base;

		echo(%name);

		if ( !isObject( %name ) )
		{
			datablock audioProfile( genericFootstepSound )
			{
				description = "audioClosest3D";
				fileName = %file;
				preload = true;
			};

			if ( !isObject( %obj = nameToID( "genericFootstepSound" ) ) )
			{
				continue;
			}

			%obj.setName( %name );
		}

		if ( ( %pos = getNumberStart( %base ) ) > 0 )
		{
			%pre = getSubStr( %base, 0, %pos );
			%post = getSubStr( %base, %pos, strLen( %base ) );

			if ( $FS::SoundCount[ %pre ] < 1 || !strLen( $FS::SoundCount[ %pre ] ) )
			{
				%list = %list SPC %pre SPC $FS::SoundNum;
			}

			if ( $FS::SoundCount[ %pre ] < %post )
			{
				$FS::SoundCount[ %pre ] = %post;
			}

			$FS::SoundName[ $FS::SoundNum ] = %pre;
			$FS::SoundIndex[ %pre ] = $FS::SoundNum;
			$FS::SoundNum++;
		}
	}

	registerOutputEvent( "fxDTSBrick", "setMaterial", "list" SPC %list );
}

function fxDTSBrick::setMaterial( %this, %idx )
{
	%this.material = $FS::SoundName[ %idx ];
	echo(%idx);
}

loadFootstepSounds();

function getMaterial(%color) //TODO: make this not shit
{
	return "flat";
}