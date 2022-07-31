using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type {A, B, C}; // 몬스터의 타입 정하기
    public Type enemyType;
    public int maxHP;
    public int curHP;
    public int score;    
    public GameObject[] Ammo;

    public GameManager gameManager;
    public Transform target; // 따라갈 목표물
    public bool isChase; //추적을 결정하는 변수
    public bool isAttack; // 공격중 인가 ?
    public BoxCollider EnemyAttack; // 몬스터의 공격

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material material;
    NavMeshAgent nav;
    Animator anim;
    public AudioClip[] auClip; //오디오

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        material = GetComponentInChildren<SkinnedMeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        GetComponent<AudioSource>().clip = auClip[0];        
        GetComponent<AudioSource>().volume = 0.4f; // 오디오 소리

        Invoke("ChaseStart", 1);
    }
    void Start() // 시작하면서 좀비 사운드 소리내기
    {
        GetComponent<AudioSource>().clip = auClip[2];
        GetComponent<AudioSource>().volume = 0.1f; // 오디오 소리
        GetComponent<AudioSource>().Play();
    }

    void ChaseStart() // 추척을 결정할 함수
    {
        
        switch(enemyType)
        {
            case Type.A:               
                isChase = true;
                anim.SetBool("isWalk", true);               
                break;
            case Type.B:               
                isChase = true;
                anim.SetBool("isRun", true);               
                break;
            case Type.C:
                isChase = true;
                anim.SetBool("isWalk", true);
                break;
        }
        

    }
    void Update()
    {
        if (nav.enabled) //목표물이 활성화 되어있을때만 경우에만 nav 작동
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase; // 완벽하게 추적이 멈추도록
        }
    }

    void FreezeVelocity()
    {
        if (isChase) // 추적 중일때만
        {
            rigid.angularVelocity = Vector3.zero; // 물리력이 회전력 안받도록
            rigid.velocity = Vector3.zero;  // 물리력이 navagent에 방해 안받도록
        }
    }
    void Targerting()
    {
        float targetRadius = 1f; // 폭
        float targetRange = 0.5f; // 거리
        

        RaycastHit[] rayHits =
            Physics.SphereCastAll(transform.position,
                                  targetRadius,
                                  transform.forward,
                                  targetRange, // 공격 거리
                                  LayerMask.GetMask("Player"));

        if(rayHits.Length > 0 && !isAttack) // rayhits에 들어왔을때 , 공격중 아닐때
        {
            GetComponent<AudioSource>().clip = auClip[3];
            GetComponent<AudioSource>().volume = 0.2f; // 오디오 소리
            GetComponent<AudioSource>().Play();
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        isChase = false; // 먼저 정지를 한다음 
        isAttack = true; // 공격중이다 true
        anim.SetBool("isAttack", true);    // 애니메이션 실행
        EnemyAttack.enabled = true; // 공격 범위 활성화 box true

        yield return new WaitForSeconds(1f);

        EnemyAttack.enabled = false; // 공격 범위 활성화 box false
        isChase = true; // 다시 추적 실행 
        isAttack = false; // 공격중이 아니다.
        anim.SetBool("isAttack", false);    // 애니메이션 실행 종료
    }
    void FixedUpdate()
    {
        Targerting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bullet")
        {          
            Bullet bullet = other.GetComponent<Bullet>(); // bullet 스크립트를 가져와서
            curHP -= bullet.damage;  // 데미지를 깎는다.
            Vector3 reactVec = transform.position - other.transform.position;
            GetComponent<AudioSource>().clip = auClip[0];
            GetComponent<AudioSource>().volume = 0.5f; // 오디오 소리
            GetComponent<AudioSource>().Play();

            StartCoroutine(OnDamage(reactVec));   
        }
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        
        material.color = Color.red;      
        reactVec = reactVec.normalized;
        reactVec += Vector3.forward;
        rigid.AddForce(reactVec * 5, ForceMode.Impulse);
        if (curHP > 0)
        {
            material.color = Color.white;
            yield return new WaitForSeconds(0.1f);          
        }
        else
        {
            EnemyAttack.enabled = false;
            material.color = Color.gray;
            gameObject.layer = 14; //enemydead;
            isChase = false; // 추적 종료
            isAttack = false;
            nav.enabled = false;           
            GetComponent<AudioSource>().clip = auClip[1];
            GetComponent<AudioSource>().volume = 0.3f; // 오디오 소리
            GetComponent<AudioSource>().Play();
            anim.SetTrigger("doDie"); // 죽음 애니메이션 트리거로 실행         

            Player player = target.GetComponent<Player>();
            player.score += score;

            int ranItem = Random.Range(0, 5);           
            Instantiate(Ammo[ranItem], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                case Type.A:
                    gameManager.enemyCntA--;
                    break;
                case Type.B:
                    gameManager.enemyCntB--;
                    break;
                case Type.C:
                    gameManager.enemyCntC--;
                    break;
            }

            Destroy(gameObject , 3f);
        }
    }
}
