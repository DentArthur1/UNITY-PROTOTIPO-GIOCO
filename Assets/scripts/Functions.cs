using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Functions
{
    //FUNZIONI AUSILIARIE
    public float distance(Vector3 obj1, Vector3 obj2)
    { //calcola la distanza tra due punti usando pitagora(funziona solo a due dimensioni)
        return Mathf.Sqrt(Mathf.Pow(obj1.x - obj2.x, 2) + Mathf.Pow(obj1.y - obj2.y, 2));
    }

    public float remap_value(float value, float from1, float from2, float to1, float to2)
    { //Rimappa la variabile value dai limiti from1, from2 ai limiti to1, to2 (Interpolazione lineare)
        return (((to2 - to1) * (value - from1)) / (from2 - from1)) + to1;  //x1 : x2 == (value - from1) : (x - to1)
    }
}
