using System;

[Serializable]
public class AdvancedRWLuaProxySensor
{
    private float _position1;
    private float _position2 = 1f;
    private AdvancedRWSpline _spline;
    private bool _state;

    public AdvancedRWLuaProxySensor(AdvancedRWSpline spline, float posBegin, float posEnd)
    {
        if ((double) posBegin < 0.0)
            posBegin += spline.splineLength;
        if ((double) posEnd <= 0.0 || (double) posEnd <= (double) posBegin)
            posEnd += spline.splineLength;
        _position1 = posBegin;
        _position2 = posEnd;
        _spline = spline;
    }

    public void RegisterProxySensor() => _spline.RegisterProxySensor(this);

    public bool GetStateAndReset()
    {
        int num = _state ? 1 : 0;
        _state = false;
        return num != 0;
    }

    public bool GetState() => _state;

    public void CheckIsTriggered(AdvancedRWCarrier sender, float pos)
    {
        if ((double) pos < (double) _position1 || (double) pos >= (double) _position2)
            return;
        _state = true;
    }
}