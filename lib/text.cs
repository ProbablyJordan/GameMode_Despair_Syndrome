function naturalGrammarList(%list) {
	%fields = getFieldCount(%list);

	if (%fields < 2) {
		return %list;
	}

	for (%i = 0; %i < %fields - 1; %i++) {
		%partial = %partial @ (%i ? ", " : "") @ getField(%list, %i);
	}

	return %partial SPC "and" SPC getField(%list, %fields - 1);
}

function muffleText(%text, %prob)
{
	if (%text $= "")
		return;
	if (%prob $= "")
		%prob = 25;
	if (%prob <= 0)
		return;
	%result = %text;
	for (%i=0;%i<strlen(%text);%i++)
	{
		if (getSubStr(%text, %i, %i+1) $= " ") //space character
			continue;
		if (getProbability(%prob))
			%result = getSubStr(%result, 0, %i) @ "#" @ getSubStr(%result, %i+1, strlen(%result));
	}
	return %result;
}

function loadDreamList()
{
	%file = new FileObject();
	%fileName = $DS::Path @ "data/dreams.txt";

	if (!%file.openForRead(%fileName))
	{
		error("ERROR: Failed to open '" @ %fileName @ "' for reading");
		%file.delete();
		return;
	}

	deleteVariables("$DS::DreamListItem_*");
	%max = -1;

	while (!%file.isEOF())
	{
		%line = %file.readLine();
		$DS::DreamListItem[%max++] = %line;
	}

	%file.close();
	%file.delete();

	$DS::DreamListMax = %max;
}

if (!$DS::LoadedDreamList)
{
	$DS::LoadedDreamList = true;
	loadDreamList();
}

function getDreamText()
{
	%text = $DS::DreamListItem[getRandom($DS::DreamListMax)];
	return %text;
}