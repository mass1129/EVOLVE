using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
public class RoomItem : MonoBehaviour
{
    //내용(방이름 (0/0))
    public Text roomInfo;

    //설명
    public Text roomDesc;

    public System.Action<string> onClickAtion;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetInfo(string roomName, int currPlayer, byte maxPlayer)
    {
        //게임오브젝트의 이름을 roomName으로!
        name = roomName;

        roomInfo.text = roomName + " ( " + currPlayer + " / " + maxPlayer + " )";

    }
    public void SetInfo(RoomInfo info)
    {
        SetInfo((string)info.CustomProperties["room_name"], info.PlayerCount, info.MaxPlayers);

        //desc 설정
        roomDesc.text = (string)info.CustomProperties["desc"];

    
    }
    public void OnClick()
    {

        //만약에 onClickAction가 null이 아니라면
        if (onClickAtion != null)
        {
            //onClickAtion실행
            onClickAtion(name);
        }

        //1. InputRoomName 게임오브젝 찾자
        //GameObject go =  GameObject.Find("InputRoomName");
        //2. InputField 컴포넌트 가져오자
        //InputField inputField = go.GetComponent<InputField>();
        //3. text에 roomName넣자
        //inputField.text = name;
    }
}
