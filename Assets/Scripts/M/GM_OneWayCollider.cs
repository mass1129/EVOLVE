using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM_OneWayCollider : MonoBehaviour
{
    public GameObject arenaPivot;
    public BoxCollider arenaCollider;
    public Collider playerCollider;
    GameObject[] hunters = new GameObject[4];
    GM_Hunter player;
    CharacterAiming monsterScript;
    Vector3 dir;
    Vector3 dir2;

    // Start is called before the first frame update
    void Start()
    {
        monsterScript = GameObject.Find("Ceratoferox(Clone)").GetComponent<CharacterAiming>();
        if (monsterScript.photonView.IsMine)
            return;

        arenaCollider = GetComponent<BoxCollider>();
        hunters = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i < hunters.Length; i++)
        {
            if (hunters[i].GetComponent<GM_Hunter>().photonView.IsMine)
                player = hunters[i].GetComponent<GM_Hunter>();
        }
        playerCollider = player.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (monsterScript.photonView.IsMine)
            return;

        dir = player.dir;  // 플레이어 이동 방향 벡터
        dir2 = arenaPivot.transform.position - player.gameObject.transform.position;  // 플레이어에서 아레나 중심까지의 벡터

        if(player.CurrentState == HunterStates.Jump || player.CurrentState == HunterStates.Falling) // 공중에 있을 때
        {
            if (Vector3.Dot(dir2, dir) >= 0)                                    // 두 벡터 간의 내적이 0보다 크거나 같으면 충돌 X (0 ~ 90도)
            {
                Physics.IgnoreCollision(arenaCollider, playerCollider, true);
            }
            else if (Vector3.Dot(dir2, dir) < 0)                                // 두 벡터 간의 내적이 0보다 작으면 충돌 O (90 ~ 180도)
            {
                Physics.IgnoreCollision(arenaCollider, playerCollider, false);
            }
        }
        else
        {
            if (Vector3.Dot(new Vector3(dir2.x, 0, dir2.z), new Vector3(dir.x, 0, dir.z)) >= 0)         // 땅에 있을 때는 yVelocity가 음수인 상태이므로 이동 방향 벡터에 영향 미침.
            {                                                                                           // 따라서 플레이어 이동 방향 벡터인 dir에서 y 값을 제외한 벡터와의 내적 구함.
                Physics.IgnoreCollision(arenaCollider, playerCollider, true);
            }
            else if (Vector3.Dot(new Vector3(dir2.x, 0, dir2.z), new Vector3(dir.x, 0, dir.z)) < 0)
            {
                Physics.IgnoreCollision(arenaCollider, playerCollider, false);
            }
        }
    } 
}
