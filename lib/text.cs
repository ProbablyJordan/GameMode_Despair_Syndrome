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