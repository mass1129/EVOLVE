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
        //닉네임이 변경될때 호출되는 함수 등록
        inputNickName.onValueChanged.AddListener(OnValueChanged);
        //닉네임에서 Enter를 쳤을때 호출되는 함수 등록
        inputNickName.onSubmit.AddListener(OnSubmit);
        //닉네임에서 focusing을 잃었을때 호출되는 함수 등록
        inputNickName.onEndEdit.AddListener(OnEndEdit);
        OnClickConnect();
        LoginWindow.SetActive(false);
        
    }

    public void OnValueChanged(string s)
    {
        //만약에 s의 길이가 0보다 크다면
        //접속 버튼을 활성화 하자
        //그렇지 않다면
        //접속버튼을 비활성화 하자
        btnConnect.interactable = s.Length > 0;
    }

    public void OnSubmit(string s)
    {
        //만약에 s의 길이가 0보다 크다면
        if (s.Length > 0)
        {
            //접속하자
            OnClickConnect();
        }
        print("OnSubmit : " + s);
    }
    public void OnEndEdit(string s)
    {

    }

    public void OnClickConnect()
    {
        //NameServer 접속(AppId, GameVersion, 지역)
        PhotonNetwork.ConnectUsingSettings();
    }
    //마스터 서버에 접속 성공, 로비 생성 및 진입을 할 수 없는 상태
    public override void OnConnected()
    {
        base.OnConnected();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    //마스터 서버에 접속, 로비 생성 및 진입이 가능
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        //닉네임 설정

        //PhotonNetwork.NickName = inputNickName.text;
        PhotonNetwork.NickName = "" + Random.Range(0,1000) ;
        //기본 로비 진입
        PhotonNetwork.JoinLobby();
        //특정 로비 진입
        //PhotonNetwork.JoinLobby(new TypedLobby("김현진 로비", LobbyType.Default));

    }

    //로비 접속 성공시 호출
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        // LobbyScene으로 이동
        PhotonNetwork.LoadLevel("1.LobbyScene");
    }

    void Update()
    {
        
    }
}