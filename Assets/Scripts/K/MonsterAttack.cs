using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using Photon.Pun;

[DisallowMultipleComponent]
public class MonsterAttack : MonoBehaviourPun, IPunObservable
{
    Animator anim;
    [SerializeField]
    private GameObject rightAttackCol;

    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private ParticleSystem OnFireSystemPrefab;
    [SerializeField]
    private FlamethrowerAttackRadius AttackRadius;

    [Space]
    [SerializeField]
    private int BurningDPS = 5;
    [SerializeField]
    private float BurnDuration = 3f;

    private ObjectPool<ParticleSystem> OnFirePool;

    private Dictionary<GM_Hunter, ParticleSystem> EnemyParticleSystems = new();

    ThrowRock throwRock;
    public Image[] skillImg = new Image[2];
    private void Awake()
    {
        OnFirePool = new ObjectPool<ParticleSystem>(CreateOnFireSystem);
        AttackRadius.OnAssultEnter += StartDamagingEnemy;
        AttackRadius.OnAssultExit += StopDamagingEnemy;
    }

    void Start()
    {
        anim = GetComponent<Animator>();   
        throwRock = GetComponent<ThrowRock>();  
    }

    
    void Update()
    {
        
        //내것이 아니라면 함수를 끝낸다.
        if (!photonView.IsMine) return;
        Attack();
        FireAttack();
        ThrowRockAttack();
    }
    public bool isAttacking= false;
    IEnumerator CheckingAttacking()
    {
        isAttacking = true;
        
        do
        {
            yield return new WaitForSeconds(0.01f);
        } while (anim.GetCurrentAnimatorStateInfo(1).IsTag("Idle"));
        isAttacking = false;
    }
    
    IEnumerator CreateRock()
    {
        createRock = true;
        throwRock.accumulatedTime = 0;
        throwRock.StopFiring();
        yield return new WaitForSeconds(0.03f);
        throwRock.StartFiring();
        yield return new WaitForSeconds(0.01f);



        createRock = false;
        anim.SetBool("ThrowRock", false);
        isRiftingRock = false;
        throwRock.StopFiring();
    }

    void Attack()
    {
        if(Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("LeftAttack");
        }    

    }

    public void OnRightAttackCollision()
    {
        rightAttackCol.SetActive(true);
        anim.ResetTrigger("LeftAttack");
    }
    public GameObject jumpAttackImpact;
    public Transform jumpAttackImpactPos;
    public GameObject jumpAttackCol;
    public float jumpVolume = 0.5f;
    public void OnJumpAttack()
    {
        
        var impact = Instantiate(jumpAttackImpact) as GameObject;
        impact.transform.position = jumpAttackImpactPos.position;
        attackSoundSource.volume = jumpVolume;
        attackSoundSource.PlayOneShot(attackSFXList[1]);
        Destroy(impact, 2);
        jumpAttackCol.SetActive(true);
    }

    public Transform crossHairTarget;
    public bool createRock = false;
    public GameObject Rock;
    bool trailEffectOn = false;
    public void IsRiftingRock()
    {
        trailEffectOn = true;
        isRiftingRock = true;
    }
    public void isAttackingTrue()
    {
        isAttacking = true;
    }
    public void isAttackingFalse()
    {
        isAttacking = false;
    }
    #region Fire
    private ParticleSystem CreateOnFireSystem()
    {
        return Instantiate(OnFireSystemPrefab);
    }

    private void StartDamagingEnemy(GM_Hunter assult)
    {
        //photonView.RPC("RpcShowBulletImpact", RpcTarget.All, assult);
        if (assult.TryGetComponent<IBurnable>(out IBurnable burnable))
        {
            burnable.StartBurning(BurningDPS);
            assult.OnDeath += HandleEnemyDeath;
            ParticleSystem onFireSystem = OnFirePool.Get();
            onFireSystem.transform.SetParent(assult.transform, false);
            onFireSystem.transform.localPosition = Vector3.zero;
            ParticleSystem.MainModule main = onFireSystem.main;
            main.loop = true;
            EnemyParticleSystems.Add(assult, onFireSystem);
        }
    }
    //[PunRPC]
    //void RpcStartDamagingEnemy(GM_Hunter assult)
    //{
        
    //}


    private void HandleEnemyDeath(GM_Hunter assult)
    {
        assult.OnDeath -= HandleEnemyDeath;
        if (EnemyParticleSystems.ContainsKey(assult))
        {
            StartCoroutine(DelayedDisableBurn(assult, EnemyParticleSystems[assult], BurnDuration));
            EnemyParticleSystems.Remove(assult);
        }
    }

    private IEnumerator DelayedDisableBurn(GM_Hunter assult, ParticleSystem Instance, float Duration)
    {
        ParticleSystem.MainModule main = Instance.main;
        main.loop = false;
        yield return new WaitForSeconds(Duration);
        Instance.gameObject.SetActive(false);
        if (assult.TryGetComponent<IBurnable>(out IBurnable burnable))
        {
            burnable.StopBurning();
        }
    }

    private void StopDamagingEnemy(GM_Hunter assult)
    {
        assult.OnDeath -= HandleEnemyDeath;
        if (EnemyParticleSystems.ContainsKey(assult))
        {
            StartCoroutine(DelayedDisableBurn(assult, EnemyParticleSystems[assult], BurnDuration));
            EnemyParticleSystems.Remove(assult);
        }
    }

    bool isFireAttacking;
    public float fireCoolTime = 10;
    float fireAttempts=10;
    void FireAttack()
    {
        fireAttempts += Time.deltaTime;
        skillImg[0].fillAmount = 1 - fireAttempts / fireCoolTime;
        if (Input.GetKeyDown(KeyCode.Alpha1)&&fireAttempts>=fireCoolTime)
        {
            anim.SetTrigger("Fire");
            isFireAttacking = true;
            fireAttempts = 0;   
        }
        if (isFireAttacking)
        {
            photonView.RPC("RpcFireActive", RpcTarget.All,true);
        }
        if (!isFireAttacking)
        {
            photonView.RPC("RpcFireActive", RpcTarget.All, false);
        }
        

    }
    public AudioSource attackSoundSource;
    public AudioClip[] attackSFXList;
    public float fireVolume = 0.1f;
    [PunRPC]
    void RpcFireActive(bool x)
    {
        ShootingSystem.gameObject.SetActive(x);

        AttackRadius.gameObject.SetActive(x);
        
       
    }
    private void FireSFX()
    {
        AudioClip clip = GetRunRandomClip(0, 0);
        attackSoundSource.volume = fireVolume;
        attackSoundSource.PlayOneShot(clip);

    }

    private AudioClip GetRunRandomClip(int a, int b)
    {
        int index = Random.Range(a, b);
        return attackSFXList[index];
    }
    public void EndFiring()
    {
        isFireAttacking=false;
    }

    bool isRiftingRock = false;
    public float rockCoolTime = 10;
    float rockAttempts = 10;
    [PunRPC]
    void RpcActiveRock(bool x)
    {
        Rock.SetActive(x);

    }

        #endregion
        void ThrowRockAttack()
        {   
            rockAttempts += Time.deltaTime;
            skillImg[1].fillAmount = 1 - rockAttempts / rockCoolTime;
        if (Input.GetKeyDown(KeyCode.Alpha3) &&rockAttempts >= rockCoolTime)
            {
                if (!isRiftingRock)
                {   
                    StopAllCoroutines();
                    rockAttempts = 0;
                    photonView.RPC("RpcActiveRock", RpcTarget.All, true);
                    anim.SetBool("ThrowRock", true);
                       
                }

               

            }
             if(trailEffectOn)
                {   
                    throwRock.hitEffect.gameObject.SetActive(true);
                    throwRock.StartFiring();
                }
        if (Input.GetKeyDown(KeyCode.Alpha3)&& isRiftingRock)
        {
            StopAllCoroutines();
            trailEffectOn = false;
            throwRock.hitEffect.gameObject.SetActive(false) ;
            photonView.RPC("RpcActiveRock", RpcTarget.All, false);
           
            StartCoroutine(CreateRock());
            
        }
        throwRock.UpdateWeapon(Time.deltaTime, crossHairTarget.position, createRock);


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
