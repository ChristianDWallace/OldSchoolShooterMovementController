using UnityEngine;

/// <summary>
/// The class in charge of setting up our camera on our gameObject. 
/// </summary>
public class PlayerControllerCamera : MonoBehaviour
{
    [Header("Camera Setup")]
    [SerializeField]
    private Transform playerView; //the Camera
    [SerializeField]
    private float playerViewYOffset = 0.6f, //the height at which the camera will be bound
    xMouseSensitivity = 50.0f,
    yMouseSensitivity = 50.0f;

    //Camera rotations
    private float rotX = 0.0f;
    private float rotY = 0.0f;


    public void CameraSetUp()
    {
        if (playerView == null)
        {
            playerView = Camera.main.transform;
            Camera.main.transform.parent = transform;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Set the camera where we want it at about 60% up the player model 
        playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);
    }

    /// <summary>
    /// Called each frame, this rotates the camera to look at the player position. 
    /// </summary>
    public void CameraLookCalculations()
    {
        //Ensure the cursor is locked to the screen
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (PlayerConrollerInput.GetFireButton())
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        //Camera rotation stuff, mouse controls this
        rotX -= PlayerConrollerInput.GetVerticalMouseInput() * xMouseSensitivity * 0.02f; //rotation up and down (along the x axis)
        rotY += PlayerConrollerInput.GetHorizontalMouseInput() * yMouseSensitivity * 0.02f; //rotation side to side (along the y axis)

        //Clamp the x rotation
        if (rotX < -90)
        {
            rotX = -90;
        }
        else if (rotX > 90)
        {
            rotX = 90;
        }
    }

    public void CameraLook()
    {
        transform.rotation = Quaternion.Euler(0, rotY, 0); //rotate the collider
        playerView.rotation = Quaternion.Euler(rotX, rotY, 0); //Rotates the camera
    }

    public void Stand()
    {
        playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);
    }

    public void Crouch()
    {
        playerView.position = new Vector3(transform.position.x, transform.position.y + (playerViewYOffset * .5f), transform.position.z);
    }

}
