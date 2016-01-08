function Player::addItem(%player,%item)
{
	%item = %item.getID();

	%client = %player.client;
	for(%i = 0; %i < %player.getDatablock().maxTools; %i++)
	{
		%tool = %player.tool[%i];
		if(%tool == 0)
		{
			%player.tool[%i] = %item;
			%player.weaponCount++;
			messageClient(%client,'MsgItemPickup','',%i,%item);
			return true;
		}
	}
	return false;
}

function Player::removeItem(%this,%item)
{
	if(!isObject(%this) || !isObject(%item.getID()))
		return;

	%item = %item.getID();

	for(%i=0;%i<%this.getDatablock().maxTools;%i++)
	{
		if(isObject(%this.tool[%i]))
		{
			%tool=%this.tool[%i].getID();
			if(%tool==%item.getID())
			{
				%this.tool[%i]=0;
				messageClient(%this.client,'MsgItemPickup','',%i,0);
				if(%this.currTool==%i)
				{
					%this.updateArm(0);
					%this.unMountImage(0);
				}
			}
		}
	}
}

function Player::removeSlot(%this, %slot) {
	%this.tool[%slot] = 0;
	messageClient(%this.client, 'MsgItemPickup', '', %slot, 0);

	if ( %this.currTool == %slot ) {
		%this.updateArm(0);
		%this.unMountImage(0);
	}
}

function Player::addTool(%this, %data, %props)
{
    %data = %data.getID();
    %maxTools = %this.getDataBlock().maxTools;

    for (%i = 0; %i < %maxTools; %i++)
    {
        if (!%this.tool[%i])
            break;

        if (!%data.customPickupMultiple && %this.tool[%i] == %data)
        {
            %props.delete();
            return -1;
        }
    }

    if (%i == %maxTools)
    {
        %props.delete();
        return -1;
    }

    %this.tool[%i] = %data;
    %this.itemProps[%i] = %props;

    if (isObject(%props))
        %props.onOwnerChange(%this);

    if (isObject(%this.client))
    {
        messageClient(%this.client, 'MsgItemPickup', '', %i, %data, true);

        if (%this.currTool == %i)
            serverCmdUseTool(%this.client, %i);
    }

    return %i;
}

function Player::removeTool(%this, %index, %ignoreProps)
{
    %this.tool[%index] = 0;

    if (!%ignoreProps && isObject(%this.itemProps[%index]))
        %this.itemProps[%index].delete();

    if (isObject(%this.client))
        messageClient(%this.client, 'MsgItemPickup', '', %index, 0, true);

    if (%this.currTool == %index)
        %this.unMountImage(0);
}

//TODO: Fix carrying items by holding leftclick still causing collision sounds even if the item is mid-air
function Item::monitorCollisionSounds(%this, %before)
{
	cancel(%this.monitorCollisionSounds);

	%data = %this.getDatablock();
	%now = vectorLen(%this.getVelocity());

	if (%before - %now >= %data.collisionThreshold)
		%data.collisionSFX.play(%this.getPosition());

	%this.monitorCollisionSounds = %this.schedule(50, "monitorCollisionSounds", %now);
}

package DSItemPackage
{
	function ItemData::onAdd(%this, %item)
	{
		Parent::onAdd(%this, %item);
		if (%this.canPickUp !$= "")
			%item.canPickUp = %this.canPickUp;
		if (isObject(%this.collisionSFX))
			%item.monitorCollisionSounds();
	}
	function Item::schedulePop(%this)
	{
		//Parent::schedulePop(%this);
		GameRoundCleanup.add(%this);
	}
};
if ($GameModeArg $= ($DS::Path @ "gamemode.txt"))
	activatePackage("DSItemPackage");
