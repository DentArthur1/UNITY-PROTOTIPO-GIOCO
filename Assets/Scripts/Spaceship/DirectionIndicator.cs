using UnityEngine;

public class DirectionIndicator : MonoBehaviour //CLASSE PER GESTIRE IL MOVIMENTO DELLA FRECCIA
{
    public Rigidbody2D ship; //oggetto nave da rappresentare
    public Functions fun; //Classe funzioni di supporto
    private Triangolo triangle; 
    public GameObject DirFinder; //oggetto Dir
    public GameObject arrow; //oggetto freccia da controllare(true vel)
    public GameObject engine; //oggetto freccia engine
    public GameObject thruster; //oggetto freccia thruster
    public GameObject expected; //oggetto freccia target
    public Camera cam; //Camera
    public int anchor_x; //coordinate x posizione freccia sullo schermo
    public int anchor_y; //coordinate y posizione freccia sullo schermo
    private float scale_x; //dimensione_x originale freccia
    private float scale_y; //dimensione_y originale freccia
    private float cam_stock; //zoom iniziale camera
    private float max_vec_magnitude; //limite massimo di magnitudine vettore velocita'(legato sempre all'engine)

    void Start()
    {
        fun = new Functions();
        cam = Camera.main;
        cam_stock = cam.orthographicSize; //zoom iniziale camera
        scale_x = arrow.transform.localScale.x; //salvo i valori scale iniziali (x)
        scale_y = arrow.transform.localScale.y; //salvo i valori scale iniziali (y)
        triangle = ship.GetComponent<Triangolo>(); //oggetto triangolo estratto dal tranform
        max_vec_magnitude = new Vector2(0, triangle.ship_config.Engine.max_vel + triangle.ship_config.Engine.boost_target_offset).magnitude; //magnitudine massima rappresentabile dal vettore grafico
        ship = ship.GetComponent<Rigidbody2D>(); //oggetto RigidBody2d estratto dal transform
    }
    void Update()
    {
        anchor_arrows();
        scale_arrow();
        rotate_arrows();
    }
    
    void anchor_arrows()
    {
        fun.anchor_obj(arrow, anchor_x, anchor_y, cam); //Ancora la posizione della freccia true vel alle coordinate specificate sullo schermo
        fun.anchor_obj(engine, anchor_x, anchor_y, cam); //Ancora la posizione della freccia engine vel alle coordinate specificate sullo schermo
        fun.anchor_obj(thruster, anchor_x, anchor_y, cam); //Ancora la posizione della freccia thruster vel alle coordinate specificate sullo schermo
        fun.anchor_obj(expected, anchor_x, anchor_y, cam); //Ancora la posizione della freccia target vel alle coordinate specificate sullo schermo
    }
    void scale_arrow() { //scala la grandezza della freccia in modo che sia costante ai cambiamenti di zoom  e ne applica la magnitudine corretta
        arrow.transform.localScale = fun.scale_obj(scale_x, scale_y * fun.remap_value(ship.linearVelocity.magnitude, 0, max_vec_magnitude, 0, 1), arrow.transform.localScale, cam, cam_stock);
        engine.transform.localScale = fun.scale_obj(scale_x, scale_y * fun.remap_value((triangle.ship_config.control_panel.engine_vel * fun.partition_vect(triangle.ship_config.control_panel.direction)).magnitude, 0, max_vec_magnitude, 0, 1), engine.transform.localScale, cam, cam_stock);
        thruster.transform.localScale = fun.scale_obj(scale_x, scale_y * fun.remap_value((triangle.ship_config.control_panel.thruster_vel * fun.partition_vect(triangle.ship_config.control_panel.direction)).magnitude, 0, max_vec_magnitude, 0, 1), thruster.transform.localScale, cam, cam_stock);
        expected.transform.localScale = fun.scale_obj(scale_x, scale_y * fun.remap_value(triangle.ship_config.control_panel.vel.magnitude, 0, max_vec_magnitude, 0, 1), expected.transform.localScale, cam, cam_stock);
    }

    void rotate_arrows() //ruota la freccia per indicare la direzione attuale della nave
    {
        arrow.transform.up = ship.linearVelocity; //true vel
        engine.transform.up = triangle.ship_config.control_panel.engine_vel * fun.partition_vect(triangle.ship_config.control_panel.direction); //engine vel
        thruster.transform.up = triangle.ship_config.control_panel.thruster_vel * fun.partition_vect(triangle.ship_config.control_panel.direction + 90); //thruster vel
        expected.transform.up = triangle.ship_config.control_panel.vel; //target vel
    }

}
