function servercmdInvis(%cl)
{
	if (!%cl.inDefaultGame() && isObject(%pl = %cl.player))
	{
		%pl.hideNode("ALL");
		%pl.setShapeNameDistance(0);
		//for (%i = 0; %i < %pl.getDatablock().maxTools; %i++)
		//{
		//	if (isObject(%item = %pl.tool[%i]) && isObject(%invItem = %item.invisItem))
		//	{
		//		%pl.tool[%i] = %invItem.getID();
		//		messageClient(%cl, 'MsgItemPickup', '', %i, %invItem.getID(), 1);
		//	}
		//}
	}
}