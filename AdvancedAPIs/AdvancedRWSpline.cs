
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
  public AnimatedPartRail[] Rails = Array.Empty<AnimatedPartRail>();
  public List<AdvancedRWDrvAssgn> drives = new List<AdvancedRWDrvAssgn>();
  public List<AdvancedRWCarrier> carriers = new List<AdvancedRWCarrier>();
  private List<AdvancedRWLuaTrigger> _triggers = new List<AdvancedRWLuaTrigger>();
  private List<AdvancedRWLuaProxySensor> _proxySensors = new List<AdvancedRWLuaProxySensor>();

  public bool rotationChanged => _lastRotChanged;

  public AdvancedRWSpline GetNext()
  {
    return _nextIdx < 0 || _nextIdx >= nextSplines.Length ? (AdvancedRWSpline) null : nextSplines[_nextIdx];
  }

  public AdvancedRWSpline GetPrev()
  {
    return _prevIdx < 0 || _prevIdx >= prevSplines.Length ? (AdvancedRWSpline) null : prevSplines[_prevIdx];
  }

  public int GetPrevIdx() => _prevIdx;

  public int GetNextIdx() => _nextIdx;

  public void SetPrevIdx(int idx)
  {
    _prevIdx = Mathf.Clamp(idx, 0, prevSplines.Length - 1);
  }

  public void SetNextIdx(int idx)
  {
    _nextIdx = Mathf.Clamp(idx, 0, nextSplines.Length - 1);
  }

  public void CleanupPrefab()
  {
    carriers = new List<AdvancedRWCarrier>();
    _triggers = new List<AdvancedRWLuaTrigger>();
    _proxySensors = new List<AdvancedRWLuaProxySensor>();
  }

  public static void GlobalLateUpdate()
  {
    float ropewayDt = GameControl.ropewayDt;
    foreach (AdvancedRWSpline instance in instances)
    {
      if (instance.carriers.Count != 0)
      {
        instance._lastRotChanged = instance.transform.rotation != instance._lastRotation;
        instance._lastRotation = instance.transform.rotation;
        foreach (AdvancedRWDrvAssgn drive in instance.drives)
        {
          if (!drive.drive || !drive.engaged)
          {
            UpdateGravitySpline(ropewayDt, instance, drive);
          }
        }
      }
    }
  }

  private static void UpdateGravitySpline(float dt, AdvancedRWSpline spline, AdvancedRWDrvAssgn drvAssgn)
  {
    double num1 = 1.0 / (double) dt;
    if ((UnityEngine.Object) drvAssgn.drive != (UnityEngine.Object) null || (double) drvAssgn.gravity == 0.0)
      return;
    bool flag1 = (double) drvAssgn.inclination < 0.0;
    AdvancedRWCarrier AdvancedRWCarrier1 = null;
    float num2 = -1f;
    AdvancedRWCarrier AdvancedRWCarrier2 = null;
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
        vector3Array[index] = !pointsInLocalSpace ? transform.InverseTransformPoint(points[points.Count - 1 - index]) : points[points.Count - 1 - index];
    }
    else
    {
      for (int index = 0; index < points.Count; ++index)
        vector3Array[index] = !pointsInLocalSpace ? transform.InverseTransformPoint(points[index]) : points[index];
    }
    this.points = vector3Array;
    ClearTriggersAndSensors();
    RefreshSpline();
  }

  public void ClearTriggersAndSensors()
  {
    _triggers = new List<AdvancedRWLuaTrigger>();
    _proxySensors = new List<AdvancedRWLuaProxySensor>();
  }

  private void Start() => RefreshSpline();

  public void Reset()
  {
    points = new Vector3[3]
    {
      new Vector3(0.0f, 0.0f, -1f),
      new Vector3(0.0f, 0.0f, 0.0f),
      new Vector3(0.0f, 0.0f, 1f)
    };
    segmentLengths = new float[2];
    cumulativeLength = new float[2];
    splineLength = 1f;
    invSplineLength = 1f / splineLength;
    nextSplines = new AdvancedRWSpline[0];
    prevSplines = new AdvancedRWSpline[0];
    drives = new List<AdvancedRWDrvAssgn>();
    RefreshSpline();
  }

  public void RegisterTrigger(AdvancedRWLuaTrigger trigger) => _triggers.Add(trigger);

  public void RegisterProxySensor(AdvancedRWLuaProxySensor sensor) => _proxySensors.Add(sensor);

  public void CheckTriggers(AdvancedRWCarrier sender, float from, float to)
  {
    foreach (AdvancedRWLuaTrigger trigger in _triggers)
      trigger.CheckIsTriggered(sender, from, to);
    foreach (AdvancedRWLuaProxySensor proxySensor in _proxySensors)
      proxySensor.CheckIsTriggered(sender, to);
  }

  public void AddRail(AnimatedPartRail rail)
  {
    if (rail == null)
      return;
    Array.Resize<AdvancedRWSpline.AnimatedPartRail>(ref Rails, Rails.Length + 1);
    Rails[Rails.Length - 1] = rail;
  }

  public int GetPointIndex(float position)
  {
    for (int pointIndex = 0; pointIndex < cumulativeLength.Length; ++pointIndex)
    {
      if ((double) position <= (double) cumulativeLength[pointIndex])
        return pointIndex;
    }
    return cumulativeLength.Length;
  }

  public Vector3 GetPoint(float position)
  {
    if ((double) position > (double) splineLength && (UnityEngine.Object) GetNext() != (UnityEngine.Object) null)
      return GetNext().GetPoint(position - splineLength);
    if ((double) position < 0.0 && (UnityEngine.Object) GetPrev() != (UnityEngine.Object) null)
      return GetPrev().GetPoint(GetPrev().splineLength + position);
    position = Mathf.Clamp(position, 0.0f, splineLength);
    int pointIndex = GetPointIndex(position);
    Vector3 point1 = points[pointIndex];
    Vector3 point2 = points[pointIndex + 1];
    float t = (float) (1.0 - ((double) cumulativeLength[pointIndex] - (double) position) / (double) segmentLengths[pointIndex]);
    return smoothMode ? transform.TransformPoint(point1 * ((float) (3.0 - 2.0 * (double) t) * t * t) + point2 * (float) (1.0 + (2.0 * (double) t - 3.0) * (double) t * (double) t)) : transform.TransformPoint(Vector3.Lerp(point1, point2, t));
  }

  public Vector3 GetDirection(float position, bool smooth)
  {
    position = Mathf.Clamp(position, 0.0f, splineLength);
    int pointIndex = GetPointIndex(position);
    Vector3 point1 = points[pointIndex];
    Vector3 point2 = points[pointIndex + 1];
    if (point1 == point2)
      return transform.TransformDirection(Vector3.up);
    Vector3 vector3_1;
    if (smooth)
    {
      Vector3 vector3_2 = pointIndex > 0 ? points[pointIndex - 1] : points[0];
      float num1 = pointIndex > 0 ? cumulativeLength[pointIndex - 1] : cumulativeLength[cumulativeLength.Length - 1];
      float num2 = cumulativeLength[pointIndex];
      float num3 = position - num1;
      if ((double) num3 < 0.0)
        num3 += cumulativeLength[cumulativeLength.Length - 1];
      float a = num3 / (num2 - num1);
      vector3_1 = Vector3.Lerp(point1 - vector3_2, point2 - point1, Mathf.Max(a, 0.01f));
    }
    else
      vector3_1 = point2 - point1;
    return transform.TransformDirection(Vector3.Normalize(vector3_1));
  }

  public Vector3 GetDirection(float position) => GetDirection(position, false);

  public int AddPoint(int index, int dir)
  {
    if (dir > 0)
    {
      if (index >= points.Length - 1)
      {
        Vector3 point1 = points[points.Length - 2];
        Vector3 point2 = points[points.Length - 1];
        Array.Resize<Vector3>(ref points, points.Length + 1);
        points[points.Length - 1] = point2 + point2 - point1;
        RefreshSpline();
        return points.Length - 1;
      }
      Array.Resize<Vector3>(ref points, points.Length + 1);
      for (int index1 = points.Length - 1; index1 >= index + 1; --index1)
        points[index1] = points[index1 - 1];
      Vector3 point3 = points[index];
      Vector3 point4 = points[index + 2];
      points[index + 1] = Vector3.Lerp(point3, point4, 0.5f);
      RefreshSpline();
      return index + 1;
    }
    if (index <= 0)
    {
      Array.Resize<Vector3>(ref points, points.Length + 1);
      for (int index2 = points.Length - 1; index2 >= 1; --index2)
        points[index2] = points[index2 - 1];
      Vector3 point5 = points[2];
      Vector3 point6 = points[1];
      points[0] = point6 + point6 - point5;
      RefreshSpline();
      return 0;
    }
    Array.Resize<Vector3>(ref points, points.Length + 1);
    for (int index3 = points.Length - 1; index3 >= index; --index3)
      points[index3] = points[index3 - 1];
    Vector3 point7 = points[index - 1];
    Vector3 point8 = points[index + 1];
    points[index] = Vector3.Lerp(point7, point8, 0.5f);
    RefreshSpline();
    return index;
  }

  public int RemovePoint(int index)
  {
    int num1 = index;
    for (int index1 = index + 1; index1 < points.Length; ++index1)
      points[index1 - 1] = points[index1];
    Array.Resize<Vector3>(ref points, points.Length - 1);
    RefreshSpline();
    int num2 = num1 < 0 ? 0 : num1;
    return num2 > points.Length - 1 ? points.Length - 1 : num2;
  }

  public void RefreshSpline()
  {
    RemoveDoubles();
    float num1 = 0.0f;
    Array.Resize<float>(ref segmentLengths, points.Length - 1);
    Array.Resize<float>(ref cumulativeLength, points.Length - 1);
    for (int index = 1; index < points.Length; ++index)
    {
      Vector3 point = points[index - 1];
      Vector3 vector3 = points[index] - point;
      segmentLengths[index - 1] = vector3.magnitude;
      num1 += segmentLengths[index - 1];
      cumulativeLength[index - 1] = num1;
    }
    float num2 = 0.0f;
    for (int index = 0; index < drives.Count; ++index)
    {
      AdvancedRWDrvAssgn drive = drives[index];
      drive.posBegin = num2;
      num2 = drive.posEnd;
      if (index >= drives.Count - 1)
        drive.posEnd = num1;
      drive.Refresh();
    }
    splineLength = num1;
    invSplineLength = 1f / splineLength;
  }

  public void RegisterCarrier(AdvancedRWCarrier carrier)
  {
    if (!((UnityEngine.Object) carrier != (UnityEngine.Object) null))
      return;
    carriers.Add(carrier);
  }

  public void UnregisterCarrier(AdvancedRWCarrier carrier)
  {
    int index = 0;
    foreach (AdvancedRWCarrier carrier1 in carriers)
    {
      if ((UnityEngine.Object) carrier == (UnityEngine.Object) carrier1)
      {
        carriers.RemoveAt(index);
        break;
      }
      ++index;
    }
  }

  public float GetTotalLength() => splineLength;

  public float GetProjectedLength()
  {
    float projectedLength = 0.0f;
    foreach (AdvancedRWDrvAssgn drive in drives)
      projectedLength += drive.GetProjectedLength();
    return projectedLength;
  }

  public AdvancedRWDrvAssgn GetRwDrvAssgn(float position)
  {
    foreach (AdvancedRWDrvAssgn drive in drives)
    {
      if ((double) position <= (double) drive.posEnd)
        return drive;
    }
    return (AdvancedRWDrvAssgn) null;
  }

  public void RemoveDoubles()
  {
    int index = 1;
    while (index < points.Length)
    {
      if (points[index - 1] == points[index])
        RemovePoint(index);
      else
        ++index;
    }
  }

  public void InverseDirection()
  {
    Vector3[] vector3Array = new Vector3[points.Length];
    int num = 0;
    for (int index = points.Length - 1; index >= 0; --index)
      vector3Array[num++] = points[index];
    points = vector3Array;
    RefreshSpline();
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