using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RogueController : MonoBehaviour
{
    public float moveSpeed = 5f;            // Speed at which the Rogue moves
    private Vector3 targetPosition;         // The position the Rogue will move towards
    private bool isMoving = false;          // Whether the Rogue is currently moving
    private GameObject targetEnemy = null;

    private Animator animator;              // Animator for the Rogue (for animations)

    public int maxHealth = 100;             // Max health of the Rogue
    private int currentHealth;              // Current health of the Rogue

    // XP and Level System Variables
    public int currentLevel = 1;            // Starting level
    public int currentXP = 0;               // Current XP
    public int xpToNextLevel = 100;         // XP needed for the next level
    public int abilityPoints = 0;           // Ability points earned through leveling up
    public float maxHealthIncrease = 100;   // Health increment per level

    // Rogue-specific abilities
    private bool isArrowUnlocked = true;    // Arrow is unlocked by default
    private bool isSmokeBombUnlocked = false;
    private bool isDashUnlocked = false;
    private bool isShowerOfArrowsUnlocked = false;
    private bool isDead = false;

    // Arrow ability variables
    public GameObject arrowPrefab;          // Arrow prefab
    public float arrowSpeed = 10f;          // Speed of the arrow
    public float arrowCooldown = 1f;       // Cooldown for Arrow ability
    private bool canUseArrow = true;        // Tracks if Arrow ability is ready to use
    public float showerRadius = 5f;         // Radius for the Shower of Arrows
    public int arrowsCount = 6;             // Number of arrows to fall in the shower
    public float arrowFallDuration = 1f;    // Time it takes for arrows to fall
    public float showerCooldown = 10f;      // Cooldown for the Shower of Arrows ability
    private bool canUseShower = true;

    // Dash ability variables
    public float dashSpeedMultiplier = 2f; // Multiplier for speed during Dash
    public float dashDuration = 0.5f;      // Duration of the Dash ability
    public float dashCooldown = 5f;        // Cooldown time for Dash ability
    private bool canUseDash = true;        // Tracks if Dash is available
    private bool isInvincible = false;

    public float walkableRange = 15f;      // Max range the Rogue can dash to

    private bool isDashing = false;        // Track if Dash mode is active

    public Slider healthSlider;
    public Slider xpSlider;
    private int tpresscount = 0;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        // Initialize sliders
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        xpSlider.minValue = 0; // Min value of XP slider
        xpSlider.maxValue = 1; // Max value of XP slider is 1 (we'll adjust the value based on progress)
        UpdateXP(); // Update the XP slider based on current XP and the required XP for the next level
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }
        UpdateSliders();

        // Handle movement input (left-click to set target position)
        if (Input.GetMouseButtonDown(0) && !isDashing) // Left-click to set the target position for movement
        {
            SetTargetPosition();
        }

        if (Input.GetKeyDown(KeyCode.Q) && isDashUnlocked && canUseDash && !isDashing) // Press "Q" to activate Dash mode
        {
            EnterDashMode();
        }

        if (Input.GetKeyDown(KeyCode.E) && isShowerOfArrowsUnlocked && canUseShower) // Press "E" to use Shower of Arrows
        {
            StartCoroutine(ShowerOfArrows());
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && abilityPoints >= 1 && !isArrowUnlocked) // Unlock Arrow ability (if locked)
        {
            UnlockArrowAbility();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && abilityPoints >= 1 && !isSmokeBombUnlocked) // Unlock Smoke Bomb ability
        {
            UnlockSmokeBombAbility();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && abilityPoints >= 1 && !isDashUnlocked) // Unlock Dash ability
        {
            UnlockDashAbility();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (currentHealth + 20 >= maxHealth)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth += 20;
            }

        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentHealth - 20 <= 0)
            {
                Die();
            }
            else
            {
                currentHealth -= 20;
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            tpresscount++;
            if (tpresscount / 2 != 0)
            {
                isInvincible = true;
            }
            else
            {
                isInvincible = false;
            }

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            arrowCooldown = 0;
            showerCooldown = 0;
            dashCooldown = 0;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            isDashUnlocked = true;
            isShowerOfArrowsUnlocked = true;
          
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            abilityPoints++;
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            currentXP += 100;
        }

        if (isInvincible)
        {
            while (currentHealth != maxHealth)
            {
                currentHealth = maxHealth;
            }
        }

        if (isDashing)
        {
            if (Input.GetMouseButtonDown(1)) // Right-click to target the dash position
            {
                StartCoroutine(PerformDash());
            }
        }
        else
        {
            // Move the Rogue if it's in motion
            if (isMoving)
            {
                MoveCharacterToTarget();
                animator.SetBool("IsMoving", true);  // Trigger moving animation
            }
            else
            {
                animator.SetBool("IsMoving", false); // Stop moving animation when not moving
            }

            // Handle Arrow ability using right-click
            if (Input.GetMouseButtonDown(1) && isArrowUnlocked && canUseArrow) // Right-click (1)
            {
                StartCoroutine(UseArrow()); // Shoot the arrow towards the selected enemy
            }
        }
    }

    void UpdateXP()
    {
        // Calculate the XP slider value as a fraction (from 0 to 1) based on the current XP and required XP for the next level
        float xpFraction = (float)currentXP / (float)xpToNextLevel;

        // Update the XP slider value
        xpSlider.value = Mathf.Clamp01(xpFraction);  // Ensure it stays between 0 and 1
    }

    void UpdateSliders()
    {
        // Update health slider
        healthSlider.value = currentHealth;

        // Update XP slider
        UpdateXP();
    }

    // Set the target position where the user clicked
    void SetTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  // Create a ray from the camera to the mouse position
        RaycastHit hit;

        // If the ray hits a point on the ground
        if (Physics.Raycast(ray, out hit))
        {
            targetPosition = hit.point;  // Set the target position to the point where the ray hits
            isMoving = true;  // Start moving the Rogue towards the target position
        }
    }

    // Move the Rogue towards the target position
    void MoveCharacterToTarget()
    {
        Vector3 moveDirection = targetPosition - transform.position; // Calculate the direction to move
        moveDirection.y = 0f; // Keep the Y value the same (flat ground)

        // Move the character towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Rotate the character to face the direction it's moving in
        if (moveDirection.magnitude > 0.1f)  // If the Rogue is still moving
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        // Stop moving once the target position is reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    // Use Arrow ability (shoot an arrow at the selected enemy or position)
    IEnumerator UseArrow()
    {
        if (!canUseArrow) yield break;

        canUseArrow = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                animator.SetTrigger("ShootArrow");

                targetEnemy = hit.collider.gameObject; // Store the targeted enemy

                // Call FaceEnemy to make the Rogue face the enemy
                FaceEnemy(hit.collider.transform.position);

                // Wait for 0.5 seconds before spawning the arrow
                yield return new WaitForSeconds(0.5f);

                // Instantiate the arrow at the Rogue's position
                GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

                // Pass the target enemy to the arrow
                arrow.GetComponent<ArrowCollision>().SetTargetEnemy(targetEnemy);

                // Set the direction for the arrow to fly toward the enemy's position
                Vector3 direction = (hit.collider.transform.position - transform.position).normalized;

                // Get the Rigidbody of the arrow and set its velocity
                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = direction * arrowSpeed;
                }

                Destroy(arrow, 5f); // Destroy the arrow after 5 seconds
            }
        }

        // Wait for cooldown before allowing Arrow to be used again
        yield return new WaitForSeconds(arrowCooldown);

        canUseArrow = true;
    }

    // Unlock Arrow ability
    void UnlockArrowAbility()
    {
        isArrowUnlocked = true;
        abilityPoints--;  // Deduct one ability point
        Debug.Log("Arrow ability unlocked!");
    }

    // Unlock Smoke Bomb ability
    void UnlockSmokeBombAbility()
    {
        isSmokeBombUnlocked = true;
        abilityPoints--;  // Deduct one ability point
        Debug.Log("Smoke Bomb ability unlocked!");
    }

    // Unlock Dash ability
    void UnlockDashAbility()
    {
        isDashUnlocked = true;
        abilityPoints--;  // Deduct one ability point
        Debug.Log("Dash ability unlocked!");
    }

    // Enter Dash mode
    void EnterDashMode()
    {
        isDashing = true;
        animator.SetTrigger("EnterDashMode");  // Trigger any animation for entering Dash mode
        Debug.Log("Dash Mode Activated. Right-click to dash to a position.");
    }

    // Perform Dash
    IEnumerator PerformDash()
    {
        // Prevent movement during Dash
        isMoving = false;
        canUseDash = false;

        // Determine the target position for the Dash
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the target position is within the walkable range
            if (Vector3.Distance(transform.position, hit.point) <= walkableRange)
            {
                // Trigger Dash animation
                animator.SetTrigger("Dash");

                // Temporarily double the Rogue's speed
                float originalSpeed = moveSpeed;
                moveSpeed *= dashSpeedMultiplier;

                Vector3 dashTarget = hit.point;

                // Move the Rogue towards the target position
                float elapsedTime = 0f;
                while (elapsedTime < dashDuration)
                {
                    // Update rotation to always face the dash target during the dash
                    Vector3 moveDirection = dashTarget - transform.position;
                    moveDirection.y = 0f; // Keep the Y value the same (flat ground)
                    transform.rotation = Quaternion.LookRotation(moveDirection); // Make the Rogue face the dash direction

                    // Move the Rogue towards the target position
                    transform.position = Vector3.MoveTowards(transform.position, dashTarget, moveSpeed * Time.deltaTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                // Reset speed after Dash
                moveSpeed = originalSpeed;

                // Exit Dash mode after the dash is complete
                isDashing = false; // Now exit Dash mode immediately after the dash is done
            }
            else
            {
                Debug.Log("Target position is out of range for Dash!");
            }
        }

        // Start Dash cooldown after completing the dash
        yield return StartCoroutine(DashCooldown());
    }


    // Dash cooldown
    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canUseDash = true;
        Debug.Log("Dash is ready!");
    }

    IEnumerator ShowerOfArrows()
    {
        canUseShower = false;

        // Get the point where the player clicked (target position for the Shower)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetPosition = hit.point;

            // Create multiple arrows falling in a random spread within the shower radius
            for (int i = 0; i < arrowsCount; i++)
            {
                Vector3 randomOffset = Random.insideUnitCircle * showerRadius; // Random position within the radius
                Vector3 arrowPosition = new Vector3(targetPosition.x + randomOffset.x, targetPosition.y + 10f, targetPosition.z + randomOffset.y); // Start above the target
                GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);

                // Apply gravity or downward motion to simulate falling
                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Use a higher fall speed, e.g., 20, to make them fall faster
                    rb.velocity = Vector3.down * 20f; // Adjust this value to make it faster or slower
                }

                // Wait before spawning the next arrow
                yield return new WaitForSeconds(0.1f);
            }

            // Apply damage and slow effect to enemies in the range
            Collider[] enemies = Physics.OverlapSphere(targetPosition, showerRadius);
            foreach (var enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    // Apply damage and slow to the enemy
                    enemy.GetComponent<MinionController>().TakeDamage(10); // Example damage value
                    enemy.GetComponent<MinionController>().ApplySlow(3f); // Slow effect duration
                }
            }
        }

        // Start cooldown for the ability
        yield return new WaitForSeconds(showerCooldown);
        canUseShower = true;
    }

    // Face the enemy target
    void FaceEnemy(Vector3 enemyPosition)
    {
        Vector3 directionToEnemy = enemyPosition - transform.position;
        directionToEnemy.y = 0; // Ignore vertical rotation
        transform.rotation = Quaternion.LookRotation(directionToEnemy);
    }

    // Take damage method (called from other classes when the Rogue is attacked)
    public void TakeDamage(int damage)
    {
        if (isDead || currentHealth <= 0) return; // If the Rogue is already dead, do nothing

        currentHealth -= damage;  // Reduce health by the damage amount
        animator.SetTrigger("Damaged");  // Trigger the "Damaged" animation

        Debug.Log("Rogue took " + damage + " damage. Current health: " + currentHealth);

        // Check if the Rogue is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handle the Rogue's death
    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");  // Trigger the "Die" animation
        Debug.Log("Rogue died.");
    }

    // Gain XP method, this can be called from other controllers or when certain conditions are met
    public void GainXP(int xpAmount)
    {
        if (currentLevel >= 4)
        {
            Debug.Log("Max level reached! No more XP can be gained.");
            return;
        }

        currentXP += xpAmount;
        Debug.Log($"Gained XP: {xpAmount}. Current XP: {currentXP}/{xpToNextLevel}");

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    // Handle leveling up the Rogue
    private void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = 100 * currentLevel;

        // Level-up benefits
        abilityPoints++;
        maxHealth += (int)maxHealthIncrease;
        currentHealth = maxHealth; // Refill HP
        Debug.Log($"Leveled up! Current Level: {currentLevel}, Max HP: {maxHealth}, Ability Points: {abilityPoints}");

        // Handle overflow XP
        if (currentXP >= xpToNextLevel)
        {
            LevelUp(); // If still exceeds next level XP, level up again
        }
    }

    void OnGUI()
    {
        float labelWidth = 200f;  // Width of each label
        float labelHeight = 20f;  // Height of each label
        float buttonWidth = 100f; // Width of "Level UP" buttons
        float xPosition = 10f;    // X-position for bottom-left alignment
        float buttonXPosition = xPosition + labelWidth + 10f; // Position for buttons (next to labels)
        float yStartPosition = Screen.height - (8 * labelHeight) - 10f; // Start from bottom

        // Display Rogue stats
        GUI.Label(new Rect(xPosition, yStartPosition, labelWidth, labelHeight), "Rogue HP: " + currentHealth);
        GUI.Label(new Rect(xPosition, yStartPosition + 20, labelWidth, labelHeight), "Level: " + currentLevel);
        GUI.Label(new Rect(xPosition, yStartPosition + 40, labelWidth, labelHeight), "XP: " + currentXP + " / " + xpToNextLevel);
        GUI.Label(new Rect(xPosition, yStartPosition + 60, labelWidth, labelHeight), "Ability Points: " + abilityPoints);

        // Ability Status and Unlock Buttons
        GUI.Label(new Rect(xPosition, yStartPosition + 90, labelWidth, labelHeight), "Arrow: " + (isArrowUnlocked ? "Unlocked" : "Locked"));
        if (!isArrowUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 90, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Arrow");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 110, labelWidth, labelHeight), "Smoke Bomb: " + (isSmokeBombUnlocked ? "Unlocked" : "Locked"));
        if (!isSmokeBombUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 110, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Smoke Bomb");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 130, labelWidth, labelHeight), "Dash: " + (isDashUnlocked ? "Unlocked" : "Locked"));
        if (!isDashUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 130, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Dash");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 150, labelWidth, labelHeight), "Shower of Arrows: " + (isShowerOfArrowsUnlocked ? "Unlocked" : "Locked"));
        if (!isShowerOfArrowsUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 150, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Shower of Arrows");
            }
        }
    }

    // Method to unlock abilities when the respective "Level UP" button is pressed
    void UnlockAbility(string abilityName)
    {
        switch (abilityName)
        {
            case "Arrow":
                isArrowUnlocked = true;
                break;
            case "Smoke Bomb":
                isSmokeBombUnlocked = true;
                break;
            case "Dash":
                isDashUnlocked = true;
                break;
            case "Shower of Arrows":
                isShowerOfArrowsUnlocked = true;
                break;
            default:
                Debug.LogError("Unknown ability: " + abilityName);
                break;
        }

        // Decrement ability points after unlocking the ability
        abilityPoints--;

        // Optional: Debug log to confirm ability unlocking
        Debug.Log(abilityName + " unlocked! Ability Points: " + abilityPoints);
    }
}
