function chooseNextClient(%queueName)
{
	%queue = $DS::Queue[%queueName];
	%count = ClientGroup.getCount();
	%words = getWordCount(%queue);
	for (%i = 0; %i < %words; %i++)
		%word[%i] = getWord(%queue, %i);
	for (%i = 0; %i < %count; %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (!isObject(%pl = %cl.player) || !%cl.inDefaultGame())
			continue;
		%bl_id = %cl.bl_id;
		%chance = 1;
		for (%j = 0; %j < %words; %j++)
			if (%word[%j] == %bl_id)
				%chance -= mPow(2, -1 - %j);
		%chance = mPow(%chance, 8);
		%chance[%i] = %chance;
		%sumChance += %chance;
	}
	%value = getRandom() * %sumChance;
	for (%i = 0; %i < %count; %i++)
	{
		%value -= %chance[%i];
		if (%value < 0)
		{
			%chose = ClientGroup.getObject(%i);
			break;
		}
	}
	if (!isObject(%chose))
		return -1;
	$DS::Queue[%queueName] = getWords(%chose.bl_id SPC %queue, 0, 19);
	return %chose;
}