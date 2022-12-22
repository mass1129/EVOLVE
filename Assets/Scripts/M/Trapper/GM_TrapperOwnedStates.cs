using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace GM_TrapperOwnedStates
{
    public class Idle : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Idle");
            entity.h = 0;
            entity.v = 0;
        }

        public override void Execute(GM_Hunter entity)
        {
            entity.yVelocity += entity.gravity * Time.deltaTime;
            entity.yVelocity = Mathf.Clamp(entity.yVelocity, -6f, 100);

            if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
            {
                entity.ChangeState(HunterStates.Move);
            }

            if(Input.GetButtonDown("Jump"))
            {
                entity.ChangeState(HunterStates.Jump);
                entity.yVelocity = entity.jumpPower;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                GameObject[] hunters = GameObject.FindGameObjectsWithTag("Player"); // Player �±� �ް� �ִ� ���ӿ�����Ʈ ���� �迭�� ����.
                for (int i = 0; i < hunters.Length; i++)
                {
                    Debug.Log(hunters[i].GetComponent<GM_Hunter>().CurrentState);
                    if (Vector3.Distance(hunters[i].transform.position, entity.transform.position) < 1.5f
                        && hunters[i].GetComponent<GM_Hunter>().CurrentState == HunterStates.Groggy) // �׷α� ������ ���Ϳ� �Ÿ��� 1.5 �̸��̸�
                    {
                        entity.ChangeState(HunterStates.Heal);
                    }
                }
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Idle");
        }
    }

    public class Move : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Move");
        }

        public override void Execute(GM_Hunter entity)
        {
            entity.yVelocity += entity.gravity * Time.deltaTime;
            entity.yVelocity = Mathf.Clamp(entity.yVelocity, -6f, 100);

            if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
            {
                entity.h = Input.GetAxis("Horizontal");
                entity.v = Input.GetAxis("Vertical");

                if (entity.v > 0.5f)
                    entity.moveSpeed = 5;
                else
                    entity.moveSpeed = 2;

                entity.dir = entity.transform.right * entity.h + entity.transform.forward * entity.v;
                entity.dir.Normalize();
                entity.dir.y = entity.yVelocity;
                entity.GetComponent<CharacterController>().Move(entity.dir * entity.moveSpeed * Time.deltaTime);

                // �̵� ���⿡ ���� Move BlendTree �� ����
                entity.SetFloat("Move_Horizontal", entity.h);
                entity.SetFloat("Move_Vertical", entity.v);

                // �̵� �߿� �������� Falling���� ���� ��ȯ
                if(!entity.GetComponent<CharacterController>().isGrounded)
                {
                    entity.ChangeState(HunterStates.Falling);
                }
            }
            else
            {
                entity.ChangeState(HunterStates.Idle);
            }

            if (Input.GetButtonDown("Jump"))
            {
                entity.ResetTrigger("Move");
                entity.ChangeState(HunterStates.Jump);
                entity.yVelocity = entity.jumpPower;
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Move");
        }
    }

    public class Jump : GM_State<GM_Hunter>
    {
        float zetpackPower = 7;

        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Jump");
        }

        public override void Execute(GM_Hunter entity)
        {
            entity.moveSpeed = 2;

            entity.yVelocity += entity.gravity * Time.deltaTime;
            entity.yVelocity = Mathf.Clamp(entity.yVelocity, -6f, 100);

            if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
            {
                entity.h = Input.GetAxis("Horizontal");
                entity.v = Input.GetAxis("Vertical");
            }

            entity.dir = entity.transform.right * entity.h + entity.transform.forward * entity.v;
            entity.dir.Normalize();
            entity.dir.y = entity.yVelocity;
            entity.GetComponent<CharacterController>().Move(entity.dir * entity.moveSpeed * Time.deltaTime);

            // ��Ʈ�� ���. Jump ���� �Է��ϸ� zetpackPower�� ���� ���
            if (Input.GetButton("Jump"))
            {
                entity.yVelocity += zetpackPower * Time.deltaTime;
            }

            // Falling ���� ��ȭ ����
            if (entity.yVelocity <= 0.1f || entity.Fuel <= 0) // yVelocity�� ������ ��, �Ǵ� Fuel ���� �Ҹ����� ��
            {
                entity.ResetTrigger("Jump");
                entity.ChangeState(HunterStates.Falling);
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.h = 0;
            entity.v = 0;
            entity.ResetTrigger("Jump");
            entity.ResetTrigger("Landing");
        }
    }

    public class Falling : GM_State<GM_Hunter>
    {
        bool isLanding = false;

        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Falling");
        }

        public override void Execute(GM_Hunter entity)
        {
            entity.moveSpeed = 2;

            if(!isLanding)
            {
                entity.yVelocity += entity.gravity * Time.deltaTime;
                entity.yVelocity = Mathf.Clamp(entity.yVelocity, -6f, 100);

                if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
                {
                    entity.h = Input.GetAxis("Horizontal");
                    entity.v = Input.GetAxis("Vertical");
                }

                entity.dir = entity.transform.right * entity.h + entity.transform.forward * entity.v;
                entity.dir.Normalize();
                entity.dir.y = entity.yVelocity;
                entity.GetComponent<CharacterController>().Move(entity.dir * entity.moveSpeed * Time.deltaTime);

                // Falling ���¿��� �ٽ� ���� ������ yVelocity �ʱ�ȭ �� Jump�� ���� ��ȯ (��� ������ Fuel ���� ��) 
                if (Input.GetButtonDown("Jump") && entity.Fuel > 0)
                {
                    entity.yVelocity = 0.2f;
                    entity.ChangeState(HunterStates.Jump);
                }

                // ���� ������ Landing �ִϸ��̼� ���
                if (entity.GetComponent<CharacterController>().isGrounded)
                {
                    entity.SetTrigger("Landing");
                    isLanding = true;
                }
            }

            if (entity.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                entity.SetTrigger("Landing");


            // Landing �ִϸ��̼� ��� �ð� ���� ������ �Է¿� ���� ���� ���·� ���� 
            if (entity.anim.GetCurrentAnimatorStateInfo(0).IsName("Landing") && entity.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
                {
                    entity.ChangeState(HunterStates.Move);
                }
                else if(Input.GetButton("Jump"))
                {
                    entity.ChangeState(HunterStates.Jump);
                    entity.yVelocity = entity.jumpPower;
                }
                else
                    entity.ChangeState(HunterStates.Idle);
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.h = 0;
            entity.v = 0;
            entity.ResetTrigger("Falling");
            entity.ResetTrigger("Landing");
            isLanding = false;
        }
    }

    public class Pushed : GM_State<GM_Hunter>
    {
        float currentTime = 0;
        float maxTime = 0.35f;
        float speed = 3;
        Vector3 dir;

        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Pushed");
        }

        public override void Execute(GM_Hunter entity)
        {
            speed -= Time.deltaTime * 5;
            dir = -entity.transform.forward;

            if (!entity.GetComponent<CharacterController>().isGrounded)
                dir.y -= Time.deltaTime * 10;

            entity.transform.position += dir * Time.deltaTime * speed;

            currentTime += Time.deltaTime;
            if (currentTime > maxTime)
            {
                if (entity.GetComponent<CharacterController>().isGrounded)
                {
                    if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
                    {
                        entity.ChangeState(HunterStates.Move);
                    }
                    else
                    {
                        entity.ChangeState(HunterStates.Idle);
                    }
                }
                else
                {
                    if (Input.GetButton("Jump"))
                    {
                        entity.ChangeState(HunterStates.Jump);
                        entity.yVelocity = entity.jumpPower;
                    }
                    else
                    {
                        entity.ChangeState(HunterStates.Falling);
                    }
                }
            }

        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Pushed");
            currentTime = 0;
            speed = 20;
        }
    }

    public class Heal : GM_State<GM_Hunter>
    {
        // �� Ŭ���� ����
        //GameObject medic = GameObject.Find("Medic");

        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Heal");
        }

        public override void Execute(GM_Hunter entity)
        {
            if (Input.GetKey(KeyCode.E))
            {
                entity.GetHeal();
            }
            else
            {
                entity.ChangeState(HunterStates.Idle);
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Heal");
        }
    }

    public class Groggy : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Groggy");
            entity.ChangeWeapon(1); // �׷α� ���� ����� ����
            entity.ActiveGroggyIcon();
        }

        public override void Execute(GM_Hunter entity)
        {
            // �ٸ� �÷��̾ ���� ü�� ȸ�� �Ǿ��� ��
           if(entity.Hp >= 100)
           {
                // ��Ȱ�� �� �׻� Shotgun ��� �ֵ���
                entity.weaponType = 0;
                entity.ChangeWeapon(0); // 0�� ����� ����
                entity.Hp = 100;
                entity.GroggyHpSlider.enabled = false;
                entity.ChangeState(HunterStates.Idle);
           }
           
           // Groggy ���¿��� groggyHp�� ���� �����Ǿ� ���������� ���� ��
           if(entity.GroggyHp <= 0)
           {
                entity.GroggyHp = 0;
                entity.ChangeState(HunterStates.Death);
           }

            entity.yVelocity += entity.gravity * Time.deltaTime;
            entity.yVelocity = Mathf.Clamp(entity.yVelocity, -6f, 100);
            entity.dir = Vector3.zero;
            entity.dir.Normalize();
            entity.dir.y = entity.yVelocity;
            entity.GetComponent<CharacterController>().Move(entity.dir * entity.moveSpeed * Time.deltaTime);
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Groggy");
            entity.InactiveGroggyIcon();
        }


    }

    public class Death : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Death");
            GameManager.instance.photonView.RPC("RPCManageLeftPlayer", RpcTarget.All, -1);
        }

        public override void Execute(GM_Hunter entity)
        {
            entity.respawnWaitTime -= Time.deltaTime;
            entity.respawnTimeText.text = $"00 : {(int)(entity.respawnWaitTime)}";
            // ���� �ð� ����
            if (entity.respawnWaitTime <= 0)
            {
                // ������ ��ҷ� �̵�

                // Idle ���·� ��Ȱ
                entity.ChangeState(HunterStates.Idle);
                entity.weaponType = 0;
                entity.ChangeWeapon(0);
                entity.Hp = 100;
                entity.GroggyHp = 100;
                entity.GroggyHpSlider.enabled = true;
                entity.respawnTimeText.text = "01 : 00";
                entity.respawnWaitTime = 60;
                GameManager.instance.photonView.RPC("RPCManageLeftPlayer", RpcTarget.All, 1);
            } 
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Death");
        }
    }

    // �⺻ ���¿� ������ �׻� ������Ʈ �ǰ� �ִ� ���� ����
    public class Global : GM_State<GM_Hunter>
    {
        float currentTime = 0;
        float accelMaxTime = 10;
        float accelCoolTime = 10;       // ��ų1 ��Ÿ��. ��ų ���� �ð� �߿��� 0���� �����ǰ�, ��ų ��� ���� �ƴ� �� ����. 
        bool isAccelerated = false;
        Image steamImage;
        Color color = new Color(255, 0, 0, 0);
        float alpha;

        float trackingWaitTime = 40;
        float trackingCoolTime = 40;     // ��ų2 ��Ÿ��. ��ų ���� ���ÿ� 0���� ���ϸ�, �ٷ� �ٽ� ����.
        GameObject[] hunters = new GameObject[4];

        float currentDashTime = 0;
        bool isDashing = false;
        float dashSpeed = 20;


        public override void Enter(GM_Hunter entity)
        {
            
        }

        public override void Execute(GM_Hunter entity)
        {
            steamImage = GameObject.Find("SteamImage").GetComponent<Image>();
            hunters = GameObject.FindGameObjectsWithTag("Player");

            // Hp�� 0 ���Ϸ� �������� Groggy ���� ��ȯ
            if (entity.Hp <= 0 && entity.CurrentState != HunterStates.Groggy && entity.CurrentState != HunterStates.Death)
            {
                entity.Hp = 0;
                entity.ChangeState(HunterStates.Groggy);
                entity.GroggyHpSlider.enabled = true;
            }

            // ���� ��ų �ߵ�
            if(Input.GetKeyDown(KeyCode.Alpha3) && accelCoolTime > 10)
            {
                isAccelerated = true;
            }

            // ��ų ���� �ð� ���� �̵� �� ����, ������ �ӵ� ����
            if(isAccelerated)
            {
                accelCoolTime = 0;
                currentTime += Time.deltaTime;

                alpha += Time.deltaTime;
                alpha = Mathf.Clamp(alpha, 0, 30.0f / 255.0f);
                color.a = alpha;
                steamImage.color = color;

                if(currentTime < accelMaxTime)
                {
                    entity.anim.speed = 1.5f;
                    entity.moveSpeed *= 1.5f;
                }
                else
                {
                    isAccelerated = false;
                    currentTime = 0;
                }
            }
            else
            {
                entity.anim.speed = 1;
                entity.moveSpeed *= 1;
                accelCoolTime += Time.deltaTime;

                alpha -= Time.deltaTime;
                alpha = Mathf.Clamp(alpha, 0, 30.0f / 255.0f);
                color.a = alpha;
                steamImage.color = color;
            }

            trackingCoolTime += Time.deltaTime;
            // Trapper ��ų �ߵ� (���� ��ġ ����)
            if (Input.GetKeyDown(KeyCode.Alpha4) && trackingCoolTime >= trackingWaitTime)
            {
                for (int i = 0; i < hunters.Length; i++)
                {
                    hunters[i].GetComponent<GM_Hunter>().IsTracking(true);
                    if(entity.photonView.IsMine)
                        entity.GetComponentInChildren<GM_TrapperScan>().TrapperScan();
                }
                trackingCoolTime = 0;
            }

            // Jump�� Falling �߿� Left Ctrl ������ �̵� �������� �뽬
            if (Input.GetKeyDown(KeyCode.LeftControl) && (entity.CurrentState == HunterStates.Jump || entity.CurrentState == HunterStates.Falling)
                && entity.Fuel > 0 && !isDashing)
            {
                isDashing = true;
                entity.Fuel -= 10;
            }

            if (isDashing)
            {
                currentDashTime += Time.deltaTime;
                dashSpeed -= Time.deltaTime * 3;
                entity.transform.position += new Vector3(entity.dir.x, 0, entity.dir.z) * Time.deltaTime * dashSpeed;


                if (currentDashTime > 1 || entity.GetComponent<CharacterController>().isGrounded)
                {
                    currentDashTime = 0;
                    isDashing = false;
                    dashSpeed = 20;
                }
            }

            // Jump ���� ���� ���������� Fuel �Ҹ�, ������ ���Ŀ��� �ٽ� ����
            if (entity.CurrentState == HunterStates.Jump)
            {
                entity.Fuel -= Time.deltaTime * 20;
            }
            else if(entity.CurrentState != HunterStates.Falling)
            {
                entity.Fuel += Time.deltaTime * 20;
            }

            // Slider �� ����
            entity.HpSlider.value = entity.Hp;
            entity.GroggyHpSlider.value = entity.GroggyHp;
            entity.BarrierSlider.value = entity.Barrier;
            entity.FuelSlider.value = entity.Fuel;

            entity.Weapon0Slider.value = entity.weapon0_currentAmo;
            entity.Weapon1Slider.value = entity.weapon1_currentAmo;
            entity.Skill1Slider.maxValue = accelMaxTime;
            entity.Skill1Slider.value = accelCoolTime;
            entity.Skill2Slider.maxValue = trackingWaitTime;
            entity.Skill2Slider.value = trackingCoolTime;
        }

        public override void Exit(GM_Hunter entity)
        {
            
        }
    }

    // �Ʒ����ʹ� upperBodyStates
    public class None : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("None");
        }

        public override void Execute(GM_Hunter entity)
        {
            if((Input.GetButton("Horizontal") || (Input.GetButton("Vertical"))) && entity.CurrentState == HunterStates.Move)
                entity.ChangeState_UpperBody(HunterUpperBodyStates.Move_Upper);
        }

        public override void Exit(GM_Hunter entity) 
        {
            entity.ResetTrigger("None");
        }
    }

    public class Move_Upper : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Move_Upper");
        }

        public override void Execute(GM_Hunter entity)
        {
            if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical") && entity.CurrentState == HunterStates.Move))
            { }
            else if(Input.GetKey(KeyCode.Mouse0))
            {
                entity.ResetTrigger("Move_Upper");
                entity.ChangeState_UpperBody(HunterUpperBodyStates.Attack);
            }
            else
            {
                entity.ResetTrigger("Move_Upper");
                entity.ChangeState_UpperBody(HunterUpperBodyStates.None);
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Move_Upper");
        }
    }

    public class Attack : GM_State<GM_Hunter>
    {
        LaserScript laserScript;

        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Attack");
            laserScript = entity.GetComponentInChildren<LaserScript>();
            if (entity.weaponType == 1)
                laserScript.EnableLaser();
        }

        public override void Execute(GM_Hunter entity)
        {
            // �ӵ� ���� ������ �߻��ϴ� �κ�
            if (entity.weaponType == 1)
            {
                if (Input.GetMouseButton(0))
                {
                    laserScript.UpdateLaser();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    laserScript.DisableLaserCaller(laserScript.disableDelay);
                }
            }

            // Attack �ִϸ��̼� ��� �ð� ���� ������ �Է¿� ���� ���� ���·� ���� 
            if (entity.anim.GetCurrentAnimatorStateInfo(1).IsName("Attack") && entity.anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.85f)
            { 
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    entity.Play("Attack", -1, 0f);
                }
                else if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
                {
                    entity.ResetTrigger("Attack");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Move_Upper);
                }
                else
                {
                    entity.ResetTrigger("Attack");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.None);
                }    
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Attack");
            if(laserScript && entity.weaponType == 1)
            {
                laserScript.DisableLaserCaller(laserScript.disableDelay);
                laserScript.ReturnMonsterSpeed();
            }
        }
    }

    public class GroggyAttack : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("GroggyAttack");
        }

        public override void Execute(GM_Hunter entity)
        {
            // Attack �ִϸ��̼� ��� �ð� ���� ������ �Է¿� ���� ���� ���·� ���� 
            if (entity.anim.GetCurrentAnimatorStateInfo(1).IsName("GroggyAttack") && entity.anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.85f)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    entity.Play("GroggyAttack", -1, 0f);
                }
                else
                {
                    entity.ResetTrigger("GroggyAttack");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.None);
                }
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("GroggyAttack");
        }
    }

    public class Replace : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Replace");
        }

        public override void Execute(GM_Hunter entity)
        {
            // Replace �ִϸ��̼� ��� �ð� ���� ������ �Է¿� ���� ���� ���·� ���� 
            if (entity.anim.GetCurrentAnimatorStateInfo(1).IsName("Replace") && entity.anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.95f)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    entity.ResetTrigger("Replace");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Attack);
                }
                else if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
                {
                    entity.ResetTrigger("Replace");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Move_Upper);
                }
                else
                {
                    entity.ResetTrigger("Replace");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.None);
                }
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Replace");
        }
    }

    public class Reload : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        {
            entity.SetTrigger("Reload");
        }

        public override void Execute(GM_Hunter entity)
        {
            // Reload �ִϸ��̼� ��� �ð� ���� ������ �Է¿� ���� ���� ���·� ���� 
            if (entity.anim.GetCurrentAnimatorStateInfo(1).IsName("Reload") && entity.anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 1f)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    entity.ResetTrigger("Reload");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Attack);
                }
                else if (Input.GetButton("Horizontal") || (Input.GetButton("Vertical")))
                {
                    entity.ResetTrigger("Reload");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Move_Upper);
                }
                else
                {
                    entity.ResetTrigger("Reload");
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.None);
                }
            }
        }

        public override void Exit(GM_Hunter entity)
        {
            entity.ResetTrigger("Reload");
            if (entity.weaponType == 0)
                entity.weapon0_currentAmo = entity.weapon0_MaxAmo;
            else
                entity.weapon1_currentAmo = entity.weapon1_MaxAmo;
        }
    }

    // �⺻ ��ü ���¿� ������ �׻� ������Ʈ �ǰ� �ִ� ���� ��ü ����
    public class Global_Upper : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        { }

        public override void Execute(GM_Hunter entity)
        {
            // weaponType �����ؼ� �׻� ���⿡ �´� �ִϸ��̼� ���
            entity.SetFloat("Weapon_Type", entity.weaponType);

            // �Ѿ� ���� �Ҹ�� ������ ���·� ��ȭ
            if (entity.weaponType == 0 && entity.weapon0_currentAmo <= 0)
            {
                entity.ChangeState_UpperBody(HunterUpperBodyStates.Reload);
            }

            if (entity.weaponType == 1 && entity.weapon1_currentAmo <= 0)
            {
                entity.ChangeState_UpperBody(HunterUpperBodyStates.Reload);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0)) 
            {
                // ��ü ���� Groggy ���¿����� ��ü ���� GroggyAttack ��ȯ ����
                if (entity.CurrentState == HunterStates.Groggy)
                {
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.GroggyAttack);
                }

                // Reload�� Replace �߿��� Attack �Ұ���
                else if (entity.CurrentUpperBodyState != HunterUpperBodyStates.Reload && entity.CurrentUpperBodyState != HunterUpperBodyStates.Replace)
                {
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Attack);
                }    
            }

            if(entity.CurrentState != HunterStates.Groggy)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    // Replace �߿��� Reload �Ұ���
                    if (entity.CurrentUpperBodyState != HunterUpperBodyStates.Replace)
                        entity.ChangeState_UpperBody(HunterUpperBodyStates.Reload);
                }

                if (Input.GetKeyDown(KeyCode.Alpha1) && entity.weaponType != 0)
                {
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Replace);
                    entity.weaponType = 0;
                    if (entity.CurrentUpperBodyState == HunterUpperBodyStates.Replace)
                    {
                        entity.Play("Replace", -1, 0f);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha2) && entity.weaponType != 1)
                {
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Replace);
                    entity.weaponType = 1;
                    if (entity.CurrentUpperBodyState == HunterUpperBodyStates.Replace)
                    {
                        entity.Play("Replace", -1, 0f);
                    }
                }

                if (entity.Hp <= 0)
                {
                    entity.Hp = 0;
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.None);
                }
            }
        }

        public override void Exit(GM_Hunter entity)
        { }
    }
}