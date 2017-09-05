using UnityEngine;
using System.Collections;
using Scripts.Player;

public class EnemyAI : MonoBehaviour {

    // The nav mesh agent's speed when patrolling.
    public float patrolSpeed = 2f;
    // The nav mesh agent's speed when chasing.			
    public float chaseSpeed = 5f;
    // The amount of time to wait when the last sighting is reached.			
    public float chaseWaitTime = 5f;
    // The amount of time to wait when the patrol way point is reached.			
    public float patrolMinWaitTime = 1f;
    public float patrolMaxWaitTime = 5f;
    // An array of transforms for the patrol route.			
    public Transform[] patrolWayPoints;

    // Reference to the EnemySight script.
    EnemySight enemySight;
    // Reference to the nav mesh agent.				
    UnityEngine.AI.NavMeshAgent nav;

    // Reference to the PlayerHealth script.				
    PlayerHealth playerHealth;    
    // A timer for the chaseWaitTime.
    float chaseTimer;
    // A timer for the patrolWaitTime.				
    float patrolTimer;
    // A counter for the way point array.				
    int wayPointIndex;
    float patrolWaitTime;

    Vector3 resetPosition;

    void Awake() {
        // Setting up the references.
        enemySight = GetComponent<EnemySight>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        playerHealth = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<PlayerHealth>();
        resetPosition = new Vector3();
        nav.speed = chaseSpeed;
    }

    void Update() {
        if (playerHealth.currentHealth <= 0)
        {
            return;
        }

        // If the player is in shooting range we shoot him.
        if (enemySight.playerInShootingRange)
        {
            Shooting();
        }
        // If the player has been sighted or heard we investigate or chase.
        else if (enemySight.lastPlayerPosition != resetPosition)
        {
            Chasing();
        }
        // Otherwise patrol.
        else
        {
            Patrolling();
        }
    }

    void Shooting() {
        // Stop the enemy where it is.        
        nav.isStopped = true;
    }

    void Chasing() {
        nav.isStopped = false;

        // Either use the global alarm position or our own position for the player.
        Vector3 lastPlayerPosition = enemySight.lastPlayerPosition;

        // Create a vector from the enemy to the last known position of the player.
        Vector3 sightingDeltaPos = lastPlayerPosition - transform.position;

        // If the the last personal sighting of the player is not close...
        if (sightingDeltaPos.sqrMagnitude > 4f) {
            // ... set the destination for the NavMeshAgent to the last personal sighting of the player.
            nav.destination = lastPlayerPosition;
        }
        
        // If near the last personal sighting...
        if (nav.remainingDistance < nav.stoppingDistance) {
            // ... increment the timer.
            chaseTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (chaseTimer >= chaseWaitTime) {
                // ... reset the timer.
                enemySight.playerSeen = false;
                chaseTimer = 0f;
                enemySight.lastPlayerPosition = new Vector3();
            }
        }
        else {
            // If not near the last sighting personal sighting of the player, reset the timer.
            chaseTimer = 0f;
            enemySight.lastPlayerPosition = new Vector3();
        }
    }

    void Patrolling() {
        if (patrolWayPoints.Length == 0) {
            return;
        }

        nav.isStopped = false;
        
        // Set an appropriate speed for the NavMeshAgent.
        nav.speed = chaseSpeed;

        // If near the next waypoint or there is no destination...
        if (nav.destination == resetPosition || nav.remainingDistance < nav.stoppingDistance) {
            if (patrolTimer == 0) {
                patrolWaitTime = Random.Range(patrolMinWaitTime, patrolMaxWaitTime);
            }

            // ... increment the timer.
            patrolTimer += Time.deltaTime;

            // If the timer exceeds the wait time...
            if (patrolTimer >= patrolWaitTime) {
                // ... increment the wayPointIndex.
                if (wayPointIndex == patrolWayPoints.Length - 1) {
                    wayPointIndex = 0;
                }
                else {
                    wayPointIndex++;
                }

                // Reset the timer.
                patrolTimer = 0;
            }
        }
        else {
            // If not near a destination, reset the timer.
            patrolTimer = 0;
        }

        // Set the destination to the patrolWayPoint.
        nav.destination = patrolWayPoints[wayPointIndex].position;
    }
}
