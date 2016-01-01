function getRandomGender(%gender)
{
  if (getRandom(1))
    return "male";
  else
    return "female";
}

function getRandomName(%gender)
{
  %middleNameCount = getRandom(0, 2);
  %name = sampleNameList("first-" @ %gender);

  for (%i = 0; %i < %middleNameCount; %i++)
    %name = %name SPC sampleNameList("first-" @ getRandomGender());

  return %name SPC samplenameList("last");
}

function sampleNameList(%nameList)
{
  return $MN::NameListItem[%nameList, getRandom($MN::NameListMax[%nameList])];
}

function loadNameList(%nameList)
{
  %file = new FileObject();
  %fileName = $MN::Path @ "data/" @ %nameList @ ".txt";

  if (!%file.openForRead(%fileName))
  {
    error("ERROR: Failed to open '" @ %fileName @ "' for reading");
    %file.delete();
    return;
  }

  deleteVariables("$MN::NameListItem" @ %nameList @ "_*");
  %max = -1;

  while (!%file.isEOF())
  {
    %line = %file.readLine();
    $MN::NameListItem[%nameList, %max++] = getWord(%line, 0);
  }

  %file.close();
  %file.delete();

  $MN::NameListMax[%nameList] = %max;
}

function loadAllNameLists()
{
  loadNameList("first-male");
  loadNameList("first-female");
  loadNameList("last");
}

if (!$MN::LoadedNameLists)
{
  $MN::LoadedNameLists = true;
  loadAllNameLists();
}
