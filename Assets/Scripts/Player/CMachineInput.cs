using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CMachineInput : MonoBehaviour, AxisState.IInputAxisProvider
{
    [HideInInspector] public InputAction horizontal;
    [HideInInspector] public InputAction vertical;

    public float GetAxisValue(int axis)
    {
        Debug.Log("we here");
        switch (axis)
        {
            case 0: return horizontal.ReadValue<Vector2>().x;
            case 1: return horizontal.ReadValue<Vector2>().y;
            case 2: return vertical.ReadValue<float>();
        }

        return 0f;
    }
}
