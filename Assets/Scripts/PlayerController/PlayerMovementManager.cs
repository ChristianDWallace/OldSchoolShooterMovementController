using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    private Cmd cmd;


    //PM_CmdScale
    //Returns the scale factor to aply cmd movements
    //this allwos the clients to use axial -127 to 127 valeus for all direcitons
    //without getting a sqrt(2) distortion in speed (1.4 speed distortion when strafe jumping)
    public float CmdScale(bool isGrounded, float moveSpeed, float moveScale, float airMoveSpeed)
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(cmd.forwardMove); //max is the absolute value of our forward move queue
        if (Mathf.Abs(cmd.rightMove) > max)
        {
            max = (int)Mathf.Abs(cmd.rightMove); //if our right move is greater than our forward move then we set that to max. 
        }
        if (max <= 0)
        {
            return 0;  //if we have a negative or a 0 in our max then just stop. 
        }

        total = Mathf.Sqrt(cmd.forwardMove * cmd.forwardMove + cmd.rightMove * cmd.rightMove); //get the square root of the squares of the forward and right values. 
        if (isGrounded)
        {
            scale = moveSpeed * max / (moveScale * total);
        }
        else
        {
            scale = airMoveSpeed * max / (moveScale * total);
        }


        return scale;
    }

    //Sets the movement direction based on player input
    public void SetMovementDir()
    {
        cmd.forwardMove = PlayerConrollerInput.GetVerticalMovement(); //Queue up the players forward desires for this frame using the vertical ais of the Get Axis function. 
        cmd.rightMove = PlayerConrollerInput.GetHorizontalMovement(); //Queue up the players movement to the left or right based on the axis input of the horizontal motion. 
    }

    public float GetCmdRight()
    {
        return cmd.rightMove;
    }

    public float GetCmdForward()
    {
        return cmd.forwardMove;
    }

}
