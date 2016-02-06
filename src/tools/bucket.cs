//TODO: Add some actual functionality. Let Buckets contain liquid - most likely just water.
//		You should be able to clean up your mop in the bucket if it's bloody.
//		Sinks may have to become a custom brick for this so you can both dump and fill the bucket without much trouble.
//ALSO: Would be cool if, while carrying the bucket, shaking it too much will spill its contents on the floor.
datablock ItemData(BucketItem) {
	canPickUp = false;
	doColorShift = false;
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
	%obj.setNodeColor("bucket32", "0.6 0.6 0.6 1");
	%obj.playThread(0, "liq-1");
	%obj.setNodeColor("liquid", "0 0.8 1 1");
}

function BucketItem::updateLiquidColor(%this, %obj)
{
	%bucketProps = %obj.getItemProps();
	%blend = BlendRGBA("0 0.8 1 0.5", "0.7 0 0" SPC mClampF(%bucketProps.blood/%this.maxBlood, 0, 1));
	%obj.setNodeColor("liquid", getWords(%blend, 0, 2) SPC 1);
}

function BucketProps::onAdd(%this)
{
	%this.blood = 0;
}
