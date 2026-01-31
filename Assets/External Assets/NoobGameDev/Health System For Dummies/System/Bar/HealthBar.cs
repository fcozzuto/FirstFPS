using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FollowCameraRotation))]
public class HealthBar : MonoBehaviour
{
    public bool isBillboarded = true;
    public bool shouldShowHealthNumbers = false;
    float finalValue;
    float animationSpeed = 0.1f;
    float leftoverAmount = 0f;

    // Caches
    public HealthSystemForDummies healthSystem;
    Image image;
    Text text;
    FollowCameraRotation followCameraRotation;

    public static Canvas healthBarObject; // The entire health bar UI element
    public bool isHealthBarVisible;

    private void Start()
    {
        healthSystem = GetComponentInParent<HealthSystemForDummies>();
        if(healthSystem == null)
        {
            Debug.Log("healthsystem is null");
        }
        image = GetComponentInChildren<Image>();
        text = GetComponentInChildren<Text>();
        followCameraRotation = GetComponent<FollowCameraRotation>();
        if(healthSystem.OnCurrentHealthChanged == null)
        {
            Debug.Log("healthsystem.OnCurrentHealthChanged is null");
        }
        healthSystem.OnCurrentHealthChanged.AddListener(ChangeHealthFill);
        healthBarObject = GetComponentInParent<Canvas>();
        healthBarObject.enabled = true; // Hide the health bar initially
        isHealthBarVisible = healthBarObject.enabled;
        Debug.Log("Healthbar parent is: " + transform.parent.gameObject.name);
    }

    public void ChangeHealthBarVisibility()
    {
        isHealthBarVisible = healthBarObject.enabled;

        healthBarObject.enabled = !isHealthBarVisible; // Show the health bar
    }

    void Update()
    {
        animationSpeed = healthSystem.AnimationDuration;

        if (!healthSystem.HasAnimationWhenHealthChanges)
        {
            image.fillAmount = healthSystem.CurrentHealthPercentage / 100;
        }

        text.text = $"{healthSystem.CurrentHealth}/{healthSystem.MaximumHealth}";

        text.enabled = shouldShowHealthNumbers;

        followCameraRotation.enabled = isBillboarded;
    }

    public void ChangeHealthFill(CurrentHealth currentHealth)
    {
        if (!healthSystem.HasAnimationWhenHealthChanges) return;

        StopAllCoroutines();
        StartCoroutine(ChangeFillAmount(currentHealth));
    }

    private IEnumerator ChangeFillAmount(CurrentHealth currentHealth)
    {
        finalValue = currentHealth.percentage / 100;

        float cacheLeftoverAmount = this.leftoverAmount;

        float timeElapsed = 0;

        while (timeElapsed < animationSpeed)
        {
            float leftoverAmount = Mathf.Lerp((currentHealth.previous / healthSystem.MaximumHealth) + cacheLeftoverAmount, finalValue, timeElapsed / animationSpeed);
            this.leftoverAmount = leftoverAmount - finalValue;
            image.fillAmount = leftoverAmount;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        this.leftoverAmount = 0;
        image.fillAmount = finalValue;
    }
}