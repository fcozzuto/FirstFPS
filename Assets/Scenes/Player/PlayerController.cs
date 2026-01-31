using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static Transform playerTransform;
    public float normalSpeed = 10f; // Walking speed
    public float sprintSpeed = 20f; // Sprinting speed
    public float currentSpeed = 0f; // Current speed
    public float mouseSensitivity = 2.0f; // Mouse sensitivity
    public float verticalLookRange = 45.0f; // Vertical look range
    public float jumpSpeed = 3.0f; // Jump speed
    public float gravity = 15.0f; // Gravity strength

    private int enemyLayerMask; // Used for Ray casting
    public static Ray ray;
    public static RaycastHit raycastHit;
    public static bool lookingAtEnemy;

    private CharacterController characterController;
    private Camera playerCamera;

    private float verticalRotation = 0;
    private float ySpeed = 0; // For vertical speed

    public Health health;

    public GameObject[] enemyObjects;
    private int enemyCount;

    private bool levelCompleted;
    GameObject gateLight;

    GameOverManager gameOverManager;
    GameObject gameOverManagerObject;

    VictoryManager victoryManager;
    GameObject victoryManagerObject;

    void Start()
    {
        health = GetComponent<Health>();
        health.ShowHealthBar();
        playerTransform = gameObject.transform;
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        enemyCount = 0;

        gateLight = GameObject.FindGameObjectWithTag("GateLight");
        if(gateLight)            
            gateLight.SetActive(!gateLight.activeSelf); // Toggle the active state of the GameObject

        levelCompleted = false;

        gameOverManagerObject = GameObject.FindGameObjectWithTag("Respawn");
        gameOverManager = gameOverManagerObject.GetComponent<GameOverManager>();
        string sceneName = SceneManager.GetActiveScene().name;

        if(sceneName == "Dark Forest - Final Boss")
        {
            victoryManagerObject = GameObject.FindGameObjectWithTag("Finish");
            victoryManager = victoryManagerObject.GetComponent<VictoryManager>();
        }
    }

    void Update()
    {
        // Handle movement
        HandleMovement();

        // Handle looking around
        HandleLookingAround();

        CountEnemies();
        
        if(!levelCompleted)
            CheckForLevelEndCondition();
    }

    private void CountEnemies()
    {
        enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemyObjects.Length;
    }

    private void CheckForLevelEndCondition()
    {
        if (enemyCount == 0)
        {
            levelCompleted = true;
            TriggerSceneChangeOrEffects();
        }
    }

    private void TriggerSceneChangeOrEffects()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if(sceneName == "Dark Forest")
        {
            SceneManager.LoadScene("Dark Forest - Final Boss");
        }
        else if(sceneName == "Intro")
        {
            gateLight.SetActive(!gateLight.activeSelf);
        }
        else // Final boss beaten
        {
            characterController.enabled = false;
            victoryManager.Victory();
        }
    }

    private void HandleMovement()
    {
        if(!characterController.enabled)
        {
            return;
        }
        float moveDirectionY = Input.GetAxis("Vertical");
        float moveDirectionX = Input.GetAxis("Horizontal");

        Vector3 move = transform.right * moveDirectionX + transform.forward * moveDirectionY;
        if (characterController.isGrounded)
        {
            ySpeed = -0.5f; // A small negative value to ensure grounding
            if (Input.GetButtonDown("Jump"))
            {
                ySpeed = jumpSpeed; // Apply jump speed
            }
        }
        else
        {
            ySpeed -= gravity * Time.deltaTime; // Apply gravity when in the air
        }

        move.y = ySpeed;

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : normalSpeed;
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    private void HandleLookingAround()
    {
        if(!characterController.enabled)
        {
            return;
        }
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * mouseX * mouseSensitivity);

        verticalRotation -= mouseY * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookRange, verticalLookRange);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        enemyLayerMask = LayerMask.GetMask("Enemy");

        //Perform the raycast using the layer mask
        if(Physics.Raycast(ray, out raycastHit, Mathf.Infinity, enemyLayerMask))
        {
            lookingAtEnemy = true;
        }
        else
        {
            lookingAtEnemy = false;
            //Debug.Log("No enemy found!");
        }
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green);
    }

    public void TakeDamage(float damage)
    {
        float healthRemaining = health.GetCurrentHealth();

        if(healthRemaining > 0)
        {
            health.TakeDamage(damage);
        }
        CheckIfPlayerDies();
    }

    private void CheckIfPlayerDies()
    {
        float healthRemaining = health.GetCurrentHealth();
        if(healthRemaining <= 0)
        {
            health.Die();
            characterController.enabled = false;
            gameOverManager.GameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "LevelExit")
        {
            SceneManager.LoadScene("Dark Forest");
        }
        else if (other.tag == "Enemy")
        {
            health.TakeDamage(5f);
            //TODO play ouch sound clip
        }
    }
}
