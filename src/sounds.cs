datablock AudioDescription( audioSilent3d : audioClose3D )
{
	maxDistance = 15;
	referenceDistange = 5;
};

datablock AudioProfile(DoorJiggleSound) {
	fileName = $DS::Path @ "res/sounds/door_jiggle.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(DoorKnockSound) {
	fileName = $DS::Path @ "res/sounds/knock.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(DoorLockSound) {
	fileName = $DS::Path @ "res/sounds/Lock.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(DoorUnlockSound) {
	fileName = $DS::Path @ "res/sounds/Unlock.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(WoodHitSound)
{
	fileName = $DS::Path @ "res/sounds/physics/woodhit.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(AnnouncementJingleSound) {
	fileName = $DS::Path @ "res/sounds/jingle.wav";
	description = audio2D;
	preload = true;
};

//SFX -- not yet adapted to new soundsystem
// new ScriptObject(GenericShellSFX)
// {
// 	class = "SFXEffect";

// 	file[main, 1] = $DS::Path @ "res/sounds/physics/shellCasing1.wav";
// 	file[main, 2] = $DS::Path @ "res/sounds/physics/shellCasing2.wav";
// 	file[main, 3] = $DS::Path @ "res/sounds/physics/shellCasing3.wav";
// };

// new ScriptObject(BuckshotShellSFX)
// {
// 	class = "SFXEffect";

// 	file[main, 1] = $DS::Path @ "res/sounds/physics/shellDrop1.wav";
// 	file[main, 2] = $DS::Path @ "res/sounds/physics/shellDrop2.wav";
// 	file[main, 3] = $DS::Path @ "res/sounds/physics/shellDrop3.wav";
// };

// new ScriptObject(WeaponSoftImpactSFX)
// {
// 	class = "SFXEffect";

// 	file[main, 1] = $DS::Path @ "res/sounds/physics/weapon_impact_soft1.wav";
// 	file[main, 2] = $DS::Path @ "res/sounds/physics/weapon_impact_soft2.wav";
// 	file[main, 3] = $DS::Path @ "res/sounds/physics/weapon_impact_soft3.wav";
// };

// new ScriptObject(WeaponHardImpactSFX)
// {
// 	class = "SFXEffect";

// 	file[main, 1] = $DS::Path @ "res/sounds/physics/weapon_impact_hard1.wav";
// 	file[main, 2] = $DS::Path @ "res/sounds/physics/weapon_impact_hard2.wav";
// 	file[main, 3] = $DS::Path @ "res/sounds/physics/weapon_impact_hard3.wav";
// };

// new ScriptObject(KeyImpactSFX)
// {
// 	class = "SFXEffect";

// 	file[main, 1] = $DS::Path @ "res/sounds/physics/key.wav";
// };
