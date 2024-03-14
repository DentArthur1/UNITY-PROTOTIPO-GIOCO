using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Functions //CLASSE FUNZIONI DI SUPPORTO
{
    public const double G = 6.67430e-11; //costante di attrazione gravitazionale universale
    public char[] stars_spectres = new char[7] {'0','B','A','F','G','K','M'};
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

    //FORMULE PIANETI
    public float get_g(float grav_mult, float mass, float radius) //calcola l'accelerazione gravitazionale
    {
        return (grav_mult * mass) / Mathf.Pow(radius, 2);
    }

    public float get_T(float distance, float grav_mult, float mass, Rigidbody2D parent = null) //calcola il periodo di rivoluzione dell'oggetto
    {
        if (parent != null) //ovvero se l'oggetto e' un pianeta o una luna
        {
            return (2 * Mathf.PI) * Mathf.Sqrt(Mathf.Pow(distance, 3) / (grav_mult * (parent.mass + mass)));
        }
        return 0;
        //in realta distance sarebbe la media tra il punto piu' vicino e piu lontato dal sole del pianeta

    }
    public float get_escape_vel(float grav_mult, float radius, float mass) //calcola la velocita' di fuga
    {
        return Mathf.Sqrt((2 * grav_mult * mass) / radius);
    }

    public float get_volume(float mass, float density) //calcola il volume dell'oggetto (formula volume sfera)
    {
        return mass / density;
    }

    public float get_expected_temp(GameObject planet, GameObject sun) //calcola la temperatura di equilibrio del pianeta
    {
        return 0f;
    }
}
