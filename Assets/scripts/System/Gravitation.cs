using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gravitation : MonoBehaviour //Classe per gestire l'attrazione gravitazionale tra i grandi corpi nello spazio (Classe Gesu' Cristo)
{
    public Functions fun;
    public GameObject template;
    public float grav_range; //range di applicazione dell'attrazione gravitazionale
    public float grav_multiplier; //moltiplicatore effetti gravita' (Functions.G per ottenere il valore reale)
    public GameObject system; //sistema di oggetti a cui viene applicato lo script

    public GameObject gen; //classe generatrice di sistemi
    private Dictionary<Rigidbody2D, Dictionary<Rigidbody2D,List<Rigidbody2D>>> system_list; // struttura dati sistema
    void Start()
    {
        fun = new Functions();
        system_list = gen.GetComponent<Generator>().generate_system(); //ottengo il sistema generato
        initial_rotation(); //inizializzo le velocita' angolari iniziali
        initial_velocity(); //attivo le velocita' tangenziali iniziali
    }

    void FixedUpdate()
    {
        gravitation(); //attivo le forze gravitazionali
    }

    //CALCOLO OGGETTI ENTRO IL RAGGIO
    Collider2D[] scan(GameObject obj) //calcolo oggetti nel raggio 
    {
        Collider2D[] stars = Physics2D.OverlapCircleAll(obj.transform.position, grav_range); //ottengo tutti gli oggetti che collidono con la sfera immaginaria
        List<Collider2D> stars_list = new List<Collider2D>();
        stars_list.AddRange(stars); //creo una copia dell'array sottoforma di lista
        stars_list.RemoveAll(oggetto => oggetto.transform.name == obj.transform.name); //escludo l'oggetto a cui e' attaccato lo script di essere classificato fra le star
        stars = stars_list.ToArray(); //riconverto la lista di oggetti stella a un array
        return stars;
    }

    //CALCOLO ATTRAZIONE GRAVITAZIONALE(Legge universale di gravitazione)
   
    void apply_gravitation(GameObject star, GameObject planet) //applica la forza esercitata da star su planet (Metodo Wrapper)
    {
        Vector2 force = fun.calculate_grav_force(star, planet, grav_multiplier);
        planet.GetComponent<Rigidbody2D>().AddForce(force);
    }
    void gravitation() //calcola e applica tutte le forze gravitazionali attive su tutti gli oggetti presenti nel sistema
    {
        Collider2D[] objs = Physics2D.OverlapCircleAll(system.transform.position, grav_range); //ottengo tutti gli oggetti che collidono con la sfera immaginaria
        foreach (Collider2D obj in objs)
        {
            calculate_gravitations(obj.gameObject);
        }
    }

    void calculate_gravitations(GameObject obj) //calcola tutte le forze gravitazionali attive sul singolo oggetto
    {
        Collider2D[] stars = scan(obj.gameObject);
        foreach (Collider2D star in stars) //per ogni oggetto rilevato dall'oggetto nel sistema
        {
            GameObject star_obj = star.gameObject;
            apply_gravitation(star_obj, obj.gameObject); //applica la forza esercitata dall'oggetto rilevato sull'oggetto nel sistema
        }
    }
    //CALCOLO VELOCITA' TANGENZIALE INIZIALE
    void initial_velocity() //calcola la velocita' tangenziale inziale dell'oggetto(influenzata dall'oggetto star) --> tale per cui Fgrav == Fcentripeta --> Ovvero tale per cui l'oggetto rimanga in orbita(ANTIORARIO)  (DA PERFEZIONARE)
    {
        foreach(var sole in system_list) //per ogni oggetto sole 
        {
           foreach(var planet in sole.Value) //per ogni pianeta appartenente al sole
            {
                Vector2 planet_vel = apply_init_vel(sole.Key.transform, planet.Key.transform); //velocita' orbita pianeta-->sole
                foreach(var moon in planet.Value)
                {
                    apply_init_vel(planet.Key.transform, moon.transform); //velocita' orbita luna-->pianeta
                    moon.velocity += planet_vel; //velocita' orbita luna = velocita' orbita luna-->pianeta + velocita' orbita pianeta-->sole
                    moon.gameObject.GetComponent<PlanetaryObject>().initial_velocity += planet_vel;
                }
            }
        }
    }

    Vector2 apply_init_vel(Transform sun, Transform planet) //applica la velocita' tangenziale iniziale necessaria all'oggetto planet per orbitare attorno all'oggetto sun
    {
        Vector2 planet_vect = planet.gameObject.GetComponent<PlanetaryObject>().initial_velocity;
        planet.GetComponent<Rigidbody2D>().velocity = planet_vect; //applico la velocita
        //per ogni luna che orbita attorno al pianeta, calcolo la loro velocita' orbitale attorno al pianeta
        return planet_vect;
    }

    //rotazione pianeti
    void initial_rotation()
    {
        foreach (var sole in system_list)
        {
            apply_rotation(sole.Key.transform);
            foreach (var planet in sole.Value)
            {
                apply_rotation(planet.Key.transform);
                foreach (var moon in planet.Value)
                {
                    apply_rotation(moon.transform);
                }
            }
        }
    }
    void apply_rotation(Transform obj)
    {
        float rot_vel = obj.GetComponent<Object>().rot;
        obj.GetComponent<Rigidbody2D>().angularVelocity = rot_vel;
    }
   
}
