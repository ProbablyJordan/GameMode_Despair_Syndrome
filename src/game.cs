if (!isObject(GameRoundCleanup))
  new SimSet(GameRoundCleanup);

package DespairSyndromePackage
{
  function Armor::onDisabled(%this, %obj)
  {
    if (%obj.isBody && isObject(GameRoundCleanup))
    {
      %obj.inhibitRemoveBody = true;
      GameRoundCleanup.add(%obj);
    }

    Parent::onDisabled(%this, %obj);
  }

  function Player::removeBody(%this)
  {
    if (!%this.inhibitRemoveBody)
      Parent::removeBody(%this);
  }

  function Armor::onUnMount(%this, %obj, %slot)
  {
    Parent::onUnMount(%data, %obj, %slot);

    if (%obj.isBody)
      %obj.playThread(3, "death1");
  }

  function MiniGameSO::Reset(%this, %client)
  {
    if (%this.owner != 0)
      return Parent::reset(%this, %client);

    if (isObject(GameRoundCleanup))
      GameRoundCleanup.deleteAll();

    Parent::reset(%this, %client);

    // Give everyone names, appearances, roles, etc
  }

  function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
  {
    if (%client.miniGame != $DefaultMiniGame)
      return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);

    if (%sourceObject.sourceObject.isBot)
    {
      %sourceClientIsBot = 1;
      %sourceClient = %sourceObject.sourceObject;
    }
    
    %player = %client.player;

    if (isObject(%player))
    {
      %player.setShapeName("", 8564862);

      if (isObject(%player.tempBrick))
      {
        %player.tempBrick.delete();
        %player.tempBrick = 0;
      }

      %player.client = 0;
    }
    else
      warn("WARNING: No player object in GameConnection::onDeath() for client '" @ %client @ "'");

    if (isObject(%client.camera) || isObject(%player))
    { // this part of the code isn't accurate
      if (%client.getControlObject() != %client.camera)
      {
        %client.camera.setMode("Corpse", %player);
        %client.camera.setControlObject(0);
        %client.setControlObject(%client.camera);
      }
    }

    %client.player = 0;
    %client.corpse = %player;

    if ($Damage::Direct[$damageType] && getSimTime() - %player.lastDirectDamageTime < 100 && %player.lastDirectDamageType !$= "")
      %damageType = %player.lastDirectDamageType;

    if (%damageType == $DamageType::Impact && isObject(%player.lastPusher) && getSimTime() - %player.lastPushTime <= 1000)
      %sourceClient = %player.lastPusher;

    %message = '%2 killed %1';

    if (%sourceClient == %client || %sourceClient == 0)
      %message = $DeathMessage_Suicide[%damageType];
    else
      %message = $DeathMessage_Murder[%damageType];

    // removed mid-air kills code here
    // removed mini-game kill points here

    %clientName = %client.getPlayerName();
    
    if (isObject(%sourceClient))
      %sourceClientName = %sourceClient.getPlayerName();
    else if (isObject(%sourceObject.sourceObject) && %sourceObject.sourceObject.getClassName() $= "AIPlayer")
      %sourceClientName = %sourceObject.sourceObject.name;
    else
      %sourceClientName = "";

    // removed mini-game checks here
    // removed death message print here
    // removed %message and %sourceClientName arguments
    messageClient(%client, 'MsgYourDeath', '', %clientName, '', %client.miniGame.respawnTime);

    commandToClient(%client, 'CenterPrint', '', 1);
    %client.miniGame.checkLastManStanding();
  }
};

if ($GameModeArg $= "Add-Ons/GameMode_Despair_Syndrome/gamemode.txt")
   activatePackage("DespairSyndromePackage");
