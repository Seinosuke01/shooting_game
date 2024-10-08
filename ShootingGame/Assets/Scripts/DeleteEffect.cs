using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteEffect : MonoBehaviour
{
    Animator animator;

    [SerializeField]
    private AudioSource deleteSound;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        if(deleteSound == null){
            deleteSound = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        var a = animator.GetCurrentAnimatorStateInfo(0);
        if(a.IsName("FinalScene")){
            deleteSound.Play();
            Destroy(this.gameObject);
        }
    }
}
