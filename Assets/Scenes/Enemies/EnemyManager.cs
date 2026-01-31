using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int numberOfEnemies = 20;
    public float spawnAreaSize = 100f;
    public float healthPercentage = 100f;
    public Transform playerTransform;
    public GameObject healthBarPrefab; // Your health bar prefab
    SkinnedMeshRenderer[] skinnedMeshRenderers;

    private void Start()
    {
        playerTransform = PlayerController.playerTransform;
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        Vector3 managerPosition = transform.position; // Get the position of the EnemyManager
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnAreaSize, spawnAreaSize) + managerPosition.x,
                0,
                Random.Range(-spawnAreaSize, spawnAreaSize) + managerPosition.z
            );
            Vector3 positionVerification = new Vector3(spawnPosition.x,0.75f,spawnPosition.y);
            if(!isObjectHere(positionVerification))
            {
                GameObject zombieEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                skinnedMeshRenderers = zombieEnemy.GetComponentsInChildren<SkinnedMeshRenderer>();
                if(!zombieEnemy.name.StartsWith("Zombie"))
                {
                    ChangeColorByType(zombieEnemy,skinnedMeshRenderers);
                }
            }
            else
            {
                i--; // keep looking
            }
        }
    }

    private void ChangeColorByType(GameObject zombieEnemy,SkinnedMeshRenderer[] skinnedMeshRenderers)
    {
        Color newColor = FindNewColor(zombieEnemy);

        if (skinnedMeshRenderers.Length > 0)
        {
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                ChangeColor(skinnedMeshRenderer, newColor);
            }
        }
        else
        {
            Debug.LogError("No SkinnedMeshRenderer found in this GameObject or its children!");
        }
    }

    private Color FindNewColor(GameObject zombieEnemy)
    {
        if (zombieEnemy.name.StartsWith("BabyZombie"))
        {
            return Color.cyan;
        }
        else if (zombieEnemy.name.StartsWith("MegaZombie"))
        {
            return Color.gray;
        }
        else
        {
            Debug.Log("Trying to change color for something other than a zombie!");
            return Color.white;
        }
    }

    bool isObjectHere(Vector3 position)
    {
        Collider[] intersecting = Physics.OverlapSphere(position, 0.5f);
        if (intersecting.Length == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void ChangeColor(SkinnedMeshRenderer renderer, Color newColor)
    {
        // Store the original color
        Color originalColor = renderer.material.GetColor("_Color");
        
        // Change to new color
        foreach (Material material in renderer.sharedMaterials)
        {
            material.SetColor("_Color", newColor);
        }
    }
}
