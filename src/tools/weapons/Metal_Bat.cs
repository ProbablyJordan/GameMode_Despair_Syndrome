////////////////////////////////////////////////////////

datablock ItemData(MetalBatItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/metal_bat.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Metal Bat";
	iconName = $DS::Path @ "res/icons/icon_Cane";
	doColorShift = false;
	colorShiftColor = "0.100 0.500 0.250 1.000";

	itemPropsClass = "MeleeProps";

	 // Dynamic properties defined by the scripts
	image = MetalBatImage;
	canDrop = true;
	canBlock = true; //Can you block w/ rightclick using this weapon?
	blockImage = MetalBatBlockImage; //Image to use when blocking
	blockSound["good"] = MeleeBlockSoundGood; //TODO: Weapon-specific block sounds
	blockSound["decent"] = MeleeBlockSoundDecent;
	blockSound["bad"] = MeleeBlockSoundBad;
	blockBaseDrain = 20; //What stamina drain do you get on best possible block
	blockMaxDrain = 90; //How much stamina can be possibly drained from you at worst block
	blockEnemyDrain = 60; //How much stamina to drain from opponent on succesful block

	w_class = 4; //Weight class: 1 is tiny, 2 is small, 3 is normal-sized, 4 is bulky
};

////////////////////////////////////////////////////////
//weapon image//////////////////////////////////////////
////////////////////////////////////////////////////////

datablock ShapeBaseImageData(MetalBatImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/metal_bat.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	eyeOffset = 0;
	rotation = eulerToMatrix( "0 0 0" );

	correctMuzzleVector = true;

	className = "WeaponImage";

	item = MetalBatItem;
	ammo = " ";
	projectile = MetalBatProjectile;
	projectileType = Projectile;

	melee = true;
	armReady = true;

	doColorShift = false;
	colorShiftColor = MetalBatItem.colorShiftColor;//"0.400 0.196 0 1.000";

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
	stateTimeoutValue[3]			= 0.4;
	stateTransitionOnTimeout[3]		= "Fire";
	stateAllowImageChange[3]		= false;
	stateSequence[3]				= "attackA";

	stateName[4]					= "Fire";
	stateTransitionOnTimeOut[4]		= "Release";
	stateTimeoutValue[4]			= 0.1;
	stateFire[4]					= true;
	stateAllowImageChange[4]		= false;
	stateSequence[4]				= "attackB";
	stateScript[4]					= "onFire";
	stateWaitForTimeout[4]			= true;

	stateName[5]					= "Release";
	stateTransitionOnTriggerUp[5]	= "Ready";
	stateScript[5]					= "EndFire";

	staminaDrain = 80;

	isWeapon = true;
	raycastEnabled = 1;
	raycastRange = 3;
	raycastFromEye = true;
	directDamage = 70;
	directDamageType = $DamageType::Brute;
	raycastHitExplosion = hammerProjectile;
};

function MetalBatImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function MetalBatImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
	ServerPlay3D(MeleeSwingSound, %obj.getHackPosition());
}

function MetalBatImage::EndFire(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function MetalBatImage::Ready(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function MetalBatImage::onCheckFire(%this, %obj, %slot)
{
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function MetalBatImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	else
		return; //dead, don't damage
	%obj.setEnergyLevel(%obj.getEnergyLevel() - %this.staminaDrain);
	parent::onFire(%this, %obj, %slot);
}

function MetalBatImage::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec)
{
	Parent::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec);
	ServerPlay3D(%col.getType() & $TypeMasks::playerObjectType ? UmbrellaHit1Sound : (%col.getDataBlock().isDoor ? WoodHitSound : UmbrellaHit2Sound), %pos, %col.getDataBlock().isDoor ? 1 : 0);
	if (!(%col.getType() & $TypeMasks::FxBrickObjectType))
		return;
	%col.doorDamage("1 13 1");
}
//Block image
datablock ShapeBaseImageData(MetalBatBlockImage : MetalBatImage)
{
	offset = "0 0 0";
	rotation = eulerToMatrix("0 -90 0"); //due to cane

	stateName[0]					= "Activate";
	stateTimeoutValue[0]			= 0.01;
	stateTransitionOnTimeout[0]		= "Ready";

	stateName[1]					= "Ready";
	stateTransitionOnTriggerDown[1] = "";
	stateAllowImageChange[1]		= true;
	stateSequence[1]				= "Ready";
};
function MetalBatBlockImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this,%obj,%slot);
	%obj.playThread(1, armReadyBoth);
	// %obj.setArmThread(look);
	%obj.isBlocking = true;
	%obj.lastBlockTime = $Sim::Time;
	%obj.regenStamina = 0;
}
function MetalBatBlockImage::onUnMount(%this, %obj, %slot)
{
	Parent::onUnMount(%this,%obj,%slot);
	%obj.playThread(1, root);
	// %obj.setArmThread(look);
	%obj.isBlocking = false;
	%obj.regenStamina = %obj.regenStaminaDefault;
}