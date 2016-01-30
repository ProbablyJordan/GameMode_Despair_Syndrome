function GameConnection::startViewingInventory(%this, %target)
{
	%player = %this.player;

	if (!isObject(%player))
		return;

	if (!isObject(%target))
		return;

	%player.isViewingInventory = 1;
	%player.inventoryTarget = %target;
	%player.inventoryIndex = 0;
	%player.inventorySlotIndex = 0;
	%player.inventoryScroll = 0;
	%this.checkDistanceLoop(%target);
	%this.updateInventoryView();
}

function GameConnection::stopViewingInventory(%this)
{
	%player = %this.player;

	if (!isObject(%player))
		return;

	%player.isViewingInventory = 0;
	cancel(%this.inventoryDistanceSchedule);
	%this.updateInventoryView();
}

function GameConnection::updateInventoryView(%this)
{
	%player = %this.player;

	if (!isObject(%player) || !%player.isViewingInventory || !isObject(%player.inventoryTarget))
	{
		commandToClient(%this, 'ClearCenterPrint');
		return;
	}

	if (vectorDist(%player.getPosition(), getWords(%player.inventoryTarget.getTransform(), 0, 2)) > 6)
	{
		%this.stopViewingInventory();
		return;
	}

	%count = 0;

	for (%i = 0; %i < %player.inventoryTarget.maxTools; %i++) {
		%slot = %player.inventoryTarget.tool[%i];

		if (isObject(%slot)) {
			%listSlot[%count] = %slot;
			%listReal[%count] = %i;
			%count++;
		}
	}

	%shown = 6;

	%last = getMin(%player.inventoryScroll + %shown, %count);
	%text = "\c6Inventory | " @ %player.inventoryIndex + 1 @ "/" @ %count;
	%text = %text @ "\n<just:left><font:palatino linotype:20>\n";

	for (%i = %player.inventoryScroll; %i < %last; %i++)
	{
		%hl = %i == %player.inventoryIndex;

		if (%hl)
			%text = %text @ "<div:1>";

		%slot = %listSlot[%i];
		%text = %text @ (%hl ? "\c6" : "\c7") @ %slot.uiName;

		if (%hl)
			%text = %text @ "<just:right>\c6 <just:left>";

		%text = %text @ "\n";
	}

	%player.inventorySlotIndex = %listReal[%player.inventoryIndex];

	%text = %text @ "<font:palatino linotype:18>\n<just:center>\c6Use \c3building controls \c6to navigate\n";
	//%text = %text @ "\c6" @ %player.craftingIndex + 1 @ " of " @ %total;
	//%text = %text @ "<just:right>" @ (%canUp ? "\c6Up �\c3" : "\c7Up �") @ " Shift Away";
	//%text = %text @ " \c6 �  " @ (%canDown ? "Down �\c3" : "\c7Down �") @ " Shift Back ";
	//%text = %text @ "\n" @ (%canSelect ? "\c6Select �\c3" : "\c7Select �") @ " Plant Brick ";
	//%text = %text @ " \c6 �  Exit � \c3Cancel Brick ";

	%this.centerPrint(%text);
}

function GameConnection::checkDistanceLoop(%this, %target)
{
	cancel(%this.checkDistanceLoop);
	if (!isObject(%player = %this.player) || !isObject(%target))
	{
		%this.stopViewingInventory();
		return;
	}
	if (vectorDist(%player.getPosition(), getWords(%target.getTransform(), 0, 2)) > 6)
	{
		%this.stopViewingInventory();
		return;
	}
	%this.checkDistanceLoop = %this.schedule(500, checkDistanceLoop, %target);
}

package RPG_InventoryView
{
	function serverCmdCancelBrick(%client)
	{
		%player = %client.player;

		if (isObject(%player) && %player.isViewingInventory)
		{
			%client.stopViewingInventory();
			return;
		}

		Parent::serverCmdCancelBrick(%client);
	}

	function serverCmdPlantBrick(%client)
	{
		%player = %client.player;

		if (isObject(%player) && %player.isViewingInventory)
		{
			if (vectorDist(%player.getPosition(), getWords(%player.inventoryTarget.getTransform(), 0, 2)) > 6)
			{
				%client.stopViewingInventory();
				return;
			}
			if (isobject(%player.inventoryTarget.tool[%player.inventorySlotIndex]))
			{
				if (%player.addTool(%player.inventoryTarget.tool[%player.inventorySlotIndex], %player.inventoryTarget.getItemProps(%player.inventorySlotIndex), 1) != -1)
				{
					%player.inventoryTarget.removeToolSlot(%player.inventorySlotIndex, 1);
					%player.inventoryTarget.itemProps[%player.inventorySlotIndex] = "";
				}
			}
			%player.playThread(2, "activate");
			%client.updateInventoryView();
			return;
		}

		Parent::serverCmdPlantBrick(%client);
	}

	function serverCmdRotateBrick(%client, %angle)
	{
		%player = %client.player;

		if (isObject(%player) && %player.isViewingInventory)
		{
			%client.updateInventoryView();
			return;
		}

		Parent::serverCmdRotateBrick(%client, %Angle);
	}

	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		%player = %client.player;

		if (isObject(%player) && %player.isViewingInventory)
		{
			if (%x != 0 && %y == 0 && %z == 0)
			{
				%count = 0;

				for (%i = 0; %i < %player.inventoryTarget.maxTools; %i++) {
					%slot = %player.inventoryTarget.tool[%i];

					if (isObject(%slot)) {
						%listSlot[%count] = %slot;
						%listReal[%count] = %i;
						%count++;
					}
				}

				%shown = 6;

				if (%x > 0)
					%player.inventoryIndex--;
				else
					%player.inventoryIndex++;

				%player.inventoryIndex = mClamp(%player.inventoryIndex, 0, %count - 1);

				if (%player.inventoryIndex < %player.inventoryScroll)
					%player.inventoryScroll = %player.inventoryIndex;
				else if (%player.inventoryIndex - %player.inventoryScroll >= %shown)
					%player.inventoryScroll = getMax(0, %player.inventoryIndex - %shown + 1);
			}

			%client.updateInventoryView();
			return;
		}

		Parent::serverCmdShiftBrick(%client, %x, %y, %z);
	}
};

activatePackage("RPG_InventoryView");
