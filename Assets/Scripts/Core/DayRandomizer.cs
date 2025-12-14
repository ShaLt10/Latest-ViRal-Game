using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FunkyCode;
public class DayRandomizer : MonoBehaviour
{
    LightCycle Light;
    private void Start()
    {
        Light = GetComponent<LightCycle>();
        Light.SetTime(Random.Range(0f, 1f));
    }
}
