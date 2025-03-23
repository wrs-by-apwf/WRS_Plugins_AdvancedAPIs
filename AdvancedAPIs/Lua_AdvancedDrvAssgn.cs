using System;
using System.Reflection;
using UnityEngine;

public class Lua_AdvancedDrvAssgn
{
    // call by the core class after the core is initialized (Awake method in core)
    public static void Awake()
    {
    }

    // call by the core class after the core is updated (Update method in core)
    public static void Update()
    {
    }

    // called by the core class after the core Setup is invoked
    public static void Setup(IntPtr L)
    {
        // Call the methods using reflection
        advancedAPIsCore.InvokeCreateModule(L, "ARWDrvAssgn");
        advancedAPIsCore.RegisterModuleFunction(L, "ARWDrvAssgn", "setDistance", new Func<IntPtr, int>(Lua_SetDistance));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWDrvAssgn", "setInclination", new Func<IntPtr, int>(Lua_setInclination));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWDrvAssgn", "setSpeedScale0", new Func<IntPtr, int>(Lua_setSpeedScale0));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWDrvAssgn", "setSpeedScale1", new Func<IntPtr, int>(Lua_setSpeedScale1));

        // log that the setup is complete
        advancedAPIsCore.LogInfo("ARWDrvAssgn APIs have been added");
    }

    internal static int Lua_SetDistance(IntPtr L)
    {
        // Ensure correct number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 4 });

        // Get Transform object
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });
        if (transformObj == null)
        {
            advancedAPIsCore.LogError("Error: Transform object is NULL! Argument 1 might be invalid.");
            return 0;
        }


        // Get the GameObject and RWSpline
        GameObject obj = transformObj.gameObject;
        RWSpline spline = obj.GetComponent<RWSpline>();
        if (spline == null)
        {
            advancedAPIsCore.LogError($"GameObject '{obj.name}' exists but does NOT have an RWSpline!");
            return 0;
        }

        // Get the boolean value (setStart)
        bool setStart = (bool)advancedAPIsCore.luaCS_assertGetBoolean.Invoke(null, new object[] { L, 2, false });

        // Get DriveNumber
        int DriveNumber = (int)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 3 });
        if (DriveNumber < 0 || DriveNumber >= spline.drives.Count)
        {
            advancedAPIsCore.LogError($"Invalid DriveNumber {DriveNumber}. Must be between 0 and {spline.drives.Count - 1}.");
            return 0;
        }


        // Fetch the Drive assignment
        RWDrvAssgn DrvAssgn = spline.drives[DriveNumber];

        // Get Distance
        float Distance = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 4 });

        // Set start or end distance
        if (setStart)
        {
            DrvAssgn.posBegin = Distance;
        }
        else
        {
            DrvAssgn.posEnd = Distance;
        }

        return 0;
    }



    internal static int Lua_setInclination(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });

        // Get the GameObject
        GameObject obj = transformObj.gameObject;

        // Get the RWSpline component
        RWSpline spline = obj.GetComponent<RWSpline>();
        if (spline == null)
        {
            advancedAPIsCore.LogError($"GameObject '{obj.name}' exists but does NOT have a MeshRenderer!");
            return 0;
        }

        // assert the numbers (position of the DrvAssgn in the drives list of the spline and the inclination)
        float DriveNumber = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 });
        float Inclination = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });

        // fetch the DrvAssgn by the index
        RWDrvAssgn DrvAssgn = spline.drives[(int)DriveNumber];

        // set the inclination
        DrvAssgn.inclination = Inclination;

        return 0;
    }

    internal static int Lua_setSpeedScale0(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });

        // Get the GameObject
        GameObject obj = transformObj.gameObject;

        // Get the RWSpline component
        RWSpline spline = obj.GetComponent<RWSpline>();
        if (spline == null)
        {
            advancedAPIsCore.LogError($"GameObject '{obj.name}' exists but does NOT have a MeshRenderer!");
            return 0;
        }

        // assert the numbers (position of the DrvAssgn in the drives list of the spline and the inclination)
        float DriveNumber = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 });
        float Inclination = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });

        // fetch the DrvAssgn by the index
        RWDrvAssgn DrvAssgn = spline.drives[(int)DriveNumber];

        // set the inclination
        DrvAssgn.speedScale0 = Inclination;

        return 0;
    }

    internal static int Lua_setSpeedScale1(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });

        // Get the GameObject
        GameObject obj = transformObj.gameObject;

        // Get the RWSpline component
        RWSpline spline = obj.GetComponent<RWSpline>();
        if (spline == null)
        {
            advancedAPIsCore.LogError($"GameObject '{obj.name}' exists but does NOT have a MeshRenderer!");
            return 0;
        }

        // assert the numbers (position of the DrvAssgn in the drives list of the spline and the inclination)
        float DriveNumber = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 });
        float Inclination = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });

        // fetch the DrvAssgn by the index
        RWDrvAssgn DrvAssgn = spline.drives[(int)DriveNumber];

        // set the inclination
        DrvAssgn.speedScale1 = Inclination;

        return 0;
    }
}