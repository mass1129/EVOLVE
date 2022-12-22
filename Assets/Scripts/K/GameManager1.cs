using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager1 : MonoBehaviourPunCallbacks
{
    public static GameManager1 instance;
    //SpawnPos Ǯ
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

        //OnPhotonSerializeView ȣ���
        PhotonNetwork.SerializationRate = 60;
        //Rpcȣ���
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
        //���࿡ �ο��� �� ��������
        


    }
    [PunRPC]
    void IMGSetActive()
    {

    }

   
    //�濡 �÷��̾ ���� ���� �� ȣ�����ִ� �Լ�
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        print(newPlayer.NickName + "�� �濡 ���Խ��ϴ�");
    }

}
