new ScriptObject(GenericShellSFX)
{
	class = "SFXEffect";

	file[main, 1] = $DS::Path @ "res/sounds/physics/shellCasing1.wav";
	file[main, 2] = $DS::Path @ "res/sounds/physics/shellCasing2.wav";
	file[main, 3] = $DS::Path @ "res/sounds/physics/shellCasing3.wav";
};

new ScriptObject(BuckshotShellSFX)
{
	class = "SFXEffect";

	file[main, 1] = $DS::Path @ "res/sounds/physics/shellDrop1.wav";
	file[main, 2] = $DS::Path @ "res/sounds/physics/shellDrop2.wav";
	file[main, 3] = $DS::Path @ "res/sounds/physics/shellDrop3.wav";
};

new ScriptObject(WeaponSoftImpactSFX)
{
	class = "SFXEffect";

	file[main, 1] = $DS::Path @ "res/sounds/physics/weapon_impact_soft1.wav";
	file[main, 2] = $DS::Path @ "res/sounds/physics/weapon_impact_soft2.wav";
	file[main, 3] = $DS::Path @ "res/sounds/physics/weapon_impact_soft3.wav";
};

new ScriptObject(WeaponHardImpactSFX)
{
	class = "SFXEffect";

	file[main, 1] = $DS::Path @ "res/sounds/physics/weapon_impact_hard1.wav";
	file[main, 2] = $DS::Path @ "res/sounds/physics/weapon_impact_hard2.wav";
	file[main, 3] = $DS::Path @ "res/sounds/physics/weapon_impact_hard3.wav";
};

new ScriptObject(KeyImpactSFX)
{
	class = "SFXEffect";

	file[main, 1] = $DS::Path @ "res/sounds/physics/key.wav";
};
