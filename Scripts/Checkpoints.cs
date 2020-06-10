using System.Collections.Generic;
using UnityEngine;

public class Checkpoints : MonoBehaviour
{
    [SerializeField] string LayerHitName = "CarCollider"; // The name of the layer set on each car


    private void OnTriggerEnter(Collider other) // Once anything goes through the wall
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(LayerHitName)) // If this object is a car
        {
            other.transform.GetComponentInParent<RLEngine>().CheckpointHit(); // Increase the car's fitness
        }
    }
}
