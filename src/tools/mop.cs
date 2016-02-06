datablock ItemData(mopItem)
{
	shapeFile = $DS::Path @ "res/shapes/tools/mop.dts";
	emap = true;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;

	uiName = "Mop";
	iconName = $DS::Path @ "res/icons/icon_Mop";
	doColorShift = true;
	colorShiftColor = "0.4 0.6 0.8";

	image = mopImage;

	itemPropsClass = "MopProps";
	maxBlood = 30;

	canDrop = true;

	w_class = 3; //Weight class: 1 is tiny, 2 is small, 3 is normal-sized, 4 is bulky
};

function MopProps::onAdd(%this)
{
	%this.blood = 0;
}

datablock ShapeBaseImageData(mopImage)
{
	shapeFile = $DS::Path @ "res/shapes/tools/mop.dts";
	emap = true;

	mountPoint = 0;
	offset = "0 0 0.4";
	correctMuzzleVector = false;

	rotation = eulerToMatrix("180 0 0");

	className = "WeaponImage";

	item = mopItem;

	melee = true;
	armReady = true;

	doColorShift = true;
	colorShiftColor = mopItem.colorShiftColor;

	stateName[0]					= "Activate";
	stateTimeoutValue[0]			= 0.0;
	stateTransitionOnTimeout[0]		= "Ready";

	stateName[1]					= "Ready";
	stateTransitionOnTriggerDown[1]	= "PreFire";
	stateAllowImageChange[1]		= true;

	stateName[2]					= "PreFire";
	stateScript[2]					= "onPreFire";
	stateAllowImageChange[2]		= true;
	stateTimeoutValue[2]			= 0.01;
	stateTransitionOnTimeout[2]		= "Fire";

	stateName[3]					= "Fire";
	stateTransitionOnTimeout[3]		= "Fire";
	stateTimeoutValue[3]			= 0.2;
	stateFire[3]					= true;
	stateAllowImageChange[3]		= true;
	stateSequence[3]				= "Fire";
	stateScript[3]					= "onFire";
	stateWaitForTimeout[3]			= true;
	stateSequence[3]				= "Fire";
	stateTransitionOnTriggerUp[3]	= "StopFire";
	stateSound[3]					= "";
	//stateTransitionOnTriggerUp[3]	= "StopFire";

	stateName[4]					= "CheckFire";
	stateTransitionOnTriggerUp[4]	= "StopFire";
	stateTransitionOnTriggerDown[4]	= "Fire";
	stateSound[4]					= "";

	stateName[5]					= "StopFire";
	stateTransitionOnTimeout[5]		= "Ready";
	stateTimeoutValue[5]			= 0.2;
	stateAllowImageChange[5]		= true;
	stateWaitForTimeout[5]			= true;
	stateSequence[5]				= "StopFire";
	stateScript[5]					= "onStopFire";
};

function mopImage::onMount(%this, %obj, %slot)
{
	parent::onMount(%this,%obj,%slot);
}

//TODO: Make mop bloody after cleaning too much blood. Require the player to click a bucket filled with water to clear up mop.
//		Bucket will contain the blood from water because of that - can be easily reflected on item for buckets.
//		Most likely will have to use ItemProps for this.
function mopImage::onFire(%this, %obj, %slot)
{
	%props = %obj.getItemProps();

	%obj.playThread(2, shiftAway);
	%point = %obj.getEyePoint();
	%vector = %obj.getEyeVector();
	%stop = vectorAdd(%point, vectorScale(%vector, 7));

	%ray = containerRayCast(%point, %stop,
		$TypeMasks::FxBrickObjectType |
		$TypeMasks::ShapeBaseObjectType |
		$TypeMasks::TerrainObjectType |
		$TypeMasks::ItemObjectType,
		%obj
	);

	if (isObject(firstWord(%ray))) {
		%pos = getWords( %ray, 1, 3 );
	}
	else {
		%pos = %stop;
	}
	if (%ray && %ray.getClassName() $= "Item" && %ray.getDataBlock() == BucketItem.getID())
	{
		%bucketProps = %ray.getItemProps();

		if (%bucketProps.blood >= BucketItem.maxBlood)
		{
			if (isObject(%obj.client))
				%obj.client.centerPrint("\c6This bucket is too bloody.", 2);

			return;
		}

		if (%props.blood <= 0)
		{
			%obj.client.centerPrint("\c6You dip the mop in the bucket, despite it being completely clean.", 2);
			return;
		}

		%availableClean = getMin(%props.blood, BucketItem.maxBlood - %bucketProps.blood);
		%bucketProps.blood += %availableClean;
		%ray.getDataBlock().updateLiquidColor(%ray);
		%props.blood -= %availableClean;

		if (%props.blood > 0)
		{
			if (isObject(%obj.client))
				%obj.client.centerPrint("\c6You partially clean the mop in the bucket.", 2);
		}
		else if (isObject(%obj.client))
			%obj.client.centerPrint("\c6You clean the mop in the bucket.", 2);

		return;
	}

	initContainerRadiusSearch(%pos, 0.75,
		$TypeMasks::ShapeBaseObjectType);

	while (isObject(%col = containerSearchNext()))
	{
		if (!%col.isBlood && !%col.isPaint)
			continue;

		%clean = getMin(getMin(%col.freshness, 0.9), MopItem.maxBlood - %props.blood);
		if (%col.freshness <= 0)
		{
			%col.delete();
			continue;
		}
		if (%clean <= 0)
			continue;
		%col.freshness -= %clean;
		%props.blood += %clean;
		%col.color = getWords(%col.color, 0, 2) SPC getWord(%col.color, 3) * 0.5;
		%col.setNodeColor("ALL", %col.color);
		%col.setScale(vectorScale(%col.getScale(), 0.8));
	}

	if (%props.blood >= MopItem.maxBlood)
	{
		// TODO: spawn/add to blood splats?
		
		if (isObject(%obj.client))
			%obj.client.centerPrint("\c6The mop is too bloody.", 2);
	}
}

function mopImage::onStopFire(%this, %obj, %slot)
{
	%obj.playThread(2, root);
}
