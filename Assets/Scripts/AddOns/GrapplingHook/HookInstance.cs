using UnityEngine;

public class HookInstance : MonoBehaviour
{
    public Vector3 currentPosition;
    public Vector3 currentVelocity;
    public LayerMask layerMask; //what we collide with; 
    public float damage = 10f; //the damage that we deal if we hit an enemy

    public Hook grapplingHook;

    public bool isLatched = false; //whether the grapple has grabbed an object. 

    public float padding = 5f;

    private Rigidbody myRB; 

    Vector3 newPosition = Vector3.zero;
    Vector3 newVelocity = Vector3.zero;

    public void SetHookInstance(Hook grapplingHook)
    {
        this.grapplingHook = grapplingHook;
    }

    private void Start()
    {
        //On start make an array of all the colliders that our projectile is inside of at spawn.
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, layerMask);
        //if we are colliding with something that has layerMask enemy then destroy that object. 
        if (initialCollisions.Length > 0)
        {

            OnHitObject(initialCollisions[0], transform.position, transform.position);
        }

        myRB.AddForce(currentVelocity, ForceMode.Impulse);
    }

    private void Update()
    {
        if ((Vector3.Magnitude(gameObject.transform.position - grapplingHook.anchorPosition.position) > grapplingHook.maxChainLength + padding) && !isLatched)
        {
            grapplingHook.StartCoroutine(grapplingHook.HookPull());
        }
    }
    

    //Did we hit a target
    void CheckHit()
    {
        //get teh direction that we fired in (along the x axis); 
        Vector3 fireDirection = (newPosition - currentPosition).normalized;
        //get the fire distance between the currentposition this frame and the next position
        float fireDistance = Vector3.Distance(newPosition, currentPosition);

        RaycastHit hit;
        if (Physics.Raycast(currentPosition, fireDirection, out hit, fireDistance, layerMask))
        {
            OnHitObject(hit.collider, hit.point, fireDirection);
        }
    }

    //Add a method here to check and see if the hook is further than max chain lenght and if so pull it back. 


    //Handle collisions. 
    void OnHitObject(Collider c, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (c.gameObject.tag == "Obstacle" || c.gameObject.tag == "Floor")
        {
            gameObject.transform.position = hitPoint;
            isLatched = true;
            grapplingHook.hookedInEnemy = false;
            grapplingHook.hookIn = true;
        }
        else
        {
            grapplingHook.StartCoroutine(grapplingHook.HookPull());
        }
    }
}
