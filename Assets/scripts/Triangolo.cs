using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Triangolo : MonoBehaviour //COMANDI --> W/S Acc. Avanti e indientro | A/D Acc. Destra e Sinistra | MOUSE Rotazione destra e sinistra
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
    public float rot_vel; //velocita' rotazione nave
    private Vector3 vel;   //Vettore spostamento nave
    public float min_rot_coeff; //valore da 0 a 1 escluso, per limitare quanto lentamente la nave deve ruotare nei grandi cambiamenti di rotta
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
        Debug.Log("Vector: " + vel + " / Engine_vel: " + engine_vel + " / Direction: " + direction + " / Thruster_vel: " + thruster_vel);
    }
    Vector3 partition_vect(float angle){ //partiziona il vettore in base all'orientamento della nave
        Vector3 vector;vector.z = 0;
        vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);    
        vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        return vector;
    }
    void check_limits(){ //Assicura che la velocitá rimanga nei limiti prestabiliti
        //MOTORE PRINCIPALE
        engine_vel = Mathf.Clamp(engine_vel, engine_min_vel, engine_max_vel);
        //THRUSTERs LATERALI
        thruster_vel = Mathf.Clamp(thruster_vel, -max_thruster_vel, max_thruster_vel);
    }
    void get_real_vel(){ //somma i due vettori: Vettore motore principale + Vettore thruster laterale
         Vector3 engine_vector = partition_vect(direction) * engine_vel; //ottengo il vettore engine isolato moltiplicando il vettore partizionato per engine_vel
         Vector3 thruster_vector = partition_vect(direction + 90) * thruster_vel; //ottengo il vettore thruster isolato moltiplicando il vettore partizionato per thruster_vel
         vel = engine_vector + thruster_vector; //sommo i due vettori e ottengo il vettore risultante
         
    }
    void update_vel(){ //calcola le velocita' e le mantiene nei limiti
         check_limits();
         get_real_vel();
    }
   
   
    void update_pos(){ //modifica la posizione del triangolo sullo schermo
         update_vel(); 
         triangle.MovePosition(new Vector3(triangle.position.x, triangle.position.y) + vel * Time.deltaTime);  //modifica la posizione
    }
    Vector3 get_mouse_pos(){ //ottiene la posizione del mouse (coordinate pixel su schermo) e ne converte le coordinate nella loro controparte in mondo di gioco
         return Camera.main.ScreenToWorldPoint(Input.mousePosition); 
    }
    void calculate_orientation(){ //calcola nuovo orientamento della nave in base al movimento del mouse e ne applica le modifiche alla prua 
         Vector3 mouse_pos = get_mouse_pos();
         Vector2 target = new Vector2(mouse_pos.x - triangle.position.x, mouse_pos.y - triangle.position.y); //Ottiene la nuova direzione che la nave deve assumere
         //float angle_coeff = (1 + min_rot_coeff) - remap_value(Vector2.Angle(triangle.transform.up,target), 0, 180, min_rot_coeff, 1); //coefficiente moltiplicativo rotazione(WORK IN PROGRESS)
         triangle.transform.up = Vector2.MoveTowards(triangle.transform.up, target, rot_vel * Time.deltaTime); //sposta linearmente la prua della nave sulla nuova direzione da assumere   
    }

    float distance(Vector3 obj1, Vector3 obj2){ //calcola la distanza tra due punti usando pitagora(funziona solo a due dimensioni)
        return Mathf.Sqrt(Mathf.Pow(obj1.x - obj2.x,2) + Mathf.Pow(obj1.y - obj2.y, 2));
    }

    float remap_value(float value, float from1, float from2, float to1, float to2){ //Rimappa la variabile value dai limiti from1, from2 ai limiti to1, to2 (Interpolazione lineare)
        return (((to2 - to1) * (value - from1)) / (from2 - from1)) + to1;  //x1 : x2 == (value - from1) : (x - to1)
    }

    void handle_inputs(){ //gestisce gli input dell'utente
         direction = normalize_orientation(triangle.rotation); //normalizza orientamento

         if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)){ //movimento avanti e indietro
             if (Input.GetKey(KeyCode.S)){ //indietro
                  engine_vel -= engine_acc; //decelerazione motore
             } else { //avanti
                  engine_vel += engine_acc; //accelerazione motore
             }  
         }

         if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)){ //movimento destra e sinistra
            if (Input.GetKey(KeyCode.D)){ 
                thruster_vel -= thruster_acc; //decelerazione thruster(verso destra)
            }else {   
                thruster_vel += thruster_acc; //accelerazione thruster(verso sinistra)
            }
         } else{
            if(flight_assist){ //azzerazione progressiva del thruster laterale
                thruster_vel = Mathf.Lerp(thruster_vel, 0f, flight_assist_efficiency * Time.deltaTime);
            }
         }
         //il mouse e' considerato come un input continuo 
    }
    void Start(){ //eseguito solo una volta
         //Cursor.visible = false;
    }
    void FixedUpdate(){ //aggiorna tutti gli oggetti legati alla fisica di gioco
         update_pos(); 
         calculate_orientation();   
    }

    void Update(){ //aggiorna il mondo di gioco generale
         handle_inputs(); 
         DEBUG_LOG();
    }
    
}
