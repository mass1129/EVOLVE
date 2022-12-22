using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GM_CreateArena : MonoBehaviourPun
{
    public GameObject monster;
    public Slider arenaTimeSlider;
    public Text arenaText;
    public GameObject cathcMonsterUI;

    GameObject[] hunters;
    public GameObject myHunter;

    [SerializeField]
    float arenaCoolTime = 100;
    float arenaMaxTime = 100;
    

    void Start()
    {
        
    }

    void Update()
    {
        monster = GameObject.Find("Ceratoferox(Clone)");
        if (myHunter == null)
        {
            hunters = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < hunters.Length; i++)
            {
                if (hunters[i].GetComponent<GM_Hunter>().photonView.IsMine)
                {
                    myHunter = hunters[i];
                    break;
                }
            }
        }
        

        arenaCoolTime += Time.deltaTime;
        if (Vector3.Distance(monster.transform.position, myHunter.transform.position) < 20 && arenaCoolTime >= arenaMaxTime)
        {
            cathcMonsterUI.SetActive(true);
            if (Input.GetKeyDown(KeyCode.F))
            {
                CreateArena();
            }
        }
        else
            cathcMonsterUI.SetActive(false);

        arenaTimeSlider.maxValue = arenaMaxTime;
        arenaTimeSlider.value = arenaCoolTime;

        if(arenaCoolTime < arenaMaxTime)
        {
            arenaText.text = "돔 충전 중";
        }
        else if(arenaCoolTime >= arenaMaxTime)
        {
            arenaText.text = "돔이 준비되었습니다";
        }
    }

    public void CreateArena()
    {
        photonView.RPC("RpcCreateArena", RpcTarget.All);
    }


    [PunRPC]
    public void RpcCreateArena()
    {
        GameObject arena = PhotonNetwork.Instantiate("Arena", monster.transform.position, monster.transform.rotation);
        arenaCoolTime = 0;
    }
}
