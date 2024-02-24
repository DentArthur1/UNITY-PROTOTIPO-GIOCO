using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D triangle;
    public Camera cam;
    public int offset;
    public int smoothSpeed;
    private Vector2 triangle_pos;
    void Start()
    {
       GameObject playerObject = GameObject.Find("Triangle");
       triangle = playerObject.GetComponent<Rigidbody2D>();
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        triangle_pos = triangle.position;
       
       
    }
}
