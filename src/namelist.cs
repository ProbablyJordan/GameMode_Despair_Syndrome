function getRandomGender()
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
	return $DS::NameListItem[%nameList, getRandom($DS::NameListMax[%nameList])];
}

function loadNameList(%nameList)
{
	%file = new FileObject();
	%fileName = $DS::Path @ "data/" @ %nameList @ ".txt";

	if (!%file.openForRead(%fileName))
	{
		error("ERROR: Failed to open '" @ %fileName @ "' for reading");
		%file.delete();
		return;
	}

	deleteVariables("$DS::NameListItem" @ %nameList @ "_*");
	%max = -1;

	while (!%file.isEOF())
	{
		%line = %file.readLine();
		$DS::NameListItem[%nameList, %max++] = getWord(%line, 0);
	}

	%file.close();
	%file.delete();

	$DS::NameListMax[%nameList] = %max;
}

function loadAllNameLists()
{
	loadNameList("first-male");
	loadNameList("first-female");
	loadNameList("last");
}

if (!$DS::LoadedNameLists)
{
	$DS::LoadedNameLists = true;
	loadAllNameLists();
}
