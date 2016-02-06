//soundproof zone funtime
if (!isObject(ZoneGroup))
	new SimSet(ZoneGroup);

registerOutputEvent(fxDTSBrick, removeZone);
registerOutputEvent(fxDTSBrick, createZoneUp, "string 5 32" TAB "string 5 32", 1);
registerOutputEvent(fxDTSBrick, setZoneSoundProof, "bool 0", 1);
function fxDTSBrick::setZoneSoundProof(%this,%val,%client)
{
	if(isObject(%this.zone))
		%this.zone.isSoundProof = %val;
}
function FxDTSBrick::createZoneUp(%this, %height, %margin)
{
	if (!isObject(%this.zone))
	{
		%this.zone = new ScriptObject();
		ZoneGroup.add(%this.zone);
	}

	%box = %this.getWorldBox();

	%this.zone.bounds =
		getWord(%box, 3) - getWord(%box, 0) - %margin SPC
		getWord(%box, 4) - getWord(%box, 1) - %margin SPC
		%height;

	%this.zone.center =
		(getWord(%box, 0) + getWord(%box, 3)) / 2 SPC
		(getWord(%box, 1) + getWord(%box, 4)) / 2 SPC
		getWord(%box, 5) + %height / 2;
}

function fxDTSBrick::removeZone(%this)
{
	if (isObject(%this.zone))
		%this.zone.delete();
}

package DSEventPackage
{
	function fxDTSBrick::onDeath(%this)
	{
		if (isObject(%this.zone))
			%this.zone.delete();
		Parent::onDeath(%this);
	}
	function fxDTSBrick::onRemove(%this)
	{
		if (isObject(%this.zone))
			%this.zone.delete();
		Parent::onRemove(%this);
	}
};
activatePackage(DSEventPackage);

//Clean-up event
registerOutputEvent(Player, cleanPlayer, "list ALL 0 chest 1 hands 2 shoes 3", 1);
function Player::cleanPlayer(%this, %type, %client)
{
	switch(%type)
	{
		case 1:
			%this.bloody["chest_front"] = false;
			%this.bloody["chest_back"] = false;
			%this.bloody["chest_lside"] = false;
			%this.bloody["chest_rside"] = false;
		case 2:
			%this.bloody["lhand"] = false;
			%this.bloody["rhand"] = false;
		case 3:
			%this.bloody["lshoe"] = false;
			%this.bloody["rshoe"] = false;
		default:
			%this.bloody["lshoe"] = false;
			%this.bloody["rshoe"] = false;
			%this.bloody["lhand"] = false;
			%this.bloody["rhand"] = false;
			%this.bloody["chest_front"] = false;
			%this.bloody["chest_back"] = false;
			%this.bloody["chest_lside"] = false;
			%this.bloody["chest_rside"] = false;
	}
	if (isObject(%client))
	{
		%client.applyBodyParts();
		%client.applyBodyColors();
	}
}

//Storage bricks events
registerOutputEvent(fxDTSBrick, makeStorage, "bool 1" TAB "int 1 32 4" TAB "string 199 128" TAB "int 1 4 3", 0);
registerOutputEvent(fxDTSBrick, setStorageItems, "string 199 128", 0);
registerOutputEvent(fxDTSBrick, setRandomStorageItems, "string 199 128", 0);
registerOutputEvent(fxDTSBrick, clearStoredItems, "", 0);
registerOutputEvent(fxDTSBrick, allowStoringItems, "bool 1", 0);
// registerOutputEvent(fxDTSBrick, viewStorage, 1);
// function fxDTSBrick::viewStorage(%this, %client)
// {
// 	if (!isObject(%player = %client.player))
// 		return;
// 	%client.startViewingInventory(%this);
// 	%player.playThread(2, "activate2");
// }

if (!isObject(StorageBrickGroup))
	new SimSet(StorageBrickGroup);
function fxDTSBrick::makeStorage(%this, %toggle, %maxtools, %starteritems, %maxwclass)
{
	if (!%toggle)
	{
		%this.clearStoredItems();
		%this.storageBrick = false;
		%this.randomLoot = "";
		StorageBrickGroup.remove(%this);
		return;
	}
	%this.storageBrick = true;
	%this.maxTools = %maxtools;
	%this.w_class_max = %maxwclass;
	%this.allowStoringItems = true;
	if (getWordCount(%starteritems > 0))
	{
		for (%i=0;%i<getWordCount(%starteritems);%i++)
		{
			%item = getWord(%starteritems, %i);
			if (isObject(%item.getID()))
			{
				%this.storeItem(%item);
			}
		}
	}
	StorageBrickGroup.add(%this);
}
function fxDTSBrick::clearStoredItems(%this)
{
	if (!%this.storageBrick)
		return;
	for (%i=0;%i<%this.maxTools;%i++)
	{
		if (isObject(%this.itemProps[%i]))
			%this.itemProps[%i].delete();
		%this.tool[%i] = "";
	}
}
function fxDTSBrick::storeItem(%this, %data, %props)
{
	%data = %data.getID();
	%maxTools = %this.maxTools;
	if (!%this.storageBrick || !isObject(%data))
		return;

	for (%i = 0; %i < %maxTools; %i++)
	{
		if (!%this.tool[%i])
			break;

		// if (!%data.customPickupMultiple && %this.tool[%i] == %data)
		// {
		// 	if (!%ignoreProps && isObject(%props))
		// 		%props.delete();
		// 	return -1;
		// }
	}

	if (%i == %maxTools)
	{
		if (!%ignoreProps && isObject(%props))
			%props.delete();
		return -1;
	}

	%this.tool[%i] = %data;

	if (isObject(%props))
	{
		%this.itemProps[%i] = %props;
		%props.itemSlot = %i;
		%props.onOwnerChange(%this);
	}

	return %i;
}
function fxDTSBrick::removeToolSlot(%this, %index, %ignoreProps) //used by inventory
{
	if (!%this.storageBrick)
		return;
	%this.tool[%index] = 0;

	if (!%ignoreProps && isObject(%this.itemProps[%index]))
		%this.itemProps[%index].delete();
}
function fxDTSBrick::setStorageItems(%this, %items)
{
	if (!%this.storageBrick)
		return;
	%this.clearStoredItems();
	for (%i=0;%i<getWordCount(%items);%i++)
	{
		%item = getWord(%items, %i);
		if (isObject(%item.getID()))
		{
			%this.storeItem(%item);
		}
	}
}
function fxDTSBrick::setRandomStorageItems(%this, %items)
{
	if (!%this.storageBrick)
		return;
	%this.randomLoot = %items;
}
function fxDTSBrick::allowStoringItems(%this, %bool)
{
	if (!%this.storageBrick)
		return;
	%this.allowStoringItems = %bool;
}