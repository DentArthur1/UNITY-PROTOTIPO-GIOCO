using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

    public float get_T(float distance, float grav_mult, float mass, Rigidbody2D parent = null) //calcola il periodo di rivoluzione dell'oggetto
    {
        if (parent != null) //ovvero se l'oggetto e' un pianeta o una luna
        {
            return (2 * Mathf.PI) * Mathf.Sqrt(Mathf.Pow(distance, 3) / (grav_mult * (parent.mass + mass)));
        }
        return 0;
        //in realta distance sarebbe la media tra il punto piu' vicino e piu lontato dal sole del pianeta

    }
    public float get_escape_vel(float grav_mult, float radius, float mass) //calcola la velocita' di fuga
    {
        return Mathf.Sqrt((2 * grav_mult * mass) / radius);
    }

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

    public float get_influence_sphere(float distance, float mass_planet, float mass_star) //calcola il raggio di influenza del pianeta tenendo conto del corpo attorno a cui orbita(stella) --> con eccentricità trascurabile***
    {
        return distance * Mathf.Pow(mass_planet / (3 * mass_star), 1f / 3f);
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
    public const double G = 6.67430e-11; //costante di attrazione gravitazionale universale
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
    public struct PlanetaryConfig //struttura wrapper di personalizzazione generazione oggetti
    {
        public PlanetaryObjConfig planet_config; //versione pianeta
        public PlanetaryObjConfig moon_config; //versione luna
    }

}
