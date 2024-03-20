using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : Object //Classe per gestire l'oggetto pianeta
{
    public float sup_temp = 0; //temperatura superficiale oggetto (calcolato internamente)
    public Functions.CompTuple[] atm_comp; //composizione atmosfera --> (elemento, percentuale)
    public Functions.CompTuple[] terrain_comp; //composizione terreno --> (elemento, percentuale)
    public float period; //tempo di rivoluzione attorno alla stella (calcolato internamente)
    public Rigidbody2D parent; //corpo attorno al quale object orbita
    public float distance; //distanza dalla stella madre *
    public float albedo; //percentuale di luce riflessa dal pianeta (no stelle)
}
