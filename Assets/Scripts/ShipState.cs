using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainState : MonoBehaviour
{
    public int totalWattsAvailable;
    private int currentWattsAvailable;

    enum Module
    {
        thrusters = 0,
        jump = 1,

        torpedo = 2,
        beam = 3,

        passiveRadar = 4,
        activeRadar = 5,
    }

    private Dictionary<Module, int> ModuleWatts = new Dictionary<Module, int>()
    {
        { Module.thrusters, 3 },
        { Module.jump, 2 },
        { Module.torpedo, 2 },
        { Module.beam, 0 },
        { Module.passiveRadar, 2 },
        { Module.activeRadar, 2 },
    };

    private Dictionary<Module, int> ModuleSafeWatts = new Dictionary<Module, int>()
    {
        { Module.thrusters, 4 },
        { Module.jump, 4 },
        { Module.torpedo, 6 },
        { Module.beam, 6 },
        { Module.passiveRadar, 4 },
        { Module.activeRadar, 4 },
    };

    private Dictionary<Module, float> ModuleJules = new Dictionary<Module, float>()
    {
        { Module.jump, 0f },
        { Module.torpedo, 0f },
        { Module.beam, 0f },
    };

    // Start is called before the first frame update
    void Start()
    {
        currentWattsAvailable = GetCurrentWattsAvaiable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    int GetCurrentWattsAvaiable()
    {
        int avaiable = totalWattsAvailable;
        foreach (KeyValuePair<Module, int> module in ModuleWatts)
        {
            totalWattsAvailable -= module.Value;
        }
        return avaiable;
    }
}
