if (!isObject(DSGameMode_Default))
{
	new ScriptObject(DSGameMode_Default)
	{
		name = "Mosh Pit";
		desc = "Classic deathmatch without roles or detectiving. It's Last Man Standing in this one!";
		class = DSGameMode;
		omit = true; //we need no such thing in OUR moirdar maysteries
	};
}