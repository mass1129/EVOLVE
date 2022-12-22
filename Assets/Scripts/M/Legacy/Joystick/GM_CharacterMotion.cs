using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_CharacterMotion : MonoBehaviourPun, IPunObservable
{
    Animator animator;
    CharacterController cc;
    

    Vector2 input;
    Vector3 rootMotion;
    Vector3 velocity;
    Vector3 fixedForward;
    //도착 위치
    Vector3 receivePos;
    //회전되야 하는 값
    Quaternion receiveRot;

    public bool isJumping = false;
    public bool isRushing = false;

    public float jumpHeight;
    public float gravity = -15.0f;
    public float stepDown;
    public float airControl;
    public float jumpDamp;
    public float groundSpeed;
    public float pushPower = 2.0F;
    



    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
            input.x = Input.GetAxis("Horizontal2");
            input.y = Input.GetAxis("Vertical2");

            animator.SetFloat("InputX", input.x);
            animator.SetFloat("InputY", input.y);

            if (Input.GetButtonDown("Jump2"))
            {
                Jump();
            }
        Rush();


    }

    public ParticleSystem rushEffect;
    void Rush()
    {
        if(Input.GetButton("Rush"))
        {
           isRushing = true;
            rushEffect.gameObject.SetActive(true);
        }
        if(Input.GetButtonUp("Rush"))
        {
            isRushing = false;
            rushEffect.gameObject.SetActive(false);
        }
        
        animator.SetBool("Rush", isRushing);
        
    }

    private void OnAnimatorMove()
    {
        rootMotion += animator.deltaPosition;
    }


    private void FixedUpdate()
    {
        if (isJumping)
        {
            UpdateInAir();
        }
        else
        {
            UpdateOnGround();
        }

    }

    private void UpdateOnGround()
    {
        Vector3 stepForwardAmount = rootMotion * groundSpeed;
        Vector3 stepDownAmount = Vector3.down * stepDown;

        cc.Move(stepForwardAmount + stepDownAmount);
        rootMotion = Vector3.zero;

        if (!cc.isGrounded)
        {
            SetinAir(0);
        }
    }

    private void UpdateInAir()
    {
        velocity.y -= gravity * Time.fixedDeltaTime;
        Vector3 displacement = velocity * Time.fixedDeltaTime;
        displacement += CalculateAirControl();
        cc.Move(displacement);
        isJumping = !cc.isGrounded;
        rootMotion = Vector3.zero;
        animator.SetBool("isJumping", isJumping);
    }

    Vector3 CalculateAirControl()
    {
        return ((transform.forward * input.y) + (transform.right * input.x) + (fixedForward * Mathf.Sqrt(2 * gravity * jumpHeight) / 2)) * (airControl / 100);
    }


    void Jump()
    {
        if (!isJumping)
        {
            float jumpVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);
            fixedForward = transform.forward.normalized;
            SetinAir(jumpVelocity);
        }
    }

    private void SetinAir(float jumpVelocity)
    {
        isJumping = true;
        velocity = animator.velocity * jumpDamp * groundSpeed;
        velocity.y = jumpVelocity;

        animator.SetBool("isJumping", true);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
            return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //데이터 보내기
        //if (stream.IsWriting) // isMine == true
        //{
        //    //position, rotation
        //    stream.SendNext(transform.rotation);
        //    stream.SendNext(transform.position);
        //}
        //데이터 받기
        //else if (stream.IsReading) // ismMine == false
        //{
        //    receiveRot = (Quaternion)stream.ReceiveNext();
        //    receivePos = (Vector3)stream.ReceiveNext();
        //}
    }
}