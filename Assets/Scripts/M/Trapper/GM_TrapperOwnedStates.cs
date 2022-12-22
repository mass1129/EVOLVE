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
                GameObject[] hunters = GameObject.FindGameObjectsWithTag("Player"); // Player 태그 달고 있는 게임오브젝트 전부 배열에 담음.
                for (int i = 0; i < hunters.Length; i++)
                {
                    Debug.Log(hunters[i].GetComponent<GM_Hunter>().CurrentState);
                    if (Vector3.Distance(hunters[i].transform.position, entity.transform.position) < 1.5f
                        && hunters[i].GetComponent<GM_Hunter>().CurrentState == HunterStates.Groggy) // 그로기 상태인 헌터와 거리가 1.5 미만이면
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

                // 이동 방향에 따라 Move BlendTree 값 설정
                entity.SetFloat("Move_Horizontal", entity.h);
                entity.SetFloat("Move_Vertical", entity.v);

                // 이동 중에 떨어지면 Falling으로 상태 전환
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

            // 제트팩 기능. Jump 지속 입력하면 zetpackPower에 의해 상승
            if (Input.GetButton("Jump"))
            {
                entity.yVelocity += zetpackPower * Time.deltaTime;
            }

            // Falling 상태 변화 조건
            if (entity.yVelocity <= 0.1f || entity.Fuel <= 0) // yVelocity가 감소할 때, 또는 Fuel 전부 소모했을 때
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

                // Falling 상태에서 다시 점프 누르면 yVelocity 초기화 및 Jump로 상태 전환 (사용 가능한 Fuel 있을 때) 
                if (Input.GetButtonDown("Jump") && entity.Fuel > 0)
                {
                    entity.yVelocity = 0.2f;
                    entity.ChangeState(HunterStates.Jump);
                }

                // 땅에 닿으면 Landing 애니메이션 재생
                if (entity.GetComponent<CharacterController>().isGrounded)
                {
                    entity.SetTrigger("Landing");
                    isLanding = true;
                }
            }

            if (entity.anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                entity.SetTrigger("Landing");


            // Landing 애니메이션 재생 시간 끝난 이후의 입력에 따라 다음 상태로 전이 
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
        // 각 클래스 선언
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
            entity.ChangeWeapon(1); // 그로기 상태 무기로 변경
            entity.ActiveGroggyIcon();
        }

        public override void Execute(GM_Hunter entity)
        {
            // 다른 플레이어에 의해 체력 회복 되었을 때
           if(entity.Hp >= 100)
           {
                // 부활할 때 항상 Shotgun 들고 있도록
                entity.weaponType = 0;
                entity.ChangeWeapon(0); // 0번 무기로 변경
                entity.Hp = 100;
                entity.GroggyHpSlider.enabled = false;
                entity.ChangeState(HunterStates.Idle);
           }
           
           // Groggy 상태에서 groggyHp도 전부 소진되어 최종적으로 죽을 때
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
            // 일정 시간 이후
            if (entity.respawnWaitTime <= 0)
            {
                // 리스폰 장소로 이동

                // Idle 상태로 부활
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

    // 기본 상태와 별개로 항상 업데이트 되고 있는 전역 상태
    public class Global : GM_State<GM_Hunter>
    {
        float currentTime = 0;
        float accelMaxTime = 10;
        float accelCoolTime = 10;       // 스킬1 쿨타임. 스킬 지속 시간 중에는 0으로 유지되고, 스킬 사용 중이 아닐 때 증가. 
        bool isAccelerated = false;
        Image steamImage;
        Color color = new Color(255, 0, 0, 0);
        float alpha;

        float trackingWaitTime = 40;
        float trackingCoolTime = 40;     // 스킬2 쿨타임. 스킬 사용과 동시에 0으로 변하며, 바로 다시 증가.
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

            // Hp가 0 이하로 내려가면 Groggy 상태 전환
            if (entity.Hp <= 0 && entity.CurrentState != HunterStates.Groggy && entity.CurrentState != HunterStates.Death)
            {
                entity.Hp = 0;
                entity.ChangeState(HunterStates.Groggy);
                entity.GroggyHpSlider.enabled = true;
            }

            // 가속 스킬 발동
            if(Input.GetKeyDown(KeyCode.Alpha3) && accelCoolTime > 10)
            {
                isAccelerated = true;
            }

            // 스킬 지속 시간 동안 이동 및 공격, 재장전 속도 증가
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
            // Trapper 스킬 발동 (몬스터 위치 추적)
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

            // Jump나 Falling 중에 Left Ctrl 누르면 이동 방향으로 대쉬
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

            // Jump 중일 때는 지속적으로 Fuel 소모, 착지한 이후에는 다시 충전
            if (entity.CurrentState == HunterStates.Jump)
            {
                entity.Fuel -= Time.deltaTime * 20;
            }
            else if(entity.CurrentState != HunterStates.Falling)
            {
                entity.Fuel += Time.deltaTime * 20;
            }

            // Slider 값 갱신
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

    // 아래부터는 upperBodyStates
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
            // 속도 저하 레이저 발사하는 부분
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

            // Attack 애니메이션 재생 시간 끝난 이후의 입력에 따라 다음 상태로 전이 
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
            // Attack 애니메이션 재생 시간 끝난 이후의 입력에 따라 다음 상태로 전이 
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
            // Replace 애니메이션 재생 시간 끝난 이후의 입력에 따라 다음 상태로 전이 
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
            // Reload 애니메이션 재생 시간 끝난 이후의 입력에 따라 다음 상태로 전이 
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

    // 기본 상체 상태와 별개로 항상 업데이트 되고 있는 전역 상체 상태
    public class Global_Upper : GM_State<GM_Hunter>
    {
        public override void Enter(GM_Hunter entity)
        { }

        public override void Execute(GM_Hunter entity)
        {
            // weaponType 감지해서 항상 무기에 맞는 애니메이션 재생
            entity.SetFloat("Weapon_Type", entity.weaponType);

            // 총알 전부 소모시 재장전 상태로 변화
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
                // 전체 상태 Groggy 상태에서만 상체 상태 GroggyAttack 전환 가능
                if (entity.CurrentState == HunterStates.Groggy)
                {
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.GroggyAttack);
                }

                // Reload나 Replace 중에는 Attack 불가능
                else if (entity.CurrentUpperBodyState != HunterUpperBodyStates.Reload && entity.CurrentUpperBodyState != HunterUpperBodyStates.Replace)
                {
                    entity.ChangeState_UpperBody(HunterUpperBodyStates.Attack);
                }    
            }

            if(entity.CurrentState != HunterStates.Groggy)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    // Replace 중에는 Reload 불가능
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