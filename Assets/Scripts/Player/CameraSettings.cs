using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSettings : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera zoomOutCam;
    [SerializeField] CinemachineVirtualCamera zoomInCam;

    [Header("Sensitivities")]
    [SerializeField] [Range(10f, 1000f)] float zoomedOutSensitivity = 250f;
    [SerializeField] [Range(10f, 1000f)] float zoomedInSensitivity = 150f;
    [SerializeField] [Range(1f, 3f)] float verticalSensitivityDivisor = 1f;

    [Header("Invert Aim")]
    [SerializeField] bool zoomedOutXInvert = false;
    [SerializeField] bool zoomedOutYInvert = true;
    [SerializeField] bool zoomedInXInvert = false;
    [SerializeField] bool zoomedInYInvert = true;

    CinemachineTransposer transposer;

    CinemachinePOV pov;

    int playerNum;

    // Start is called before the first frame update
    void Start()
    {
        playerNum = PlayerManager.instance.GetIndex(gameObject) + 1;
    }

        // Update is called once per frame
        void FixedUpdate()
    {
        if (playerNum == 1)
        {
            zoomedInSensitivity = StateVariables.p1zoomedInSens;
            zoomedOutSensitivity = StateVariables.p1zoomedOutSens;
        }

        else if (playerNum == 2)
        {
            zoomedInSensitivity = StateVariables.p2zoomedInSens;
            zoomedOutSensitivity = StateVariables.p2zoomedOutSens;
        }

        else if (playerNum == 3)
        {
            zoomedInSensitivity = StateVariables.p3zoomedInSens;
            zoomedOutSensitivity = StateVariables.p3zoomedOutSens;
        }

        else if (playerNum == 4)
        {
            zoomedInSensitivity = StateVariables.p4zoomedInSens;
            zoomedOutSensitivity = StateVariables.p4zoomedOutSens;
        }

        zoomOutCam.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = zoomedOutSensitivity / verticalSensitivityDivisor;
        zoomOutCam.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = zoomedOutSensitivity;
        zoomOutCam.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InvertInput = zoomedOutXInvert;
        zoomOutCam.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InvertInput = zoomedOutYInvert;

        zoomInCam.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = zoomedInSensitivity / verticalSensitivityDivisor;
        zoomInCam.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = zoomedInSensitivity;
        zoomInCam.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InvertInput = zoomedInXInvert;
        zoomInCam.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InvertInput = zoomedInYInvert;
    }

    public void SetZoomedOutSensitivity(float sensitivity)
    {
        zoomedOutSensitivity = sensitivity;
    }

    public void SetZoomedInSensitivity(float sensitivity)
    {
        zoomedInSensitivity = sensitivity;
    }

    public void SetZoomedOutInvert(bool x, bool y)
    {
        zoomedOutXInvert = x;
        zoomedOutYInvert = y;
    }

    public void SetZoomedInInvert(bool x, bool y)
    {
        zoomedInXInvert = x;
        zoomedInYInvert = y;
    }
}
