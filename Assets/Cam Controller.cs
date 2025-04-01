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
        switch (playerController.cameraMode)
        {
            case GameManager.CameraMode.FP:
                transform.position = Vector3.Lerp(transform.position, CameraFollowTarget.position, Time.deltaTime * (playerController.Get_isVaulting() ? 7f : 3f));
                Quaternion targetRot = CameraFollowTarget.rotation;
                targetRot.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (playerController.Get_isVaulting() ? 7f : 3f));
                break;


            case GameManager.CameraMode.TP:
                transform.position = Vector3.Lerp(transform.position, playerController.thirdPersonTF.position, Time.deltaTime * 5);
                transform.rotation = Quaternion.Slerp(transform.rotation, playerController.thirdPersonTF.rotation, Time.deltaTime * 5);
                break;

                break;

        }

        
    }
}
