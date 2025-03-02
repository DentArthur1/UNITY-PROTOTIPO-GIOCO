using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Functions;

public class Generator: MonoBehaviour //CLASSE PER GESTIRE LA GENERAZIONE PROCEDURALE DEI SISTEMI (Generazione Top-Down)
{
  
    //Classi di Supporto
    public GameObject System;
    public ObjectGenerator obj_gen = new ObjectGenerator();
    public Functions fun = new Functions();

    //Limiti simulazione
    [Range(0.0000001f, 100f)] public float planet_perturbation_limit; //limite forza massimo perturbazioni orbitali
    [Range(0.0000001f, 100f)] public float moon_perturbation_limit; //limite forza massimo perturbazioni orbitali
    [Range(0f, 100f)] public float infra_planet_min_distance; //minima distanza fisica possibile fra i pianeti
    [Range(0f, 100f)] public float infra_moon_min_distance; //minima distanza fisica possibile fra i pianeti
    [Range(0f, 3f)] public float moon_distance_mult; //moltiplicatore distanza generazione lune
    [Range(0f, 10000f)] public float max_system_lenght; //distanza massima di generazione pianeti 
    [Range(0f, 100f)] public int max_moons_pp; //numero massimo di lune per pianeta
    [Range(0f, 100f)] public int max_planet_ps; //numero massimo di pianeti per ogni sole
    [Range(0f, 100f)] public int max_suns_ps; //numero massimo di soli per sistema planetario

    //Configurazione Pianeti
    public List<PlanetaryConfig> planetary_config;

    //Configurazione stelle
    public List<StellarConfig> stellar_config;

    public Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>> generate_system() 
    {
        Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>> system = new Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>>();
        for (int i = 0; i < max_suns_ps; i++) //per ogni sole da generare
        {
            Rigidbody2D sun = generate_sun().GetComponent<Rigidbody2D>();
            List<Rigidbody2D> planets = generate_planetary_objects(sun.gameObject, fun.classificazioneOggetti[0], max_planet_ps);
            Dictionary<Rigidbody2D, List<Rigidbody2D>> orbiters = new Dictionary<Rigidbody2D, List<Rigidbody2D>>();
            foreach (Rigidbody2D planet in planets) //genero le lune per ogni pianeta
            {
                List<Rigidbody2D> moons = generate_planetary_objects(planet.gameObject, fun.classificazioneOggetti[2], max_moons_pp); //ottengo le lune generate per il pianeta
                orbiters.Add(planet, moons); //aggiungo il pianeta con le sue lune generate al dizionario
            }
            system.Add(sun, orbiters); //aggiungo il sole e i suoi pianeti e lune orbitanti al sistema
        }
        return system;
    }

    List<Rigidbody2D> generate_planetary_objects(GameObject orbitee, string type, int max_objs)//Generazione pianeti
    {
        List<Rigidbody2D> orbiters = new List<Rigidbody2D>();
        //Stabilisco il numero degli oggetti da provare a generare
        int num_objs = UnityEngine.Random.Range(0, max_objs + 1); //da 0 a max_objs estremi inclusi
        //stabilisco il range di generazione dei pianeti
        //Distinguo il caso pianeta dal caso luna
        float max_distance; float infra_min;
        if (type.CompareTo(fun.classificazioneOggetti[2]) == 0) //devo generare delle lune
        {
            //calcolo la sfera di influenza di orbitee
            PlanetaryObject pianeta = orbitee.GetComponent<PlanetaryObject>();
            max_distance = moon_distance_mult * fun.get_influence_sphere(pianeta.distance,0f, pianeta.mass, pianeta.parent.mass); //CORREGGERE -->TENERE CONTO DI ECCENTRICITA
            //Caso Critico: max_distance dentro il raggio del pianeta
            infra_min = infra_moon_min_distance;
        } else //devo generare dei pianeti
        {
            max_distance = max_system_lenght;
            infra_min = infra_planet_min_distance;
        }
        (float, float) distance_range = (orbitee.GetComponent<Object>().radius, max_distance); //genero il range di distanza
        print(distance_range);
        for (int i = 0; i < num_objs; i++) //per ogni oggetto da provare a generare
        {
            //stabilisco il tipo di pianeta da generare
            PlanetaryObjConfig planet_config = get_random_planetary_type(type); //tipo pianeta
            float planet_g = UnityEngine.Random.Range(planet_config.g_min, planet_config.g_max); //usata per calcolare massa dato il raggio
            float planet_radius = UnityEngine.Random.Range(orbitee.GetComponent<Object>().radius * planet_config.radius_scale_min, orbitee.GetComponent<Object>().radius * planet_config.radius_scale_max); //genero raggio in scala rispetto a orbitee
            float planet_rot_vel = UnityEngine.Random.Range(planet_config.vel_rot_min, planet_config.vel_rot_max);
            float planet_albedo = UnityEngine.Random.Range(planet_config.albedo_min, planet_config.albedo_max);
            float planet_mass = fun.get_mass(planet_g, System.GetComponent<Gravitation>().grav_multiplier, planet_radius);
            float planet_age = UnityEngine.Random.Range(planet_config.age_min, planet_config.age_max);
            CompTuple[] planet_comp = generate_comp(fun.elementiOggettiPlanetari);
            CompTuple[] planet_atm_comp = generate_comp(fun.elementiAtmosfereOggettiPlanetari);
            //generazione distanza pianeta
            (float, float) new_distance_range = squeeze_in(orbiters, planet_radius, planet_mass, distance_range, infra_min);
            if (new_distance_range.Item1 == -1) //non ho trovato la distanza ottimale, salto il resto del loop
            {
                continue;
            }
            float planet_distance = UnityEngine.Random.Range(new_distance_range.Item1, new_distance_range.Item2);
            GameObject planetary_obj = obj_gen.initialize_planetary_object(planet_radius, planet_mass, type, type + "-" + fun.RandomString(10), System, planet_distance, orbitee.GetComponent<Rigidbody2D>(), planet_age,
                                                     planet_rot_vel, planet_albedo, planet_comp, planet_atm_comp, planet_config.class_name); //creazione oggetto effettivo
            //aggiunta oggetto a lista di orbiters
            orbiters.Add(planetary_obj.GetComponent<Rigidbody2D>());
        }
        return orbiters;
   
    }


    (float, float) squeeze_in(List<Rigidbody2D> generated_objs,float radius, float mass, (float, float) dist_range, float infra_obj_min_distance) //trova il primo intervallo utile di generazione delle distanze per pianeti e lune --> motivo per cui i pianeti si "accumulano" vicino alla stella
    {
        //Ordino la lista degli oggetti generati per la loro distanza dal sole (in modo crescente)
        generated_objs.Sort((obj1, obj2) => obj1.GetComponent<PlanetaryObject>().distance.CompareTo(obj2.GetComponent<PlanetaryObject>().distance));

        foreach (Rigidbody2D obj in generated_objs)
        {
            //determino in che tipo di range sono limite - pianeta / pianeta - pianeta / pianeta - limite
            //1 caso
            if (generated_objs.IndexOf(obj) == 0)
            {
                //trovo, se esiste, la distanza ottimale di generazione dell'oggetto tale che non ci siano perturbazioni orbitali con il vicino
                float offset = fun.get_r(mass, obj.gameObject, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier);
                float distance = (obj.GetComponent<PlanetaryObject>().distance - offset); //a sinistra del pianeta
                if (dist_range.Item1 + radius + infra_obj_min_distance < distance - radius - infra_obj_min_distance) //range --> Compreso tra Orbitee e Oggetto da cui prendere le distanze
                {
                    return (dist_range.Item1 + radius, distance - radius - infra_obj_min_distance);
                }
                else if (generated_objs.Count == 1) //nel caso sia presente solo un elemento devo assegnargli l'intervallo successivo direttamente qui (pena un eccezione)
                {
                    if (obj.GetComponent<PlanetaryObject>().distance + offset + radius + infra_obj_min_distance < dist_range.Item2 - radius) //range --> Compreso tra Oggetto da cui prendere le distanze e limite massimo distanza sistema
                    {
                        return (obj.GetComponent<PlanetaryObject>().distance + offset + radius + infra_obj_min_distance, dist_range.Item2 - radius);
                    }  
                }
            }
            else if (generated_objs.IndexOf(obj) == generated_objs.Count - 1) //3 Caso
            {
                float offset = fun.get_r(mass, obj.gameObject, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier);
                float distance = obj.GetComponent<PlanetaryObject>().distance + offset; //a destra del pianeta
                if (distance + radius + infra_obj_min_distance < dist_range.Item2 - radius) //range --> Compreso tra Oggetto da cui prendere le distanze e limite massimo di distanza
                {
                    return (distance + radius + infra_obj_min_distance, dist_range.Item2 - radius);
                }
            }
            else //2 Caso (Caso generale)
            {
                float offset1 = fun.get_r(mass, obj.gameObject, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier); //distanza necessaria all oggetto 1
                float offset2 = fun.get_r(mass, generated_objs.ElementAt(generated_objs.IndexOf(obj) + 1).gameObject, planet_perturbation_limit, System.GetComponent<Gravitation>().grav_multiplier); //distanza necessaria all oggetto 2
                (float, float) limits = (obj.GetComponent<PlanetaryObject>().distance + offset1, generated_objs.ElementAt(generated_objs.IndexOf(obj) + 1).GetComponent<PlanetaryObject>().distance - offset2);
                if (limits.Item1 + radius + infra_obj_min_distance < limits.Item2 - radius - infra_obj_min_distance) //range --> Compreso tra Oggetto1 da cui prendere le distanze e Oggetto2 da cui prendere le distanze
                {
                    return (limits.Item1 + radius + infra_obj_min_distance, limits.Item2 - radius - infra_obj_min_distance); //returno la media tra i due limiti
                }
            }
        }
        if (generated_objs.Count != 0) //se arrivo qui e gli oggetti piazzati sono > 0 allora e' perche' non ho trovato il range, per cui lancio errore
        {
            Debug.LogError("Optimal range not found while generating objs \nLimiting planet generation");
            return (-1, 0);
        }
        else //nel caso in cui non abbia ancora generato un oggetto ritorno la media fra i limiti originari
        {
            if (dist_range.Item1 + radius + infra_obj_min_distance < dist_range.Item2 - radius) //controllo che il range di default sia valido
            {
                return (dist_range.Item1 + radius + infra_obj_min_distance, dist_range.Item2 - radius); //restringo il range per comprendere lo spazio fisico dovuto al diametro dell'oggetto
            } else //range di default non valido, errore
            {
                Debug.LogError("Default Range not valid \nLimiting planet generation");
                return (-1, 0);
            }
            
        }

    }

    GameObject generate_sun() //generazione oggetti stellari 
     {
        StellarObjConfig stellar_config = get_random_stellar_type(); //ottengo tipo di stella randomic0
        //Genero i valori randomici
        float star_rot_vel = UnityEngine.Random.Range(stellar_config.vel_rot_min, stellar_config.vel_rot_max);
        float star_radius = UnityEngine.Random.Range(stellar_config.radius_min, stellar_config.radius_max);
        float star_g = UnityEngine.Random.Range(stellar_config.g_min, stellar_config.g_max);
        float star_age = UnityEngine.Random.Range(stellar_config.age_min, stellar_config.age_max);
        float star_lum = UnityEngine.Random.Range(stellar_config.lum_min, stellar_config.lum_max);
        float star_temp = UnityEngine.Random.Range(stellar_config.temp_min, stellar_config.temp_max);
        float star_mass = fun.get_mass(star_g, System.GetComponent<Gravitation>().grav_multiplier, star_radius); //calcolo la massa dati raggio e g
        //Assegno i valori e creo la stella
        GameObject sun = obj_gen.initialize_stellar_object(star_radius, star_mass, fun.classificazioneOggetti[1], "SUN-" + fun.RandomString(10), System, star_age, star_rot_vel, star_lum, stellar_config.spectrum, star_temp);
        return sun;

    }
   CompTuple[] generate_comp(string[] comp_data) //genera la composizione dell'oggetto (per ora totalmente randomico)
    {
        List<string> comp_list = new List<string>(); //converto l'array in una lista 
        comp_list.AddRange(comp_data);
        List<CompTuple> composition = new List<CompTuple>(); //composizione generata
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
            composition.Add(new CompTuple(element, rand_composition));
        }
        //converto la lista in array
        CompTuple[] comp_array = composition.ToArray();
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
    PlanetaryObjConfig get_random_planetary_type(string type) //per ottenere il file di configurazione di un tipo di pianeta randomico
    {
        int index = UnityEngine.Random.Range(0, planetary_config.Count);

        if (type.CompareTo(fun.classificazioneOggetti[2]) == 0){  //Caso luna
            return planetary_config.ElementAt(index).moon_config;
        }//Caso pianeta
        return planetary_config.ElementAt(index).planet_config;
    }

    StellarObjConfig get_random_stellar_type() //Per ottenere il file di configurazione di un tipo di stella randomico
    {
        int index = UnityEngine.Random.Range(0, stellar_config.Count);
        return stellar_config.ElementAt(index).stellar_config;
    }
}
