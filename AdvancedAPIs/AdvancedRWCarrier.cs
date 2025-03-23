

/*
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
  public CarrierSeat[] passengerSeats = new CarrierSeat[0];
  public float[] passengerProbabilities = new float[0];
  public SkierNavigator[] passengers = new SkierNavigator[0];
  public List<AnimatedRWPart> animatedParts = new List<AnimatedRWPart>();
  public List<AnimatedCarrierPart> animatedCarrierParts = new List<AnimatedCarrierPart>();
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

  public float currentMass => this.emptyWeight + this.payload;

  public float moveDelta => this._moveDelta;

  public bool fixedSpeed => this._fixedSpeed;

  public bool isActive => this._isActive;

  public void SetSpeed(float speed) => this._currentSpeed = speed;

  public float GetSpeed() => this._currentSpeed;

  public AdvancedRWSpline GetSpline() => this._spline;

  public void TeleportTo(AdvancedRWSpline spline, float position)
  {
    if ((double) position < 0.0)
      position += spline.splineLength;
    this.SetPosition(spline, position, position, teleport: true);
  }

  public float GetPosition() => this._position;

  public void Move(float delta)
  {
    this.SetPosition(this._spline, this._position + delta, this._lastPosition);
    if ((double) Mathf.Abs(delta) < 0.001)
      return;
    foreach (AnimatedRWPart animatedPart in this.animatedParts)
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
    this.animatedParts.Add(new AnimatedRWPart(transf, (float) (360.0 / (double) diameter / 3.1415927410125732), axis));
  }

  public void AddCarrierPart(AdvancedRWCarrier.AnimatedCarrierPart part)
  {
    if (part == null)
      return;
    this.animatedCarrierParts.Add(part);
  }

  public void BlockNextGlobalUpdate() => this._blockNextGlobalUpdate = true;

  public bool GetIsBlocked() => this._blockNextGlobalUpdate;

  public static void GlobalLateUpdate()
  {
    float ropewayDt = GameControl.ropewayDt;
    float num = 1f / ropewayDt;
    AdvancedRWSpline.GlobalLateUpdate();
    foreach (AdvancedRWCarrier carrierInstance in AdvancedRWCarrier.carrierInstances)
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

  public void OnEnable() => this._isActive = true;

  public void OnDisable() => this._isActive = false;

  public void OnDestroy()
  {
    AdvancedRWCarrier.carrierInstances.Remove(this);
    CarrierManager.g_inst?.RemoveCarrier(this);
    if (!((UnityEngine.Object) this._spline != (UnityEngine.Object) null))
      return;
    _spline.UnregisterCarrier(this);
  }

  public static AdvancedRWCarrier GetClosestCarrier(
    Vector3 position,
    float range = 5f,
    Func<AdvancedRWCarrier, bool> validate = null)
  {
    float num = range * range;
    AdvancedRWCarrier closestCarrier = (AdvancedRWCarrier) null;
    foreach (AdvancedRWCarrier carrierInstance in carrierInstances)
    {
      float sqrMagnitude = (carrierInstance.transform.position - position).sqrMagnitude;
      if ((double) sqrMagnitude <= (double) num && (validate == null || validate(carrierInstance)))
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
    if ((UnityEngine.Object) spl == (UnityEngine.Object) this._spline && (double) pos == (double) this._position)
      return;
    if (recursionCounter > 15)
    {
      LuaAPI.WriteToLog("Warning in AdvancedRWCarrier.SetPosition (C#): Recursion count " + (object) recursionCounter + " reached on a spline position of " + pos.ToString());
    }
    else
    {
      this._position = pos;
      if ((UnityEngine.Object) spl != (UnityEngine.Object) this._spline)
      {
        if ((UnityEngine.Object) this._spline != (UnityEngine.Object) null)
          this._spline.UnregisterCarrier(this);
        this._spline = spl;
        if ((UnityEngine.Object) this._spline != (UnityEngine.Object) null)
          this._spline.RegisterCarrier(this);
      }
      if ((UnityEngine.Object) this._spline == (UnityEngine.Object) null || (double) lastPosition == (double) this._position)
        return;
      this._endStopActive = 0;
      if ((double) this._position < 0.0)
      {
        AdvancedRWSpline prev = this._spline.GetPrev();
        if ((UnityEngine.Object) prev != (UnityEngine.Object) null)
        {
          float pos1 = this._position + prev.splineLength;
          if (!teleport)
          {
            this._spline.CheckTriggers(this, lastPosition, 0.0f);
            this.UpdateSplinePositionCarrierParts(this._spline, lastPosition, 0.0f);
          }
          this.SetPosition(prev, pos1, prev.splineLength, recursionCounter + 1, teleport);
          return;
        }
        float num = 0.0f;
        AdvancedRWSpline spline = this._spline;
        this._endStopActive = -1;
        if (!teleport)
        {
          this._spline.CheckTriggers(this, lastPosition, num);
          this.UpdateSplinePositionCarrierParts(this._spline, lastPosition, num);
        }
        this._moveDelta = Mathf.Min(num - this._position, 0.0f);
        this._position = num;
        if ((double) this._currentSpeed < 0.0)
          this.SetSpeed(0.0f);
      }
      else if ((double) this._position > (double) this._spline.splineLength)
      {
        AdvancedRWSpline next = this._spline.GetNext();
        if ((UnityEngine.Object) next != (UnityEngine.Object) null)
        {
          float pos2 = this._position - this._spline.splineLength;
          if (!teleport)
          {
            this._spline.CheckTriggers(this, lastPosition, this._spline.splineLength);
            this.UpdateSplinePositionCarrierParts(this._spline, lastPosition, this._spline.splineLength);
          }
          this.SetPosition(next, pos2, 0.0f, recursionCounter + 1, teleport);
          return;
        }
        float splineLength = this._spline.splineLength;
        AdvancedRWSpline spline = this._spline;
        this._endStopActive = 1;
        if (!teleport)
        {
          this._spline.CheckTriggers(this, lastPosition, splineLength);
          this.UpdateSplinePositionCarrierParts(this._spline, lastPosition, splineLength);
        }
        this._moveDelta = Mathf.Max(splineLength - this._position, 0.0f);
        this._position = splineLength;
        if ((double) this._currentSpeed <= 0.0)
          return;
        this.SetSpeed(0.0f);
        return;
      }
      if (!teleport)
      {
        this._spline.CheckTriggers(this, lastPosition, this._position);
        this.UpdateSplinePositionCarrierParts(this._spline, lastPosition, this._position);
      }
      this._lastPosition = this._position;
    }
  }

  public void SetInitialPosition(AdvancedRWSpline spl, float pos)
  {
    this._spline = spl;
    this._position = pos;
    this._lastPosition = pos;
    if ((UnityEngine.Object) this._spline != (UnityEngine.Object) null)
      this._spline.RegisterCarrier(this);
    this.UpdateSplinePositionCarrierParts(this._spline, this._lastPosition, this._position);
  }

  private void Awake() => CarrierManager.g_inst?.AddCarrier(this);

  public void UpdateCarrier()
  {
    float ropewayDt = GameControl.ropewayDt;
    float ropewayNonscaledDt = GameControl.ropewayNonscaledDt;
    if ((UnityEngine.Object) this._spline != (UnityEngine.Object) null)
    {
      Vector3 point = this._spline.GetPoint(this._position);
      Vector3 direction = this._spline.GetDirection(this._position, true);
      if (this.invertZOrientation)
        direction *= -1f;
      Quaternion quaternion1 = Quaternion.LookRotation(direction, Vector3.up);
      this._lastSlopeX = this._slopeAngleX;
      this._slopeAngleX = quaternion1.eulerAngles.x * ((float) Math.PI / 180f);
      Quaternion quaternion2 = Quaternion.Euler((float) (((double) this._displacementX * (this.invertZOrientation ? -1.0 : 1.0) - (double) this._slopeAngleX) * 57.295780181884766), 0.0f, 0.0f);
      this.transform.localPosition = point;
      this.transform.localRotation = quaternion1;
      this.rotX.localRotation = quaternion2;
      RWDrvAssgn rwDrvAssgn = this._spline.GetRwDrvAssgn(this._position);
      this._fixedSpeed = false;
      if (rwDrvAssgn != null && rwDrvAssgn.engaged)
      {
        float speedAt = rwDrvAssgn.GetSpeedAt(this._position);
        if (this._endStopActive > 0 && (double) speedAt > 0.0)
          this._fixedSpeed = true;
        else if (this._endStopActive < 0 && (double) speedAt < 0.0)
        {
          this._fixedSpeed = true;
        }
        else
        {
          this._currentSpeed = rwDrvAssgn.GetSpeedAt(this._position);
          this._fixedSpeed = true;
        }
      }
      else
        this._currentSpeed = Mathf.MoveTowards(this._currentSpeed, 0.0f, 0.1f * ropewayDt);
    }
    else
      this._currentSpeed = 0.0f;
    float dv = 0.0f;
    if (this._initialize > 0)
      --this._initialize;
    else
      dv = Mathf.Clamp(this._currentSpeed - this._lastSpeed, -3f, 3f) / GameControl.ropewayTimescale;
    this.SimulateAngularSwinging(dv, ropewayNonscaledDt);
    this._lastSpeed = this._currentSpeed;
    this.UpdateSpawnDespawn();
    this.UpdateCarrierParts();
  }

  private void SimulateAngularSwinging(float dv, float totalDt)
  {
    if ((double) totalDt <= 0.0)
      return;
    float a = 0.0333333351f;
    float f = this._displacementX - this._lastSlopeX;
    float num1 = 0.0f;
    while (true)
    {
      float num2 = Mathf.Min(a, totalDt - num1);
      if ((double) num2 > 0.0)
      {
        num1 += num2;
        float num3 = (float) ((double) dv * (double) num2 / (double) totalDt * (double) Mathf.Cos(f) - 9.8100004196167 * (double) num2 * (double) Mathf.Sin(this._displacementX)) * this.InvCOGRadius;
        this._angularVelX *= Mathf.Pow(1f - this.dampingCoeffX, 30f * num2 * GameControl.ropewayTimescale);
        this._angularVelX += num3;
        this._displacementX += this._angularVelX * num2;
      }
      else
        break;
    }
  }

  private void UpdateSpawnDespawn()
  {
    if (!SkierSpawner.skiersEnabled)
    {
      this.DespawnPassengers();
    }
    else
    {
      float distanceFromCamera = GameControl.g_inst.GetSqrDistanceFromCamera(this.transform.position);
      bool flag1 = (double) distanceFromCamera <= (double) SkierSpawner.spawnDistanceSqr;
      bool flag2 = (double) this._lastDistanceFromCamera <= (double) SkierSpawner.spawnDistanceSqr;
      this._lastDistanceFromCamera = distanceFromCamera;
      if (flag1 == flag2)
        return;
      if (flag1)
      {
        for (int seatId = 0; seatId < this.passengerSeats.Length; ++seatId)
        {
          if (!(bool) (UnityEngine.Object) this.passengers[seatId] && (bool) (UnityEngine.Object) this.passengerSeats[seatId].seat && (double) this.passengerProbabilities[seatId] >= 0.5)
          {
            SkierNavigator skierNavigator = SkierSpawner.g_inst.SpawnNewSkier(0.5f);
            if ((bool) (UnityEngine.Object) skierNavigator)
            {
              skierNavigator.EnterCarrier(this, this.passengerSeats[seatId], seatId);
              this.passengers[seatId] = skierNavigator;
            }
          }
        }
      }
      else
        this.DespawnPassengers();
    }
  }

  public void SetCarrierPartPosition(string channel, float position, int moveMode = 0)
  {
    foreach (AdvancedRWCarrier.AnimatedCarrierPart animatedCarrierPart in this.animatedCarrierParts)
    {
      if (animatedCarrierPart != null && animatedCarrierPart.name == channel)
        animatedCarrierPart.SetPosition(position, moveMode);
    }
  }

  public AdvancedRWCarrier.AnimatedCarrierPart GetCarrierPart(string channel)
  {
    foreach (AdvancedRWCarrier.AnimatedCarrierPart animatedCarrierPart in this.animatedCarrierParts)
    {
      if (animatedCarrierPart != null && animatedCarrierPart.name == channel)
        return animatedCarrierPart;
    }
    return (AdvancedRWCarrier.AnimatedCarrierPart) null;
  }

  private void UpdateCarrierParts()
  {
    foreach (AdvancedRWCarrier.AnimatedCarrierPart animatedCarrierPart in this.animatedCarrierParts)
    {
      if ((double) animatedCarrierPart.targetPosition != (double) animatedCarrierPart.position)
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
    if ((UnityEngine.Object) this._spline == (UnityEngine.Object) null || this._spline.rails.Length == 0)
      return;
    bool flag = (double) newPosition > (double) lastPosition;
    foreach (AdvancedRWSpline.AnimatedPartRail rail in spline.rails)
    {
      if (rail != null && (flag ? ((double) newPosition < (double) rail.position0 ? 1 : ((double) lastPosition > (double) rail.position1 ? 1 : 0)) : ((double) newPosition > (double) rail.position1 ? 1 : ((double) lastPosition < (double) rail.position0 ? 1 : 0))) == 0)
      {
        foreach (AdvancedRWCarrier.AnimatedCarrierPart animatedCarrierPart in this.animatedCarrierParts)
        {
          if (animatedCarrierPart != null && !(animatedCarrierPart.name != rail.channel) && animatedCarrierPart != null)
          {
            float newPosition1 = !flag || (double) newPosition < (double) rail.position1 ? (flag || (double) newPosition > (double) rail.position0 ? Mathf.Lerp(rail.value0, rail.value1, Mathf.InverseLerp(rail.position0, rail.position1, newPosition)) : rail.value0) : rail.value1;
            animatedCarrierPart.SetPosition(newPosition1, rail.type);
          }
        }
      }
    }
  }

  public void DespawnPassengers(bool remove = false)
  {
    for (int index = 0; index < this.passengerSeats.Length; ++index)
    {
      if ((bool) (UnityEngine.Object) this.passengers[index])
      {
        this.passengers[index].Despawn();
        this.passengers[index] = (SkierNavigator) null;
        this.passengerProbabilities[index] = 1f;
      }
      if (remove)
        this.passengerProbabilities[index] = 0.0f;
    }
  }

  public float GetPassengerCount()
  {
    float passengerCount = 0.0f;
    for (int index = 0; index < this.passengerSeats.Length; ++index)
    {
      if ((bool) (UnityEngine.Object) this.passengers[index])
        ++passengerCount;
      else
        passengerCount += this.passengerProbabilities[index];
    }
    return passengerCount;
  }

  public float FindCollisionPartner(bool direction, out AdvancedRWCarrier carrier)
  {
    float collisionPartner = 0.01f;
    AdvancedRWCarrier AdvancedRWCarrier = this;
    if (direction)
    {
      foreach (AdvancedRWCarrier carrier1 in this._spline.carriers)
      {
        if (!((UnityEngine.Object) carrier1 == (UnityEngine.Object) this) && (double) carrier1.GetPosition() > (double) this._position && (double) this.GetSpeed() >= (double) carrier1.GetSpeed())
        {
          float num = (float) ((double) carrier1.GetPosition() - (double) this._position - 0.5 * ((double) this.length + (double) carrier1.length));
          if ((double) num <= (double) collisionPartner)
          {
            collisionPartner = num;
            AdvancedRWCarrier = carrier1;
          }
        }
      }
      if ((UnityEngine.Object) AdvancedRWCarrier == (UnityEngine.Object) this)
      {
        AdvancedRWSpline next = this._spline.GetNext();
        if ((UnityEngine.Object) next != (UnityEngine.Object) null)
        {
          foreach (AdvancedRWCarrier carrier2 in next.carriers)
          {
            if (!((UnityEngine.Object) carrier2 == (UnityEngine.Object) this) && (double) this.GetSpeed() >= (double) carrier2.GetSpeed())
            {
              float num = (float) ((double) carrier2.GetPosition() + (double) this._spline.splineLength - (double) this._position - 0.5 * ((double) this.length + (double) carrier2.length));
              if ((double) num <= (double) collisionPartner)
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
      foreach (AdvancedRWCarrier carrier3 in this._spline.carriers)
      {
        if (!((UnityEngine.Object) carrier3 == (UnityEngine.Object) this) && (double) carrier3.GetPosition() < (double) this._position && (double) this.GetSpeed() <= (double) carrier3.GetSpeed())
        {
          float num = (float) ((double) this._position - (double) carrier3.GetPosition() - 0.5 * ((double) this.length + (double) carrier3.length));
          if ((double) num <= (double) collisionPartner)
          {
            collisionPartner = num;
            AdvancedRWCarrier = carrier3;
          }
        }
      }
      if ((UnityEngine.Object) AdvancedRWCarrier == (UnityEngine.Object) this)
      {
        AdvancedRWSpline prev = this._spline.GetPrev();
        if ((UnityEngine.Object) prev != (UnityEngine.Object) null)
        {
          foreach (AdvancedRWCarrier carrier4 in prev.carriers)
          {
            if (!((UnityEngine.Object) carrier4 == (UnityEngine.Object) this) && (double) this.GetSpeed() <= (double) carrier4.GetSpeed())
            {
              float num = (float) ((double) this._position - (double) carrier4.GetPosition() + (double) prev.splineLength - 0.5 * ((double) this.length + (double) carrier4.length));
              if ((double) num <= (double) collisionPartner)
              {
                collisionPartner = num;
                AdvancedRWCarrier = carrier4;
              }
            }
          }
        }
      }
    }
    carrier = (UnityEngine.Object) AdvancedRWCarrier == (UnityEngine.Object) this ? (AdvancedRWCarrier) null : AdvancedRWCarrier;
    return collisionPartner;
  }

  public bool FindCollisionPartner2(float dt, bool direction, out AdvancedRWCarrier carrier)
  {
    float num1 = 0.01f;
    AdvancedRWCarrier AdvancedRWCarrier = this;
    float num2 = this._position + this._currentSpeed * dt;
    bool flag = false;
    foreach (AdvancedRWCarrier carrier1 in this._spline.carriers)
    {
      if (!((UnityEngine.Object) carrier1 == (UnityEngine.Object) this))
      {
        float num3 = carrier1._position + carrier1._currentSpeed * dt;
        if ((direction ? ((double) num3 <= (double) num2 ? 1 : 0) : ((double) num3 >= (double) num2 ? 1 : 0)) == 0)
        {
          float num4 = Mathf.Abs(num3 - num2) - (float) (0.5 * ((double) this.length + (double) carrier1.length));
          if ((double) num4 <= (double) num1)
          {
            num1 = num4;
            AdvancedRWCarrier = carrier1;
            flag = (double) Mathf.Abs(this._currentSpeed - AdvancedRWCarrier._currentSpeed) <= 0.029999999329447746 && AdvancedRWCarrier.fixedSpeed == this.fixedSpeed;
          }
        }
      }
    }
    if ((UnityEngine.Object) AdvancedRWCarrier == (UnityEngine.Object) this)
    {
      AdvancedRWSpline AdvancedRWSpline = direction ? this._spline.GetNext() : this._spline.GetPrev();
      if ((UnityEngine.Object) AdvancedRWSpline != (UnityEngine.Object) null)
      {
        float num5 = direction ? this._spline.splineLength : -AdvancedRWSpline.splineLength;
        foreach (AdvancedRWCarrier carrier2 in AdvancedRWSpline.carriers)
        {
          if (!((UnityEngine.Object) carrier2 == (UnityEngine.Object) this))
          {
            float num6 = Mathf.Abs(carrier2._position + carrier2._currentSpeed * dt + num5 - num2) - (float) (0.5 * ((double) this.length + (double) carrier2.length));
            if ((double) num6 <= (double) num1)
            {
              num1 = num6;
              AdvancedRWCarrier = carrier2;
              flag = (double) Mathf.Abs(this._currentSpeed - AdvancedRWCarrier._currentSpeed) <= 0.029999999329447746 && AdvancedRWCarrier.fixedSpeed == this.fixedSpeed;
            }
          }
        }
      }
    }
    carrier = (UnityEngine.Object) AdvancedRWCarrier == (UnityEngine.Object) this ? (AdvancedRWCarrier) null : AdvancedRWCarrier;
    return !((UnityEngine.Object) carrier == (UnityEngine.Object) null) && flag;
  }

  public bool CheckEnableGravity(bool direction)
  {
    float num = direction ? 0.2f : -0.2f;
    RWDrvAssgn rwDrvAssgn1 = this._spline.GetRwDrvAssgn(this._position);
    RWDrvAssgn rwDrvAssgn2 = (RWDrvAssgn) null;
    if (direction && (double) this._position + (double) num > (double) this._spline.splineLength)
    {
      AdvancedRWSpline next = this._spline.GetNext();
      if ((UnityEngine.Object) next != (UnityEngine.Object) null)
        rwDrvAssgn2 = next.GetRwDrvAssgn(this._position + num - this._spline.splineLength);
    }
    else if (!direction && (double) this._position + (double) num < 0.0)
    {
      AdvancedRWSpline prev = this._spline.GetPrev();
      if ((UnityEngine.Object) prev != (UnityEngine.Object) null)
        rwDrvAssgn2 = prev.GetRwDrvAssgn(this._position + num + prev.splineLength);
    }
    else
      rwDrvAssgn2 = this._spline.GetRwDrvAssgn(this._position + num);
    if (rwDrvAssgn2 == null || rwDrvAssgn1 == rwDrvAssgn2)
      return true;
    float speedAt = rwDrvAssgn2.GetSpeedAt(this._position + num);
    if ((double) num > 0.0 && (double) speedAt >= 0.0)
      return true;
    return (double) num < 0.0 && (double) speedAt <= 0.0;
  }

  public void MoveDeltaWithCollisionCheck(float dt)
  {
    this._moveDelta = this._currentSpeed * dt;
    if ((double) this._currentSpeed == 0.0)
      return;
    AdvancedRWCarrier carrier;
    float collisionPartner = this.FindCollisionPartner((double) this._currentSpeed > 0.0, out carrier);
    if ((UnityEngine.Object) carrier == (UnityEngine.Object) null || (double) collisionPartner > (double) Mathf.Abs(this.moveDelta))
      return;
    this._moveDelta = collisionPartner * Mathf.Sign(this._currentSpeed);
  }

  public bool PickupPassengerIfPossible(SkierNavigator skier, int seatId)
  {
    if (seatId >= this.passengers.Length || seatId >= this.passengerSeats.Length || !((UnityEngine.Object) this.passengers[seatId] == (UnityEngine.Object) null) || !((UnityEngine.Object) this.passengerSeats[seatId].seat != (UnityEngine.Object) null))
      return false;
    skier.EnterCarrier(this, this.passengerSeats[seatId], seatId);
    this.passengers[seatId] = skier;
    this.passengerProbabilities[seatId] = 1f;
    return true;
  }

  public void PassengerLeft(int seatId)
  {
    if (seatId >= this.passengers.Length)
      return;
    this.passengers[seatId] = (SkierNavigator) null;
    this.passengerProbabilities[seatId] = 0.0f;
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

  [Serializable]
  public struct CarrierSeat(Transform transf, string trigger = "", bool skis = false, bool sticks = false)
  {
    public Transform seat = transf;
    public bool showSkis = skis;
    public bool showSticks = sticks;
    public string animationTrigger = trigger;
  }

  [Serializable]
  public class AnimatedCarrierPart
  {
    public Animation animation;
    public string name;
    public float position;
    public float targetPosition;

    public void UpdateAnimation()
    {
      this.animation[this.animation.clip.name].time = this.position * this.animation.clip.length;
      this.animation.Play();
      this.animation.Sample();
      this.animation.Stop();
    }

    public void SetPosition(float newPosition, int type = 1)
    {
      switch (type)
      {
        case 0:
          this.targetPosition = newPosition;
          break;
        case 1:
          this.targetPosition = this.position = newPosition;
          break;
        case 2:
          this.targetPosition = this.position = Mathf.Max(this.position, newPosition);
          break;
        case 3:
          this.targetPosition = this.position = Mathf.Min(this.position, newPosition);
          break;
      }
      this.UpdateAnimation();
    }

    public void SetPositionOverride(float newPositionJump, float newTarget)
    {
      this.position = newPositionJump;
      this.targetPosition = newTarget;
      this.UpdateAnimation();
    }
  }
}
*/
