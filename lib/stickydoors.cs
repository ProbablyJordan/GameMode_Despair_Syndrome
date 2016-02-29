package StickyDoors
{
	function fxDtsBrick::door(%brick, %rot, %cl)
	{
		%data = %brick.getDatablock();
		if (!%data.isDoor || %data.isJVS)
		{
			Parent::door(%brick, %rot, %cl);
			return;
		}
		if ((%data.isOpen && (%rot == 0 || %rot == 1)) || %rot == 4)
		{
			if (%brick.isCCW)
				%closeData = %data.closedCCW;
			else
				%closeData = %data.closedCW;
			if (%brick.angleID & 1)
			{
				%sizeX = %closeData.brickSizeY / 2;
				%sizeY = %closeData.brickSizeX / 2;
			}
			else
			{
				%sizeX = %closeData.brickSizeX / 2;
				%sizeY = %closeData.brickSizeY / 2;
			}
			%sizeZ = %closeData.brickSizeZ / 5;
			%box = %sizeX SPC %sizeY SPC %sizeZ;
			%full = "0 0 0 " @ %box;
			initContainerBoxSearch(%brick.getPosition(), %box, $Typemasks::PlayerObjectType);
			while (isObject(%obj = containerSearchNext()))
				if(TComp_Touching(%brick, %obj, %full))
					%beSticky = 1;
			if (!%beSticky)
				Parent::door(%brick, %rot, %cl);
		}
		else
		{
			Parent::door(%brick, %rot, %cl);
			return;
		}
	}
};
activatePackage("StickyDoors");

function Player::getWorldBoxFixed(%pl)
{
	%box = %pl.getWorldBox();
	if (!$FixBoundingBox[%pl.getDatablock().shapeFile])
	return %box;
	%dX = (getWord(%box, 3) - getWord(%box, 0)) / 8;
	%dY = (getWord(%box, 4) - getWord(%box, 1)) / 8;
	%dZ = (getWord(%box, 5) - getWord(%box, 2)) / 4;
	%cX = (getWord(%box, 3) + getWord(%box, 0)) / 2;
	%cY = (getWord(%box, 4) + getWord(%box, 1)) / 2;
	%cZ = getWord(%box, 2);
	return (%cX - %dX) SPC (%cY - %dY) SPC %cZ SPC (%cX + %dX) SPC (%cY + %dY) SPC (%cZ + %dZ);
}
$FixBoundingBox["base/data/shapes/player/m.dts"] = 1;
$FixBoundingBox["base/data/shapes/player/m_despairsyndrome.dts"] = 1;
function TComp_Inside(%brick, %obj, %box)
{
	if(!isObject(%obj))
		return false;
	%type = %obj.getType();
	if(%type & $Typemasks::PlayerObjectType)
		%bounds = %obj.getWorldBoxFixed();
	else %bounds = %obj.getWorldBox();
	%pos = %brick.getPosition();
	%d0 = getWord(%box, 3) / 2;
	%d1 = getWord(%box, 4) / 2;
	%d2 = getWord(%box, 5) / 2;
	%c0 = getWord(%box, 0) + getWord(%pos, 0);
	%c1 = getWord(%box, 1) + getWord(%pos, 1);
	%c2 = getWord(%box, 2) + getWord(%pos, 2);
	for(%i=0;%i<3;%i++)
	{
		if(getWord(%bounds, %i) < %c[%i] - %d[%i])
			return false;
		if(getWord(%bounds, %i + 3) > %c[%i] + %d[%i])
			return false;
	}
	return true;
}

function TComp_Touching(%brick, %obj, %box)
{
	if(!isObject(%obj))
		return false;
	if(TComp_Inside(%brick, %obj, %box))
		return true;
	return !TComp_Outside(%brick, %obj, %box);
}

function TComp_Outside(%brick, %obj, %box)
{
	if(!isObject(%obj))
		return false;
	%type = %obj.getType();
	if(%type & $Typemasks::PlayerObjectType)
		%bounds = %obj.getWorldBoxFixed();
	else %bounds = %obj.getWorldBox();
	%pos = %brick.getPosition();
	%d0 = getWord(%box, 3) / 2;
	%d1 = getWord(%box, 4) / 2;
	%d2 = getWord(%box, 5) / 2;
	%c0 = getWord(%box, 0) + getWord(%pos, 0);
	%c1 = getWord(%box, 1) + getWord(%pos, 1);
	%c2 = getWord(%box, 2) + getWord(%pos, 2);
	for(%i=0;%i<3;%i++)
	{
		if(getWord(%bounds, %i) >= %c[%i] + %d[%i])
			return true;
		if(getWord(%bounds, %i + 3) <= %c[%i] - %d[%i])
			return true;
	}
	return false;
}