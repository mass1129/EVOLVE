using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
public class RoomItem : MonoBehaviour
{
    //����(���̸� (0/0))
    public Text roomInfo;

    //����
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
        //���ӿ�����Ʈ�� �̸��� roomName����!
        name = roomName;

        roomInfo.text = roomName + " ( " + currPlayer + " / " + maxPlayer + " )";

    }
    public void SetInfo(RoomInfo info)
    {
        SetInfo((string)info.CustomProperties["room_name"], info.PlayerCount, info.MaxPlayers);

        //desc ����
        roomDesc.text = (string)info.CustomProperties["desc"];

    
    }
    public void OnClick()
    {

        //���࿡ onClickAction�� null�� �ƴ϶��
        if (onClickAtion != null)
        {
            //onClickAtion����
            onClickAtion(name);
        }

        //1. InputRoomName ���ӿ����� ã��
        //GameObject go =  GameObject.Find("InputRoomName");
        //2. InputField ������Ʈ ��������
        //InputField inputField = go.GetComponent<InputField>();
        //3. text�� roomName����
        //inputField.text = name;
    }
}
