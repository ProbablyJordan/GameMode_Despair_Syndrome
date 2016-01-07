$DS::Path = filePath(expandFileName("./description.txt")) @ "/";

//lib
exec("./lib/colors.cs");
exec("./lib/decals.cs");
exec("./lib/itemprops.cs");
exec("./lib/math-angles-vectors.cs");
exec("./lib/player-hit-region.cs");
exec("./lib/sfx.cs");
exec("./lib/text.cs");
exec("./lib/items.cs");
//src/tools
exec("./src/tools/bucket.cs");
exec("./src/tools/mop.cs");
//src
exec("./src/player.cs");
exec("./src/sounds.cs");
exec("./src/namelist.cs");
exec("./src/character.cs");
exec("./src/footsteps.cs");
exec("./src/blood.cs");
exec("./src/health.cs");
exec("./src/game.cs");

function serverCmdReload(%client)
{
	if(%client.bl_id != getNumKeyID())
	{
		return;
	}
	exec("./server.cs");
}