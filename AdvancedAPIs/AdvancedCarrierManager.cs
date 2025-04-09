using HR.Lua;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedCarrierManager : MonoBehaviour
{
    public static AdvancedCarrierManager g_inst;
    private List<AdvancedRWCarrier> carriers = new List<AdvancedRWCarrier>();

    private void Update()
    {
        for (int index = 0; index < carriers.Count; ++index)
            carriers[index].UpdateCarrier();
    }

    public void AddCarrier(AdvancedRWCarrier carrier) => carriers.Add(carrier);

    public void RemoveCarrier(AdvancedRWCarrier carrier) => carriers.Remove(carrier);
    
    private void Awake()
    {
        if ((bool) (Object) g_inst)
            LuaAPI.WriteToLog("Error: Two or more objects of class AdvancedCarrierManager detected!");
        else
            g_inst = this;
    }

    private void OnDestroy()
    {
        if (!(g_inst == this))
            return;
        g_inst = null;
    }
}