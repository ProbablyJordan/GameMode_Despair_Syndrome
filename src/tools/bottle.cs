datablock ItemData(EmptyBottleItem) {
	canPickUp = false;
	doColorShift = false;
	colorShiftColor = "1 1 1 1";
	shapeFile = $DS::Path @ "res/shapes/tools/bottle.dts";

	uiName = "Empty Bottle";
	canDrop = false;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;
};

datablock ShapeBaseImageData(EmptyBottleImage)
{
	className = "WeaponImage";
	shapeFile = $DS::Path @ "res/shapes/tools/bottle.dts";

	item = EmptyBottleItem;
	armReady = true;

	doColorShift = EmptyBottleItem.doColorShift;
	colorShiftColor = EmptyBottleItem.colorShiftColor;
};
