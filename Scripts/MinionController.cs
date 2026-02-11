using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MinionController : MonoBehaviour
{
    public float moveSpeed = 5f;   // Default movement speed of the enemy
    private float originalSpeed;   // Store original speed to reset later
    private bool isSlowed = false; // Whether the minion is slowed
    public int maxHealth = 20;     // Health points for the Minion
    private int currentHealth;     // Current health of the Minion
    private Animator animator;

    // Behavior
    public float detectionRange = 10f; // Range to detect the Sorcerer, Barbarian, or Rogue
    private bool isAlerted = false;    // Is the Minion alerted to the target?
    private bool isAttacking = false;  // Is the Minion attacking?
    private bool isDead = false;       // Is the Minion dead?
    private bool isIdle = true;        // Is the Minion idle?

    private Transform sorcerer;        // Reference to the Sorcerer (if any)
    private Transform barbarian;       // Reference to the Barbarian (if any)
    private Transform rogue;           // Reference to the Rogue (if any)
    private Transform clone;           // Reference to the Clone (if any)

    // Attack settings
    public float attackRange = 2f;      // Range at which the Minion can attack
    public int attackDamage = 5;        // Damage dealt by the Minion
    public float attackDelay = 1.5f;    // Delay between attacks in seconds

    // Slider UI for Minion HP
    public Slider healthSlider;            // Reference to the slider UI
    public Vector3 sliderOffset = new Vector3(0, 2f, 0); // Adjust the Y-axis for positioning the slider above the minion



    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        originalSpeed = moveSpeed; // Store the original speed when the game starts
        sorcerer = GameObject.FindGameObjectWithTag("Sorcerer")?.transform;  // Get the Sorcerer by its tag
        barbarian = GameObject.FindGameObjectWithTag("Barbarian")?.transform; // Get the Barbarian by its tag
        rogue = GameObject.FindGameObjectWithTag("Rogue")?.transform; // Get the Rogue by its tag

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        // Handle the Minion's AI behavior (detection and movement)
        HandleDetectionAndMovement();
        // Update Slider Position
        UpdateHealthSliderPosition();
    }

    void UpdateHealthSliderPosition()
    {
        if (healthSlider != null)
        {
            Vector3 worldPosition = transform.position + sliderOffset;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            healthSlider.transform.position = screenPosition;
        }
    }



    void HandleDetectionAndMovement()
    {
        originalSpeed = moveSpeed; // Ensure that original speed is always updated in case it's changed
        if (isDead) return;

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
            target = clone;
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);  // Trigger alert animation
        }
        // Otherwise, check if the Sorcerer is within range
        else if (sorcerer != null && Vector3.Distance(transform.position, sorcerer.position) <= detectionRange)
        {
            target = sorcerer;
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);
        }
        // Otherwise, check if the Barbarian is within range
        else if (barbarian != null && Vector3.Distance(transform.position, barbarian.position) <= detectionRange)
        {
            target = barbarian;
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);
        }
        // Otherwise, check if the Rogue is within range
        else if (rogue != null && Vector3.Distance(transform.position, rogue.position) <= detectionRange)
        {
            target = rogue;
            isAlerted = true;
            isIdle = false;
            animator.SetBool("IsAlerted", true);
        }
        else
        {
            isAlerted = false;
            isIdle = true;
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

    void MoveTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Rotate the Minion to face the target
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

    private IEnumerator AttackTarget(Transform target)
    {
        if (isAttacking) yield break;  // Prevent multiple attacks before the delay is over

        isAttacking = true;
        animator.SetTrigger("Attack");  // Trigger attack animation

        // Apply damage to the target (either the Sorcerer, Barbarian, Rogue, or Clone)
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            if (target.CompareTag("Barbarian"))
            {
                target.GetComponent<BarbarianController>().TakeDamage(attackDamage);
            }
            else if (target.CompareTag("Sorcerer"))
            {
                target.GetComponent<SorcererController>().TakeDamage(attackDamage);
            }
            else if (target.CompareTag("Rogue"))
            {
                target.GetComponent<RogueController>().TakeDamage(attackDamage);
            }
            else if (target.CompareTag("Clone"))
            {
                // Handle Clone damage here
            }
        }

        // Wait for the delay before allowing another attack
        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
    }

    // Apply slow effect
    public void ApplySlow(float duration)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            moveSpeed = originalSpeed / 4f; // Slow the minion to 25% of its original speed
            StartCoroutine(RemoveSlowAfterTime(duration));
        }
    }

    // Remove the slow effect after a duration
    IEnumerator RemoveSlowAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed; // Reset to original speed
        isSlowed = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Damaged");
        Debug.Log("Minion took " + damage + " damage.");

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            healthSlider.gameObject.SetActive(true); // Show the slider on taking damage
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Grant XP when the Minion dies
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        Debug.Log("Minion died.");

        // Award XP to the respective character
        if (sorcerer != null)
        {
            sorcerer.GetComponent<SorcererController>().GainXP(10); // Minions give 10 XP to the Sorcerer
        }
        else if (barbarian != null)
        {
            barbarian.GetComponent<BarbarianController>().GainXP(10); // Minions give 10 XP to the Barbarian
        }
        else if (rogue != null)
        {
            rogue.GetComponent<RogueController>().GainXP(10); // Minions give 10 XP to the Rogue
        }

        Destroy(gameObject, 2f);  // Delay to allow death animation
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 90, 200, 20), "Minion HP: " + currentHealth);
    }
}
