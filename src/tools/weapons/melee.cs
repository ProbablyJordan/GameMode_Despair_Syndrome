datablock AudioProfile(MeleeBlockSoundGood)
{
	filename = $DS::Path @ "res/sounds/tools/block_good.wav";
	description = AudioClose3d;
	preload = true;
};
datablock AudioProfile(MeleeBlockSoundDecent)
{
	filename = $DS::Path @ "res/sounds/tools/block_decent.wav";
	description = AudioClose3d;
	preload = true;
};
datablock AudioProfile(MeleeBlockSoundBad)
{
	filename = $DS::Path @ "res/sounds/tools/block_bad.wav";
	description = AudioClose3d;
	preload = true;
};

datablock AudioProfile(MeleeSwingSound)
{
	filename = $DS::Path @ "res/sounds/tools/melee_swing.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(meleeKnifeSwingSound)
{
	filename = $DS::Path @ "res/sounds/tools/knife_swing.wav";
	description = AudioClosest3d;
	preload = true;
};

	// Variables for item datablock:

	// canBlock = true; //Can you block w/ rightclick using this weapon?
	// blockImage = ""; //Image to use when blocking
	// blockSound["good"] = MeleeBlockSoundGood;
	// blockSound["decent"] = MeleeBlockSoundDecent;
	// blockSound["bad"] = MeleeBlockSoundBad;
	// blockBaseDrain = 5; //What stamina drain do you get on best possible block
	// blockMaxDrain = 100; //How much stamina can be possibly drained from you at worst block
	// blockEnemyDrain = 44; //How much stamina to drain from opponent on succesful block

package MeleePackage
{
	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);
		if(!%obj.getMountedImage(0) || !%obj.getMountedImage(0).item.canBlock)
			return;
		if(%slot == 4)
		{
			if(%state) {
				%obj.mountImage(%obj.getMountedImage(0).item.blockImage, 0);
			}
			else {
				// %obj.playThread(3, shiftRight);
				%obj.mountImage(%obj.getMountedImage(0).item.image, 0);
			}
		}
	}

	function projectileData::onCollision(%this, %obj, %col, %pos, %fade, %normal)
	{
		%obj.normal = %normal;
		parent::onCollision(%this, %obj, %col, %pos, %fade, %normal);
	}

	function Armor::damage(%this, %obj, %src, %pos, %damage, %type)
	{
		if(%src.getType() & $TypeMasks::PlayerObjectType)
		{
			%vector = %src.getForwardVector();
		}
		else
		{
			%vector = vectorScale(%src.normal, -1);
		}
		if(%obj.isBlocking && %obj.getMountedImage(0) && vectorDot(%obj.getForwardVector(), %vector) < 0)
		{
			%item = %obj.getMountedImage(0).item;
			%time = ($Sim::Time - %obj.lastBlockTime) / 5;
			%drain = getMax(%item.blockBaseDrain, %item.blockMaxDrain*mClampF(%time, 0, 1));
			%quality = %time < 0.1 ? "Good" : (%time < 0.5 ? "Decent" : "Bad");
			if (%obj.getEnergyLevel() < %drain)
				%quality = "Bad";
			%obj.setEnergyLevel(%obj.getEnergyLevel() - %drain);
			if (%quality $= "Good")
			{
				%time = 0;
				if (%src.getType() & $TypeMasks::PlayerObjectType)
					%src.setEnergyLevel(%src.getEnergyLevel() - %item.blockEnemyDrain);
			}
			%damage = %damage * mClampF(%time, 0, 1);
			serverPlay3D(MeleeBlockSound @ %quality, %pos);
		}
		Parent::damage(%this, %obj, %src, %pos, %damage, %type);
	}
};
activatePackage(MeleePackage);
