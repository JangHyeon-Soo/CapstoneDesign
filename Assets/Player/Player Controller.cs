using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Animations.Rigging;


public class PlayerController : MonoBehaviour
{
    [Header("Player")]

    public Transform playerTF;
    public Transform playerCamHolder;
    public Transform playerCam;
    public Transform playerBody;
    
    [Space(20)]

    [Header("Movement")]
    public Rigidbody rb;
    public CharacterController controller;
    public float WalkSpeed;
    public float RunSpeed;

    public float camRotation_y;

    bool isRun;
    float moveSpeed;
    [Space(20)]

    [Header("Mantle Sense Point")]
    public GameObject point1;
    public GameObject point2;
    public GameObject point3;
    #region 점프관련 변수


    public float jumpHeight = 2f; // ���� ����
    public float gravity = -9.81f; // �߷� ��
    private Vector3 velocity;
    private bool isGrounded;
    private bool jumpPressed;
    float forwardForce = 3f;
    private Vector3 moveInput;
    Vector3 movement;
    #endregion
    #region 인풋 액션

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction inputAction;
    InputAction runAction;

    #endregion
    [Space(20)]

    [Header("Animation")]
    public Animator animator;
    public GameObject rigObject;
    public float ikWeight = 0f; // IK 적용 강도 (0 ~ 1)
    public GameObject handPositionSphere;
    public Transform originPosition;

    public string vaultingAnimationName;

    [Range(0.1f, 0.5f)]
    public float IKHandsWidth = 0.2f;

    [Space(20)]

    public Transform Spine1;

    float xRot;
    float yRot;

    bool mantleCheck;
    bool isVaulting;
    GameObject mantleObject;
    Vector3 handPos;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.CursorOff();

        playerBody.localRotation = Quaternion.Euler(0, 0, 0);
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        lookAction = playerInput.actions.FindAction("Look");
        runAction = playerInput.actions.FindAction("Run");

    }

   

    void OnAnimatorIK(int layerIndex)
    {
        if (animator && isVaulting)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            

            animator.SetIKPosition(AvatarIKGoal.LeftHand, handPos + -transform.right * IKHandsWidth);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.LookRotation(transform.forward));

            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);

            animator.SetIKPosition(AvatarIKGoal.RightHand, handPos + transform.right * IKHandsWidth);
            animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(transform.forward));
        }
    }
    // Update is called once per frame
    void Update()
    {
        move();
        look();
        mantleCheck = MantleCheck();

        ikWeight = mantleCheck ? 1 : 0;
       
        if (isVaulting)
        {
            
            ikWeight = Mathf.Lerp(ikWeight, 1, Time.deltaTime * 5f);
            // 애니메이션이 제공하는 deltaPosition을 transform에 적용
            animator.applyRootMotion = true;
            transform.position += animator.deltaPosition;
            transform.rotation = animator.rootRotation;


            
            if (GetComponent<Collider>() is not null)
            {
                GetComponent<Collider>().enabled = false;
            }

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.96)
            {

                transform.position += transform.forward * 0.5f;
                InitializeMovement();
                //rb.useGravity = true;

                if (GetComponent<Collider>() is not null)
                {
                    GetComponent<Collider>().enabled = true;
                }
            }



        }

        else ikWeight = Mathf.Lerp(ikWeight, 1, Time.deltaTime * 5f);


        handPositionSphere.transform.position = handPos;
    }

    public void RootMotionOff()
    {
        animator.applyRootMotion = false;
    }
    public void OnRun(InputValue inputValue)
    {
        isRun = inputValue.Get<float>() == 1 ? true : false;
        Debug.Log(isRun);
    }
    private void InitializeMovement()
    {
        isVaulting = false;
        rb.isKinematic = false;
        animator.applyRootMotion = false;
    }

    public void CorrectPos()
    {
        
    }

    public void move()
    {
        if (isVaulting) return;

        
        moveSpeed = isRun ? RunSpeed : WalkSpeed;
        animator.SetBool("IsRun", isRun);

        moveInput =  isVaulting ? Vector3.zero :moveAction.ReadValue<Vector3>().normalized; 
        movement = transform.forward * moveInput.z + transform.right * moveInput.x;
        movement.y = rb.linearVelocity.y;

        #region Variation of Animator
        if(isGrounded)
        {
            
            if (moveInput.x > 0)
            {
                animator.SetFloat("Horizontal", 1);
            }

            else if (moveInput.x < 0)
            {
                animator.SetFloat("Horizontal", -1);
            }

            else
            {
                animator.SetFloat("Horizontal", 0);
            }


            if (moveInput.z > 0)
            {
                animator.SetFloat("Vertical", 1);
            }

            else if (moveInput.z < 0)
            {
                animator.SetFloat("Vertical", -1);
            }

            else
            {
                animator.SetFloat("Vertical", 0);
            }

        }

        else
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
        }


        animator.SetBool("Is Move", moveInput.magnitude > 0 ? true : false);
        #endregion

        if(!isVaulting && CanMove(movement))
        {
            transform.position += movement * Time.deltaTime * moveSpeed;
        }

        isGrounded = GroundCheck();
    }

    public void look()
    {
        if (isVaulting)
        {
            rigObject.GetComponent<Rig>().weight = 0;
            return;
        }

        


        rigObject.GetComponent<Rig>().weight = 1;
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        
        yRot += lookInput.x * Time.deltaTime * 10f;
        playerTF.rotation = Quaternion.Euler(0, yRot, 0);

        //if (playerCam.GetComponent<CamController>().IsCollided) return;

        xRot -= lookInput.y * Time.deltaTime * 10f;
        xRot = Mathf.Clamp(xRot, -camRotation_y, camRotation_y);
        playerCamHolder.localRotation = Quaternion.Euler(xRot, 0, 0);
    }

    public void OnJump(InputValue inputValue)
    {
        if (isVaulting) return;

        isGrounded = GroundCheck();

        if (!isGrounded)
        {

            if (mantleCheck)
            {

                isVaulting = true;
                rb.isKinematic = true;

                RaycastHit hit;
                bool isHit = Physics.CapsuleCast(transform.position + transform.forward * -0.25f,
                transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height, 0), 0.25f, transform.forward, out hit, 0.75f);

                Collider col = mantleObject.GetComponent<Collider>();
                transform.position = new Vector3(hit.point.x, col.bounds.center.y + col.bounds.size.y / 2 - 1.7f, hit.point.z);

                animator.Play(vaultingAnimationName);
                return;
            }

            else return;


        }

        else
        {
            if (moveInput == Vector3.zero)
            {
                animator.SetTrigger("Jump");
                //animator.applyRootMotion = true;
            }
            else
            {
                animator.SetTrigger("Jump");
                //animator.applyRootMotion = true;
                Jump();
            }
        }
      
    }

    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    public bool GroundCheck()
    {
        RaycastHit hit;
        if(Physics.Raycast(point1.transform.position, Vector3.down, out hit, 0.45f))
        {
            //animator.applyRootMotion = false;
            return true;
        }

        //animator.applyRootMotion = true;
        return false;
    }

    public bool MantleCheck()
    {

        RaycastHit hit;
        bool isHit = Physics.CapsuleCast(transform.position + transform.forward * -0.25f,
            transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height, 0), 0.25f,transform.forward, out hit, 0.75f);
        Debug.DrawRay(point1.transform.position + transform.forward * -0.25f, point1.transform.forward * 0.75f, isHit ? Color.red : Color.white, Time.deltaTime, false);
        if (isHit)
        {
            mantleObject = hit.transform.gameObject;

            Vector3 center = mantleObject.GetComponent<Collider>().bounds.center;
            // 박스의 크기를 로컬에서 월드 크기로 변환
            Vector3 size = mantleObject.GetComponent<Collider>().bounds.size;
            Vector3 result = new Vector3(hit.point.x, center.y + size.y / 2, hit.point.z);

            if ( Mathf.Abs(result.y - transform.position.y )<= 2.5f)
            {
                handPos = result;
                return true;
            }
        }

        mantleObject = null;
        return false;

 

    }

    bool CanMove(Vector3 direction)
    {
        RaycastHit hit;
        return !Physics.Raycast(point1.transform.position, direction, out hit, 0.5f);

    }

    public bool Get_isVaulting()
    {
        return isVaulting || !isGrounded;
    }

}
