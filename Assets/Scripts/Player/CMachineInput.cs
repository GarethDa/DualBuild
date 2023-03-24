using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CMachineInput : MonoBehaviour, AxisState.IInputAxisProvider
{
    [HideInInspector] public InputAction horizontal;
    [HideInInspector] public InputAction vertical;

    [SerializeField] PauseMenu pauseMenu; 

    public float GetAxisValue(int axis)
    {
        if (pauseMenu.GetIsPaused())
            return 0f;

        switch (axis)
        {
            case 0:
                {
                    if (horizontal.ReadValue<Vector2>().x < 0.3f && horizontal.ReadValue<Vector2>().x > -0.3f)
                        return 0f;
                    else
                        return horizontal.ReadValue<Vector2>().x;
                }
            case 1:
                {
                    if (horizontal.ReadValue<Vector2>().y < 0.3f && horizontal.ReadValue<Vector2>().y > -0.3f)
                        return 0f;
                    else
                        return horizontal.ReadValue<Vector2>().y;
                }
            case 2:
                {
                    if (vertical.ReadValue<float>() < 0.3f && vertical.ReadValue<float>() > -0.3f)
                        return 0f;
                    else
                        return vertical.ReadValue<float>();

                }

            default: return 0f;
        }
    }
}
