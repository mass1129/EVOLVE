using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GM_RifleBullet : GM_Bullet
{
    Transform firePos;                      // �ѱ� ��ġ
    Transform aimLookAt;                    // �Ѿ� �߻� ��ġ
    public GameObject bulletImpactPref;     // �Ѿ� ���� �� �ð� ȿ��
    public GameObject[] bulletHoles;        // �Ѿ� ���� �� �Ѿ� ���� ȿ��
    public GameObject muzzleFlashPref;      // �Ѿ� �߻� �� �߻��ϴ� �ѱ� �Ҳ�xa

    float bulletSpeed = 40;
    Vector3 dir;
    Ray ray;
    RaycastHit hit;
    float currentTime = 0;

    void Start()
    {
        firePos = GameObject.Find("RifleFirePos").transform;
        aimLookAt = GameObject.Find("MedicAimLookAt").transform;
        dir = aimLookAt.position - firePos.position;
        ray = new Ray(firePos.position, dir);
        // �ѱ� �Ҳ� ���� �� 1�� �ڿ� ����
        GameObject muzzleFlash = Instantiate(muzzleFlashPref, firePos.position, Quaternion.LookRotation(-ray.direction));
        Destroy(muzzleFlash, 2);

        damage = 10;
    }

    void Update()
    {
        transform.position += ray.direction * bulletSpeed * Time.deltaTime;
        transform.forward = ray.direction;

        currentTime += Time.deltaTime;
        if (currentTime > 2 && photonView.IsMine)
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
