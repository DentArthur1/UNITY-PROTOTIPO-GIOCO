using UnityEngine;

public class PlayerView : MonoBehaviour //SCRIPT PER IL MOVIMENTO DELLA VIDEOCAMERA PRINCIPALE DEL PLAYER
{
    public Camera main_cam; //camera da muovere
    public Transform target; //target da seguire
    public float smooth_follow; //costante per la velocita' di risposta della camera al movimento del target
    public float zoom_scroll; //quantita' di zoom ottenuto per scroll del mouse
    public float zoom_speed; //velocita' raggiungimento zoom
    public float max_zoom_in; //limite zoom in
    public float max_zoom_out; //limite zoom out
    private float zoom; //zoom attuale camera
    public float distance; //Distanza camera sul piano delle z
    private float vel = 0f; //ref velocita' attuale smoothdamp

    void Start()
    {
        main_cam = Camera.main; //setto la camera come camera principale
        zoom = main_cam.orthographicSize;
    }
    void Update()
    {
        zoom -= Input.GetAxis("Mouse ScrollWheel") * zoom_scroll; //ottengo l'input del mouse
        main_cam.orthographicSize = Mathf.SmoothDamp(main_cam.orthographicSize, zoom, ref vel, zoom_speed); //applico la variazione allo zoom della camera
        zoom = Mathf.Clamp(zoom, max_zoom_in, max_zoom_out); //confino lo zoom della camera tra max_zoom_in e max_zoom_out
        Vector3 off_set_target = new Vector3(target.position.x, target.position.y, target.position.z - distance);
        main_cam.transform.position = Vector3.MoveTowards(main_cam.transform.position, off_set_target, Time.deltaTime * smooth_follow); //interpola linearmente il valore della posizione della camera tra quello attuale e il target
    }

}