using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class NPC : MonoBehaviour
{
    public Transform player;
    private Rigidbody _body;
    private RaycastHit _toPlayer;
    private List<NPC> _nearNpCs;
    private float _heatCrumbs; // At the time og making a heat map...
    //private float _maxDist; // Will be used in skeletons
    private float _vel; // Velocity max magnitude
    public bool Alarmed { get; private set; }
    public NPC(bool alarmed)
    {
        Alarmed = alarmed;
        _body = gameObject.GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
