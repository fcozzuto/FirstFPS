
using UnityEngine;

public class GunScript : MonoBehaviour
{
    public LayerMask layerMask;
    public Animator anim;

    public float attackDamage = 15f;  // Amount of damage to deal
    public float attackDelay = 0.1f;     // Time in seconds between attacks
    private float nextAttackTime = 0f; // Time when the player can attack again

    public static bool IsRecoilAnimationDone { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        IsRecoilAnimationDone = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRecoilAnimationDone && Input.GetButtonDown("Fire1"))
        {
            Shoot();
            StartCoroutine(CrosshairController.CrosshairColorChangeOnShoot());
        }
    }

    void EndRecoil ()
    {
        anim.SetBool("fired", false);
        IsRecoilAnimationDone = true;
    }

    void Shoot()
    {
        // Check if it's time to attack
        if (Time.time >= nextAttackTime)
        {
            if (IsRecoilAnimationDone) // For the first shot, it will be true
            {
                anim.SetBool("fired", true);
                CrosshairController.CrosshairAnimator.SetTrigger("shotFired");
                MuzzleController.MuzzleAnimator.SetTrigger("shotFired");
                gameObject.GetComponent<AudioSource>().Play();
        
                RaycastHit hit = PlayerController.raycastHit;

                //Perform the raycast using the layer mask
                if(PlayerController.lookingAtEnemy)
                {
                    hit.collider.gameObject.GetComponent<ZombieController>().GotHit(attackDamage);
                    //hit.collider.gameObject.GetComponent<AudioSource>().Play();
                }
                IsRecoilAnimationDone = false;
                nextAttackTime = Time.time + attackDelay; // Set the next attack time
            }
        }
    }
}
