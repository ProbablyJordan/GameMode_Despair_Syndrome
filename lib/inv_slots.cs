if($InventorySlotVer > 1) return; //Newer version is already executed
$InventorySlotVer = 1;
package ProperInventorySlotFix
{
	function GameConnection::setControlObject(%this,%obj)
	{
		Parent::setControlObject(%this,%obj);
		if(%obj == %this.player)
			commandToClient(%this,'PlayGui_CreateToolHud',%obj.getDatablock().maxTools);
	}
	function Player::changeDatablock(%this,%data,%client)
	{
		Parent::changeDatablock(%this,%data,%client);
		if(%data != %this.getDatablock())
			commandToClient(%this.client,'PlayGui_CreateToolHud',%data.maxTools);
	}
	function MinigameSO::addMember(%this,%client)
	{
		Parent::addMember(%this,%client);
		if(isObject(%client) && isObject(%obj = %client.player))
			commandToClient(%client,'PlayGui_CreateToolHud',%obj.getDatablock().maxTools);
	}
	function Armor::onNewDatablock(%this,%data)
	{
		Parent::onNewDatablock(%this,%data);
		if(isObject(%this.client))
			commandToClient(%this.client,'PlayGui_CreateToolHud',%data.maxTools);
	}
};
activatePackage(ProperInventorySlotFix);