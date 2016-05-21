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
exec("./lib/timedfiring.cs");
exec("./lib/timedraycast.cs");

exec("./Src/Admin.cs");
exec("./Src/Admin_API.cs");
exec("./Src/Admin_Mod.cs");
exec("./Src/Blood.cs");
exec("./Src/Bottomprint.cs");
exec("./Src/Character.cs");
exec("./Src/Chat.cs");
exec("./Src/Connection.cs");
exec("./Src/DeadGames.cs");
exec("./Src/Docs_Commands.cs");
exec("./Src/Events.cs");
exec("./Src/Examine.cs");
exec("./Src/Footsteps.cs");
exec("./Src/Game.cs");
exec("./Src/Gamemodes.cs");
exec("./Src/Health.cs");
exec("./Src/Inventory.cs");
exec("./Src/Player_inventory.cs");
exec("./Src/Invis.cs");
exec("./Src/Namelist.cs");
exec("./Src/Overwrite.cs");
exec("./Src/Player.cs");
exec("./Src/QueueChooser.cs");
exec("./Src/Sleep.cs");
exec("./Src/Sounds.cs");

exec("./Src/Gamemodes/Default.cs");
exec("./Src/Gamemodes/Killer.cs");
exec("./Src/Gamemodes/DespairTrial.cs");
exec("./Src/Gamemodes/Purge.cs");

exec("./Src/Tools/Bloodpack.cs");
exec("./Src/Tools/Bottle.cs");
exec("./Src/Tools/Bucket.cs");
exec("./Src/Tools/Mop.cs");
exec("./Src/Tools/Key.cs");
exec("./Src/Tools/Lockpick.cs");

exec("./Src/Tools/Food/Cheeseburger.cs");

exec("./Src/Tools/Weapons/_Init.cs");

//if (isFile("Add-Ons/Script_UserDataGatherer/Server.cs"))
//	exec("Add-Ons/Script_UserDataGatherer/Server.cs");

swordExplosion.soundProfile = "";
hammerExplosion.lightStartRadius = 0;
hammerExplosion.lightEndRadius = 0;
if (!isObject($DS::GameMode))
	$DS::GameMode = DSGameMode_Trial;