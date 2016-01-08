function getRandomGender(%gender)
{
	if (getRandom(1))
		return "male";
	else
		return "female";
}

function getRandomName(%gender)
{
	return sampleNameList("first-" @ %gender) SPC sampleNameList("last");
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
