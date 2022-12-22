using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


[DisallowMultipleComponent]
public class GM_MonsterAttack : MonoBehaviour
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


    private void Awake()
    {
        OnFirePool = new ObjectPool<ParticleSystem>(CreateOnFireSystem);
        //AttackRadius.OnAssultEnter += StartDamagingEnemy;
        //AttackRadius.OnAssultExit += StopDamagingEnemy;
    }

    void Start()
    {
        anim = GetComponent<Animator>();   
    }

    
    void Update()
    {
        Attack();
        FireAttack();
    }

    void Attack()
    {
        if(Input.GetButtonDown("Attack"))
        {
            anim.SetTrigger("LeftAttack");
        }    

    }
    public void OnRightAttackCollision()
    {
        rightAttackCol.SetActive(true);
    }





    #region Fire
    private ParticleSystem CreateOnFireSystem()
    {
        return Instantiate(OnFireSystemPrefab);
    }

    private void StartDamagingEnemy(GM_Assult assult)
    {
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

    private void HandleEnemyDeath(GM_Assult assult)
    {
        assult.OnDeath -= HandleEnemyDeath;
        if (EnemyParticleSystems.ContainsKey(assult))
        {
            StartCoroutine(DelayedDisableBurn(assult, EnemyParticleSystems[assult], BurnDuration));
            EnemyParticleSystems.Remove(assult);
        }
    }

    private IEnumerator DelayedDisableBurn(GM_Assult assult, ParticleSystem Instance, float Duration)
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

    private void StopDamagingEnemy(GM_Assult assult)
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
        if (Input.GetButtonDown("FireAttack")&&fireAttempts>=fireCoolTime)
        {
            anim.SetTrigger("Fire");
            isFireAttacking = true;
            fireAttempts = 0;   
        }
        if(isFireAttacking)
        {
            ShootingSystem.gameObject.SetActive(true);
            AttackRadius.gameObject.SetActive(true);
        }
        if(!isFireAttacking)
        {
            AttackRadius.gameObject.SetActive(false);
            ShootingSystem.gameObject.SetActive(false);
        }
       
    }
    public void EndFiring()
    {
        isFireAttacking=false;
    }
    #endregion
}
