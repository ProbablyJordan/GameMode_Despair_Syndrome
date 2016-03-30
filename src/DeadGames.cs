function deadGamesParse(%cl, %msg)
{
	%text = filterString(%msg, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ");
	if(%text $= "Shoot me")
	{
		if($Roulette $= "")
		{
			$Chambers = 6;
			$Roulette = 1;
		}
		if($LastShot == %cl)
		{
			if (isObject(%char = %cl.character))
				%name = getWord(%char.name, 0);
			else
				%name = %cl.name;
			GM_Say("Let someone else have a chance, "@%name@"!");
		}
		else if($Roulette)
		{
			$Roulette = 0;
			$LastShot = %cl;
			switch($Chambers)
			{
				case 6:
					%odds = "six";
				case 5:
					%odds = "five";
				case 4:
					%odds = "four";
				case 3:
					%odds = "three";
				case 2:
					%odds = "two";
				case 1:
					%odds = "one";
			}
			if (isObject(%char = %cl.character))
				%name = getWord(%char.name, 0);
			else
				%name = %cl.name;
			GM_Say("*points the gun at "@%name@"... (Odds: One in "@%odds@")*");
			schedule(1000, 0, "GM_Roulette", %cl);
		}
	}
}

function GM_Roulette(%cl, %stage, %hit)
{
	if(%stage)
	{
		$roulette = 1;
		if(isObject(%cl))
		{
			if (isObject(%char = %cl.character))
				%name = getWord(%char.name, 0);
			else
				%name = %cl.name;
			GM_Say(%name @ (%hit?" was "@getMurderMessage($DamageType::Gun)@"! Oh my!":" got lucky and lives to see another day!"));
		}
		if($chambers == 0 || %hit)
		{
			$chambers = 6;
			$LastShot = "";
		}
	}
	else
	{
		%miss = getRandom(0, $chambers--);
		GM_Say(%miss?"*click*":"*BANG!!!*");
		if(isObject(%cl))
			schedule(1000, 0, "GM_Roulette", %cl, 1, !%miss);
	}
}

function filterString(%text, %tokens)  //This isn't default - cover our asses
{
	for(%i=0;%i<strLen(%text);%i++)
		if(strPos(%tokens, getSubStr(%text, %i, 1)) != -1)
			%result = %result@getSubStr(%text, %i, 1);
	return %result;
}

function DG_GetPureText(%text)
{
	return trim(filterString(strLwr(%text)));
}

function getMurderMessage(%damageType)
{
	%word0 = "bloodily\tbrutally\tcruelly\thorribly\tmercilessly\tmessily\truthlessly\tviciously\tviolently";
	//Charles:	Mercilessly, ruthlessly, violently.
	//Xalos:	Bloodily, brutally, cruelly, horribly, messily, viciously.
	%word1 = "butchered\tcleaved\tcut down\tdecapitated\tdisembodied\tdismembered\tdissected\tmurdered\tshot down\tshredded\tslaughtered";
	%weap1 = "knife\tknife\knife\tall\tall\tall\tall\tknife\tall\tbullet\tknife\tall";
	//Charles:	Cleaved, cut down.
	//Pvt. Block:	Decapitated, shredded.
	//Xalos:	Butchered, disembodied, dismembered, dissected, murdered, shot down, slaughtered.
	for(%i=0;%i<getFieldCount(%word1);%i++)
		if(getField(%weap1, %i) $= "All")
			%word2 = %word2 TAB getField(%word1, %i);
	if(%damageType == $DamageType::SniperRifle)
		for(%i=0;%i<getFieldCount(%word1);%i++)
			if(getField(%weap1, %i) $= "Sniper")
				%word2 = %word2 TAB getField(%word1, %i);
	if(%damageType == $DamageType::SniperRifle || %damageType == $DamageType::Gun)
		for(%i=0;%i<getFieldCount(%word1);%i++)
			if(getField(%weap1, %i) $= "Bullet")
				%word2 = %word2 TAB getField(%word1, %i);
	if(%damageType == $DamageType::Sword || %damageType == $DamageType::ButterflyKnifeDirect)
		for(%i=0;%i<getFieldCount(%word1);%i++)
			if(getField(%weap1, %i) $= "Knife")
				%word2 = %word2 TAB getField(%word1, %i);
	%word2 = trim(%word2);
	return getField(%word0, getRandom(0, getFieldCount(%word0) - 1)) SPC getField(%word2, getRandom(0, getFieldCount(%word2) - 1));
}

function GM_Say(%msg)
{
	schedule(1000, 0, "GM_Talk", %msg);
}

function GM_Talk(%msg)
{
	for(%i=0;%i<ClientGroup.getCount();%i++)
	{
		%cl = ClientGroup.getObject(%i);
		if (!isObject(%cl.player))
			commandToClient(%cl, 'chatMessage', %cl, '', '', "\c7[BOT] Game Master<color:aaaaaa>: "@%msg, "", "Game Master", "", %msg);
	}
	echo("Game Master: "@%msg);
}