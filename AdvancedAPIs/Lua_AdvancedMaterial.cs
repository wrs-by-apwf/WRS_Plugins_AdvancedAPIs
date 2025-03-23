using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public class Lua_AdvancedMaterial
{
    private static string CustomLogosPath = advancedAPIsCore.WrsGamePath + "/CustomLogos";

    // call by the core class after the core is initialized (Awake method in core)
    public static void Awake()
    {
        // the module allow for custom logo on the carrier so we need to create the folder for the logos if it does not exist
        if (!Directory.Exists(CustomLogosPath))
        {
            advancedAPIsCore.LogInfo("Creating Custom Logos folder");
            Directory.CreateDirectory(CustomLogosPath);
        }
    }

    // call by the core class after the core is updated (Update method in core)
    public static void Update()
    {
        
    }

    // called by the core class after the core Setup is invoked
    public static void Setup(IntPtr L)
    {
        // Call the methods using reflection
        advancedAPIsCore.InvokeCreateModule(L, "AMaterial");
        advancedAPIsCore.RegisterModuleFunction(L, "AMaterial", "setVector", new Func<IntPtr, int>(Lua_setMaterialVector));
        advancedAPIsCore.RegisterModuleFunction(L, "AMaterial", "setColor", new Func<IntPtr, int>(Lua_setMaterialColor));
        advancedAPIsCore.RegisterModuleFunction(L, "AMaterial", "setFloat", new Func<IntPtr, int>(Lua_setMaterialFloat));
        
        // custom logo api 
        advancedAPIsCore.RegisterModuleFunction(L, "AMaterial", "getTexturesList", new Func<IntPtr, int>(Lua_getTexturesList));
        advancedAPIsCore.RegisterModuleFunction(L, "AMaterial", "setCustomTexture", new Func<IntPtr, int>(Lua_setCustomTexture));

        // log that the setup is complete
        advancedAPIsCore.LogInfo("AMaterial APIs have been added");
    }
    internal static int Lua_setMaterialVector(IntPtr L)
    {
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 6 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });
        
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
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2 });

        // assert the numbers
        byte integer1 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 3 });
        byte integer2 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 4 });
        byte integer3 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 5 });
        byte integer4 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 6 });

        // combine the numbers into a color
        Vector4 vec = new Vector4(integer1, integer2, integer3, integer4);

        // Set the float value
        ObjectMaterial.SetColor(str, vec);

        return 0;
    }
    internal static int Lua_setMaterialColor(IntPtr L)
    {
        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 6 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });
        
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
        string str = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2 });

        // assert the numbers
        byte integer1 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 3 });
        byte integer2 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 4 });
        byte integer3 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 5 });
        byte integer4 = (byte)advancedAPIsCore.luaCS_assertGetInteger.Invoke(null, new object[] { L, 6 });

        // combine the numbers into a color
        Color color = new Color32(integer1, integer2, integer3, integer4);

        // Set the float value
        ObjectMaterial.SetColor(str, color);

        return 0;
    }
    internal static int Lua_setMaterialFloat(IntPtr L)
    {

        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });
        
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
    
    internal static int Lua_getTexturesList(IntPtr L)
    {
        advancedAPIsCore.LogInfo("ask for the list of textures");
        
        // get all the filenames for the files that end with .png in the CustomLogos folder, compile them into a string and return it
        string[] files = Directory.GetFiles(CustomLogosPath, "*.png");
        string result = "";
        foreach (string file in files)
        {
            result += Path.GetFileName(file) + ",\n";
        }
        
        // log the result
        advancedAPIsCore.LogInfo(result);
        
        // push the string to the lua stack
        advancedAPIsCore.lua_pushstring.Invoke(null, new object[] { L, result });
        
        return 1;
    }
    internal static int Lua_setCustomTexture(IntPtr L)
    {

        // assert the number of arguments
        advancedAPIsCore.luaCS_assertNumArgs.Invoke(null, new object[] { L, 3 });

        // get the transform object bind to the id in the lua stack
        Transform transformObj = (Transform)advancedAPIsCore.luaCS_assertGetTransformFromLuaAPI.Invoke(null, new object[] { L, 1, false });
        
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
        string filename = (string)advancedAPIsCore.luaCS_assertGetString.Invoke(null, new object[] { L, 2, false });

        // if the filename extension is not .png then we refuse to do anything
        if (!filename.EndsWith(".png"))
        {
            advancedAPIsCore.LogError("The texture file must be a .png file");
            return 0;
        }
        
        // check if the file exists
        string path = CustomLogosPath + "/" + filename;
        
        if (!File.Exists(path))
        {
            advancedAPIsCore.LogError($"The file '{filename}' does not exist in the CustomLogos folder");
            return 0;
        }
        
        // fetch the size of the texture file
        FileInfo fi = new FileInfo(path);
        long size = fi.Length;
        
        // Load the texture 
        Texture2D texture = new Texture2D((int)size, (int)size, TextureFormat.RGBA32, false);
        byte[] fileData = File.ReadAllBytes(path);
        texture.LoadRawTextureData(fileData);
        texture.Apply();
        
        // Set the texture to the material
        ObjectMaterial.SetTexture("_MainTex", texture);

        return 0;
    }
    
    
}