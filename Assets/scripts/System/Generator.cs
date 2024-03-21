using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using UnityEngine;

public class Generator: MonoBehaviour //CLASSE PER GESTIRE LA GENERAZIONE PROCEDURALE DEI SISTEMI (Generazione Top-Down)
{
  
    //Classi di Supporto
    public GameObject System;
    public ObjectGenerator obj_gen = new ObjectGenerator();
    public Functions fun = new Functions();

    //PIANETI E LUNE
    //range di valori generati in base a criteri (RANGE GENERATI)
    (float, float) mass_range, radius_range, distance_range; //modificati al momento della generazione degli oggetti
    //range di valori arbitrario scelto da me (RANGE ARBITRARI) -->Assumo queste costanti come indipendenti dal contesto in cui ci troviamo
    [Range(0f, 1f)] public float albedo_min; //limite minimo generazione albedo
    [Range(0f, 1f)] public float albedo_max; //limite minimo generazione albedo
    [Range(0f, 10f)] public float rot_min; //limite minimo generazione velocita' di rotazione oggetto
    [Range(0f, 10f)] public float rot_max; //limite massimo generazione velocita' di rotazione oggetto
    [Range(0f, 300f)] public float planet_distance_div; //moltiplicatore distanza pianeta --> inversamente proporzionale alla distanza degli oggeti
    [Range(0f, 1000f)] public float moon_distance_div; //moltiplicatore distanza luna --> inversamente proporzionale alla distanza degli oggeti
    [Range(0f, 0.001f)] public float mass_scale_upper; //limite massimo moltiplicatore generazione massa oggetti
    [Range(0f, 0.001f)] public float mass_scale_lower; //limite minimo moltiplicatore generazione massa oggetti
    [Range(0f, 0.10f)] public float radius_scale_upper; //limite massimo moltiplicatore generazione massa oggetti
    [Range(0f, 0.10f)] public float radius_scale_lower; //limite minimo moltiplicatore generazione massa oggetti 
    [Range(0.0000001f, 100f)] public float planet_perturbation_limit; //limite forza massimo perturbazioni orbitali
    [Range(0f, 100f)] public float infra_obj_min_distance; //minima distanza fisica possibile fra i pianeti
    //[Range(0f, 1f)] public float moon_perturbation_limit; //limite forza massimo perturbazioni orbitali

    //STELLE
    //Limiti numero oggetti generati
    public int max_moons_pp = 1; //numero massimo di lune per pianeta
    public int max_planet_ps = 1; //numero massimo di pianeti per ogni sole
    public int max_suns_ps = 1; //numero massimo di soli per sistema planetario

    public Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>> generate_system() 
    {
        Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>> system = new Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>>();
        for (int i = 0; i < max_suns_ps; i++) //per ogni sole da generare
        {
            Rigidbody2D sun = generate_sun().GetComponent<Rigidbody2D>();
            List<Rigidbody2D> planets = generate_planets(sun.gameObject, fun.classificazioneOggetti[0], max_planet_ps);
            Dictionary<Rigidbody2D, List<Rigidbody2D>> orbiters = new Dictionary<Rigidbody2D, List<Rigidbody2D>>();
            foreach (Rigidbody2D planet in planets) //genero le lune per ogni pianeta
            {
                List<Rigidbody2D> moons = generate_planets(planet.gameObject, fun.classificazioneOggetti[2], max_moons_pp); //ottengo le lune generate per il pianeta
                orbiters.Add(planet, moons); //aggiungo il pianeta con le sue lune generate al dizionario
            }
            system.Add(sun, orbiters); //aggiungo il sole e i suoi pianeti e lune orbitanti al sistema
        }
        return system;
    }

    List<Rigidbody2D> generate_planets(GameObject orbitee, string type, int max)//genera gli oggetti attorno all'oggetto orbitee (pianeti / lune)
    {
        List<Rigidbody2D> orbiters = new List<Rigidbody2D>();
        //determino i range dei valori da assegnare
        Object dati_obj = orbitee.GetComponent<Object>(); //non mi serve sapere che oggetto sia, in quanto le informazioni di cui ho bisogno sono poche
        mass_range = get_range(dati_obj.mass, mass_scale_lower, mass_scale_upper);
        radius_range =  get_range(dati_obj.radius, radius_scale_lower, radius_scale_upper);
        get_distance_range(type, orbitee);
        int obj_number = UnityEngine.Random.Range(0, max + 1); //stabiliamo il numero di oggetti da generare
        //genero gli oggetti

        for (int i = 0; i < obj_number; i++) //per ogni oggetto da generare le assegno caratteristiche semi-randomiche(dipendenti dalle caratteristiche di orbitee)
        {
            float obj_radius = UnityEngine.Random.Range(radius_range.Item1, radius_range.Item2);
            float obj_mass = UnityEngine.Random.Range(mass_range.Item1, mass_range.Item2);
            (float, float) new_dist_range =  squeeze_in(orbiters,orbitee, obj_radius, obj_mass, distance_range); //ottengo nuovo range 
            if (new_dist_range.Item1 == -1) //nel caso fallisca la generazione di una distanza, non genero l'oggetto e provo con il successivo
            {
                continue; 
            }
            float obj_distance = UnityEngine.Random.Range(new_dist_range.Item1, new_dist_range.Item2);
            float obj_albedo = UnityEngine.Random.Range(albedo_min, albedo_max);
            float obj_age = UnityEngine.Random.Range(0f, dati_obj.age); //l'oggetto orbitante deve essere piu giovane di orbitee
            float obj_rot = UnityEngine.Random.Range(rot_min, rot_max);
            Functions.CompTuple[] obj_comp = generate_comp(fun.elementiPianeti);
            Functions.CompTuple[] obj_atm_comp = generate_comp(fun.elementiAtmosferePianeti);
            GameObject obj = obj_gen.initialize_planet(obj_radius, obj_mass, type, fun.RandomString(10), System, obj_distance, orbitee.GetComponent<Rigidbody2D>(), obj_age,
                                                     obj_rot,obj_albedo, obj_comp, obj_atm_comp); //creazione oggetto effettivo
            orbiters.Add(obj.GetComponent<Rigidbody2D>()); //aggiungo l'oggetto creato alla lista
        }
        return orbiters;
   
    }

    void get_distance_range(string type, GameObject orbitee) //ottiene il range in cui i pianeti/lune possono spawnare
    {
        float dist_mult; 
        if (type.CompareTo(fun.classificazioneOggetti[0]) == 0) { //se l'oggetto e' un pianeta
            dist_mult = planet_distance_div;
        } 
        else {  //se l'oggetto e' una luna
            dist_mult = moon_distance_div;
        }
        float approx_optimal_distance = fun.get_approximate_distance(orbitee, dist_mult, System.GetComponent<Gravitation>().grav_multiplier); //ottengo distanza ottimale dell'oggetto da orbitee
        distance_range = (orbitee.GetComponent<Object>().radius + infra_obj_min_distance, orbitee.GetComponent<Object>().radius + approx_optimal_distance); //stabilisco il range, e faccio in modo che il limite minimo sia pari al raggio del padre e il massimo pari alla distanza ottenuta con la funzione
        //(raggio_orbitee + minima distanza fra pianeti, massima distanza pianeti da orbitee)
        print(distance_range);
    }

     (float, float) squeeze_in(List<Rigidbody2D> generated_objs,GameObject Orbitee, float radius,float mass, (float, float) dist_range) //trova il primo intervallo utile di generazione delle distanze per pianeti e lune --> motivo per cui i pianeti si "accumulano" vicino alla stella
    {
        //Ordino la lista degli oggetti generati per la loro distanza dal sole (in modo crescente)
        generated_objs.Sort((obj1, obj2) => obj1.GetComponent<Planet>().distance.CompareTo(obj2.GetComponent<Planet>().distance));

        foreach (Rigidbody2D obj in generated_objs)
        {
            //determino in che tipo di range sono limite - pianeta / pianeta - pianeta / pianeta - limite
            //1 caso
            if (generated_objs.IndexOf(obj) == 0)
            {
                    //trovo,se esiste, la distanza ottimale di generazione dell'oggetto tale che non ci siano perturbazioni orbitali con il vicino
                    //(trovo la distanza dall'oggetto che poi devo sottrarre alla sua distanza dal sole per ottenere quello che mi serve effettivamente)
                    float offset = fun.get_r(mass, obj.GetComponent<Planet>().mass, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier);
                    float distance = (obj.GetComponent<Planet>().distance - offset); //a sinistra del pianeta
                    if (dist_range.Item1 + radius < distance) //se la distanza per lo spawn e' compresa tra il limite inf + il raggio dell'oggetto e l'oggetto successivo allora ok
                    {
                        return (dist_range.Item1 + radius , distance - obj.GetComponent<Planet>().radius - radius);
                    } else if (generated_objs.Count == 1) //nel caso sia presente solo un elemento devo assegnargli l'intervallo successivo direttamente qui (pena un eccezione)
                    {
                        return (obj.GetComponent<Planet>().distance + offset + radius, dist_range.Item2 - radius);
                    }
            } else if (generated_objs.IndexOf(obj) == generated_objs.Count - 1) //3 Caso
            {
                    float offset = fun.get_r(mass, obj.GetComponent<Planet>().mass, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier);
                    float distance = obj.GetComponent<Planet>().distance + offset; //a destra del pianeta
                    if (distance + radius < dist_range.Item2 - radius) //se la distanza piu' il raggio e' minore del limite massimo di distanza allora ok
                    {
                        return (distance + radius, dist_range.Item2 - radius);
                    }
            } else //2 Caso (Caso generale)
            {
                    float offset1 = fun.get_r(mass, obj.GetComponent<Planet>().mass, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier); //distanza necessaria all oggetto 1
                    float offset2 = fun.get_r(mass, generated_objs.ElementAt(generated_objs.IndexOf(obj) + 1).GetComponent<Planet>().mass, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier); //distanza necessaria all oggetto 2
                    (float, float) limits = (obj.GetComponent<Planet>().distance + offset1, generated_objs.ElementAt(generated_objs.IndexOf(obj) + 1).GetComponent<Planet>().distance - offset2);
                    if (limits.Item1 + radius < limits.Item2 - radius) //Se il range esiste e non e' negativo e vi e' lo spazio fisico
                    {
                        return (limits.Item1 + radius, limits.Item2 - radius); //returno la media tra i due limiti
                    }
            }     
        }
        if(generated_objs.Count != 0) //se arrivo qui e gli oggetti da piazzare sono > 0 allora e' perche' non ho trovato il range, per cui lancio errore
        {
            Debug.Log("Optimal range not found while generating objs \nLimiting planet generation");
            return (-1, 0);
        } else //nel caso in cui non abbia ancora generato oggetto ritorno la media fra i limiti originari
        {
            return (dist_range.Item1 + radius, dist_range.Item2 - radius); //restringo il range per comprendere lo spazio fisico dovuto al diametro dell'oggetto
        }

    }

     GameObject generate_sun() //generazione stelle 
     {
        char spectrum = get_spectrum(fun.stars_spectres); //genero lo spettro
        GameObject sun = obj_gen.initialize_star(10f, 500000f, "Stella", "Sole", System, 100f, 1f, 10f, spectrum); //HARDCODED per ora
        return sun;
     }

    (float, float) get_range(float datum, float lower_mult, float upper_mult) //ottiene range in scala rispetto al valore datum
    {
        return (datum * lower_mult,  datum * upper_mult);
    }
    Functions.CompTuple[] generate_comp(string[] comp_data) //genera la composizione dell'oggetto (per ora totalmente randomico)
    {
        List<string> comp_list = new List<string>(); //converto l'array in una lista 
        comp_list.AddRange(comp_data);
        List<Functions.CompTuple> composition = new List<Functions.CompTuple>(); //composizione generata
        float remaining_percentage = 1f; //per generare una composizione che abbia senso
        while(comp_list.Count > 0) //finche' non ho estratto tutti gli elementi
        {
            //estraggo un indice randomico 0 - list.lenght()
            int rand_index = UnityEngine.Random.Range(0, comp_list.Count);
            string element = comp_list[rand_index];
            //elimino l'elemento estratto dalla lista per non estrarlo di nuovo
            comp_list.RemoveAt(rand_index);
            //ottengo la percentuale dell'elemento estratto
            float rand_composition = UnityEngine.Random.Range(0f, remaining_percentage);
            remaining_percentage -= rand_composition; //sottraggo la composizione estratta
            //aggiungo la composizione alla lista
            composition.Add(new Functions.CompTuple(element, rand_composition));
        }
        //converto la lista in array
        Functions.CompTuple[] comp_array = composition.ToArray();
        return comp_array;
    }
    
    char get_spectrum((char, float)[] spectres) //genera il tipo della stella in base alle percentuali reali (Da implementare)
    {
        char spectrum;
        double rand_number = UnityEngine.Random.Range(0, 1); //estraggo un double da 0 a 1
        //in base al range in cui finisce rand_number determino lo spettro della stella estratto
        double[] cumulative_perc = fun.get_cumulative_perc(spectres);
        //determino l'indice della stella estratta
        int index = Array.FindIndex(cumulative_perc, p => p >= rand_number); //indice del numero p tale che p >= rand_number
        //determino la stealla estratta
        spectrum = spectres[index].Item1;
        return spectrum;
    }
}
