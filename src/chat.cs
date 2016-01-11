package ChatPackage
{
  function serverCmdStartTalking(%client)
  {
    if (!%client.inDefaultGame())
      Parent::serverCmdStartTalking(%client);
  }

  function serverCmdStartTalking(%client)
  {
    if (!%client.inDefaultGame())
      Parent::serverCmdStopTalking(%client);
  }

  function serverCmdMessageSent(%client, %text)
  {
    if (!%client.inDefaultGame() || !isObject(%client.character))
      return Parent::serverCmdMessageSent(%client, %text);

    %text = trim(stripMLControlChars(%text));

    if (%text $= "")
      return;

    messageAll('', '<color:ffaa44>%1<color:ffffff>: %2',
      %client.character.name, %text);
  }

  function serverCmdTeamMessageSent(%client, %text)
  {
    if (!%client.inDefaultGame())
      return Parent::serverCmdMessageSent(%client, %text);

    messageClient(%client, '', '\c5Team chat does nothing right now.');
  }
};

activatePackage("ChatPackage");
