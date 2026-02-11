using UnityEngine;
using System.Collections;

public class Minion : MonoBehaviour
{
    public float moveSpeed = 3f;          // Speed at which the Minion moves
    public int maxHealth = 20;           // Health points for the Minion
    private int currentHealth;            // Current health of the Minion
    private Animator animator;

    // Behavior
    public float detectionRange = 10f;    // Range to detect the Sorcerer, Barbarian, or Rogue
    private bool isAlerted = false;       // Is the Minion alerted to the target?
    private bool isAttacking = false;     // Is the Minion attacking?
    private bool isDead = false;          // Is the Minion dead?
    private bool isIdle = true;           // Is the Minion idle?

    private Transform sorcerer;           // Reference to the Sorcerer (if any)
    private Transform barbarian;          // Reference to the Barbarian (if any)
    private Transform rogue;              // Reference to the Rogue (if any)
    private Transform clone;              // Reference to the Clone (if any)

    // Attack settings
    public float attackRange = 2f;        // Range at which the Minion can attack
    public int attackDamage = 5;          // Damage dealt by the Minion
    public float attackDelay = 1.5f;      // Delay between attacks in seconds

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
        if (isDead) return;  // If the Minion is dead, do nothing

        // Check if the clone is within detection range (prioritize clone over Sorcerer, Barbarian, or Rogue)
        if (clone == null)
        {
            clone = GameObject.FindGameObjectWithTag("Clone")?.transform; // Find the clone if it exists
        }

        // Determine the target
        Transform target = null;

        // If the clone is within detection range, prioritize the clone
        if (clone != null && Vector3.Distance(transform.position, clone.position) <= detectionRange)
        {
            target = clone;  // Prioritize the clone
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Minion is alerted to the Clone!");
        }
        // Otherwise, check if the Sorcerer is within range
        else if (sorcerer != null && Vector3.Distance(transform.position, sorcerer.position) <= detectionRange)
        {
            target = sorcerer;  // Otherwise, target the Sorcerer
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Minion is alerted to the Sorcerer!");
        }
        // Otherwise, check if the Barbarian is within range
        else if (barbarian != null && Vector3.Distance(transform.position, barbarian.position) <= detectionRange)
        {
            target = barbarian;  // Otherwise, target the Barbarian
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Minion is alerted to the Barbarian!");
        }
        // Otherwise, check if the Rogue is within range
        else if (rogue != null && Vector3.Distance(transform.position, rogue.position) <= detectionRange)
        {
            target = rogue;  // Otherwise, target the Rogue
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
            Debug.Log("Minion is alerted to the Rogue!");
        }
        else
        {
            isAlerted = false;
            isIdle = true;  // No target, Minion goes idle
            animator.SetBool("IsAlerted", false);  // Stop alert animation
        }

        // If the Minion is alerted and there's a target, move towards it
        if (isAlerted && target != null && !isAttacking)
        {
            MoveTowardsTarget(target);
        }

        // If the Minion is idle, it should stay in place
        if (isIdle)
        {
            animator.SetBool("IsIdle", true); // Assuming you have an "IsIdle" animation state
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
                Debug.Log("Minion attacked the Barbarian for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Sorcerer"))  // Check if it's the Sorcerer
            {
                target.GetComponent<SorcererController>().TakeDamage(attackDamage);  // Apply damage to Sorcerer
                Debug.Log("Minion attacked the Sorcerer for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Rogue"))  // Check if it's the Rogue
            {
                target.GetComponent<RogueController>().TakeDamage(attackDamage);  // Apply damage to Rogue
                Debug.Log("Minion attacked the Rogue for " + attackDamage + " damage.");
            }
            else if (target.CompareTag("Clone"))
            {
                // Handle the Clone interaction here if needed
                Debug.Log("Minion attacked the Clone!");
            }
        }

        // Wait for the delay before allowing another attack
        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
    }

    // Take damage
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Damaged");
        Debug.Log("Minion took " + damage + " damage.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 90, 200, 20), "Minion HP: " + currentHealth);
    }

    // Minion dies
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Debug.Log("Minion died.");

        // Notify the Sorcerer, Barbarian, or Rogue to gain XP
        if (sorcerer != null)
        {
            sorcerer.GetComponent<SorcererController>().GainXP(10);  // Minions give 10 XP to the Sorcerer
        }
        else if (barbarian != null)
        {
            barbarian.GetComponent<BarbarianController>().GainXP(10);  // Minions give 10 XP to the Barbarian
        }
        else if (rogue != null)
        {
            rogue.GetComponent<RogueController>().GainXP(10);  // Minions give 10 XP to the Rogue
        }

        Destroy(gameObject, 2f);  // Delay to allow death animation
    }
}