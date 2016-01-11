$DS::Path = filePath(expandFileName("./description.txt")) @ "/";

//lib
exec("./lib/colors.cs");
exec("./lib/decals.cs");
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
exec("./src/game.cs");
exec("./src/chat.cs");
//src/tools
exec("./src/tools/bucket.cs");
exec("./src/tools/mop.cs");
exec("./src/tools/key.cs");
//src/tools/weapons
exec("./src/tools/weapons/testsword.cs");

package OnePunchMan
{
  function serverCmdActivateStuff(%client)
  {
    Parent::serverCmdActivateStuff(%client);

    if (true) // oh god
    {
      %control = %client.getControlObject();
      %a = %control.getEyePoint();
      %b = vectorAdd(%a, vectorScale(%control.getEyeVector(), 100));
      %ray = containerRayCast(%a, %b, $TypeMasks::FxBrickObjectType);
      if (%ray)
        %ray.fakeKillBrick("0 0 0", 120);
    }
  }
};

activatePackage("OnePunchMan");
