using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleController : MonoBehaviour
{
    public static Animator MuzzleAnimator { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        MuzzleAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
