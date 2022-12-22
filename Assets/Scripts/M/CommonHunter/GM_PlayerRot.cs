using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_PlayerRot : MonoBehaviourPun
{
    float mx, my, rx; 
    public float ry;
    float rotSpeed = 200;
    public Transform camPos;

    private void Awake()
    {
        if (!photonView.IsMine)
            this.enabled = false;
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    [PunRPC]
    void RPCCharActiveFalse()
    {
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (!(transform.gameObject.GetComponent<GM_Hunter>().CurrentState == HunterStates.Pushed)
            && !(transform.gameObject.GetComponent<GM_Hunter>().CurrentState == HunterStates.Death))
        {
            mx = Input.GetAxis("Mouse X");
            my = Input.GetAxis("Mouse Y");

            rx += my * rotSpeed * Time.deltaTime;
            rx = Mathf.Clamp(rx, -50, 50);
            ry += mx * rotSpeed * Time.deltaTime;

            camPos.localEulerAngles = new Vector3(-rx, 0, 0);
            transform.localEulerAngles = new Vector3(0, ry, 0);
        }

        if (GameManager.instance.playerWin[0] || GameManager.instance.playerWin[1])
        {
            photonView.RPC("RPCCharActiveFalse", RpcTarget.All);
        }
    }
}
