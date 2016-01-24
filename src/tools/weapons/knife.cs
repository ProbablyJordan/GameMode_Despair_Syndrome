//audio
datablock AudioProfile(KnifeHitSound1)
{
	filename = $DS::Path @ "res/sounds/tools/melee_knife_01.wav";
	description = AudioClose3d;
	preload = true;
};
datablock AudioProfile(KnifeHitSound2)
{
	filename = $DS::Path @ "res/sounds/tools/melee_knife_02.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(KnifeHitFleshSound1)
{
	filename = $DS::Path @ "res/sounds/tools/stab-01.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(KnifeHitFleshSound2)
{
	filename = $DS::Path @ "res/sounds/tools/stab-02.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(KnifeHitFleshSound3)
{
	filename = $DS::Path @ "res/sounds/tools/stab-03.wav";
	description = AudioClosest3d;
	preload = true;
};

////////////////////////////////////////////////////////

datablock ItemData(KnifeItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/chef_knife.dts";
	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Knife";
	iconName = $DS::Path @ "res/icons/icon_Knife";
	doColorShift = false;
	colorShiftColor = "0.100 0.500 0.250 1.000";

	itemPropsClass = "MeleeProps";

	 // Dynamic properties defined by the scripts
	image = KnifeImage;
	canDrop = true;
	canBlock = false; //Blocking with a KNIFE sounds incredibly hard.
	// blockImage = KnifeBlockImage; //Image to use when blocking
	// blockSound["good"] = MeleeBlockSoundGood; //TODO: Weapon-specific block sounds
	// blockSound["decent"] = MeleeBlockSoundDecent;
	// blockSound["bad"] = MeleeBlockSoundBad;
	// blockBaseDrain = 40; //What stamina drain do you get on best possible block
	// blockMaxDrain = 300; //How much stamina can be possibly drained from you at worst block
	// blockEnemyDrain = 15; //How much stamina to drain from opponent on succesful block
};

function KnifeItem::onAdd(%this, %obj)
{
	Parent::onAdd(%this, %obj);
	// %props = %obj.getItemProps();
	// if (!%props.bloody)
		%obj.hideNode("blood");
}

////////////////////////////////////////////////////////
//weapon image//////////////////////////////////////////
////////////////////////////////////////////////////////

datablock ShapeBaseImageData(KnifeImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/chef_knife.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0";
	eyeOffset = 0;
	rotation = eulerToMatrix( "0 0 0" );

	correctMuzzleVector = true;

	className = "WeaponImage";

	item = KnifeItem;
	ammo = " ";
	projectile = KnifeProjectile;
	projectileType = Projectile;

	melee = true;
	armReady = true;

	doColorShift = false;
	colorShiftColor = KnifeItem.colorShiftColor;//"0.400 0.196 0 1.000";

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

	staminaDrain = 12;

	isWeapon = true;
	raycastEnabled = 1;
	raycastRange = 2.5;
	raycastFromEye = true;
	directDamage = 15;
	backstabMult = 3; //Rather huge backstab bonus
	directDamageType = $DamageType::Sharp;
	raycastHitExplosion = hammerProjectile;
};

function KnifeImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function KnifeImage::onPreFire(%this, %obj, %slot)
{
	%obj.playThread(1, armReadyRight);
	%obj.playThread(2, activate);
	// ServerPlay3D(MeleeSwingSound, %pos);
}

function KnifeImage::EndFire(%this, %obj, %slot)
{
	%obj.playThread(1, root);
}

function KnifeImage::Ready(%this, %obj, %slot)
{
	%obj.playThread(1, root);
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function KnifeImage::onCheckFire(%this, %obj, %slot)
{
	if (%obj.getEnergyLevel() < %this.staminaDrain)
		%obj.setImageAmmo(0, 0);
	else
		%obj.setImageAmmo(0, 1);
}

function KnifeImage::onFire(%this, %obj, %slot)
{
	if(%obj.getDamagePercent() < 1.0)
		%obj.playThread(2, shiftTo);
	else
		return; //dead, don't damage
	%obj.setEnergyLevel(%obj.getEnergyLevel() - %this.staminaDrain);
	parent::onFire(%this, %obj, %slot);
}

function KnifeImage::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec)
{
	Parent::onRaycastCollision(%this, %obj, %col, %pos, %normal, %vec);
	ServerPlay3D((%col.getType() & $TypeMasks::playerObjectType || %col.getType() & $TypeMasks::corpseObjectType)? KnifeHitFleshSound @ getRandom(1,3) : KnifeHitSound @ getRandom(1,2), %pos);
}