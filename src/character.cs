function getRandomAppearance(%gender)
{
	%appearance = new ScriptObject()
	{
		skinColor = getRandomSkinColor();
		faceName = getRandomFaceName(%gender);
		decalName = getRandomDecalName();
		hairName = getRandomHairName(%gender);
		shirtColor = getRandomGenericColor();
		pantsColor = getRandomPantsColor();
		shoesColor = getRandomPantsColor();
		hairColor = getRandomHairColor();
	};
	return %appearance;
}

function getRandomSkinColor()
{
	%index = -1;
	%color[%index++] = "0.956863 0.878431 0.784314";
	%color[%index++] = "1 0.878431 0.611765";
	%color[%index++] = "1 0.603922 0.423529";
	if (getRandom(1, 10) == 1) //rare color
		%color[%index++] = "0.392157 0.196078 0";
	%pick = getRandom(%index);
	%r = max(min(getWord(%color[%pick], 0) + 0.05 - getRandom() * 0.1, 1), 0);
	%g = max(min(getWord(%color[%pick], 1) + 0.05 - getRandom() * 0.1, 1), 0);
	%b = max(min(getWord(%color[%pick], 2) + 0.05 - getRandom() * 0.1, 1), 0);

	return %r SPC %g SPC %b SPC 1;
}

function getRandomHairColor()
{
	//Natural colors
	//Grays/"blues"
	%i = -1;
	%color[%i++] = "0.753 0.816 0.816";
	%color[%i++] = "0.439 0.502 0.565";
	%color[%i++] = "0.251 0.251 0.376";
	%color[%i++] = "0.125 0.063 0.188";
	//Browns (light)
	%color[%i++] = "0.816 0.816 0.69";
	%color[%i++] = "0.627 0.502 0.376";
	%color[%i++] = "0.376 0.251 0.251";
	%color[%i++] = "0.251 0.125 0.125";
	//Browns
	%color[%i++] = "0.878 0.69 0.376";
	%color[%i++] = "0.753 0.502 0.251";
	%color[%i++] = "0.627 0.314 0.125";
	%color[%i++] = "0.251 0 0";
	//Orange
	%color[%i++] = "1 0.502 0.251";
	%color[%i++] = "0.753 0.251 0";
	%color[%i++] = "0.502 0.125 0";
	%color[%i++] = "0.251 0.063 0";
	//Blonde
	%color[%i++] = "1 1 0.627";
	%color[%i++] = "1 0.753 0.251";
	%color[%i++] = "0.753 0.502 0";
	%color[%i++] = "0.502 0.251 0";
	//Ginger
	%color[%i++] = "1 0.251 0";
	%color[%i++] = "1 0.502 0";
	%color[%i++] = "1 0.878 0";
	//Dyed colors
	if (getRandom(1, 3) == 1) //sorta rarer
	{
		//Green
		%color[%i++] = "0.753 1 0";
		%color[%i++] = "0.251 0.753 0";
		%color[%i++] = "0 0.502 0.251";
		%color[%i++] = "0 0.251 0.251";
		//Teal
		%color[%i++] = "0 1 0.502";
		%color[%i++] = "0 0.753 0.502";
		%color[%i++] = "0 0.502 0.502";
		%color[%i++] = "0 0.251 0.376";
		//Blues
		%color[%i++] = "0.502 0.941 1";
		%color[%i++] = "0 0.753 1";
		%color[%i++] = "0 0.251 0.753";
		%color[%i++] = "0.125 0 0.502";
		//Purple
		%color[%i++] = "0.878 0.627 1";
		%color[%i++] = "0.753 0.251 1";
		%color[%i++] = "0.502 0 0.753";
		//Pinks
		%color[%i++] = "1 0.753 0.753";
		%color[%i++] = "1 0.376 0.502";
		%color[%i++] = "0.753 0 0.376";
		%color[%i++] = "0.502 0 0.376";
	}

	return %color[getRandom(0, %i)] SPC 1;
}

function getRandomGenericColor()
{
	%color0 = "0.9 0 0";
	%color1 = "0.9 0 0";
	%color2 = "0.74902 0.180392 0.482353";
	%color3 = "0.388235 0 0.117647";
	%color4 = "0.133333 0.270588 0.270588";
	%color5 = "0 0.141176 0.333333";
	%color6 = "0.105882 0.458824 0.768627";
	%color7 = "1 1 1";
	%color8 = "0.0784314 0.0784314 0.0784314";
	%color9 = "0.92549 0.513726 0.678431";
	%color10 = "0 0.5 0.25";
	%color11 = "0.784314 0.921569 0.490196";
	%color12 = "0.541176 0.698039 0.552941";
	%color13 = "0.560784 0.929412 0.960784";
	%color14 = "0.698039 0.662745 0.905882";
	%color15 = "0.878431 0.560784 0.956863";
	%color16 = "0.888 0 0";
	%color17 = "1 0.5 0";
	%color18 = "0.99 0.96 0";
	%color19 = "0.2 0 0.8";
	%color20 = "0 0.471 0.196";
	%color21 = "0 0.2 0.64";
	%color22 = "0.596078 0.160784 0.392157";
	%color23 = "0.55 0.7 1";
	%color24 = "0.85 0.85 0.85";
	%color25 = "0.1 0.1 0.1";
	%color26 = "0.9 0.9 0.9";
	%color27 = "0.75 0.75 0.75";
	%color28 = "0.5 0.5 0.5";
	%color29 = "0.2 0.2 0.2";
	%color30 = "0.901961 0.341176 0.0784314";

	return %color[getRandom(0, 30)] SPC 1;
}
function getRandomPantsColor()
{
	%color0 = "0.75 0.75 0.75";
	%color1 = "0.2 0.2 0.2";
	%color2 = "0.388 0 0.117";
	%color3 = "0.133 0.27 0.27";
	%color4 = "0 0.141 0.333";
	%color5 = "0.078 0.078 0.078";

	return %color[getRandom(0, 5)] SPC 1;
}


//Some faces are from winterbite face pack located here: https://www.dropbox.com/s/misn71lpawwi8le/Face_WinterBite.zip
function getRandomFaceName(%gender)
{
	%high = -1;
	%choice[%high++] = "smiley";
	if (%gender $= "male")
	{
		%choice[%high++] = "Male07Smiley";
		%choice[%high++] = "BrownSmiley";
		%choice[%high++] = "memeHappy";
		%choice[%high++] = "memeBlockMongler";
		// %choice[%high++] = "smileyBlonde";
		%choice[%high++] = "smileyCreepy";
		//Winterbite faces:
		%choice[%high++] = "smileyST";
		%choice[%high++] = "kleinerSmiley";
		%choice[%high++] = "kleinerSmiley2ST";
		%choice[%high++] = "kleinerSmiley2";
	}
	else
	{
		//Winterbite faces:
		%choice[%high++] = "smileyfST";
		%choice[%high++] = "smileyfCreepy";
		%choice[%high++] = "smileyf";
		%choice[%high++] = "KleinerfSmileysST";
		%choice[%high++] = "KleinerfSmiley";
	}

	return %choice[getRandom(%high)];
}

function getRandomDecalName()
{
	%high = -1;

	%choice[%high++] = "";
	%choice[%high++] = "Mod-Suit";
	%choice[%high++] = "Mod-Pilot";
	%choice[%high++] = "Mod-Army";
	%choice[%high++] = "Meme-Mongler";
	%choice[%high++] = "Medieval-YARLY";
	%choice[%high++] = "Medieval-Rider";
	%choice[%high++] = "Medieval-ORLY";
	%choice[%high++] = "Medieval-Lion";
	%choice[%high++] = "Medieval-Eagle";
	%choice[%high++] = "Hoodie";
	%choice[%high++] = "Alyx";

	return %choice[getRandom(%high)];
}

function getRandomHairName(%gender)
{
	%high = -1;
	//Unisex hairs
	%choice[%high++] = "hair_fabio";
	%choice[%high++] = "hair_wig";
	if (%gender $= "male") //Male hairs
	{
		%choice[%high++] = "hair_cornrows";
		%choice[%high++] = "hair_emo";
		%choice[%high++] = "hair_familiar";
		%choice[%high++] = "hair_greaser";
		%choice[%high++] = "hair_layered";
		%choice[%high++] = "hair_messy";
		%choice[%high++] = "hair_neat";
		%choice[%high++] = "hair_suav";
		// %choice[%high++] = "hair_bowl";
		%choice[%high++] = "hair_flattop";
		%choice[%high++] = "hair_freddie";
		%choice[%high++] = "hair_jewfro";
		%choice[%high++] = "hair_mohawk";
		%choice[%high++] = "hair_mullet";
		%choice[%high++] = "hair_parted";
		%choice[%high++] = "hair_phoenix";
		%choice[%high++] = "hair_pompadour";
		%choice[%high++] = "hair_punk";
		%choice[%high++] = "hair_shaggy";
		// %choice[%high++] = "";
	}
	else //Female hairs
	{
		%choice[%high++] = "hair_bobcut";
		%choice[%high++] = "hair_broad";
		%choice[%high++] = "hair_bunn";
		%choice[%high++] = "hair_headband";
		%choice[%high++] = "hair_mom";
		%choice[%high++] = "hair_ponytail";
		%choice[%high++] = "hair_daphne";
		%choice[%high++] = "hair_velma";
		%choice[%high++] = "hair_mahiru";
		%choice[%high++] = "hair_maya";
		// %choice[%high++] = "";
	}

	return %choice[getRandom(%high)];
}

function getRandomSpecialChar(%char)
{
	//%char[%chars++-1] = "Name\tChance\tGender\tSkin\tFace\tDecal\tHair\tShirt\tPants\tShoes\tHair";
	%char[%chars++-1] = "William T. Riker\t1\tM\t1 0.8 0.6\tMale07Smiley\tSpace-New\thair_messy\t0.7 0.1 0.1\t0.1 0.1 0.1\t0.1 0.1 0.1\t0.05 0.025 0";
	%char[%chars++-1] = "Mr. T\t1\tM\t0.4 0.2 0\tBrownSmiley\tMedieval-Tunic\thair_mohawk\t0 0 0\t0.8 0 0\t0.2 0.2 0.2\t0 0 0";
	%char[%chars++-1] = "Jamie Hyneman\t1\tM\t1 0.8 0.6\tJamie\tAAA-None\tscoutHat\t1 1 1\t0.2 0.2 0.2\t0.4 0.2 0\t0.1 0.1 0.1";
	%char[%chars++-1] = "Adam Savage\t1\tM\t1 0.8 0.6\tAdamSavage\tAAA-None\theadSkin\t0.2 0.2 0.2\t0.2 0.2 0.2\t0.4 0.2 0\t1 0.8 0.6";
	for (%i = 0; %i < %chars; %i++)
		%chance += getField(%char[%i], 1);
	%choose = getRandom() * %chance;
	for (%i = 0; %i < %chars; %i++)
	{
		%choose -= getField(%char[%i], 1);
		if (%choose <= 0)
		{
			%choose = %char[%i];
			break;
		}
	}
	%char.name = getField(%choose, 0);
	if(getField(%choose, 2) $= "M")
		%char.gender = "male";
	else
		%char.gender = "female";
	%app = new ScriptObject();
	%app.skinColor = getField(%choose, 3) @ " 1";
	%app.faceName = getField(%choose, 4);
	%app.decalName = getField(%choose, 5);
	%app.hairName = getField(%choose, 6);
	%app.shirtColor = getField(%choose, 7) @ " 1";
	%app.pantsColor = getField(%choose, 8) @ " 1";
	%app.shoesColor = getField(%choose, 9) @ " 1";
	%app.hairColor = getField(%choose, 10) @ " 1";
	%char.appearance = %app;
}

package DSCharacterPackage
{
	function GameConnection::applyBodyParts(%this)
	{
		%obj = %this.player; 
		%character = %this.character;
		if (!isObject(%obj) || %this.miniGame != $DefaultMiniGame || !isObject(%character))
		{
			Parent::applyBodyParts(%this);
			return;
		}
		%appearance = %character.appearance;
		%gender = %character.gender;

		%obj.hideNode("ALL");

		%obj.unHideNode("headSkin");
		%obj.unHideNode((%gender $= "female" ? "fem" : "") @ "chest");
		%obj.unHideNode("pants");
		%obj.unHideNode("larm" @ (%gender $= "female" ? "slim" : ""));
		%obj.unHideNode("rarm" @ (%gender $= "female" ? "slim" : ""));
		%obj.unHideNode("lhand");
		%obj.unHideNode("rhand");

		%obj.unHideNode("lshoe");
		%obj.unHideNode("rshoe");

		if (%appearance.hairName !$= "")
			%obj.unHideNode(%appearance.hairName);

		if (%obj.bloody["lshoe"])
			%obj.unHideNode("lshoe_blood");
		if (%obj.bloody["rshoe"])
			%obj.unHideNode("rshoe_blood");
		if (%obj.bloody["lhand"])
			%obj.unHideNode("lhand_blood");
		if (%obj.bloody["rhand"])
			%obj.unHideNode("rhand_blood");
		if (%obj.bloody["chest_front"])
			%obj.unHideNode((%gender $= "female" ? "fem" : "") @ "chest_blood_front");
		if (%obj.bloody["chest_back"])
			%obj.unHideNode((%gender $= "female" ? "fem" : "") @ "chest_blood_back");
		if (%obj.bloody["chest_lside"])
			%obj.unHideNode((%gender $= "female" ? "fem" : "") @ "chest_blood_lside");
		if (%obj.bloody["chest_rside"])
			%obj.unHideNode((%gender $= "female" ? "fem" : "") @ "chest_blood_rside");

		%obj.setHeadUp(false);
	}

	function GameConnection::applyBodyColors(%this)
	{
		%obj = %this.player; 
		%character = %this.character;
		if (!isObject(%obj) || %this.miniGame != $DefaultMiniGame || !isObject(%character))
		{
			Parent::applyBodyColors(%this);
			return;
		}
		%obj.applyBodyColors();
	}
};

function Player::applyBodyColors(%obj)
{
	if (!isObject(%character = %obj.client.character) && !isObject(%character = %obj.character))
		return;
	%appearance = %character.appearance;
	%obj.setDecalName(%appearance.decalName);
	%obj.setFaceName(%appearance.faceName);
	if (%obj.charred)
	{
		%obj.setNodeColor("headSkin", getCharredColor(%appearance.skinColor));
		%obj.setNodeColor("chest", getCharredColor(%appearance.shirtColor));
		%obj.setNodeColor("femchest", getCharredColor(%appearance.shirtColor));
		%obj.setNodeColor("pants", getCharredColor(%appearance.pantsColor));
		%obj.setNodeColor("lshoe", getCharredColor(%appearance.shoesColor));
		%obj.setNodeColor("rshoe", getCharredColor(%appearance.shoesColor));
		%obj.setNodeColor("larm", getCharredColor(%appearance.shirtColor));
		%obj.setNodeColor("rarm", getCharredColor(%appearance.shirtColor));
		%obj.setNodeColor("larmslim", getCharredColor(%appearance.shirtColor));
		%obj.setNodeColor("rarmslim", getCharredColor(%appearance.shirtColor));
		%obj.setNodeColor("lhand", getCharredColor(%appearance.skinColor));
		%obj.setNodeColor("rhand", getCharredColor(%appearance.skinColor));
		%obj.setNodeColor(%appearance.hairName, getCharredColor(%appearance.hairColor));
	}
	else
	{
		%obj.setNodeColor("headSkin", %appearance.skinColor);
		%obj.setNodeColor("chest", %appearance.shirtColor);
		%obj.setNodeColor("femchest", %appearance.shirtColor);
		%obj.setNodeColor("pants", %appearance.pantsColor);
		%obj.setNodeColor("lshoe", %appearance.shoesColor);
		%obj.setNodeColor("rshoe", %appearance.shoesColor);
		%obj.setNodeColor("larm", %appearance.shirtColor);
		%obj.setNodeColor("rarm", %appearance.shirtColor);
		%obj.setNodeColor("larmslim", %appearance.shirtColor);
		%obj.setNodeColor("rarmslim", %appearance.shirtColor);
		%obj.setNodeColor("lhand", %appearance.skinColor);
		%obj.setNodeColor("rhand", %appearance.skinColor);
		//hair color
		%obj.setNodeColor(%appearance.hairName, %appearance.hairColor);
		//Set blood colors.
		%obj.setNodeColor("lshoe_blood", "0.7 0 0 1");
		%obj.setNodeColor("rshoe_blood", "0.7 0 0 1");
		%obj.setNodeColor("lhand_blood", "0.7 0 0 1");
		%obj.setNodeColor("rhand_blood", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_front", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_back", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_lside", "0.7 0 0 1");
		%obj.setNodeColor("chest_blood_rside", "0.7 0 0 1");
		%obj.setNodeColor("femchest_blood_front", "0.7 0 0 1");
		%obj.setNodeColor("femchest_blood_back", "0.7 0 0 1");
	}
}

function getCharredColor(%color)
{
	%value = getWord(%color, 0) * 0.2126 + getWord(%color, 1) * 0.7152 + getWord(%color, 2) * 0.0722;
	%value = (%value + 0.3) / 2;
	return %value SPC %value SPC %value SPC getWord(%color, 3);
}

if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSCharacterPackage");