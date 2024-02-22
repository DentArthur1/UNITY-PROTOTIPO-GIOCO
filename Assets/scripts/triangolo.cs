using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class triangolo : MonoBehaviour
{
    public Rigidbody2D triangle;     //oggetto nave
    private float rotation;   //variabile orientamento nave
    public int velocità;  //velocita' spostamento generale

    private Vector3 vel;   //Vettore spostamento nave

    float normalize_orientation(float angle){ //notazione classica per calcolo seno e coseno
        angle += 90;
        if (angle < 0){
             angle = 260 + 90 - math.abs(angle); //(normalizza da 0 a 360 gradi)
        }
        return angle;
    }
    void calculate_vel(float angle){ //calcola la velocita' basata sull'orientamento della nave
        //VELOCITY
        //true velocity
        vel.y = Mathf.Sin(rotation * Mathf.Deg2Rad);    
        vel.x = Mathf.Cos(rotation * Mathf.Deg2Rad);
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
         rotation = normalize_orientation(triangle.rotation);    //normalize orientation 
         
                     //FORWARD AND BACKWARDS
         if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)){
             
             calculate_vel(rotation);

             if (Input.GetKey(KeyCode.S)){ //BACKWARDS
                  vel = -vel;
             }
             triangle.MovePosition(transform.position + vel * Time.deltaTime * velocità);  //update position
             
         }
                     //LEFT AND RIGHT MOVEMENT
         if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)){

            if (Input.GetKey(KeyCode.D)){ //right movement
                rotation -= 90;
            }else {   //left movement
                rotation += 90;
            }
            calculate_vel(rotation);

            triangle.MovePosition(transform.position + vel * Time.deltaTime * velocità); //update position
           
         }

         print(vel); //debug
         


         
         
        
      
    }
    
}
