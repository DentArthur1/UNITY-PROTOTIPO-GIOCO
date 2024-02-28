using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI; //per il testo a schermo

public class Triangolo : MonoBehaviour //COMANDI --> W/S Acc. Avanti e indientro | A/D Acc. Destra e Sinistra | MOUSE Rotazione destra e sinistra
{
    public Rigidbody2D triangle;  //oggetto nave
    public Functions fun; //per le funzioni ausiliarie
    public Transform bullet_reference; //riferimento per la classe Proiettile
    public float direction;   //variabile direzione nave (pubblico per essere visto dalla classe proiettile)
    public float engine_acc;  //accelerazione spostamento generale
    public float engine_max_vel; //velocita' spostamento massima
    public float engine_min_vel; //velocita' massima di retromarcia
    private float engine_vel = 0; //velocita' corrente della nave
    public float engine_dead_zone; //intervallo di velocita' nel quale il flight_assist opera per automaticamente azzerare la velocita'
    private float last_engine_vel = 0; //velocita' engine al momento dell'attivazione del boost(per tornare a quel punto dopo che il booster ha finito)
    private float thruster_vel = 0; //velocita' corrente di spostamento laterale
    public float max_thruster_vel; //velocita' di spostamento laterale massima
    public float boost_target_offset; //velocita' massima ottenuta con il boost
    private float boost_increment = 0; //incremento attuale alla velocita' massima
    public float boost_acc; //accelerazione boost(velocita' con cui raggiunge boost_target)
    public float boost_dec; //decelerazione boost(velocita' con cui il boost termina i suoi effetti)
    public float boost_cooldown; //tempo di cooldown per il booster
    private float boost_time; //tempo prima che si puo' usare il boost
    private Boolean boost_bool = false; //booleano che indica se il boost e' attivo
    private Boolean unboost_bool = false; //Booleano ceh indica se il de-boost e' attivo
    public float thruster_acc; //accelerazione thruster laterali
    public float rot_vel; //velocita' rotazione nave
    private Vector3 vel;   //Vettore spostamento nave 
    public float min_rot_coeff; //valore da 0 a 1 escluso, per limitare quanto lentamente la nave deve ruotare nei grandi cambiamenti di rotta
    public Boolean flight_assist; //azzeramento automatico thruster e rotazione
    public float flight_assist_efficiency; //efficienza meccanismo flight assist
    //Proiettile
    public float bullet_cooldown_time; //tempo di cooldown tra un proiettile e l'altro(non troppo piccolo)
    private float wep_time; //tempo prima che si puo' sparare
    //Testo a schermo
    public Text vector, engine, dir, thruster, weapon, boost;

    float normalize_orientation(float angle){ //notazione classica per calcolo seno e coseno
        angle += 90;
        if (angle < 0){
             angle = 360 - math.abs(angle); //(normalizza da 0 a 360 gradi)
        } 
        return angle;
    }
    void DEBUG_LOG(){ //messaggio di debug generale
        Debug.Log("Vector: " + vel + " / Engine_vel: " + engine_vel + " / Direction: " + direction + " / Thruster_vel: " + thruster_vel + " / Cooldown: " + wep_time);
    }

    void print_values() //Testo a schermo di valori (DEBUG A SCHERMO)
    {
        vector.text = "Vec: " + vel;engine.text = "Eng: " + engine_vel; dir.text = "Dir: " + direction; thruster.text = "Thr: " + thruster_vel; weapon.text = "Wep: " + wep_time; boost.text = "Boost: " + boost_time;
    }

    public Vector3 partition_vect(float angle){ //partiziona il vettore in base all'orientamento della nave (public per essere usato dalla classe proiettile)
        Vector3 vector;vector.z = 0;
        vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);    
        vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        return vector;
    }
    void check_limits(){ //Assicura che la velocitá rimanga nei limiti prestabiliti
        //MOTORE PRINCIPALE
        engine_vel = Mathf.Clamp(engine_vel, engine_min_vel, engine_max_vel + boost_increment); //attivando il booster, boost_increment aumenta il tetto massimo di engine_vel
        //THRUSTERs LATERALI
        thruster_vel = Mathf.Clamp(thruster_vel, -max_thruster_vel, max_thruster_vel);
        //BOOSTER 
        boost_increment = Mathf.Clamp(boost_increment, 0, boost_target_offset);
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
         triangle.transform.up = Vector2.MoveTowards(triangle.transform.up, target, rot_vel * Time.deltaTime); //sposta linearmente la prua della nave sulla nuova direzione da assumere   
    }

    void check_boost() //attiva il booster della nave(gestione incremento del boost della nave)
    {
        if (boost_bool) //se il booster e' in fase di accelerazione continuo a incrementare
        {
            engine_vel = Mathf.Lerp(engine_vel, engine_max_vel + boost_target_offset, boost_acc * Time.deltaTime);//incrementa la velocita' del motore
            boost_increment = Mathf.Lerp(boost_increment, boost_target_offset, boost_acc * Time.deltaTime); //incrementa il valore attuale del booster
            if (engine_vel >= engine_max_vel + boost_target_offset - 0.5f) //se il booster ha raggiunto il target, smetti di incrementare(- 0.5f dovuto al fatto che il lerp non raggiunge mai esattamente il target in questa situazione)
            {
                boost_bool = false; //spegni il booster
                unboost_bool = true; //attiva sequenza di de-boost;
            }
        }    
    }

    void check_unboost() //gestione del decremento del booster
    {
        if (unboost_bool) //booster in fase di decremento
        {
            if (boost_increment > 0) //se il booster e' ancora in fase di decremento
            {
                boost_increment = Mathf.MoveTowards(boost_increment, 0, boost_dec * Time.deltaTime); //decrementa il boost attuale
            }
            else //il booster ha smesso di decrementarsi, devo riportare la velocita' a quella al momento dell'attivazione del booster
            {
                if (engine_vel > last_engine_vel) //se devo ancora decrementarla
                {
                    if(last_engine_vel < 0) //se attivo il boost quando sono in retromarcia, devo riportare la nave a una velocita' pari a 0
                    {
                        last_engine_vel = 0;
                    }
                    engine_vel = Mathf.MoveTowards(engine_vel, last_engine_vel, boost_dec * Time.deltaTime); //decremento velocita' fino al momento di attivazione
                } else
                {
                    unboost_bool = false; //la velocita' ha raggiunto il punto di prima, termino la sequenza di de-boost
                }
            }      
        }
    }

    void handle_inputs(){ //gestisce gli input dell'utente
         direction = normalize_orientation(triangle.rotation); //normalizza orientamento

         if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)){ //movimento avanti e indietro
             if (Input.GetKey(KeyCode.S)){ //indietro
                  engine_vel -= engine_acc; //decelerazione motore
                  last_engine_vel -= engine_acc; //usato per la sequenza di de-boost(TEMPORANEO)
             } else { //avanti
                  engine_vel += engine_acc; //accelerazione motore
                  last_engine_vel += engine_acc; //usato per la sequenza di de-boost(TEMPORANEO)
            }  
         } else
            {
                if(flight_assist && engine_vel >= -engine_dead_zone && engine_vel <= engine_dead_zone) // azzerazione progressiva engine_vel se compreso nell'intervallo della dead_zone
                {
                    engine_vel = Mathf.MoveTowards(engine_vel, 0, flight_assist_efficiency * Time.deltaTime);
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
                thruster_vel = Mathf.MoveTowards(thruster_vel, 0, flight_assist_efficiency * Time.deltaTime);
            }
         }

        if (Input.GetKey(KeyCode.Space)) //azione boost motore
        {
            if(timing(ref boost_time, boost_cooldown)) //se il timer e passato e non sono ancora oltre la velocita' massima
            {
                boost_bool = true;//indico al resto dello script che il boost e' attivo
                last_engine_vel = engine_vel;
                boost_time -= Time.deltaTime; //attivo il timer
            }
        }

         if (Input.GetMouseButton(0)) //azione fuoco
         {
            if (timing(ref wep_time,bullet_cooldown_time)) //se il cooldown e' passato, la navicella puo sparare
            {
                bullet_reference.GetComponent<Proiettile>().shoot_bullet(); //spara il proiettile
                wep_time -= Time.deltaTime; //dice alla funzione timing di iniziare il conteggio
            }
         }
         check_boost(); //Gestione del booster
         check_unboost(); //Gestione del de-Booster
         timing(ref wep_time, bullet_cooldown_time);//aggiorna il timer weapon
         timing(ref boost_time, boost_cooldown);//aggiorna il timer booster
    }
    Boolean timing(ref float time, float timer) //ritorna true se il timer non e' stato ancora inizializzato
    {
        if(time == timer)//il timer non e' stato ancora inizializzato
        {
            return true;

        } else //il timer e stato inizializzato(ovvero time e' stato diminuito)
        {
            time -= Time.deltaTime; 
            if (time <= 0) { //se il timer ha raggiunto o superato 0, lo resetto
                time = timer;
            }
            return false;
        }

    }
    void Start(){ //eseguito solo una volta
        wep_time = bullet_cooldown_time; //settaggio timer cooldown a bullet_cooldown_time
        boost_time = boost_cooldown; //settaggio timer cooldown a boost_cooldown
    }
    void FixedUpdate(){ //aggiorna tutti gli oggetti legati alla fisica di gioco
         update_pos(); 
         calculate_orientation();   
    }

    void Update(){ //aggiorna il mondo di gioco generale
         handle_inputs(); 
         DEBUG_LOG();
         print_values();
    }


    
}

