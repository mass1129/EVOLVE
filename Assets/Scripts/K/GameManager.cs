using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    //SpawnPos 풀
    public Transform[] spawnPos;

    public GameObject SelectUI;
    public GameObject choiceUI;
    public GameObject WaitingUI;

    public Button RoleconfirmButton;
    public Button StartconfirmButton;
    public GameObject[] playerPrefabsPool = new GameObject[5];
    public Button[] buttonsPool = new Button[5];
    public Image[] imagesPool = new Image[5];
    public List<PhotonView> players = new List<PhotonView>();
    public GameObject[] winCutScene;
    public bool[] playerWin = new bool[2];
    public GameObject[] disableObj;
    int leftHunter = 4;
    public int LeftHunter
    {
        get
        {
            return leftHunter;
        }
        set
        {
            leftHunter = value;
            if(leftHunter <= 0)
            {
                GameOver(1);


            }
        }
    }
    [PunRPC]
    void RPCManageLeftPlayer(int i)
    {
        leftHunter += i;
    }
    public void GameOver(int i)
    {
        photonView.RPC("RPCGameOver", RpcTarget.All, i);
    }
    [PunRPC]
    void RPCGameOver(int i)
    {
        
        for(int j=0; j<disableObj.Length; j++)
        {
            disableObj[j].SetActive(false);
        }
        
        winCutScene[i].SetActive(true);
        playerWin[i] = true;
    }
    private void Awake()
    {
        instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        SelectUI.SetActive(true);
        RoleconfirmButton.interactable = false;
        StartconfirmButton.interactable = false;
        
        photonView.RPC("RPCStartButtonSetActive", RpcTarget.MasterClient);
        //OnPhotonSerializeView 호출빈도
        PhotonNetwork.SerializationRate = 60;
        //Rpc호출빈도
       PhotonNetwork.SendRate = 60;


        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            GameOver(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            GameOver(1);
        }
    }

    #region
    public void OnRoleButtonClick(int num)
    {
        
        
        if (preRole > -1)
            buttonsPool[preRole].interactable = true;
        
        buttonsPool[num].interactable = false;
        preRole = num;

        //photonView.RPC("RPCButtonSetActive", RpcTarget.All);
        RoleconfirmButton.interactable = true;

        
    }    
    public void OnRoleConfirmRole()
    {
        
        photonView.RPC("RPCButtonSetActive", RpcTarget.AllBuffered,preRole);
        choiceUI.SetActive(false);
        WaitingUI.SetActive(true);
        photonView.RPC("RPCIMGSetActive", RpcTarget.AllBuffered, preRole);

    }
    public void OnStartButtonClick()
    {
        photonView.RPC("RPCOnStart", RpcTarget.All);

    }
    public GM_GameTimer gametimer;
    public AudioSource bgSound;
    public float bgVolume = 0.13f;
    [PunRPC]
    void RPCOnStart()
    {
        SelectUI.SetActive(false);
        gametimer.isStarted = true;
        bgSound.volume = bgVolume;
        bgSound.Play();

        PhotonNetwork.Instantiate(playerPrefabsPool[preRole].gameObject.name, spawnPos[preRole].position, Quaternion.identity);
    }
    public void AddPlayer(PhotonView pv)
    {
        players.Add(pv);
        //만약에 인원이 다 들어왔으면
        


    }
    public int preRole = -1;
    [PunRPC]
    void RPCButtonSetActive(int i)
    {
        
        buttonsPool[i].interactable = false;
        
    }
    [PunRPC]
    void RPCStartButtonSetActive()
    {

        StartconfirmButton.interactable = true;

    }
    [PunRPC]
    void RPCIMGSetActive(int i)
    {
        imagesPool[i].gameObject.SetActive(true);
    }

    //방에 플레이어가 참여 했을 때 호출해주는 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        print(newPlayer.NickName + "이 방에 들어왔습니다");
    }
    #endregion





}
