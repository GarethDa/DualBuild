using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSettings : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera zoomOutCam;
    [SerializeField] CinemachineVirtualCamera zoomInCam;
    [SerializeField] CinemachineBrain cMachineBrain;

    [Header("Sensitivities")]
    [SerializeField] [Range(10f, 1000f)] float zoomedOutSensitivity = 250f;
    [SerializeField] [Range(10f, 1000f)] float zoomedInSensitivity = 150f;
    [SerializeField] [Range(1f, 3f)] float verticalSensitivityDivisor = 1f;

    [Header("Invert Aim")]
    [SerializeField] bool zoomedOutXInvert = false;
    [SerializeField] bool zoomedOutYInvert = true;
    [SerializeField] bool zoomedInXInvert = false;
    [SerializeField] bool zoomedInYInvert = true;

    [SerializeField] private float blendTime = 0.1f;

    CinemachineTransposer transposer;

    CinemachinePOV pov;

    int playerNum;

    bool teleported = false;

    // Start is called before the first frame update
    void Start()
    {
        playerNum = PlayerManager.instance.GetIndex(gameObject) + 1;
    }

        // Update is called once per frame
    void FixedUpdate()
    {
        cMachineBrain.m_DefaultBlend.m_Time = blendTime;

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

    public void SetBlendTime(float time)
    {
        blendTime = time;
    }

    public void TeleportCam(Vector3 originalPos, Vector3 newPos)
    {
        Vector3 offset = newPos - originalPos;
        Debug.Log("Offset: " + offset.magnitude);

        zoomOutCam.OnTargetObjectWarped(gameObject.transform.Find("Follow Target"), offset);
        zoomInCam.OnTargetObjectWarped(gameObject.transform.Find("Follow Target"), offset);
    }
}
