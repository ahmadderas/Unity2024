using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SorcererController : MonoBehaviour
{
    public float moveSpeed = 5f;           // Speed at which the character moves
    public GameObject fireballPrefab;      // Fireball prefab
    public Transform fireballSpawnPoint;  // Right hand's transform
    public float fireballSpeed = 10f;     // Speed of the fireball
    private Animator animator;

    public float health = 100f;            // Sorcerer’s health
    private bool isDead = false;           // Flag to track if Sorcerer is dead

    private Vector3 targetPosition;       // The position to which the character will move
    private bool isMoving = false;        // Whether the character is currently moving
    private bool canCastFireball = true;  // Determines if the fireball can be cast
    public float fireballCooldown = 1f;   // Cooldown time for casting fireball
    public float fireballCastRange = 15f;  // Fireball cast range


    private Camera mainCamera;            // Main camera

    // Clone and Teleport Variables
    public GameObject clonePrefab;        // The clone prefab
    public float cloneDuration = 5f;      // How long the clone stays
    public float teleportCooldown = 10f; // Cooldown for teleporting
    public float cloneCooldown = 10f;    // Cooldown for cloning
    private bool canTeleport = true;      // Determines if teleportation can be performed
    private bool canClone = true;

    private bool isTeleporting = false;   // Indicates whether the Sorcerer is in teleport mode
    private bool isCloning = false;
    private bool isInferno = false;

    // Inferno Variables
    public GameObject infernoPrefab;      // Inferno ring prefab
    public float infernoDuration = 5f;    // Duration of the flame ring
    public float infernoRange = 900000000f;      // Maximum range for Inferno placement
    public float infernoCooldown = 10f;   // Cooldown for Inferno ability
    private bool canUseInferno = true;    // Tracks whether Inferno ability can be used

    // Teleport and Clone Range Variables
    public float teleportRange = 20f;     // Maximum range for teleporting
    public float cloneRange = 20f;        // Maximum range for creating clones
    public LayerMask ignoreLayer;  // Layer to ignore (i.e., Inferno Layer)

    // XP and Level System Variables
    public int currentLevel = 1;          // Starting level
    public int currentXP = 0;             // Current XP
    public int xpToNextLevel = 100;       // XP needed for the next level
    public int abilityPoints = 0;         // Ability points earned through leveling up
    public float maxHealthIncrease = 100; // Health increment per level

    // Ability States
    private bool isBasicUnlocked = true;  // Basic ability is unlocked by default
    private bool isCloneUnlocked = false;
    private bool isTeleportUnlocked = false;
    private bool isInfernoUnlocked = false;
    private bool isInvincible = false;

    public Slider healthSlider;
    public Slider xpSlider;

    // Cooldown Timers
    private float cloneCooldownRemaining = 0f;
    private float teleportCooldownRemaining = 0f;
    private float infernoCooldownRemaining = 0f;
    private int tpresscount = 0;

    void Start()
    {
        // Get the main camera and animator
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();

        // Initialize sliders
        healthSlider.maxValue = maxHealthIncrease;
        healthSlider.value = health;

        xpSlider.minValue = 0; // Min value of XP slider
        xpSlider.maxValue = 1; // Max value of XP slider is 1 (we'll adjust the value based on progress)
        UpdateXP(); // Update the XP slider based on current XP and the required XP for the next level

        // Initialize sliders

    }

    void Update()
    {
        if (isDead) return; // If Sorcerer is dead, stop any further updates

        UpdateSliders();

        // Input for abilities
        if (Input.GetMouseButtonDown(1) && canCastFireball)
        {
            TryCastFireball();
        }

        if (Input.GetKeyDown(KeyCode.Q) && canClone && isCloneUnlocked)
        {
            isCloning = true;
            isTeleporting = false;
            isInferno = false;
        }

        if (isCloning && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(HandleClone());
        }

        if (Input.GetKeyDown(KeyCode.W) && canTeleport && isTeleportUnlocked)
        {
            isTeleporting = true;
            isCloning = false;
            isInferno = false;
        }

        if (isTeleporting && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(HandleTeleport());
        }

        if (Input.GetKeyDown(KeyCode.E) && canUseInferno && isInfernoUnlocked)
        {
            isInferno = true;
            isTeleporting = false;
            isCloning = false;
            canCastFireball = false;
        }

        if (isInferno)
        {
            ActivateInfernoMode();
        }

        // Unlock abilities using keys 1, 2, and 3
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isCloneUnlocked && abilityPoints > 0)
        {
            isCloneUnlocked = true;
            abilityPoints--;
            Debug.Log("Clone ability unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !isTeleportUnlocked && abilityPoints > 0)
        {
            isTeleportUnlocked = true;
            abilityPoints--;
            Debug.Log("Teleport ability unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && !isInfernoUnlocked && abilityPoints > 0)
        {
            isInfernoUnlocked = true;
            abilityPoints--;
            Debug.Log("Inferno ability unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (health + 20 >= maxHealthIncrease)
            {
                health = maxHealthIncrease;
            }
            else
            {
                health += 20;
            }

        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (health - 20 <= 0)
            {
                Die();
            }
            else
            {
                health -= 20;
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
            fireballCooldown = 0;
            infernoCooldown = 0;
            teleportCooldown = 0;
            cloneCooldown = 0;
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            isCloneUnlocked = true;
            isTeleportUnlocked = true;
            isInfernoUnlocked= true;
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
            while (health != maxHealthIncrease)
            {
                health = maxHealthIncrease;
            }
        }

        // Decrease cooldowns over time
        if (cloneCooldownRemaining > 0) cloneCooldownRemaining -= Time.deltaTime;
        if (teleportCooldownRemaining > 0) teleportCooldownRemaining -= Time.deltaTime;
        if (infernoCooldownRemaining > 0) infernoCooldownRemaining -= Time.deltaTime;

        // Clamp cooldowns to zero
        cloneCooldownRemaining = Mathf.Max(cloneCooldownRemaining, 0);
        teleportCooldownRemaining = Mathf.Max(teleportCooldownRemaining, 0);
        infernoCooldownRemaining = Mathf.Max(infernoCooldownRemaining, 0);

        // Left-click to set target position for movement (if not in teleport or inferno mode)
        if (Input.GetMouseButtonDown(0) && !isTeleporting && !isCloning) // Left-click (0)
        {
            SetTargetPosition();
        }

        // Move the character if it's in motion
        if (isMoving)
        {
            MoveCharacterToTarget();
            animator.SetBool("IsMoving", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
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
        healthSlider.value = health;

        // Update XP slider
        UpdateXP();
    }

    // Add the TakeDamage method
    public void TakeDamage(float damage)
    {
        if (isDead) return; // If Sorcerer is dead, do not process further damage

        health -= damage; // Reduce health by the damage amount
        animator.SetTrigger("Damaged");
        Debug.Log("Sorcerer took damage! Health: " + health);

        // Check if health has reached zero
        if (health <= 0)
        {
            Die();
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

        // Display Wanderer stats
        GUI.Label(new Rect(xPosition, yStartPosition, labelWidth, labelHeight), "Wanderer HP: " + health);
        GUI.Label(new Rect(xPosition, yStartPosition + 20, labelWidth, labelHeight), "Level: " + currentLevel);
        GUI.Label(new Rect(xPosition, yStartPosition + 40, labelWidth, labelHeight), "XP: " + currentXP + " / " + xpToNextLevel);
        GUI.Label(new Rect(xPosition, yStartPosition + 60, labelWidth, labelHeight), "Ability Points: " + abilityPoints);

        // Ability Status and Unlock Buttons
        GUI.Label(new Rect(xPosition, yStartPosition + 90, labelWidth, labelHeight), "Basic: " + (isBasicUnlocked ? "Unlocked" : "Locked"));
        if (!isBasicUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 90, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Basic");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 110, labelWidth, labelHeight), "Clone: " + (isCloneUnlocked ? $"Unlocked (Cooldown: {cloneCooldownRemaining:F0}s)" : "Locked"));
        if (!isCloneUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 110, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Clone");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 130, labelWidth, labelHeight), "Teleport: " + (isTeleportUnlocked ? $"Unlocked (Cooldown: {teleportCooldownRemaining:F0}s)" : "Locked"));
        if (!isTeleportUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 130, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Teleport");
            }
        }

        GUI.Label(new Rect(xPosition, yStartPosition + 150, labelWidth, labelHeight), "Inferno: " + (isInfernoUnlocked ? $"Unlocked (Cooldown: {infernoCooldownRemaining:F0}s)" : "Locked"));
        if (!isInfernoUnlocked && abilityPoints > 0)
        {
            if (GUI.Button(new Rect(buttonXPosition, yStartPosition + 150, buttonWidth, labelHeight), "Level UP"))
            {
                UnlockAbility("Inferno");
            }
        }
    }

    void UnlockAbility(string abilityName)
    {
        if (abilityPoints > 0)
        {
            switch (abilityName)
            {
                case "Clone":
                    if (!isCloneUnlocked)
                    {
                        isCloneUnlocked = true;
                        abilityPoints--;
                        Debug.Log("Clone ability unlocked! Ability Points: " + abilityPoints);
                    }
                    break;

                case "Teleport":
                    if (!isTeleportUnlocked)
                    {
                        isTeleportUnlocked = true;
                        abilityPoints--;
                        Debug.Log("Teleport ability unlocked! Ability Points: " + abilityPoints);
                    }
                    break;

                case "Inferno":
                    if (!isInfernoUnlocked)
                    {
                        isInfernoUnlocked = true;
                        abilityPoints--;
                        Debug.Log("Inferno ability unlocked! Ability Points: " + abilityPoints);
                    }
                    break;

                default:
                    Debug.LogError("Unknown ability: " + abilityName);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Not enough Ability Points to unlock " + abilityName);
        }
    }





    // Method to handle Sorcerer death
    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");  // Trigger a death animation if one is set up
        Debug.Log("Sorcerer has died!");
        // You can add more death-related logic here, like disabling movement, playing death animations, etc.
    }

    // Handle Clone Placement
    private IEnumerator HandleClone()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && Vector3.Distance(hit.point, transform.position) <= cloneRange)
        {
            CreateCloneAt(hit.point);
        }
        else
        {
            // Move to the target location first
            while (Vector3.Distance(hit.point, transform.position) > cloneRange)
            {
                targetPosition = hit.point;
                isMoving = true;

                // Move the character towards the target
                MoveCharacterToTarget();

                yield return null;
            }
            CreateCloneAt(hit.point);  // Once in range, create the clone
        }
    }

    // Handle Teleportation
    private IEnumerator HandleTeleport()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && Vector3.Distance(hit.point, transform.position) <= teleportRange)
        {
            PerformTeleportTo(hit.point);
        }
        else
        {
            // Move to the target location first
            while (Vector3.Distance(hit.point, transform.position) > teleportRange)
            {
                targetPosition = hit.point;
                isMoving = true;

                // Move the character towards the target
                MoveCharacterToTarget();

                yield return null;
            }
            PerformTeleportTo(hit.point);  // Once in range, teleport the character
        }
    }

    // Create the Clone at the target position
    void CreateCloneAt(Vector3 targetPosition)
    {
        if (isTeleporting)
        {
            Debug.Log("Cannot create a clone while teleporting.");
            return;
        }

        if (Vector3.Distance(targetPosition, transform.position) <= cloneRange)
        {
            GameObject clone = Instantiate(clonePrefab, targetPosition, Quaternion.identity);
            isCloning = false;
            StartCoroutine(CloneCooldown());
            Destroy(clone, cloneDuration);
        }
    }

    // Perform teleportation to the target position
    void PerformTeleportTo(Vector3 targetPosition)
    {
        if (Vector3.Distance(targetPosition, transform.position) <= teleportRange)
        {
            transform.position = targetPosition;
            isTeleporting = false;
            canTeleport = false;
            StartCoroutine(TeleportCooldown());
        }
    }

    // Cooldown coroutines for teleport and clone
    private IEnumerator CloneCooldown()
    {
        canClone = false;
        cloneCooldownRemaining = cloneCooldown;
        yield return new WaitForSeconds(cloneCooldown);
        canClone = true;
    }

    private IEnumerator TeleportCooldown()
    {
        canTeleport = false;
        teleportCooldownRemaining = teleportCooldown; // Start cooldown timer
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    // Inferno Ability
    void ActivateInfernoMode()
    {
        Debug.Log("Inferno mode activated. Click to place the ring of flames.");
        StartCoroutine(PlaceInferno());
        isInferno = false;
    }

    private IEnumerator PlaceInferno()
    {
        bool infernoPlaced = false;

        // Prevent placing multiple Infernos at the same time
        if (isInferno && !infernoPlaced)
        {
            while (!infernoPlaced)
            {
                if (Input.GetMouseButtonDown(1)) // Right-click to place the Inferno
                {
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit) && Vector3.Distance(hit.point, transform.position) <= infernoRange)
                    {
                        Vector3 infernoPosition = new Vector3(hit.point.x, 0f, hit.point.z); // Ensure Y is 0
                        GameObject inferno = Instantiate(infernoPrefab, infernoPosition, Quaternion.identity);
                        canCastFireball = true;
                        isInferno = false;  // Set to false to prevent further placements while this one is active
                        Debug.Log("Inferno created at: " + hit.point);

                        // Destroy the Inferno after its duration
                        Destroy(inferno, infernoDuration);

                        // Start cooldown
                        StartCoroutine(InfernoCooldown());
                        infernoPlaced = true; // Inferno placed, set the flag
                    }
                    else
                    {
                        // Optionally, show a message or visual cue that the location is invalid or out of range
                        Debug.Log("Invalid location or out of range for Inferno.");
                    }
                }

                yield return null; // Wait until next frame before checking input again
            }
        }
    }

    private IEnumerator InfernoCooldown()
    {
        canUseInferno = false;
        infernoCooldownRemaining = infernoCooldown; // Start cooldown timer
        yield return new WaitForSeconds(infernoCooldown);
        canUseInferno = true;
    }


    // Try casting fireball if the ray hits an enemy
    void TryCastFireball()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform a raycast from the camera to where the user clicked
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignoreLayer)) // Ignore Inferno layer
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // If the ray hits an enemy, get the position of the enemy
                Vector3 targetPosition = hit.collider.transform.position;

                // Cast the fireball towards the enemy's position
                animator.SetTrigger("UseFireball");
                isMoving = false;
                FaceEnemy(targetPosition);  // Optionally face the enemy

                SpawnFireball(targetPosition);
                StartCoroutine(FireballCooldown());
            }
            else
            {
                Debug.Log("No enemy targeted!");
            }
        }
    }

    // Spawn fireball directly at the enemy (bypass obstacles)
    void SpawnFireball(Vector3 targetPosition)
    {
        // Cast a ray from the camera to where the player clicked
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // If the ray hits an object
        if (Physics.Raycast(ray, out hit))
        {
            // Instantiate the fireball at the spawn point
            GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, fireballSpawnPoint.rotation);

            // Calculate direction towards the target (enemy)
            Vector3 directionToTarget = (hit.point - fireballSpawnPoint.position).normalized;

            // Set fireball velocity to move towards the target
            Rigidbody rb = fireball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = directionToTarget * fireballSpeed;
            }

            // Pass the target enemy (hit.collider.gameObject) to the fireball
            FireballCollision fireballScript = fireball.GetComponent<FireballCollision>();
            if (fireballScript != null)
            {
                fireballScript.SetTargetEnemy(hit.collider.gameObject);  // Set the enemy target for the fireball
            }

            // Destroy the fireball after a certain amount of time
            Destroy(fireball, 5f);
            Debug.Log("Fireball launched towards: " + hit.point);
        }
    }





    private IEnumerator FireballCooldown()
    {
        canCastFireball = false;
        yield return new WaitForSeconds(fireballCooldown);
        canCastFireball = true;
    }

    // XP System
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

    private void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = 100 * currentLevel;

        // Level-up benefits
        abilityPoints++;
        health += maxHealthIncrease;
        health = currentLevel * maxHealthIncrease; // Refill HP
        Debug.Log($"Leveled up! Current Level: {currentLevel}, Max HP: {health}, Ability Points: {abilityPoints}");

        // Handle overflow XP
        if (currentXP >= xpToNextLevel)
        {
            LevelUp(); // If still exceeds next level XP, level up again
        }
    }

    void SetTargetPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            targetPosition = hit.point;
            isMoving = true;
        }
    }

    void MoveCharacterToTarget()
    {
        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0f;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (moveDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    void FaceEnemy(Vector3 enemyPosition)
    {
        Vector3 directionToEnemy = enemyPosition - transform.position;
        directionToEnemy.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToEnemy);
    }
}