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

datablock ExplosionData(advSwordExplosion : swordExplosion)
{
	soundProfile = "";
};

datablock ProjectileData(advSwordProjectile : swordProjectile)
{
	uiName = "";
	explosion = advSwordExplosion;
};

function advSwordProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(swordHitSound, %pos);
	parent::onExplode(%this, %obj, %pos);
}


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
	stateTimeoutValue[0]			= 0.01;
	stateTransitionOnTimeout[0]		= "Ready";

	stateName[1]					= "Ready";
	stateTransitionOnTriggerDown[1] = "CheckFire";
	stateSequence[1]				= "root";
	stateScript[1]					= "Ready";
	stateAllowImageChange[1]		= 1;

	stateName[2]					= "CheckFire";
	stateScript[2]					= "onCheckFire";
	stateTransitionOnAmmo[2]		= "PreFire";
	stateTransitionOnNoAmmo[2]		= "Release";

	stateName[3]					= "PreFire";
	stateScript[3]					= "onPreFire";
	stateTimeoutValue[3]			= 0.1;
	stateTransitionOnTimeout[3]		= "Fire";
	stateAllowImageChange[3]		= false;

	stateName[4]					= "Fire";
	stateTransitionOnTimeOut[4]		= "Release";
	stateTimeoutValue[4]			= 0.1;
	stateFire[4]					= true;
	stateAllowImageChange[4]		= false;
	stateSequence[4]				= "Fire";
	stateScript[4]					= "onFire";
	stateWaitForTimeout[4]			= true;

	stateName[5]					= "Release";
	stateTransitionOnTriggerUp[5]	= "Ready";
	stateScript[5]					= "EndFire";

	raycastEnabled = 1;
	raycastRange = 4;
	raycastFromEye = true;
	directDamage = 20;
	directDamageType = $DamageType::AdvSword;
	raycastHitExplosion = advSwordProjectile;
};

function AdvSwordImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < 20)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function AdvSwordImage::onUnMount(%this, %obj, %slot)
{
}

function AdvSwordImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
}

function AdvSwordImage::EndFire(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function AdvSwordImage::Ready(%this, %obj, %slot)
{	
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < 20)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function AdvSwordImage::onCheckFire(%this, %obj, %slot)
{
	if (%obj.getEnergyLevel() < 20)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function AdvSwordImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	%obj.setEnergyLevel(%obj.getEnergyLevel() - 20);
	parent::onFire(%this, %obj, %slot);
}

datablock ShapeBaseImageData(AdvSwordBlockImage : AdvSwordImage)
{
	offset = "0 0 0";
	rotation = eulerToMatrix("0 -45 0");

	stateName[0]					= "Activate";
	stateTimeoutValue[0]			= 0.01;
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
			%drain = 60*mClampF(%time, 0, 1);
			%quality = %time < 0.25 ? "Good" : (%time < 0.75 ? "Decent" : "Bad");
			if (%obj.getEnergyLevel() < %drain)
				%quality = "Bad";
			%obj.setEnergyLevel(%obj.getEnergyLevel() - %drain);
			if (%quality $= "Good")
				%time = 0;
			%damage = %damage * mClampF(%time, 0, 1);
			serverPlay3D(AdvSwordBlockSound @ %quality, %pos);
		}

		// else if (%coeff = vectorDot(%obj.getForwardVector(), %src.getForwardVector()) > 0)
		// {
		// 	%damage = %damage * (2 + %coeff);
		// }

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