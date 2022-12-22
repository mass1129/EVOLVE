using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    //방이름 inputField
    public InputField inputRoomName;
    //비밀번호 inputField
    public InputField inputPassword;
    //방참가 button
    public Button btnJoin;
    //방생성 button
    public Button btnCreate;

    //방의 정보들
    Dictionary<string, RoomInfo> roomCache = new Dictionary<string, RoomInfo>();
    public Transform trListContent;

    public int toScene;
    public GameObject loadingScreen;
    public Slider slider;
    //public GameObject[] mapThumbs;

    void Start()
    {
        //방이름이 변경될때 호출되는 함수 등록
        inputRoomName.onValueChanged.AddListener(OnRoomNameValueChanged);
       

    }
    public void OnRoomNameValueChanged(string s)
    {
        //참가
        btnJoin.interactable = s.Length > 0;
        //생성
        btnCreate.interactable = s.Length > 0;
    }
    public void OnMaxPlayerValueChanged(string s)
    {
        //생성
        btnCreate.interactable = s.Length > 0 && inputRoomName.text.Length > 0;
    }


    void Update()
    {

    }

    //방생성
    public void CreateRoom()
    {
        //방옵션 셋팅 
        RoomOptions roomOptions = new RoomOptions();

        //최대인원 (0이면 최대인원 = 255)byte변수는 255명까지...
        roomOptions.MaxPlayers = 5;
        //룸 목록에 보이는 여부 (기본은 true)
        roomOptions.IsVisible = true;
        //custom 정보 셋팅
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash["desc"] = "";
        hash["room_name"] = inputRoomName.text;
        hash["password"] = inputPassword.text;
        roomOptions.CustomRoomProperties = hash;

        //custom 정보를 공개하는 설정
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "room_name", "desc"};
        //방을 만든다.(방 이름은 만든다 함수에서 호출)
        PhotonNetwork.CreateRoom(inputRoomName.text + inputPassword.text, roomOptions);



    }

    //방 생성 완료
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("OnCreatedRoom");
    }

    //방 생성 실패시 알려주는 함수
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        print("OnCreateRoomFailed" + "," + returnCode + "," + message);


    }


    //방입장
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

    //방 입장 실패시 호출되는 함수
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print("OnJoinRoomFailed, " + returnCode + ", " + message);
    }

    //방에 대한 정보가 변경되면 호출 되는 함수(추가/삭제/수정)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        //룸리스트 UI를 전체삭제
        DeleteRoomListUI();
        //룸리스트 정보를 업데이트
        UpdateRoomCache(roomList);
        //룸리스트 UI 전체 생성
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
            //수정, 삭제
            if (roomCache.ContainsKey(roomList[i].Name))
            {
                //만약에 해당 룸이 삭제된것이라면
                if (roomList[i].RemovedFromList)
                {
                    //roomCache에서 해당 정보를 삭제
                    roomCache.Remove(roomList[i].Name);
                }
                //그렇지 않다면
                else
                {
                    //정보 수정
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
            //룸아이템 만든다.
            GameObject go = Instantiate(roomItemFactory, trListContent);
            //룸아이템정보를 셋팅(방제목(0/0))
            RoomItem item = go.GetComponent<RoomItem>();
            item.SetInfo(info);

            //roomItem 버튼이 클릭되면 호출되는 함수 등록
            item.onClickAtion = SetRoomName;
            
            string desc = (string)info.CustomProperties["desc"];
            
            print(desc);
        }
    }

    //이전 Thumbnail id
    
    void SetRoomName(string room)
    {
        //룸이름 복사
        inputRoomName.text = room;
    }

   
    
}
