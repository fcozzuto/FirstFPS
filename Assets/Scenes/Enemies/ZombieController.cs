using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    // Zombie being shot variables
    public Color newColor = Color.red; // The new color to change to
    private float colorChangeDuration = 0.5f; // Duration for the color change
    SkinnedMeshRenderer[] skinnedMeshRenderers;

    // Animation variables
    Animator zombieAnimator;

    // AI Navigation variables
    public Transform[] patrolPoints;  // Array of patrol points
    //public float patrolRadius = 5f; // Patrol point radius
    public float patrolAreaSize = 20f;
    public float speed = 3.5f;        // Patrol speed
    public float chaseSpeed;          // Chase speed
    public float waitTime = 2f;       // Time to wait at each point
    public float detectionRange = 20f; // Range to detect the player
    private bool isOnHighAlert;
    private int detectionBoostAllowance;
    public Transform playerTransform;   // Reference to the player

    private NavMeshAgent agent;
    private int currentPointIndex = 0;
    private bool waiting = false;
    public float rotationSpeed = 10f;   // Speed of rotation

    // Health logic variables
    public float attackDamage = 10f;  // Amount of damage to deal
    public float attackDelay = 1f;     // Time in seconds between attacks
    private float nextAttackTime = 0f; // Time when the enemy can attack again

    public Health health;
    private bool isZombieDead;

    // Start is called before the first frame update
    void Start()
    {
        currentPointIndex = 0;
        isZombieDead = false;
        health = GetComponent<Health>();
        chaseSpeed = speed * 2f;
        isOnHighAlert = false;
        detectionBoostAllowance = 1;

        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        zombieAnimator = GetComponent<Animator>();
        
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        playerTransform = PlayerController.playerTransform;

        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
        else
        {
            GeneratePatrolPoints();
        }
    }

    // Update is called once per frame
    void Update()
    {       
        Patrol();
        //CheckForObstacles();
    }

    private void GeneratePatrolPoints()
    {
        patrolPoints = new Transform[4]; // Initialize the array
        Vector3 currentTransformPosition = transform.position;
        // Set the radius to half the distance of the patrol area
        float radius = patrolAreaSize / 2f;
        // Calculate the angles for the 4 points (0, 90, 180, 270 degrees)
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            // Convert degrees to radians
            float angle = i * 90 * Mathf.Deg2Rad;

            // Calculate the patrol point position
            float x = currentTransformPosition.x + radius * Mathf.Cos(angle);
            float z = currentTransformPosition.z + radius * Mathf.Sin(angle);

            // Create a new Transform for each patrol point
            Vector3 patrolPointPosition = new Vector3(x, currentTransformPosition.y, z);
            patrolPoints[i] = new GameObject("PatrolPoint_" + (i + 1)).transform;
            patrolPoints[i].position = patrolPointPosition;
        }
    }
    private void Patrol()
    {
        if (!waiting)
        {
            playerTransform = PlayerController.playerTransform;            
            // Check if the player is within detection range
            if (Vector3.Distance(transform.position, playerTransform.position) < detectionRange)
            {
                // Chase the player
                ChasePlayer();
                return;
            }
            
            // Check if the agent has reached the patrol point
            if (agent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAtPoint());
            }
            else
            {
                // Trigger movement animation
                zombieAnimator.SetBool("isMoving", true);
            }
        }
    }

    private void CheckForObstacles()
    {
        RaycastHit hit;
        // Raycast forward to check for obstacles
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f)) // Adjust distance as needed
        {
            if (hit.collider.GetComponent<NavMeshObstacle>() != null) // Check if the hit object is a NavMeshObstacle
            {
                Debug.Log("Zombie encountered a NavMeshObstacle. Changing path.");
                StopZombieMovement();                
                SetNewDestination(hit.point); // Pass the hit point to find the opposite direction
            }
        }
    }

    private void StopZombieMovement()
    {
        zombieAnimator.SetBool("isMoving", false);  // Trigger stop moving animation
        agent.velocity = Vector3.zero; // Stop the zombie
        agent.isStopped = true; // Optionally set isStopped to true
    }

    private void SetNewDestination(Vector3 hitPoint)
    {
        // Calculate the opposite direction
        Vector3 directionAwayFromObstacle = (transform.position - hitPoint).normalized;
        // Randomize the angle to move away at an angle
        float angleOffset = UnityEngine.Random.Range(-45f, 45f); // Adjust the angle range as needed
        Quaternion rotation = Quaternion.Euler(0, angleOffset, 0);
        Vector3 angledDirection = rotation * directionAwayFromObstacle;
        Vector3 newDestination = transform.position + angledDirection * 5f; // Adjust the distance as needed

        // Replace the current patrol point with the new destination
        patrolPoints[currentPointIndex].position = newDestination;

        // Set the new destination
        agent.SetDestination(newDestination);
        StartZombieMovement();
    }

    private void StartZombieMovement()
    {
        zombieAnimator.SetBool("isMoving", true);  // Trigger stop moving animation
        agent.isStopped = false; // Resume movement after setting the new destination
    }

    private void ChasePlayer()
    {
        if (isZombieDead)
        {
            return;
        }       
        agent.SetDestination(playerTransform.position);
        zombieAnimator.SetBool("isChasing", true);  // Trigger movement animation when chasing
        agent.speed = chaseSpeed;

        if (Vector3.Distance(transform.position, playerTransform.position) < 3f)
        {
            RemoveDetectionRangeBoost();
            zombieAnimator.SetBool("isChasing", false); // Trigger stop chasing animation
            zombieAnimator.SetBool("isNearPlayer", true); // Attack player
            agent.isStopped = true;  // Stop the agent

            AttackPlayer();
        }
        else
        {
            agent.SetDestination(playerTransform.position);
            zombieAnimator.SetBool("isNearPlayer", false); // Can't attack player
            agent.isStopped = false;  // Resume movement
        }
    }

    private void AttackPlayer()
    {
        // Make sure the zombie stays facing the player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        Health playerHealth = playerTransform.gameObject.GetComponent<Health>();
        // Check if it's time to attack
        if (Time.time >= nextAttackTime)
        {
            if (playerHealth != null)
            {
                playerTransform.gameObject.GetComponent<PlayerController>().TakeDamage(attackDamage); // Deal damage to the player
                nextAttackTime = Time.time + attackDelay; // Set the next attack time
            }
        }
    }

    private IEnumerator WaitAtPoint()
    {        
        if (isZombieDead)
        {
            yield break;
        }       
        zombieAnimator.SetBool("isMoving", false);  // Trigger stop moving animation
        zombieAnimator.SetBool("isChasing", false); // Trigger stop chasing animation
        agent.speed = speed;
        waiting = true;  // Set waiting state to true
        agent.isStopped = true;  // Stop the agent

        yield return new WaitForSeconds(waitTime);  // Wait for a specified time

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;  // Move to the next point
        agent.SetDestination(patrolPoints[currentPointIndex].position);  // Set the new destination
        agent.isStopped = false;  // Resume movement
        waiting = false;  // Reset waiting state
    }

    public void GotHit(float damage)
    {    
        float healthRemaining = health.GetCurrentHealth();

        if (healthRemaining > 0)
        {
            if (health.GetCurrentHealth() == 100f)
            {
                health.ShowHealthBar();
            }
            if(this.gameObject.name.StartsWith("Zombie"))
            {
                health.TakeDamage(damage);
            }
            else if(this.gameObject.name.StartsWith("BabyZombie"))
            {
                health.TakeDamage(damage*3f);
            }
            else if(this.gameObject.name.StartsWith("MegaZombie"))
            {
                health.TakeDamage(damage/2);
            }
            else
            {
                Debug.Log("No target found!");
                return;
            }
            CheckIfZombieDies();
        }

        ApplyDetectionRangeBoost();

        if (skinnedMeshRenderers.Length > 0)
        {
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                StartCoroutine(ChangeColorTemporarily(skinnedMeshRenderer, newColor, colorChangeDuration));
            }
        }
        else
        {
            Debug.LogError("No SkinnedMeshRenderer found in this GameObject or its children!");
        }
    }

    private void CheckIfZombieDies()
    {
        float healthRemaining = health.GetCurrentHealth();
        if (healthRemaining <= 0)
        {
            //Debug.Log("Time to die, zombie!");
            isZombieDead = true;
            health.Die();
            HandleZombieDeath();
        }
    }

    private void HandleZombieDeath()
    {
        agent.SetDestination(transform.position); // Change position to where the zombie stands, to make sure it doesn't finish its movement after dying        
        zombieAnimator.SetBool("isMoving", false);  // Trigger stop moving animation
        zombieAnimator.SetBool("isChasing", false); // Trigger stop chasing animation
        zombieAnimator.SetTrigger("isDead");       

        // Start coroutine to wait for the animation to finish
        StartCoroutine(WaitAndDestroy(2f));
    }

    private IEnumerator WaitAndDestroy(float duration)
    {
        yield return new WaitForSeconds(duration);  // Wait for a specified time

        ZombieController.Destroy(this.gameObject);
    }

    private void ApplyDetectionRangeBoost()
    {
        if (detectionBoostAllowance > 0)
        {
            detectionBoostAllowance--;
            detectionRange *= 2f; // Boost detection range
            isOnHighAlert = true;
        }
    }

    private void RemoveDetectionRangeBoost()
    {
        if (isOnHighAlert)
        {
            if (detectionBoostAllowance == 0)
            {
                detectionBoostAllowance++;
                detectionRange /= 2f; // Restore detection range
                isOnHighAlert = false;
            }
        }
    }

    private IEnumerator ChangeColorTemporarily(SkinnedMeshRenderer renderer, Color newColor, float duration)
    {
        // Store the original color
        Color originalColor = renderer.material.GetColor("_Color");
        //Debug.Log("Current color before changing: " + originalColor.ToString());
        
        // Change to new color
        foreach (Material material in renderer.sharedMaterials)
        {
            material.SetColor("_Color", newColor);
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(colorChangeDuration);

        // Change back to original color
        foreach (Material material in renderer.sharedMaterials)
        {
            material.SetColor("_Color", originalColor);
        }
    }
}
