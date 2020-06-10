using UnityEngine;

public class WallsDetector : MonoBehaviour
{
    [SerializeField] string LayerHitName = "CarCollider"; // The name of the layer set on each car

    private void OnCollisionEnter(Collision collision) // Once anything hits the wall
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(LayerHitName)) // Make sure it's a car
        {
            collision.transform.GetComponent<RLEngine>().WallHit(); // If it is a car, tell it that it just hit a wall
        }
    }
}
