using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class IsometricGravity : MonoBehaviour //Classe per simulare una gravita' in un mondo isometrico/ in prospettiva
{
    //Per ogni oggetto fisico prendo delle informazioni:
    //1)Punto di appoggio: Tengo traccia delle coordinate di un punto detto di appoggio che indicano la locazione in cui la sprite collide con il terreno
    //Viene modificato dall'oggetto fisico, internamente
    //Se le coordinate dell'estremita' della sprite dell'oggetto corrispondono alle coordinate del punto di appoggio, allora l'oggetto e' a terra e non devo applicare alcuna gravita'
    //Se non corrispondono, applico la gravita' fino all'infinito o fino a che non raggiungo un punto di appoggio
    //Il punto di appoggio si modifica con il movimento xy del giocatore internamente allo script di movimento dell'oggetto
    //2)La variabile on_air di viene settata a true quando si vuole attivare la fisica di caduta, successivamente a caduta terminata verra' modificata da IsometricGravity

    Functions fun = new Functions();

    public float grav_range; //Raggio cerchio di rilevamento oggetti a cui applicare la gravita'
    public float grav_const; //costante gravitazionale pianeta

    void FixedUpdate() //Fixed perche' aggiorno un oggetto fisico
    {
        apply_gravity();
    }

    void apply_gravity() //Loopo attraverso ogni oggetto fisico con una componente IsoPhysicsObject e ne applico la gravita' ove necessario
    {
        Collider2D[] oggetti = Physics2D.OverlapCircleAll(transform.position, grav_range);
        foreach(var oggetto in oggetti)
        {
                GameObject target = oggetto.gameObject;
                Rigidbody2D target_rb = target.GetComponent<Rigidbody2D>();
                IsoPhysicsObject physics_data = target.GetComponent<IsoPhysicsObject>();

                if(physics_data != null) //se l'oggetto ha la componente richiesta lo analizzo
                { 
                   handle_free_fall(physics_data, target_rb, target);
                }
                
            }
        }
    void handle_free_fall(IsoPhysicsObject physics_data, Rigidbody2D target_rb , GameObject target)
    {
        if (physics_data.on_air) //Gestione free fall
        {
            if (physics_data.fall_point.y < target_rb.position.y)//Se il punto di backing si trova sotto l'oggetto allora applico la forza
            {
                target_rb.AddForce(Vector2.down * grav_const); //applico la gravita'
                physics_data.fall_point += physics_data.initial_vel * Time.deltaTime; //aggiorno il punto di caduta del player in base al movimento
            }
            else
            {
                target_rb.position = physics_data.fall_point; //setto la posizione precisa, per ritornare al punto di prima
                target_rb.velocity = Vector2.zero;  //fermo l'oggetto
                physics_data.on_air = false;
            }
        }
        //Generazione nuovi paramentri free fall(Ricalcolo)
        physics_data.on_tile = get_tile_on(target);
        if (physics_data.on_tile != "FLOOR")
        {
            physics_call(physics_data, calculate_freefall_point(target), physics_data.initial_vel);
            print(physics_data.fall_point);
        }
    }

    public void physics_call(IsoPhysicsObject physics_obj, Vector2 fall_point, Vector2 initial_vel) //Metodo per simulare una caduta all'altezza scelta
    {
        physics_obj.fall_point = fall_point;
        physics_obj.on_air = true;
        physics_obj.initial_vel = initial_vel;
    }

    public string get_tile_on(GameObject body) //Ottiene il nome della tile su cui mi trovo
    {
        RaycastHit2D tile_hit = Physics2D.Raycast(body.GetComponent<SpriteRenderer>().bounds.min, Vector2.down, 0.1f); //aggiustare la collisione
        if (tile_hit) //L'oggetto e' posizionato su una tile
        {
            return tile_hit.transform.gameObject.tag;
        } else //L'oggetto e' nel vuoto
        {
            return null;
        }
    }

    public RaycastHit2D get_first_tile_below(GameObject body) //Ottiene il nome della tile immediatamente sotto
    {
        return Physics2D.Raycast(body.GetComponent<SpriteRenderer>().bounds.min, Vector2.down, 50f); 
    }
    public Vector2 calculate_freefall_point(GameObject body)
    {
        Vector2 free_fall_point;
        free_fall_point.x = body.GetComponent<Rigidbody2D>().position.x;

        //Calcolo coordinate y di free fall
        RaycastHit2D tile_hit = get_first_tile_below(body);
        if (tile_hit && tile_hit.transform.gameObject.tag != "WALL") //Se ho una tile e non e' un muro ne calcolo le coordinate
        {
            free_fall_point.y = body.GetComponent<Rigidbody2D>().position.y - tile_hit.distance;
        } else //Non ho una tile, cado all'infinito
        {
            free_fall_point.y = float.NegativeInfinity;
        }
        return free_fall_point;
    }

    }

  

    

