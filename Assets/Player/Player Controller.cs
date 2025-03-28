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
    
    [Space(20)]

    [Header("Movement")]
    public Rigidbody rb;
    public CharacterController controller;
    public float WalkSpeed;
    public float RunSpeed;

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


        if(isVaulting)
        {
            animator.applyRootMotion = true;
            // 애니메이션이 제공하는 deltaPosition을 transform에 적용
            if (GetComponent<Collider>() is not null)
            {
                GetComponent<Collider>().enabled = false;
            }

            transform.position += animator.deltaPosition;
            transform.rotation = animator.rootRotation;
            

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

        handPositionSphere.transform.position = handPos;
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


        moveSpeed = isRun ? WalkSpeed : moveSpeed;
        animator.SetBool("IsRun", isRun);

        moveInput = moveAction.ReadValue<Vector3>().normalized; 
        movement = transform.forward * moveInput.z + transform.right * moveInput.x;
        movement.y = 0;

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

        animator.SetBool("Is Move", movement.magnitude > 0);
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
        xRot -= lookInput.y * Time.deltaTime * 10f;

        xRot = Mathf.Clamp(xRot, -60f, 60f);
        yRot += lookInput.x * Time.deltaTime * 10f;

        playerTF.rotation = Quaternion.Euler(0, yRot, 0);
        playerCamHolder.localRotation = Quaternion.Euler(xRot, 0, 0);
        //Spine1.transform.localRotation = Quaternion.Euler(xRot, Spine1.transform.localRotation.y, Spine1.transform.localRotation.z);
    }

    public void OnJump(InputValue inputValue)
    {
        isGrounded = GroundCheck();

        if (!isGrounded)
        {
            if (mantleCheck)
            {
                isVaulting = true;
                rb.isKinematic = true;
                transform.position =
                    new Vector3(transform.position.x, mantleObject.GetComponent<Collider>().bounds.center.y + mantleObject.GetComponent<Collider>().bounds.size.y / 2 - 1.7f, transform.position.z);

                animator.Play("Braced Hang To Crouch");
            }

            
        }

        else
        {
            if (moveInput == Vector3.zero)
            {
                animator.SetTrigger("Jump");
            }
            else
            {
                animator.SetTrigger("Jump");
                Jump();
            }
        }
      
    }

    public void Jump()
    {
        //if (!CanMove(movement)) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    public bool GroundCheck()
    {
        RaycastHit hit;
        if(Physics.Raycast(point1.transform.position, Vector3.down, out hit, 0.5f))
        {
            return true;
        }

        return false;
    }

    public bool MantleCheck()
    {

        RaycastHit hit;
        bool isHit = Physics.CapsuleCast(transform.position,
            transform.position + new Vector3(0, GetComponent<CapsuleCollider>().height, 0), 0.25f,transform.forward, out hit, 0.5f);
        Debug.DrawRay(point1.transform.position, point1.transform.forward * 0.5f, isHit ? Color.red : Color.white, Time.deltaTime, false);
        if (isHit)
        {
            mantleObject = hit.transform.gameObject;

            Vector3 center = mantleObject.GetComponent<Collider>().bounds.center;
            // 박스의 크기를 로컬에서 월드 크기로 변환
            Vector3 size = mantleObject.GetComponent<Collider>().bounds.size;
            Vector3 result = new Vector3(hit.point.x, center.y + size.y / 2, hit.point.z);

            if(result.y - transform.position.y <= 2.5f)
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
        return !Physics.Raycast(point1.transform.position, direction, out hit, 0.3f);

    }

}
