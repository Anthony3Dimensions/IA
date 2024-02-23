using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;

public class IAenemigo : MonoBehaviour

{
    public enum State
    {
        Patrolling,

        Chasing,

        Searching,

        Attackin,

        Waiting
    }

    public State currentState;

    NavMeshAgent enemyAgent;

    Transform playerTransform;

    [SerializeField] Transform patrolAreaCenter;
    [SerializeField] Vector2 patrolAreaSize;
    [SerializeField] float visionRange = 15;
    [SerializeField] float visionAngle = 90;
    Vector3 lastTargetPosition;

    float searchTimer;
    [SerializeField]float serachWaitTime = 15;
    [SerializeField]float searchRadius = 30;

    public Transform[] points;
    [SerializeField] int pointDestination = 0; 
    [SerializeField] float rangeAttack = 5;

    private bool repeat = true;

    // Start is called before the first frame update
    void Awake()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        enemyAgent.autoBraking = false;
        currentState = State.Patrolling;
        enemyAgent.destination = points[pointDestination].position;
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                 Patrol();
            break;
            case State.Chasing:
                 Chase();
            break;
            case State.Searching:
                 Search();
            break;
            
            case State.Waiting:
                Wait();

            break;
             case State.Attackin:
                Attack();
            break;

        }
    }

  

    void Patrol()
    {
        if(OnRange())
        {
            currentState = State.Chasing;
        }

        if(enemyAgent.remainingDistance < 0.5f)
        {
            currentState = State.Waiting;
            //SetRandomPoint();
        }
    }

    void Search()
    {
        if(OnRange() == true)
        {
            searchTimer = 0;
            currentState = State.Chasing;
        }
        searchTimer += Time.deltaTime;

        if(searchTimer < serachWaitTime)
        {
            if(enemyAgent.remainingDistance < 0.5f)
            {
                 Debug.Log("Buscando punto aleatorio");

                 Vector3 randomSearchPoint = lastTargetPosition + Random.insideUnitSphere * searchRadius;
                randomSearchPoint.y = lastTargetPosition.y;
                enemyAgent.destination = randomSearchPoint;
            }
          
        }

        else
        {
            currentState = State.Patrolling;

        }
    }

    void Chase()
    {
        enemyAgent.destination = playerTransform.position;

        if(OnRange() == false)
        {
            currentState = State.Searching;
        }
        if(OnRangeAttack() == true)
        {
            currentState = State.Attackin;
            
        }

    }

    /*void SetRandomPoint()
    {
        float randomX = Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2);
        float randomZ = Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2);
        Vector3 randomPoint = new Vector3(randomX, 0f, randomZ) + patrolAreaCenter.position;

        enemyAgent.destination = randomPoint;
    }*/

    bool OnRange()
    {
      
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if(distanceToPlayer <= visionRange && angleToPlayer < visionAngle * 0.5f)
        {
            if(playerTransform.position == lastTargetPosition)
            {
                return true;
            }
            
            RaycastHit hit;
            if(Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
            {
                if(hit.collider.CompareTag("Player"))
                {
                    lastTargetPosition = playerTransform.position;

                    return true;
                }
            }

            return false;
           
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(patrolAreaCenter.position, new Vector3(patrolAreaSize.x, 0, patrolAreaSize.y));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.green;
        Vector3  fovLine1 = Quaternion.AngleAxis(visionAngle * 0.5f, transform.up) * transform.forward * visionRange;

        Vector3 fovLine2 = Quaternion.AngleAxis(-visionAngle * 0.5f, transform.up) * transform.forward * visionRange;

        Gizmos.DrawLine(transform.position, transform.position + fovLine1); 

        Gizmos.DrawLine(transform.position, transform.position + fovLine2); 
    }

    void GoPointNext()
    {
        if(points.Length == 0)
        {
            return;
        }

        pointDestination = (pointDestination + 1) % points.Length;
        enemyAgent.destination = points[pointDestination].position;
    }

    void Wait()
    {
        if (repeat == true)
        {
            StartCoroutine ("Waiting");
        }
    }

     IEnumerator Waiting()
    {
        repeat = false;
        yield return new WaitForSeconds (5);
        GoPointNext();
        currentState = State.Patrolling;
        repeat = true;
    }

    void Attack()
    {
       
        Debug.Log("Attack");
        currentState = State.Chasing;
    }

      bool OnRangeAttack()
    {
        

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if(distanceToPlayer <= visionRange && angleToPlayer< visionAngle* 0.2f)
        {
            

            if(playerTransform.position == lastTargetPosition)
            {
                return true;
            }

            RaycastHit hit;
            if(Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
            {
                if(hit.collider.CompareTag("Player"))
                {
                    lastTargetPosition = playerTransform.position;

                    return true;
                }
            }
            return false;
        }

        return false;
    }


 

    
}
