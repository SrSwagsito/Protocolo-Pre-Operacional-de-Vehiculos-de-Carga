using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el zoom suave de la cámara ortográfica.
/// Asignar en el Inspector: targetCamera (o se toma Camera.main automáticamente).
/// Llamar ZoomOut() desde Module4Manager al completar la Fase 1.
/// </summary>
public class CameraTransitionController : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineVirtualCamera stackingCamera;
    public CinemachineVirtualCamera ropeCamera;

    /// <summary>Llamar desde Module4Manager al completar Fase 1.</summary>
    public void TransitionToRopeView()
    {
        stackingCamera.Priority = 0;
        ropeCamera.Priority = 10;
        // CinemachineBrain ejecuta el blend automáticamente
    }

    public void ResetToStackView()
    {
        ropeCamera.Priority = 0;
        stackingCamera.Priority = 10;
    }
}
