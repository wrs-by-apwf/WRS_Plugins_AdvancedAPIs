using HR.Lua;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedRWCarrier : MonoBehaviour
{
    public const float FRICTION_DEC = 0.1f;
    public const float COLGROUP_MAXSPEEDDELTA = 0.03f;
    public float InvCOGRadius = 1f;
    public float dampingCoeffX = 1f / 400f;
    public float emptyWeight = 800f;
    public float payload;
    public float totalWeight = 1200f;
    public float weight = 500f;
    public float length = 1.8f;
    public RWCarrier.CarrierSeat[] passengerSeats = Array.Empty<RWCarrier.CarrierSeat>();
    public float[] passengerProbabilities = new float[0];
    public SkierNavigator[] passengers = new SkierNavigator[0];
    public List<AnimatedRWPart> animatedParts = new List<AnimatedRWPart>();
    public List<RWCarrier.AnimatedCarrierPart> animatedCarrierParts = new List<RWCarrier.AnimatedCarrierPart>();
    public Transform rotX;
    public bool invertZOrientation;
    protected AdvancedRWSpline _spline;
    protected float _position;
    protected bool _blockNextGlobalUpdate;
    private float _lastPosition;
    private int _endStopActive;
    protected float _currentSpeed;
    private float _lastSpeed;
    private float _moveDelta;
    private bool _fixedSpeed = true;
    private bool _isActive;
    private float _slopeAngleX;
    private float _lastSlopeX;
    private float _displacementX;
    private float _angularVelX;
    private float _lastDistanceFromCamera;
    private int _initialize = 5;
    public Transform interieurCamTransf;
    public static List<AdvancedRWCarrier> carrierInstances = new List<AdvancedRWCarrier>();

    public float currentMass => emptyWeight + payload;

    public float moveDelta => _moveDelta;

    public bool fixedSpeed => _fixedSpeed;

    public bool isActive => _isActive;

    public void SetSpeed(float speed) => _currentSpeed = speed;

    public float GetSpeed() => _currentSpeed;

    public AdvancedRWSpline GetSpline() => _spline;

    public void TeleportTo(AdvancedRWSpline spline, float position)
    {
        if ((double)position < 0.0)
            position += spline.splineLength;
        SetPosition(spline, position, position, teleport: true);
    }

    public float GetPosition() => _position;

    public void Move(float delta)
    {
        SetPosition(_spline, _position + delta, _lastPosition);
        if ((double)Mathf.Abs(delta) < 0.001)
            return;
        foreach (AnimatedRWPart animatedPart in animatedParts)
        {
            if (animatedPart.axis == 1)
                animatedPart.transform.Rotate(delta * animatedPart.scale, 0.0f, 0.0f, Space.Self);
            else if (animatedPart.axis == 2)
                animatedPart.transform.Rotate(0.0f, delta * animatedPart.scale, 0.0f, Space.Self);
            else
                animatedPart.transform.Rotate(0.0f, 0.0f, delta * animatedPart.scale, Space.Self);
        }
    }

    public void AddAnimatedPart(Transform transf, float diameter = 1f, int axis = 1)
    {
        animatedParts.Add(new AnimatedRWPart(transf, (float)(360.0 / (double)diameter / 3.1415927410125732), axis));
    }

    public void AddCarrierPart(RWCarrier.AnimatedCarrierPart part)
    {
        if (part == null)
            return;
        animatedCarrierParts.Add(part);
    }

    public void BlockNextGlobalUpdate() => _blockNextGlobalUpdate = true;

    public bool GetIsBlocked() => _blockNextGlobalUpdate;

    public static void GlobalLateUpdate()
    {
        float ropewayDt = GameControl.ropewayDt;
        float num = 1f / ropewayDt;
        AdvancedRWSpline.GlobalLateUpdate();
        foreach (AdvancedRWCarrier carrierInstance in carrierInstances)
        {
            if (carrierInstance._blockNextGlobalUpdate)
                carrierInstance._blockNextGlobalUpdate = false;
            else if (carrierInstance.isActive)
            {
                carrierInstance.Move(carrierInstance.moveDelta);
                carrierInstance.MoveDeltaWithCollisionCheck(ropewayDt);
                carrierInstance.SetSpeed(carrierInstance.moveDelta * num);
            }
        }
    }

    public void OnEnable() => _isActive = true;

    public void OnDisable() => _isActive = false;

    public void OnDestroy()
    {
        carrierInstances.Remove(this);
        AdvancedCarrierManager.g_inst?.RemoveCarrier(this);
        if (!((UnityEngine.Object)_spline != (UnityEngine.Object)null))
            return;
        _spline.UnregisterCarrier(this);
    }

    public static AdvancedRWCarrier GetClosestCarrier(
        Vector3 position,
        float range = 5f,
        Func<AdvancedRWCarrier, bool> validate = null)
    {
        float num = range * range;
        AdvancedRWCarrier closestCarrier = (AdvancedRWCarrier)null;
        foreach (AdvancedRWCarrier carrierInstance in carrierInstances)
        {
            float sqrMagnitude = (carrierInstance.transform.position - position).sqrMagnitude;
            if ((double)sqrMagnitude <= (double)num && (validate == null || validate(carrierInstance)))
            {
                num = sqrMagnitude;
                closestCarrier = carrierInstance;
            }
        }

        return closestCarrier;
    }

    public void SetPosition(
        AdvancedRWSpline spl,
        float pos,
        float lastPosition,
        int recursionCounter = 0,
        bool teleport = false)
    {
        if ((UnityEngine.Object)spl == (UnityEngine.Object)_spline && (double)pos == (double)_position)
            return;
        if (recursionCounter > 15)
        {
            LuaAPI.WriteToLog("Warning in AdvancedRWCarrier.SetPosition (C#): Recursion count " + (object)recursionCounter + " reached on a spline position of " + pos.ToString());
        }
        else
        {
            _position = pos;
            if ((UnityEngine.Object)spl != (UnityEngine.Object)_spline)
            {
                if ((UnityEngine.Object)_spline != (UnityEngine.Object)null)
                    _spline.UnregisterCarrier(this);
                _spline = spl;
                if ((UnityEngine.Object)_spline != (UnityEngine.Object)null)
                    _spline.RegisterCarrier(this);
            }

            if ((UnityEngine.Object)_spline == (UnityEngine.Object)null || (double)lastPosition == (double)_position)
                return;
            _endStopActive = 0;
            if ((double)_position < 0.0)
            {
                AdvancedRWSpline prev = _spline.GetPrev();
                if ((UnityEngine.Object)prev != (UnityEngine.Object)null)
                {
                    float pos1 = _position + prev.splineLength;
                    if (!teleport)
                    {
                        _spline.CheckTriggers(this, lastPosition, 0.0f);
                        UpdateSplinePositionCarrierParts(_spline, lastPosition, 0.0f);
                    }

                    SetPosition(prev, pos1, prev.splineLength, recursionCounter + 1, teleport);
                    return;
                }

                float num = 0.0f;
                AdvancedRWSpline spline = _spline;
                _endStopActive = -1;
                if (!teleport)
                {
                    _spline.CheckTriggers(this, lastPosition, num);
                    UpdateSplinePositionCarrierParts(_spline, lastPosition, num);
                }

                _moveDelta = Mathf.Min(num - _position, 0.0f);
                _position = num;
                if ((double)_currentSpeed < 0.0)
                    SetSpeed(0.0f);
            }
            else if ((double)_position > (double)_spline.splineLength)
            {
                AdvancedRWSpline next = _spline.GetNext();
                if ((UnityEngine.Object)next != (UnityEngine.Object)null)
                {
                    float pos2 = _position - _spline.splineLength;
                    if (!teleport)
                    {
                        _spline.CheckTriggers(this, lastPosition, _spline.splineLength);
                        UpdateSplinePositionCarrierParts(_spline, lastPosition, _spline.splineLength);
                    }

                    SetPosition(next, pos2, 0.0f, recursionCounter + 1, teleport);
                    return;
                }

                float splineLength = _spline.splineLength;
                AdvancedRWSpline spline = _spline;
                _endStopActive = 1;
                if (!teleport)
                {
                    _spline.CheckTriggers(this, lastPosition, splineLength);
                    UpdateSplinePositionCarrierParts(_spline, lastPosition, splineLength);
                }

                _moveDelta = Mathf.Max(splineLength - _position, 0.0f);
                _position = splineLength;
                if ((double)_currentSpeed <= 0.0)
                    return;
                SetSpeed(0.0f);
                return;
            }

            if (!teleport)
            {
                _spline.CheckTriggers(this, lastPosition, _position);
                UpdateSplinePositionCarrierParts(_spline, lastPosition, _position);
            }

            _lastPosition = _position;
        }
    }

    public void SetInitialPosition(AdvancedRWSpline spl, float pos)
    {
        _spline = spl;
        _position = pos;
        _lastPosition = pos;
        if ((UnityEngine.Object)_spline != (UnityEngine.Object)null)
            _spline.RegisterCarrier(this);
        UpdateSplinePositionCarrierParts(_spline, _lastPosition, _position);
    }

    private void Awake() => AdvancedCarrierManager.g_inst?.AddCarrier(this);

    public void UpdateCarrier()
    {
        float ropewayDt = GameControl.ropewayDt;
        float ropewayNonscaledDt = GameControl.ropewayNonscaledDt;
        if ((UnityEngine.Object)_spline != (UnityEngine.Object)null)
        {
            Vector3 point = _spline.GetPoint(_position);
            Vector3 direction = _spline.GetDirection(_position, true);
            if (invertZOrientation)
                direction *= -1f;
            Quaternion quaternion1 = Quaternion.LookRotation(direction, Vector3.up);
            _lastSlopeX = _slopeAngleX;
            _slopeAngleX = quaternion1.eulerAngles.x * ((float)Math.PI / 180f);
            Quaternion quaternion2 = Quaternion.Euler((float)(((double)_displacementX * (invertZOrientation ? -1.0 : 1.0) - (double)_slopeAngleX) * 57.295780181884766), 0.0f, 0.0f);
            transform.localPosition = point;
            transform.localRotation = quaternion1;
            rotX.localRotation = quaternion2;
            AdvancedRWDrvAssgn AdvancedRWDrvAssgn = _spline.GetRwDrvAssgn(_position);
            _fixedSpeed = false;
            if (AdvancedRWDrvAssgn != null && AdvancedRWDrvAssgn.engaged)
            {
                float speedAt = AdvancedRWDrvAssgn.GetSpeedAt(_position);
                if (_endStopActive > 0 && (double)speedAt > 0.0)
                    _fixedSpeed = true;
                else if (_endStopActive < 0 && (double)speedAt < 0.0)
                {
                    _fixedSpeed = true;
                }
                else
                {
                    _currentSpeed = AdvancedRWDrvAssgn.GetSpeedAt(_position);
                    _fixedSpeed = true;
                }
            }
            else
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0.0f, 0.1f * ropewayDt);
        }
        else
            _currentSpeed = 0.0f;

        float dv = 0.0f;
        if (_initialize > 0)
            --_initialize;
        else
            dv = Mathf.Clamp(_currentSpeed - _lastSpeed, -3f, 3f) / GameControl.ropewayTimescale;
        SimulateAngularSwinging(dv, ropewayNonscaledDt);
        _lastSpeed = _currentSpeed;
        /*UpdateSpawnDespawn();*/
        UpdateCarrierParts();
    }

    private void SimulateAngularSwinging(float dv, float totalDt)
    {
        if ((double)totalDt <= 0.0)
            return;
        float a = 0.0333333351f;
        float f = _displacementX - _lastSlopeX;
        float num1 = 0.0f;
        while (true)
        {
            float num2 = Mathf.Min(a, totalDt - num1);
            if ((double)num2 > 0.0)
            {
                num1 += num2;
                float num3 = (float)((double)dv * (double)num2 / (double)totalDt * (double)Mathf.Cos(f) - 9.8100004196167 * (double)num2 * (double)Mathf.Sin(_displacementX)) * InvCOGRadius;
                _angularVelX *= Mathf.Pow(1f - dampingCoeffX, 30f * num2 * GameControl.ropewayTimescale);
                _angularVelX += num3;
                _displacementX += _angularVelX * num2;
            }
            else
                break;
        }
    }

    /*private void UpdateSpawnDespawn()
    {
        if (!SkierSpawner.skiersEnabled)
        {
            DespawnPassengers();
        }
        else
        {
            float distanceFromCamera = GameControl.g_inst.GetSqrDistanceFromCamera(transform.position);
            bool flag1 = distanceFromCamera <= SkierSpawner.spawnDistanceSqr;
            bool flag2 = _lastDistanceFromCamera <= SkierSpawner.spawnDistanceSqr;
            _lastDistanceFromCamera = distanceFromCamera;
            if (flag1 == flag2)
                return;
            if (flag1)
            {
                for (int seatId = 0; seatId < passengerSeats.Length; ++seatId)
                {
                    if (!(bool)(UnityEngine.Object)passengers[seatId] && (bool)(UnityEngine.Object)passengerSeats[seatId].seat && (double)passengerProbabilities[seatId] >= 0.5)
                    {
                        SkierNavigator skierNavigator = SkierSpawner.g_inst.SpawnNewSkier(0.5f);
                        if ((bool)(UnityEngine.Object)skierNavigator)
                        {
                            skierNavigator.EnterCarrier(morphPassengerLeftToRWCarrier(this), passengerSeats[seatId], seatId);
                            passengers[seatId] = skierNavigator;
                        }
                    }
                }
            }
            else
            {
                DespawnPassengers();
            }
        }
    }*/

    public void SetCarrierPartPosition(string channel, float position, int moveMode = 0)
    {
        foreach (RWCarrier.AnimatedCarrierPart animatedCarrierPart in animatedCarrierParts)
        {
            if (animatedCarrierPart != null && animatedCarrierPart.name == channel)
                animatedCarrierPart.SetPosition(position, moveMode);
        }
    }

    public RWCarrier.AnimatedCarrierPart GetCarrierPart(string channel)
    {
        foreach (RWCarrier.AnimatedCarrierPart animatedCarrierPart in animatedCarrierParts)
        {
            if (animatedCarrierPart != null && animatedCarrierPart.name == channel)
                return animatedCarrierPart;
        }

        return (RWCarrier.AnimatedCarrierPart)null;
    }

    private void UpdateCarrierParts()
    {
        foreach (RWCarrier.AnimatedCarrierPart animatedCarrierPart in animatedCarrierParts)
        {
            if ((double)animatedCarrierPart.targetPosition != (double)animatedCarrierPart.position)
            {
                animatedCarrierPart.position = Mathf.MoveTowards(animatedCarrierPart.position, animatedCarrierPart.targetPosition, Time.deltaTime / animatedCarrierPart.animation.clip.length);
                animatedCarrierPart.UpdateAnimation();
            }
        }
    }

    private void UpdateSplinePositionCarrierParts(
        AdvancedRWSpline spline,
        float lastPosition,
        float newPosition)
    {
        if ((UnityEngine.Object)_spline == (UnityEngine.Object)null || _spline.Rails.Length == 0)
            return;
        bool flag = (double)newPosition > (double)lastPosition;
        foreach (AdvancedRWSpline.AnimatedPartRail rail in spline.Rails)
        {
            if (rail != null && (flag
                    ? ((double)newPosition < (double)rail.position0 ? 1 : ((double)lastPosition > (double)rail.position1 ? 1 : 0))
                    : ((double)newPosition > (double)rail.position1 ? 1 : ((double)lastPosition < (double)rail.position0 ? 1 : 0))) == 0)
            {
                foreach (RWCarrier.AnimatedCarrierPart animatedCarrierPart in animatedCarrierParts)
                {
                    if (animatedCarrierPart != null && !(animatedCarrierPart.name != rail.channel) && animatedCarrierPart != null)
                    {
                        float newPosition1 = !flag || (double)newPosition < (double)rail.position1
                            ? (flag || (double)newPosition > (double)rail.position0 ? Mathf.Lerp(rail.value0, rail.value1, Mathf.InverseLerp(rail.position0, rail.position1, newPosition)) : rail.value0)
                            : rail.value1;
                        animatedCarrierPart.SetPosition(newPosition1, rail.type);
                    }
                }
            }
        }
    }

    public void DespawnPassengers(bool remove = false)
    {
        for (int index = 0; index < passengerSeats.Length; ++index)
        {
            if ((bool)(UnityEngine.Object)passengers[index])
            {
                passengers[index].Despawn();
                passengers[index] = (SkierNavigator)null;
                passengerProbabilities[index] = 1f;
            }

            if (remove)
                passengerProbabilities[index] = 0.0f;
        }
    }

    public float GetPassengerCount()
    {
        float passengerCount = 0.0f;
        for (int index = 0; index < passengerSeats.Length; ++index)
        {
            if ((bool)(UnityEngine.Object)passengers[index])
                ++passengerCount;
            else
                passengerCount += passengerProbabilities[index];
        }

        return passengerCount;
    }

    public float FindCollisionPartner(bool direction, out AdvancedRWCarrier carrier)
    {
        float collisionPartner = 0.01f;
        AdvancedRWCarrier AdvancedRWCarrier = this;
        if (direction)
        {
            foreach (AdvancedRWCarrier carrier1 in _spline.carriers)
            {
                if (!((UnityEngine.Object)carrier1 == (UnityEngine.Object)this) && (double)carrier1.GetPosition() > (double)_position && (double)GetSpeed() >= (double)carrier1.GetSpeed())
                {
                    float num = (float)((double)carrier1.GetPosition() - (double)_position - 0.5 * ((double)length + (double)carrier1.length));
                    if ((double)num <= (double)collisionPartner)
                    {
                        collisionPartner = num;
                        AdvancedRWCarrier = carrier1;
                    }
                }
            }

            if ((UnityEngine.Object)AdvancedRWCarrier == (UnityEngine.Object)this)
            {
                AdvancedRWSpline next = _spline.GetNext();
                if ((UnityEngine.Object)next != (UnityEngine.Object)null)
                {
                    foreach (AdvancedRWCarrier carrier2 in next.carriers)
                    {
                        if (!((UnityEngine.Object)carrier2 == (UnityEngine.Object)this) && (double)GetSpeed() >= (double)carrier2.GetSpeed())
                        {
                            float num = (float)((double)carrier2.GetPosition() + (double)_spline.splineLength - (double)_position - 0.5 * ((double)length + (double)carrier2.length));
                            if ((double)num <= (double)collisionPartner)
                            {
                                collisionPartner = num;
                                AdvancedRWCarrier = carrier2;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            foreach (AdvancedRWCarrier carrier3 in _spline.carriers)
            {
                if (!((UnityEngine.Object)carrier3 == (UnityEngine.Object)this) && (double)carrier3.GetPosition() < (double)_position && (double)GetSpeed() <= (double)carrier3.GetSpeed())
                {
                    float num = (float)((double)_position - (double)carrier3.GetPosition() - 0.5 * ((double)length + (double)carrier3.length));
                    if ((double)num <= (double)collisionPartner)
                    {
                        collisionPartner = num;
                        AdvancedRWCarrier = carrier3;
                    }
                }
            }

            if ((UnityEngine.Object)AdvancedRWCarrier == (UnityEngine.Object)this)
            {
                AdvancedRWSpline prev = _spline.GetPrev();
                if ((UnityEngine.Object)prev != (UnityEngine.Object)null)
                {
                    foreach (AdvancedRWCarrier carrier4 in prev.carriers)
                    {
                        if (!((UnityEngine.Object)carrier4 == (UnityEngine.Object)this) && (double)GetSpeed() <= (double)carrier4.GetSpeed())
                        {
                            float num = (float)((double)_position - (double)carrier4.GetPosition() + (double)prev.splineLength - 0.5 * ((double)length + (double)carrier4.length));
                            if ((double)num <= (double)collisionPartner)
                            {
                                collisionPartner = num;
                                AdvancedRWCarrier = carrier4;
                            }
                        }
                    }
                }
            }
        }

        carrier = (UnityEngine.Object)AdvancedRWCarrier == (UnityEngine.Object)this ? (AdvancedRWCarrier)null : AdvancedRWCarrier;
        return collisionPartner;
    }

    public bool FindCollisionPartner2(float dt, bool direction, out AdvancedRWCarrier carrier)
    {
        float num1 = 0.01f;
        AdvancedRWCarrier AdvancedRWCarrier = this;
        float num2 = _position + _currentSpeed * dt;
        bool flag = false;
        foreach (AdvancedRWCarrier carrier1 in _spline.carriers)
        {
            if (!((UnityEngine.Object)carrier1 == (UnityEngine.Object)this))
            {
                float num3 = carrier1._position + carrier1._currentSpeed * dt;
                if ((direction ? ((double)num3 <= (double)num2 ? 1 : 0) : ((double)num3 >= (double)num2 ? 1 : 0)) == 0)
                {
                    float num4 = Mathf.Abs(num3 - num2) - (float)(0.5 * ((double)length + (double)carrier1.length));
                    if ((double)num4 <= (double)num1)
                    {
                        num1 = num4;
                        AdvancedRWCarrier = carrier1;
                        flag = (double)Mathf.Abs(_currentSpeed - AdvancedRWCarrier._currentSpeed) <= 0.029999999329447746 && AdvancedRWCarrier.fixedSpeed == fixedSpeed;
                    }
                }
            }
        }

        if ((UnityEngine.Object)AdvancedRWCarrier == (UnityEngine.Object)this)
        {
            AdvancedRWSpline AdvancedRWSpline = direction ? _spline.GetNext() : _spline.GetPrev();
            if ((UnityEngine.Object)AdvancedRWSpline != (UnityEngine.Object)null)
            {
                float num5 = direction ? _spline.splineLength : -AdvancedRWSpline.splineLength;
                foreach (AdvancedRWCarrier carrier2 in AdvancedRWSpline.carriers)
                {
                    if (!((UnityEngine.Object)carrier2 == (UnityEngine.Object)this))
                    {
                        float num6 = Mathf.Abs(carrier2._position + carrier2._currentSpeed * dt + num5 - num2) - (float)(0.5 * ((double)length + (double)carrier2.length));
                        if ((double)num6 <= (double)num1)
                        {
                            num1 = num6;
                            AdvancedRWCarrier = carrier2;
                            flag = (double)Mathf.Abs(_currentSpeed - AdvancedRWCarrier._currentSpeed) <= 0.029999999329447746 && AdvancedRWCarrier.fixedSpeed == fixedSpeed;
                        }
                    }
                }
            }
        }

        carrier = (UnityEngine.Object)AdvancedRWCarrier == (UnityEngine.Object)this ? (AdvancedRWCarrier)null : AdvancedRWCarrier;
        return !((UnityEngine.Object)carrier == (UnityEngine.Object)null) && flag;
    }

    public bool CheckEnableGravity(bool direction)
    {
        float num = direction ? 0.2f : -0.2f;
        AdvancedRWDrvAssgn rwDrvAssgn1 = _spline.GetRwDrvAssgn(_position);
        AdvancedRWDrvAssgn rwDrvAssgn2 = (AdvancedRWDrvAssgn)null;
        if (direction && (double)_position + (double)num > (double)_spline.splineLength)
        {
            AdvancedRWSpline next = _spline.GetNext();
            if ((UnityEngine.Object)next != (UnityEngine.Object)null)
                rwDrvAssgn2 = next.GetRwDrvAssgn(_position + num - _spline.splineLength);
        }
        else if (!direction && (double)_position + (double)num < 0.0)
        {
            AdvancedRWSpline prev = _spline.GetPrev();
            if ((UnityEngine.Object)prev != (UnityEngine.Object)null)
                rwDrvAssgn2 = prev.GetRwDrvAssgn(_position + num + prev.splineLength);
        }
        else
            rwDrvAssgn2 = _spline.GetRwDrvAssgn(_position + num);

        if (rwDrvAssgn2 == null || rwDrvAssgn1 == rwDrvAssgn2)
            return true;
        float speedAt = rwDrvAssgn2.GetSpeedAt(_position + num);
        if ((double)num > 0.0 && (double)speedAt >= 0.0)
            return true;
        return (double)num < 0.0 && (double)speedAt <= 0.0;
    }

    public void MoveDeltaWithCollisionCheck(float dt)
    {
        _moveDelta = _currentSpeed * dt;
        if ((double)_currentSpeed == 0.0)
            return;
        AdvancedRWCarrier carrier;
        float collisionPartner = FindCollisionPartner((double)_currentSpeed > 0.0, out carrier);
        if ((UnityEngine.Object)carrier == (UnityEngine.Object)null || (double)collisionPartner > (double)Mathf.Abs(moveDelta))
            return;
        _moveDelta = collisionPartner * Mathf.Sign(_currentSpeed);
    }

    /*public bool PickupPassengerIfPossible(SkierNavigator skier, int seatId)
    {
        if (seatId >= passengers.Length || seatId >= passengerSeats.Length || !((UnityEngine.Object)passengers[seatId] == (UnityEngine.Object)null) || !((UnityEngine.Object)passengerSeats[seatId].seat != (UnityEngine.Object)null))
            return false;
        skier.EnterCarrier(this, passengerSeats[seatId], seatId);
        passengers[seatId] = skier;
        passengerProbabilities[seatId] = 1f;
        return true;
    }*/

    public void PassengerLeft(int seatId)
    {
        if (seatId >= passengers.Length)
            return;
        passengers[seatId] = (SkierNavigator)null;
        passengerProbabilities[seatId] = 0.0f;
    }

    private static Vector3 BezierPathCalculation(
        Vector3 p0,
        Vector3 p1,
        Vector3 p2,
        Vector3 p3,
        float t)
    {
        float num1 = t * t;
        float num2 = t * num1;
        float num3 = 1f - t;
        float num4 = num3 * num3;
        return num3 * num4 * p0 + 3f * num4 * t * p1 + 3f * num3 * num1 * p2 + num2 * p3;
    }
}