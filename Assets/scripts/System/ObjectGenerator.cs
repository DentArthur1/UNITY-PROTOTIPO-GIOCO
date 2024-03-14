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
    public GameObject initialize_obj(float radius,float mass, string type,string name, GameObject sys, float distance = 0, Rigidbody2D parent = null,
        float age = 0, float rot = 0, float density = 0, float albedo = 0)    //crea un oggetto con le suddette caratteristiche e ne ritorna lo stesso     (metodo costruttore)                  
    {
        System = sys;
        god = System.GetComponent<Gravitation>(); //riferimento classe gravitation(sistema)
        generate(radius,mass,type,name,distance,parent,age,rot,density, albedo); //generazione oggetto
        return obj;
    }

    //GENERAZIONE OGGETTO
    void generate(float radius, float mass, string type,string name, float distance, Rigidbody2D parent,
        float age,float rot, float density, float albedo) //funzione wrapper per inizializzare l'oggetto e le sue caratteristiche
    {
        create_object();
        assign_values(radius,mass,type,name,distance,parent,age,rot, density, albedo);
        set_spawn(); //solo per il sole
        give_distance(distance, parent);
        give_mass(mass);
        give_radius(radius);
    }
    void give_radius(float radius) //assegna il raggio
    {
        obj.transform.localScale = new Vector3(obj.transform.localScale.x * (radius / obj.GetComponent<CircleCollider2D>().radius), obj.transform.localScale.y * (radius / obj.GetComponent<CircleCollider2D>().radius), 0);  
    }
    void give_mass(float mass) //assegna la massa
    {
        obj.GetComponent<Rigidbody2D>().mass = mass;
    }

    void give_distance(float distance, Rigidbody2D parent) //assegna la distanza dalla stella o pianeta
    {
        if (obj.GetComponent<Object>().type.CompareTo("stella") < 0) //ovvero se l'oggetto e' un pianeta o una luna
        {
            obj.transform.position = new Vector3(parent.transform.position.x + distance, parent.transform.position.y, 0);
        }
    }

    void set_spawn() //assegna la posizione del sole
    {
        if (obj.GetComponent<Object>().type.CompareTo("stella") == 0)
        {
            obj.transform.position = System.transform.position;
        }
    }

     void create_object()//crea effettivamente l'oggetto RigidBody2D
    {
        obj = GameObject.Instantiate(god.template, System.transform);
        obj.AddComponent<Rigidbody2D>(); obj.GetComponent<Rigidbody2D>().angularDrag = 0; obj.GetComponent<Rigidbody2D>().gravityScale = 0;
        obj.AddComponent<Object>();
        //la posizione non viene settata qui, bensi' in give_distance
    }

    void assign_values(float radius, float mass, string type,string name,float distance, Rigidbody2D parent, //assegno i valori dell'oggetto alla sua classe object
        float age, float rot, float density, float albedo)
    {
        Object dati = obj.GetComponent<Object>();
        dati.radius = radius; dati.mass = mass; dati.type = type; dati.distance = distance; dati.parent = parent;
        dati.age = age; dati.rot = rot; dati.name = name; dati.density = density; dati.albedo = albedo;
        
        //calcolo valori mancanti
        dati.volume = fun.get_volume(mass, density); dati.period = fun.get_T(distance, god.grav_multiplier, mass,parent);
        dati.g = fun.get_g(god.grav_multiplier, mass, radius); dati.escape_vel = fun.get_escape_vel(god.grav_multiplier, radius, mass);
        //dati.sup_temp = fun.get_expected_temp(obj); gestire caso pianeta/sole
    }
   

}
