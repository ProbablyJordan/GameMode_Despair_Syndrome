package RaycastWeaponPackage
{
	function WeaponImage::onFire(%this, %obj, %slot)
	{
		if (!%this.raycastEnabled)
		{
			return Parent::onFire(%this, %obj, %slot);
		}

		if (%this.raycastRange <= 0)
		{
			return;
		}

		if (%obj.lastShot[%this] + %this.minShotTime > getSimTime())
		{
			return;
		}

		%obj.lastShot[%this] = getSimTime();

		if (%this.raycastMask $= "")
		{
			%mask = (
				$TypeMasks::FxBrickObjectType       |
				$TypeMasks::PlayerObjectType        |
				$TypeMasks::StaticShapeObjectType   |
				$TypeMasks::TerrainObjectType       |
				$TypeMasks::VehicleObjectType
			);
		}
		else
		{
			%mask = %this.raycastMask;
		}

		if (%this.raycastFromEye)
		{
			%basePoint = %obj.getEyePoint();
			%baseVector = %obj.getEyeVector();
		}
		else
		{
			%losPoint = %obj.getLOSPoint(%mask, %this.raycastRange);

			%basePoint = %obj.getMuzzlePoint(%slot);
			%baseVector = vectorNormalize(vectorSub(%losPoint, %basePoint));
		}

		%count = 1;

		if (%this.raycastMinCount !$= "" && %this.raycastMaxCount !$= "")
		{
			%count = getRandom(%this.raycastMinCount, %this.raycastMaxCount);
		}
		else if (%this.raycastCount !$= "")
		{
			%count = %this.raycastCount;
		}

		if (isObject(LagCompensationManager))
		{
			LagCompensationManager.compensateForClient(%obj.client);
		}

		for (%i = 0; %i < %count; %i++)
		{
			%spread = %this.getRaycastSpread(%obj, %slot, %i);

			if (%spread $= "0 0 0")
			{
				%vector = %baseVector;
			}
			else
			{
				%matrix = matrixCreateFromEuler(%spread);
				%vector = matrixMulVector(%matrix, %baseVector);
			}

			%vec = vectorScale(%vector, %this.raycastRange);
			%end = vectorAdd(%basePoint, %vec);

			%ray = containerRayCast(%basePoint, %end, %mask, %obj);

			if (%ray)
			{
				%b = getWords(%ray, 1, 3);
				%this.onRaycastCollision(%obj, getWord(%ray, 0), %b, getWords(%ray, 4, 6), %vector);
			}
			else
			{
				%b = %end;
				%this.onRaycastMissed(%obj, %end, %vector);
			}

			if (isObject(%this.raycastTracer))
			{
				createTracer(%basePoint, %b, %this.raycastTracer, %this.raycastTracerSize, %this.raycastTracerColor);
			}
		}

		if (isObject(LagCompensationManager))
		{
			LagCompensationManager.stopCompensating();
		}
	}
};

activatePackage("RaycastWeaponPackage");

function getRandomScalar()
{
	return getRandom() * 2 - 1;
}

function SimObject::hasMethod(%this, %name)
{
	return (
		isFunction(%this.class, %name) ||
		isFunction(%this.getName(), %name) ||
		isFunction(%this.getClassName(), %name)
	);
}

function WeaponImage::getRaycastSpread(%this, %obj, %slot, %index)
{
	if (%this.raycastSpread !$= "")
	{
		%scalars = getRandomScalar() SPC getRandomScalar() SPC getRandomScalar();
		return vectorScale(%scalars, mDegToRad(%this.raycastSpread / 2));
	}

	return "0 0 0";
}

function WeaponImage::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec)
{
	if (isObject(%this.raycastHitExplosion))
	{
		%explosion = new Projectile()
		{
			datablock = %this.raycastHitExplosion;

			initialPosition = %pos;
			initialVelocity = 0;
		};

		MissionCleanup.add(%explosion);

		%explosion.setScale(%obj.getScale());
		%explosion.explode();
	}

	if (%col.getType() & ($TypeMasks::PlayerObjectType | $TypeMasks::VehicleObjectType))
	{
		if (isObject(%col.spawnBrick) && %col.spawnBrick.getGroup().client == %obj.client)
		{
			%spawned = 1;
		}

		if (miniGameCanDamage(%obj, %col) == 1 || %spawned)
		{
			%this.damage(%obj, %col, %pos, %normal, %vec);
		}
	}
}

function WeaponImage::onRaycastMissed(%this, %obj, %pos, %vec)
{
}

function WeaponImage::damage(%this, %obj, %col, %pos, %normal, %vec)
{
	if (%this.directDamage <= 0)
	{
		return;
	}

	%type = $DamageType::Direct;

	if (%this.directDamageType)
	{
		%type = %this.directDamageType;
	}

	%col.damage(%obj, %pos, %this.directDamage * %obj.getSize(), %type);

	if (%this.raycastImpactImpulse)
	{
		%col.applyImpulse(%pos, vectorScale(%vec, %this.impactImpulse));
	}
	
	if (%this.raycastVerticalImpulse)
	{
		%col.applyImpulse(%pos, vectorScale("0 0 1", %this.verticalImpulse));
	}
}

function Player::getLOSPoint(%this, %mask, %dist)
{
	%eyePoint = %this.getEyePoint();
	%eyeVector = %this.getEyeVector();

	%endPoint = vectorAdd(%eyePoint, vectorScale(%eyeVector, %dist));
	%ray = containerRayCast(%eyePoint, %endPoint, %mask, %this);

	if (%ray)
	{
		return getWords(%ray, 1, 3);
	}

	return %endPoint;
}

function Player::getSize(%this)
{
	%scale = %this.getScale();

	%x = getWord(%scale, 0);
	%y = getWord(%scale, 1);
	%z = getWord(%scale, 2);

	if (%x > %y && %x > %z) return %x;
	if (%y > %x && %y > %z) return %y;

	return %z;
}

function createTracer(%a, %b, %dataBlock, %size, %color)
{
	if (%size $= "")
	{
		%size = %dataBlock.tracerSize;

		if (%size $= "")
		{
			%size = 1;
		}
	}

	if (%color $= "")
	{
		%color = %dataBlock.tracerColor;

		if (%color $= "")
		{
			%color = "1 1 1 1";
		}
	}

	%offset = vectorSub(%a, %b);
	%normal = vectorNormalize(%offset);

	%xyz = vectorNormalize(vectorCross("1 0 0", %normal));
	%pow = mRadToDeg(mACos(vectorDot("1 0 0", %normal))) * -1;

	%obj = new staticShape()
	{
		dataBlock = %dataBlock;
		scale = vectorLen(%offset) * 2 SPC %size SPC %size;

		position = vectorScale(vectorAdd(%a, %b), 0.5);
		rotation = %xyz SPC %pow;
	};

	missionCleanup.add(%obj);
	%obj.setNodeColor("ALL", %color);

	%obj.schedule(16, playThread, 0, %dataBlock.tracerThread);
	%obj.schedule(%datablock.tracerTime ? %dataBlock.tracerTime : 64, delete);
}