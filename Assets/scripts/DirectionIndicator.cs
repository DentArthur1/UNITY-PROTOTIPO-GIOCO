using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class DirectionIndicator : MonoBehaviour //CLASSE PER GESTIRE IL MOVIMENTO DELLA FRECCIA
{
    public Rigidbody2D ship; //oggetto nave da rappresentare
    public Functions fun; //Classe funzioni di supporto
    public GameObject arrow; //oggetto freccia da controllare
    public Camera cam; //Camera
    public int anchor_x; //coordinate x posizione freccia sullo schermo
    public int anchor_y; //coordinate y posizione freccia sullo schermo
    private float scale_x; //dimensione_x originale freccia
    private float scale_y; //dimensione_y originale freccia
    private float cam_stock; //zoom iniziale camera
    private float mult_factor; //variabile di moltiplicazione scala oggetto freccia

    void Start()
    {
        fun = new Functions();
        cam = Camera.main;
        cam_stock = cam.orthographicSize; //zoom iniziale camera
        scale_x = arrow.transform.localScale.x; //salvo i valori scale iniziali (x)
        scale_y = arrow.transform.localScale.y; //salvo i valori scale iniziali (y)
    }
    void Update()
    {
        arrow.transform.position = cam.ScreenToWorldPoint(new Vector3(anchor_x, anchor_y, 0)); //Ancora la posizione della freccia alle coordinate specificate sullo schermo
        scale_arrow();
        rotate_arrow();
    }

    void scale_arrow() { //scala la grandezza della freccia in modo che sia costante ai cambiamenti di zoom
        mult_factor = (cam.orthographicSize / cam_stock);
        arrow.transform.localScale = new Vector3(scale_x * mult_factor, scale_y * mult_factor, arrow.transform.localScale.z);
    }

    void rotate_arrow() //ruota la freccia per indicare la direzione attuale della nave
    {
        ship = ship.GetComponent<Rigidbody2D>(); //oggetto triangolo estratto dal transform
        arrow.transform.up = ship.velocity; //rappresenta la velocita' con la freccia
    }
}
