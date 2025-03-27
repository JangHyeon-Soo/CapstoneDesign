using UnityEngine;

public class CamController : MonoBehaviour
{
    public Transform HeadBone;
    public Transform aimPoint;

    [Range(1, 5)]
    public float stabilizeSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, HeadBone.position, Time.deltaTime * stabilizeSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, HeadBone.rotation, Time.deltaTime * stabilizeSpeed);
    }
}
