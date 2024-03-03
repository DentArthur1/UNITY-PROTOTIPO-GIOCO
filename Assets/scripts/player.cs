using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public float moveSpeed;
    public bool isMoving;
    private Vector2 input;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
          input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            Debug.Log("This is input.x" + input.x);
            Debug.Log("This is input.y" + input.y);


            if (input != Vector2.zero)
            {
                animator.SetFloat("Move X", input.x);
                animator.SetFloat("Move Y", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x * Time.deltaTime * moveSpeed;
                targetPos.y += input.y * Time.deltaTime * moveSpeed;
                transform.position = targetPos;
                isMoving = true;
            } else 
            {
            isMoving = false;
            }
        
        animator.SetBool("isMoving", isMoving);
    }
}
