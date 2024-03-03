using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour //SCRIPT PER IL MOVIMENTO DELLA VIDEOCAMERA PRINCIPALE
{
    public Camera main_cam; //camera da muovere
    public Transform target; //target da seguire
    public float smooth_follow; //costante per la velocita' di risposta della camera al movimento del target
    public float zoom_scroll; //quantita' di zoom ottenuto per scroll del mouse
    public float zoom_speed; //velocita' raggiungimento zoom
    private float scrollwheel_input; //input dell'utente
    public float max_zoom_in; //limite zoom in
    public float max_zoom_out; //limite zoom out

    void Start()
    {
        main_cam = Camera.main; //setto la camera come camera principale
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 off_set_target = new Vector3(target.position.x, target.position.y, 0);
        main_cam.transform.position = Vector3.MoveTowards(main_cam.transform.position, off_set_target, Time.deltaTime * smooth_follow); //interpola linearmente il valore della posizione della camera tra quello attuale e il target
        scrollwheel_input = - Input.GetAxis("Mouse ScrollWheel") * zoom_scroll; //ottengo l'input del mouse
        main_cam.orthographicSize = Mathf.Lerp(main_cam.orthographicSize, main_cam.orthographicSize + scrollwheel_input, Time.deltaTime * zoom_speed); //applico la variazione allo zoom della camera
        main_cam.orthographicSize = Mathf.Clamp(main_cam.orthographicSize, max_zoom_in, max_zoom_out); //confino lo zoom della camera tra max_zoom_in e max_zoom_out
    }
}