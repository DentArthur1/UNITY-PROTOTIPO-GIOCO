using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour //SCRIPT PER IL MOVIMENTO DELLA VIDEOCAMERA PRINCIPALE
{
    public Camera main_cam; //camera da muovere
    public Transform target; //target da seguire
    public float offset_x;
    public float offset_y;
    public float smooth_follow; //costante per la velocita' di risposta della camera al movimento del target
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 off_set_target = new Vector3(target.position.x + offset_x, target.position.y + offset_y, 0);
        main_cam.transform.position = Vector3.Lerp(main_cam.transform.position, off_set_target, Time.deltaTime * smooth_follow); //interpola linearmente il valore della posizione della camera tra quello attuale e il target
    }
}
