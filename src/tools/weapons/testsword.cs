datablock AudioProfile(AdvSwordBlockSoundGood)
{
	filename = $DS::Path @ "res/sounds/tools/block_good.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(AdvSwordBlockSoundDecent)
{
	filename = $DS::Path @ "res/sounds/tools/block_decent.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(AdvSwordBlockSoundBad)
{
	filename = $DS::Path @ "res/sounds/tools/block_bad.wav";
	description = AudioClosest3d;
	preload = true;
};

//////////
// item //
//////////
datablock ItemData(AdvSwordItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/sword.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Test Sword";
	iconName = "add-ons/Weapon_Sword/icon_Sword";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	 // Dynamic properties defined by the scripts
	image = AdvSwordImage;
	canDrop = true;
};

AddDamageType("AdvSword",   '<bitmap:add-ons/Weapon_Sword/CI_sword> %1',    '%2 <bitmap:add-ons/Weapon_Sword/CI_sword> %1',0.75,1);
////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(AdvSwordImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/sword.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0 0 0";

	// When firing from a point offset from the eye, muzzle correction
	// will adjust the muzzle vector to point to the eye LOS point.
	// Since this weapon doesn't actually fire from the muzzle point,
	// we need to turn this off.  
	correctMuzzleVector = false;

	// eyeOffset = "0.7 1.2 -0.25";

	// Add the WeaponImage namespace as a parent, WeaponImage namespace
	// provides some hooks into the inventory system.
	className = "WeaponImage";

	//melee particles shoot from eye node for consistancy
	melee = true;
	doRetraction = false;
	//raise your arm up or not
	armReady = false;

	//casing = " ";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Images have a state system which controls how the animations
	// are run, which sounds are played, script callbacks, etc. This
	// state system is downloaded to the client so that clients can
	// predict state changes and animate accordingly.  The following
	// system supports basic ready->fire->reload transitions as
	// well as a no-ammo->dryfire idle state.
	useExitState = 1;

	// Initial start up state

	stateName[0]					= "Activate";
	stateAllowImageChange[0]		= 0;
	stateTimeoutValue[0]			= 0.4;
	stateTransitionOnTimeout[0]		= "Ready";

	stateName[1]					= "Ready";
	stateTransitionOnTriggerDown[1] = "PreFire";
	stateSequence[1]				= "root";
	stateScript[1]					= "Ready";
	stateAllowImageChange[1]		= 1;

	stateName[2]					= "PreFire";
	stateScript[2]					= "onPreFire";
	stateTimeoutValue[2]			= 0.1;
	stateTransitionOnTimeout[2]		= "Fire";
	stateAllowImageChange[2]		= false;

	stateName[3]					= "Fire";
	stateTransitionOnTimeout[3]		= "Charge";
	stateTimeoutValue[3]			= 0.1;
	stateFire[3]					= true;
	stateAllowImageChange[3]		= false;
	stateSequence[3]				= "Fire";
	stateScript[3]					= "onFire";
	stateWaitForTimeout[3]			= true;

	stateName[4]					= "Charge";
	stateSequence[4]				= "Charge";
	stateTransitionOnTriggerUp[4]	= "Ready";
	stateTransitionOnTimeout[4]		= "ChargeReady";
	stateTimeoutValue[4]			= 0.5; // short shorts
	stateScript[4]					= "onCharge";
	stateWaitForTimeout[4]			= false;

	stateName[5]					= "ChargeReady";
	stateTransitionOnTriggerUp[5]	= "FireSlash";
	stateSequence[5]				= "chargeSlash";
	stateTransitionOnTimeout[5]		= "ChargeStabReady";
	stateTimeoutValue[5]			= 1;
	stateScript[5]					= "onChargeReady";
	stateWaitForTimeout[5]			= false;

	stateName[6]					= "FireSlash";
	stateTransitionOnTimeout[6]		= "Ready";
	stateTimeoutValue[6]			= 0.3;
	stateFire[6]					= true;
	stateAllowImageChange[6]		= false;
	stateSequence[6]				= "slash";
	stateScript[6]					= "onSlash";
	stateWaitForTimeout[6]			= true;

	stateName[7]					= "ChargeStabReady";
	stateTransitionOnTriggerUp[7]	= "FireStab";
	stateSequence[7]				= "chargeStab";
	stateScript[7]					= "onChargeStabReady";
	stateWaitForTimeout[7]			= false;

	stateName[8]					= "FireStab";
	stateTransitionOnTimeout[8]		= "Ready";
	stateTimeoutValue[8]			= 0.3;
	stateFire[8]					= true;
	stateAllowImageChange[8]		= false;
	stateSequence[8]				= "stab";
	stateWaitForTimeout[8]			= true;
	stateScript[8]					= "onStab";

	raycastEnabled = 1;
	raycastRange = 2.5;

	directDamage = 20;
	directDamageType = $DamageType::AdvSword;
	raycastHitExplosion = SwordProjectile;
};

function AdvSwordImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function AdvSwordImage::onUnMount(%this, %obj, %slot)
{
	%obj.swordState = "";
}

function AdvSwordImage::onActivate(%this, %obj, %slot)
{
	%obj.playThread(1, "rotCW");
}

function AdvSwordImage::onExit(%this, %obj, %slot)
{
	%obj.playThread(1, "rotCCW");
}

function AdvSwordImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
}

function AdvSwordImage::Ready(%this, %obj, %slot)
{	
	%obj.playThread(1, root);
	%obj.swordState = "";
}

function AdvSwordImage::onCharge(%this, %obj, %slot)
{
	//Don't do anything yet
}

function AdvSwordImage::onChargeReady(%this, %obj, %slot) //Slash is ready
{
	%obj.playThread(2, plant);
	serverPlay3D(brickPlantSound, %obj.getHackPosition());
}

function AdvSwordImage::onChargeStabReady(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	%obj.playThread(2, plant);
	serverPlay3D(brickPlantSound, %obj.getHackPosition());
}

function AdvSwordImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	
}

function AdvSwordImage::onSlash(%this, %obj, %slot)
{
	%obj.playThread(2, shiftLeft);
	%obj.swordState = "slash";
	
}

function AdvSwordImage::onStab(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.swordState = "stab";
	
}

datablock ShapeBaseImageData(AdvSwordBlockImage : AdvSwordImage)
{
	offset = "0 0 0";
	rotation = eulerToMatrix("0 -45 0");

	stateName[0]					= "Activate";
	stateTimeoutValue[0]			= 0.1;
	stateTransitionOnTimeout[0]		= "Ready";

	stateName[1]					= "Ready";
	stateTransitionOnTriggerDown[1] = "";
	stateAllowImageChange[1]		= true;
	stateSequence[1]				= "Ready";
};

function AdvSwordBlockImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this,%obj,%slot);
	%obj.playThread(1, armReadyRight);
	%obj.setArmThread(armAttack);
	%obj.isBlocking = true;
	%obj.lastBlockTime = $Sim::Time;
	serverPlay3D(SwordEquipSound, %obj.getHackPosition());
}
function AdvSwordBlockImage::onUnMount(%this, %obj, %slot)
{
	Parent::onUnMount(%this,%obj,%slot);
	%obj.playThread(1, root);
	%obj.setArmThread(look);
	%obj.isBlocking = false;
}

package AdvSwordPackage
{
	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);
		if(%obj.getMountedImage(0) != nameToID(AdvSwordImage) && %obj.getMountedImage(0) != nameToID(AdvSwordBlockImage))
		{
			return;
		}
		// if(%this.canJet)
		// {
		// 	return;
		// }
		if(%slot == 4)
		{
			if(%state) {
				%obj.playThread(3, shiftLeft);
				%obj.mountImage(AdvSwordBlockImage, 0);
			}
			else {
				// %obj.playThread(3, shiftRight);
				%obj.mountImage(AdvSwordImage, 0);
			}
		}
	}

	function projectileData::onCollision(%this, %obj, %col, %pos, %fade, %normal)
	{
		%obj.normal = %normal;
		parent::onCollision(%this, %obj, %col, %pos, %fade, %normal);
	}

	function Armor::damage(%this, %obj, %src, %pos, %damage, %type)
	{
		if(%src.getType() & $TypeMasks::PlayerObjectType)
		{
			%vector = %src.getForwardVector();
		}
		else
		{
			%vector = vectorScale(%src.normal, -1);
		}
		if(%obj.isBlocking && vectorDot(%obj.getForwardVector(), %vector) < 0)
		{
			%time = ($Sim::Time - %obj.lastBlockTime) / 5;
			%damage = %damage * mClampF(%time, 0, 1);
			%quality = %time < 0.25 ? "Good" : (%time < 0.75 ? "Decent" : "Bad");
			serverPlay3D(AdvSwordBlockSound @ %quality, %pos);
			//If quality is "bad", throw the sword outta the user's hands!
		}

		else if (%coeff = vectorDot(%obj.getForwardVector(), %src.getForwardVector()) > 0)
		{
			if(%src.swordState $= "stab") //Backstab
				%damage = %damage * (4 + %coeff); //Lods of damage
			else
				%damage = %damage * (1 + %coeff);
		}

		if(%src.swordState $= "stab")
		{
			//Do stuff
		}
		if(%src.swordState $= "slash")
		{
			//Do stuff
		}

		// if(%type == $DamageType::AdvSword)
		// {
		// 	%factor = getMin(%factor + %damage / 4, 12);
		// 	%obj.setVelocity(vectorAdd(%obj.getVelocity(), vectorScale(%src.getForwardVector(), %factor)));
		// 	%obj.setVelocity(vectorAdd(%obj.getVelocity(), "0 0" SPC %factor));
		// }
		// talk(%damage);
		Parent::damage(%this, %obj, %src, %pos, %damage, %type);
	}
};
activatePackage(AdvSwordPackage);