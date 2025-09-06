using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraEnding : MonoBehaviour
{
    public CinemachineVirtualCamera cam;
    public Transform target;
    public void UpdateTarget()
    {
        cam.Follow = target;
    }
}
