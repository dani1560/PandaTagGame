using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Firebase.Analytics;

public class GamePlayHandler : MonoBehaviour
{
    public static GamePlayHandler Instance;

    public PlayerMovementAndClimbing player;


    public GameObject winPanel;
    public GameObject failPanel;
    public GameObject reSpawnPanel;

    public GameObject AIPrefab;
    public List<GameObject> AI_Active;
    public List<GameObject> AI_HuntersActive;
  

    public float time;

    public Transform[] AI_Places;
    public TextMeshProUGUI timeTxt;

    public GameObject AIKillparticle;

    [Header("Bomb Tag")]
    public GameObject bombTagObjCanvas;

    [Header("Hunter vs Runner")]
    public int runnerLives = 3;
    public GameObject[] runnerLivesObj;
    public GameObject runnerObjCanvas;
    public GameObject hunterVsRunnerObjCanvas;
    public TextMeshProUGUI hunterCountTxt;
    public TextMeshProUGUI runnerCountTxt;

    [Header("Info")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoTitle;
    public TextMeshProUGUI infoDetail;

    [Header("Paused")]
    public GameObject pausePanel;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1.0f;
    }
    private void Start()
    {
        AI_Active = new List<GameObject>();
        AI_HuntersActive = new List<GameObject>();
        if (GameManager.Instance.isBombTag)
        {
          
            infoTitle.text = "Bomer";
            infoDetail.text = "If You Have The Bomb Rush To Others Before the Time Out and Explodes";
            time = 90f;
            for (int i = 0;i<5;i++)
            {
                GameObject gg=Instantiate(AIPrefab, AI_Places[Random.Range(0, AI_Places.Length)].transform.position, Quaternion.identity);
                AI_Active.Add(gg);
                gg.GetComponent<AIController>().followPlayer = false;
                gg.GetComponent<AIController>().isHunter = false;
                gg.GetComponent<AIController>().red.SetActive(true);
            }
            bombTagObjCanvas.SetActive(true);
        }
        else
        {
            time = 120f;
            if (GameManager.Instance.isRunner)
            {
                infoTitle.text = "You Are Runner";
                infoDetail.text = "Run Away from hunters before time runs out";
                runnerObjCanvas.SetActive(true);
                StartCoroutine(AddHunter(10, 15f));
            }
            else
            {
                infoTitle.text = "You Are Hunter";
                infoDetail.text = "Catch as many runners as possible before time runs out.";
                StartCoroutine(AddHunter(5, 15f));
            }
            for (int i = 0; i < 10; i++)
            {
                GameObject gg = Instantiate(AIPrefab, AI_Places[Random.Range(0, AI_Places.Length/2)].transform.position, Quaternion.identity);
                AI_Active.Add(gg);
                gg.GetComponent<AIController>().followPlayer = false;
                gg.GetComponent<AIController>().isHunter = false;
                gg.GetComponent<AIController>().red.SetActive(true);

            }
            for (int i = 0; i < 2; i++)
            {
                GameObject gg = Instantiate(AIPrefab, AI_Places[Random.Range(AI_Places.Length / 2, AI_Places.Length)].transform.position, Quaternion.identity);
                AI_HuntersActive.Add(gg);
                gg.GetComponent<AIController>().followPlayer = false;
                gg.GetComponent<AIController>().isHunter = true;
                gg.GetComponent<NavMeshAgent>().speed = 4f;
                gg.GetComponent<AIController>().green.SetActive(true);

            }

            hunterVsRunnerObjCanvas.SetActive(true);
            UpdateHuntersVsRunnersCount();
        }
        infoPanel.SetActive(true);
       
        StartCoroutine(StartTimer());
        Time.timeScale = 0f;
    }
    public void GameStart()
    {
        Time.timeScale = 1f;
    }
    IEnumerator AddHunter(int limit,float timeWait)
    {
        for (int i = 0;i<limit;i++)
        {
            yield return new WaitForSeconds(timeWait);
            GameObject gg = Instantiate(AIPrefab, AI_Places[Random.Range(0, AI_Places.Length)].transform.position, Quaternion.identity);
            AI_HuntersActive.Add(gg);
            gg.GetComponent<AIController>().followPlayer = false;
            gg.GetComponent<AIController>().isHunter = true;
            gg.GetComponent<NavMeshAgent>().speed = 4f;
            gg.GetComponent<AIController>().green.SetActive(true);
            UpdateHuntersVsRunnersCount();
        }
    }
    public void UpdateHuntersVsRunnersCount()
    {
        hunterCountTxt.text = AI_HuntersActive.Count.ToString();
        runnerCountTxt.text=AI_Active.Count.ToString();
    }
    public void UpdateRunnerLives()
    {
        runnerLivesObj[runnerLives].SetActive(false);
       reSpawnPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void ReSpawn()
    {
        player.transform.position = AI_Places[Random.Range(0,AI_Places.Length)].transform.position;
        player.CinemachineCameraTarget.transform.localRotation = Quaternion.identity;
        Time.timeScale = 1.0f;
        reSpawnPanel.SetActive(false);
    }
    public void Pause()
    {
        if(pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void Home()
    {
     
        SceneManager.LoadScene(0);
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private float sessionTime;
    public static string FormatSeconds(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    IEnumerator StartTimer()
    {
        sessionTime = 0f;

        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            time -= 1f;
            sessionTime += 1f;
            timeTxt.text = FormatSeconds((int)time);
        }

        if (GameManager.Instance.isBombTag)
        {
            if (player.HasBomb)
            {
                failPanel.SetActive(true);
            }
            else
            {
                winPanel.SetActive(true);
            }
        }
        else
        {
            if (GameManager.Instance.isRunner)
            {
                if (!failPanel.activeSelf && runnerLives >= 0)
                {
                    winPanel.SetActive(true);
                }
                else
                {
                    if (!winPanel.activeSelf)
                    {
                        failPanel.SetActive(true);
                    }
                }
            }
            else
            {
                if (AI_Active.Count == 0 && !failPanel.activeSelf)
                {
                    winPanel.SetActive(true);

                    // Log game session time
                    FirebaseAnalytics.LogEvent("gamesession_time", new Parameter("time", sessionTime));
                }
                else if (!winPanel.activeSelf)
                {
                    failPanel.SetActive(true);
                }
            }
        }

        Time.timeScale = 0f;
    }
}
