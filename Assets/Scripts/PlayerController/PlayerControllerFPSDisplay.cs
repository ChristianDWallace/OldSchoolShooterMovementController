using UnityEngine;

public class PlayerControllerFPSDisplay : MonoBehaviour
{
    //print() sytle of GUI information
    public GUIStyle guiStyle;

    //FPS calculations//
    public float fpsDisplayRate = 4.0f; //4 updates per second

    private int frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;

    private Vector3 velocity = Vector3.zero;

    private float playerTopVelocity = 0.0f; //the maximum velocity attained by the player during the play 

    public void CalculateFPS(Vector3 controllerVelocity)
    {
        //FRAMES PER SECOND CALCULATION
        frameCount++;
        dt += Time.deltaTime;
        //if the delta time value is greater than 1 over the fps display rate (the number of frames in a second. 
        if (dt > 1.0 / fpsDisplayRate)
        {
            fps = Mathf.Round(frameCount / dt); //the fps is the rounded value of frameCount in this second divided by delta time for this second
            frameCount = 0; //the frame count is reset to 0
            dt -= 1.0f / fpsDisplayRate; //the dt subtracts the value of this calculation bringing it back to its initial value. 
        }

        velocity = controllerVelocity;
    }

    public void SetTopVelocity(Vector3 playerVelocity)
    {
        if (playerVelocity.magnitude > playerTopVelocity)
        {
            playerTopVelocity = playerVelocity.magnitude; //if we overcome our previous speed record log it. 
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 400, 100), "FPS: " + fps, guiStyle);
        var ups = velocity;
        ups.y = 0;
        GUI.Label(new Rect(0, 15, 400, 100), "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups", guiStyle);
        GUI.Label(new Rect(0, 30, 400, 100), "Top Speed: " + Mathf.Round(playerTopVelocity * 100) / 100 + "ups", guiStyle);
    }
}
