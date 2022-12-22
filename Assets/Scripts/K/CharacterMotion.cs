using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CharacterMotion : MonoBehaviourPun, IPunObservable
{
    Animator animator;
    CharacterController cc;
    MonsterAttack mAttack;

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

    public GameObject rushAttackCol;


    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;
    public Image[] skillImg = new Image[2];
    private void OnAnimatorMove()
    {
        rootMotion += animator.deltaPosition;
    }

    private void Awake()
    {
        if (!photonView.IsMine)
            this.enabled = false;
    }
    private void FixedUpdate()
    {


    }
    void Start()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        GameManager.instance.AddPlayer(photonView);
        mAttack = GetComponent<MonsterAttack>();
    }

    // Update is called once per frame
    void Update()
    {
        

        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        animator.SetFloat("InputX", input.x);
        animator.SetFloat("InputY", input.y);

        jumpAttempts += Time.deltaTime;
        skillImg[1].fillAmount = 1 - jumpAttempts / jumpCoolTime;
        if (Input.GetKeyDown(KeyCode.Space)&& !isRushing)
        {
            Jump();
        }
        Rush();
        GroundedCheck();



        if (isJumping)
        {
            UpdateInAir();
        }
        else
        {
            UpdateOnGround();
        }
        
    }
    public AudioClip[] stepClip;
    public AudioClip[] runClip;
    public AudioSource moveAudioSource;

    private void insideStep()
    {
        AudioClip clip = GetRandomClip(0, 4);
        moveAudioSource.volume = walkVolume;
        moveAudioSource.PlayOneShot(clip);

    }

    private AudioClip GetRandomClip(int a, int b)
    {
        int index = Random.Range(a, b);
        return stepClip[index];
    }
    public float runVolume = 0.5f;
    public float walkVolume = 0.8f;
    private void RunSFX()
    {
        AudioClip clip = GetRunRandomClip(0, 4);
        moveAudioSource.volume = runVolume;
        moveAudioSource.PlayOneShot(clip);

    }

    private AudioClip GetRunRandomClip(int a, int b)
    {
        int index = Random.Range(a, b);
        return runClip[index];
    }


    #region Rush
    public ParticleSystem rushEffect;
    public float rushCoolTime = 10;
    float rushAttempts = 10;
    void Rush()
    {
        rushAttempts += Time.deltaTime;
        skillImg[0].fillAmount = 1 - rushAttempts / rushCoolTime;
        if(Input.GetKey(KeyCode.Alpha2) && !isJumping && rushAttempts>=rushCoolTime)
        {
            rushAttempts = 0;
            photonView.RPC("RpcRushAttack", RpcTarget.All, true);
            
        }
        if (Input.GetKeyUp(KeyCode.Alpha2) || rushAttempts >= 3)
        {
           
            photonView.RPC("RpcRushAttack", RpcTarget.All, false);
           
        }
        
        animator.SetBool("Rush", isRushing);
        
    }
    void OnRushColOn()
    {
        rushAttackCol.SetActive(true);
    }
    [PunRPC]
    void RpcRushAttack(bool x)
    {
        isRushing = x;
        rushEffect.gameObject.SetActive(x);
        if(!x)
        rushAttackCol.SetActive(x);

    }
    #endregion

  

    private void UpdateOnGround()
    {
        Vector3 stepForwardAmount = rootMotion * groundSpeed;
        Vector3 stepDownAmount = Vector3.down * stepDown;

        cc.Move(stepForwardAmount + stepDownAmount);
        rootMotion = Vector3.zero;

        if (!Grounded)
        {
            SetinAir(0);
        }
    }

    private void UpdateInAir()
    {
        velocity.y -= gravity * Time.deltaTime;
        Vector3 displacement = velocity * Time.deltaTime;
        displacement += CalculateAirControl();
        cc.Move(displacement);
        isJumping = !cc.isGrounded;
        rootMotion = Vector3.zero;
        animator.SetBool("isJumping", isJumping);
    }
    public float jumpFowardPower = 5;
    Vector3 CalculateAirControl()
    {
        return ((transform.right * input.x) + (fixedForward * Mathf.Clamp(input.y,0f,1f) * Mathf.Sqrt(2 * gravity * jumpHeight))) * (airControl / 100);
    }
    public float jumpCoolTime = 5;
    float jumpAttempts = 5;

    void Jump()
    {
        if (!isJumping&&!mAttack.isAttacking&&jumpAttempts>=jumpCoolTime)
        {
            jumpAttempts = 0;
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
    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

       
    }
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
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