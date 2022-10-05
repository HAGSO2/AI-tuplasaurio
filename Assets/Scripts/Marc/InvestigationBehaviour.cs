using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigationBehaviour : MonoBehaviour
{
    [Tooltip("Choose view range")]
    [SerializeField] float viewRange = 40;
    [SerializeField] float maximumDistanceCheck = 5;
    [SerializeField] float speed = 10f;
    [SerializeField] float turnSpeed = 10f;

    [Tooltip("Select the layer you want the NPC to avoid")]
    [SerializeField] LayerMask layerMaskForInvestigation;

    Vector3 minimumDirection;
    Vector3 maximumDirection;

    Vector3 randomDirection;
    Vector3 targetPoint;

    float movementSpeed;

    Rigidbody rb;

    bool investigating = false;
    bool choseDirection = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        investigating = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(investigating)
        {
            if (!choseDirection)    // WHEN INVESTIGATING CHOSE A DIRECTION
            {
                ChooseRandomDirection();
                // Check if this random direction is valid
                if (!Physics.Raycast(transform.position, randomDirection, maximumDistanceCheck, layerMaskForInvestigation))
                {
                    targetPoint = transform.position + randomDirection * maximumDistanceCheck;

                    choseDirection = true;
                    Debug.Log("Direction Chosen!");
                    Debug.Log(targetPoint);
                }
            }
            else    // WHEN A DIRECTION IS CHOSEN, MOVE TOWARDS THE OBJECTIVE
            {
                MoveTo(targetPoint);
            }
        }
    }

    // MOVING
    private void MoveTo(Vector3 target)
    {
        if (DistanceLessThan(0.75f, target))
            movementSpeed = 0;
        else
            movementSpeed = 2;

        Rotate(target);

        rb.MovePosition(transform.position + transform.forward * Time.fixedDeltaTime * movementSpeed);
    }

    private bool DistanceLessThan(float distance, Vector3 target)
    {
        return Vector3.Distance(target, transform.position) < distance;
    }
    private void Rotate(Vector3 target)
    {
        if (rb.velocity != Vector3.zero)
        {
            Vector3 goal = new Vector3(target.x, transform.position.y, target.z);

            Vector3 direction = goal - transform.position;
            direction.Normalize();

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.fixedDeltaTime, 0f);

            rb.MoveRotation(Quaternion.LookRotation(desiredForward));
        }
    }

    // DIRECTIONS
    void ChooseRandomDirection()
    {
        Debug.Log("Chosing direction...");
        float randomAngle = Random.Range(-viewRange / 2, viewRange / 2);
        randomDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
    }


    // GIZMO STUFF Visual feedback in editor for easy tweaking of values
    private void OnValidate()
    {
        minimumDirection = Quaternion.AngleAxis(-viewRange / 2, Vector3.up) * transform.forward;
        maximumDirection = Quaternion.AngleAxis(viewRange / 2, Vector3.up) * transform.forward;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, transform.forward * maximumDistanceCheck);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, minimumDirection * maximumDistanceCheck);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, maximumDirection * maximumDistanceCheck);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, randomDirection * maximumDistanceCheck);
    }
}
