using UnityEngine;

public class CamController : MonoBehaviour
{
    public PlayerController playerController;
    public bool CollisionDetect;
    public LayerMask collisionLayer;

    public Transform headBone;
    public Transform CameraFollowTarget;
    public Transform aimPoint;

    [Range(1, 5)]
    public float stabilizeSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, CameraFollowTarget.position, Time.deltaTime * (playerController.Get_isVaulting()? 20f : stabilizeSpeed));
        transform.rotation = Quaternion.Slerp(transform.rotation, CameraFollowTarget.rotation, Time.deltaTime * (playerController.Get_isVaulting() ? 20f : stabilizeSpeed));
    }
}
