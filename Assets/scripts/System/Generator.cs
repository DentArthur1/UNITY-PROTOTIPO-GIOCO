using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator: MonoBehaviour //CLASSE PER GESTIRE LA GENERAZIONE PROCEDURALE DEI SISTEMI
{
    public GameObject System;
    public ObjectGenerator obj_gen = new ObjectGenerator();
    public int max_moons_pp = 3; //numero massimo di lune per pianeta
    public int max_planet_ps = 10; //numero massimo di pianeti per ogni sole
    //range di valori generati in base a criteri (RANGE GENERATI)
    (float, float) mass_range, radius_range; //modificati al momento della generazione degli oggetti
    //range di valori arbitrario scelto da me (RANGE ARBITRARI)
    public (float, float) density_range = (0.1f, 5f), albedo_range = (0.1f, 1f), age_range = (0.1f, 100f), lum_range = (0.1f, 100f),
                          rot_range = (0f, 10f);//modificati nell'editor unity

    public Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>> generate_system() //(PROVVISORIO)
    {
        GameObject sun = obj_gen.initialize_obj(1f, 500000f, "stella","sole", System);
        Rigidbody2D sun_obj = sun.GetComponent<Rigidbody2D>();
        GameObject earth = obj_gen.initialize_obj(0.3f, 5000f, "pianeta","terra", System, 20f, sun_obj);
        Rigidbody2D earth_obj = earth.GetComponent<Rigidbody2D>();
        GameObject moon = obj_gen.initialize_obj(0.05f, 50f, "luna", "luna", System, 0.7f, earth_obj);
        Rigidbody2D moon_obj = moon.GetComponent<Rigidbody2D>();
        // Creazione del dizionario composto come mostrato (per gentil concessione di chatgpt)
        Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>> system =
            new Dictionary<Rigidbody2D, Dictionary<Rigidbody2D, List<Rigidbody2D>>>()
            {
                {
                    // Coppia chiave-valore esterna
                    sun_obj, new Dictionary<Rigidbody2D, List<Rigidbody2D>>()
                    {
                        {
                            // Coppia chiave-valore interna
                            earth_obj, new List<Rigidbody2D>() {moon_obj} //luna terra
                        }
                    }
                }
            };
        return system;
    }


    List<Rigidbody2D> generate_moons(GameObject planet, GameObject sun)//genera le lune attorno al pianeta (Orbitee --> oggetto attorno al quale le lune orbitano)
    {
        List<Rigidbody2D> moons = new List<Rigidbody2D>();
        int moon_number = Random.Range(0, max_moons_pp); //generiamo da 0 a 3 possibili lune per ogni pianeta
        //determino i range dei valori da assegnare
        Object dati_pianeta = planet.GetComponent<Object>();
        mass_range = get_range(dati_pianeta.mass);
        radius_range =  get_range(dati_pianeta.radius);

        //genero le lune
        for(int i = 0; i < moon_number; i++) //per ogni luna da generare le assegno caratteristiche semi-randomiche(dipendenti dalle caratteristiche di Orbitee)
        {
            float moon_radius = Random.Range(radius_range.Item1, radius_range.Item2);
            float moon_mass = Random.Range(mass_range.Item1, mass_range.Item2);

        }
        return null; //da completare***
    }

    (float, float) get_range(float datum) //ottiene range in scala rispetto al valore datum
    {
        return (datum / 15,  datum / 10);
    }
}
