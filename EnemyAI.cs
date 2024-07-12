using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAiTutorial : MonoBehaviour
{

    public NavMeshAgent agent;

    public Transform player;
    public Transform spawnPoint;

    public LayerMask whatIsGround, whatIsPlayer;


    public float health;
    
    public GameObject ragdollPrefab;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public bool isRagdollCreated;
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    public float damage;

    public float closeR;

    private Animator animator;

    public AudioClip shootSound1;
    public AudioClip shootSound; 
    private AudioSource audioSource; 

    public AudioSource shootingAudioSource;
    float maxDistance = 20.0f; 

    public bool conditionMet = false;

     private float timeSinceLastSound = 0f;
    private float soundInterval = 0.48f;

    public float startAttack, endAttack, startWalk, endWalk, startWalk1, endWalk1, startRun, endRun, startRun1, endRun1;

    public bool WalkPlayed, WalkPlayed1, RunPlayed, RunPlayed1;

    bool particleCheck = true;
    public string playerPos;
    
    private bool isSoundPlaying = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag(playerTag).transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Расчёт момента времени анимации
    private void StateTimer(AnimatorStateInfo stateInfo)
    {
        currentTime = stateInfo.normalizedTime * stateInfo.length;

        if (stateInfo.loop)
        {
            currentTime %= stateInfo.length;
        }
    }

    // Срабатывание звуков шагов 
    private void TwoStepSound(
        float startFirstStep, 
        float endFirstStep, 
        float startSecondStep, 
        float endSecondStep
    )
    {
        if (currentTime >= startFirstStep && currentTime <= endFirstStep && firstStepPlayed) 
        {          
            audioSource.PlayOneShot(stepSound);  
            firstStepPlayed = false;
        }

        else if (!(currentTime >= startRun && currentTime <= endRun))
        {
            firstStepPlayed = true;
        }

        else if (currentTime >= startSecondStep && currentTime <= endSecondStep && secondStepPlayed) 
        {          
            audioSource.PlayOneShot(stepSound);  
            secondStepPlayed = false;               
        }

        else if (!(currentTime >= startSecondStep && currentTime <= endSecondStep))
        {
            secondStepPlayed = true;
        }
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        clipName = currentClipInfo[0].clip.name;

        // Вызов срабатывания звуков в определённые моменты анимации
        switch (clipName)
        {
            case "attack":

                StateTimer(stateInfo);

                if (currentTime >= startAttackTime && currentTime <= endAttackTime)
                {          
                    Invoke("AttackPlayer", 0f);   
                }

                break;

            case "run":
                
                StateTimer(stateInfo);
                TwoStepSound(
                    startRunFirstStepTime, 
                    endRunFirstStepTime, 
                    startRunSecondStepTime, 
                    endRunSecondStepTime
                    );
                break;

            case "walk":

                StateTimer(stateInfo);
                TwoStepSound(
                    startWalkFirstStepTime, 
                    endWalkFirstStepTime, 
                    startWalkSecondStepTime, 
                    endWalkSecondStepTime
                    );
                break;

            default:
                break;
        }

        // if (stateInfo.IsName("attack"))
        // {
        //     float currentTimeAttack = stateInfo.normalizedTime * stateInfo.length;

        //     if (stateInfo.loop)
        //     {
        //         currentTime %= stateInfo.length;
        //     }

        //     if (currentTimeAttack >= startAttackTime && currentTimeAttack <= endAttackTime) 
        //     {          
        //         Invoke("AttackPlayer", 0f);   
        //     }
        // }
        // else if (stateInfo.IsName("walk"))
        // {
        //     float currentTimeWalk = stateInfo.normalizedTime * stateInfo.length;

        //     if (stateInfo.loop)
        //     {
        //         currentTimeWalk %= stateInfo.length;
        //     }

        //     if (currentTimeWalk >= startWalkFirstStepTime && currentTimeWalk <= endWalkFirstStepTime) 
        //     {            
        //         if (firstStepWalkPlayed) 
        //         {
        //             audioSource.PlayOneShot(stepSound);  
        //             firstStepWalkPlayed = false;
        //         }
        //     }
        //     else 
        //     {
        //         firstStepWalkPlayed = true;
        //     }

        //     if (currentTimeWalk >= startWalkSecondStepTime && currentTimeWalk <= endWalkSecondStepTime) 
        //     {            
        //         if (secondStepWalkPlayed) 
        //         {
        //             audioSource.PlayOneShot(stepSound);  
        //             secondStepWalkPlayed = false;
        //         }               
        //     }
        //     else 
        //     {
        //         secondStepWalkPlayed = true;
        //     }
        // }
        // else if (stateInfo.IsName("run"))
        // {
        //     float currentTimeRun = stateInfo.normalizedTime * stateInfo.length;

        //     if (stateInfo.loop)
        //     {
        //         currentTimeRun %= stateInfo.length;
        //     }

        //     if ((currentTimeRun >= startRunFirstStepTime && currentTimeRun <= endRunFirstStepTime)) 
        //     {          
        //         if (firstStepRunPlayed) 
        //         {
        //             audioSource.PlayOneShot(stepSound);  
        //             firstStepRunPlayed = false;
        //         }               
        //     }
        //     else 
        //     {
        //         firstStepRunPlayed = true;
        //     }

        //     if ((currentTimeRun >= startRunFSecondStepTime && currentTimeRun <= endRunSecondStepTime)) 
        //     {          
        //         if (secondStepRunPlayed) 
        //         {
        //             audioSource.PlayOneShot(stepSound);  
        //             secondStepRunPlayed = false;
        //         }
        //     }
        //     else 
        //     {
        //         secondStepRunPlayed= true;
        //     }
        // }

        //Изменение громкости звуков, взависимости от расстояния до игрока
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        volume = 1.0f - Mathf.Clamp01(distanceToPlayer / maxDistance);
        audioSource.volume = volume;

        // Срабатывание функций и их отмена, взависимости от того, в какой зоне врага находится игрок
        if (!playerInSightRange && !playerInAttackRange)
        {
            animator.SetBool("run", false);
            animator.SetBool("walk", true);

            CancelInvoke(nameof(AttackPlayer));
            CancelInvoke(nameof(ResetAttack));

            alreadyAttacked = false;
            
            Patroling();  
        }  

        else if (playerInSightRange && !playerInAttackRange)
        {
            animator.SetBool("run", true);
            animator.SetBool("attack", false);

            CancelInvoke(nameof(AttackPlayer));
            CancelInvoke(nameof(ResetAttack));

            alreadyAttacked = false;
 
            ChasePlayer(); 
        }

        else if (playerInAttackRange && playerInSightRange) 
        {     
            animator.SetBool("attack", true);

            agent.SetDestination(transform.position);
            playerPosition = new Vector3(
                player.position.x, 
                transform.position.y, 
                player.position.z
                );

            transform.LookAt(playerPosition);
        }
    }

    // Патрулирование
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);
            agent.speed = 0.5f;  
            
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    // Установление случайной точки для патрулирования
    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(
            transform.position.x + randomX, 
            transform.position.y, 
            transform.position.z + randomZ
        );

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    // Погоня за игроком
    private void ChasePlayer()
    {
        agent.speed = 2.5f;
        agent.SetDestination(transform.position);

        Vector3 playerPosition = new Vector3(
            player.position.x, 
            transform.position.y, 
            player.position.z);

        transform.LookAt(playerPosition);
        agent.SetDestination(player.position);
    }

    // Атака игрока
    private void AttackPlayer()
    {
        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(
                projectile, 
                spawnPoint.position, 
                Quaternion.identity
            ).GetComponent<Rigidbody>();

            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            audioSource.PlayOneShot(attackSound);
            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    // Повтор атаки
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    
    // Отрисовка зон атаки и видимости
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    // Получение урона врагом
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet") && !isRagdollCreated)
        {
            if (health <= minHealth) 
            {
                Instantiate(
                    ragdollPrefab, 
                    transform.position, 
                    transform.rotation);
            
                isRagdollCreated = true;
                Destroy(gameObject);
            } 
            else
            {
                health -= getDamage;
            }   
        }
    }
}