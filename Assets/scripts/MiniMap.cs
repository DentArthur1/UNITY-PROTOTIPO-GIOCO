using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public GameObject map; //oggetto mappa
    public Camera cam; //oggetto Camera
    public Functions fun;
    public GameObject triangle; //Oggetto triangolo da disegnare al centro
    public GameObject circle; //oggetto cerchio(limite mappa)
    public Rigidbody2D target; //oggetto target da blittare sulla mappa
    public Transform ship; //Oggetto nave
    public float anchor_x; //posizione x della mappa sullo schermo
    public float anchor_y; //posizione y della mappa sullo schermo
    private float scale_x; //dimensione_x originale mappa
    private float scale_y; //dimensione_y originale mappa
    private float cam_stock; //zoom iniziale camera
    public float map_range; //range della mappa(raggio cerchio invisibile con centro la navicella)
    public float circle_radius; //raggio cerchio mappa
    private float radar_time; //tempo timer radar
    public float radar_scan_time; //tempo totale timer radar
    private string[] map_filter; //array di stringhe contenente gli oggetti da non mostrare sulla mappa 
    private (Rigidbody2D[], (float, float)[]) info; //tupla contentente i dati degli oggetti trovati dal radar + le coordinate su schermo dei loro rispettivi target da disegnare sulla mappa
    void Start()
    {
        cam = Camera.main;
        fun = new Functions();
        scale_x = map.transform.localScale.x; //scala_x iniziale mappa
        scale_y = map.transform.localScale.y; //scala_y iniziale mappa
        cam_stock = cam.orthographicSize; //zoom iniziale camera
        radar_time = radar_scan_time; //settaggio timer iniziale
        map_filter = new string[] { "Proiettile", "Triangle" }; //settaggio filtro mappa
    }

    void Update()
    {
        fun.anchor_obj(map, anchor_x, anchor_y, cam); //ancoro la mappa al lato dello schermo
        rotate_triangle(); //ruoto il triangolo al centro
        map.transform.localScale = fun.scale_obj(scale_x, scale_y, map.transform.localScale, cam, cam_stock); //scalo la dimensione della mappa e dei target
        if (fun.timing(ref radar_time, radar_scan_time))
        {
            delete_targets();  //elimino i target blittati precedentemente
            info = manage_targets(); //rappresento nuovi target
            radar_time -= Time.deltaTime; //faccio partire il cooldown
        }
        move_targets(); //muovo i target con la mappa
        fun.timing(ref radar_time, radar_scan_time); //faccio scorrere il timer

    }

    void rotate_triangle() //ruota il triangolo in base alla rotazione della nave
    {
        triangle.transform.up = ship.transform.up;
    }

    Collider2D[] scan() //ottiene tutti gli oggetti nel raggio della mappa
    {

        Collider2D[] oggetti = Physics2D.OverlapCircleAll(ship.transform.position, map_range); //ottengo tutti gli oggetti che collidono con la sfera immaginaria
        List<Collider2D> lista_oggetti = new List<Collider2D>();
        lista_oggetti.AddRange(oggetti); //creo una copia dell'array sottoforma di lista
        lista_oggetti.RemoveAll(oggetto => !filter_targets(map_filter, oggetto.name)); //elimino tutti gli elementi che sono presenti nel filtro
        oggetti = lista_oggetti.ToArray(); //riconverto la lista a array
        return oggetti;
    }

    (Rigidbody2D[], (float, float)[]) manage_targets() //rappresenta i target in scala sulla minimappa
    {
        Collider2D[] oggetti = scan(); //array oggetti trovati nel raggio
        Rigidbody2D[] targets = new Rigidbody2D[oggetti.Length]; //array oggetti target da disegnare sulla mappa
        (float, float)[] positions = new (float, float)[oggetti.Length]; //array di tuple contenenti le posizioni su schermo di ogni target disegnato
        int counter = 0; //indice
        foreach(Collider2D obj in oggetti) //itera fra gli oggetti rilevati dal radar
        {
            float orig_delta_x = obj.transform.position.x - ship.transform.position.x; //calcolo deltaX
            float orig_delta_y = obj.transform.position.y - ship.transform.position.y; //calcolo deltaY
            float scaled_delta_x = fun.remap_value(orig_delta_x, -map_range, +map_range, -circle_radius, +circle_radius); //calcolo deltaX scalato
            float scaled_delta_y = fun.remap_value(orig_delta_y, -map_range, +map_range, -circle_radius, +circle_radius); //calcolo deltaY scalato
            float x_value = triangle.transform.position.x + scaled_delta_x * (cam.orthographicSize / cam_stock); //applico deltaX al triangolo al centro della minimappa
            float y_value = triangle.transform.position.y + scaled_delta_y * (cam.orthographicSize / cam_stock); //applico deltaY al triangolo al centro della minimappa
            Rigidbody2D target_copia; //copia target
            target_copia = Instantiate<Rigidbody2D>(target, new Vector3(x_value, y_value, 0), triangle.transform.rotation, map.transform); //creo il target alle coordinate calcolate
            targets[counter] = target_copia; //aggiungo il target al mio array
            Vector3 to_screen = cam.WorldToScreenPoint(target_copia.position); //ottengo le posizioni su schermo del target
            positions[counter] = (to_screen.x, to_screen.y); //inserisco il risultato nel mio array di tuple
            counter++; //incremento indice accesso 
        }           
        return (targets, positions);
    }

    void delete_targets() //elimina gli oggetti nell'array indicato
    {
        if (info.Item1 != null)
        {
            foreach (Rigidbody2D obj in info.Item1)
            {
              Destroy(obj.gameObject); //distruggo il target
            }
        }
       
    }

    void move_targets() //muove i target blittati sulla mappa con il movimento del player
    {
        if (info.Item1 != null)
        {
            int counter = 0;
            foreach(Rigidbody2D target in info.Item1)
            {
                fun.anchor_obj(target.gameObject, info.Item2[counter].Item1, info.Item2[counter].Item2, cam); //ancora il target.i alle sue coordinate su schermo calcolate in blit_targets()
                counter++; 
            }
        }
    }

    bool filter_targets(string[] filter, string name) //mostra solo gli oggetti desiderati sul radar (Tutti - filter) (DA IMPLEMENTARE)
    {
        foreach(string str in filter)
        {
            if(name.Contains(str)) //se il nome dell'oggetto contiene "name"
            {
                return false; //l'oggetto e' contenuto nel filtro
            }
        }
        return true; //l'oggetto non e' contenuto nel filtro
    }
}
