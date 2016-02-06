datablock ItemData(bloodpackItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/bloodpack.dts";
	rotate = false;
	mass = 1;
	density = 0.4;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Bloodpack";
	iconName = $DS::Path @ "res/icons/Icon_bloodpack";
	doColorShift = false;

	 // Dynamic properties defined by the scripts
	image = bloodpackImage;
	canDrop = true;
	w_class = 2; //Weight class: 1 is tiny, 2 is small, 3 is normal-sized, 4 is bulky
	customPickupMultiple = true;
};

datablock ShapeBaseImageData(bloodpackImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/bloodpack.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0.05 0.1 0";
	eyeOffset = 0; //"0.7 1.2 -0.25";
	rotation = eulerToMatrix( "0 -90 0" );

	className = "WeaponImage";
	item = bloodpackItem;

	//raise your arm up or not
	armReady = true;

	doColorShift = false;

	// Initial start up state
	stateName[0]					= "Ready";
	stateTransitionOnTriggerDown[0]	= "Fire";
	stateAllowImageChange[0]		= true;

	stateName[1]					= "Fire";
	stateTransitionOnTimeout[1]		= "Ready";
	stateAllowImageChange[1]		= true;
	stateScript[1]					= "onFire";
	stateTimeoutValue[1]			= 1;
};

function bloodpackImage::onFire(%this,%obj,%slot)
{
	%obj.doSplatterBlood(getRandom(6, 12));
	serverPlay3d(bloodSpillSound, %obj.getHackPosition());
	%obj.bloody["rhand"] = true;
	%obj.bloody["chest_front"] = true;
	if (isObject(%obj.client))
		%obj.client.applyBodyParts();
	%obj.removeToolSlot(%obj.currTool);
}