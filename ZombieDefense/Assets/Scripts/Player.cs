using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed;
    public float Rate; // 공격 속도


    public int ammo; // 현재 탄수
    public int health;
    public int score;
    

    public int MaxAmmo; // 탄창에 넣을수 있는 최대값
    public int MaxHealth;
    public int curAmmo; // 현재 소지 탄

    public GameObject bullet; // 총알
    public GameObject bulletCase; // 탄피
    public Transform bulletPos;  //소환될 총알 위치
    public Transform bulletCasePos; // 소환될 탄피 위치
    public Camera followCamera; // 카메라
    public GameManager gameManager;

    public AudioClip[] auClip; //오디오

    float hAxis;
    float vAxis;
    float fireDelay; //공격준비

    bool fDown; // 공격입력
    bool rDown; // 장전
    bool jDown; // 점프
    

    bool isJump; // 점프를 하고있는지 확인
    bool isDodge; // 회피
    bool isFireReady; //공격 준비
    bool isReload; // 장전 시간
    bool isDamage; // 공격 맞았을때
    bool isDead;

    Vector3 moveVec; // 이동
    Vector3 dodgeVec; // 회피
    Rigidbody rigid;
    Animator anim;
    

    


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        GetComponent<AudioSource>().clip = auClip[0];
        GetComponent<AudioSource>().volume = 0.2f; // 오디오 소리

        // PlayerPrefs.SetInt("MaxScore", 112500);
       
    }


    void Update()
    {
        GetInput(); // 제일 위에 해야 아래 함수가 작동한다.

        Move();
        Turn();
        Jump();
        Dodge();
        Attack();
        Reload();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1"); //마우스 왼쪽
        rDown = Input.GetButtonDown("Reload"); // R
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if(isDodge)
        {
            moveVec = dodgeVec;
        }
        if(isReload || fDown || isDamage && !isDead)
        {
            moveVec = Vector3.zero;
        }

        transform.position += moveVec * speed * Time.deltaTime;

        anim.SetBool("isRun", moveVec != Vector3.zero); // 무브가 0이 아니면
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec); // 나아가는 방향으로 바라보기

        //마우스에 의한 회전
        if (fDown && !isDead) // 왼쪽 클릭을 했을 때만
        {         
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // 스크린에서 월드로 ray를 쏘는 함수
            RaycastHit rayHit; // 마우스 찍었을때 그걸 담는 변수
            if (Physics.Raycast(ray, out rayHit, 100)) // out  == return 처럼 반환값을 주어진 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; //y 축을 없엔다.
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDead) // 키를 입력했거나 점프가 false 일때
        {
            rigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
            isJump = true;
        }
    }
    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isJump && !isDead) // 키를 입력했거나 점프가 false 일때
        {
            dodgeVec = moveVec;
            speed *= 1.5f;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
        }
    }
    void DodgeOut()
    {
        speed /= 1.5f;
        isDodge = false;
    }



    void Attack()
    {
        fireDelay += Time.deltaTime; // fireDelay를 채워나간다.
        isFireReady = Rate < fireDelay; // 현재 공격속도보다 커지면 fireReady가 true;

        if (fDown && isFireReady && !isDodge && ammo > 0 && !isDead)
        {
            ammo--; //총알 쏠때 마이너스

            GetComponent<AudioSource>().clip = auClip[0];
            GetComponent<AudioSource>().volume = 0.1f; // 오디오 소리
            GetComponent<AudioSource>().Play();
            StartCoroutine("Shot");
            anim.SetTrigger("doShot");

            fireDelay = 0; // 쿨타임

        }
    }

    void Reload()
    {
        if (curAmmo == 0 && ammo == 0)
            return;

        if (rDown && !isJump && !isReload && isFireReady & !isDead)
        {
            GetComponent<AudioSource>().clip = auClip[1];
            GetComponent<AudioSource>().Play();
            isReload = true;
            anim.SetTrigger("doReload");
            
            Invoke("ReloadOut", 3f);           
        }
        
    }

    void ReloadOut()
    {
        int reAmmo = MaxAmmo - ammo; // 최대 탄창수 - 현재총에있는 탄
        if (!(curAmmo < reAmmo))
        {
            ammo = ammo + reAmmo; // 현재 총에 넣어준다.
            curAmmo -= reAmmo; // 가진 총알에서 그만큼 빼준다.
            isReload = false;
        }
        else
        {
            Debug.Log("Not Enough Ammo");
            isReload = false;
            return;
        }      
    }

    void onDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        gameManager.GameOver();
    }
    IEnumerator Shot()
    {
        //총알 발사
        
        GameObject intantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 100;
        yield return null;
        //탄피 배출
        GameObject intantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody CaseRigid = intantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.right * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        CaseRigid.AddForce(caseVec, ForceMode.Impulse);
        CaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        yield return null;
    }



    //아이템 획득 , 상대방 공격
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    curAmmo += item.value;
                    break;
                    
                case Item.Type.Health:
                    health += item.value;
                    if (health > MaxHealth) // max 값을 못넘기게
                        health = MaxHealth;
                    break;
            }
            Destroy(other.gameObject); // 먹고 난 후 사라짐.
        }
        else if(other.gameObject.tag == "EnemyBullet")
        {
            
            if (!isDamage)
            {              
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;
                StartCoroutine(OnDamage());              
            }          
        }

    }

    IEnumerator OnDamage()
    {

        // 피를 흘리게 오브젝트를 복제할 예정.
        isDamage = true;
        anim.SetBool("isHit",true);
        yield return new WaitForSeconds(0.7f);
        anim.SetBool("isHit", false);
        isDamage = false;

        if (health <= 0)
            onDie();           
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJump = false;
        }
    }
    
}
