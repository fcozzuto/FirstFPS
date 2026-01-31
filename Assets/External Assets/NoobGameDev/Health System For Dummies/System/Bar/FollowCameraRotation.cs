using UnityEngine;

public class FollowCameraRotation : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        target = PlayerController.playerTransform;
        if (target == null)
        {
            target = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + target.forward);
    }
}
