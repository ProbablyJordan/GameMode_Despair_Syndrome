package DSExaminePackage
{
	function Player::activateStuff(%this)
	{
		Parent::activateStuff(%this);
		%client = %this.client;
		if (!isObject(%client) || !%client.inDefaultGame())
			return;

		%start = %this.getEyePoint();
		%end = vectorAdd(%start, vectorScale(%this.getEyeVector(), 6));

		%mask = $TypeMasks::FxBrickObjectType | $TypeMasks::PlayerObjectType;
		%ray = containerRayCast(%start, %end, %mask, %this);

		if (!%ray || !(%ray.getType() & $TypeMasks::PlayerObjectType))
			return;

		%other = %ray.client;

		if (!isObject(%other) || !%other.inDefaultGame() || !isObject(%other.character))
			return;

		%text = "\c6" @ (%other.character.gender $= "Male" ? "His" : "Her") @ " name is \c3" @ %other.character.name;

		if (%ray.tool[%ray.currTool].uiName !$= "")
		{
			%text = %text @ "\n\c6They're holding a \c3" @ %ray.tool[%ray.currTool].uiName @ "\c6.\n";
		}

		%health = %ray.getHealth() / %ray.getMaxHealth();

		if (%health <= 0.35)
			%text = %text @ "<color:ff66aa>They look badly injured.\n";
		else if (%health <= 0.7)
			%text = %text @ "<color:ffaa66>They look injured.\n";
		else if (%health < 1)
			%text = %text @ "\c6They're slightly injured.\n";

		%client.centerPrint(%text, 3);
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSExaminePackage");