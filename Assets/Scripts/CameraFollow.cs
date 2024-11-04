using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform objectToFollow;
    [SerializeField] Vector3 offset;
    Vector3 velocity;
    void FixedUpdate()
    {
        if(objectToFollow != null)
        {
            Vector3 desiredPosition = objectToFollow.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, .25f);
        }
    }
}
