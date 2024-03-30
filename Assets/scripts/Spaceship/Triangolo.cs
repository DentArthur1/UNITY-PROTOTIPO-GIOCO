using System;
using UnityEngine;
using UnityEngine.UI; //per il testo a schermo
using static Functions;

public class Triangolo : MonoBehaviour //COMANDI --> W/S Acc. Avanti e indientro | A/D Acc. Destra e Sinistra | MOUSE Rotazione destra e sinistra
{
    //RIFERIMENTI
    public Rigidbody2D triangle;  //oggetto nave
    public Functions fun = new Functions(); //per le funzioni ausiliarie
    public Proiettile arsenal = new Proiettile(); //riferimento per la classe Proiettile
    public Rigidbody2D proj_prefab; //Prefab proiettile

    //Struttura personalizzazione nave
    public ShipConfig ship_config; 
    //Testo a schermo
    public Text vector, engine, thruster, weapon, boost, lat_boost, boost_boolean, unboost_boolean, latboost_boolean, latunboost_boolean, flight_assist_boolean, FPS, MotorSwitch; //variabili testo
  
    void DEBUG_LOG(){ //messaggio di debug generale
        Debug.Log("Vector: " + ship_config.control_panel.vel + " / Engine_vel: " + ship_config.control_panel.engine_vel + " / Direction: " + ship_config.control_panel.direction + " / Thruster_vel: " + ship_config.control_panel.thruster_vel + " / Cooldown: " + ship_config.control_panel.wep_time);
    }

    void print_values() //Testo a schermo di valori (DEBUG A SCHERMO)
    {
        vector.text = "Vec: " + triangle.velocity;engine.text = "Eng: " + ship_config.control_panel.engine_vel; thruster.text = "Thr: " + ship_config.control_panel.thruster_vel; weapon.text = "Wep: " + ship_config.control_panel.wep_time; boost.text = "Boost: " + ship_config.control_panel.boost_time; lat_boost.text = "Lat Boost: " + ship_config.control_panel.lat_boost_time;
        boost_boolean.text = "Boost bool: " + ship_config.control_panel.boost_bool; unboost_boolean.text = "Unboost bool: " + ship_config.control_panel.unboost_bool; latboost_boolean.text = "Lat boost bool: " + ship_config.control_panel.lat_boost_bool; latunboost_boolean.text = "Lat unboost bool: " + ship_config.control_panel.lat_unboost_bool;
        flight_assist_boolean.text = "Flight assist: " + ship_config.control_panel.flight_assist; FPS.text = "FPS: " + 1f / Time.deltaTime; 
        MotorSwitch.text = "Motor Switch: " + ship_config.control_panel.motor_switch;
    }

    void check_limits(){ //Assicura che la velocitá rimanga nei limiti prestabiliti
        //MOTORE PRINCIPALE
        ship_config.control_panel.engine_vel = Mathf.Clamp(ship_config.control_panel.engine_vel, ship_config.Engine.min_vel, ship_config.Engine.max_vel + ship_config.control_panel.boost_increment); //attivando il booster, boost_increment aumenta il tetto massimo di engine_vel
        ship_config.control_panel.last_engine_vel = Mathf.Clamp(ship_config.control_panel.last_engine_vel, ship_config.Engine.min_vel, ship_config.Engine.max_vel); //limiti della memorizzazione dell'ultima velocita' engine prima dellla chiamata del boost
        //THRUSTERs LATERALI
        if (ship_config.control_panel.lat_boost_increment >= 0) //booster non attivo o attivo verso sinistra
        {
            ship_config.control_panel.thruster_vel = Mathf.Clamp(ship_config.control_panel.thruster_vel, -ship_config.Thruster.max_vel, ship_config.Thruster.max_vel + ship_config.control_panel.lat_boost_increment);
        } else //booster attivo verso destra
        {
            ship_config.control_panel.thruster_vel = Mathf.Clamp(ship_config.control_panel.thruster_vel, -ship_config.Thruster.max_vel + ship_config.control_panel.lat_boost_increment, ship_config.Thruster.max_vel);
        }
        ship_config.control_panel.last_thruster_vel = Mathf.Clamp(ship_config.control_panel.last_thruster_vel, -ship_config.Thruster.max_vel, ship_config.Thruster.max_vel); //limiti della memorizzazione dell'ultima velocita' thruster prima dellla chiamata del boost
        //Gli incrementi dei boost sono gestiti dalle funzioni boost e unboost

    }
    void get_real_vel(){ //somma i due vettori: Vettore motore principale + Vettore thruster laterale
         Vector3 engine_vector = fun.partition_vect(ship_config.control_panel.direction) * ship_config.control_panel.engine_vel; //ottengo il vettore engine isolato moltiplicando il vettore partizionato per engine_vel
         Vector3 thruster_vector = fun.partition_vect(ship_config.control_panel.direction + 90) * ship_config.control_panel.thruster_vel; //ottengo il vettore thruster isolato moltiplicando il vettore partizionato per thruster_vel
         ship_config.control_panel.vel = engine_vector + thruster_vector; //sommo i due vettori e ottengo il vettore risultante   
    }
   
    void update_vel(){ //modifica la posizione del triangolo sullo schermo
         get_real_vel(); //ottengo il vettore congiunto di forze
         triangle.AddForce(ship_config.control_panel.vel - new Vector3(triangle.velocity.x, triangle.velocity.y, 0)); //le forze congiunte di thruster e engine alla nave sottratte alla velocità attuale --> Per limitare l'aggiunta di forze e mantenere la velocità costante
    }
    Vector3 get_mouse_pos(){ //ottiene la posizione del mouse (coordinate pixel su schermo) e ne converte le coordinate nella loro controparte in mondo di gioco
         return Camera.main.ScreenToWorldPoint(Input.mousePosition); 
    }
    void calculate_orientation(){ //calcola nuovo orientamento della nave in base al movimento del mouse e ne applica le modifiche alla prua 
         ship_config.control_panel.direction = fun.normalize_orientation(triangle.rotation); //normalizza orientamento
         Vector3 mouse_pos = get_mouse_pos();
         Vector2 target = new Vector2(mouse_pos.x - triangle.position.x, mouse_pos.y - triangle.position.y); //Ottiene la nuova direzione che la nave deve assumere
         triangle.transform.up = Vector2.MoveTowards(triangle.transform.up, target, ship_config.misc.rot_vel * Time.deltaTime); //sposta linearmente la prua della nave sulla nuova direzione da assumere   
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
            if ((engine_max + Mathf.Abs(boost_target)).CompareTo(Mathf.Abs(engine)) == 0) //se il booster ha raggiunto il target, smetti di incrementare
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
                if (engine.CompareTo(last_engine) != 0) //se devo ancora decrementarla 
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
    void manage_boosters() //Gestisce i booster thruster e engine
    {
        //BOOSTER PRINCIPALE
        check_boost(ref ship_config.control_panel.boost_bool, ref ship_config.control_panel.engine_vel, ref ship_config.control_panel.boost_increment, ref ship_config.control_panel.unboost_bool, ship_config.Engine.max_vel, ship_config.Engine.boost_target_offset, ship_config.Engine.boost_acc); //Gestione del booster principale
        check_unboost(ref ship_config.control_panel.unboost_bool, ref ship_config.control_panel.engine_vel, ref ship_config.control_panel.boost_increment, ship_config.Engine.boost_dec, ref ship_config.control_panel.last_engine_vel, true); //Gestione del de-Booster principale
        fun.timing(ref ship_config.control_panel.boost_time, ship_config.Engine.boost_cooldown);//aggiorna il timer booster principale
        //BOOSTER LATERALE
        check_boost(ref ship_config.control_panel.lat_boost_bool, ref ship_config.control_panel.thruster_vel, ref ship_config.control_panel.lat_boost_increment, ref ship_config.control_panel.lat_unboost_bool, ship_config.Thruster.max_vel, ship_config.control_panel.lat_boost_target_offset, ship_config.Thruster.boost_acc);
        check_unboost(ref ship_config.control_panel.lat_unboost_bool, ref ship_config.control_panel.thruster_vel, ref ship_config.control_panel.lat_boost_increment, ship_config.Thruster.boost_dec, ref ship_config.control_panel.last_thruster_vel, false); //Gestione del de-Booster principale
        fun.timing(ref ship_config.control_panel.lat_boost_time, ship_config.Thruster.boost_cooldown);
    }
    void manage_weapons() //Gestisce l'arsenale della nave
    {
        fun.timing(ref ship_config.control_panel.wep_time, ship_config.projectile.cooldown_time);//aggiorna il timer weapon
    }

    void engine_control() //gestisce input per engine principale
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        { //movimento avanti e indietro
            if (Input.GetKey(KeyCode.S))
            { //indietro
                ship_config.control_panel.engine_vel -= ship_config.Engine.acc * Time.deltaTime; //decelerazione motore
                ship_config.control_panel.last_engine_vel -= ship_config.Engine.acc * Time.deltaTime; //usato per la sequenza di de-boost
            }
            else
            { //avanti
                ship_config.control_panel.engine_vel += ship_config.Engine.acc * Time.deltaTime; //accelerazione motore
                ship_config.control_panel.last_engine_vel += ship_config.Engine.acc * Time.deltaTime; //usato per la sequenza di de-boost
            }
        }
        else
        {
            if (ship_config.control_panel.flight_assist && ship_config.control_panel.engine_vel >= -ship_config.Engine.dead_zone && ship_config.control_panel.engine_vel <= ship_config.Engine.dead_zone) // azzerazione progressiva engine_vel se compreso nell'intervallo della dead_zone
            {
                ship_config.control_panel.engine_vel = Mathf.MoveTowards(ship_config.control_panel.engine_vel, 0, ship_config.misc.flight_assist_efficiency * Time.deltaTime);
            }
        }
    }

    void thruster_control() //gestisce input per i thruster laterali
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        { //movimento destra e sinistra
            if (Input.GetKey(KeyCode.D))
            {
                ship_config.control_panel.thruster_vel -= ship_config.Thruster.acc * Time.deltaTime; //decelerazione thruster(verso destra)
                ship_config.control_panel.last_thruster_vel -= ship_config.Thruster.acc * Time.deltaTime; //usato per la sequenza di de-boost laterale
            }
            else
            {
                ship_config.control_panel.thruster_vel += ship_config.Thruster.acc * Time.deltaTime; //accelerazione thruster(verso sinistra)
                ship_config.control_panel.last_thruster_vel += ship_config.Thruster.acc * Time.deltaTime; //usato per la sequenza di de-boost laterale
            }
        }
        else
        {
            if (ship_config.control_panel.flight_assist && !ship_config.control_panel.lat_boost_bool)
            { //azzerazione progressiva del thruster laterale(se il booster laterale non e' attivo)
                ship_config.control_panel.thruster_vel = Mathf.MoveTowards(ship_config.control_panel.thruster_vel, 0, ship_config.misc.flight_assist_efficiency * Time.deltaTime);
            }
        }
    }

    void booster_control() //gestisce gli input per i booster
    {
        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.D)) //azione boost laterale destro / sinistro
        {
            if (fun.timing(ref ship_config.control_panel.lat_boost_time, ship_config.Thruster.boost_cooldown)) //controllo se il tempo necessario per il cooldown e' passato
            {
                ship_config.control_panel.lat_boost_bool = true; //attivo la sequenza di  boost laterale e' attivo
                ship_config.control_panel.last_thruster_vel = Mathf.Clamp(ship_config.control_panel.thruster_vel, -ship_config.Thruster.max_vel, ship_config.Thruster.max_vel); //memorizzo l'ultima velocita' laterale prima del boost, e mi assicuro che sia nei limiti
                ship_config.control_panel.lat_boost_time -= Time.deltaTime; //attivo il timer
                if (Input.GetKey(KeyCode.D)) //booster laterale destro(negativo)
                {
                    ship_config.control_panel.lat_boost_target_offset = -ship_config.Thruster.boost_target_offset; //offset negativo
                }
                else
                { //boster laterale sinistro(positivo)
                    ship_config.control_panel.lat_boost_target_offset = ship_config.Thruster.boost_target_offset; //offset positivo
                }
            }

        }
        else if (Input.GetKey(KeyCode.Space)) //azione boost motore
        {
            if (fun.timing(ref ship_config.control_panel.boost_time, ship_config.Engine.boost_cooldown)) //se il timer e passato e non sono ancora oltre la velocita' massima
            {
                ship_config.control_panel.boost_bool = true;//indico al resto dello script che il boost e' attivo
                ship_config.control_panel.last_engine_vel = Mathf.Clamp(ship_config.control_panel.engine_vel, ship_config.Engine.min_vel, ship_config.Engine.max_vel); //memorizzo l'ultima velocita' del motore e mi assicuro che sia nei limiti
                ship_config.control_panel.boost_time -= Time.deltaTime; //attivo il timer
            }
        }
    }
    void weapon_control() //gestisce i fire groups
    {
        if (Input.GetMouseButton(0)) //azione fuoco
        {
            if (fun.timing(ref ship_config.control_panel.wep_time, ship_config.projectile.cooldown_time)) //se il cooldown e' passato, la navicella puo sparare
            {
                arsenal.shoot_bullet(triangle.transform.position, ship_config.control_panel.direction, triangle.transform.rotation, proj_prefab, ship_config.projectile); //spara il proiettile
                ship_config.control_panel.wep_time -= Time.deltaTime; //dice alla funzione timing di iniziare il conteggio
            }
        }
    }

    void misc_control() //gestisce gli input misc
    {
        if (Input.GetKeyDown(KeyCode.F)) //attivazione disattivazione flight assist
        {
            ship_config.control_panel.flight_assist = !ship_config.control_panel.flight_assist;
        }
        if (Input.GetKeyDown(KeyCode.Z)) //Accensione spegnimento motore
        {
            ship_config.control_panel.motor_switch = !ship_config.control_panel.motor_switch;
        }
    }
    void handle_inputs(){ //gestisce gli input dell'utente
         if(ship_config.control_panel.motor_switch) //se il motore e' acceso, tengo conto degli input
         {
            engine_control();
            thruster_control();
            booster_control();
         }
         //Controlli a motore spento: Accensione e flight_assist
         misc_control();
         weapon_control();
         check_limits(); //limito i valori raccolti come input
    }

    void Start(){ //eseguito solo una volta
        ship_config.control_panel.wep_time = ship_config.projectile.cooldown_time; //settaggio timer cooldown a bullet_cooldown_time
        ship_config.control_panel.boost_time = ship_config.Engine.boost_cooldown; //settaggio timer cooldown a boost_cooldown
        ship_config.control_panel.lat_boost_time = ship_config.Thruster.boost_cooldown; //settaggio timer cooldown a lat_boost_cooldown
    }
    void FixedUpdate(){ //aggiorna tutti gli oggetti legati alla fisica di gioco
         if (ship_config.control_panel.motor_switch) //se il motore e' acceso, aggiorno la velocità
         {
            update_vel();
         }
         calculate_orientation();
    }

    void Update(){ //aggiorna il mondo di gioco generale
         handle_inputs();
         manage_boosters();
         manage_weapons();
         print_values();
    }


    
}

