using UnityEngine;

public class View : MonoBehaviour //SCRIPT PER IL MOVIMENTO DELLA VIDEOCAMERA PRINCIPALE
{
    public Camera main_cam; //camera da muovere
    public Transform target; //target da seguire
    public float smooth_follow; //costante per la velocita' di risposta della camera al movimento del target
    public float zoom_scroll; //quantita' di zoom ottenuto per scroll del mouse
    public float zoom_speed; //velocita' raggiungimento zoom
    public float max_zoom_in; //limite zoom in
    private float vel = 0f; //ref velocita' attuale smoothdamp
    private float zoom; //zoom attuale camera
    public float max_zoom_out; //limite zoom out

    void Start()
    {
        main_cam = Camera.main; //setto la camera come camera principale
        zoom = main_cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        zoom -= Input.GetAxis("Mouse ScrollWheel") * zoom_scroll; //ottengo l'input del mouse
        main_cam.orthographicSize = Mathf.SmoothDamp(main_cam.orthographicSize, zoom, ref vel, zoom_speed); //applico la variazione allo zoom della camera
        zoom = Mathf.Clamp(zoom, max_zoom_in, max_zoom_out); //confino lo zoom della camera tra max_zoom_in e max_zoom_out
        Vector3 off_set_target = new Vector3(target.position.x, target.position.y, 0);
        main_cam.transform.position = Vector3.MoveTowards(main_cam.transform.position, off_set_target, Time.deltaTime * smooth_follow); //interpola linearmente il valore della posizione della camera tra quello attuale e il target
    }

}