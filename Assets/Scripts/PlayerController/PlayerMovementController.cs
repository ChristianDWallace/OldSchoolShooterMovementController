using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerControllerCamera))]
[RequireComponent(typeof(PlayerControllerPhysicsCalulations))]
[RequireComponent(typeof(PlayerControllerFPSDisplay))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField]
    public float crouchHeight = 0.65f; //the height multiplier of the player. 

    [SerializeField]
    private float maxSlopeAngle = 45f; //the slope angle that we start applying physics to stop the player from climbing certain slopes.

    [SerializeField]
    private LayerMask crouchCollisionChecks;

    [SerializeField]
    private bool calculateFPS = true;

    private PlayerControllerFPSDisplay fps; 

    private CharacterController controller;

    private PlayerControllerCamera myCamera;

    private PlayerControllerPhysicsCalulations physicsCalculations;

    private float normalHeight;

    //Quake 3: Players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    private bool wishCrouch = false;

    private bool validStand;

    private Vector3 playerVelocity = Vector3.zero; //The speed and direction the player is moving

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        myCamera = GetComponent<PlayerControllerCamera>();
        physicsCalculations = GetComponent<PlayerControllerPhysicsCalulations>();
        fps = GetComponent<PlayerControllerFPSDisplay>();

        if (calculateFPS)
        {
            fps.enabled = true;
        }
        else
        {
            fps.enabled = false; 
        }

        myCamera.CameraSetUp();
        physicsCalculations.SetUp();

        normalHeight = controller.height;
        crouchHeight = normalHeight * crouchHeight;
    }

    // Update is called once per frame
    void Update()
    {
        myCamera.CameraLookCalculations();
        myCamera.CameraLook();
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

        if (controller.isGrounded)
        {
            playerVelocity = physicsCalculations.GroundMove(wishJump, playerVelocity, controller.isGrounded); //if the player is touching the ground this frame then we calculate our acceleration for the next frame based on the ground movement with friction. 
        }
        else if (!controller.isGrounded)
        {
            playerVelocity = physicsCalculations.AirMove(playerVelocity); //if we are not touching the ground this frame then calculate our acceleration for the next frame based on air movement parameters
        }

        if (calculateFPS)
        {
            fps.CalculateFPS(controller.velocity);

            fps.SetTopVelocity(playerVelocity);
        }
        //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast
        //Set the camera's position to the transform
        if (!wishCrouch && validStand)
        {
            myCamera.Stand();
        }
        else if (wishCrouch)
        {
            myCamera.Crouch();
        }
    }

    void FixedUpdate()
    {
        //If the player has queued up a jump then jump
        if (wishJump && controller.isGrounded)
        {
            //Debug.Log("Jumped"); 
            playerVelocity.y = physicsCalculations.GetJumpSpeed(); //add the jump speed constant to the y velocity to cause the player to jump. 
            wishJump = false; //the players jump queue is false. 
        }
        //Move the controller
        //Debug.Log(playerVelocity);

        controller.Move(playerVelocity * Time.deltaTime);
    }

    //Handles crouching
    void CrouchCharacter()
    {
        gameObject.GetComponent<CharacterController>().height = crouchHeight;
        physicsCalculations.CrouchCharacter();
        //transform.position = new Vector3(transform.position.x, transform.position.y * crouchHeight, transform.position.z); 
    }

    void CrouchControls()
    {
        if (PlayerConrollerInput.GetCrouchKey())
        {
            wishCrouch = true;
        }
        else
        {
            wishCrouch = false;
        }
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
            physicsCalculations.UnCrouch();
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

    //control collision specific items. 
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (hit.normal == -transform.up)
        {

            if (!controller.isGrounded)
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

            if (_Angle > maxSlopeAngle)
            {
                //Debug.Log("Angle thing called"); 
                Vector3 _velocityToObstacle = Vector3.Project(playerVelocity, hit.normal);
                playerVelocity -= _velocityToObstacle;
            }
        }
    }
}
