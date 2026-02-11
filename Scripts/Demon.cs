using UnityEngine;
using System.Collections;

public class Demon : MonoBehaviour
{
    public float moveSpeed = 3f;          // Speed at which the Demon moves
    public int maxHealth = 50;            // Health points for the Demon
    private int currentHealth;            // Current health of the Demon
    private Animator animator;

    // Behavior
    public float detectionRange = 10f;    // Range to detect the Wanderer
    private bool isAlerted = false;       // Is the Demon alerted to the target?
    private bool isAttacking = false;     // Is the Demon attacking?
    private bool isDead = false;          // Is the Demon dead?
    private bool isPatrolling = true;     // Is the Demon patrolling its camp?

    private Transform sorcerer;           // Reference to the Sorcerer (if any)
    private Transform barbarian;          // Reference to the Barbarian (if any)
    private Transform rogue;              // Reference to the Rogue (if any)
    private Transform clone;              // Reference to the Clone (if any)
    private Transform target;             // Reference to the current target (Sorcerer, Barbarian, Rogue, or Clone)

    // Attack settings
    public float attackRange = 2f;        // Range at which the Demon can attack
    public int attackDamage = 10;         // Damage dealt by the Demon
    public float attackDelay = 1.5f;      // Delay between attacks in seconds

    // Demon abilities
    public float swordSwingDelay = 1f;    // Delay between sword swings
    public float explosiveThrowDelay = 2f;  // Delay between explosive throws
    public GameObject explosivePrefab;    // Explosive object to throw

    private int attackCount = 0;          // Keeps track of how many sword swings have been made

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        sorcerer = GameObject.FindGameObjectWithTag("Sorcerer")?.transform;  // Get the Sorcerer by its tag
        barbarian = GameObject.FindGameObjectWithTag("Barbarian")?.transform; // Get the Barbarian by its tag
        rogue = GameObject.FindGameObjectWithTag("Rogue")?.transform; // Get the Rogue by its tag
    }

    void Update()
    {
        if (isDead) return;  // If the Demon is dead, do nothing

        // Check if the clone is within detection range (prioritize clone over Sorcerer, Barbarian, or Rogue)
        if (clone == null)
        {
            clone = GameObject.FindGameObjectWithTag("Clone")?.transform; // Find the clone if it exists
        }

        // Determine the target
        target = null;

        // If the clone is within detection range, prioritize the clone
        if (clone != null && Vector3.Distance(transform.position, clone.position) <= detectionRange)
        {
            target = clone;  // Prioritize the clone
            isAlerted = true;
            isPatrolling = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Demon is alerted to the Clone!");
        }
        // Otherwise, check if the Sorcerer is within range
        else if (sorcerer != null && Vector3.Distance(transform.position, sorcerer.position) <= detectionRange)
        {
            target = sorcerer;  // Otherwise, target the Sorcerer
            isAlerted = true;
            isPatrolling = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Demon is alerted to the Sorcerer!");
        }
        // Otherwise, check if the Barbarian is within range
        else if (barbarian != null && Vector3.Distance(transform.position, barbarian.position) <= detectionRange)
        {
            target = barbarian;  // Otherwise, target the Barbarian
            isAlerted = true;
            isPatrolling = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Demon is alerted to the Barbarian!");
        }
        // Otherwise, check if the Rogue is within range
        else if (rogue != null && Vector3.Distance(transform.position, rogue.position) <= detectionRange)
        {
            target = rogue;  // Otherwise, target the Rogue
            isAlerted = true;
            isPatrolling = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Demon is alerted to the Rogue!");
        }

        if (isAlerted && !isAttacking)
        {
            if (attackCount < 2)
            {
                SwingSword();  // Attack twice with the sword
            }
            else
            {
                ThrowExplosive();  // Throw explosive after two sword swings
                attackCount = 0;  // Reset attack count after throwing explosive
            }
        }

        if (isDead) return;  // If the Demon is dead, do nothing

        else
        {
            isAlerted = false;
            isPatrolling = true;  // If no target is found, Demon patrols its camp
            animator.SetBool("IsAlerted", false);  // Stop alert animation
        }

        // If the Demon is alerted and there's a target, move towards it
        if (isAlerted && target != null && !isAttacking)
        {
            MoveTowardsTarget(target);
        }

        // If the Demon is patrolling, it should patrol its camp (define patrol behavior as needed)
        if (isPatrolling)
        {
            animator.SetBool("IsPatrolling", true);
        }
    }

    // Move towards the target (either the Sorcerer, Barbarian, Rogue, or Clone)
    void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Rotate the Minion to face the target (either the Sorcerer, Barbarian, Rogue, or Clone)
        if (Vector3.Distance(transform.position, target.position) > attackRange)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            // If the Minion is in range, attack the target
            StartCoroutine(AttackTarget(target));
        }
    }

    // Patrol behavior (can be expanded to make the Demon walk around its camp)
    void PatrolCamp()
    {
        // Implement patrolling logic (e.g., walking between points within the camp)
        // For simplicity, this just moves the Demon in a loop. Expand as needed.
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    // Coroutine to handle attack with delay
    private IEnumerator AttackTarget(Transform target)
    {
        if (isAttacking) yield break;  // Prevent multiple attacks before the delay is over

        isAttacking = true;
        animator.SetTrigger("Attack");  // Trigger attack animation

        // Apply damage to the target (either the Sorcerer, Barbarian, Rogue, or Clone)
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            if (target.CompareTag("Barbarian"))  // Check if it's the Barbarian
            {
                target.GetComponent<BarbarianController>().TakeDamage(attackDamage);  // Apply damage to Barbarian
                Debug.Log("Demon attacked the Barbarian for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Sorcerer"))  // Check if it's the Sorcerer
            {
                target.GetComponent<SorcererController>().TakeDamage(attackDamage);  // Apply damage to Sorcerer
                Debug.Log("Demon attacked the Sorcerer for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Rogue"))  // Check if it's the Rogue
            {
                target.GetComponent<RogueController>().TakeDamage(attackDamage);  // Apply damage to Rogue
                Debug.Log("Demon attacked the Rogue for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Clone"))
            {
                // Handle the Clone interaction here if needed
                Debug.Log("Demon attacked the Clone!");
            }
        }

        // Wait for the delay before allowing another attack
        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
    }

    void SwingSword()
    {
        if (isDead || isAttacking) return;  // Prevent swing if demon is dead or already attacking

        isAttacking = true;
        animator.SetTrigger("SwingSword");  // Trigger sword swing animation

        // Wait for the sword swing to finish before applying damage
        // Assuming a swing delay for timing the attack
        StartCoroutine(SwordSwingDelay());
    }

    private IEnumerator SwordSwingDelay()
    {
        // Wait for the sword swing animation to finish before applying damage
        yield return new WaitForSeconds(swordSwingDelay);

        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Apply damage to the target (Wanderer, Sorcerer, Barbarian, Rogue, or Clone)
            if (target.CompareTag("Barbarian"))
            {
                target.GetComponent<BarbarianController>().TakeDamage(attackDamage);
                Debug.Log("Demon swung sword at the Barbarian for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Sorcerer"))
            {
                target.GetComponent<SorcererController>().TakeDamage(attackDamage);
                Debug.Log("Demon swung sword at the Sorcerer for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Rogue"))
            {
                target.GetComponent<RogueController>().TakeDamage(attackDamage);
                Debug.Log("Demon swung sword at the Rogue for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Clone"))
            {
                // Handle the Clone interaction here if needed
                Debug.Log("Demon swung sword at the Clone!");
            }
        }

        isAttacking = false;  // Reset attacking status after attack
    }

    // Method to throw an explosive
    void ThrowExplosive()
    {
        if (explosivePrefab != null)
        {
            Instantiate(explosivePrefab, transform.position + transform.forward, Quaternion.identity);
            Debug.Log("Demon threw an explosive!");
        }
    }

    // Take damage method
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Method to handle the Demon's death
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Debug.Log("Demon has died.");
    }
}
