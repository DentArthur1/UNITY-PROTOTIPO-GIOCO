using System;
using Unity.Mathematics;
using UnityEngine;

public class Triangolo : MonoBehaviour //COMANDI --> W/S Avanti Indietro | A/D Sinistra Destra | Q/E Rotazione destra e sinistra
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
    private Vector2 partial_rot; //vettore rotazione parziale nave
    public Boolean flight_assist; //azzeramento automatico thruster e rotazione
    public float mouse_dz_radius; //zona morta azione mouse
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
    void calculate_orientation(){ //calcola nuovo orientamento della nave in base al movimento del mouse
         Vector3 mouse_pos = Input.mousePosition; //Ottiene posizione del mouse (Coordinate (x,y) sullo schermo)
         mouse_pos = Camera.main.ScreenToWorldPoint(mouse_pos); //Converte la posizione relativa del mouse sullo schermo in un punto del mondo di gioco

         if(distance(triangle.transform.position, mouse_pos) >= mouse_dz_radius){ //se la posizione del mouse si trova a una distanza maggiore di mouse_dz_radius allora modifica la rotazione
               Vector2 target = new Vector2(mouse_pos.x - triangle.position.x, mouse_pos.y - triangle.position.y); //Ottiene la nuova direzione che la nave deve assumere
               partial_rot = Vector2.Lerp(partial_rot, target, rot_vel * Time.deltaTime); //interpola linearmente tra i due vettori, per ottenere un effetto "smooth" della rotazione
               triangle.transform.up = partial_rot; //modifica la rotazione della nave con il valore parziale interpolato
         }
         
    }

    float distance(Vector3 obj1, Vector3 obj2){ //calcola la distanza tra due punti usando pitagora
        float cat1 = obj1.x - obj2.x;
        float cat2 = obj1.y - obj2.y;
        float distance = Mathf.Sqrt(Mathf.Pow(cat1,2) + Mathf.Pow(cat2, 2));
        return distance;
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
