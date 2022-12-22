using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_ShotgunBullet : GM_Bullet
{
    Transform firePos;                      // �ѱ� ��ġ
    Transform aimLookAt;                    // �Ѿ� �߻� ��ġ
    public GameObject bulletImpactPref;     // �Ѿ� ���� �� �ð� ȿ��
    public GameObject[] bulletHoles;        // �Ѿ� ���� �� �Ѿ� �ڱ���
    public GameObject muzzleFlashPref;      // �Ѿ� �߻� �� �߻��ϴ� �ѱ� �Ҳ�
   
    float bulletSpeed = 50;
    Vector3 dir;
    Ray ray;
    RaycastHit hit;
    float currentTime = 0;

    void Start()
    {
        firePos = GameObject.Find("ShotgunFirePos").transform;
        aimLookAt = GameObject.Find("AssultAimLookAt").transform;
        dir = (aimLookAt.position - firePos.position) + new Vector3(Random.Range(-3f, 3f), Random.Range(-1.5f, 3f), 0); // �Ѿ� ���� �����ϰ� ����
        ray = new Ray(firePos.position, dir);
        // �ѱ� �Ҳ� ���� �� ���� �ð� �ڿ� ����
        GameObject muzzleFlash = Instantiate(muzzleFlashPref, firePos.position, Quaternion.LookRotation(-ray.direction));
        Destroy(muzzleFlash, 2);

        damage = 20;
    }

    void Update()
    {
        transform.position += ray.direction * bulletSpeed * Time.deltaTime;
        transform.forward = ray.direction;

        currentTime += Time.deltaTime;
        if(currentTime > 1 && photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // �Ѿ� ���Ϳ��� ���� �� ������ �ο�
        var hitBox = other.GetComponent<HitBox>();
        if (hitBox)
        {
            Debug.Log("hit");
            hitBox.OnRaycastHit(this, transform.forward);
        }

        GameObject bulletImpact = Instantiate(bulletImpactPref);
        GameObject bulletHole = Instantiate(bulletHoles[Random.Range(0, 4)]);

        // Trigger �Ǵ� ���� �Ѿ��� ��ġ���� transform.foward ��ŭ�� �Ÿ� �� ��ġ���� Ray ���� Raycast�� ��ġ�� bulletImpact ����
        if (Physics.Raycast(transform.position - transform.forward, ray.direction, out hit))
        {
            bulletImpact.transform.position = hit.point;
            bulletImpact.transform.forward = hit.normal;

            bulletHole.transform.position = hit.point;
            bulletHole.transform.forward = -hit.normal;
        }
        if(photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        Destroy(bulletHole, 5);

    }
}
