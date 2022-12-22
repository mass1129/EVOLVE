using UnityEngine;

public class GM_Compass : MonoBehaviour
{
    Vector3 dir;
    Vector3 monsterDir;
    Vector3 radarDir;
    GameObject monster;
    public GameObject player;
    public GameObject radarPivot;
    public GameObject body;

    float circleR = 80;          // ������
    float deg;                   // ����
    public GameObject[] texts;   // �������� �ؽ�Ʈ �迭


    void Start()
    {
       
    }


    void Update()
    {
        monster = GameObject.Find("Ceratoferox(Clone)");
        dir.z = player.transform.eulerAngles.y;
        transform.localEulerAngles = dir;

        if (dir.z < 0)
            deg = 360 + dir.z;
        else
            deg = dir.z;


        // �ؽ�Ʈ�� Circle �߽����� �� �
        for (int i = 0; i < 4; i++)
        {
            var rad = Mathf.Deg2Rad * (deg + 90 * i);
            var x = circleR * Mathf.Cos(rad);
            var y = circleR * Mathf.Sin(rad);

            texts[i].transform.position = transform.position + new Vector3(x, y);
        }

        // radar ������

        monsterDir = monster.transform.position - player.transform.position; // ���� ���� ����
        body.transform.forward = monsterDir;
        radarDir.z = body.transform.localEulerAngles.y;
        
        radarPivot.transform.localEulerAngles = -radarDir;
    }
}
