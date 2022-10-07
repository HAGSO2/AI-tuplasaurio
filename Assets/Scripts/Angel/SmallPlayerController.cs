using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class SmallPlayerController : MonoBehaviour
{
    public Transform[] points;
    private int _indpos;
    private Vector3 _movDir;
    public float vel = 2.5f;

    private Rigidbody _body;
    // Start is called before the first frame update
    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _indpos = 0;
        StartCoroutine(GetTo(points[0]));
    }

    // Update is called once per frame
    private IEnumerator GetTo(Transform desired)
    {
        set_speed(desired.position);
        yield return new WaitUntil(() => _body.velocity.magnitude < 5);
        if ((transform.position - desired.position).magnitude > 0.2f)
            StartCoroutine(GetTo(desired));
        else
        {
            _indpos++;
            if(_indpos < points.Length)
                StartCoroutine(GetTo(points[_indpos]));
        }
    }
    private void set_speed(Vector3 runnigAt)//if stop == true will stop on the point is running, if not, will not stop
    {
        //Changes speed direction to go to the runningPoint

        Vector3 position = transform.position;
        Vector3 desired = runnigAt - position;
        Vector3 steering = desired - _body.velocity;
        steering.Normalize();
        if (Vector3.Angle(desired, steering) < 30 || (transform.position - runnigAt).magnitude > 10)
        {
            _movDir = steering;
            _body.AddForce(steering * vel);
        }
        else
        {
            _body.AddForce(_movDir * vel);
        }
    }
}
