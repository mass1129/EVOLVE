using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager1 : MonoBehaviourPunCallbacks
{
    public static GameManager1 instance;
    //SpawnPos 풀
    public Vector3[] spawnPos;

    public GameObject SelectUI;
    
    public GameObject[] playerPrefabsPool = new GameObject[5];
    
    public List<PhotonView> players = new List<PhotonView>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SelectUI.SetActive(true);

        //OnPhotonSerializeView 호출빈도
        PhotonNetwork.SerializationRate = 60;
        //Rpc호출빈도
       PhotonNetwork.SendRate = 60;


        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnRoleButtonClick(int num)
    {
        Debug.Log("click");
        SelectUI.SetActive(false);

        PhotonNetwork.Instantiate(playerPrefabsPool[num].gameObject.name, Vector3.zero, Quaternion.identity);
    }    
    

    public void AddPlayer(PhotonView pv)
    {
        players.Add(pv);
        //만약에 인원이 다 들어왔으면
        


    }
    [PunRPC]
    void IMGSetActive()
    {

    }

   
    //방에 플레이어가 참여 했을 때 호출해주는 함수
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        print(newPlayer.NickName + "이 방에 들어왔습니다");
    }

}
