package DSCharacterPackage
{
	function GameConnection::applyBodyParts(%this)
	{
		%obj = %this.player;

		if (!isObject(%obj) || %this.miniGame != $DefaultMiniGame)
		{
			Parent::applyBodyParts(%this);
			return;
		}
		%obj.hideNode("ALL");

		%obj.unHideNode("headSkin");
		%obj.unHideNode("chest");
		%obj.unHideNode("pants");
		%obj.unHideNode("larm");
		%obj.unHideNode("rarm");
		%obj.unHideNode("lhand");
		%obj.unHideNode("rhand");

		%obj.unHideNode("lshoe");
		%obj.unHideNode("rshoe");

		if (%obj.bloody["lshoe"])
			%obj.unHideNode("lshoe_blood");
		if (%obj.bloody["rshoe"])
			%obj.unHideNode("rshoe_blood");
		if (%obj.bloody["lhand"])
			%obj.unHideNode("lhand_blood");
		if (%obj.bloody["rhand"])
			%obj.unHideNode("rhand_blood");
		if (%obj.bloody["chest_front"])
			%obj.unHideNode("chest_blood_front");
		if (%obj.bloody["chest_back"])
			%obj.unHideNode("chest_blood_back");
		if (%obj.bloody["chest_lside"])
			%obj.unHideNode("chest_blood_lside");
		if (%obj.bloody["chest_rside"])
			%obj.unHideNode("chest_blood_rside");
	}

	function GameConnection::applyBodyColors(%this)
	{
		%obj = %this.player;

		if (!isObject(%obj) || %this.miniGame != $DefaultMiniGame)
		{
			Parent::applyBodyColors(%this);
			return;
		}
		%obj.setDecalName(%this.decalName);
		%obj.setFaceName(%this.faceName);
		%obj.setNodeColor("headSkin", %this.headColor);
		%obj.setNodeColor("chest", %this.chestColor);
		%obj.setNodeColor("pants", %this.hipColor);
		%obj.setNodeColor("lshoe", %this.llegColor);
		%obj.setNodeColor("rshoe", %this.rlegColor);
		%obj.setNodeColor("larm", %this.larmColor);
		%obj.setNodeColor("rarm", %this.rarmColor);
		%obj.setNodeColor("lhand", %this.lhandColor);
		%obj.setNodeColor("rhand", %this.rhandColor);
		//Set blood colors.
		%obj.setNodeColor("lshoe_blood", "0.7 0 0 1");
		%obj.setNodeColor("rshoe_blood", "0.7 0 0 1");
		%obj.setNodeColor("lhand_blood", "0.7 0 0 1");
		%obj.setNodeColor("rhand_blood", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_front", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_back", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_lside", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_rside", "0.7 0 0 1");
	}
};

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSCharacterPackage");