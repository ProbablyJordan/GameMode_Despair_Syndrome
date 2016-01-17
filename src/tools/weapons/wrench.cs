//audio
datablock AudioProfile(WrenchHit1Sound)
{
	filename    = $DS::Path @ "res/sounds/tools/melee_blunt.wav";
	description = AudioClose3d;
	preload = true;
};
datablock AudioProfile(WrenchHit2Sound)
{
	filename    = $DS::Path @ "res/sounds/tools/melee_blunt2.wav";
	description = AudioClose3d;
	preload = true;
};

////////////////////////////////////////////////////////

datablock ItemData(MonkeyWrenchItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/Monkey_Wrench.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Monkey Wrench";
	iconName = $DS::Path @ "res/icons/icon_Wrench";
	doColorShift = false;
	colorShiftColor = "0.100 0.500 0.250 1.000";

	 // Dynamic properties defined by the scripts
	image = MonkeyWrenchImage;
	canDrop = true;
	canBlock = true; //Can you block w/ rightclick using this weapon?
	blockImage = WrenchBlockImage; //Image to use when blocking
	blockSound["good"] = MeleeBlockSoundGood; //TODO: Weapon-specific block sounds
	blockSound["decent"] = MeleeBlockSoundDecent;
	blockSound["bad"] = MeleeBlockSoundBad;
	blockBaseDrain = 30; //What stamina drain do you get on best possible block
	blockMaxDrain = 150; //How much stamina can be possibly drained from you at worst block
	blockEnemyDrain = 33; //How much stamina to drain from opponent on succesful block
};

////////////////////////////////////////////////////////
//weapon image//////////////////////////////////////////
////////////////////////////////////////////////////////

datablock ShapeBaseImageData(MonkeyWrenchImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/Monkey_Wrench.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	eyeOffset = 0;
	rotation = eulerToMatrix( "0 0 0" );

	correctMuzzleVector = true;

	className = "WeaponImage";

	item = MonkeyWrenchItem;
	ammo = " ";
	projectile = WrenchProjectile;
	projectileType = Projectile;

	melee = true;
	armReady = true;

	doColorShift = false;
	colorShiftColor = MonkeyWrenchItem.colorShiftColor;//"0.400 0.196 0 1.000";

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

	staminaDrain = 25;

	isWeapon = true;
	raycastEnabled = 1;
	raycastRange = 3;
	raycastFromEye = true;
	directDamage = 30;
	directDamageType = $DamageType::Blunt;
	raycastHitExplosion = hammerProjectile;
};

function MonkeyWrenchImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function MonkeyWrenchImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
	ServerPlay3D(MeleeSwingSound, %obj.getHackPosition());
}

function MonkeyWrenchImage::EndFire(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function MonkeyWrenchImage::Ready(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function MonkeyWrenchImage::onCheckFire(%this, %obj, %slot)
{
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function MonkeyWrenchImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	else
		return; //dead, don't damage
	%obj.setEnergyLevel(%obj.getEnergyLevel() - %this.staminaDrain);
	parent::onFire(%this, %obj, %slot);
}

function MonkeyWrenchImage::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec)
{
	Parent::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec);
	ServerPlay3D(%col.getType() & $TypeMasks::playerObjectType ? WrenchHit1Sound : WrenchHit2Sound, %pos, %col.getDataBlock().isDoor ? 1 : 0);
	if (!(%col.getType() & $TypeMasks::FxBrickObjectType))
		return;

	%data = %col.getDataBlock();

	if (!%data.isDoor)
		return;

	%random = getRandom(9);

	%col.doorHits += %random < 2 ? 0 : (%random < 9 ? 1 : 2);

	if (%col.doorHits >= 6)
	{
		%col.doorOpen(%col.isCCW, %obj.client);
		%col.lockState = false;
		%col.broken = true;
	}
}
//Block image
datablock ShapeBaseImageData(WrenchBlockImage : MonkeyWrenchImage)
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
function WrenchBlockImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this,%obj,%slot);
	%obj.playThread(1, armReadyRight);
	%obj.playThread(3, shiftLeft);
	%obj.setArmThread(armAttack);
	%obj.isBlocking = true;
	%obj.lastBlockTime = $Sim::Time;
	%obj.regenStamina = 0;
}
function WrenchBlockImage::onUnMount(%this, %obj, %slot)
{
	Parent::onUnMount(%this,%obj,%slot);
	%obj.playThread(1, root);
	%obj.setArmThread(look);
	%obj.isBlocking = false;
	%obj.regenStamina = %obj.regenStaminaDefault;
}