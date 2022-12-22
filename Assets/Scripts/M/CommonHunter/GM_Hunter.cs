using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum HunterStates // Hunter의 기본 상태
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

public enum HunterUpperBodyStates // Hunter의 상체 상태
{
    None,
    Move_Upper,
    Attack,
    GroggyAttack,
    Replace,
    Reload,
    Global
}

// 모든 Hunter가 사용하는 에이전트 클래스
public class GM_Hunter : MonoBehaviourPun, IPunObservable, IDamageable, IBurnable
{
    public int hp;                           // 체력
    public int groggyHp;                     // Groggy 상태에서의 체력
    public int barrier;                      // 방어막
    public float fuel;                       // 제트팩 연료
    public int weaponType;                   // 무기 종류 (0 : ShotGun / 1 : MissileLauncher)
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
    [HideInInspector] public float gravity = -6f; // 원작과 비슷한 느낌을 살리기 위해 중력 낮춤
    [HideInInspector] public float jumpPower = 2;
    [HideInInspector] public float h, v;
    [HideInInspector] public Vector3 dir;

    public bool isTracking = false;
    public AudioSource scanSound;

    // 각 멤버 변수에 대한 Property 정의
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

    public HunterStates CurrentState { get; set; } // 현재 기본 상태
    public HunterUpperBodyStates CurrentUpperBodyState { get; set; } // 현재 상체 상태

    // Hunter가 가지고 있는 모든 상태, 상태 관리자. 기본 상태와 상체 상태 따로 관리
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

    // PunRPC 함수들은 항상 오브젝트에 붙어있는 컴포넌트에서 실행되어야 한다. 따라서 virtual 클래스로 구현하여 각 직업군들의 클래스에서 override로 재정의 해줘야 한다.
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

    public virtual void Heal() // 메딕이 사용하는 스킬
    {
        
    }

    [PunRPC]
    public virtual void RpcHeal()
    {
        
    }

    public virtual void SupportBarrier() // 서포트가 사용하는 스킬
    {

    }

    [PunRPC]
    public virtual void RpcSupportBarrier()
    {

    }

    public virtual void WeaponSetTrigger(string s)  // 서포트 무기 애니메이터 트리거
    {
        //photonView.RPC("RpcWeaponSetTrigger", RpcTarget.All, s);
    }

    [PunRPC]
    public virtual void RpcWeaponSetTrigger(string s)
    {
        //weaponAnim.SetTrigger(s);
    }

    public virtual void WeaponResetTrigger(string s)  // 서포트 무기 애니메이터 리셋 트리거
    {
        //photonView.RPC("RpcWeaponResetTrigger", RpcTarget.All, s);
    }

    [PunRPC]
    public virtual void RpcWeaponResetTrigger(string s)
    {
        //weaponAnim.ResetTrigger(s);
    }

    public virtual void AssultBarrier()    // 어썰트가 사용하는 스킬
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
        GameObject[] hunters = GameObject.FindGameObjectsWithTag("Player"); // Player 태그 달고 있는 게임오브젝트 전부 배열에 담음.
        GameObject groggyHunter;
        for (int i = 0; i < hunters.Length; i++)
        {
            if (Vector3.Distance(hunters[i].transform.position, transform.position) < 1.5f
                && hunters[i].GetComponent<GM_Hunter>().CurrentState == HunterStates.Groggy) // 그로기 상태인 헌터와 거리가 1.5 미만이면
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

    public virtual void ChangeWeapon(int weaponNum)    // 그로기 상태 <-> 일반 상태 무기 변경
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

    public virtual void ActiveGroggyIcon()    // 그로기 상태 아이콘 액티브
    {
        photonView.RPC("RpcActiveGroggyIcon", RpcTarget.All);
    }

    [PunRPC]
    public virtual void RpcActiveGroggyIcon()
    {
        gameObject.transform.Find("GroggyCanvas").gameObject.SetActive(true);
    }

    public virtual void InactiveGroggyIcon()    // 그로기 상태 아이콘 인액티브
    {
        photonView.RPC("RpcInactiveGroggyIcon", RpcTarget.All);
    }

    [PunRPC]
    public virtual void RpcInactiveGroggyIcon()
    {
        gameObject.transform.Find("GroggyCanvas").gameObject.SetActive(false);
    }

    public virtual void IsTracking(bool trackingState)    // 헌터 스킬에 의한 레이더 활성화
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
        // isTracking이면 스킬 지속 시간(trackingMaxTime)동안 레이더 보임
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
