using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroll : MonoBehaviour
{
    public Transform[] waypoints;

    int n_waypoint;
    bool forward;
    //bool playerIsInFront;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        forward = true;
        rb = GetComponent<Rigidbody>();

        waypoints[n_waypoint].gameObject.SetActive(true);
        MoveTo(waypoints[0]);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(n_waypoint);
    }

    private void FixedUpdate()
    {
        Patrolling();
    }

    private void Patrolling()
    {
        MoveTo(waypoints[n_waypoint]);

        Rotate(waypoints[n_waypoint]);
    }

    private void MoveTo(Transform waypoint)
    {
        Vector3 direction = waypoint.position - transform.position;

        direction.Normalize();

        rb.MovePosition(transform.position + direction * Time.fixedDeltaTime * 2f);
    }

    private void Rotate(Transform waypoint)
    {
        if (rb.velocity != Vector3.zero)
        {
            Vector3 goal = new Vector3(waypoint.position.x, transform.position.y, waypoint.position.z);

            Vector3 direction = goal - transform.position;
            direction.Normalize();

            float turnSpeed = 25f;

            Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction, turnSpeed * Time.fixedDeltaTime, 0f);

            rb.MoveRotation(Quaternion.LookRotation(desiredForward));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // When reaching a waypoint move to the next or the previous waypoint
        // Also deactivates the used waypoint and activates the next waypoint
        if (other.CompareTag("Waypoint"))
        {
            waypoints[n_waypoint].gameObject.SetActive(false);

            if (n_waypoint + 1 == waypoints.Length)
                forward = false;
            else if (n_waypoint - 1 < 0)
                forward = true;

            if (!forward)
                n_waypoint--;
            else
                n_waypoint++;

            waypoints[n_waypoint].gameObject.SetActive(true);
        }
    }
    /*
    void isPlayerInFront()
    {
        RaycastHit hit;
        if (Physics.Raycast(rayCastTransform.position, transform.forward, out hit, 4f))
        {
            //Debug.Log(hit.transform.tag);
            if (hit.transform.CompareTag("Player"))
            {
                playerIsInFront = true;
            }
            else
            {
                playerIsInFront = false;
            }
        }
        else
        {
            playerIsInFront = false;
        }
    }*/
}
