datablock ExplosionData(swordExplosion)
{
	soundProfile = "";
};

package DSOverwritePackage
{
	function swordProjectile::onExplode(%this, %obj, %pos)
	{
		ServerPlay3D(swordHitSound, %pos);
		parent::onExplode(%this, %obj, %pos);
	}
};
activatePackage(DSOverwritePackage);