using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor") // bullet case
        {
            Destroy(gameObject, 3); // 떨어지고 3초뒤에 사라지게
        }
    }
   void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall" || other.gameObject.tag == "Enemy") // bullet
        {
            Destroy(gameObject);
        }
        else
            Destroy(gameObject, 5f);
    }
}
