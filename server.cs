$DS::Path = filePath(expandFileName("./description.txt")) @ "/";

//lib
exec("./lib/colors.cs");
exec("./lib/decals.cs");
exec("./lib/environment.cs");
exec("./lib/itemprops.cs");
exec("./lib/math-angles-vectors.cs");
exec("./lib/player-hit-region.cs");
exec("./lib/soundsystem.cs");
exec("./lib/sfx.cs");
exec("./lib/Support_Raycasts.cs");
exec("./lib/text.cs");
exec("./lib/items.cs");
//src
exec("./src/overwrite.cs");
exec("./src/events.cs");
exec("./src/player.cs");
exec("./src/sounds.cs");
exec("./src/namelist.cs");
exec("./src/character.cs");
exec("./src/footsteps.cs");
exec("./src/blood.cs");
exec("./src/health.cs");
exec("./src/chat.cs");
exec("./src/gamemodes.cs");
exec("./src/game.cs");
exec("./src/admin.cs");
//src/gamemodes
exec("./src/gamemodes/default.cs");
exec("./src/gamemodes/killer.cs");
//src/tools
exec("./src/tools/bucket.cs");
exec("./src/tools/mop.cs");
exec("./src/tools/key.cs");
//src/tools/weapons
exec("./src/tools/weapons/melee.cs"); //should probably move this to lib but eh
exec("./src/tools/weapons/testsword.cs");
exec("./src/tools/weapons/umbrella.cs");
exec("./src/tools/weapons/cane.cs");
exec("./src/tools/weapons/wrench.cs");
exec("./src/tools/weapons/pan.cs");
exec("./src/tools/weapons/knife.cs");
swordExplosion.soundProfile = "";