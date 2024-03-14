using System;
using UnityEngine;
using UnityEngine.UI; //per il testo a schermo

public class Triangolo : MonoBehaviour //COMANDI --> W/S Acc. Avanti e indientro | A/D Acc. Destra e Sinistra | MOUSE Rotazione destra e sinistra
{
    //RIFERIMENTI
    public Rigidbody2D triangle;  //oggetto nave
    public Functions fun; //per le funzioni ausiliarie
    public Transform bullet_reference; //riferimento per la classe Proiettile
    //ENGINE
    public float engine_acc;  //accelerazione spostamento generale
    public float engine_max_vel; //velocita' spostamento massima
    public float engine_min_vel; //velocita' massima di retromarcia
    public float engine_vel = 0; //velocita' corrente della nave
    public float engine_dead_zone; //intervallo di velocita' nel quale il flight_assist opera per automaticamente azzerare la velocita'
    private float last_engine_vel = 0; //velocita' engine al momento dell'attivazione del boost(per tornare a quel punto dopo che il booster ha finito)
    //BOOSTER
    public float boost_target_offset; //offset di velocita' massimo ottenuto con il boost
    private float boost_increment = 0; //incremento attuale alla velocita' massima
    public float boost_acc; //accelerazione boost(velocita' con cui raggiunge boost_target_offset)
    public float boost_dec; //decelerazione boost(velocita' con cui il boost termina i suoi effetti)
    public float boost_cooldown; //tempo di cooldown per il booster
    private float boost_time; //variabile timer booster principale
    private Boolean boost_bool = false; //booleano che indica se il boost e' attivo
    private Boolean unboost_bool = false; //Booleano che indica se il de-boost e' attivo
    //LATERAL BOOSTER
    public float lat_boost_targ_offset; //offset di velocita' laterale massimo ottenuto con il boost
    private float lat_boost_target_offset; //offset di velocita' laterale massimo attuale(puo essere positivo o negativo)
    private float lat_boost_increment = 0; //incremento attuale alla velocita' laterale massima
    public float lat_boost_acc; //accelerazione boost laterale(velocita' con cui raggiunge lat_boost_target_offset)
    public float lat_boost_dec; //decelerazione boost laterlae(velocita' con cui il boost laterale termina i suoi effetti)
    public float lat_boost_cooldown; //tempo di cooldown per il booster laterale
    private float lat_boost_time; //variabile timer booster laterale
    private Boolean lat_boost_bool = false; //booleano che indica se il boost laterale e' attivo
    private Boolean lat_unboost_bool = false; //booleano che indica se il de-boost laterale e' attivo
    //THRUSTER
    public float thruster_acc; //accelerazione thruster laterali
    public float thruster_vel = 0; //velocita' corrente di spostamento laterale
    public float max_thruster_vel; //velocita' di spostamento laterale massima
    private float last_thruster_vel; //usata per il booster laterale
    //DIRECTION
    public float rot_vel; //velocita' rotazione nave
    public Vector3 vel;   //Vettore spostamento nave 
    public Boolean flight_assist; //azzeramento automatico thruster e rotazione
    public float flight_assist_efficiency; //efficienza meccanismo flight assist
    public float direction; //variabile direzione nave(angolo)
    public float ship_drag_coeff; //coefficiente drag della nave nello spazio (responsivita' ai cambi di velocita', valori minori minore responsivita')
    //Proiettile
    public float bullet_cooldown_time; //tempo di cooldown tra un proiettile e l'altro(non troppo piccolo)
    private float wep_time; //tempo prima che si puo' sparare
    //Testo a schermo
    public Text vector, engine, thruster, weapon, boost, lat_boost, boost_boolean, unboost_boolean, latboost_boolean, latunboost_boolean, flight_assist_boolean, FPS; //variabili testo
    //ANIMAZIONI


    void DEBUG_LOG(){ //messaggio di debug generale
        Debug.Log("Vector: " + vel + " / Engine_vel: " + engine_vel + " / Direction: " + direction + " / Thruster_vel: " + thruster_vel + " / Cooldown: " + wep_time);
    }

    void print_values() //Testo a schermo di valori (DEBUG A SCHERMO)
    {
        vector.text = "Vec: " + triangle.velocity;engine.text = "Eng: " + engine_vel; thruster.text = "Thr: " + thruster_vel; weapon.text = "Wep: " + wep_time; boost.text = "Boost: " + boost_time; lat_boost.text = "Lat Boost: " + lat_boost_time;
        boost_boolean.text = "Boost bool: " + boost_bool; unboost_boolean.text = "Unboost bool: " + unboost_bool; latboost_boolean.text = "Lat boost bool: " + lat_boost_bool; latunboost_boolean.text = "Lat unboost bool: " + lat_unboost_bool;
        flight_assist_boolean.text = "Flight assist: " + flight_assist; FPS.text = "FPS: " + 1f / Time.deltaTime;
    }

    void check_limits(){ //Assicura che la velocitá rimanga nei limiti prestabiliti
        //MOTORE PRINCIPALE
        engine_vel = Mathf.Clamp(engine_vel, engine_min_vel, engine_max_vel + boost_increment); //attivando il booster, boost_increment aumenta il tetto massimo di engine_vel
        last_engine_vel = Mathf.Clamp(engine_vel, engine_min_vel, engine_max_vel); //limiti della memorizzazione dell'ultima velocita' engine prima dellla chiamata del boost
        //THRUSTERs LATERALI
        if (lat_boost_increment >= 0) //booster non attivo o attivo verso sinistra
        {
            thruster_vel = Mathf.Clamp(thruster_vel, -max_thruster_vel, max_thruster_vel + lat_boost_increment);
        } else //booster attivo verso destra
        {
            thruster_vel = Mathf.Clamp(thruster_vel, -max_thruster_vel + lat_boost_increment, max_thruster_vel);
        }
        last_thruster_vel = Mathf.Clamp(thruster_vel, -max_thruster_vel, max_thruster_vel); //limiti della memorizzazione dell'ultima velocita' thruster prima dellla chiamata del boost
        //BOOSTER MOTORE
        boost_increment = Mathf.Clamp(boost_increment, 0, boost_target_offset);
    }
    void get_real_vel(){ //somma i due vettori: Vettore motore principale + Vettore thruster laterale
         Vector3 engine_vector = fun.partition_vect(direction) * engine_vel; //ottengo il vettore engine isolato moltiplicando il vettore partizionato per engine_vel
         Vector3 thruster_vector = fun.partition_vect(direction + 90) * thruster_vel; //ottengo il vettore thruster isolato moltiplicando il vettore partizionato per thruster_vel
         vel = engine_vector + thruster_vector; //sommo i due vettori e ottengo il vettore risultante   
    }
   
    void update_pos(){ //modifica la posizione del triangolo sullo schermo
         get_real_vel();
         triangle.velocity = Vector3.MoveTowards(triangle.velocity, vel, ship_drag_coeff * Time.deltaTime); //applico il drag alla nave
    }
    Vector3 get_mouse_pos(){ //ottiene la posizione del mouse (coordinate pixel su schermo) e ne converte le coordinate nella loro controparte in mondo di gioco
         return Camera.main.ScreenToWorldPoint(Input.mousePosition); 
    }
    void calculate_orientation(){ //calcola nuovo orientamento della nave in base al movimento del mouse e ne applica le modifiche alla prua 
         Vector3 mouse_pos = get_mouse_pos();
         Vector2 target = new Vector2(mouse_pos.x - triangle.position.x, mouse_pos.y - triangle.position.y); //Ottiene la nuova direzione che la nave deve assumere
         triangle.transform.up = Vector2.MoveTowards(triangle.transform.up, target, rot_vel * Time.deltaTime); //sposta linearmente la prua della nave sulla nuova direzione da assumere   
    }
    
    void check_boost(ref Boolean boost, ref float engine, ref float boost_inc, ref Boolean unboost, float engine_max, float boost_target, float boost_acc) //attiva il booster della nave(gestione incremento del boost della nave)
    {
        if (boost) //se il booster e' in fase di accelerazione continuo a incrementare
        {
            boost_inc = Mathf.MoveTowards(boost_inc, boost_target, boost_acc * Time.deltaTime); //incrementa il valore attuale del booster
            if (boost_inc >= 0) // boost verso sinistra o boost principale, limite massimo di velocita' == engine_max + boost_inc
            {
                engine = Mathf.MoveTowards(engine, engine_max + boost_inc, boost_acc * Time.deltaTime);//incrementa la velocita' del motore
            }else // boost verso destra, limite massimo di velocita' == -engine_max + boost_inc
            {
                engine = Mathf.MoveTowards(engine, -engine_max + boost_inc, boost_acc * Time.deltaTime);//incrementa la velocita' del motore
            }
            if (engine_max + Mathf.Abs(boost_target) == Mathf.Abs(engine)) //se il booster ha raggiunto il target, smetti di incrementare(- 0.2f dovuto al fatto che il lerp non raggiunge mai esattamente il target in questa situazione)
            {
                boost = false; //spegni il booster
                unboost = true; //attiva sequenza di de-boost;
            }
        }    
    }

    void check_unboost(ref Boolean unboost, ref float engine, ref float boost_inc, float boost_dec,ref float last_engine, Boolean eng_mode) //gestione del decremento del booster
    {
        if (unboost) //booster in fase di decremento
        {
            if (boost_inc != 0) //se la velocita' del booster e' ancora maggiore di 0
            {
                boost_inc = Mathf.MoveTowards(boost_inc, 0, boost_dec * Time.deltaTime); //decrementa il boost attuale
            }
            else //il booster ha smesso di decrementarsi, devo riportare la velocita' a quella al momento dell'attivazione del booster
            {
                if (engine == last_engine) //se devo ancora decrementarla (0.1f e' semplicemente l'area di successo)
                {
                    if(last_engine < 0 && eng_mode) //se attivo il boost quando sono in retromarcia, devo riportare la nave a una velocita' pari a 0
                    {
                        last_engine = 0;
                    }
                    engine = Mathf.MoveTowards(engine, last_engine, boost_dec * Time.deltaTime); //decremento velocita' fino al momento di attivazione
                } else
                {
                    unboost = false; //la velocita' ha raggiunto il punto di prima, termino la sequenza di de-boost
                }
            }      
        }
    }

    void handle_inputs(){ //gestisce gli input dell'utente
         direction = fun.normalize_orientation(triangle.rotation); //normalizza orientamento
         if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)){ //movimento avanti e indietro
             if (Input.GetKey(KeyCode.S)){ //indietro
                  engine_vel -= engine_acc * Time.deltaTime; //decelerazione motore
                  last_engine_vel -= engine_acc * Time.deltaTime; //usato per la sequenza di de-boost
             } else { //avanti
                  engine_vel += engine_acc * Time.deltaTime; //accelerazione motore
                  last_engine_vel += engine_acc * Time.deltaTime; //usato per la sequenza di de-boost
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
                thruster_vel -= thruster_acc * Time.deltaTime; //decelerazione thruster(verso destra)
                last_thruster_vel -= thruster_acc * Time.deltaTime; //usato per la sequenza di de-boost laterale
            }else {   
                thruster_vel += thruster_acc * Time.deltaTime; //accelerazione thruster(verso sinistra)
                last_thruster_vel += thruster_acc * Time.deltaTime; //usato per la sequenza di de-boost laterale
            }
         }else{
            if(flight_assist && !lat_boost_bool){ //azzerazione progressiva del thruster laterale(se il booster laterale non e' attivo)
                thruster_vel = Mathf.MoveTowards(thruster_vel, 0, flight_assist_efficiency * Time.deltaTime);
            }
         }

         if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.D)) //azione boost laterale destro / sinistro
         {
            if(fun.timing(ref lat_boost_time, lat_boost_cooldown)) //controllo se il tempo necessario per il cooldown e' passato
            {
                lat_boost_bool = true; //attivo la sequenza di  boost laterale e' attivo
                last_thruster_vel = Mathf.Clamp(thruster_vel, -max_thruster_vel, max_thruster_vel); //memorizzo l'ultima velocita' laterale prima del boost, e mi assicuro che sia nei limiti
                lat_boost_time -= Time.deltaTime; //attivo il timer
                if (Input.GetKey(KeyCode.D)) //booster laterale destro(negativo)
                {
                    lat_boost_target_offset = -lat_boost_targ_offset; //offset negativo
                }else
                { //boster laterale sinistro(positivo)
                    lat_boost_target_offset = lat_boost_targ_offset; //offset positivo
                }
            }
           
         }else  if (Input.GetKey(KeyCode.Space)) //azione boost motore
         {
            if(fun.timing(ref boost_time, boost_cooldown)) //se il timer e passato e non sono ancora oltre la velocita' massima
            {
                boost_bool = true;//indico al resto dello script che il boost e' attivo
                last_engine_vel = Mathf.Clamp(engine_vel, engine_min_vel, engine_max_vel); //memorizzo l'ultima velocita' del motore e mi assicuro che sia nei limiti
                boost_time -= Time.deltaTime; //attivo il timer
            }
         }


         if (Input.GetMouseButton(0)) //azione fuoco
         {
            if (fun.timing(ref wep_time,bullet_cooldown_time)) //se il cooldown e' passato, la navicella puo sparare
            {
                bullet_reference.GetComponent<Proiettile>().shoot_bullet(); //spara il proiettile
                wep_time -= Time.deltaTime; //dice alla funzione timing di iniziare il conteggio
            }
         }

         if (Input.GetKeyDown(KeyCode.F)) //attivazione disattivazione flight assist
         {
            flight_assist = !flight_assist;
         }
         check_limits(); 
         //BOOSTER PRINCIPALE
         check_boost(ref boost_bool,ref engine_vel,ref boost_increment,ref unboost_bool, engine_max_vel, boost_target_offset, boost_acc); //Gestione del booster principale
         check_unboost(ref unboost_bool, ref engine_vel,ref boost_increment, boost_dec,ref last_engine_vel, true); //Gestione del de-Booster principale
         fun.timing(ref boost_time, boost_cooldown);//aggiorna il timer booster principale
         //BOOSTER LATERALE
         check_boost(ref lat_boost_bool, ref thruster_vel, ref lat_boost_increment, ref lat_unboost_bool, max_thruster_vel, lat_boost_target_offset, lat_boost_acc);
         check_unboost(ref lat_unboost_bool, ref thruster_vel, ref lat_boost_increment, lat_boost_dec, ref last_thruster_vel, false); //Gestione del de-Booster principale
         fun.timing(ref lat_boost_time, lat_boost_cooldown);
         //WEAPONS
         fun.timing(ref wep_time, bullet_cooldown_time);//aggiorna il timer weapon
    }

    void Start(){ //eseguito solo una volta
        wep_time = bullet_cooldown_time; //settaggio timer cooldown a bullet_cooldown_time
        boost_time = boost_cooldown; //settaggio timer cooldown a boost_cooldown
        lat_boost_time = lat_boost_cooldown; //settaggio timer cooldown a lat_boost_cooldown
        fun = new Functions(); //inizializzo la classe delle funzioni ausiliarie
    }
    void FixedUpdate(){ //aggiorna tutti gli oggetti legati alla fisica di gioco
         update_pos(); 
         calculate_orientation();
        
    }

    void Update(){ //aggiorna il mondo di gioco generale
         handle_inputs(); 
         //DEBUG_LOG();
         print_values();
    }


    
}

