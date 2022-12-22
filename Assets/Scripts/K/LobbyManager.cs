using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    //���̸� inputField
    public InputField inputRoomName;
    //��й�ȣ inputField
    public InputField inputPassword;
    //������ button
    public Button btnJoin;
    //����� button
    public Button btnCreate;

    //���� ������
    Dictionary<string, RoomInfo> roomCache = new Dictionary<string, RoomInfo>();
    public Transform trListContent;

    public int toScene;
    public GameObject loadingScreen;
    public Slider slider;
    //public GameObject[] mapThumbs;

    void Start()
    {
        //���̸��� ����ɶ� ȣ��Ǵ� �Լ� ���
        inputRoomName.onValueChanged.AddListener(OnRoomNameValueChanged);
       

    }
    public void OnRoomNameValueChanged(string s)
    {
        //����
        btnJoin.interactable = s.Length > 0;
        //����
        btnCreate.interactable = s.Length > 0;
    }
    public void OnMaxPlayerValueChanged(string s)
    {
        //����
        btnCreate.interactable = s.Length > 0 && inputRoomName.text.Length > 0;
    }


    void Update()
    {

    }

    //�����
    public void CreateRoom()
    {
        //��ɼ� ���� 
        RoomOptions roomOptions = new RoomOptions();

        //�ִ��ο� (0�̸� �ִ��ο� = 255)byte������ 255�����...
        roomOptions.MaxPlayers = 5;
        //�� ��Ͽ� ���̴� ���� (�⺻�� true)
        roomOptions.IsVisible = true;
        //custom ���� ����
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash["desc"] = "";
        hash["room_name"] = inputRoomName.text;
        hash["password"] = inputPassword.text;
        roomOptions.CustomRoomProperties = hash;

        //custom ������ �����ϴ� ����
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "room_name", "desc"};
        //���� �����.(�� �̸��� ����� �Լ����� ȣ��)
        PhotonNetwork.CreateRoom(inputRoomName.text + inputPassword.text, roomOptions);



    }

    //�� ���� �Ϸ�
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("OnCreatedRoom");
    }

    //�� ���� ���н� �˷��ִ� �Լ�
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("OnCreateRoomFailed" + "," + returnCode + "," + message);


    }


    //������
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(inputRoomName.text + inputPassword.text);


    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("OnJoinedRoom");
        PhotonNetwork.LoadLevel("2.SelectSceneReal1");
    }

    //�� ���� ���н� ȣ��Ǵ� �Լ�
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("OnJoinRoomFailed, " + returnCode + ", " + message);
    }

    //�濡 ���� ������ ����Ǹ� ȣ�� �Ǵ� �Լ�(�߰�/����/����)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        //�븮��Ʈ UI�� ��ü����
        DeleteRoomListUI();
        //�븮��Ʈ ������ ������Ʈ
        UpdateRoomCache(roomList);
        //�븮��Ʈ UI ��ü ����
        CreateRoomListUI();
    }

    void DeleteRoomListUI()
    {
        foreach (Transform tr in trListContent)
        {
            Destroy(tr.gameObject);
        }
    }
    void UpdateRoomCache(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            //����, ����
            if (roomCache.ContainsKey(roomList[i].Name))
            {
                //���࿡ �ش� ���� �����Ȱ��̶��
                if (roomList[i].RemovedFromList)
                {
                    //roomCache���� �ش� ������ ����
                    roomCache.Remove(roomList[i].Name);
                }
                //�׷��� �ʴٸ�
                else
                {
                    //���� ����
                    roomCache[roomList[i].Name] = roomList[i];
                }

            }
            else
            {
                roomCache[roomList[i].Name] = roomList[i];
            }
        }
    }

    public GameObject roomItemFactory;
    void CreateRoomListUI()
    {
        foreach (RoomInfo info in roomCache.Values)
        {
            //������� �����.
            GameObject go = Instantiate(roomItemFactory, trListContent);
            //������������� ����(������(0/0))
            RoomItem item = go.GetComponent<RoomItem>();
            item.SetInfo(info);

            //roomItem ��ư�� Ŭ���Ǹ� ȣ��Ǵ� �Լ� ���
            item.onClickAtion = SetRoomName;
            
            string desc = (string)info.CustomProperties["desc"];
            
            print(desc);
        }
    }

    //���� Thumbnail id
    
    void SetRoomName(string room)
    {
        //���̸� ����
        inputRoomName.text = room;
    }

   
    
}
