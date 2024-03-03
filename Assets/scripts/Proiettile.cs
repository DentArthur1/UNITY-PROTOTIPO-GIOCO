using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Proiettile : MonoBehaviour //CLASSE PER GESTIRE L'OGGETTO PROIETTILE
{
    public Rigidbody2D proiettile; //oggetto proiettile
    public Transform ship; //nave riferimento
    public Functions fun; //classe funzioni di supporto
    public int bullet_speed;  //velocita' proiettile
    public float bullet_spawn_vert; //offset di spawn del proiettile(lungo la direzione della nave)
    public float time_to_destroy; //tempo oltre il quale il proiettile si autodistrugge

    void Start()
    {
        fun = new Functions();//inizializzo classe funzioni di supporto
    }

    public void shoot_bullet() //mitraglietta classica
    {
        Triangolo triangle = ship.GetComponent<Triangolo>(); //oggetto triangolo estratto dal transform
        Rigidbody2D proiettile_clone; //clone del proiettile da creare
        proiettile_clone = Instantiate(proiettile, triangle.transform.position + fun.partition_vect(triangle.direction) * bullet_spawn_vert, triangle.transform.rotation); //creo il nuovo oggetto proiettile(con un offsett rispetto alla prua della nave)
        Vector3 bullet_direction = fun.partition_vect(triangle.direction); //ottengo il vettore della direzione del proiettile(uso la funz. partition_vect per ottenere il vettore della direzione))
        proiettile_clone.velocity = bullet_direction * bullet_speed; //calcolo la velocita' del proiettile e gliela assegno
        Destroy(proiettile_clone.gameObject, time_to_destroy); //elimina il proiettile dopo time_to_destroy
    }

    
}