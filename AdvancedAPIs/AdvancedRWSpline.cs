
/*
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedRWSpline : MonoBehaviour
{
  public static List<AdvancedRWSpline> instances = new List<AdvancedRWSpline>();
  public Vector3[] points;
  public float[] segmentLengths;
  public float[] cumulativeLength;
  public float splineLength;
  public float invSplineLength;
  public bool smoothMode;
  public AdvancedRWSpline[] nextSplines;
  public AdvancedRWSpline[] prevSplines;
  private int _nextIdx;
  private int _prevIdx;
  protected Quaternion _lastRotation = Quaternion.identity;
  protected bool _lastRotChanged = true;
  public AdvancedRWSpline.AnimatedPartRail[] rails = new AdvancedRWSpline.AnimatedPartRail[0];
  public List<RWDrvAssgn> drives = new List<RWDrvAssgn>();
  public List<AdvancedRWCarrier> carriers = new List<AdvancedRWCarrier>();
  private List<RWLuaTrigger> _triggers = new List<RWLuaTrigger>();
  private List<RWLuaProxySensor> _proxySensors = new List<RWLuaProxySensor>();

  public bool rotationChanged => this._lastRotChanged;

  public AdvancedRWSpline GetNext()
  {
    return this._nextIdx < 0 || this._nextIdx >= this.nextSplines.Length ? (AdvancedRWSpline) null : this.nextSplines[this._nextIdx];
  }

  public AdvancedRWSpline GetPrev()
  {
    return this._prevIdx < 0 || this._prevIdx >= this.prevSplines.Length ? (AdvancedRWSpline) null : this.prevSplines[this._prevIdx];
  }

  public int GetPrevIdx() => this._prevIdx;

  public int GetNextIdx() => this._nextIdx;

  public void SetPrevIdx(int idx)
  {
    this._prevIdx = Mathf.Clamp(idx, 0, this.prevSplines.Length - 1);
  }

  public void SetNextIdx(int idx)
  {
    this._nextIdx = Mathf.Clamp(idx, 0, this.nextSplines.Length - 1);
  }

  public void CleanupPrefab()
  {
    this.carriers = new List<AdvancedRWCarrier>();
    this._triggers = new List<RWLuaTrigger>();
    this._proxySensors = new List<RWLuaProxySensor>();
  }

  public static void GlobalLateUpdate()
  {
    float ropewayDt = GameControl.ropewayDt;
    foreach (AdvancedRWSpline instance in AdvancedRWSpline.instances)
    {
      if (instance.carriers.Count != 0)
      {
        instance._lastRotChanged = instance.transform.rotation != instance._lastRotation;
        instance._lastRotation = instance.transform.rotation;
        foreach (RWDrvAssgn drive in instance.drives)
        {
          if (!((UnityEngine.Object) drive.drive != (UnityEngine.Object) null) && (double) drive.gravity != 0.0)
            AdvancedRWSpline.UpdateGravitySpline(ropewayDt, instance, drive);
        }
      }
    }
  }

  private static void UpdateGravitySpline(float dt, AdvancedRWSpline spline, RWDrvAssgn drvAssgn)
  {
    double num1 = 1.0 / (double) dt;
    if ((UnityEngine.Object) drvAssgn.drive != (UnityEngine.Object) null || (double) drvAssgn.gravity == 0.0)
      return;
    bool flag1 = (double) drvAssgn.inclination < 0.0;
    AdvancedRWCarrier AdvancedRWCarrier1 = (AdvancedRWCarrier) null;
    float num2 = -1f;
    AdvancedRWCarrier AdvancedRWCarrier2 = (AdvancedRWCarrier) null;
    float num3 = -1f;
    foreach (AdvancedRWCarrier carrier in spline.carriers)
    {
      if (carrier.isActive)
      {
        if ((double) carrier.GetPosition() >= (double) drvAssgn.posBegin && (double) carrier.GetPosition() < (double) drvAssgn.posEnd)
        {
          if ((double) num3 < 0.0 || (flag1 ? ((double) carrier.GetPosition() > (double) num3 ? 1 : 0) : ((double) carrier.GetPosition() < (double) num3 ? 1 : 0)) != 0)
          {
            num3 = carrier.GetPosition();
            AdvancedRWCarrier2 = carrier;
          }
        }
        else if ((flag1 ? ((double) carrier.GetPosition() >= (double) drvAssgn.posEnd ? 1 : 0) : ((double) carrier.GetPosition() < (double) drvAssgn.posBegin ? 1 : 0)) != 0 && ((double) num2 < 0.0 || (flag1 ? ((double) carrier.GetPosition() <= (double) num2 ? 1 : 0) : ((double) carrier.GetPosition() >= (double) num2 ? 1 : 0)) != 0))
        {
          AdvancedRWCarrier1 = carrier;
          num2 = carrier.GetPosition();
        }
      }
    }
    if ((UnityEngine.Object) AdvancedRWCarrier2 == (UnityEngine.Object) null)
      return;
    float num4 = -1f;
    float a = 0.0f;
    if ((UnityEngine.Object) AdvancedRWCarrier1 != (UnityEngine.Object) null)
    {
      if ((flag1 ? ((double) AdvancedRWCarrier1.GetSpeed() < 0.0 ? 1 : 0) : ((double) AdvancedRWCarrier1.GetSpeed() > 0.0 ? 1 : 0)) != 0)
      {
        double num5 = (double) AdvancedRWCarrier1.GetPosition() + (double) AdvancedRWCarrier1.GetSpeed() * (double) dt;
        AdvancedRWCarrier1.Move(AdvancedRWCarrier1.GetSpeed() * dt);
        double num6 = 0.5 * (double) AdvancedRWCarrier1.length * (flag1 ? -1.0 : 1.0);
        num4 = (float) (num5 + num6);
        a = AdvancedRWCarrier1.GetSpeed();
        AdvancedRWCarrier1.BlockNextGlobalUpdate();
      }
      else
      {
        num4 = num2 + (float) (0.5 * (double) AdvancedRWCarrier1.length * (flag1 ? -1.0 : 1.0));
        a = AdvancedRWCarrier1.GetSpeed();
      }
    }
    List<AdvancedRWCarrier> AdvancedRWCarrierList = new List<AdvancedRWCarrier>();
    do
    {
      AdvancedRWCarrierList.Add(AdvancedRWCarrier2);
      float num7 = AdvancedRWCarrier2.GetSpeed() + (AdvancedRWCarrier2.CheckEnableGravity((double) drvAssgn.gravity > 0.0) ? drvAssgn.gravity * dt : 0.0f);
      float num8 = AdvancedRWCarrier2.GetPosition() + num7 * dt;
      if ((double) num4 >= 0.0)
      {
        bool flag2 = false;
        float num9 = num4 + (float) (0.5 * (double) AdvancedRWCarrier2.length * (flag1 ? -1.0 : 1.0));
        if (flag1)
        {
          if ((double) num8 >= (double) num9)
          {
            num8 = num9;
            flag2 = true;
          }
        }
        else if ((double) num8 <= (double) num9)
        {
          num8 = num9;
          flag2 = true;
        }
        float delta = num8 - AdvancedRWCarrier2.GetPosition();
        if (flag2)
          AdvancedRWCarrier2.SetSpeed(flag1 ? Mathf.Min(a, num7) : Mathf.Max(a, num7));
        else
          AdvancedRWCarrier2.SetSpeed(num7);
        AdvancedRWCarrier2.Move(delta);
        AdvancedRWCarrier2.BlockNextGlobalUpdate();
      }
      else
      {
        AdvancedRWCarrier2.Move(num7 * dt);
        AdvancedRWCarrier2.SetSpeed(num7);
        AdvancedRWCarrier2.BlockNextGlobalUpdate();
      }
      num4 = AdvancedRWCarrier2.GetPosition() + (float) (0.5 * (double) AdvancedRWCarrier2.length * (flag1 ? -1.0 : 1.0));
      a = AdvancedRWCarrier2.GetSpeed();
      float num10 = -1f;
      AdvancedRWCarrier AdvancedRWCarrier3 = (AdvancedRWCarrier) null;
      if (flag1)
      {
        foreach (AdvancedRWCarrier carrier in spline.carriers)
        {
          if (carrier.isActive && (double) carrier.GetPosition() >= (double) drvAssgn.posBegin && (double) carrier.GetPosition() < (double) drvAssgn.posEnd && (double) carrier.GetPosition() <= (double) AdvancedRWCarrier2.GetPosition() && !carrier.GetIsBlocked() && !AdvancedRWCarrierList.Contains(carrier) && ((double) num10 < 0.0 || (double) carrier.GetPosition() >= (double) num10))
          {
            num10 = carrier.GetPosition();
            AdvancedRWCarrier3 = carrier;
          }
        }
      }
      else
      {
        foreach (AdvancedRWCarrier carrier in spline.carriers)
        {
          if (carrier.isActive && (double) carrier.GetPosition() >= (double) drvAssgn.posBegin && (double) carrier.GetPosition() < (double) drvAssgn.posEnd && (double) carrier.GetPosition() >= (double) AdvancedRWCarrier2.GetPosition() && !carrier.GetIsBlocked() && !AdvancedRWCarrierList.Contains(carrier) && ((double) num10 < 0.0 || (double) carrier.GetPosition() <= (double) num10))
          {
            num10 = carrier.GetPosition();
            AdvancedRWCarrier3 = carrier;
          }
        }
      }
      AdvancedRWCarrier2 = AdvancedRWCarrier3;
    }
    while ((UnityEngine.Object) AdvancedRWCarrier2 != (UnityEngine.Object) null);
  }

  private void Awake() => AdvancedRWSpline.instances.Add(this);

  private void OnDestroy() => AdvancedRWSpline.instances.Remove(this);

  public static AdvancedRWSpline BuildSpline(
    AdvancedRWSpline spline,
    List<Vector3> points,
    bool pointsInLocalSpace,
    bool inverse,
    AdvancedRWSpline[] bottomConnection,
    AdvancedRWSpline[] topConnection)
  {
    if ((UnityEngine.Object) spline == (UnityEngine.Object) null)
      spline = AdvancedRWSpline.New();
    spline.ReplaceSplinePoints(points, pointsInLocalSpace, inverse);
    spline.prevSplines = inverse ? topConnection : bottomConnection;
    spline.nextSplines = !inverse ? topConnection : bottomConnection;
    return spline;
  }

  public static AdvancedRWSpline New() => new GameObject("spline").AddComponent<AdvancedRWSpline>();

  public void ReplaceSplinePoints(List<Vector3> points, bool pointsInLocalSpace, bool inverse)
  {
    Vector3[] vector3Array = new Vector3[points.Count];
    if (inverse)
    {
      for (int index = 0; index < points.Count; ++index)
        vector3Array[index] = !pointsInLocalSpace ? this.transform.InverseTransformPoint(points[points.Count - 1 - index]) : points[points.Count - 1 - index];
    }
    else
    {
      for (int index = 0; index < points.Count; ++index)
        vector3Array[index] = !pointsInLocalSpace ? this.transform.InverseTransformPoint(points[index]) : points[index];
    }
    this.points = vector3Array;
    this.ClearTriggersAndSensors();
    this.RefreshSpline();
  }

  public void ClearTriggersAndSensors()
  {
    this._triggers = new List<RWLuaTrigger>();
    this._proxySensors = new List<RWLuaProxySensor>();
  }

  private void Start() => this.RefreshSpline();

  public void Reset()
  {
    this.points = new Vector3[3]
    {
      new Vector3(0.0f, 0.0f, -1f),
      new Vector3(0.0f, 0.0f, 0.0f),
      new Vector3(0.0f, 0.0f, 1f)
    };
    this.segmentLengths = new float[2];
    this.cumulativeLength = new float[2];
    this.splineLength = 1f;
    this.invSplineLength = 1f / this.splineLength;
    this.nextSplines = new AdvancedRWSpline[0];
    this.prevSplines = new AdvancedRWSpline[0];
    this.drives = new List<RWDrvAssgn>();
    this.RefreshSpline();
  }

  public void RegisterTrigger(RWLuaTrigger trigger) => this._triggers.Add(trigger);

  public void RegisterProxySensor(RWLuaProxySensor sensor) => this._proxySensors.Add(sensor);

  public void CheckTriggers(AdvancedRWCarrier sender, float from, float to)
  {
    foreach (RWLuaTrigger trigger in this._triggers)
      trigger.CheckIsTriggered(sender, from, to);
    foreach (RWLuaProxySensor proxySensor in this._proxySensors)
      proxySensor.CheckIsTriggered(sender, to);
  }

  public void AddRail(AdvancedRWSpline.AnimatedPartRail rail)
  {
    if (rail == null)
      return;
    Array.Resize<AdvancedRWSpline.AnimatedPartRail>(ref this.rails, this.rails.Length + 1);
    this.rails[this.rails.Length - 1] = rail;
  }

  public int GetPointIndex(float position)
  {
    for (int pointIndex = 0; pointIndex < this.cumulativeLength.Length; ++pointIndex)
    {
      if ((double) position <= (double) this.cumulativeLength[pointIndex])
        return pointIndex;
    }
    return this.cumulativeLength.Length;
  }

  public Vector3 GetPoint(float position)
  {
    if ((double) position > (double) this.splineLength && (UnityEngine.Object) this.GetNext() != (UnityEngine.Object) null)
      return this.GetNext().GetPoint(position - this.splineLength);
    if ((double) position < 0.0 && (UnityEngine.Object) this.GetPrev() != (UnityEngine.Object) null)
      return this.GetPrev().GetPoint(this.GetPrev().splineLength + position);
    position = Mathf.Clamp(position, 0.0f, this.splineLength);
    int pointIndex = this.GetPointIndex(position);
    Vector3 point1 = this.points[pointIndex];
    Vector3 point2 = this.points[pointIndex + 1];
    float t = (float) (1.0 - ((double) this.cumulativeLength[pointIndex] - (double) position) / (double) this.segmentLengths[pointIndex]);
    return this.smoothMode ? this.transform.TransformPoint(point1 * (float) (3.0 - 2.0 * (double) t) * t * t + point2 * (float) (1.0 + (2.0 * (double) t - 3.0) * (double) t * (double) t)) : this.transform.TransformPoint(Vector3.Lerp(point1, point2, t));
  }

  public Vector3 GetDirection(float position, bool smooth)
  {
    position = Mathf.Clamp(position, 0.0f, this.splineLength);
    int pointIndex = this.GetPointIndex(position);
    Vector3 point1 = this.points[pointIndex];
    Vector3 point2 = this.points[pointIndex + 1];
    if (point1 == point2)
      return this.transform.TransformDirection(Vector3.up);
    Vector3 vector3_1;
    if (smooth)
    {
      Vector3 vector3_2 = pointIndex > 0 ? this.points[pointIndex - 1] : this.points[0];
      float num1 = pointIndex > 0 ? this.cumulativeLength[pointIndex - 1] : this.cumulativeLength[this.cumulativeLength.Length - 1];
      float num2 = this.cumulativeLength[pointIndex];
      float num3 = position - num1;
      if ((double) num3 < 0.0)
        num3 += this.cumulativeLength[this.cumulativeLength.Length - 1];
      float a = num3 / (num2 - num1);
      vector3_1 = Vector3.Lerp(point1 - vector3_2, point2 - point1, Mathf.Max(a, 0.01f));
    }
    else
      vector3_1 = point2 - point1;
    return this.transform.TransformDirection(Vector3.Normalize(vector3_1));
  }

  public Vector3 GetDirection(float position) => this.GetDirection(position, false);

  public int AddPoint(int index, int dir)
  {
    if (dir > 0)
    {
      if (index >= this.points.Length - 1)
      {
        Vector3 point1 = this.points[this.points.Length - 2];
        Vector3 point2 = this.points[this.points.Length - 1];
        Array.Resize<Vector3>(ref this.points, this.points.Length + 1);
        this.points[this.points.Length - 1] = point2 + point2 - point1;
        this.RefreshSpline();
        return this.points.Length - 1;
      }
      Array.Resize<Vector3>(ref this.points, this.points.Length + 1);
      for (int index1 = this.points.Length - 1; index1 >= index + 1; --index1)
        this.points[index1] = this.points[index1 - 1];
      Vector3 point3 = this.points[index];
      Vector3 point4 = this.points[index + 2];
      this.points[index + 1] = Vector3.Lerp(point3, point4, 0.5f);
      this.RefreshSpline();
      return index + 1;
    }
    if (index <= 0)
    {
      Array.Resize<Vector3>(ref this.points, this.points.Length + 1);
      for (int index2 = this.points.Length - 1; index2 >= 1; --index2)
        this.points[index2] = this.points[index2 - 1];
      Vector3 point5 = this.points[2];
      Vector3 point6 = this.points[1];
      this.points[0] = point6 + point6 - point5;
      this.RefreshSpline();
      return 0;
    }
    Array.Resize<Vector3>(ref this.points, this.points.Length + 1);
    for (int index3 = this.points.Length - 1; index3 >= index; --index3)
      this.points[index3] = this.points[index3 - 1];
    Vector3 point7 = this.points[index - 1];
    Vector3 point8 = this.points[index + 1];
    this.points[index] = Vector3.Lerp(point7, point8, 0.5f);
    this.RefreshSpline();
    return index;
  }

  public int RemovePoint(int index)
  {
    int num1 = index;
    for (int index1 = index + 1; index1 < this.points.Length; ++index1)
      this.points[index1 - 1] = this.points[index1];
    Array.Resize<Vector3>(ref this.points, this.points.Length - 1);
    this.RefreshSpline();
    int num2 = num1 < 0 ? 0 : num1;
    return num2 > this.points.Length - 1 ? this.points.Length - 1 : num2;
  }

  public void RefreshSpline()
  {
    this.RemoveDoubles();
    float num1 = 0.0f;
    Array.Resize<float>(ref this.segmentLengths, this.points.Length - 1);
    Array.Resize<float>(ref this.cumulativeLength, this.points.Length - 1);
    for (int index = 1; index < this.points.Length; ++index)
    {
      Vector3 point = this.points[index - 1];
      Vector3 vector3 = this.points[index] - point;
      this.segmentLengths[index - 1] = vector3.magnitude;
      num1 += this.segmentLengths[index - 1];
      this.cumulativeLength[index - 1] = num1;
    }
    float num2 = 0.0f;
    for (int index = 0; index < this.drives.Count; ++index)
    {
      RWDrvAssgn drive = this.drives[index];
      drive.posBegin = num2;
      num2 = drive.posEnd;
      if (index >= this.drives.Count - 1)
        drive.posEnd = num1;
      drive.Refresh();
    }
    this.splineLength = num1;
    this.invSplineLength = 1f / this.splineLength;
  }

  public void RegisterCarrier(AdvancedRWCarrier carrier)
  {
    if (!((UnityEngine.Object) carrier != (UnityEngine.Object) null))
      return;
    this.carriers.Add(carrier);
  }

  public void UnregisterCarrier(AdvancedRWCarrier carrier)
  {
    int index = 0;
    foreach (AdvancedRWCarrier carrier1 in this.carriers)
    {
      if ((UnityEngine.Object) carrier == (UnityEngine.Object) carrier1)
      {
        this.carriers.RemoveAt(index);
        break;
      }
      ++index;
    }
  }

  public float GetTotalLength() => this.splineLength;

  public float GetProjectedLength()
  {
    float projectedLength = 0.0f;
    foreach (RWDrvAssgn drive in this.drives)
      projectedLength += drive.GetProjectedLength();
    return projectedLength;
  }

  public RWDrvAssgn GetRwDrvAssgn(float position)
  {
    foreach (RWDrvAssgn drive in this.drives)
    {
      if ((double) position <= (double) drive.posEnd)
        return drive;
    }
    return (RWDrvAssgn) null;
  }

  public void RemoveDoubles()
  {
    int index = 1;
    while (index < this.points.Length)
    {
      if (this.points[index - 1] == this.points[index])
        this.RemovePoint(index);
      else
        ++index;
    }
  }

  public void InverseDirection()
  {
    Vector3[] vector3Array = new Vector3[this.points.Length];
    int num = 0;
    for (int index = this.points.Length - 1; index >= 0; --index)
      vector3Array[num++] = this.points[index];
    this.points = vector3Array;
    this.RefreshSpline();
  }

  public class AnimatedPartRail
  {
    public float position0;
    public float position1;
    public float value0;
    public float value1;
    public string channel;
    public int type;
  }
}
*/
