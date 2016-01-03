package DSHealthPackage
{
	function Armor::damage(%this, %obj, %src, %pos, %damage, %type)
	{
		talk(%obj.getRegion(%pos));
		Parent::damage(%this, %obj, %src, %pos, %damage, %type);
	}
	function Projectile::onCollision(%this, %obj, %col, %pos, %fade, %normal)
	{
		parent::onCollision(%this, %obj, %col, %pos, %fade, %normal);
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSHealthPackage");