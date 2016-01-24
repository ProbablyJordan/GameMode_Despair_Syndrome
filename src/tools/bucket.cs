//TODO: Add some actual functionality. Let Buckets contain liquid - most likely just water.
//		You should be able to clean up your mop in the bucket if it's bloody.
//		Sinks may have to become a custom brick for this so you can both dump and fill the bucket without much trouble.
//ALSO: Would be cool if, while carrying the bucket, shaking it too much will spill its contents on the floor.
datablock ItemData(BucketItem) {
	canPickUp = false;
	doColorShift = true;
	colorShiftColor = "0.6 0.6 0.6 1";
	shapeFile = $DS::Path @ "res/shapes/tools/bucket.dts";

	uiName = "Bucket";
	canDrop = false;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	itemPropsClass = "BucketProps";
	maxBlood = 100;
};

function BucketItem::onAdd(%this, %obj)
{
	Parent::onAdd(%this, %obj);
	// %obj.alpha = 0.5;
	// %obj.setNodeColor("liquid", "0.2 0.8 1" SPC %obj.alpha);
}

function BucketItem::updateLiquidColor(%this, %obj)
{
	// %bucketProps = %obj.getItemProps();
	// %obj.setNodeColor("liquid", BlendRGBA("0.2 0.8 1", "0.7 0 0" SPC %bucketProps.blood/%this.maxBlood) SPC getMax(%obj.alpha, %bucketProps.blood/%this.maxBlood));
}

function BucketProps::onAdd(%this)
{
	%this.blood = 0;
}
