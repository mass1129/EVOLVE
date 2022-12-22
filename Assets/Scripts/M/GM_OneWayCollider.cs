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

        dir = player.dir;  // �÷��̾� �̵� ���� ����
        dir2 = arenaPivot.transform.position - player.gameObject.transform.position;  // �÷��̾�� �Ʒ��� �߽ɱ����� ����

        if(player.CurrentState == HunterStates.Jump || player.CurrentState == HunterStates.Falling) // ���߿� ���� ��
        {
            if (Vector3.Dot(dir2, dir) >= 0)                                    // �� ���� ���� ������ 0���� ũ�ų� ������ �浹 X (0 ~ 90��)
            {
                Physics.IgnoreCollision(arenaCollider, playerCollider, true);
            }
            else if (Vector3.Dot(dir2, dir) < 0)                                // �� ���� ���� ������ 0���� ������ �浹 O (90 ~ 180��)
            {
                Physics.IgnoreCollision(arenaCollider, playerCollider, false);
            }
        }
        else
        {
            if (Vector3.Dot(new Vector3(dir2.x, 0, dir2.z), new Vector3(dir.x, 0, dir.z)) >= 0)         // ���� ���� ���� yVelocity�� ������ �����̹Ƿ� �̵� ���� ���Ϳ� ���� ��ħ.
            {                                                                                           // ���� �÷��̾� �̵� ���� ������ dir���� y ���� ������ ���Ϳ��� ���� ����.
                Physics.IgnoreCollision(arenaCollider, playerCollider, true);
            }
            else if (Vector3.Dot(new Vector3(dir2.x, 0, dir2.z), new Vector3(dir.x, 0, dir.z)) < 0)
            {
                Physics.IgnoreCollision(arenaCollider, playerCollider, false);
            }
        }
    } 
}
