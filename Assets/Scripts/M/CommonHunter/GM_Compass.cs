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

    float circleR = 80;          // 반지름
    float deg;                   // 각도
    public GameObject[] texts;   // 동서남북 텍스트 배열


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


        // 텍스트들 Circle 중심으로 원 운동
        for (int i = 0; i < 4; i++)
        {
            var rad = Mathf.Deg2Rad * (deg + 90 * i);
            var x = circleR * Mathf.Cos(rad);
            var y = circleR * Mathf.Sin(rad);

            texts[i].transform.position = transform.position + new Vector3(x, y);
        }

        // radar 움직임

        monsterDir = monster.transform.position - player.transform.position; // 몬스터 방향 벡터
        body.transform.forward = monsterDir;
        radarDir.z = body.transform.localEulerAngles.y;
        
        radarPivot.transform.localEulerAngles = -radarDir;
    }
}
