if (!isObject(DSGameMode_Default))
{
	new ScriptObject(DSGameMode_Default)
	{
		name = "Default";
		desc = "There are no rules. You guys decide who wants to go killin', now.";
		class = DSGameMode;
		omit = true;
	};
}