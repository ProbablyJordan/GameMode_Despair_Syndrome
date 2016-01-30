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
		if (getRandom(0, 100) < %prob)
			%result = getSubStr(%result, 0, %i) @ "#" @ getSubStr(%result, %i+1, strlen(%result));
	}
	return %result;
}