$DS::Path = filePath(expandFileName("./description.txt")) @ "/";
exec("./prefs.cs");

exec("./res/shapes/debug/init.cs");

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
// exec("./lib/stickydoors.cs");
exec("./lib/sfx.cs");
exec("./lib/Support_Raycasts.cs");
exec("./lib/text.cs");
exec("./lib/items.cs");
exec("./lib/timedfiring.cs");
exec("./lib/timedraycast.cs");

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
exec("./src/admin_API.cs");
exec("./src/connection.cs");

exec("./src/gamemodes/default.cs");
exec("./src/gamemodes/killer.cs");
exec("./src/gamemodes/despairtrial.cs");

exec("./src/tools/bloodpack.cs");
exec("./src/tools/bottle.cs");
exec("./src/tools/bucket.cs");
exec("./src/tools/mop.cs");
exec("./src/tools/key.cs");
exec("./src/tools/lockpick.cs");

exec("./src/tools/food/cheeseburger.cs");

exec("./src/tools/weapons/melee.cs");
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
	$DS::GameMode = DSGameMode_Trial;