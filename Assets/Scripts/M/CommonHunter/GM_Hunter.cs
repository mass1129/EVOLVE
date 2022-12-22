using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum HunterStates // Hunter�� �⺻ ����
{
    Idle,
    Move,
    Jump,
    Falling,
    Pushed,
    Heal,
    Groggy,
    Death,
    Global
}

public enum HunterUpperBodyStates // Hunter�� ��ü ����
{
    None,
    Move_Upper,
    Attack,
    GroggyAttack,
    Replace,
    Reload,
    Global
}

// ��� Hunter�� ����ϴ� ������Ʈ Ŭ����
public class GM_Hunter : MonoBehaviourPun, IPunObservable, IDamageable, IBurnable
{
    public int hp;                           // ü��
    public int groggyHp;                     // Groggy ���¿����� ü��
    public int barrier;                      // ��
    public float fuel;                       // ��Ʈ�� ����
    public int weaponType;                   // ���� ���� (0 : ShotGun / 1 : MissileLauncher)
    public float respawnWaitTime = 40;
    public Animator anim;
    public Animator weaponAnim;

    public Slider HpSlider;
    public Slider BarrierSlider;
    public Slider FuelSlider;
    public Slider GroggyHpSlider;
    public Text respawnTimeText;

    public Slider miniHpSlider;
    public Slider miniBarrierSlider;
    public Slider miniGroggyHpSlider;
    public GameObject miniSlider;

    public int weapon0_MaxAmo;
    public int weapon1_MaxAmo;
    public int weapon0_currentAmo;
    public float weapon1_currentAmo;

    public Slider Weapon0Slider;
    public Slider Weapon1Slider;
    public Slider Skill1Slider;
    public Slider Skill2Slider;

    public float yVelocity = 0;
    public float moveSpeed = 2;
    [HideInInspector] public float gravity = -6f; // ���۰� ����� ������ �츮�� ���� �߷� ����
    [HideInInspector] public float jumpPower = 2;
    [HideInInspector] public float h, v;
    [HideInInspector] public Vector3 dir;

    public bool isTracking = false;
    public AudioSource scanSound;

    // �� ��� ������ ���� Property ����
    public int Hp
    {
        set
        {
            hp = Mathf.Clamp(value, 0, 100);
            HpSlider.value = hp;
        }
        get => hp;
    }

    public int GroggyHp
    {
        set
        {
            groggyHp = Mathf.Clamp(value, 0, 100);
            GroggyHpSlider.value = groggyHp;
        }
        get => groggyHp;
    }

    public int Barrier
    {
        set
        {
            barrier = Mathf.Clamp(value, 0, 100);
            BarrierSlider.value = barrier;
        }
        get => barrier;
    }

    public float Fuel
    {
        set
        {
            fuel = Mathf.Clamp(value, 0, 100);
            FuelSlider.value = fuel;
        }
        get => fuel;
    }

    [SerializeField]
    private bool _IsBurning;
    public bool IsBurning { get => _IsBurning; set => _IsBurning = value; }

    private Coroutine BurnCoroutine;

    public event DeathEvent OnDeath;
    public delegate void DeathEvent(GM_Assult Assult);

    public void TakeDamage(int Damage)
    {
        if (Barrier > 0)
        {
            Barrier -= Damage;
        }
        if (Barrier <= 0 && Hp>0)
        {
            Hp -= Damage;
        }
        if(Barrier <= 0 && Hp <=0 && GroggyHp>0)
        {
            GroggyHp -= Damage;
        }

        if (GroggyHp <= 0)
        {
            GroggyHp = 0;
            OnDeath?.Invoke(GetComponent<GM_Assult>());
            StopBurning();
        }
    }

    public void StartBurning(int DamagePerSecond)
    {
        IsBurning = true;
        if (BurnCoroutine != null)
        {
            StopCoroutine(BurnCoroutine);
        }

        BurnCoroutine = StartCoroutine(Burn(DamagePerSecond));
    }

    private IEnumerator Burn(int DamagePerSecond)
    {
        float minTimeToDamage = 1f / DamagePerSecond;
        WaitForSeconds wait = new WaitForSeconds(minTimeToDamage);
        int damagePerTick = Mathf.FloorToInt(minTimeToDamage) + 1;

        TakeDamage(damagePerTick);
        while (IsBurning)
        {
            yield return wait;
            TakeDamage(damagePerTick);
        }
    }

    public void StopBurning()
    {
        IsBurning = false;
        if (BurnCoroutine != null)
        {
            StopCoroutine(BurnCoroutine);
        }
    }

    public HunterStates CurrentState { get; set; } // ���� �⺻ ����
    public HunterUpperBodyStates CurrentUpperBodyState { get; set; } // ���� ��ü ����

    // Hunter�� ������ �ִ� ��� ����, ���� ������. �⺻ ���¿� ��ü ���� ���� ����
    public GM_State<GM_Hunter>[] states;
    public GM_StateMachine<GM_Hunter> stateMachine;
    public GM_State<GM_Hunter>[] upperBodyStates;
    public GM_StateMachine<GM_Hunter> upperBodyStateMachine;


    private void Start()
    {

    }
    private void Update()
    { 
        if (!photonView.IsMine)
            return;

        
        Updated();
        Updated_UpperBody();
        RadarOnOff();

        miniSlider.SetActive(false);
    }

    public void Updated()
    {
        stateMachine.Execute();
    }

    public void Updated_UpperBody()
    {
        upperBodyStateMachine.Execute();
    }

    public void ChangeState(HunterStates newState)
    {
        CurrentState = newState;
        stateMachine.ChangeState(states[(int)newState]);
    }

    public void ChangeState_UpperBody(HunterUpperBodyStates newState)
    {
        CurrentUpperBodyState = newState;
        upperBodyStateMachine.ChangeState(upperBodyStates[(int)newState]);
    }

    public void RevertToPreviousState()
    {
        stateMachine.RevertToPreviousState();
    }
   
    public void RevertToPreviousState_UpperBody()
    {
        upperBodyStateMachine.RevertToPreviousState();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting) // isMine == true
        {
            stream.SendNext(Hp);
            stream.SendNext(Barrier);
            stream.SendNext(GroggyHp);
            stream.SendNext(CurrentState);

            stream.SendNext(Hp);
            stream.SendNext(Barrier);
            stream.SendNext(GroggyHp);
        }
        else if(stream.IsReading) // isMine == false
        {
            Hp = (int)stream.ReceiveNext();
            Barrier = (int)stream.ReceiveNext();
            GroggyHp = (int)stream.ReceiveNext();
            CurrentState = (HunterStates)stream.ReceiveNext();

            miniHpSlider.value = (int)stream.ReceiveNext();
            miniBarrierSlider.value = (int)stream.ReceiveNext();
            miniGroggyHpSlider.value = (int)stream.ReceiveNext();
        }
    }

    // PunRPC �Լ����� �׻� ������Ʈ�� �پ��ִ� ������Ʈ���� ����Ǿ�� �Ѵ�. ���� virtual Ŭ������ �����Ͽ� �� ���������� Ŭ�������� override�� ������ ����� �Ѵ�.
    public virtual void SetTrigger(string s)
    {
        /*photonView.RPC("RpcSetTrigger", RpcTarget.All, s);*/
    }

    [PunRPC]
    public virtual void RpcSetTrigger(string s)
    {
        /*anim.SetTrigger(s);*/
    }

    public virtual void ResetTrigger(string s)
    {
        /*photonView.RPC("RpcResetTrigger", RpcTarget.All, s);*/
    }

    [PunRPC]
    public virtual void RpcResetTrigger(string s)
    {
        /*anim.ResetTrigger(s);*/
    }

    public virtual void SetFloat(string s, float f)
    {
        /*photonView.RPC("RpcSetFloat", RpcTarget.All, s, f);*/
    }

    [PunRPC]
    public virtual void RpcSetFloat(string s, float f)
    {
        /*anim.SetFloat(s, f);*/
    }

    public virtual void Play(string s, int layer, float normallizedTime)
    {
        /*photonView.RPC("RpcPlay", RpcTarget.All, s, layer, normallizedTime);*/
    }

    [PunRPC]
    public virtual void RpcPlay(string s, int layer, float normalizedTime)
    {
        /*anim.Play(s, layer, normalizedTime);*/
    }

    public virtual void Heal() // �޵��� ����ϴ� ��ų
    {
        
    }

    [PunRPC]
    public virtual void RpcHeal()
    {
        
    }

    public virtual void SupportBarrier() // ����Ʈ�� ����ϴ� ��ų
    {

    }

    [PunRPC]
    public virtual void RpcSupportBarrier()
    {

    }

    public virtual void WeaponSetTrigger(string s)  // ����Ʈ ���� �ִϸ����� Ʈ����
    {
        //photonView.RPC("RpcWeaponSetTrigger", RpcTarget.All, s);
    }

    [PunRPC]
    public virtual void RpcWeaponSetTrigger(string s)
    {
        //weaponAnim.SetTrigger(s);
    }

    public virtual void WeaponResetTrigger(string s)  // ����Ʈ ���� �ִϸ����� ���� Ʈ����
    {
        //photonView.RPC("RpcWeaponResetTrigger", RpcTarget.All, s);
    }

    [PunRPC]
    public virtual void RpcWeaponResetTrigger(string s)
    {
        //weaponAnim.ResetTrigger(s);
    }

    public virtual void AssultBarrier()    // ���Ʈ�� ����ϴ� ��ų
    {
        photonView.RPC("RpcAssultBarrier", RpcTarget.All);
    }

    [PunRPC]
    public virtual void RpcAssultBarrier()
    {

    }

    public virtual void GetHeal()
    {
        photonView.RPC("RpcGetHeal", RpcTarget.All);
    }

    [PunRPC]
    public virtual void RpcGetHeal()
    {
        GameObject[] hunters = GameObject.FindGameObjectsWithTag("Player"); // Player �±� �ް� �ִ� ���ӿ�����Ʈ ���� �迭�� ����.
        GameObject groggyHunter;
        for (int i = 0; i < hunters.Length; i++)
        {
            if (Vector3.Distance(hunters[i].transform.position, transform.position) < 1.5f
                && hunters[i].GetComponent<GM_Hunter>().CurrentState == HunterStates.Groggy) // �׷α� ������ ���Ϳ� �Ÿ��� 1.5 �̸��̸�
            {
                groggyHunter = hunters[i];
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Heal") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f)
                {    
                    groggyHunter.GetComponent<GM_Hunter>().Hp = 100;
                }
            }
            if (photonView.IsMine && anim.GetCurrentAnimatorStateInfo(0).IsName("Heal") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                Debug.Log(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
                ChangeState(HunterStates.Idle);
            }
        }
    }

    public virtual void ChangeWeapon(int weaponNum)    // �׷α� ���� <-> �Ϲ� ���� ���� ����
    {
        photonView.RPC("RpcChangeWeapon", RpcTarget.All, weaponNum);
    }

    [PunRPC]
    public virtual void RpcChangeWeapon(int weaponNum)
    {
        if (weaponNum == 0)
            GetComponentInChildren<GM_ChangeWeapon>().ActiveWeapon0();
        else
            GetComponentInChildren<GM_ChangeWeapon>().ActiveGroggyWeapon();
    }

    public virtual void ActiveGroggyIcon()    // �׷α� ���� ������ ��Ƽ��
    {
        photonView.RPC("RpcActiveGroggyIcon", RpcTarget.All);
    }

    [PunRPC]
    public virtual void RpcActiveGroggyIcon()
    {
        gameObject.transform.Find("GroggyCanvas").gameObject.SetActive(true);
    }

    public virtual void InactiveGroggyIcon()    // �׷α� ���� ������ �ξ�Ƽ��
    {
        photonView.RPC("RpcInactiveGroggyIcon", RpcTarget.All);
    }

    [PunRPC]
    public virtual void RpcInactiveGroggyIcon()
    {
        gameObject.transform.Find("GroggyCanvas").gameObject.SetActive(false);
    }

    public virtual void IsTracking(bool trackingState)    // ���� ��ų�� ���� ���̴� Ȱ��ȭ
    {
        photonView.RPC("RpcIsTracking", RpcTarget.All, trackingState);
    }

    [PunRPC]
    public virtual void RpcIsTracking(bool trackingState)
    {
        if (trackingState == true)
            isTracking = true;
        else
            isTracking = false;

        scanSound.Play();
    }

    float currentTrackingTime = 0;
    float trackingMaxTime = 5;
    public Image radarImage;
    Color radarColor = new Color(255, 0, 0, 0);
    float radarAlpha;

    public void RadarOnOff()
    {
        // isTracking�̸� ��ų ���� �ð�(trackingMaxTime)���� ���̴� ����
        if (isTracking)
        {
            currentTrackingTime += Time.deltaTime;

            radarAlpha += Time.deltaTime;
            radarAlpha = Mathf.Clamp(radarAlpha, 0, 1);
            radarColor.a = radarAlpha;
            radarImage.color = radarColor;

            if (currentTrackingTime > trackingMaxTime)
            {
                isTracking = false;
                currentTrackingTime = 0;
            }
        }
        else
        {
            radarAlpha -= Time.deltaTime;
            radarAlpha = Mathf.Clamp(radarAlpha, 0, 1);
            radarColor.a = radarAlpha;
            radarImage.color = radarColor;
        }
    }
}
