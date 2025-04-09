using HR.Lua;
using System;

[Serializable]
public class AdvancedRWLuaTrigger
{
    private float _position;
    private AdvancedRWSpline _spline;
    private LuaFunction _callback;

    public AdvancedRWLuaTrigger(AdvancedRWSpline spline, float pos, LuaFunction callbackFunction)
    {
        if (pos < 0.0)
            pos += spline.splineLength;
        _position = pos;
        _callback = callbackFunction;
        _spline = spline;
    }

    public void RegisterTrigger() => _spline.RegisterTrigger(this);

    public void CheckIsTriggered(AdvancedRWCarrier sender, float from, float to)
    {
        if (((double) from <= (double) to ? ((double) from >= (double) this._position ? 0 : ((double) to >= (double) this._position ? 1 : 0)) : ((double) from <= (double) this._position ? 0 : ((double) to <= (double) this._position ? 1 : 0))) == 0)
            return;
        LuaAPI.ActivateCallback(this._callback, LuaAPI.GetObjectId((object) sender.transform));
    }
}