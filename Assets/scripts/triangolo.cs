using System;
using Unity.Mathematics;
using UnityEngine;

public class Triangolo : MonoBehaviour
{
    public Rigidbody2D triangle;     //oggetto nave
    private float direction;   //variabile direzione nave
    public float engine_acc;  //accelerazione spostamento generale
    public float engine_max_vel; //velocita' spostamento massima
    public float engine_min_vel; //velocita' massima di retromarcia
    private float engine_vel = 0; //velocita' corrente della nave
    private float thruster_vel = 0; //velocita' corrente di spostamento laterale
    public float max_thruster_vel; //velocita' di spostamento laterale massima
    public float thruster_acc; //accelerazione thruster laterali
    private float rot_vel = 0; //velocita' di rotazione corrente
    public float max_rot_vel; //velocita' massima di rotazione
    public float rot_acc; //accelerazione velocita' di rotazione
    private Vector3 vel;   //Vettore spostamento nave
    public Boolean flight_assist; //azzeramento automatico thruster e rotazione
    public float flight_assist_efficiency; //efficienza meccanismo flight assist

    float normalize_orientation(float angle){ //notazione classica per calcolo seno e coseno
        angle += 90;
        if (angle < 0){
             angle = 260 + 90 - math.abs(angle); //(normalizza da 0 a 360 gradi)
        } 
        
        return angle;
    }
    void DEBUG_LOG(){ //messaggio di debug generale
        Debug.Log("Vector: " + vel + " / Engine_vel: " + engine_vel + " / Direction: " + direction + " / Thruster_vel: " + thruster_vel + " / Rot_vel: " + rot_vel);
    }
    Vector3 calculate_vel(float angle, Vector3 vector){ //calcola la velocita' basata sull'orientamento della nave
        vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);    
        vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        return vector;
    }
    void check_vel(){ //Assicura che la velocitá rimanga nei limiti prestabiliti
        //MOTORE PRINCIPALE
        if(engine_vel >= engine_max_vel){
            engine_vel = engine_max_vel;
        } else if (engine_vel <= engine_min_vel){
            engine_vel = engine_min_vel;
        }

        //THRUSTERs LATERALI
        if(thruster_vel >= max_thruster_vel){
            thruster_vel = max_thruster_vel;
        } else if (thruster_vel <= -max_thruster_vel){
            thruster_vel = -max_thruster_vel;
        }
        
        //THRUSTER ROTAZIONE
        if(rot_vel >= max_rot_vel){
            rot_vel = max_rot_vel;
        } else if (rot_vel <= -max_rot_vel){
            rot_vel = -max_rot_vel;
        }
    }
    void get_real_vel(){ //somma i due vettori: Vettore motore principale + Vettore thruster laterale
         vel *= engine_vel; //moltiplico il vettore vel per la velocita' del motore attuale
         Vector3 thuster_vect = calculate_vel(direction + 90, vel) * thruster_vel; //calcolo il vettore perpendicolare alla direzione attuale
         vel += thuster_vect; //sommo i due vettori e ottengo il vettore risultante
    }
   
    void rotate_ship(){
          triangle.transform.Rotate(0,0,rot_vel * Time.deltaTime);
    }
    void update_pos(){ //modifica la posizione del triangolo sullo schermo
         vel = calculate_vel(direction, vel);
         check_vel();
         get_real_vel();
         rotate_ship();
         triangle.MovePosition(transform.position + vel * Time.deltaTime);  //modifica la posizione
    }

    void handle_inputs(){ //gestisce gli input dell'utente
         direction = normalize_orientation(triangle.rotation); //normalizza orientamento

         if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)){ //movimento avanti e indietro
             if (Input.GetKey(KeyCode.S)){ //indietro
                  engine_vel -= engine_acc;
             } else { //avanti
                  engine_vel += engine_acc;
             }
             
         }

         if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)){ //movimento destra e sinistra
            if (Input.GetKey(KeyCode.D)){ 
                thruster_vel -= thruster_acc; //ottiene direzione perpendicolare alla direzione attuale(verso destra)
            }else {   
                thruster_vel += thruster_acc; //ottiene direzione perpendicolare alla direzione attuale(verso sinistra)
            }
         } else{
            if(flight_assist){ //azzerazione progressiva del thruster laterale
                thruster_vel = Mathf.Lerp(thruster_vel, 0f, flight_assist_efficiency * Time.deltaTime);
            }
         }

         if(Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E)){ //rotazione assiale nave
            if (Input.GetKey(KeyCode.E)){ //rotazione verso destra
                  rot_vel -= rot_acc;
            } else { //rotazione verso sinistra
                  rot_vel += rot_acc;
            }
         } else {
            if(flight_assist){ //azzerazione progressiva del thruster di rotazione
                rot_vel = Mathf.Lerp(rot_vel, 0f, flight_assist_efficiency * Time.deltaTime);
            }
         }


    }
    void Start(){ //eseguito solo una volta
       
    }
    void FixedUpdate(){ //aggiorna tutti gli oggetti legati alla fisica di gioco
         update_pos();    
    }

    void Update(){ //aggiorna il mondo di gioco generale
         handle_inputs(); 
         DEBUG_LOG();
    }
    
}
