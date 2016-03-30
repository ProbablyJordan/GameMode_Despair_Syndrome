////////////////////////////////////////////////////////

datablock ItemData(WoodBatItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/wooden_bat.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Wooden Bat";
	iconName = $DS::Path @ "res/icons/icon_Cane";
	doColorShift = false;
	colorShiftColor = "0.100 0.500 0.250 1.000";

	itemPropsClass = "MeleeProps";

	 // Dynamic properties defined by the scripts
	image = WoodBatImage;
	canDrop = true;
	canBlock = true; //Can you block w/ rightclick using this weapon?
	blockImage = WoodBatBlockImage; //Image to use when blocking
	blockSound["good"] = MeleeBlockSoundGood; //TODO: Weapon-specific block sounds
	blockSound["decent"] = MeleeBlockSoundDecent;
	blockSound["bad"] = MeleeBlockSoundBad;
	blockBaseDrain = 15; //What stamina drain do you get on best possible block
	blockMaxDrain = 70; //How much stamina can be possibly drained from you at worst block
	blockEnemyDrain = 50; //How much stamina to drain from opponent on succesful block

	w_class = 4; //Weight class: 1 is tiny, 2 is small, 3 is normal-sized, 4 is bulky
};

////////////////////////////////////////////////////////
//weapon image//////////////////////////////////////////
////////////////////////////////////////////////////////

datablock ShapeBaseImageData(WoodBatImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/wooden_bat.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	eyeOffset = 0;
	rotation = eulerToMatrix( "0 0 0" );

	correctMuzzleVector = true;

	className = "WeaponImage";

	item = WoodBatItem;
	ammo = " ";
	projectile = WoodBatProjectile;
	projectileType = Projectile;

	melee = true;
	armReady = true;

	doColorShift = false;
	colorShiftColor = WoodBatItem.colorShiftColor;//"0.400 0.196 0 1.000";

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
	stateTimeoutValue[3]			= 0.25;
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

	staminaDrain = 20;

	isWeapon = true;
	raycastEnabled = 1;
	raycastRange = 3;
	raycastFromEye = true;
	directDamage = 30;
	directDamageType = $DamageType::Brute;
	raycastHitExplosion = hammerProjectile;
};

function WoodBatImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function WoodBatImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
	ServerPlay3D(MeleeSwingSound, %obj.getHackPosition());
}

function WoodBatImage::EndFire(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function WoodBatImage::Ready(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function WoodBatImage::onCheckFire(%this, %obj, %slot)
{
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function WoodBatImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	else
		return; //dead, don't damage
	%obj.setEnergyLevel(%obj.getEnergyLevel() - %this.staminaDrain);
	parent::onFire(%this, %obj, %slot);
}

function WoodBatImage::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec)
{
	Parent::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec);
	ServerPlay3D(%col.getType() & $TypeMasks::playerObjectType ? UmbrellaHit1Sound : (%col.getDataBlock().isDoor ? WoodHitSound : UmbrellaHit2Sound), %pos, %col.getDataBlock().isDoor ? 1 : 0);
	if (!(%col.getType() & $TypeMasks::FxBrickObjectType))
		return;

	%data = %col.getDataBlock();

	if (!%data.isDoor)
		return;
	if (%col.lockID $= "")
	{
		// %col.doorOpen(%col.isCCW, %obj.client);
		return;
	}
	%random = getRandom(14);

	%col.doorHits += %random < 2 ? 0 : (%random < 14 ? 1 : 2);

	if (%col.doorHits >= %col.doorMaxHits)
	{
		%col.doorOpen(%col.isCCW, %obj.client);
		%col.lockState = false;
		%col.broken = true;
	}
}
//Block image
datablock ShapeBaseImageData(WoodBatBlockImage : WoodBatImage)
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
function WoodBatBlockImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this,%obj,%slot);
	%obj.playThread(1, armReadyBoth);
	// %obj.setArmThread(look);
	%obj.isBlocking = true;
	%obj.lastBlockTime = $Sim::Time;
	%obj.regenStamina = 0;
}
function WoodBatBlockImage::onUnMount(%this, %obj, %slot)
{
	Parent::onUnMount(%this,%obj,%slot);
	%obj.playThread(1, root);
	// %obj.setArmThread(look);
	%obj.isBlocking = false;
	%obj.regenStamina = %obj.regenStaminaDefault;
}