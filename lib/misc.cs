function Player::doAfter(%this, %time, %ticks, %prevPosition, %done)
{
	cancel(%this.doAfter);
	if (%ticks $= "")
		%ticks = 1;

	if (%prevPosition !$= "" && %this.getPosition() !$= %prevPosition)
	{
		return -1; //Fail
	}
	if (%done >= %ticks)
	{
		return 1; //Success
	}
	%prevPosition = %this.getPosition();
	%timefraction = mFloor(%time/%ticks);
	%this.schedule(%timefraction, doAfter, %ticks, %prevPosition, %done++);
}

function Player::doAfterTarget(%this, %target, %time, %ticks, %prevPosition, %prevTargetTransform, %done)
{
	cancel(%this.doAfter);
	if (%ticks $= "")
		%ticks = 1;

	if (!isObject(%target) || %prevPosition $= "" || %this.getPosition() !$= %prevPosition || %target.getTransform() !$= %prevTargetTransform)
	{
		return -1; //Fail
	}
	if (%done >= %ticks)
	{
		return 1; //Success
	}
	%prevPosition = %this.getPosition();
	%prevTargetTransform = %target.getTransform();
	%timefraction = mFloor(%time/%ticks);
	%this.schedule(%timefraction, doAfterTarget, %target, %time, %ticks, %prevPosition, %prevTargetTransform, %done++);
}

function mAtan2(%x, %y)
{
	if(!%x && !%y) return 0;
	%a = mAcos(%x / mSqrt(%x*%x + %y*%y));
	return %y >= 0 ? %a : -%a;
}

function ShapeBase::PointAt(%this, %pos)
{
	%thispos = %this.getPosition();
	%delta = VectorSub(%pos, %thispos);
	%dX = getWord(%delta, 0);
	%dY = getWord(%delta, 1);
	%dZ = getWord(%delta, 2);
	%hyp = VectorLen(%dX SPC %dY SPC 0);

	%rotZ = mAtan(%dX, %dY) * -1;
	%rotX = mAtan(%dZ, %hyp);

	%this.setTransform(%thispos SPC eulerRadToMatrix(%rotX SPC 0 SPC %rotZ));
}