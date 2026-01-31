using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public GameObject healthBarPrefab; // Assign health bar prefab in the inspector
    private GameObject healthBarInstance;
    private Image healthBarFill;

    private void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
        HideHealthBar();
    }

    private void CreateHealthBar()
    {
        Vector3 position = GetComponentInParent<Transform>().position;
        if (healthBarPrefab)
        {
            // Create the health bar instance
            healthBarInstance = Instantiate(healthBarPrefab, position, Quaternion.identity);
            
            // Set the health bar as a child of this GameObject
            healthBarInstance.transform.SetParent(transform, false);

            // Set initial position above the character
            healthBarInstance.transform.localPosition = new Vector3(0, 1.5f, 0); // Adjust this value for height
            healthBarFill = healthBarInstance.GetComponentInChildren<Image>();
            UpdateHealthBar();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    private void Update()
    {
        // If needed, you can still update the position based on local space
        if (healthBarInstance)
        {
            // This can be removed if using SetParent
            healthBarInstance.transform.localPosition = new Vector3(0, 2f, 0); // Adjust as needed
            if(LayerMask.LayerToName(gameObject.layer) == "Enemy")
            {
                Vector3 directionToPlayer = (PlayerController.playerTransform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                healthBarInstance.transform.rotation = Quaternion.Slerp(healthBarInstance.transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    public void Die()
    {
        Destroy(healthBarInstance); // Destroy health bar
    }

    public void ShowHealthBar()
    {
        if (healthBarInstance)
            healthBarInstance.SetActive(true);
    }

    public void HideHealthBar()
    {
        if (healthBarInstance)
            healthBarInstance.SetActive(false);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentHealth(float newCurrentHealth)
    {
        currentHealth = newCurrentHealth;
    }
}
