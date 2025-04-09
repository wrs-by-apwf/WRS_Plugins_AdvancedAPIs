using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using UnityEngine;
using HarmonyLib;

[BepInPlugin("api.apwf.advancedAPIs", "Advanced APIs", "1.0.0")]
public class advancedAPIsCore : BaseUnityPlugin
{
    // my documents path + my games + "WinterResortSimulator_Season2"
    public static string WrsGamePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games/WinterResortSimulator_Season2/";
    
    /* NativeLua Reflection Methods */
    private static Type nativeLuaType;
    public static MethodInfo luaCS_createModuleMethod;
    public static MethodInfo luaCS_registerModuleFunctionMethod;
    public static MethodInfo luaCS_assertNumArgs;
    public static MethodInfo luaCS_getOptArg;
    public static MethodInfo luaCS_assertGetObjectFromLuaAPI;
    public static MethodInfo luaCS_assertGetTransformFromLuaAPI;
    public static MethodInfo luaCS_assertGetComponentFromLuaAPI;
    public static MethodInfo luaCS_assertGetComponentFromLuaAPIGeneric;
    public static MethodInfo luaCS_pushObjectFromLuaAPI;
    public static MethodInfo lua_pushnil;
    public static MethodInfo lua_pushnumber;
    public static MethodInfo lua_pushinteger;
    public static MethodInfo lua_pushstring;
    public static MethodInfo luaCS_assertGetString;
    public static MethodInfo luaCS_assertGetNumber;
    public static MethodInfo luaCS_assertGetBoolean;
    public static MethodInfo luaCS_assertGetInteger;
    public static MethodInfo lua_getglobal;
    public static MethodInfo lua_isnil;
    public static MethodInfo lua_pop;
    
    /* LuaAPI Reflection Methods */
    private static Type LuaAPIType;
    public static MethodInfo GetObjectId;
    
    // save this instance to be able to work with the logger & bepinhex
    public static advancedAPIsCore _instance;
        
    // create a registry of the other .cs scripts to call the methods
    private static readonly Type[] _modules = new Type[]
    {
        // Lua APIs
        typeof(Lua_AdvancedMaterial),
        typeof(Lua_AdvancedDrvAssgn),
        typeof(Lua_AdvancedRWCarrier),
    };

    public void Awake()
    {
        // initialize the update root 
        rootUpdate.Start();
        
        // apply the instance so static method can use the logger
        _instance = this;

        // apply manually the harmony patch 
        var manualharmony = new Harmony("api.apwf.materials");
        var StartNewLuaVMoriginal = AccessTools.Method(typeof(HR.Lua.LuaAPI), "StartNewLuaVM", new Type[] { typeof(bool) });
        var StartNewLuaVMpostfix = AccessTools.Method(typeof(advancedAPIsCore), "StartNewLuaVM_Postfix");

        if (StartNewLuaVMoriginal == null)
        {
            Logger.LogError("StartNewLuaVM method not found for manual patching.");
        }
        else
        {
            manualharmony.Patch(StartNewLuaVMoriginal, postfix: new HarmonyMethod(StartNewLuaVMpostfix));
            Logger.LogInfo("Manual patch applied to StartNewLuaVM.");
        }

        // log all the manual patched methods from the plugin
        var manualPatchedMethods = manualharmony.GetPatchedMethods();
        foreach (var method in manualPatchedMethods)
        {
            Logger.LogInfo($"Patched method: {method.DeclaringType}.{method.Name}");
        }

        // log all the patched methods from all the plugins
        var allPatchedMethods = Harmony.GetAllPatchedMethods();
        foreach (var method in allPatchedMethods)
        {
            Logger.LogInfo($"[ALL] Patched method: {method.DeclaringType}.{method.Name}");
        }


        // start to log the plugin loaded
        Logger.LogInfo("Advanced APIs Loaded!");

        // Use reflection to get access to internal NativeLua methods
        nativeLuaType = Type.GetType("HR.Lua.NativeLua, Assembly-CSharp");
        LuaAPIType = Type.GetType("HR.Lua.LuaAPI, Assembly-CSharp");

        if (nativeLuaType == null)
        {
            // Log an error if the type is not found
            Logger.LogError("Failed to find NativeLua type.");
            return;
        }
        
        if (LuaAPIType == null)
        {
            // Log an error if the type is not found
            Logger.LogError("Failed to find LuaAPI type.");
            return;
        }

        // Get the methods using reflection
        luaCS_createModuleMethod =
            nativeLuaType.GetMethod("luaCS_createModule", BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_registerModuleFunctionMethod = nativeLuaType.GetMethod("luaCS_registerModuleFunction",
            BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetObjectFromLuaAPI = nativeLuaType.GetMethod("luaCS_assertGetObjectFromLuaAPI",
            BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetTransformFromLuaAPI = nativeLuaType.GetMethod("luaCS_assertGetTransformFromLuaAPI",
            BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetComponentFromLuaAPI = nativeLuaType.GetMethod("luaCS_assertGetComponentFromLuaAPI",
            BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_pushObjectFromLuaAPI = nativeLuaType.GetMethod("luaCS_pushObjectFromLuaAPI",
            BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_getOptArg = nativeLuaType.GetMethod("luaCS_getOptArg", BindingFlags.NonPublic | BindingFlags.Static);
        lua_pushnumber = nativeLuaType.GetMethod("lua_pushnumber", BindingFlags.NonPublic | BindingFlags.Static);
        lua_pushinteger = nativeLuaType.GetMethod("lua_pushinteger", BindingFlags.NonPublic | BindingFlags.Static);
        lua_pushnil = nativeLuaType.GetMethod("lua_pushnil", BindingFlags.NonPublic | BindingFlags.Static);
        lua_pushstring = nativeLuaType.GetMethod("lua_pushstring", BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertNumArgs =
            nativeLuaType.GetMethod("luaCS_assertNumArgs", BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetString =
            nativeLuaType.GetMethod("luaCS_assertGetString", BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetNumber =
            nativeLuaType.GetMethod("luaCS_assertGetNumber", BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetBoolean =
            nativeLuaType.GetMethod("luaCS_assertGetBoolean", BindingFlags.NonPublic | BindingFlags.Static);
        luaCS_assertGetInteger =
            nativeLuaType.GetMethod("luaCS_assertGetInteger", BindingFlags.NonPublic | BindingFlags.Static);
        lua_getglobal = nativeLuaType.GetMethod("lua_getglobal", BindingFlags.NonPublic | BindingFlags.Static);
        lua_isnil = nativeLuaType.GetMethod("lua_isnil", BindingFlags.NonPublic | BindingFlags.Static);
        lua_pop = nativeLuaType.GetMethod("lua_pop", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo StartNewLuaVM =
            typeof(HR.Lua.LuaAPI).GetMethod("StartNewLuaVM", BindingFlags.NonPublic | BindingFlags.Static);

        // make sure the method is not null
        if (StartNewLuaVM == null)
        {
            Logger.LogError("Failed to find StartNewLuaVM method.");
            return;
        }
        else
        {
            Logger.LogInfo("StartNewLuaVM method found.");
        }


        // Log an error if the methods are not found
        if (luaCS_createModuleMethod == null || luaCS_registerModuleFunctionMethod == null ||
            luaCS_assertGetObjectFromLuaAPI == null || luaCS_assertNumArgs == null ||
            luaCS_assertGetString == null || luaCS_assertGetNumber == null)
        {
            Logger.LogError("Failed to find methods in NativeLua.");
            return;
        }

        // Log that the reflection setup is complete
        Logger.LogInfo("Reflection setup for NativeLua complete.");
        
        
        // call all the modules of the plugin with the same method name ("Awake")
        foreach (var module in _modules)
        {
            MethodInfo awakeMethod = module.GetMethod("Awake", BindingFlags.Public | BindingFlags.Static);
            if (awakeMethod != null)
            {
                awakeMethod.Invoke(null, null);
            }
        }
        
    }

    private void Update()
    {
        foreach (var module in _modules)
        {
            MethodInfo awakeMethod = module.GetMethod("Update", BindingFlags.Public | BindingFlags.Static);
            if (awakeMethod != null)
            {
                awakeMethod.Invoke(null, null);
            }
        }
    }


    public static void StartNewLuaVM_Postfix(bool isReboot = false)
    
    {
        if (_instance == null)
        {
            Debug.LogError("Instance of AdvancedAPIsCore is null!");
            return;
        }

        _instance.Logger.LogInfo("Postfix for StartNewLuaVM called");

        // Use reflection to get the new Lua state (_L)
        FieldInfo luaStateField = typeof(HR.Lua.LuaAPI).GetField("_L", BindingFlags.NonPublic | BindingFlags.Static);

        if (luaStateField == null)
        {
            _instance.Logger.LogError("Failed to retrieve HR.Lua.LuaAPI._L field!");
            return;
        }

        IntPtr luaState = (IntPtr)luaStateField.GetValue(null); // Get the actual value of _L

        if (luaState == IntPtr.Zero)
        {
            _instance.Logger.LogError("Lua state (_L) is NULL! Cannot continue.");
            return;
        }

        _instance.Logger.LogInfo($"Lua state (_L) obtained via reflection: {luaState}");
        
        Setup(luaState);
    }

    private static void Setup(IntPtr L)
    {
        // Log that the setup is starting
        _instance.Logger.LogInfo("Advanced APIs Core Start Setup");
        
        // Now call the setup function for all the modules with the new Lua state
        foreach (var module in _modules)
        {
            MethodInfo awakeMethod = module.GetMethod("Setup", BindingFlags.Public | BindingFlags.Static);
            if (awakeMethod != null)
            {
                _instance.Logger.LogInfo( "Calling Setup for " + module.Name);
                awakeMethod.Invoke(null, new object[] { L });
            }
        }

        // log that the setup is complete
        _instance.Logger.LogInfo("Advanced APIs Core End Setup");
    }

    public static void InvokeCreateModule(IntPtr L, string moduleName)
    {
        // Check if the method is null
        if (luaCS_createModuleMethod == null)
        {
            _instance.Logger.LogError("luaCS_createModuleMethod is NULL! Cannot register function.");
            return;
        }

        try
        {
            // Use reflection to call luaCS_createModule
            luaCS_createModuleMethod.Invoke(null, new object[] { L, moduleName });
        }
        catch (Exception e)
        {
            // Log any exceptions that occur
            _instance.Logger.LogError($"Failed to create module: {moduleName}. Exception: {e}");
        }

        // Log that the module has been created
        _instance.Logger.LogInfo($"Module: {moduleName} has been created");


        // now verify that the module has been created
        lua_getglobal.Invoke(null, new object[] { L, moduleName });

        bool exists = !(bool)lua_isnil.Invoke(null, new object[] { L, -1 });

        lua_pop.Invoke(null, new object[] { L, 1 });

        if (!exists)
        {
            _instance.Logger.LogError($"❌ Module {moduleName} is missing after creation !");
        }
        
    }


    public static void RegisterModuleFunction(IntPtr L, string module, string functionName, Delegate functionDelegate)
    {

        // Check if the method is null
        if (luaCS_registerModuleFunctionMethod == null)
        {
            _instance.Logger.LogError("luaCS_registerModuleFunctionMethod is NULL! Cannot register function.");
            return;
        }

        try
        {
            // Use reflection to call luaCS_registerModuleFunction
            luaCS_registerModuleFunctionMethod.Invoke(null, new object[] { L, module, functionName, functionDelegate });
        }
        catch (Exception e)
        {
            // Log any exceptions that occur
            _instance.Logger.LogError($"Failed to register function: {functionName} in module: {module}. Exception: {e}");
        }
    }

    
    /*
     * Logging Methods 
     */
    
    public static void LogInfo(string message)
    {
        _instance.Logger.LogInfo(message);
    }
    
    public static void LogError(string message)
    {
        _instance.Logger.LogError(message);
    }
    
    public static void LogWarning(string message)
    {
        _instance.Logger.LogWarning(message);
    }
}