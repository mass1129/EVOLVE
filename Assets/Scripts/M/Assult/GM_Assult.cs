using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GM_Assult : GM_Hunter
{
    public GameObject camPos;
    public GameObject compass;

    private void Awake()
    {
        if (!photonView.IsMine)
            return;

        // Assult가 가질 수 있는 상태 개수만큼 메모리 할당, 각 상태에 클래스 메모리 할당. states[(int)HunterStates.Idle].Execute()와 같은 방식으로 사용.
        states = new GM_State<GM_Hunter>[9];
        states[(int)HunterStates.Idle] = new GM_AssultOwnedStates.Idle();
        states[(int)HunterStates.Move] = new GM_AssultOwnedStates.Move();
        states[(int)HunterStates.Jump] = new GM_AssultOwnedStates.Jump();
        states[(int)HunterStates.Falling] = new GM_AssultOwnedStates.Falling();
        states[(int)HunterStates.Pushed] = new GM_AssultOwnedStates.Pushed();
        states[(int)HunterStates.Heal] = new GM_AssultOwnedStates.Heal();
        states[(int)HunterStates.Groggy] = new GM_AssultOwnedStates.Groggy();
        states[(int)HunterStates.Death] = new GM_AssultOwnedStates.Death();
        states[(int)HunterStates.Global] = new GM_AssultOwnedStates.Global();

        // upperBodyStates는 기존 states와 따로 관리.
        upperBodyStates = new GM_State<GM_Hunter>[7];
        upperBodyStates[(int)HunterUpperBodyStates.None] = new GM_AssultOwnedStates.None();
        upperBodyStates[(int)HunterUpperBodyStates.Move_Upper] = new GM_AssultOwnedStates.Move_Upper();
        upperBodyStates[(int)HunterUpperBodyStates.Attack] = new GM_AssultOwnedStates.Attack();
        upperBodyStates[(int)HunterUpperBodyStates.Replace] = new GM_AssultOwnedStates.Replace();
        upperBodyStates[(int)HunterUpperBodyStates.Reload] = new GM_AssultOwnedStates.Reload();
        upperBodyStates[(int)HunterUpperBodyStates.GroggyAttack] = new GM_AssultOwnedStates.GroggyAttack();
        upperBodyStates[(int)HunterUpperBodyStates.Global] = new GM_AssultOwnedStates.Global_Upper();

        // 상태를 관리하는 StateMachine에 메모리 할당 및 첫 상태 결정
        stateMachine = new GM_StateMachine<GM_Hunter>();
        stateMachine.SetUp(this, states[(int)HunterStates.Idle]);
        stateMachine.SetGlobalState(states[(int)HunterStates.Global]);

        upperBodyStateMachine = new GM_StateMachine<GM_Hunter>();
        upperBodyStateMachine.SetUp(this, upperBodyStates[(int)HunterUpperBodyStates.None]);
        upperBodyStateMachine.SetGlobalState(upperBodyStates[(int)HunterUpperBodyStates.Global]);

        // 각 자원들 초기화
        Hp = 100;
        GroggyHp = 100;
        Barrier = 100;
        Fuel = 100;

        // Slider Value도 초기화
        HpSlider.maxValue = 100;
        HpSlider.value = 100;
        BarrierSlider.maxValue = 100;
        BarrierSlider.value = 100;
        FuelSlider.maxValue = 100;
        FuelSlider.value = 100;
        GroggyHpSlider.maxValue = 100;
        GroggyHpSlider.value = 100;
        GroggyHpSlider.enabled = false;

        // 총알 최대 및 현재 개수 초기화
        weapon0_MaxAmo = 10;
        weapon1_MaxAmo = 10;
        weapon0_currentAmo = weapon0_MaxAmo;
        weapon1_currentAmo = weapon1_MaxAmo;

        // Slider Valuer도 초기화
        Weapon0Slider.maxValue = weapon0_MaxAmo;
        Weapon1Slider.maxValue = weapon1_MaxAmo;
        Weapon0Slider.value = weapon0_MaxAmo;
        Weapon1Slider.value = weapon1_MaxAmo;

        respawnTimeText = GameObject.Find("ReviveTimeText").GetComponent<Text>();
        respawnTimeText.text = "00 : 40";
    }
    private void Start()
    {
        if (photonView.IsMine)
        {
            camPos.SetActive(true);
            compass.SetActive(true);
        }    
    }

    public override void SetTrigger(string s)
    {
        photonView.RPC("RpcSetTrigger", RpcTarget.AllBuffered, s);
    }

    [PunRPC]
    public override void RpcSetTrigger(string s)
    {
        anim.SetTrigger(s);
    }

    public override void ResetTrigger(string s)
    {
        photonView.RPC("RpcResetTrigger", RpcTarget.AllBuffered, s);
    }

    [PunRPC]
    public override void RpcResetTrigger(string s)
    {
        anim.ResetTrigger(s);
    }

    public override void SetFloat(string s, float f)
    {
        photonView.RPC("RpcSetFloat", RpcTarget.AllBuffered, s, f);
    }

    [PunRPC]
    public override void RpcSetFloat(string s, float f)
    {
        anim.SetFloat(s, f);
    }

    public override void Play(string s, int layer, float normallizedTime)
    {
        photonView.RPC("RpcPlay", RpcTarget.AllBuffered, s, layer, normallizedTime);
    }

    [PunRPC]
    public override void RpcPlay(string s, int layer, float normalizedTime)
    {
        anim.Play(s, layer, normalizedTime);
    }

    public override void AssultBarrier()
    {
        photonView.RPC("RpcAssultBarrier", RpcTarget.All);
    }

    [PunRPC]
    public override void RpcAssultBarrier()
    {
        gameObject.transform.Find("AssultBarrier").gameObject.SetActive(true);
    }
}
