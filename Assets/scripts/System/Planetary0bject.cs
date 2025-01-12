using UnityEngine;
using static Functions;
using Unity.VisualScripting;
public class PlanetaryObject : Object //Classe per gestire i corpi planetari 
{
    //FARE UNA STRUCT
    public Rigidbody2D parent; //corpo attorno al quale object orbita
    public Vector2 initial_velocity; //Velocit� iniziale corpo planetario
    public Vector2 absolute_vector; //Vettore velocit� assoluta corpo planetario
    public float absolute_vel; //magnitudine vettore velocit� assoluta
    public Vector2 orbital_vector; //Vettore velocit� orbitale corpo planetario
    public float orbital_vel; //magnitudine vettore velocit� orbitale
    public float relative_angle; //Angolo relativo oggetto rispetto alla posizione del padre
    public float period; //tempo di rivoluzione attorno alla stella (calcolato internamente)
    public float eccentricity; //Eccentricit� orbita oggetto
    public float apoapsis; //Afelio orbita
    public float periapsis; //Perielio orbita
    public float semi_major_axis; //semi asse maggiore ellisse
    public float semi_minor_axis; //semi asse minore ellisse
    public Vector2 distance_vector; //vettore distanza stella madre
    public float distance; //distanza dalla stella madre *
    public Vector2 ellipse_center; //centro di massa tra i due oggetti
    public float influence_sphere; //raggio sfera di influenza oggetto
    public float sup_temp = 0; //temperatura superficiale oggetto (calcolato internamente)
    public CompTuple[] atm_comp; //composizione atmosfera --> (elemento, percentuale)
    public CompTuple[] terrain_comp; //composizione terreno --> (elemento, percentuale)
    public string class_; //tipo del pianeta (roccioso, gigante gassoso, waterworld, earthlike, etc)
    public float albedo; //percentuale di luce riflessa dal pianeta (no stelle)

    //Misurazione velocit� angolare
    private float last_angle;
    private float last_time;

    void Start()
    {
        measure_relative_angle();
        last_angle = relative_angle;
        last_time = Time.time;
    }
    void FixedUpdate() //FIXED perch� mi baso su oggetti regolati da fisica
    {
        update_values();
    }

    void update_values() //Aggiorno i valori orbitali del corpo
    {
        distance_vector = this.GetComponent<Rigidbody2D>().position - parent.position;
        absolute_vector = this.GetComponent<Rigidbody2D>().linearVelocity;
        absolute_vel = absolute_vector.magnitude;
        distance = distance_vector.magnitude;
        compute_mass_center();
        measure_orbital_velocity();
        eccentricity = fun.get_eccentricity(orbital_vector, distance_vector, G_COST * parent.mass, fun.get_orbital_momentum(orbital_vector, distance_vector)).magnitude;
        semi_major_axis = fun.get_semi_major_axis(G_COST * parent.mass, orbital_vel, distance);
        semi_minor_axis = semi_major_axis * Mathf.Sqrt(1 - eccentricity * eccentricity);
        periapsis = semi_major_axis * (1 - eccentricity);
        apoapsis = semi_major_axis * (1 + eccentricity);
        influence_sphere = fun.get_influence_sphere(semi_major_axis, eccentricity, mass, parent.mass);
        period = fun.get_T(semi_major_axis, G_COST, mass, parent);
    }

    void measure_relative_angle() //misuro l'angolo relativo dell'oggetto rispetto al padre
    {
        relative_angle = Mathf.Atan2((this.GetComponent<Rigidbody2D>().position - parent.position).y, (this.GetComponent<Rigidbody2D>().position - parent.position).x) * Mathf.Rad2Deg;
        if (relative_angle <= 0)
        {
            relative_angle = 360 + relative_angle;
        }
    }

    void measure_orbital_velocity() //misuro la velocit� angolare basandomi sull'ultimo intervallo di tempo e di relative_angle appena trascorso
    {
        measure_relative_angle(); //ottengo l'angolo relativo attuale dell'oggetto in    
        //calcolo velocit� orbitale
        float delta_angle = relative_angle - last_angle;
        float delta_time = Time.time - last_time;
        orbital_vel = (delta_angle / delta_time) * Mathf.Deg2Rad * distance;
        orbital_vector = distance_vector.Perpendicular1().normalized * orbital_vel; //DA CAMBIARE ---> NON RAPPRESENTA LA DIREZIONE ORBITALE --> VALIDO SOLO PER ORBITE POCO ECCENTRICHE
        //aggiorno valori last
        last_time = Time.time;
        last_angle = relative_angle;
    }

    void compute_mass_center()
    {
        ellipse_center = (mass * this.GetComponent<Rigidbody2D>().position + parent.mass * parent.position) / (mass + parent.mass);
    }

 
}//il movimento del sole causa lo sfarfallamento delle linee orbitali***

