$DS::Path = filePath(expandFileName("./description.txt")) @ "/";

//lib
exec("./lib/colors.cs");
exec("./lib/decals.cs");
exec("./lib/environment.cs");
exec("./lib/inv_slots.cs");
exec("./lib/itemprops.cs");
exec("./lib/math-angles-vectors.cs");
exec("./lib/misc.cs");
exec("./lib/noObservers.cs");
exec("./lib/player-hit-region.cs");
exec("./lib/soundsystem.cs");
exec("./lib/sfx.cs");
exec("./lib/Support_Raycasts.cs");
exec("./lib/text.cs");
exec("./lib/items.cs");
exec("./lib/timedfiring.cs");
exec("./lib/timedraycast.cs");
//src
exec("./src/overwrite.cs");
exec("./src/events.cs");
exec("./src/player.cs");
exec("./src/examine.cs");
exec("./src/sounds.cs");
exec("./src/namelist.cs");
exec("./src/character.cs");
exec("./src/footsteps.cs");
exec("./src/blood.cs");
exec("./src/bottomPrint.cs");
exec("./src/health.cs");
exec("./src/inventory.cs");
exec("./src/docs_commands.cs");
exec("./src/chat.cs");
exec("./src/gamemodes.cs");
exec("./src/game.cs");
exec("./src/admin.cs");
exec("./src/connection.cs");
//src/gamemodes
exec("./src/gamemodes/default.cs");
exec("./src/gamemodes/killer.cs");
exec("./src/gamemodes/despairtrial.cs");
//src/tools
exec("./src/tools/bucket.cs");
exec("./src/tools/mop.cs");
exec("./src/tools/key.cs");
exec("./src/tools/lockpick.cs");
//src/tools/weapons
exec("./src/tools/weapons/melee.cs"); //should probably move this to lib but eh
exec("./src/tools/weapons/testsword.cs");
exec("./src/tools/weapons/umbrella.cs");
exec("./src/tools/weapons/cane.cs");
exec("./src/tools/weapons/wrench.cs");
exec("./src/tools/weapons/pan.cs");
exec("./src/tools/weapons/knife.cs");
swordExplosion.soundProfile = "";
hammerExplosion.lightStartRadius = 0;
hammerExplosion.lightEndRadius = 0;
if (!isObject($DS::GameMode))
	$DS::GameMode = DSGameMode_Trial; //"Main" mode in works