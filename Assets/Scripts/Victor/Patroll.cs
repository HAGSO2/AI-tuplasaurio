using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroll : MonoBehaviour
{
    public Transform[] waypoints;

    int n_waypoint;
    bool forward;
    bool playerIsInFront;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        forward = true;
        rb = GetComponent<Rigidbody>();
        MoveToWaypoint(waypoints[0]);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(n_waypoint);
    }

    private void FixedUpdate()
    {
        MoveToWaypoint(waypoints[n_waypoint]);

        RotateMainModel(waypoints[n_waypoint]);
    }

    private void MoveToWaypoint(Transform waypoint)
    {
        Vector3 direction = waypoint.position - transform.position;

        direction.Normalize();

        rb.MovePosition(transform.position + direction * Time.deltaTime * 5f);
    }

    private void RotateMainModel(Transform waypoint)
    {
        if (rb.velocity != Vector3.zero)
        {
            Vector3 direction = waypoint.position - transform.position;

            direction.Normalize();
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);

            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, toRotation, 100f * Time.deltaTime));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // When reaching a waypoint move to the next or the previous waypoint
        if (other.CompareTag("Waypoint"))
        {
            if (n_waypoint + 1 == waypoints.Length)
                forward = false;
            else if (n_waypoint - 1 < 0)
                forward = true;

            if (!forward)
                n_waypoint--;
            else
                n_waypoint++;
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
