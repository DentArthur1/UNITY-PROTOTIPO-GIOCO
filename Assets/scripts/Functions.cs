using System;
using UnityEngine;

public class Functions //CLASSE FUNZIONI DI SUPPORTO
{
    public const double G = 6.67430e-11; //costante di attrazione gravitazionale universale
    //FUNZIONI AUSILIARIE
    public float distance(Vector3 obj1, Vector3 obj2)
    { //calcola la distanza tra due punti usando pitagora(funziona solo a due dimensioni)
        return Mathf.Sqrt(Mathf.Pow(obj1.x - obj2.x, 2) + Mathf.Pow(obj1.y - obj2.y, 2));
    }

    public float remap_value(float value, float from1, float from2, float to1, float to2)
    { //Rimappa la variabile value dai limiti from1, from2 ai limiti to1, to2 (Interpolazione lineare)
        return to1 + (to2 - to1) * (value - from1) / (from2 - from1);  //x1 : x2 == (value - from1) : (x - to1)
    }

    public Boolean timing(ref float time, float timer) //ritorna true se il timer non e' stato ancora inizializzato
    {
        //funzionamento: 1)chiamata funzione timing 2)decremento time 3)chiamo la funzione in update per far proseguire il timer(time = scorrimento tempo attuale, timer = tempo totale timer)
        if (time == timer)//il timer non e' stato ancora inizializzato
        {
            return true;

        }
        else //il timer e stato inizializzato(ovvero time e' stato diminuito)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            { //se il timer ha raggiunto o superato 0, lo resetto
                time = timer;
            }
            return false;
        }
    }

    public Vector3 partition_vect(float angle)
    { //partiziona il vettore in base all'orientamento dell'oggetto 
        Vector3 vector; vector.z = 0;
        vector.y = Mathf.Sin(angle * Mathf.Deg2Rad);
        vector.x = Mathf.Cos(angle * Mathf.Deg2Rad);
        return vector;
    }

    public float normalize_orientation(float angle)
    { //notazione classica per calcolo seno e coseno
        angle += 90;
        if (angle < 0)
        {
            angle = 360 - Mathf.Abs(angle); //(normalizza da 0 a 360 gradi)
        }
        return angle;
    }

    public void anchor_obj(GameObject obj, float anchor_x, float anchor_y, Camera cam) //ancora l'oggetto alle coordinate specificate
    {
        obj.transform.position = cam.ScreenToWorldPoint(new Vector3(anchor_x, anchor_y, 0));
    }

    public Vector3 scale_obj(float scale_x, float scale_y, Vector3 LocalScale, Camera cam, float cam_stock) //scala l'oggetto in base allo zoom attuale della videocamera
    {
        return new Vector3(scale_x * (cam.orthographicSize / cam_stock), scale_y * (cam.orthographicSize / cam_stock), LocalScale.z);
    }

    public bool filter_targets(string[] filter, string name) //controlla se name e' contenuto in filter
    {
        foreach (string str in filter)
        {
            if (name.Contains(str)) //se il nome dell'oggetto contiene "name"
            {
                return false; //l'oggetto e' contenuto nel filtro
            }
        }
        return true; //l'oggetto non e' contenuto nel filtro
    }

}
