// Decompiled with JetBrains decompiler
// Type: RWDrvAssgn
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 52680E34-E592-4022-8EF3-C68010F52EE0
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Winter Resort Simulator Season 2\WinterResortSimulator_Season2_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class AdvancedRWDrvAssgn
{
  public float speedScale0 = 1f;
  public float speedScale1 = 1f;
  public float posBegin;
  public float posEnd = 1f;
  public bool isAcceleratorDrive;
  private float _invLength = 1f;
  private bool _engaged = true;
  [HideInInspector]
  public float _inclination;
  [HideInInspector]
  public float _gravity;
  public RWDrive drive;
  private float _v1sq = 1f;
  private float _acc2;

  public bool engaged
  {
    get => _engaged && (UnityEngine.Object) drive != (UnityEngine.Object) null && drive.engaged;
    set => _engaged = value;
  }

  public float inclination
  {
    get => _inclination;
    set
    {
      _inclination = value;
      _gravity = -9.81f * Mathf.Sin(value * ((float) Math.PI / 180f));
    }
  }

  public float gravity => _gravity;

  public float acc
  {
    get
    {
      return (UnityEngine.Object) drive == (UnityEngine.Object) null ? _acc2 * 0.5f * _invLength : _acc2 * 0.5f * _invLength * drive.maxSpeed * drive.maxSpeed;
    }
  }

  public void Refresh()
  {
    _invLength = (float) (1.0 / ((double) posEnd - (double) posBegin));
    _v1sq = speedScale0 * speedScale0;
    _acc2 = speedScale1 * speedScale1 - _v1sq;
    inclination = inclination;
  }

  public float GetProjectedLength()
  {
    return (float) (2.0 * ((double) posEnd - (double) posBegin) / ((double) speedScale0 + (double) speedScale1));
  }

  public float GetSpeedAt(float position)
  {
    if ((double) position < (double) posBegin || (double) position > (double) posEnd)
    {
      return 0.0f;
    }
    float speedAt = 0.0f;
    if ((UnityEngine.Object) drive != (UnityEngine.Object) null)
    {
      speedAt = drive.currentSpeed;
      if (isAcceleratorDrive)
        return speedAt;
    }
    return Mathf.Sqrt(_v1sq + (position - posBegin) * _invLength * _acc2) * speedAt;
  }

  public float GetScaleAt(float position, bool overrideAccelerator = false)
  {
    if ((double) position < (double) posBegin || (double) position > (double) posEnd)
    {
      Debug.Log((object) "Invalid GetScaleAt Call");
      return 0.0f;
    }
    return isAcceleratorDrive && !overrideAccelerator ? 1f : Mathf.Sqrt(_v1sq + (position - posBegin) * _invLength * _acc2);
  }
}
