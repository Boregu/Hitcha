using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private bool isAlive = true;
    [SerializeField] private int revivalCount = 0;
    [SerializeField] private float baseRevivalDifficulty = 5f; // Base number of pumps needed
    
    [Header("Heart Animation")]
    [SerializeField] private Transform heartSprite; // Reference to the heart sprite
    [SerializeField] private float normalPumpDuration = 0.5f; // Duration of normal pump animation
    [SerializeField] private float dyingPumpDuration = 0.3f; // Duration of dying pump animation
    [SerializeField] private float pumpScaleMultiplier = 1.2f; // How much the heart grows when pumping
    [SerializeField] private float returnToNormalDuration = 0.3f; // How long it takes to return to normal size
    
    [Header("Revival Settings")]
    [SerializeField] private float currentPumpProgress = 0f;
    [SerializeField] private float pumpDecayRate = 0.5f; // How fast the pump progress decreases
    [SerializeField] private float difficultyIncrease = 2f; // How much harder it gets each death
    
    private Vector3 originalScale;
    private float currentPumpDuration;
    private bool isPumping = false;
    private float timeSinceLastPump = 0f;

    private void Start()
    {
        if (heartSprite == null)
        {
            Debug.LogError("Please assign the heart sprite in the inspector!");
            return;
        }
        
        originalScale = heartSprite.localScale;
        currentPumpDuration = normalPumpDuration;
    }

    private void Update()
    {
        if (!isAlive)
        {
            HandleDeathState();
        }
        
        AnimateHeart();
    }

    private void HandleDeathState()
    {
        currentPumpDuration = dyingPumpDuration;
        
        // Decay pump progress over time
        currentPumpProgress = Mathf.Max(0, currentPumpProgress - (pumpDecayRate * Time.deltaTime));
        
        // Calculate required pumps based on revival count
        float requiredPumps = baseRevivalDifficulty + (revivalCount * difficultyIncrease);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Increase progress when space is pressed
            currentPumpProgress += 1f;
            StartHeartPump();
            
            // Check if enough pumps to revive
            if (currentPumpProgress >= requiredPumps)
            {
                Revive();
            }
        }
    }

    private void Revive()
    {
        isAlive = true;
        currentPumpProgress = 0f;
        currentPumpDuration = normalPumpDuration;
        revivalCount++;
        Debug.Log($"Player revived! Revival count: {revivalCount}");
    }

    public void Kill()
    {
        if (isAlive)
        {
            isAlive = false;
            currentPumpProgress = 0f;
            Debug.Log("Player died! Spam space to revive!");
        }
    }

    private void StartHeartPump()
    {
        isPumping = true;
        timeSinceLastPump = 0f;
    }

    private void AnimateHeart()
    {
        if (isPumping)
        {
            timeSinceLastPump += Time.deltaTime;
            
            if (timeSinceLastPump < currentPumpDuration)
            {
                // Pump up phase
                float scale = 1f + (pumpScaleMultiplier - 1f) * (timeSinceLastPump / currentPumpDuration);
                heartSprite.localScale = originalScale * scale;
            }
            else if (timeSinceLastPump < currentPumpDuration + returnToNormalDuration)
            {
                // Return to normal phase
                float t = (timeSinceLastPump - currentPumpDuration) / returnToNormalDuration;
                float scale = pumpScaleMultiplier - ((pumpScaleMultiplier - 1f) * t);
                heartSprite.localScale = originalScale * scale;
            }
            else
            {
                // Animation complete
                heartSprite.localScale = originalScale;
                isPumping = false;
            }
        }
    }
}
