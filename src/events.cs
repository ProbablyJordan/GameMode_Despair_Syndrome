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