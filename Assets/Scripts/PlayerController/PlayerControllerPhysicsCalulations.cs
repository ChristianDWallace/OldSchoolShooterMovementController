using UnityEngine;

[RequireComponent(typeof(PlayerMovementManager))]
public class PlayerControllerPhysicsCalulations : MonoBehaviour
{

    [Header("Occurs Every Frame")]
    //FRAME OCCURING FACTORS
    [SerializeField]
    private float gravity = 25.0f;
    [SerializeField]
    private float friction = 6f; //Ground friction

    [Header("Movement")]
    //MOVEMENT STUFF//
    [SerializeField]
    private float moveSpeed = 11.0f; //Ground move speed

    [SerializeField]
    private float airMoveSpeed = 4.0f, //moveSpeed while in the air
        runAcceleration = 14.0f, //Ground acceleration
        runDeceleration = 10.0f, //Decceleration that occurs when running on the ground
        airAcceleration = 1.0f, //Air acceleration
        airDecceleration = 2.0f, //Decceleration experienced when opposite strafing
        airControl = 0.3f, //how precise air control is
        sideStrafeAcceleration = 50.0f, //How fast acceelration occurs to get up to sideStrafeSpeed 
        sideStrafeSpeed = 1.0f, //What the max speed to generate when side strafing
        jumpSpeed = 8.0f, //the speed at which the character's up axis gains when hitting jump
        moveScale = 1.0f;


    [SerializeField]
    private LayerMask crouchCollisionChecks;

    private float crouchSpeed,
        crouchAcceleration,
        crouchDeceleration;

    private float currentGroundMoveSpeed;
    private float currentGroundAcceleration;
    private float currentGroundDeceleration;

    private Vector3 moveDirectionNorm = Vector3.zero; //The normal of the move direction the player is headed in. 

    //Used to display real time friction values
    private float playerFriction = 0.0f;

    private PlayerMovementManager manager;


    public void SetUp()
    {
        currentGroundAcceleration = runAcceleration;
        currentGroundDeceleration = runDeceleration;
        currentGroundMoveSpeed = moveSpeed;

        crouchSpeed = moveSpeed / 2;
        crouchAcceleration = runAcceleration / 2;
        crouchDeceleration = runDeceleration / 2;

        manager = GetComponent<PlayerMovementManager>();
    }


    /// <summary>
    /// Executes when the player is moving through the air. 
    /// </summary>
    /// <param name="playerVelocity"></param>
    public Vector3 AirMove(Vector3 playerVelocity)
    {
        Vector3 wishDir; //the directional input the player is trying to make this frame. 

        float acceleration; //the acceleration for this frame to be applied at the end of the frame. 

        manager.SetMovementDir(); //set the movement direction based on wish direction. 
        float scale = manager.CmdScale(false, moveSpeed, moveScale, airMoveSpeed);

        wishDir = new Vector3(manager.GetCmdRight(), 0, manager.GetCmdForward()); //get the desired direction of movement based on player input using the axis of input. 
        wishDir = transform.TransformDirection(wishDir); //translates the local transform direction of x,y, and z into a world space direction. So instead of the wish direction referring to only the local transform of the player character it now refers to the world space the character resides in. 

        float wishSpeed = wishDir.magnitude; //the magnitude or length of the wish direction is the speed we wish to apply in the given direction. It is important to note that this speed is non-normalized, so when moving in the air we are not multiplying our speed of movement by 1 in a given direction, but on the ground this wish speed is always normalized by this point. 
        wishSpeed *= airMoveSpeed; //apply the air move speed function to increase speed in the given direciton.

        wishDir.Normalize(); //wish direction no longer needs a magnitude as it is now just a placeholder for the direction we want to apply speed. 
        moveDirectionNorm = wishDir; //the move direction normal is now equal to the normalized wish direction. 
        wishSpeed *= scale; //wishSpeed applies model scale. 

        //CPM AirControl 
        float wishSpeed2 = wishSpeed; //used for calculating the players air control, given that we apply acceleration using wishSpeed before we look at the players control possibilites. 
        if (Vector3.Dot(playerVelocity, wishDir) < 0)
        {
            acceleration = airDecceleration; //if our wish direction is breaking the current player velocity (going in an opposite direction requiring us to slow down which happens whent he camera turns beyond 90 degrees of the current travel direction) then apply deceleration. 
        }
        else
        {
            acceleration = airAcceleration; //Otherwise we want to accelerate within a 180 degree radius of the direction we are currently facing. 
        }

        //if the player is only strafing left or right
        if (manager.GetCmdForward() == 0 && manager.GetCmdRight() != 0)
        {
            if (wishSpeed > sideStrafeSpeed)
            {
                wishSpeed = sideStrafeSpeed; //clamp wish speed while strafing
            }
            acceleration = sideStrafeSpeed; //we can only accelerate using side strafe speed if we are soully moving left or right. 
        }

        playerVelocity = Accelerate(wishDir, wishSpeed, acceleration, playerVelocity); //call the accelerate function to apply the acceleration for this frame, and clip any excess accleration off of the value we just calculated for our given wihs direction. 
        if (airControl > 0)
        {
            playerVelocity = AirControl(wishDir, wishSpeed2, playerVelocity);
        }
        //!CPM: Aircontrol

        //Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime; //add gravity to the player at the end of this calculation. 

        return playerVelocity;
    }

    //Air control occurs when the player is in the air
    //it allows players to move side to side much faster rather than 
    //being sluggish when it comes to cornering.
    private Vector3 AirControl(Vector3 wishDir, float wishSpeed, Vector3 playerVelocity)
    {
        float zSpeed;
        float speed;
        float dot;
        float k;

        //Can't control movement if we are not trying to move in some direction forward or backward, and if we do not wish to add speed in that direction (I.E. if the direction we are trying to move does not have a wish speed). 
        if (Mathf.Abs(manager.GetCmdForward()) == 0 || Mathf.Abs(wishSpeed) == 0)
        {
            return playerVelocity;
        }



        zSpeed = playerVelocity.y; //in the quake engine the z axis is the one that dictates up and down movement so we store the player velocity in the up and down direction in a float named zSpeed here
        playerVelocity.y = 0; //we want the player velocity to be 0 along the y axis for now. 
        //Next two lines are equivalent ot IDTECHS vectornormalize()
        speed = playerVelocity.magnitude; //now we get the magnitude of the movement along the Unity x and z directions. 
        playerVelocity.Normalize(); //Now we normalize so we know just the directions. 

        dot = Vector3.Dot(playerVelocity, wishDir); //get the innate difference between the player velocity magnitude and the magnitude of the wish direction (the difference in cosine valeus of the speed in these two direction). 
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime; // a constant modifier that is a factor of air control which determines how much of our wish direction change is actually applied when the player changes direction. 

        //Change direction while slowing down
        if (dot > 0)
        {
            //if the difference in our desired speed of direction and velocity is positive (I.E. we are changing directions within 90 degrees of our current direction). 
            playerVelocity.x = playerVelocity.x * speed + wishDir.x * k; //then we take the normalized player speed and add the speed + the wish direction speed times the constant to see the change in direction. 
            playerVelocity.y = playerVelocity.y * speed + wishDir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishDir.z * k;

            playerVelocity.Normalize(); //re normalize player velocity to see what the change is . 
            moveDirectionNorm = playerVelocity; //so the new normal of our move direction is equal to the current player velocity normal 
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zSpeed; //Note this line where we add back in our original gravitational speed unchanged. 
        playerVelocity.z *= speed;

        return playerVelocity;
    }

    //Called every frame when the engine detects that the player is on the ground
    public Vector3 GroundMove(bool wishJump, Vector3 playerVelocity, bool isGrounded)
    {
        Vector3 wishDir;

        //Do not apply friction if the player is queuing up the next jump
        if (!wishJump)
        {
            playerVelocity = ApplyFriction(1.0f, playerVelocity, isGrounded);
        }
        else
        {
            playerVelocity = ApplyFriction(0f, playerVelocity, isGrounded);
        }

        manager.SetMovementDir(); //set the wish direction of the next frame (it will be applied next frame even though it is grabbed in this frame). 
        float scale = manager.CmdScale(isGrounded, moveSpeed, moveScale, airMoveSpeed); //get the scale of the player model 

        wishDir = new Vector3(manager.GetCmdRight(), 0, manager.GetCmdForward()); //the wish direction is the intended movement for the next frame 
        wishDir = transform.TransformDirection(wishDir); //change wish direction from local to the palyer transform into a global value. 
        wishDir.Normalize(); //normalize it because all that matters is the intended directional change. 
        moveDirectionNorm = wishDir;

        var wishSpeed = wishDir.magnitude; //the magnitude of the wish direction which has now been normalized is the wishspeed. 
        wishSpeed *= moveSpeed; //add the movespeed value to it. 

        playerVelocity = Accelerate(wishDir, wishSpeed, runAcceleration, playerVelocity); //accelerate 

        //Reset the gravity velocity
        playerVelocity.y = -gravity * Time.deltaTime; //add gravity at the end of this frame. 

        return playerVelocity;
    }

    //Applies friction to the player called in both the air and on the ground
    public Vector3 ApplyFriction(float t, Vector3 playerVelocity, bool isGrounded)
    {
        Vector3 vec = playerVelocity; //Equivalent to : VectorCopy();
        //float vel;
        float speed;
        float newSpeed;
        float control;
        float drop;

        vec.y = 0.0f; //we don't care about the upwards or downwards force in this calculation. 
        speed = vec.magnitude; //the magnitude of the x and z values of our player velocity is the speed. 
        drop = 0.0f;

        if (isGrounded)
        {
            control = speed < runDeceleration ? runDeceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newSpeed = speed - drop; //so now we detract the drop from our speed
        playerFriction = newSpeed; //and the player friction value becomes our new speed that we calculated. 
        if (newSpeed < 0)
        {
            newSpeed = 0; //if the new speed is too low we just make it 0. 
        }
        if (speed > 0)
        {
            newSpeed /= speed; //else it is equal to itself / speed. 
        }

        playerVelocity.x *= newSpeed; //apply the new speed along the x and z axis. 
        playerVelocity.z *= newSpeed;

        return playerVelocity;
    }

    //Here is where the STRAFE JUMPING MAGIC HAPPENS. THIS IS CALLED EVERY FRAME
    //and as long as the player is in the air and isn't about to collide with something
    //This entirely determines the players velocity frame by frame
    //The alternate left and right key taps instantly changes wish direction by 90 degrees from the point of orientation. 
    //
    private Vector3 Accelerate(Vector3 wishDir, float wishSpeed, float acceleration, Vector3 playerVelocity)
    {
        //Wish speed is the factor of acceleration for our given movement system (in the ground or in the air) * the magnitude of our wish direction. 


        //Wish direction is the direction the player intends to travel. It is a combination of the players view angle and arrow keys. 
        //When the player presses and releases arrow keys wish direction changes relative to the view angle. 
        //The return of this accelerate function is the players velocity for the next frame. 
        float addSpeed;
        float accelSpeed;
        float currentSpeed;

        //THE DOT PRODUCT IS THE VALUE OF THE MAGNITUDE OF THE TWO VECTORS MULTIPLIES TOGETHER AND THEN MULTIPLIED BY THE COSINE OF THE ANGLE BETWEEN THEM. 
        //Because of the way this works out mathematically, we want the difference between our orientation of the left and right angles to be as close to 180 as possible
        //because at 180 the dot is at its smallest and negative, meaning that when it is subtracted from our wished acceleration speed we gain speed on top of the wish. 



        //The goal in strafe jumping is to create the smallest current speed as possible because "current speed" could more accurately be called (Speed Penalty); 
        currentSpeed = Vector3.Dot(playerVelocity, wishDir); //assign a current speed that the player is moving. Geometrically you can model current speed by drawing a line from the tip of the velocity anticipated in the next frame to the wish direction at a right angle. 
        //Thus current speed is the distance of the right angle (modeled between the velocity and wish direction) to the center of the player. 
        //Note that current speed is not accurately named and is different from actual speed which is simply the length of velocity. 

        addSpeed = wishSpeed - currentSpeed; //Addspeed is taken to be the nominal running speed (320 UPS in quake 3) subtracted from the current speed. 
        //Add speed is a correspondance in our imaginary triangle between the tip of the wishdirection arrow and the curren speed line. 
        if (addSpeed <= 0)
        {
            return playerVelocity;
        }
        accelSpeed = acceleration * Time.deltaTime * wishSpeed;
        if (accelSpeed > addSpeed)
        {
            accelSpeed = addSpeed; //we clip the acceleration value if it is greater than the addspeed function which is the difference between the limiting function and the calculated current speed (hypothetical 320 UPS - currentSpeed) in our engine. 
        }

        playerVelocity.x += accelSpeed * wishDir.x;
        playerVelocity.z += accelSpeed * wishDir.z;

        return playerVelocity;
    }

    /// <summary>
    /// Handles Crouching
    /// </summary>
    public void CrouchCharacter()
    {
        moveSpeed = crouchSpeed;
        runAcceleration = crouchAcceleration;
        runDeceleration = crouchDeceleration;
    }

    /// <summary>
    /// Uncrouches player
    /// </summary>
    public void UnCrouch()
    {
        moveSpeed = currentGroundMoveSpeed;
        runAcceleration = currentGroundAcceleration;
        runDeceleration = currentGroundDeceleration;
    }

    public float GetJumpSpeed()
    {
        return jumpSpeed;
    }
}
