using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MinionAI : MonoBehaviour
{
    public enum MinionState { Idle, Walking, Running, Punching, GettingDamaged, Stunned, Dying }
    public MinionState currentState;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform wanderer;
    private float detectionRange = 10f;
    private float attackRange = 2f;
    private float followRange = 15f;
    private int damageAmount = 5;
    private float health = 50f;
    private bool isAlerted = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        wanderer = GameObject.FindWithTag("Wanderer").transform;

        // Set the agent's properties for smooth behavior
        agent.updateRotation = true; // Rotate the agent automatically based on the path
        agent.updatePosition = true; // Allow the agent to move
        agent.speed = 3.5f;  // Speed for running
        agent.acceleration = 8f;  // Acceleration
        agent.stoppingDistance = 1f;  // How close the agent gets before stopping
    }

    private void Update()
    {
        float distanceToWanderer = Vector3.Distance(transform.position, wanderer.position);

        Debug.Log("Distance to Wanderer: " + distanceToWanderer);

        if (health <= 0f)
        {
            Die();
            return;
        }

        // Alert and start chasing if within detection range
        if (distanceToWanderer <= detectionRange && !isAlerted)
        {
            Alert();
            Debug.Log("Minion alerted!");
        }

        // If alerted, move towards the Wanderer
        if (isAlerted)
        {
            if (distanceToWanderer <= attackRange)
            {
                currentState = MinionState.Punching;
                animator.SetTrigger("Punch");
                PerformAttack();
            }
            else
            {
                currentState = MinionState.Running;
                animator.SetTrigger("Run");
                agent.SetDestination(wanderer.position); // Move towards the Wanderer
            }
        }
        // If the Wanderer is out of the follow range, go back to idle
        else if (distanceToWanderer > followRange)
        {
            currentState = MinionState.Idle;
            animator.SetTrigger("Idle");
            agent.ResetPath(); // Stop following the Wanderer if out of range
        }
        else
        {
            Patrol(); // Patrol if not alerted
        }

        UpdateMinionState();
    }

    private void Patrol()
    {
        if (currentState != MinionState.Punching)
        {
            currentState = MinionState.Walking;
            animator.SetTrigger("Walk");
            // Optionally, set a random destination for patrol if needed
        }
    }

    private void PerformAttack()
    {
        // Attack logic: Apply damage to the Wanderer
        if (wanderer != null)
        {
            BarbarianController barbarianScript = wanderer.GetComponent<BarbarianController>();
            SorcererController sorcererScript = wanderer.GetComponent<SorcererController>();
            RogueController rogueScript = wanderer.GetComponent<RogueController>();
            if (barbarianScript != null)
            {
                barbarianScript.TakeDamage(damageAmount); // Call TakeDamage from Barbarian script
            }

            if (sorcererScript != null)
            {
                sorcererScript.TakeDamage(damageAmount); // Call TakeDamage from Sorcerer script
            }

            if (rogueScript != null)
            {
                rogueScript.TakeDamage(damageAmount); // Call TakeDamage from Rogue script
            }
        }
    }

    private void UpdateMinionState()
    {
        if (agent.isOnNavMesh)
        {
            switch (currentState)
            {
                case MinionState.Idle:
                    agent.isStopped = true; // Stop moving when idle
                    break;
                case MinionState.Walking:
                    agent.isStopped = false; // Move when walking
                    break;
                case MinionState.Running:
                    agent.isStopped = false; // Move faster when running
                    break;
                case MinionState.Punching:
                    agent.isStopped = true; // Stop moving when attacking
                    break;
                case MinionState.GettingDamaged:
                    // Handle damage animation (could trigger a reaction)
                    break;
                case MinionState.Stunned:
                    // Handle stunned animation
                    break;
                case MinionState.Dying:
                    // Handle dying animation and behavior
                    break;
            }
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not on a valid NavMesh.");
        }
    }

    // Method for taking damage
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health > 0)
        {
            currentState = MinionState.GettingDamaged;
            animator.SetTrigger("GetDamaged");
        }
        else
        {
            Die();
        }
    }

    // Method for dying
    private void Die()
    {
        currentState = MinionState.Dying;
        animator.SetTrigger("Die");
        agent.isStopped = true; // Stop the agent when dead
        Destroy(gameObject, 3f); // Destroy after death animation
    }

    // Alert method to make the minion start chasing the Wanderer
    public void Alert()
    {
        isAlerted = true;
        currentState = MinionState.Running;
        animator.SetTrigger("Run");
        agent.SetDestination(wanderer.position); // Start moving towards the Wanderer
    }
}
