//ZoneBricks stuff
if (!isObject(ZoneSet))
	new SimSet(ZoneSet);

registerOutputEvent(fxDTSBrick, setZoneSoundProof, "bool 0" , 1);
registerOutputEvent(fxDTSBrick, adjustZoneSize, "list Set 0 Add 1" TAB "vector 40000 -40000 0" , 1);
function fxDTSBrick::setZoneSoundProof(%brick,%val,%client)
{
	if(%brick.isZoneBrick == 1)
		%brick.physicalZone.isSoundProof = %val;
}
function fxDTSBrick::adjustZoneSize(%brick, %type, %vector, %client) //Broken event, do not use. Needs fixing.
{
	if(!isObject(%brick.physicalZone))
		return;

	%x = %type ? getWord(%brick.physicalZone.getScale(), 0) : 0;
	%y = %type ? getWord(%brick.physicalZone.getScale(), 1) : 0;
	%z = %type ? getWord(%brick.physicalZone.getScale(), 2) : 0;
	talk(%x SPC %y SPC %z SPC "/" SPC %vector);
	%brick.physicalZone.setScale(%x + getWord(%vector, 0) SPC %y + getWord(%vector, 1) SPC %z + getWord(%vector, 2));
	talk(%brick.physicalZone.getTransform());
	%brick.physicalZone.setTransform(vectorAdd(getWords(%brick.getWorldBox(), 0, 2), %brick.physicalZone.getScale()) SPC getWords(%brick.physicalZone.getTransform(), 3, 6));
	talk(%brick.physicalZone.getTransform());
}


package DSEventPackage
{
	function fxDTSBrick::setZone(%brick, %dir, %Addition, %vector, %client)
	{
		parent::setZone(%brick, %dir, %Addition, %vector, %client);
		if(isObject(%brick.physicalZone))
			ZoneSet.add(%brick.physicalZone);
	}
	function zoneTrigger::onEnterTrigger(%this,%trigger,%obj)
	{
		if (%obj.getClassName() $= "Player")
		{
			%obj.currentZone = %trigger.triggerBrick.physicalZone;
		}
		parent::onEnterTrigger(%this,%trigger,%obj);
	}
	function zoneTrigger::onLeaveTrigger(%this,%trigger,%obj)
	{
		if (%obj.getClassName() $= "Player")
		{
			%obj.currentZone = "";
		}
		parent::onLeaveTrigger(%this,%trigger,%obj);
	}
};
activatePackage(DSEventPackage);