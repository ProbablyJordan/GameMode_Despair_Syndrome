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

	itemPropsClass = "MeleeProps";

	 // Dynamic properties defined by the scripts
	image = AdvSwordImage;
	canBlock = true; //Can you block w/ rightclick using this weapon?
	blockImage = AdvSwordBlockImage; //Image to use when blocking
	blockSound["good"] = MeleeBlockSoundGood;
	blockSound["decent"] = MeleeBlockSoundDecent;
	blockSound["bad"] = MeleeBlockSoundBad;
	blockBaseDrain = 15; //What stamina drain do you get on best possible block
	blockMaxDrain = 100; //How much stamina can be possibly drained from you at worst block
	blockEnemyDrain = 44; //How much stamina to drain from opponent on succesful block
	canDrop = true;
	
	w_class = 3; //Weight class: 1 is tiny, 2 is small, 3 is normal-sized, 4 is bulky
};

datablock ShapeBaseImageData(AdvSwordImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/sword.dts";
	emap = true;
	item = AdvSwordItem;
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
	stateTimeoutValue[3]			= 0.2;
	stateTransitionOnTimeout[3]		= "Fire";
	stateAllowImageChange[3]		= false;

	stateName[4]					= "Fire";
	stateTransitionOnTimeOut[4]		= "Release";
	stateTimeoutValue[4]			= 0.2;
	stateFire[4]					= true;
	stateAllowImageChange[4]		= false;
	stateSequence[4]				= "Fire";
	stateScript[4]					= "onFire";
	stateWaitForTimeout[4]			= true;

	stateName[5]					= "Release";
	stateTransitionOnTriggerUp[5]	= "Ready";
	stateScript[5]					= "EndFire";

	staminaDrain = 15;

	isWeapon = true;
	raycastEnabled = 1;
	raycastRange = 3;
	raycastFromEye = true;
	directDamage = 25;
	directDamageType = $DamageType::Sharp;
	raycastHitExplosion = SwordProjectile;
};

function AdvSwordImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function AdvSwordImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
	// ServerPlay3D(meleeKnifeSwingSound, %pos);
}

function AdvSwordImage::EndFire(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function AdvSwordImage::Ready(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function AdvSwordImage::onCheckFire(%this, %obj, %slot)
{
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function AdvSwordImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	else
		return; //dead, don't damage
	%obj.setEnergyLevel(%obj.getEnergyLevel() - %this.staminaDrain);
	parent::onFire(%this, %obj, %slot);
}

function AdvSwordImage::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec)
{
	Parent::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec);
	ServerPlay3D(%col.getType() & $TypeMasks::FxBrickObjectType && %col.getDataBlock().isDoor ? WoodHitSound : swordHitSound, %pos, %col.getDataBlock().isDoor ? 1 : 0);
	if (!(%col.getType() & $TypeMasks::FxBrickObjectType))
		return;

	%data = %col.getDataBlock();

	if (!%data.isDoor)
		return;

	%random = getRandom(9);

	%col.doorHits += %random < 2 ? 0 : (%random < 9 ? 1 : 2);

	if (%col.doorHits >= 6)
		%col.fakeKillBrick("10 10 0", -1);
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
	%obj.playThread(3, shiftLeft);
	%obj.setArmThread(armAttack);
	%obj.isBlocking = true;
	%obj.lastBlockTime = $Sim::Time;
	%obj.regenStamina = 0;
	// serverPlay3D(SwordEquipSound, %obj.getHackPosition());
}
function AdvSwordBlockImage::onUnMount(%this, %obj, %slot)
{
	Parent::onUnMount(%this,%obj,%slot);
	%obj.playThread(1, root);
	%obj.setArmThread(look);
	%obj.isBlocking = false;
	%obj.regenStamina = %obj.regenStaminaDefault;
}