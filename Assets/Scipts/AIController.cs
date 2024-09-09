using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIController : MonoBehaviour
{
    public bool followPlayer = true;
    public Transform[] randomLocations; // Assign this array in the Inspector with the locations you want the AI to potentially go to
    private NavMeshAgent agent;
    private Transform playerTransform;
    private int currentDestinationIndex = -1;
    public bool hasBomb;
    public GameObject bombObject;

    public bool isHunter;
    public Transform currentAI;
    public GameObject green;
    public GameObject red;
    private void Awake()
    {
        hasBomb = false;
        bombObject.SetActive(false);

        randomLocations = GamePlayHandler.Instance.AI_Places;
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Attempt to find the player GameObject by tag
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (playerGameObject != null)
        {
            playerTransform = playerGameObject.transform;
        }
        else
        {
            Debug.LogWarning("Player GameObject not found. Make sure it's tagged correctly.");
        }

        // If not following the player, move to a random location to start
        if (!followPlayer)
        {
            MoveToNextRandomLocation();
        }
    }

    void Update()
    {
        if (isHunter)
        {
            if (currentAI == null)
            {

                MoveToNextAILocation();
                return;
            }
            agent.speed = 4.5f;
            agent.SetDestination(currentAI.position);
        }
        else if ((followPlayer || hasBomb) && playerTransform != null)
        {
            // Move towards the player
            if(hasBomb)
            {
                agent.speed = 10f;
            }
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            agent.speed = 3.5f;
            // Check if we've reached our current destination
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                MoveToNextRandomLocation();
            }
            agent.SetDestination(randomLocations[currentDestinationIndex].position);
        }
    }

    void MoveToNextRandomLocation()
    {
        if (randomLocations.Length == 0)
        {
            Debug.LogError("No random locations assigned.");
            return;
        }

        // Pick a new random location that isn't the same as the last one
        int newIndex;
        do
        {
            newIndex = Random.Range(0, randomLocations.Length);
        } while (newIndex == currentDestinationIndex);

        currentDestinationIndex = newIndex;
        agent.SetDestination(randomLocations[currentDestinationIndex].position);
    }
    void MoveToNextAILocation()
    {
        if (((Random.Range(0, 100)) > 50 && GameManager.Instance.isRunner) || GamePlayHandler.Instance.AI_Active.Count == 0)
        {
            currentAI = playerTransform;
        }
        else
        {
            if (GamePlayHandler.Instance.AI_Active.Count == 0)
            {
                Debug.LogError("No random locations assigned.");
                return;
            }

            // Pick a new random location that isn't the same as the last one
            int newIndex;
            newIndex = Random.Range(0, GamePlayHandler.Instance.AI_Active.Count);


            currentDestinationIndex = newIndex;
            if (GamePlayHandler.Instance.AI_Active[currentDestinationIndex] != null)
            {
                currentAI = GamePlayHandler.Instance.AI_Active[currentDestinationIndex].transform;
            }
      
        }

    }
    private void OnTriggerEnter(Collider other)
    {



        if (isHunter)
        {
            if (other.gameObject.tag == "AI")
            {
                if (!other.gameObject.GetComponent<AIController>().isHunter)
                {
                    Destroy(Instantiate(GamePlayHandler.Instance.AIKillparticle, other.transform.position, Quaternion.identity), 3f);
                    GamePlayHandler.Instance.AI_Active.Remove(other.gameObject);
                    Destroy(other.gameObject);
                    GamePlayHandler.Instance.AI_Active.RemoveAll(x => x == null);
                    GamePlayHandler.Instance.UpdateHuntersVsRunnersCount();
                }
                if(!GameManager.Instance.isRunner)
                {
                    if (GamePlayHandler.Instance.AI_Active.Count == 0 && !GamePlayHandler.Instance.failPanel.activeSelf)
                    {
                        GamePlayHandler.Instance.winPanel.SetActive(true);
                    }
                }
            }
          
        }



    }
}
