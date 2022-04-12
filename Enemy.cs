using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHP;
    public int curHP;

    bool isBlood;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material material;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        material = GetComponent<MeshRenderer>().material;
        isBlood = GetComponentInChildren<ParticleSystem>();
       
    }

    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>(); // bullet 스크립트를 가져와서
            curHP -= bullet.damage;  // 데미지를 깎는다.

            StartCoroutine(OnDamage());   
        }
    }

    IEnumerator OnDamage()
    {
        material.color = Color.red;
        isBlood = true;
        yield return new WaitForSeconds(0.1f);
        isBlood = false;
        yield return new WaitForSeconds(0.1f);
        if(curHP > 0)
        {
            material.color = Color.white;
        }
        else
        {
            material.color = Color.gray;
            gameObject.layer = 14; //enemydead;
            Destroy(gameObject, 5);
        }
    }
}
