using UnityEngine;

public static class PlayerConrollerInput
{

    [SerializeField]
    private static string fireButton = "Fire1",
    alternativeFireButton = "Fire2";

    [Header("Movement Inputs")]
    [SerializeField]
    private static string crouchButton = "Crouch";
    [SerializeField]
    private static string jumpButton = "Jump";

    public static float GetVerticalMouseInput()
    {
        return Input.GetAxis("Mouse Y");
    }

    public static float GetHorizontalMouseInput()
    {
        return Input.GetAxis("Mouse X");
    }

    public static bool GetFireButton()
    {
        if (Input.GetButton(fireButton))
        {
            return true;
        }

        return false; 
    }

    public static bool GetAlternativeFireButton()
    {
        if (Input.GetButton(alternativeFireButton))
        {
            return true;
        }

        return false; 
    }

    public static bool GetJumpButtonDown()
    {
        if (Input.GetButtonDown(jumpButton))
        {
            return true; 
        }
        else
        {
            return false; 
        }
    }

    public static bool GetCrouchKey()
    {
        if (Input.GetButton(crouchButton))
        {
            return true; 
        }

        return false; 
    }

    public static float GetVerticalMovement()
    {
        return Input.GetAxisRaw("Vertical");
    }

    public static float GetHorizontalMovement()
    {
        return Input.GetAxisRaw("Horizontal");
    }
}
