using UnityEngine;

//This engine is designed to facilitate QUAKE 3 style strafe jumping and movement. This engine 
//Allows the player to strafe jump, a function that is utilized in the QUAKE 3 engine. This movement phenomena
//Comes about due to the hardcap set on player movement in the quake engine when the palyer is moving in any given direction. 
//This works because in the Quake engine speed and movement are not only based on keystrokes but also on the direction the player is looking.
//So if the player is looking forwards and holding the left and forward keys for movement, then the player will move in a line 45 degrees between the right angle
//that the forward and left vectors make. However, this angle of motion is seperate from actual motion and based on the (forward) of the player camera. So the player 
//Can look left or right into a jump to increase the angle from the actual movement direction. In this sense, holding left and forward always results in a forward motion of 45 degrees
//From the forward motion of the camera. However, Vectors apply velocity on a multitude of axis at once.
//Put simply, strafe jumping is a combination of friction reduction due to maximizing air time, with a manipulation of the way that movement is applied based on the direction the player is looking.
//By jumping left and right, the player accelerates at 45 degress of forward to the camera, and by looking slightly left and right, the player's acceleration vector becomes greater, but the actual velocity applied
//Averages out over the the alternating camera swings to essentially be forward motion that the player wishes to achieve. 

//Additionally this engine uses friction to slow down the player, and friction is applied to ground surfaces. However, the air has no friction, so the player is able to overcome
//friction by continuously jumping on a surface to avoid the decceleration effects. 

//It is important to note that the above method described of manipulating the math where the player alternates key presses and camera oritentation between left and right is called SINGLE BEAT STRAFING,
//However, strafing can also be achieved through various other methods to the same effect, such as HALF BEAT STRAFING, which works the same as single beat, but instead of looking left and right, the player only
//Looks to the left or to the right (meaning one way or the other) in between jumps, and then looks back to center before flicking back in the same direction as before. In combination with this, the player holds forwards
//Only during the jumps in which they are moving towards the angle in which they are looking. (so if the player is flicking the mouse left they only hold forward when they are also strafing left).
//This works because the angular math behind motion of the key presses is based on 45 degree angles between the direction of intended forward momentum and player orientation when strafing. However, when the player lets off of 
//the forward facing key, that difference becomes double that (or 90). thus, if the player were able to perfectly flick from the forward direction they wished to move in, and the exact same leftwards facing angle with every jump,
//While also alternating between the left and forward key combination, and the right facing combination, the mathetmatical values for the acceleration vectors to the left or the right would be identicle, and the forward momentum gained
//Would be the same as if they were SINGLE BEAT STRAFING. 

//This is all to point out that the key factor is the angle between our velocity but why? 
//Because this engine has a cap on our movement speed when moving in any given direction, we cannot gain above a set number of units per second of movement in a key based direction.
//However, we can gain movement in another direction so long as we do not go over the cap. 
//SO when we try to apply acceleration at some angle different than the current velocity, we will be allowed to gain that acceleration so long as we do not eclipse the cap in that direction.


//Considering the angle between velocity and acceleration is called DELTA
//The math looks like this: 
//Velocity * Acceleration = VCosine(delta); 

//This reads as the projection of the velocity onto the new acceleration

//Another condition that must be met
//VCosine(delta) < maxUnitsPerSecond; 

//Note that, because of the cosine function, when you try to apply acceleration to a delta that is greater than 90 degrees from the current velocity, the 
//New projected velocity is negative.

//Also note that the velocity (speed), friction, and acceleration are all constants applied at intervals. So the player acceleration is constant
//Say from 0 to 10 in one second. So if the player can get to 100 UPS velocity, they would take 10 seconds to reach that velocity. However, there is
//One case in the engine where these constants are not true, and that is the case where the difference between the current frame and the next frame
//Are less than the constant acceleration, (say currently the movement is 95UPS and 100UPS is the cap) in this case the game would calculate that the
//Player needs to add 5 acceleration units to their speed in the next frame to get to the cap. HOWEVER the range of angle delta in which this situation exists in engine
//is very small. 

//So velocity is speed * direction. Acceleration is the change in speed vs change in direction of that velocity. So although the acceleration is constant
//The direction that the acceleration is being applied in dictates how much the acceleration changes the speed versus changing the direction of the velocity vector. 
//This means that by applying the acceleration at an angle as close to the current velocity as possible, you maximize the speed increase over the directional increase
//Which is the fundamental idea behind strafe jumping. So the smaller the delta (or difference between camera and current velocity) the more we chang eour speed over our direction

//The limiting factor here is 
//Delta = arcCos(320/velocity); 
//However in order to recieve the constant acceleration rather than the trimmed acceleration that is calculated with small delta values we need delta to be slightly larger
//In this case the function looks like so
//DELTA = ArcCos((320 -acceleration)/velocity);

//This function means that as we strafe jump we are adding speed by essentially clipping extra acceleration by looking away from the velocity direction but not too much. 
//This also means that the angle DELTA required to attain this speed increase with acceleration constant increases as velocity increases, or the angle of orientation is 
//DEPENDENT on the velocity. So the ideal angle for applying the most speed possible in a strafe jump gets larger the faster the player is moving in a specific direction, and approaches
//90 degrees as it approaches infinity. 

//So in using these angles the acceleration that is, in a way skimmed off the top of the engine with each successful jump, causes the current velocity v to change into a new
//velocity r. 

//We can use trig to calculate the size and change in direction of the r velocity

// so the resultant speed r after one frame in the game engine is denoted by a piecewise function representing the various states of r relating to s (the speed in engine) of that frame
//r = Sqrt(v^2 + a^2 + 2vacos(delta)), vCos(delta) <= s(engine speed) - a
//r = SQRT(v^2Sin^2(delta) + s^2), s-a < vCos(delta) < s
//r = v, vCos(delta) > s

//The acceleration per frame a(sT) is a factor of the acceleration constant divided by the frames in a single second. 
//So if the acceleration was 320 UPS and the frame rate was 125 fps the a(sT) would be 2.56 UPS. 

//In order to gain the optimal speed increase in a strafe jump, delta must be within a finite limit. In QUAKE 3 the difference between optimal Delta in both the left and right direction is 3 degrees. This means
//That the margin of error for this is very small. And as the velocity increases the margin of error becomes ever smaller. Also, because of the engine cutting off acceleration values 
//that are close to the current velocity the margin of error for too small of an angle is almost nonexistant, whereas the margin of error for too wide of an angle is much more forgiving. 

//If we apply an optimal Delta we get a resultant velocity r of
//r = SQRT(v^2 - a^2 + 2as)

//This function displays the fact that the maximum increase in speed per frame becomes lower the larger the velocity. 

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    [Header("Player Setup")]
    public Transform playerView; //the Camera
    public float playerViewYOffset = 0.6f; //the height at which the camera will be bound
    public float xMouseSensitivity = 30.0f;
    public float yMouseSensitivity = 30.0f;
    public float crouchHeight = .5f; //the height multiplier of the player. 
    public float normalHeight = 1f;

    [Header("Occurs Every Frame")]
    //FRAME OCCURING FACTORS
    public float gravity = 20.0f;

    public float friction = 6f; //Ground friction

    [Header("Movement")]
    //MOVEMENT STUFF//
    public float moveSpeed = 7.0f; //Ground move speed
    public float airMoveSpeed = 7.0f; //moveSpeed while in the air
    public float runAcceleration = 14.0f; //Ground acceleration
    public float runDeceleration = 10.0f; //Decceleration that occurs when running on the ground
    public float airAcceleration = 2.0f; //Air acceleration
    public float airDecceleration = 2.0f; //Decceleration experienced when opposite strafing
    public float airControl = 0.3f; //how precise air control is
    public float sideStrafeAcceleration = 50.0f; //How fast acceelration occurs to get up to sideStrafeSpeed 
    public float sideStrafeSpeed = 1.0f; //What the max speed to generate when side strafing
    public float jumpSpeed = 8.0f; //the speed at which the character's up axis gains when hitting jump
    public float moveScale = 1.0f;
    public float crouchSpeed;
    public float crouchAcceleration;
    public float crouchDeceleration;

    public float slopeAngle = 45; //the slope angle that we start applying physics to stop the player from climbing certain slopes.

    private float currentGroundMoveSpeed;
    private float currentGroundAcceleration;
    private float currentGroundDeceleration;

    public LayerMask crouchCollisionChecks;

    [Header("Movement Controls")]
    public string CROUCH = "Crouch";

    //print() sytle of GUI information
    public GUIStyle style;

    //FPS calculations//
    public float fpsDisplayRate = 4.0f; //4 updates per second

    private int frameCount = 0;
    private float dt = 0.0f;
    private float fps = 0.0f;

    private CharacterController _controller;

    //Camera rotations
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    private Vector3 moveDirectionNorm = Vector3.zero; //The normal of the move direction the player is headed in. 
    private Vector3 playerVelocity = Vector3.zero; //The speed and direction the player is moving
    private float playerTopVelocity = 0.0f; //the maximum velocity attained by the player during the play session. 

    //Quake 3: Players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    private bool wishCrouch = false;

    bool validStand;

    //Used to display real time friction values
    private float playerFriction = 0.0f;

    //Player commands stores wish commands that the player asks for (forward back jump etc)
    //These wish commands are then processed through this stored value, rather than when they are inputted. 
    private Cmd _cmd;

    private void Start()
    {
        if (playerView == null)
        {
            playerView = Camera.main.transform;
            Camera.main.transform.parent = transform; 
        }
        //Start music
        //ObjectTracker.audioManager.Play("Music");

        //Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentGroundAcceleration = runAcceleration;
        currentGroundDeceleration = runDeceleration;
        currentGroundMoveSpeed = moveSpeed;
        normalHeight = GetComponent<CharacterController>().height;
        crouchHeight = normalHeight * 0.65f;
        crouchSpeed = moveSpeed / 2;
        crouchAcceleration = runAcceleration / 2;
        crouchDeceleration = runDeceleration / 2;
        //Set the camera where we want it at about 60% up the player model 
        playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);

        _controller = GetComponent<CharacterController>();
    }

    private void Update()
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

        //Ensure the cursor is locked to the screen
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        //Camera rotation stuff, mouse controls this
        rotX -= Input.GetAxis("Mouse Y") * xMouseSensitivity * 0.02f; //rotation up and down (along the x axis)
        rotY += Input.GetAxis("Mouse X") * yMouseSensitivity * 0.02f; //rotation side to side (along the y axis)

        //Clamp the x rotation
        if (rotX < -90)
        {
            rotX = -90;
        }
        else if (rotX > 90)
        {
            rotX = 90;
        }

        transform.rotation = Quaternion.Euler(0, rotY, 0); //rotate the collider
        playerView.rotation = Quaternion.Euler(rotX, rotY, 0); //Rotates the camera

        //Movement starts here//



        CrouchControls();
        if (wishCrouch)
        {
            CrouchCharacter();
        }
        else
        {
            UnCrouch();
        }


        if (!wishCrouch && validStand)
        {
            QueueJump(); //See if the player is trying to jump
        }

        if (_controller.isGrounded)
        {
            GroundMove(); //if the player is touching the ground this frame then we calculate our acceleration for the next frame based on the ground movement with friction. 
        }
        else if (!_controller.isGrounded)
        {
            AirMove(); //if we are not touching the ground this frame then calculate our acceleration for the next frame based on air movement parameters
        }


        //Calculate top velocity to show how fast the player is moving. 
        Vector3 udp = playerVelocity;
        udp.y = 0.0f; //we don't want to take into account falling velocity
        if (playerVelocity.magnitude > playerTopVelocity)
        {
            playerTopVelocity = playerVelocity.magnitude; //if we overcome our previous speed record log it. 
        }

        //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast
        //Set the camera's position to the transform
        if (!wishCrouch && validStand)
        {
            playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);
        }
        else if (wishCrouch)
        {
            playerView.position = new Vector3(transform.position.x, transform.position.y + (playerViewYOffset * .5f), transform.position.z);
        }
    }

    private void FixedUpdate()
    {
        //If the player has queued up a jump then jump
        if (wishJump && _controller.isGrounded)
        {
            //Debug.Log("Jumped"); 
            playerVelocity.y = jumpSpeed; //add the jump speed constant to the y velocity to cause the player to jump. 
            wishJump = false; //the players jump queue is false. 
        }
        //Move the controller
        //Debug.Log(playerVelocity);
        _controller.Move(playerVelocity * Time.deltaTime);
    }

    /// <summary>
    /// This checks for the controls for crouching. 
    /// </summary>
    void CrouchControls()
    {
        if (Input.GetButton(CROUCH))
        {
            wishCrouch = true;
        }
        else
        {
            wishCrouch = false;
        }
    }


    //MOVEMENT

    //Sets the movement direction based on player input
    private void SetMovementDir()
    {
        _cmd.forwardMove = Input.GetAxisRaw("Vertical"); //Queue up the players forward desires for this frame using the vertical ais of the Get Axis function. 
        _cmd.rightMove = Input.GetAxisRaw("Horizontal"); //Queue up the players movement to the left or right based on the axis input of the horizontal motion. 
    }

    //Handles crouching
    void CrouchCharacter()
    {
        gameObject.GetComponent<CharacterController>().height = crouchHeight;
        moveSpeed = crouchSpeed;
        runAcceleration = crouchAcceleration;
        runDeceleration = crouchDeceleration;
        //transform.position = new Vector3(transform.position.x, transform.position.y * crouchHeight, transform.position.z); 
    }

    /// <summary>
    /// Uncrouches player
    /// </summary>
    void UnCrouch()
    {
        if (UnCrouchCheckAbove())
        {
            wishCrouch = false;
            gameObject.GetComponent<CharacterController>().height = normalHeight;
            moveSpeed = currentGroundMoveSpeed;
            runAcceleration = currentGroundAcceleration;
            runDeceleration = currentGroundDeceleration;
        }
    }

    /// <summary>
    /// This Method checks if something is overhead of the player when they try to stand up and stops them from standing if there is something. 
    /// </summary>
    /// <returns> true if there is soemthing overHead, false if not</returns>
    bool UnCrouchCheckAbove()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, normalHeight, crouchCollisionChecks))
        {
            validStand = false;
        }
        else
        {
            validStand = true;
        }

        return validStand;
    }

    //Queues the next jump just like in Quake 3
    private void QueueJump()
    {
        if (Input.GetButtonDown("Jump") && !wishJump)
        {
            wishJump = true; //If we want to jump and have not already queued the wish to jump then queue it now. 
        }

        if (Input.GetButtonUp("Jump"))
        {
            wishJump = false; //else if we release the jump button then set wish jump to false. 
        }
    }

    //Executes when the player is moving in the air
    private void AirMove()
    {
        Vector3 wishDir; //the directional input the player is trying to make this frame. 

        float acceleration; //the acceleration for this frame to be applied at the end of the frame. 

        SetMovementDir(); //set the movement direction based on wish direction. 
        float scale = CmdScale();

        wishDir = new Vector3(_cmd.rightMove, 0, _cmd.forwardMove); //get the desired direction of movement based on player input using the axis of input. 
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
        if (_cmd.forwardMove == 0 && _cmd.rightMove != 0)
        {
            if (wishSpeed > sideStrafeSpeed)
            {
                wishSpeed = sideStrafeSpeed; //clamp wish speed while strafing
            }
            acceleration = sideStrafeSpeed; //we can only accelerate using side strafe speed if we are soully moving left or right. 
        }

        Accelerate(wishDir, wishSpeed, acceleration); //call the accelerate function to apply the acceleration for this frame, and clip any excess accleration off of the value we just calculated for our given wihs direction. 
        if (airControl > 0)
        {
            AirControl(wishDir, wishSpeed2);
        }
        //!CPM: Aircontrol

        //Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime; //add gravity to the player at the end of this calculation. 
    }

    //Air control occurs when the player is in the air
    //it allows players to move side to side much faster rather than 
    //being sluggish when it comes to cornering.
    private void AirControl(Vector3 wishDir, float wishSpeed)
    {
        float zSpeed;
        float speed;
        float dot;
        float k;

        //Can't control movement if we are not trying to move in some direction forward or backward, and if we do not wish to add speed in that direction (I.E. if the direction we are trying to move does not have a wish speed). 
        if (Mathf.Abs(_cmd.forwardMove) == 0 || Mathf.Abs(wishSpeed) == 0)
        {
            return;
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
    }

    //Called every frame when the engine detects that the player is on the ground
    private void GroundMove()
    {
        Vector3 wishDir;

        //Do not apply friction if the player is queuing up the next jump
        if (!wishJump)
        {
            ApplyFriction(1.0f);
        }
        else
        {
            ApplyFriction(0f);
        }

        SetMovementDir(); //set the wish direction of the next frame (it will be applied next frame even though it is grabbed in this frame). 
        float scale = CmdScale(); //get the scale of the player model 

        wishDir = new Vector3(_cmd.rightMove, 0, _cmd.forwardMove); //the wish direction is the intended movement for the next frame 
        wishDir = transform.TransformDirection(wishDir); //change wish direction from local to the palyer transform into a global value. 
        wishDir.Normalize(); //normalize it because all that matters is the intended directional change. 
        moveDirectionNorm = wishDir;

        var wishSpeed = wishDir.magnitude; //the magnitude of the wish direction which has now been normalized is the wishspeed. 
        wishSpeed *= moveSpeed; //add the movespeed value to it. 

        Accelerate(wishDir, wishSpeed, runAcceleration); //accelerate 

        //Reset the gravity velocity
        playerVelocity.y = -gravity * Time.deltaTime; //add gravity at the end of this frame. 
    }

    //Applies friction to the player called in both the air and on the ground
    private void ApplyFriction(float t)
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

        if (_controller.isGrounded)
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
    }

    //Here is where the STRAFE JUMPING MAGIC HAPPENS. THIS IS CALLED EVERY FRAME
    //and as long as the player is in the air and isn't about to collide with something
    //This entirely determines the players velocity frame by frame
    //The alternate left and right key taps instantly changes wish direction by 90 degrees from the point of orientation. 
    //
    private void Accelerate(Vector3 wishDir, float wishSpeed, float acceleration)
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
            return;
        }
        accelSpeed = acceleration * Time.deltaTime * wishSpeed;
        if (accelSpeed > addSpeed)
        {
            accelSpeed = addSpeed; //we clip the acceleration value if it is greater than the addspeed function which is the difference between the limiting function and the calculated current speed (hypothetical 320 UPS - currentSpeed) in our engine. 
        }

        playerVelocity.x += accelSpeed * wishDir.x;
        playerVelocity.z += accelSpeed * wishDir.z;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 400, 100), "FPS: " + fps, style);
        var ups = _controller.velocity;
        ups.y = 0;
        GUI.Label(new Rect(0, 15, 400, 100), "Speed: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups", style);
        GUI.Label(new Rect(0, 30, 400, 100), "Top Speed: " + Mathf.Round(playerTopVelocity * 100) / 100 + "ups", style);
    }

    //PM_CmdScale
    //Returns the scale factor to aply cmd movements
    //this allwos the clients to use axial -127 to 127 valeus for all direcitons
    //without getting a sqrt(2) distortion in speed (1.4 speed distortion when strafe jumping)
    private float CmdScale()
    {
        int max;
        float total;
        float scale;

        max = (int)Mathf.Abs(_cmd.forwardMove); //max is the absolute value of our forward move queue
        if (Mathf.Abs(_cmd.rightMove) > max)
        {
            max = (int)Mathf.Abs(_cmd.rightMove); //if our right move is greater than our forward move then we set that to max. 
        }
        if (max <= 0)
        {
            return 0;  //if we have a negative or a 0 in our max then just stop. 
        }

        total = Mathf.Sqrt(_cmd.forwardMove * _cmd.forwardMove + _cmd.rightMove * _cmd.rightMove); //get the square root of the squares of the forward and right values. 
        if (_controller.isGrounded)
        {
            scale = moveSpeed * max / (moveScale * total);
        }
        else
        {
            scale = airMoveSpeed * max / (moveScale * total);
        }


        return scale;
    }

    //get the player velocity during this frame. 
    public Vector3 GetCurrentVelocity()
    {
        return playerVelocity;
    }

    public Vector3 GetPlayerOrigin()
    {
        return transform.position;
    }

    public void SetCurrentVelocity(Vector3 velocity)
    {
        playerVelocity = velocity;
    }

    //control collision specific items. 
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (hit.normal == -transform.up)
        {

            if (!_controller.isGrounded)
            {
                //Debug.Log("normal " + hit.normal);
                //Debug.Log("transform up " + transform.up); 
                Vector3 currentVelocity = Vector3.Project(playerVelocity, hit.normal);
                //Debug.Log(currentVelocity); 
                if (currentVelocity.y > 1)
                {
                    playerVelocity -= currentVelocity;
                }
                else
                {
                    playerVelocity -= Vector3.up;
                }
            }

            return;
        }


        if (hit.gameObject.tag == "Obstacle" || hit.gameObject.tag == "Enemy" || hit.gameObject.tag == "HookEnemy" || hit.gameObject.tag == "Hookable")
        {
            float _Angle = Vector3.Angle(hit.normal, Vector3.up);

            if (_Angle > slopeAngle)
            {
                //Debug.Log("Angle thing called"); 
                Vector3 _velocityToObstacle = Vector3.Project(playerVelocity, hit.normal);
                playerVelocity -= _velocityToObstacle;
            }
        }
    }

}

