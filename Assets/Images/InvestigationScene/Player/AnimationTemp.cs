using UnityEngine;

public class AnimationTemp : MonoBehaviour
{
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int state = 0;
        if (Input.GetKey(KeyCode.W)) state = 1;
        else if (Input.GetKey(KeyCode.S)) state = 4;
        else if (Input.GetKey(KeyCode.A)) state = 2;
        else if (Input.GetKey(KeyCode.W)) state = 3;
        else state = 0;
        animator.SetInteger("State", state);
    }
}
