using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public InputField inputNickName;
    
    public Button btnConnect;
    public GameObject LoginWindow;
    public GameObject mainMenuFade;
    public void OnClickPlay()
    {
        mainMenuFade.SetActive(true);
        LoginWindow.SetActive(true);
    }
    public void OnClickTutorial()
    {
        Debug.Log("clickTutorial");
    }
    void Start()
    {
        //�г����� ����ɶ� ȣ��Ǵ� �Լ� ���
        inputNickName.onValueChanged.AddListener(OnValueChanged);
        //�г��ӿ��� Enter�� ������ ȣ��Ǵ� �Լ� ���
        inputNickName.onSubmit.AddListener(OnSubmit);
        //�г��ӿ��� focusing�� �Ҿ����� ȣ��Ǵ� �Լ� ���
        inputNickName.onEndEdit.AddListener(OnEndEdit);
        OnClickConnect();
        LoginWindow.SetActive(false);
        
    }

    public void OnValueChanged(string s)
    {
        //���࿡ s�� ���̰� 0���� ũ�ٸ�
        //���� ��ư�� Ȱ��ȭ ����
        //�׷��� �ʴٸ�
        //���ӹ�ư�� ��Ȱ��ȭ ����
        btnConnect.interactable = s.Length > 0;
    }

    public void OnSubmit(string s)
    {
        //���࿡ s�� ���̰� 0���� ũ�ٸ�
        if (s.Length > 0)
        {
            //��������
            OnClickConnect();
        }
        print("OnSubmit : " + s);
    }
    public void OnEndEdit(string s)
    {

    }

    public void OnClickConnect()
    {
        //NameServer ����(AppId, GameVersion, ����)
        PhotonNetwork.ConnectUsingSettings();
    }
    //������ ������ ���� ����, �κ� ���� �� ������ �� �� ���� ����
    public override void OnConnected()
    {
        base.OnConnected();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    //������ ������ ����, �κ� ���� �� ������ ����
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        //�г��� ����

        //PhotonNetwork.NickName = inputNickName.text;
        PhotonNetwork.NickName = "" + Random.Range(0,1000) ;
        //�⺻ �κ� ����
        PhotonNetwork.JoinLobby();
        //Ư�� �κ� ����
        //PhotonNetwork.JoinLobby(new TypedLobby("������ �κ�", LobbyType.Default));

    }

    //�κ� ���� ������ ȣ��
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        // LobbyScene���� �̵�
        PhotonNetwork.LoadLevel("1.LobbyScene");
    }

    void Update()
    {
        
    }
}