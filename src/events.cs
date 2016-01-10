//ZoneBricks stuff
if (!isObject(ZoneSet))
	new SimSet(ZoneSet);

registerOutputEvent(fxDTSBrick, setZoneSoundProof, "bool 0" , 1);

function fxDTSBrick::setZoneSoundProof(%brick,%val,%client)
{
	if(%brick.isZoneBrick == 1)
		%brick.physicalZone.isSoundProof = %val;
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