using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class triangolo : MonoBehaviour
{
    public Rigidbody2D triangle;
    private float rotation;
    public int velocità;
    private Vector3 vel;

    float normalize_orientation(float angle){ //notazione classica per calcolo seno e coseno
        angle += 90;
        if (angle < 0){
             angle = 260 + 90 - math.abs(angle);
        }
        return angle;
    }
    void Start()
    {
       
    }

    // Update is called once per frame
     void FixedUpdate()
    {
         //ORIENTATION
         Vector3 mouse_pos = Input.mousePosition; //get mouse position
         mouse_pos = Camera.main.ScreenToWorldPoint(mouse_pos); //convert screenposition to wordposition
         Vector2 direction = new Vector2(mouse_pos.x - transform.position.x, mouse_pos.y - transform.position.y); //get relative orientation from the triangle
         triangle.transform.up = direction; //updates orientation

         //VELOCITY
         rotation = normalize_orientation(triangle.rotation);
         vel.y = Mathf.Sin(rotation * Mathf.Deg2Rad);
         vel.x = Mathf.Cos(rotation * Mathf.Deg2Rad);
         print(vel);

         if(Input.GetKey(KeyCode.W)){
             triangle.MovePosition(transform.position + vel * Time.deltaTime * velocità);
         }
         
        
      
    }
    
}
