using UnityEngine;

public class CamController : MonoBehaviour
{
    public bool CollisionDetect;


    public Transform headBone;
    public Transform CameraFollowTarget;
    public Transform aimPoint;

    [Range(1, 5)]
    public float stabilizeSpeed;


    // Update is called once per frame
    void Update()
    {
        if(CollisionDetect)
        {
            transform.position = Vector3.Lerp(transform.position, CameraFollowTarget.position, Time.deltaTime * stabilizeSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, CameraFollowTarget.rotation, Time.deltaTime * stabilizeSpeed);
            return;
        }

        RaycastHit hit;

        bool isHit = Physics.Raycast(headBone.position, CameraFollowTarget.position, out hit, (CameraFollowTarget.position - headBone.position).magnitude);

        if(!isHit)
        {
            transform.position = Vector3.Lerp(transform.position, CameraFollowTarget.position, Time.deltaTime * stabilizeSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, CameraFollowTarget.rotation, Time.deltaTime * stabilizeSpeed);
        }

        else
        {
            transform.position = Vector3.Lerp(transform.position, hit.point + -(CameraFollowTarget.position - headBone.position).normalized * 0.2f, Time.deltaTime * stabilizeSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, CameraFollowTarget.rotation, Time.deltaTime * stabilizeSpeed);
        }
    }
}
