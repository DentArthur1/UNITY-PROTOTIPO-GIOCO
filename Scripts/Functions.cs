using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Functions //CLASSE FUNZIONI DI SUPPORTO
{
    //FUNZIONI AUSILIARIE
    public float distance(Vector3 obj1, Vector3 obj2)
    { //calcola la distanza tra due punti usando pitagora(funziona solo a due dimensioni)
        return Mathf.Sqrt(Mathf.Pow(obj1.x - obj2.x, 2) + Mathf.Pow(obj1.y - obj2.y, 2));
    }

    public float remap_value(float value, float from1, float from2, float to1, float to2)
    { //Rimappa la variabile value dai limiti from1, from2 ai limiti to1, to2 (Interpolazione lineare)
        return to1 + (to2 - to1) * (value - from1) / (from2 - from1);  //x1 : x2 == (value - from1) : (x - to1)
    }

    public Boolean timing(ref float time, float timer) //ritorna true se il timer non e' stato ancora inizializzato
    {
        //funzionamento: 1)chiamata funzione timing 2)decremento time 3)chiamo la funzione in update per far proseguire il timer(time = scorrimento tempo attuale, timer = tempo totale timer)
        if (time == timer)//il timer non e' stato ancora inizializzato
        {
            return true;

        }
        else //il timer e stato inizializzato(ovvero time e' stato diminuito)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            { //se il timer ha raggiunto o superato 0, lo resetto
                time = timer;
            }
            return false;
        }
    }

    public Vector3 partition_vect(float angle)
    { //partiziona il vettore in base all'orientamento dell'oggetto 
        Vector3 vector; vector.z = 0;
        vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);
        vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        return vector;
    }

    public float normalize_orientation(float angle)
    { //notazione classica per calcolo seno e coseno
        angle += 90;
        if (angle < 0)
        {
            angle = 360 - Mathf.Abs(angle); //(normalizza da 0 a 360 gradi)
        }
        return angle;
    }

    public void anchor_obj(GameObject obj, float anchor_x, float anchor_y, Camera cam) //ancora l'oggetto alle coordinate specificate
    {
        obj.transform.position = cam.ScreenToWorldPoint(new Vector3(anchor_x, anchor_y, 0));
    }

    public Vector3 scale_obj(float scale_x, float scale_y, Vector3 LocalScale, Camera cam, float cam_stock) //scala l'oggetto in base allo zoom attuale della videocamera
    {
        return new Vector3(scale_x * (cam.orthographicSize / cam_stock), scale_y * (cam.orthographicSize / cam_stock), LocalScale.z);
    }

    public bool filter_targets(string[] filter, string name) //controlla se name e' contenuto in filter
    {
        foreach (string str in filter)
        {
            if (name.Contains(str)) //se il nome dell'oggetto contiene "name"
            {
                return false; //l'oggetto e' contenuto nel filtro
            }
        }
        return true; //l'oggetto non e' contenuto nel filtro
    }

    //FUNZIONI PIANETI
    public float get_g(float grav_mult, float mass, float radius) //calcola l'accelerazione gravitazionale dell'oggetto
    {
        return (grav_mult * mass) / Mathf.Pow(radius, 2);
    }

    public float get_mass(float g, float grav_mult, float radius) //calcola la massa dell'oggetto
    {
        return (Mathf.Pow(radius, 2) * g) / grav_mult;
    } 

    public float get_T(float semi_major_axis, float grav_mult, float mass, Rigidbody2D parent = null) //calcola il periodo di rivoluzione dell'oggetto
    {
        if (parent != null) //ovvero se l'oggetto e' un pianeta o una luna
        {
            return (2 * Mathf.PI) * Mathf.Sqrt(Mathf.Pow(semi_major_axis, 3) / (grav_mult * (parent.mass + mass))); 
        }
        return 0;
    }
    public float get_escape_vel(float g, float radius) //calcola la velocita' di fuga sulla superficie dell'oggetto
    {
        return Mathf.Sqrt(2 * g * radius);
    }

    public Vector2 get_orbital_vel(GameObject sun, GameObject planet, float grav_mult) //Calcola la velocità orbitale necessaria all'oggetto per ottenere un orbita stabile
    {
        Vector2 grav_force = calculate_grav_force(sun, planet, grav_mult);
        Vector2 perp_grav_force = grav_force.Perpendicular1().normalized;
        float planet_vel = Mathf.Sqrt((grav_mult * (sun.GetComponent<Rigidbody2D>().mass + planet.GetComponent<Rigidbody2D>().mass)) / (planet.transform.position - sun.transform.position).magnitude);
        Vector2 planet_vel_vector = perp_grav_force * planet_vel;
        return planet_vel_vector;
    }
    public float get_angular_vel(float delta_angle, float delta_time)
    {
        return delta_angle / delta_time;

    }
    public Vector2 calculate_grav_force(GameObject sun, GameObject planet, float grav_mult) //Calcola l'attrazione gravitazionale che il corpo sun esercita su planet
    {
        //Ottengo vettore distanza normalizzato
        Vector2 dist_vector = (planet.transform.position - sun.transform.position);
        Vector2 dist_vector_normalized = dist_vector.normalized;

        float mass_product = sun.GetComponent<Rigidbody2D>().mass * planet.GetComponent<Rigidbody2D>().mass;
        Vector2 grav_force = - grav_mult * (mass_product / Mathf.Pow(dist_vector.magnitude, 2)) * dist_vector_normalized;
        return grav_force;
    }
    //Calcolo orbita eccentrica
    public Vector2 get_eccentricity(Vector2 orbital_vector, Vector2 distance_vector, float grav_parameter, float orbital_momentum) //Calcola l'eccentricità dell'orbita di orbiter
    {
        float eccentricity_x = (distance_vector.x / distance_vector.magnitude) - ((orbital_momentum * orbital_vector.y) / grav_parameter);
        float eccentricity_y = (distance_vector.y / distance_vector.magnitude) + ((orbital_momentum * orbital_vector.x) / grav_parameter);
        return new Vector2(eccentricity_x, eccentricity_y);
    }
    public float get_semi_major_axis(float grav_parameter, float vel, float distance) //Ottiene semi-asse maggiore orbita ellittica
    {
        return (grav_parameter * distance) / ((2 * grav_parameter) - (distance * vel * vel));
    }
    public float get_orbital_momentum(Vector2 orbital_vector, Vector2 distance_vector) //Ottiene momento orbitale specifico
    {
        return distance_vector.x * orbital_vector.y - distance_vector.y * orbital_vector.x;
    }
    //Fine calcolo orbita eccentrica
    public float get_volume(float radius) //calcola il volume dell'oggetto (formula volume sfera)
    {
         return 4 / 3 * Mathf.PI * Mathf.Pow(radius, 3);
    }
    public float get_density(float mass, float volume)
    {
        return mass / volume;
    }
    public float get_r(float mass1, GameObject obj2, float force,  float grav_mult) //ottiene la distanza tra i due oggetti necessaria per ottenere una forza == force
    {
        float r =  Mathf.Sqrt((grav_mult * mass1 * obj2.GetComponent<Rigidbody2D>().mass) / force);
        if (r < obj2.GetComponent<Object>().radius) //se la distanza calcolata e' dentro il raggio di obj2 ne calcolo la distanza esterna che tiene conto del suo raggio
        {
            r = obj2.GetComponent<Object>().radius;
        }
        return r;
    }
    public float get_influence_sphere(float semi_major_axis, float eccentricity, float mass_planet, float mass_star) //calcola il raggio di influenza del pianeta tenendo conto del corpo attorno a cui orbita(stella)
    {
        return semi_major_axis * (1 - eccentricity) * Mathf.Pow(mass_planet / (3 * (mass_star + mass_planet)), 1f / 3f);
    }
    public float get_expected_temp(GameObject planet, GameObject sun) //calcola la temperatura di equilibrio del pianeta
    {
        return 0f;
    }
    public void print_composition((string, float)[] comp_array) //stampa a schermo la composizione dell'oggetto
    {
        Debug.Log("Composition : {\n");
        foreach (var comp in comp_array)
        {
            Debug.Log("Element: " + comp.Item1 + "---Perc: " + comp.Item2 + "%\n");
        }
        Debug.Log("}");
    }

    public double[] get_cumulative_perc((char, float)[] array) //ottiene l'array delle percentuali cumulative di una CDF
    {
        double[] cumul_percs = new double[array.Length];
        cumul_percs[0] = array[0].Item2;
        for(int i = 1; i < cumul_percs.Length; i++)
        {
            cumul_percs[i] = cumul_percs[i - 1] + array[i].Item2;
        }

        return cumul_percs;
    }

    public string RandomString(int length)
    {
        System.Random rand = new System.Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[rand.Next(s.Length)]).ToArray());
    }
    //COSTANTI
    public float G; //costante di attrazione gravitazionale universale
    public (char, float)[] stars_spectres = new (char, float)[]{('M', 0.765f),
                                                                ('K', 0.121f),
                                                                ('G', 0.076f),
                                                                ('F', 0.03f),
                                                                ('A', 0.006f),
                                                                ('B', 0.0013f),
                                                                ('O', 3E-06f)}; //main sequence -->percentuali cumulative di probabilita'
    public string[] elementiOggettiPlanetari = { //(CHATGPT)
        "Idrogeno",     // Comune in molti pianeti gassosi come Giove e Saturno
        "Elio",         // Anche questo abbondante nei pianeti gassosi
        "Ossigeno",     // Comune nella composizione di atmosfere e corpi rocciosi
        "Carbonio",     // Presente in forme organiche e in molti composti rocciosi
        "Azoto",        // Importante componente di atmosfere planetarie
        "Ferro",        // Comune nei pianeti rocciosi e nei loro nuclei
        "Silicio",      // Presente in abbondanza nei pianeti terrestri
        "Alluminio",    // Comune nella crosta di pianeti rocciosi
        "Magnesio",     // Spesso trovato nella composizione di pianeti rocciosi
        "Sodio",        // Presente in tracce nelle atmosfere di alcuni pianeti
        "Potassio",     // Trovato in tracce in varie forme di roccia e minerale
        "Calcio"        // Un elemento comune nelle rocce planetarie
    };
    public string[] elementiAtmosfereOggettiPlanetari = {
        "Azoto",        // Presente in abbondanza nell'atmosfera terrestre
        "Ossigeno",     // Importante per la respirazione e la vita come la conosciamo
        "Anidride carbonica", // Presente in atmosfere di pianeti come Marte e Venere
        "Idrogeno",     // Comune nei giganti gassosi come Giove e Saturno
        "Elio",         // Abbondante nelle atmosfere dei pianeti gassosi
        "Metano",       // Trovato in atmosfere di pianeti come Urano e Nettuno
        "Vapore acqueo",    // Presente in atmosfere di pianeti con condizioni adatte
        "Ammoniaca",    // Trovato in tracce nelle atmosfere dei giganti gassosi
        "Argon",        // Un gas nobile presente in atmosfere planetarie
        "Xeno",         // Un altro gas nobile, trovato in tracce in atmosfere planetarie
        "Ossido di azoto",  // Trovato in tracce in alcune atmosfere planetarie
        "Ozono"         // Importante per la protezione dagli UV nell'atmosfera terrestre
    };
    public string[] classificazioneOggetti = //Tipi di Object
    {
        "Planet",
        "Star",
        "Moon"
    };
    //Strutture dati
    [System.Serializable]
    public class CompTuple //Classe per la gestione delle composizioni degli oggetti
    {
        public string element;
        public float percentage;

        public CompTuple(string k, float v)
        {
            element = k;
            percentage = v;
        }
    }
    [System.Serializable]
    public struct PlanetaryObjConfig //Struttura di personalizzazione generazione oggetto planetario
    {
        [Range(0f, 1f)] public float albedo_min; //limite minimo generazione albedo
        [Range(0f, 1f)] public float albedo_max; //limite massimo generazione albedo
        [Range(0f, 10f)] public float vel_rot_min; //limite minimo generazione velocita' di rotazione oggetto
        [Range(0f, 10f)] public float vel_rot_max; //limite massimo generazione velocita' di rotazione oggetto
        [Range(0f, 20f)] public float g_min; //limite minimo generazione accelerazione grav. superficiale
        [Range(0f, 20f)] public float g_max; //limite minimo generazione accelerazione grav. superficiale
        [Range(0f, 1f)] public float radius_scale_min; //limite minimo generazione raggio in scala rispetto a orbitee
        [Range(0f, 1f)] public float radius_scale_max; //limite minimo generazione raggio in scala rispetto a orbitee
        [Range(0f, 1000f)] public float age_min; //limite minimo generazione age
        [Range(0f, 1000f)] public float age_max; //limite minimo generazione age
        public string class_name; //Classificazione oggetto planetario
    }

    [System.Serializable]
    public struct PlanetaryConfig //struttura wrapper di personalizzazione generazione oggetti planetari
    {
        public PlanetaryObjConfig planet_config; //versione pianeta
        public PlanetaryObjConfig moon_config; //versione luna
    }

    [System.Serializable]
    public struct StellarObjConfig //Struttura di personalizzazione generazione oggetto stellare
    {
        [Range(0f, 100f)] public float vel_rot_min; //limite minimo generazione velocita' di rotazione oggetto
        [Range(0f, 100f)] public float vel_rot_max; //limite massimo generazione velocita' di rotazione oggetto
        [Range(0f, 100f)] public float g_min; //limite minimo generazione accelerazione grav. superficiale
        [Range(0f, 100f)] public float g_max; //limite massimo generazione accelerazione grav. superficiale
        [Range(0f, 30f)] public float radius_min; //limite minimo generazione raggio 
        [Range(0f, 30f)] public float radius_max; //limite massimo generazione raggio 
        [Range(0f, 100000f)] public float age_min; //limite minimo generazione age
        [Range(0f, 100000f)] public float age_max; //limite massimo generazione age
        [Range(0f, 10000f)] public float lum_min; //limite minimo  generazione luminosità oggetto stellare
        [Range(0f, 10000f)] public float lum_max; //limite massimo generazione luminosità oggetto stellare
        [Range(0f, 10000f)] public float temp_min; //limite minimo  generazione temperatura oggetto stellare
        [Range(0f, 10000f)] public float temp_max; //limite massimo generazione temperatura oggetto stellare
        public char spectrum; //Classificazione oggetto stellare
    }

    [System.Serializable]
    public struct StellarConfig //struttura wrapper di personalizzazione generazione oggetti stellari
    {
        public StellarObjConfig stellar_config; 

    }
    //Navicella
    [System.Serializable]
    public struct ShipMotorConfig //struct di configurazione motori nave
    {
        [Range(0f, 100f)] public float acc;  //accelerazione 
        [Range(0f, 100f)] public float max_vel; //velocita' massima
        [Range(-100f, 100f)] public float min_vel; //velocita' massima 
        [Range(0f, 100f)] public float dead_zone; //intervallo di velocita' nel quale il flight_assist opera per automaticamente azzerare la velocita'
        [Range(0f, 100f)] public float boost_target_offset; //offset di velocita' massimo ottenuto con il boost
        [Range(0f, 100f)] public float boost_acc; //accelerazione boost(velocita' con cui raggiunge boost_target_offset)
        [Range(0f, 100f)] public float boost_dec; //decelerazione boost(velocita' con cui il boost termina i suoi effetti)
        [Range(0f, 100f)] public float boost_cooldown; //tempo di cooldown per il booster
    }
    [System.Serializable]
    public struct ShipWeaponConfig //Struct di configurazione arsenale nave
    {
        [Range(0f, 100f)] public float cooldown_time; //tempo di cooldown tra un un utilizzo e l'altro(non troppo piccolo)
        [Range(0f, 100f)] public float bullet_speed;  //velocita' oggetto
        [Range(0f, 100f)] public float bullet_spawn_vert; //offset di spawn dell'oggetto(lungo la direzione della nave)
        [Range(0f, 100f)] public float time_to_destroy; //tempo oltre il quale l'oggetto si autodistrugge

    }
    [System.Serializable]
    public struct ShipControlPanel //Struct di variabili della nava
    {
        //Engine
        public float engine_vel; //velocita' corrente della nave
        public float last_engine_vel; //velocita' engine al momento dell'attivazione del boost(per tornare a quel punto dopo che il booster ha finito)
        public float boost_increment; //incremento attuale alla velocita' massima
        public float boost_time; //variabile timer booster principale
        public bool boost_bool; //booleano che indica se il boost e' attivo
        public bool unboost_bool; //Booleano che indica se il de-boost e' attivo
        //Thruster
        public float thruster_vel; //velocita' corrente di spostamento laterale
        public float last_thruster_vel; //usata per il booster laterale
        public float lat_boost_target_offset; //offset di velocita' laterale massimo attuale(puo essere positivo o negativo)
        public float lat_boost_increment; //incremento attuale alla velocita' laterale massima
        public float lat_boost_time; //variabile timer booster laterale
        public bool lat_boost_bool; //booleano che indica se il boost laterale e' attivo
        public bool lat_unboost_bool; //booleano che indica se il de-boost laterale e' attivo
        //Misc
        public Vector3 vel;   //Vettore spostamento nave 
        public bool flight_assist; //azzeramento automatico thruster e rotazione
        public bool motor_switch; //accensione spegnimento motore
        public float direction; //variabile direzione nave(angolo)
        public float wep_time; //tempo prima che si puo' sparare 

    }

    [System.Serializable]
    public struct ShipMiscConfig //Struct di configurazione valori misc
    {
        public float rot_vel;
        public float flight_assist_efficiency;
    }
    [System.Serializable]
    public struct ShipConfig
    {
        public ShipMotorConfig Engine;
        public ShipMotorConfig Thruster;
        public ShipMiscConfig misc;
        public ShipWeaponConfig projectile;
        public ShipControlPanel control_panel;

    }

}
