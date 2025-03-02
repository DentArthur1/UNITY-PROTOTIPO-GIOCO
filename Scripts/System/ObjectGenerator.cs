using Unity.VisualScripting;
using UnityEngine;

public class ObjectGenerator //CLASSE PER GESTIRE LA GENERAZIONE DEGLI OGGETTI PLANETARI E STELLARI
{
    //Classi di supporto
    public GameObject obj; //oggetto creato e a cui viene e assegnata la classe Object
    public GameObject System; //oggetto sistema 
    private Gravitation god; //classe gravitation
    private Functions fun = new Functions(); //classe funzioni ausiliarie

    //COSTRUZIONE OGGETTO PLANETARIO
    public GameObject initialize_planetary_object(float radius, float mass, string type, string name, GameObject sys, float distance, Rigidbody2D parent,
        float age, float rot,float albedo, Functions.CompTuple[] terrain_comp, Functions.CompTuple[] atm_comp, string planetary_class) //Wrapper per gestire la generazione dell'oggetto PLANETARIO
    {
        god = sys.GetComponent<Gravitation>(); //riferimento classe gravitation(sistema)
        generate_planetary_object(radius, mass, type, name, distance, parent, age, rot, albedo, terrain_comp, atm_comp, sys, planetary_class); //generazione oggetto
        return obj;
    }
    void generate_planetary_object(float radius, float mass, string type, string name, float distance, Rigidbody2D parent,
       float age, float rot,float albedo, Functions.CompTuple[] terrain_comp, Functions.CompTuple[] atm_comp, GameObject sys, string planetary_class)
    {
        create_body(type, sys);
        give_distance(distance, parent);
        assign_base_values(radius, mass, type, name,age, rot);
        assign_planetary_values(parent, distance, albedo, terrain_comp, atm_comp, planetary_class);
        shape_body(mass, radius);
    }
    void give_distance(float distance, Rigidbody2D parent) //assegna la distanza dall'oggetto stellare o planetario
    {
        obj.transform.position = new Vector3(parent.transform.position.x + distance, parent.transform.position.y, 0);
    }
    void assign_planetary_values(Rigidbody2D parent, //assegno i valori specifici dell'oggetto planetario
         float distance, float albedo, Functions.CompTuple[] terrain_comp, Functions.CompTuple[] atm_comp, string planetary_class)
    {
        PlanetaryObject dati_pianeta = obj.GetComponent<PlanetaryObject>();
        dati_pianeta.terrain_comp = terrain_comp; dati_pianeta.albedo = albedo;
        dati_pianeta.parent = parent; dati_pianeta.distance = distance; dati_pianeta.class_ = planetary_class;
        dati_pianeta.atm_comp = atm_comp;
        dati_pianeta.initial_velocity = fun.get_orbital_vel(parent.gameObject, obj.gameObject, god.grav_multiplier);
    }
    //COSTRUZIONE OGGETTO STELLARE
    public GameObject initialize_stellar_object(float radius, float mass, string type, string name, GameObject sys,
        float age, float rot,float lum, char spectrum, float temp) //Wrapper per gestire la generazione dell'oggetto stellare
    {
        god = sys.GetComponent<Gravitation>();
        generate_stellar_object(radius, mass, type, name, age, rot, lum, spectrum, sys, temp);
        return obj;
    }
    public void generate_stellar_object(float radius, float mass, string type, string name,
       float age, float rot,float lum, char spectrum, GameObject sys, float temp)
    {
        create_body(type, sys);
        assign_base_values(radius, mass, type, name, age, rot);
        assign_stellar_values(lum, spectrum, temp);
        shape_body(mass, radius);
        set_spawn(god.transform.position);
    }
    void set_spawn(Vector3 pos) //assegna la posizione del sole
    {
        obj.transform.position = pos;
    }

    void assign_stellar_values(float lum, char spectrum, float temp) //Assegno i valori specifici cell'oggetto stellare
    {
          StellarObject dati_stella = obj.GetComponent<StellarObject>();
          dati_stella.spectre = spectrum; dati_stella.lum = lum; dati_stella.temp = temp;
    }

    //Funzioni Comuni a tutti i tipi di oggetti 
    void shape_body(float mass, float radius) //Assegna forma e massa all'oggetto
    {
        give_mass(mass);
        give_radius(radius);
    }
    void give_radius(float radius) //assegna il raggio all'oggetto
    {
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * (radius / obj.GetComponent<CircleCollider2D>().radius), obj.transform.localScale.y * (radius / obj.GetComponent<CircleCollider2D>().radius), 0);
    }
    void give_mass(float mass) //assegna la massa all'oggetto
    {
        obj.GetComponent<Rigidbody2D>().mass = mass;
    }
    void create_body(string type, GameObject system) //crea l'oggetto e ne aggiunge una componente rigidbody
    {
        obj = GameObject.Instantiate(god.template, system.transform);
        add_component(type);
    }
    void add_component(string type) //Aggiunge la componente determinata da type all'oggetto
    {
        if (type.CompareTo(fun.classificazioneOggetti[0]) == 0 || type.CompareTo(fun.classificazioneOggetti[2]) == 0) //se l'oggetto e' un pianeta o una luna ne creo l'oggetto corrispondente
        {
            obj.AddComponent<PlanetaryObject>();
        }
        else if (type.CompareTo(fun.classificazioneOggetti[1]) == 0) //se l'oggetto e' una stella
        {
            obj.AddComponent<StellarObject>();
        }
    }
    void assign_base_values(float radius, float mass, string type, string name,
        float age, float rot) //gestisce l'assegnazione dei valori che ogni oggetto possiede
    {
        //Gestisco i dati che ogni oggetto con superclasse Object possiede
        Object dati = obj.GetComponent<Object>();
        dati.radius = radius; dati.mass = mass; dati.type = type;
        dati.age = age; dati.rot = rot; dati.name = name;
        dati.volume = fun.get_volume(radius); dati.g = fun.get_g(god.grav_multiplier, mass, radius);
        dati.escape_vel = fun.get_escape_vel(dati.g, radius);
        dati.density = fun.get_density(dati.mass, dati.volume);
        dati.G_COST = god.grav_multiplier;
    }
}
