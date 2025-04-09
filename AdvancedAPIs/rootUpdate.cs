using UnityEngine;


public class rootUpdate : MonoBehaviour
{
    public static rootUpdate g_inst;
    
    public static void Start()
    {
        if (g_inst == null)
        {
            GameObject go = new GameObject("AdvancedCSharpRootUpdate");
            g_inst = go.AddComponent<rootUpdate>();
        }
    }
    
    public void LateUpdate()
    {
        AdvancedRWCarrier.GlobalLateUpdate();
    }
}