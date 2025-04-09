using System;
using System.Reflection;
using UnityEngine;

public class Lua_AdvancedRWCarrier
{
    private static Type nativeLuaType;
    private static bool _checkForDeletion = false;

    // call by the core class after the core is initialized (Awake method in core)
    void Awake()
    {
    }

    private void Update()
    {
    }

    // called by the core class after the core Setup is invoked
    private static void Setup(IntPtr L)
    {
        // Call the methods using reflection
        advancedAPIsCore.InvokeCreateModule(L, "ARWCarrier");
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "registerCarrier",
            new Func<IntPtr, int>(Lua_registerCarrier));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "setParameters",
            new Func<IntPtr, int>(Lua_setCarrierParameters));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "setDampingCoeff",
            new Func<IntPtr, int>(Lua_setCarrierDampingCoeff));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "addPassengerSeat",
            new Func<IntPtr, int>(Lua_carrierAddSeat));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "removePassengerSeats",
            new Func<IntPtr, int>(Lua_carrierRemoveSeats));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getPassengerCount",
            new Func<IntPtr, int>(Lua_carrierGetPassengerCount));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getPassengerProbability",
            new Func<IntPtr, int>(Lua_carrierGetPassengerProbability));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "despawnPassengers",
            new Func<IntPtr, int>(Lua_carrierDespawnPassengers));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "addAnimatedPart",
            new Func<IntPtr, int>(Lua_addCarrierAnimatedPart));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "stopClosestCarrier",
            new Func<IntPtr, int>(Lua_stopClosestCarrier));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "pushClosestCarrier",
            new Func<IntPtr, int>(Lua_pushClosestCarrier));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "teleport",
            new Func<IntPtr, int>(Lua_teleportCarrier));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getSpeed",
            new Func<IntPtr, int>(Lua_getCarrierSpeed));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getTotalMass",
            new Func<IntPtr, int>(Lua_getTotalCarrierMass));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "setSpeed", new Func<IntPtr, int>(Lua_setCarrierSpeed));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getSpline",
            new Func<IntPtr, int>(Lua_getCarrierSpline));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getPosition",
            new Func<IntPtr, int>(Lua_getCarrierPosition));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "addCarrierPart",
            new Func<IntPtr, int>(Lua_addCarrierPart));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "getCarrierPartPosition",
            new Func<IntPtr, int>(Lua_getCarrierPartPosition));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "setCarrierPartPosition",
            new Func<IntPtr, int>(Lua_setCarrierPartPosition));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "overrideCarrierPartPosition",
            new Func<IntPtr, int>(Lua_overrideCarrierPartPosition));
        advancedAPIsCore.RegisterModuleFunction(L, "ARWCarrier", "setInvertZOrientation",
            new Func<IntPtr, int>(Lua_setInvertZOrientation));

        // log that the setup is complete
        advancedAPIsCore.LogInfo("ARWCarrier APIs have been added");
    }

    internal static int Lua_setMaterialFloat(IntPtr L)
    {
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj =
            (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });

        // Get the GameObject
        GameObject obj = transformObj.gameObject;

        // Get the MeshRenderer
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            advancedAPIsCore.LogError($"GameObject '{obj.name}' exists but does NOT have a MeshRenderer!");
            return 0;
        }

        // Get the Material
        Material ObjectMaterial = mr.sharedMaterial;

        // assert the string
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2, false });

        // assert the number
        float number = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });

        // Cast the number to double
        double num = number;

        // Set the float value
        if (ObjectMaterial != null && !string.IsNullOrEmpty(str) && num != 0.0)
        {
            ObjectMaterial.SetFloat(str, (float)num);
        }

        return 0;
    }

    internal static int Lua_registerCarrier(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        Transform transformFromLuaApi =
            (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });
        AdvancedRWSpline componentFromLuaApi =
            (AdvancedRWSpline)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 2, typeof(AdvancedRWSpline) });
        float number = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        AdvancedRWCarrier AdvancedRWCarrier = transformFromLuaApi.GetComponent<AdvancedRWCarrier>();
        if (AdvancedRWCarrier == null)
        {
            AdvancedRWCarrier = transformFromLuaApi.gameObject.AddComponent<AdvancedRWCarrier>();
            AdvancedRWCarrier.carrierInstances.Add(AdvancedRWCarrier);
        }

        AdvancedRWCarrier.SetInitialPosition(componentFromLuaApi, number);
        return 0;
    }

    internal static int Lua_setCarrierParameters(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 4 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier componentFromLuaApi =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null,
                new object[] { L, 1, typeof(AdvancedRWCarrier) });
        Transform transformFromLuaApi =
            (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 2, false });
        float number1 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        float number2 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 4 });
        componentFromLuaApi.InvCOGRadius = 1f / number1;
        componentFromLuaApi.length = number2;
        componentFromLuaApi.rotX = transformFromLuaApi;
        return 0;
    }

    internal static int Lua_setCarrierDampingCoeff(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 2 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier = (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null,
            new object[] { L, 1, typeof(AdvancedRWCarrier) });
        carrier.dampingCoeffX = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 });
        return 0;
    }

    internal static int Lua_carrierAddSeat(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier componentFromLuaApi =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        Transform transformFromLuaApi =
            (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 2, false });
        string trigger = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 3, false });
        bool skis = !(bool)advancedAPIsCore.luaCS_getOptArg.Invoke(null, new object[] { L, 4 }) ||
                    (bool)advancedAPIsCore.luaCS_assertGetBoolean.Invoke(null, new object[] { L, 4 });
        bool sticks = (bool)advancedAPIsCore.luaCS_getOptArg.Invoke(null, new object[] { L, 5 }) &&
                      (bool)advancedAPIsCore.luaCS_assertGetBoolean.Invoke(null, new object[] { L, 5 });
        int length = componentFromLuaApi.passengerSeats.Length;
        Array.Resize(ref componentFromLuaApi.passengerSeats, length + 1);
        Array.Resize(ref componentFromLuaApi.passengers, length + 1);
        componentFromLuaApi.passengerSeats[length] =
            new RWCarrier.CarrierSeat(transformFromLuaApi, trigger, skis, sticks);
        componentFromLuaApi.passengerProbabilities = new float[componentFromLuaApi.passengerSeats.Length];
        return 0;
    }

    internal static int Lua_getCarrierSpeed(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        advancedAPIsCore.lua_pushnumber.Invoke(null, new object[] { L, carrier.GetSpeed() });
        return 1;
    }

    internal static int Lua_setCarrierSpeed(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 2 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        carrier.SetSpeed((float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 }));
        return 0;
    }


    internal static int Lua_setInvertZOrientation(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 2 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        carrier.invertZOrientation = (bool)advancedAPIsCore.luaCS_assertGetBoolean.Invoke(null, new object[] { L, 2 });
        return 0;
    }

    internal static int Lua_carrierRemoveSeats(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });

        Array.Resize(ref carrier.passengerSeats, 0);
        Array.Resize(ref carrier.passengers, 0);
        carrier.passengerProbabilities = new float[0];
        return 0;
    }

    internal static int Lua_carrierGetPassengerCount(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        advancedAPIsCore.lua_pushinteger.Invoke(null,
            new object[] { L, (int)Mathf.Floor(carrier.GetPassengerCount() + 0.5f) });
        return 1;
    }

    internal static int Lua_carrierGetPassengerProbability(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });

        float n = 0.0f;
        for (int index = 0; index < carrier.passengerProbabilities.Length; ++index)
        {
            n += carrier.passengerProbabilities[index];
        }

        advancedAPIsCore.lua_pushnumber.Invoke(null, new object[] { L, (double)n });
        return 1;
    }

    internal static int Lua_carrierDespawnPassengers(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        carrier.DespawnPassengers(true);
        return 0;
    }

    internal static int Lua_addCarrierAnimatedPart(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        Transform transformFromLuaApi =
            (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 2, false });
        float number = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        int integer = (int)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 4 });
        if (carrier != null && transformFromLuaApi != null)
        {
            carrier.AddAnimatedPart(transformFromLuaApi, number, integer);
        }

        return 0;
    }

    private static bool ValidateCarrier(AdvancedRWCarrier carrier)
    {
        return carrier != null && !carrier.fixedSpeed;
    }

    internal static int Lua_stopClosestCarrier(IntPtr L)
    {
        advancedAPIsCore.lua_pushinteger.Invoke(null, new object[] { L, 1 });
        double number1 = (double)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 1 });
        float number2 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 });
        float number3 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        float number4 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 4 });
        float range = 3.5f;
        if ((bool)advancedAPIsCore.luaCS_getOptArg.Invoke(null, new object[] { L, 5 }))
        {
            range = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 5 });
        }

        double y = number2;
        double z = number3;

        AdvancedRWCarrier closestCarrier =
            AdvancedRWCarrier.GetClosestCarrier(new Vector3((float)number1, (float)y, (float)z), range, (ValidateCarrier));
        if (!ValidateCarrier(closestCarrier))
        {
            advancedAPIsCore.lua_pushnil.Invoke(null, new object[] { L });
            return 1;
        }

        float speed = closestCarrier.GetSpeed();
        closestCarrier.SetSpeed(Mathf.MoveTowards(speed, 0.0f, number4));

        // before using luaCS_pushObjectFromLuaAPI we must make it like componentFromLuaApi (generic)
        MethodInfo luaCS_pushObjectFromLuaAPI =
            advancedAPIsCore.luaCS_pushObjectFromLuaAPI.MakeGenericMethod(typeof(Transform));
        return (int)luaCS_pushObjectFromLuaAPI.Invoke(null, new object[] { L, closestCarrier?.transform });
    }

    internal static int Lua_pushClosestCarrier(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 4 });

        double number1 = (double)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 1 });
        double number2 = (double)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 2 });
        double number3 = (double)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        double number4 = (double)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 4 });
        float range = 3.5f;
        if ((bool)advancedAPIsCore.luaCS_getOptArg.Invoke(null, new object[] { L, 5 }))
        {
            range = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 5 });
        }

        float max = 1.5f;
        if ((bool)advancedAPIsCore.luaCS_getOptArg.Invoke(null, new object[] { L, 6 }))
        {
            max = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 6 });
        }

        AdvancedRWCarrier closestCarrier =
            AdvancedRWCarrier.GetClosestCarrier(new Vector3((float)number1, (float)number2, (float)number3), range,
                (ValidateCarrier));
        if (!ValidateCarrier(closestCarrier))
        {
            advancedAPIsCore.lua_pushnil.Invoke(null, new object[] { L });
            return 1;
        }

        Vector3 vector3 =
            closestCarrier.transform.InverseTransformPoint(new Vector3((float)number1, (float)number2, (float)number3));
        float num = vector3.z / new Vector2(vector3.x, vector3.z).magnitude;
        if (closestCarrier.invertZOrientation)
        {
            num *= -1f;
        }

        float speed = Mathf.Clamp(closestCarrier.GetSpeed() - num * (float)number4, -max, max);
        closestCarrier.SetSpeed(speed);

        // before using luaCS_pushObjectFromLuaAPI we must make it like componentFromLuaApi (generic)
        MethodInfo luaCS_pushObjectFromLuaAPI =
            advancedAPIsCore.luaCS_pushObjectFromLuaAPI.MakeGenericMethod(typeof(Transform));

        return (int)luaCS_pushObjectFromLuaAPI.Invoke(null, new object[] { L, closestCarrier?.transform });
    }

    internal static int Lua_teleportCarrier(IntPtr L)
    {
        advancedAPIsCore.lua_pushinteger.Invoke(null, new object[] { L, 3 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWSpline
        var luaCS_assertGetComponentFromLuaAPI2 =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWSpline));

        AdvancedRWSpline spline =
            (AdvancedRWSpline)luaCS_assertGetComponentFromLuaAPI2.Invoke(null, new object[] { L, 2, typeof(AdvancedRWSpline) });

        float number = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        carrier.TeleportTo(spline, number);
        return 0;
    }

    internal static int Lua_getCarrierSpline(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });


        advancedAPIsCore.lua_pushinteger.Invoke(null,
            new object[]
                { L, advancedAPIsCore.GetObjectId.Invoke(null, new object[] { carrier.GetSpline().transform }) });

        return 1;
    }

    internal static int Lua_getCarrierPosition(IntPtr L)
    {
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });

        advancedAPIsCore.lua_pushnumber.Invoke(null, new object[] { L, carrier.GetPosition() });

        return 1;
    }

    internal static int Lua_addCarrierPart(IntPtr L)
    {
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        // get the transform object bind to the id in the lua stack
        Transform transformFromLuaApi =
            (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });

        // get the AdvancedRWCarrier component from the transform
        AdvancedRWCarrier componentFromLuaApi =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 2, typeof(AdvancedRWCarrier) });

        // get the Animation component from the transform
        Animation componentFromLuaApi2 =
            (Animation)advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 3, typeof(Animation) });

        // get the string from the lua stack
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 4, false });

        // create a new AnimatedCarrierPart object
        RWCarrier.AnimatedCarrierPart part = new RWCarrier.AnimatedCarrierPart()
        {
            animation = componentFromLuaApi2,
            name = str,
            position = 0.0f,
            targetPosition = 0.0f
        };

        // add the AnimatedCarrierPart to the AdvancedRWCarrier
        componentFromLuaApi.AddCarrierPart(part);

        // return 0 to indicate success
        return 0;
    }

    internal static int Lua_getTotalCarrierMass(IntPtr L)
    {
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 1 });
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));

        AdvancedRWCarrier carrier = (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });

        advancedAPIsCore.lua_pushnumber.Invoke(null, new object[] { L, carrier.currentMass });
        return 1;
    }

    internal static int Lua_getCarrierPartPosition(IntPtr L)
    {
        
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 2 });
        
        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));
        
        // get the AdvancedRWCarrier component from the transform
        AdvancedRWCarrier componentFromLuaApi =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        
        // get the string from the lua stack
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2, false });
        
        // get the AnimatedCarrierPart from the AdvancedRWCarrier
        RWCarrier.AnimatedCarrierPart carrierPart = componentFromLuaApi.GetCarrierPart(str);
        
        // if the carrierPart is not null, push its position and targetPosition to the lua stack
        if (carrierPart != null)
        {
            advancedAPIsCore.lua_pushnumber.Invoke(null, new object[] { L, carrierPart.position });
            advancedAPIsCore.lua_pushnumber.Invoke(null, new object[] { L, carrierPart.targetPosition });
        }
        else
        {
            // if the carrierPart is null, push nil to the lua stack
            advancedAPIsCore.lua_pushnil.Invoke(null, new object[] { L });
            advancedAPIsCore.lua_pushnil.Invoke(null, new object[] { L });
        }
        
        // return 2 (two element given to the lua stack)
        return 2;
    }

    internal static int Lua_setCarrierPartPosition(IntPtr L)
    {
        
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 4 });
        
        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));
        
        // get the AdvancedRWCarrier component from the transform
        AdvancedRWCarrier componentFromLuaApi =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        
        // get the string from the lua stack
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2, false });
        // get the float from the lua stack
        float number = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        
        // get the int from the lua stack
        int integer = (int)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 4 });
        
        // set the carrier part position
        componentFromLuaApi.SetCarrierPartPosition(str, number, integer);
        
        // return 0 (no elements given to the lua stack)
        return 0;
        
    }

    internal static int Lua_overrideCarrierPartPosition(IntPtr L)
    {
        
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 4 });
        
        // first transform "luaCS_assertGetComponentFromLuaAPI" into a generic method that cast the component to AdvancedRWCarrier
        var luaCS_assertGetComponentFromLuaAPI =
            advancedAPIsCore.luaCS_assertGetComponentFromLuaAPI.MakeGenericMethod(typeof(AdvancedRWCarrier));
        
        // get the AdvancedRWCarrier component from the transform
        AdvancedRWCarrier componentFromLuaApi =
            (AdvancedRWCarrier)luaCS_assertGetComponentFromLuaAPI.Invoke(null, new object[] { L, 1, typeof(AdvancedRWCarrier) });
        
        // get the string from the lua stack
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2, false });
        
        // get the float from the lua stack
        float number1 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 3 });
        
        // get the float from the lua stack
        float number2 = (float)advancedAPIsCore.luaCS_assertGetNumber.Invoke(null, new object[] { L, 4 });
        
        // set the carrier part position
        componentFromLuaApi.GetCarrierPart(str)?.SetPositionOverride(number1, number2);
        
        // return 0 (no elements given to the lua stack)
        return 0;
    }
}