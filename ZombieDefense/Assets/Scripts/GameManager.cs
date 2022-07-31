using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public GameObject ammoTruck;
    public GameObject healTruck;
    public GameObject startZone;
    public GameObject Light; // 낮 밤
    

    public int stage; // 스테이지
    public float playTime; // 플레이한 시간
    public bool isBattle; // 싸우는 상황 온 오프
    //public float battlePlayTime; // 싸우는 시간

    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;

    public Transform[] enemyZones;
    public GameObject[] enemies;
    public List<int> ememyList;



    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;
    public Text maxScoreTxt;

    // gamePanel
    public Text scoreTxt;
    public Text stageTxt;
    public Text playTimeTxt;

    public Text playerHealthTxt;
    public Text playerAmmoTxt;
    public Text playerCarAmmoTxt;


    public Text curScoreText;

   

    void Awake()
    {    //string.Format() 함수로 문자열 양식 적용
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));        
        ememyList = new List<int>();
    }

    void Start()
    {        
    }

    public void GameStart()
    {       
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }
    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;       
    }

    public void StageStart()
    {
       
        Light.SetActive(false);
        ammoTruck.SetActive(false);
        healTruck.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(InBattle());
    }
    public void StageEnd()
    {      
        
        Light.SetActive(true);
        player.transform.position = new Vector3(1.43f, 0, -2.53f);

        ammoTruck.SetActive(true);
        healTruck.SetActive(true);
        startZone.SetActive(true);

        foreach (Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(false);
        }

        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        for(int index = 0; index < stage; index++)
        {
            int ran = Random.Range(0, 3);
            ememyList.Add(ran);

            switch (ran)
            {
                case 0:
                    enemyCntA++;
                    break;
                case 1:
                    enemyCntB++;
                    break;
                case 2:
                    enemyCntC++;
                    break;
            }
        }
        while(ememyList.Count > 0)
        {
            int ranZone = Random.Range(0, 4);
            GameObject instantEnemy = Instantiate(enemies[ememyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.gameManager = this;
            ememyList.RemoveAt(0);
            yield return new WaitForSeconds(5f);
        }      

        while(enemyCntA + enemyCntB + enemyCntC > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(4f);
        StageEnd();
    }
    void Update()
    {

        if (isBattle)
        {
            playTime += Time.deltaTime;
        }
        
        
    }
    //update()가 끝난 후 호출되는 생명주기
    void LateUpdate() 
    {   //상단 UI
        scoreTxt.text = string.Format("{0:n0}", player.score);
        stageTxt.text = "STAGE " + stage;
        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);


        // 플레이어 UI
        playTimeTxt.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);
        playerHealthTxt.text = player.health + " / " + player.MaxHealth;
        playerAmmoTxt.text = string.Format("{0:n0}", player.ammo) + " / " + player.MaxAmmo;
        playerCarAmmoTxt.text = string.Format("{0:n0}", player.curAmmo);

    }
}
