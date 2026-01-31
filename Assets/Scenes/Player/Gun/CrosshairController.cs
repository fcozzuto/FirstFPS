using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    private const float Seconds = 0.001f;
    public static RawImage crosshairImage;

    public static Animator CrosshairAnimator { get; set; }
/*
    public float maxDistance = 75f;
    public Vector3 originalSize = new Vector3(1f, 1f, 1f);
    public Vector3 newSize = new Vector3(5f, 5f, 1f);
*/
    public Vector2 originalSize;// Original size
    public Vector2 newSize;  // New size for enemies

    void Start()
    {
        // Get the RawImage component on this GameObject
        crosshairImage = GetComponent<RawImage>();
        originalSize = new Vector2(50f, 50f);  // Original size
        newSize = new Vector2(75f, 75f);  // New size for enemies
        crosshairImage.rectTransform.sizeDelta = originalSize;  // Set initial size

        // Check if crosshairImage was found
        if (crosshairImage == null)
        {
            Debug.LogError("Crosshair RawImage component not found on this GameObject!");
        }

        CrosshairAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        AdjustCrosshairSize();
    }

    public void AdjustCrosshairSize()
    {
        RaycastHit hit = PlayerController.raycastHit;

        //Perform the raycast using the layer mask
        if(PlayerController.lookingAtEnemy)
        {
            //Debug.Log("we're looking at: " + hit.collider.name);
            //Debug.Log("enemy distance is: " + hit.distance);
            float scale = Mathf.Clamp01(hit.distance / 50f);
            //Debug.Log("scale is now: " + scale);
            crosshairImage.rectTransform.sizeDelta = Vector3.Lerp(originalSize, newSize, scale * 3);
            //Debug.Log("New vector, after transformation is: " + crosshairImage.rectTransform.localScale.ToString());
            ChangeCrosshairColor();
            return;
        }
        // Reset size when not looking at an enemy
        crosshairImage.rectTransform.sizeDelta = originalSize; // Reset to minimum size
        RevertCrosshairColor();
    }

    public static IEnumerator CrosshairColorChangeOnShoot()
    {
        CrosshairController.ChangeCrosshairColor();
        while(!GunScript.IsRecoilAnimationDone)
        {
            yield return new WaitForSeconds(Seconds);
        }
        CrosshairController.RevertCrosshairColor();
    }

    public static void ChangeCrosshairColor()
    {
        // Ensure crosshairImage is assigned
        if (crosshairImage != null)
        {
            crosshairImage.color = Color.red; // Change to red
            // Handle shooting input
            //if (Input.GetButtonDown("Fire1"))
            {
                //Debug.Log("Shooting!");
                //crosshairImage.color = Color.red; // Change to red
            }
        }
    }

    public static void RevertCrosshairColor()
    {
        // Revert the color back to white on mouse button release

        // Ensure crosshairImage is assigned
        if (crosshairImage == null)
        {
            Debug.Log("CrosshairImage is not assigned");
        }
        crosshairImage.color = Color.white; // Reset to white
    }

    public static void ChangeCrosshairColor(Color color)
    {
        // Ensure crosshairImage is assigned
        if (crosshairImage == null)
        {
            Debug.Log("CrosshairImage is not assigned");
        }
        crosshairImage.color = color; // Change the crosshair to the selected color
    }
}