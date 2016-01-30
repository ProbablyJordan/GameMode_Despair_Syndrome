datablock ItemData(CheeseburgerItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = $DS::Path @ "res/shapes/tools/Cheeseburger.dts";
	rotate = false;
	mass = 1;
	density = 0.4;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Cheeseburger";
	iconName = $DS::Path @ "res/icons/Icon_Cheeseburger";
	doColorShift = false;

	 // Dynamic properties defined by the scripts
	image = CheeseburgerImage;
	canDrop = true;
};

datablock ShapeBaseImageData(CheeseburgerImage)
{
	// Basic Item properties
	shapeFile = $DS::Path @ "res/shapes/tools/Cheeseburger.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0.05 0.1 0";
	eyeOffset = 0; //"0.7 1.2 -0.25";
	rotation = eulerToMatrix( "-90 -90 0" );

	className = "WeaponImage";
	item = CheeseburgerItem;

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

function CheeseburgerImage::onFire(%this,%obj,%slot)
{
	//CONSUME
}