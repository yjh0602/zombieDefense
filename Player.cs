using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public float speed;
    float hAxis;
    float vAxis;

    public float Rate; // 공격 속도


    public int ammo; // 현재 탄수
    public int health;
    

    public int MaxAmmo; // 최대 넘을수없는값
   
    public int MaxHealth;

    
    public int WmaxAmmo; // 무기의 먹었을때 탄

    public GameObject bullet;
    public GameObject bulletCase;
    public Transform bulletPos;
    public Transform bulletCasePos;

    public Camera followCamera;

    

    bool fDown; // 공격입력
    bool rDown; // 장전
    bool jDown;
    

    bool isJump; // 점프를 하고있는지 확인
    bool isFireReady; //공격 준비
    bool isReload; // 장전 시간

    Vector3 moveVec;

    Rigidbody rigid;
    Animator anim;

    float fireDelay;


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        GetInput(); // 제일 위에 해야 아래 함수가 작동한다.

        Move();
        Turn();
        Jump();
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

        if(isReload || fDown)
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
        if (fDown) // 왼쪽 클릭을 했을 때만
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
        if (jDown && !isJump) // 키를 입력했거나 점프가 false 일때
        {
            rigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
            isJump = true;
        }
    }

    void Attack()
    {
        fireDelay += Time.deltaTime; // fireDelay를 채워나간다.
        isFireReady = Rate < fireDelay; // 현재 공격속도보다 커지면 fireReady가 true;

        if (fDown && isFireReady && ammo > 0)
        {
            ammo--; //총알 쏠때 마이너스
            
            StartCoroutine("Shot");
            anim.SetTrigger("doShot");

            fireDelay = 0; // 쿨타임

        }
    }

    void Reload()
    {
        if (WmaxAmmo == 0)
            return;

        if (rDown && !isJump && ammo == 0)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        if(WmaxAmmo > 0)
        {
            WmaxAmmo -= 30;
            ammo = 30;
        }   
        isReload = false;
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



    //아이템 획득
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    WmaxAmmo += item.value;
                    if (WmaxAmmo > MaxAmmo) // max 값을 못넘기게
                        WmaxAmmo = MaxAmmo;
                    break;
                    
                case Item.Type.Health:
                    health += item.value;
                    if (health > MaxHealth) // max 값을 못넘기게
                        health = MaxHealth;
                    break;
            }
            Destroy(other.gameObject); // 먹고 난 후 사라짐.
        }
        

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isJump = false;
        }
    }
    
}
