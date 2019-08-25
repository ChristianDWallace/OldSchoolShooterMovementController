using System.Collections;
using UnityEngine;

public class Hook : MonoBehaviour
{
    [Header("Hook Setup")]
    //GRAPPLE HOOK STUFF
    public GameObject grappleHook; //the hook object to be launched. 
    public Transform anchorPosition; //The position from which the grappling hook is launched and anchored

    [Header("Hook Bools")]
    //HOOK BOOLS
    public bool hookOn; //if the hook has been thrown;
    public bool hookIn; //if the hook has set in something
    public bool shrinkOn; //if we are climbing the chain
    public bool growOn; //if we are falling down the chain

    public bool hookedInEnemy; //if the object we are hookedIn is an enemy

    public bool pulling; //if we have started the pulling coroutine. 

    [Header("Hook Control")]
    //The keyboard shortcut names for the functions of the hook
    public string I_HOOK = "LaunchHook";
    public string I_GROW = "GrowHook";
    public string I_SHRINK = "ShrinkHook";
    public string I_STOP = "DisengageHook";

    [Header("Chain")]
    //CHAIN STUFF
    public float minChainLength = 6f; //the minimum length that the chain can be
    public float maxChainLength = 36f; //the maximum length the chain can be
    public float chainLinkLength = 2f; //the length of an individual link of the chain
    public float growShrinkRate = 6f; //the speed at which we ascend or descend the chain;

    public float pullShrinkRate = 25f; //the rate at which we pull objects towards us. 
    public float minPullLength = 3f; //the minimum distance of a pull

    public float ropeSnapDistance = 5f; //the distance at which the rope will snap if the player is pulled too far away

    public float speed = 35f; //the speed of the hook when thrown. 

    public float restraintForceDefault = 1f; //the value of the restraint that the hook experiences when the player is at the edge of its length. 

    public float waitToPull = .25f; //the time we wait before pulling the chain back to us when stuck to an enemy. 

    private bool canUseHook;

    GameObject currentHook; //the hook we have currently thrown out

    float currentChainLength; //the length of the chain at this moment
    float currentMaxChainLength; //the max length of the chain at the given time. 
    //float airSpeedHolder; //holds the initial air control value

    bool currentMaxSet; //if the current maximum length has been set

    private PlayerControllerPhysicsCalulations physics;

    void Start()
    {
        physics = GetComponent<PlayerControllerPhysicsCalulations>();
    }


    public void UseHook(Vector3 playerVelocity, float gravity, bool isGrounded)
    {
        if (canUseHook && PlayerConrollerInput.GetAlternativeFireButton())
        {
            HookFire();
        }
        //if the hook is latched check for other user input. 
        if (hookIn && canUseHook)
        {
            HookControls(); //Check for user hook input
            HookBehavior(playerVelocity, gravity, isGrounded); //Check for hook behaviour
        }
    }

    //if the hook is fired check these inputs as well. 
    private void HookControls()
    {
        if (Input.GetButtonDown(I_STOP) && hookOn)
        {
            if (currentHook != null)
            {
                currentHook.transform.DetachChildren();
            }
            StartCoroutine(HookPull());
        }
        if (Input.GetButton(I_GROW) && !shrinkOn)
        {
            growOn = true;
        }
        else
        {
            growOn = false;
        }

        if (Input.GetButton(I_SHRINK) && !growOn)
        {
            shrinkOn = true;
        }
        else
        {
            shrinkOn = false;
        }
    }

    //delete the chain and the hook and reset all the values for the chain; 
    public void DropHook()
    {
        //PLAY CHAIN RETURN SOUND
        if (hookedInEnemy)
        {
            currentHook.transform.DetachChildren();
            hookedInEnemy = false;
        }
        Destroy(currentHook);
        hookOn = false;
        currentMaxSet = false;
        hookIn = false;
        pulling = false;
    }

    //The behavior of the hook
    public Vector3 HookBehavior(Vector3 playerVelocity, float gravity, bool isGrounded)
    {
        if (hookIn)
        {
            SetCurrentChainLength();
        }

        Vector3 _chainVector; //the vector of the chain itself

        //If the object we have hooked is not an enemy
        if (!hookedInEnemy)
        {
            Vector3 _velocityToHook; //Players velocity moving to or away from the hook
            float _chainLength; //the length of the chain
            float f1, f2; //the restraints on the chain forces

            //grow the chain when the player prompts it to grow

            if ((hookOn && hookIn && growOn) && (currentChainLength < maxChainLength))
            {
                currentMaxChainLength += (growShrinkRate * Time.deltaTime); //if we are growing then make the chain bigger

                if (currentMaxChainLength > maxChainLength)
                {
                    currentMaxChainLength = maxChainLength;
                }
                //PLAY CHAIN DESCEND SOUND HERE
            }

            //SHRINK THE CHAIN
            if ((hookOn && hookIn && shrinkOn) && (currentChainLength > minChainLength))
            {
                currentMaxChainLength -= growShrinkRate * Time.deltaTime;

                if (currentMaxChainLength < minChainLength)
                {
                    currentMaxChainLength = minChainLength;
                }
                //PLAY CHAIN CLIMB SOUND HERE
            }

            //CHAIN PHYSICS
            _chainVector = currentHook.transform.position - (anchorPosition.position);
            //Debug.Log("chain Vector" + _chainVector);
            _chainLength = Mathf.Abs(Vector3.Magnitude(_chainVector));
            //Debug.Log("Chain Magnitude " + _chainLength); 

            //if the players location is beyond the chain's reach
            if (_chainLength >= currentMaxChainLength)
            {
                //Determine the players velocity component of the chain vector. This means determine the player motion in regards to the chain vector itself 
                _velocityToHook = Vector3.Project(playerVelocity, _chainVector);

                //Debug.Log(_velocityToHook); 

                f2 = (_chainLength - currentChainLength) * restraintForceDefault; //the restrainement force default. This is the difference in our current chain length versus our intended chain length multiplied by some increasing factor. 

                //Debug.Log(f2); 

                //if player's velocity is heading away from the hook
                //the dot function, when using normalized vectors, returns -1 if they are facing away from eachother, and 0 if they are perpendicular.
                if (Vector3.Dot(playerVelocity.normalized, _chainVector.normalized) < 0)
                {
                    //Debug.Log("chainLength: " + _chainLength + " currentChainLength " + currentChainLength);
                    //if the chain has stretched more than 5 units
                    if (_chainLength >= currentChainLength)
                    {
                        //Remove the player's velocity component when moving away from the hook
                        //Debug.Log("velocity to hook: " + _velocityToHook); 
                        playerVelocity = (playerVelocity - _velocityToHook);
                    }

                    f1 = f2; //our force is equal to whatever we have calculated based on our various stipulations
                }
                else //if the player's velocity is heading towards the hook
                {
                    //if the magnitude of our velocity to the hook is less than our restraint force
                    if (Vector3.Magnitude(_velocityToHook) < f2)
                    {
                        f1 = f2 - Vector3.Magnitude(_velocityToHook); //then subtract the difference and assign. 
                    }
                    else
                    {
                        f1 = 0; //else we have no restraint force
                    }
                }
            }
            else
            {
                f1 = 0;
            }

            playerVelocity = physics.ApplyFriction(1 / currentChainLength * 0.11f, playerVelocity, isGrounded);  //Add friction to the rope swing

            //Applies chain restraint
            playerVelocity = (playerVelocity + _chainVector.normalized * f1);
        }
        else if (hookedInEnemy && !pulling)
        {
            pulling = true;
            StartCoroutine(HookPull());
        }

        //Debug.Log("Velocity Of this frame = " + (player.GetCurrentVelocity() + _chainVector.normalized * f1));
        //prevent sticking on the ground caused by the friction routines in the engine
        if (playerVelocity.y > gravity * 0.5)
        {

        }

        return playerVelocity; 
    }

    private void HookFire()
    {
        if (PlayerConrollerInput.GetAlternativeFireButton() && !hookOn)
        {
            currentHook = Instantiate(grappleHook, anchorPosition.position, anchorPosition.rotation); //grab the hook projectile and set its andchor to the child position anchor of the player model. 
            HookInstance hook = currentHook.GetComponent<HookInstance>(); //propel the hook forward.
            hook.SetHookInstance(this);
            hook.currentVelocity = speed * hook.transform.forward;
            hookOn = true; //flip the hook bool to true so that the object knows a hook exists in the world. 
        }
    }



    //pulls the hook towards the player; 
    public IEnumerator HookPull()
    {
        if (pulling)
        {
            yield return new WaitForSeconds(waitToPull);
        }

        float percent = 0;
        float step;
        if (currentHook != null)
        {
            step = (pullShrinkRate / (Vector3.Magnitude(currentHook.transform.position - transform.position) + minPullLength) * Time.fixedDeltaTime);
        }
        else
        {
            step = 1;
        }


        // Debug.Log("We have called the coroutine");

        while (percent < 1)
        {
            if (currentHook != null)
            {
                percent += step;

                currentHook.transform.position = Vector3.Lerp(currentHook.transform.position, anchorPosition.position, percent);

                if (currentChainLength <= minPullLength)
                {
                    currentHook.transform.DetachChildren();
                }
                yield return new WaitForFixedUpdate();
            }
            else
            {
                percent = 1;
            }

        }

        if (currentHook != null)
        {
            DropHook();
        }

    }

    //Set the length of the chain. 
    private void SetCurrentChainLength()
    {
        //if the hook just touched something for the first time with no previous chain length parameters
        if (!currentMaxSet && !hookedInEnemy)
        {
            //set the chain length = the distance the player is from the ball
            currentChainLength = (Vector3.Magnitude(currentHook.transform.position - anchorPosition.position));

            //if we are too far away move closer
            if (currentChainLength > maxChainLength)
            {
                currentChainLength = maxChainLength;
            }
            else if (currentChainLength < minChainLength)
            {
                //if we are too close move further away
                currentChainLength = minChainLength;
            }
        }
        else if (hookedInEnemy)
        {
            //if we are hooked in an enemy we calculate the currentHook value based on the position. 
            currentChainLength = (Vector3.Magnitude(currentHook.transform.position - anchorPosition.position));
        }
        else
        {
            currentChainLength = currentMaxChainLength; //otherwise the chain length should always be the desired length the player has set. 
        }

        //if we just set the length for the first time, then place the distance in our currentMaxChainLength variable and keep track of the fact it is set. 
        if (!currentMaxSet && !hookedInEnemy)
        {
            currentMaxChainLength = currentChainLength;
            currentMaxSet = true;
        }

        if (!hookIn)
        {
            currentChainLength = 0f;
            currentMaxChainLength = 0f;
            currentMaxSet = false;
        }

    }

}
