using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


public class ObjectGenerator //CLASSE PER GESTIRE LA GENERAZIONE DEGLI OGGETTI PIANETA, LUNA e STELLA
{
    //Classi di supporto
    public GameObject obj; //oggetto creato e a cui viene e assegnata la classe Object
    public GameObject System; //oggetto sistema 
    private Gravitation god; //classe gravitation
    private Functions fun = new Functions(); //classe funzioni ausiliarie

    //COSTRUZIONE PIANETA
    public GameObject initialize_planet(float radius, float mass, string type, string name, GameObject sys, float distance, Rigidbody2D parent,
        float age, float rot,float albedo, Functions.CompTuple[] terrain_comp, Functions.CompTuple[] atm_comp) //Wrapper per gestire la generazione dell'oggetto pianeta
    {
        god = sys.GetComponent<Gravitation>(); //riferimento classe gravitation(sistema)
        generate_planet(radius, mass, type, name, distance, parent, age, rot, albedo, terrain_comp, atm_comp, sys); //generazione oggetto
        return obj;
    }
    void generate_planet(float radius, float mass, string type, string name, float distance, Rigidbody2D parent,
       float age, float rot,float albedo, Functions.CompTuple[] terrain_comp, Functions.CompTuple[] atm_comp, GameObject sys)
    {
        create_body(type, sys);
        assign_base_values(radius, mass, type, name,age, rot);
        assign_planet_values(parent, distance, albedo, terrain_comp, atm_comp);
        shape_body(mass, radius);
        give_distance(distance, parent);
    }
    void give_distance(float distance, Rigidbody2D parent) //assegna la distanza dalla stella o pianeta 
    {
        obj.transform.position = new Vector3(parent.transform.position.x + distance, parent.transform.position.y, 0);
    }
    void assign_planet_values(Rigidbody2D parent, //assegno i valori specifici dell'oggetto pianeta
         float distance, float albedo, Functions.CompTuple[] terrain_comp, Functions.CompTuple[] atm_comp)
    {
        Planet dati_pianeta = obj.GetComponent<Planet>();
        dati_pianeta.terrain_comp = terrain_comp; dati_pianeta.albedo = albedo;
        dati_pianeta.parent = parent; dati_pianeta.distance = distance;
        dati_pianeta.atm_comp = atm_comp; dati_pianeta.period = fun.get_T(distance, god.grav_multiplier, dati_pianeta.mass, dati_pianeta.parent);
    }
    //COSTRUZIONE STELLA
    public GameObject initialize_star(float radius, float mass, string type, string name, GameObject sys,
        float age, float rot,float lum, char spectrum) //Wrapper per gestire la generazione dell'oggetto stella
    {
        god = sys.GetComponent<Gravitation>();
        generate_star(radius, mass, type, name, age, rot, lum, spectrum, sys);
        return obj;
    }
    public void generate_star(float radius, float mass, string type, string name,
       float age, float rot,float lum, char spectrum, GameObject sys)
    {
        create_body(type, sys);
        assign_base_values(radius, mass, type, name, age, rot);
        assign_star_values(lum, spectrum);
        shape_body(mass, radius);
        set_spawn(god.transform.position);
    }
    void set_spawn(Vector3 pos) //assegna la posizione del sole
    {
        obj.transform.position = pos;
    }

    void assign_star_values(float lum, char spectrum) //Assegno i valori specifici cell'oggetto stella
    {
          Star dati_stella = obj.GetComponent<Star>();
          dati_stella.spectre = spectrum; dati_stella.lum = lum;
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
        obj.AddComponent<Rigidbody2D>(); obj.GetComponent<Rigidbody2D>().angularDrag = 0; obj.GetComponent<Rigidbody2D>().gravityScale = 0;
        add_component(type);
    }
    void add_component(string type) //Aggiunge la componente determinata da type all'oggetto
    {
        if (type.CompareTo(fun.classificazioneOggetti[0]) == 0 || type.CompareTo(fun.classificazioneOggetti[2]) == 0) //se l'oggetto e' un pianeta o una luna ne creo l'oggetto corrispondente
        {
            obj.AddComponent<Planet>();
        }
        else if (type.CompareTo(fun.classificazioneOggetti[1]) == 0) //se l'oggetto e' una stella
        {
            obj.AddComponent<Star>();
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
        dati.escape_vel = fun.get_escape_vel(god.grav_multiplier, radius, mass);
        dati.density = fun.get_density(dati.mass, dati.volume);
    }
}
