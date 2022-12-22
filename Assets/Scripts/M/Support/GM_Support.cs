using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GM_Support : GM_Hunter
{
    public GameObject camPos;
    public GameObject compass;
    private void Awake()
    {
        if (!photonView.IsMine)
            return;

        // Support�� ���� �� �ִ� ���� ������ŭ �޸� �Ҵ�, �� ���¿� Ŭ���� �޸� �Ҵ�. states[(int)HunterStates.Idle].Execute()�� ���� ������� ���.
        states = new GM_State<GM_Hunter>[9];
        states[(int)HunterStates.Idle] = new GM_SupportOwnedStates.Idle();
        states[(int)HunterStates.Move] = new GM_SupportOwnedStates.Move();
        states[(int)HunterStates.Jump] = new GM_SupportOwnedStates.Jump();
        states[(int)HunterStates.Falling] = new GM_SupportOwnedStates.Falling();
        states[(int)HunterStates.Pushed] = new GM_SupportOwnedStates.Pushed();
        states[(int)HunterStates.Heal] = new GM_SupportOwnedStates.Heal();
        states[(int)HunterStates.Groggy] = new GM_SupportOwnedStates.Groggy();
        states[(int)HunterStates.Death] = new GM_SupportOwnedStates.Death();
        states[(int)HunterStates.Global] = new GM_SupportOwnedStates.Global();

        // upperBodyStates�� ���� states�� ���� ����.
        upperBodyStates = new GM_State<GM_Hunter>[7];
        upperBodyStates[(int)HunterUpperBodyStates.None] = new GM_SupportOwnedStates.None();
        upperBodyStates[(int)HunterUpperBodyStates.Move_Upper] = new GM_SupportOwnedStates.Move_Upper();
        upperBodyStates[(int)HunterUpperBodyStates.Attack] = new GM_SupportOwnedStates.Attack();
        upperBodyStates[(int)HunterUpperBodyStates.Replace] = new GM_SupportOwnedStates.Replace();
        upperBodyStates[(int)HunterUpperBodyStates.Reload] = new GM_SupportOwnedStates.Reload();
        upperBodyStates[(int)HunterUpperBodyStates.GroggyAttack] = new GM_SupportOwnedStates.GroggyAttack();
        upperBodyStates[(int)HunterUpperBodyStates.Global] = new GM_SupportOwnedStates.Global_Upper();

        // ���¸� �����ϴ� StateMachine�� �޸� �Ҵ� �� ù ���� ����
        stateMachine = new GM_StateMachine<GM_Hunter>();
        stateMachine.SetUp(this, states[(int)HunterStates.Idle]);
        stateMachine.SetGlobalState(states[(int)HunterStates.Global]);

        upperBodyStateMachine = new GM_StateMachine<GM_Hunter>();
        upperBodyStateMachine.SetUp(this, upperBodyStates[(int)HunterUpperBodyStates.None]);
        upperBodyStateMachine.SetGlobalState(upperBodyStates[(int)HunterUpperBodyStates.Global]);

        // �� �ڿ��� �ʱ�ȭ
        Hp = 100;
        GroggyHp = 100;
        Barrier = 100;
        Fuel = 100;

        // Slider Value�� �ʱ�ȭ
        HpSlider.maxValue = 100;
        HpSlider.value = 100;
        BarrierSlider.maxValue = 100;
        BarrierSlider.value = 100;
        FuelSlider.maxValue = 100;
        FuelSlider.value = 100;
        GroggyHpSlider.maxValue = 100;
        GroggyHpSlider.value = 100;
        GroggyHpSlider.enabled = false;

        // �Ѿ� �ִ� �� ���� ���� �ʱ�ȭ
        weapon0_MaxAmo = 40;
        weapon1_MaxAmo = 1;
        weapon0_currentAmo = weapon0_MaxAmo;
        weapon1_currentAmo = weapon1_MaxAmo;

        // Slider Valuer�� �ʱ�ȭ
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

    public override void SupportBarrier()
    {
        photonView.RPC("RpcSupportBarrier", RpcTarget.All);
    }

    [PunRPC]
    public override void RpcSupportBarrier()
    {
        if (photonView.IsMine)
            PhotonNetwork.Instantiate("BarrierEffect", transform.position + transform.up * 4.5f, transform.rotation);

        GameObject[] hunters = GameObject.FindGameObjectsWithTag("Player"); // Player �±� �ް� �ִ� ���ӿ�����Ʈ ���� ����Ʈ�� ����.
        for (int i = 0; i < hunters.Length; i++)
        {
            if (Vector3.Distance(hunters[i].transform.position, gameObject.transform.position) < 10) // �� ���� ������Ʈ��� ����Ʈ���� �Ÿ� 10 �̸��̸�
            {
                if (hunters[i].GetComponent<GM_Hunter>().photonView.IsMine) // �ش� ������Ʈ�� Mine�̸�(Mine PC���� �ٸ� PC�� OnSeralizePhontonView�� �����ϱ� ����)
                    hunters[i].GetComponent<GM_Hunter>().Barrier += 40;

                hunters[i].transform.Find("BarrierBuff").gameObject.SetActive(true);
            }
        }
    }

    public override void WeaponSetTrigger(string s)
    {
        photonView.RPC("RpcWeaponSetTrigger", RpcTarget.All, s);
    }

    [PunRPC]
    public override void RpcWeaponSetTrigger(string s)
    {
        weaponAnim.SetTrigger(s);
    }

    public override void WeaponResetTrigger(string s)  // ����Ʈ ���� �ִϸ����� ���� Ʈ����
    {
        photonView.RPC("RpcWeaponResetTrigger", RpcTarget.All, s);
    }

    [PunRPC]
    public override void RpcWeaponResetTrigger(string s)
    {
        weaponAnim.ResetTrigger(s);
    }
}
