using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BarbarianController : MonoBehaviour
{
    public float moveSpeed = 5f;        // Speed at which the Barbarian moves
    private Vector3 targetPosition;     // The position the Barbarian will move towards
    private bool isMoving = false;      // Whether the Barbarian is currently moving

    private Animator animator;          // Animator for the Barbarian (for animations)

    public int maxHealth = 100;         // Max health of the Barbarian
    private int currentHealth;          // Current health of the Barbarian

    // Attack Range and Bash functionality
    public float attackRange = 15f;      // Range within which the Barbarian can perform the Bash
    public float bashCooldownTime = 1f; // Cooldown time for Bash ability
    private bool isBashing = false;     // Whether the Barbarian is currently performing the Bash attack
    private bool isCharging = false; // Tracks if the Barbarian is in Charge Mode
    private bool isDead = false;
    private bool canBash = true;        // Tracks if Bash is off cooldown

    // Shield functionality
    public GameObject shieldPrefab;         // Prefab for the shield visual effect
    private GameObject activeShield;        // Reference to the active shield
    private bool isShieldActive = false;    // Whether the shield is currently active
    private float shieldDuration = 3f;      // Duration for which the shield lasts
    private bool canShield = true;          // Tracks if the shield is off cooldown
    public float shieldCooldownTime = 5f;   // Cooldown time for the shield

    // Iron Maelstorm functionality
    public float ironMaelstormRadius = 5f;  // Radius within which enemies will be hit by the spinning attack
    public float ironMaelstormCooldown = 10f; // Cooldown for Iron Maelstorm
    private bool canUseIronMaelstorm = true; // If the Barbarian can use Iron Maelstorm

    // Charge functionality (add these in your class)
    public float chargeRange = 15f;      // The maximum range of the Charge ability
    public float chargeSpeed = 10f;      // Speed of the charge
    public float chargeCooldown = 10f;   // Cooldown for the charge ability
    private bool canUseCharge = true;    // Whether the Barbarian can use Charge

    // XP and Level System Variables
    public int currentLevel = 1;          // Starting level
    public int currentXP = 0;             // Current XP
    public int xpToNextLevel = 100;       // XP needed for the next level
    public int abilityPoints = 0;         // Ability points earned through leveling up
    public float maxHealthIncrease = 100; // Health increment per level

    // Ability Unlock States
    private bool isBashUnlocked = true;      // Bash is unlocked by default
    private bool isIronMaelstormUnlocked = false;
    private bool isChargeUnlocked = false;
    private bool isShieldUnlocked = false;   // Shield is unlocked by default
    private bool isInvincible = false;

    public Slider healthSlider;
    public Slider xpSlider;

    private int tpresscount;
    private int cpresscount;



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

        // Handle movement and attack inputs
        if (Input.GetMouseButtonDown(0)) // Left-click (0) to set the target position for movement
        {
            SetTargetPosition();
        }

        if (Input.GetMouseButtonDown(1) && canBash && !isBashing) // Right-click (1) for Bash ability
        {
            TryBash();
        }

        if (Input.GetKeyDown(KeyCode.W) && canShield && isShieldUnlocked) // Press "W" to activate the Shield
        {
            ActivateShield();
        }

        if (Input.GetKeyDown(KeyCode.Q) && canUseIronMaelstorm && isIronMaelstormUnlocked) // Press "Q" to activate Iron Maelstorm
        {
            UseIronMaelstorm();
        }

        if (Input.GetKeyDown(KeyCode.E) && canUseCharge && isChargeUnlocked) // Press "E" to activate Charge
        {
            Charge();
        }

        // Unlock abilities using keys 1, 2, and 3
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isIronMaelstormUnlocked && abilityPoints > 0)
        {
            isIronMaelstormUnlocked = true;
            abilityPoints--;
            Debug.Log("Iron Maelstorm ability unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !isChargeUnlocked && abilityPoints > 0)
        {
            isChargeUnlocked = true;
            abilityPoints--;
            Debug.Log("Charge ability unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && !isShieldUnlocked && abilityPoints > 0)
        {
            isShieldUnlocked = true;
            abilityPoints--;
            Debug.Log("Shield ability unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.H) ) 
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

        if (Input.GetKeyDown(KeyCode.C)){
            bashCooldownTime = 0;
            chargeCooldown = 0;
            ironMaelstormCooldown = 0;
            shieldCooldownTime = 0;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            isChargeUnlocked = true;
            isIronMaelstormUnlocked = true;
            isShieldUnlocked = true;
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

        // Move the Barbarian if it's in motion
        if (isMoving && !isBashing) // Only move if not performing a Bash or Iron Maelstorm
        {
            MoveCharacterToTarget();
            animator.SetBool("IsMoving", true);  // Trigger moving animation
        }
        else
        {
            animator.SetBool("IsMoving", false); // Stop moving animation when not moving
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
            isMoving = true;  // Start moving the Barbarian towards the target position
        }
    }

    // Move the Barbarian towards the target position
    void MoveCharacterToTarget()
    {
        Vector3 moveDirection = targetPosition - transform.position; // Calculate the direction to move
        moveDirection.y = 0f; // Keep the Y value the same (flat ground)

        // Move the character towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Rotate the character to face the direction it's moving in
        if (moveDirection.magnitude > 0.1f)  // If the Barbarian is still moving
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        // Stop moving once the target position is reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    // Attempt to Bash an enemy
    void TryBash()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the raycast hits an enemy within the range
        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Enemy"))
        {
            Vector3 enemyPosition = hit.collider.transform.position;
            isMoving = false;

            // If the enemy is out of range, move towards it first
            if (Vector3.Distance(transform.position, enemyPosition) > attackRange)
            {
                targetPosition = enemyPosition;
                isMoving = true;
                StartCoroutine(MoveAndBash(hit.collider));
            }
            else
            {
                StartCoroutine(PerformBash(hit.collider));
            }
        }
        else
        {
            Debug.Log("No enemy targeted or enemy is not within range.");
        }
    }

    void ActivateShield()
    {
        if (!canShield || isShieldActive || !isShieldUnlocked) return; // Prevent activation if on cooldown or already active

        isShieldActive = true;
        canShield = false;

        animator.SetTrigger("Shield");  // Make sure the "Shield" trigger is set in the Animator

        // Spawn the shield visual effect at the Barbarian's position
        activeShield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);

        // Attach the shield to the Barbarian
        activeShield.transform.SetParent(transform);

        // Apply an offset to raise the shield slightly above the Barbarian
        activeShield.transform.localPosition = new Vector3(0, 1f, 0);  // Adjust the Y-value (2f can be modified as per your needs)

        Debug.Log("Shield activated!");

        // Start the shield duration coroutine
        StartCoroutine(ShieldDuration());
    }

    private IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(shieldDuration); // Wait for the shield to expire

        isShieldActive = false;

        // Destroy the shield visual effect
        if (activeShield != null)
        {
            Destroy(activeShield);
        }

        Debug.Log("Shield expired!");

        // Start the cooldown coroutine
        StartCoroutine(ShieldCooldown());
    }

    private IEnumerator ShieldCooldown()
    {
        yield return new WaitForSeconds(shieldCooldownTime);
        canShield = true; // Reset the cooldown
        Debug.Log("Shield ready!");
    }

    // Charge Ability (add this method in the class)
    void Charge()
    {
        if (!canUseCharge || !isChargeUnlocked) return;

        canUseCharge = false;

        // Trigger Charge animation
        animator.SetTrigger("Charge");

        // Get the target position for the charge (left-clicked location)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Move towards the target position at charge speed
            targetPosition = hit.point;

            // Start the charge movement coroutine
            StartCoroutine(PerformCharge());
        }

        // Start the cooldown for Charge ability
        StartCoroutine(ChargeCooldown());
    }



    private IEnumerator PerformCharge()
{
    // Move the Barbarian quickly towards the target position
    while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chargeSpeed * Time.deltaTime);

        // Check for enemies in the path of the charge
        Collider[] enemiesInPath = Physics.OverlapSphere(transform.position, 1f); // You can adjust the radius

        foreach (var enemy in enemiesInPath)
        {
            if (enemy.CompareTag("Enemy"))
            {
                MinionController minionController = enemy.GetComponent<MinionController>();
                if (minionController != null)
                {
                    minionController.TakeDamage(5);  // Example damage for enemies hit by charge
                }
            }
        }

        yield return null; // Wait until the next frame
    }

    // Once the charge is complete, stop moving
    isMoving = false;
}


    private IEnumerator ChargeCooldown()
    {
        yield return new WaitForSeconds(chargeCooldown);
        canUseCharge = true;
        Debug.Log("Charge ability ready!");
    }




    // Perform the Iron Maelstorm ability
    void UseIronMaelstorm()
    {
        if (!canUseIronMaelstorm || !isIronMaelstormUnlocked) return;

        canUseIronMaelstorm = false;

        // Trigger the Iron Maelstorm animation
        animator.SetTrigger("Spin");

        // Find and damage enemies within the specified radius
        Collider[] enemies = Physics.OverlapSphere(transform.position, ironMaelstormRadius);
        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // Apply damage to the enemy
                MinionController minionController = enemy.GetComponent<MinionController>();
                if (minionController != null)
                {
                    minionController.TakeDamage(10);  // Example damage amount
                }
            }
        }

        // Start the cooldown for Iron Maelstorm
        StartCoroutine(IronMaelstormCooldown());
    }

    private IEnumerator IronMaelstormCooldown()
    {
        yield return new WaitForSeconds(ironMaelstormCooldown);
        canUseIronMaelstorm = true;
        Debug.Log("Iron Maelstorm ready!");
    }

    // Face the enemy target
    void FaceTarget(Vector3 targetPosition)
    {
        Vector3 directionToEnemy = (targetPosition - transform.position).normalized;
        directionToEnemy.y = 0; // Ignore vertical rotation
        transform.rotation = Quaternion.LookRotation(directionToEnemy);
    }

    // Perform the Bash ability with a small delay
    private IEnumerator PerformBash(Collider enemy)
    {
        isBashing = true;
        canBash = false; // Set Bash on cooldown

        // Ensure the Barbarian is facing the target
        FaceTarget(enemy.transform.position);

        animator.SetTrigger("Bash");  // Trigger the Bash animation

        // Continuously face the enemy during the Bash delay
        float bashDelay = 0.5f; // Delay before applying damage
        float elapsed = 0f;

        while (elapsed < bashDelay)
        {
            FaceTarget(enemy.transform.position);
            elapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Apply damage to the targeted enemy
        MinionController minionController = enemy.GetComponent<MinionController>();
        if (minionController != null)
        {
            minionController.TakeDamage(5);  // Apply damage to the Minion
        }

        isBashing = false;

        // Start Bash cooldown
        yield return new WaitForSeconds(bashCooldownTime - bashDelay); // Account for the delay
        canBash = true;
    }

    // Move towards the enemy and perform Bash when in range
    private IEnumerator MoveAndBash(Collider enemy)
    {
        while (Vector3.Distance(transform.position, enemy.transform.position) > attackRange)
        {
            // Calculate the direction and set a target position that keeps the Barbarian within attack range
            Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            targetPosition = enemy.transform.position - directionToEnemy * attackRange;

            // Move towards the calculated position
            MoveCharacterToTarget();
            yield return null; // Wait until the next frame
        }

        // Once within range, face the target and perform the Bash attack
        FaceTarget(enemy.transform.position);
        StartCoroutine(PerformBash(enemy));
    }

    // Take damage method (called from other classes when the Barbarian is attacked)
    public void TakeDamage(int damage)
    {
        if (isDead || currentHealth <= 0) return; // If the Barbarian is already dead, do nothing

        if (isShieldActive)
        {
            Debug.Log("Barbarian is shielded and takes no damage!");
            return; // Ignore damage while the shield is active
        }

        currentHealth -= damage;  // Reduce health by the damage amount
        animator.SetTrigger("Damaged");  // Trigger the "Damaged" animation

        Debug.Log("Barbarian took " + damage + " damage. Current health: " + currentHealth);

        // Check if the Barbarian is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handle the Barbarian's death
    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");  // Trigger the "Die" animation
        Debug.Log("Barbarian died.");
    }

    // Gain XP method, this can be called from other controllers or when certain conditions are met
    public void GainXP(int xpAmount)
    {
        if (currentLevel >= 4)
        {
            currentXP = 400;
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


    // Handle leveling up the Barbarian
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

    void UnlockAbility(string abilityName)
    {
        if (abilityPoints <= 0) return; // Ensure the player has ability points

        switch (abilityName)
        {
            case "Bash":
                isBashUnlocked = true;
                Debug.Log("Bash ability unlocked!");
                break;

            case "IronMaelstorm":
                isIronMaelstormUnlocked = true;
                Debug.Log("Iron Maelstorm ability unlocked!");
                break;

            case "Charge":
                isChargeUnlocked = true;
                Debug.Log("Charge ability unlocked!");
                break;

            case "Shield":
                isShieldUnlocked = true;
                Debug.Log("Shield ability unlocked!");
                break;
        }

        abilityPoints--; // Deduct an ability point
    }



    void OnGUI()
    {
        float labelWidth = 200f;  // Width of each label
        float labelHeight = 20f;  // Height of each label
        float buttonWidth = 100f; // Width of "Level UP" buttons
        float xPosition = 10f;    // X-position for bottom-left alignment
        float buttonXPosition = xPosition + labelWidth + 10f; // Position for buttons (next to labels)
        float yStartPosition = Screen.height - (8 * labelHeight) - 10f; // Start from bottom

        // Display Barbarian stats
        GUI.Label(new Rect(xPosition, yStartPosition, labelWidth, labelHeight), "Barbarian HP: " + currentHealth);
        GUI.Label(new Rect(xPosition, yStartPosition + 20, labelWidth, labelHeight), "Level: " + currentLevel);
        GUI.Label(new Rect(xPosition, yStartPosition + 40, labelWidth, labelHeight), "XP: " + currentXP + " / " + xpToNextLevel);
        GUI.Label(new Rect(xPosition, yStartPosition + 60, labelWidth, labelHeight), "Ability Points: " + abilityPoints);

        // Ability Status and Unlock Buttons
        GUI.Label(new Rect(xPosition, yStartPosition + 90, labelWidth, labelHeight), "Bash: " + (isBashUnlocked ? "Unlocked" : "Locked"));
        if (!isBashUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 90, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Bash");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 110, labelWidth, labelHeight), "Iron Maelstorm: " + (isIronMaelstormUnlocked ? "Unlocked" : "Locked"));
        if (!isIronMaelstormUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 110, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("IronMaelstorm");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 130, labelWidth, labelHeight), "Charge: " + (isChargeUnlocked ? "Unlocked" : "Locked"));
        if (!isChargeUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 130, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Charge");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 150, labelWidth, labelHeight), "Shield: " + (isShieldUnlocked ? "Unlocked" : "Locked"));
        if (!isShieldUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 150, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Shield");
            }
        }
    }


}
