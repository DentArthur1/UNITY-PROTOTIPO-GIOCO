using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour //CLASSE DATI ASSEGNATA AGLI OGGETTI PIANETA, STELLA, LUNA DOPO LA GENERAZIONE CON ObjectGenerator e Generator
{
    public float radius; //raggio oggetto *
    public Rigidbody2D parent; //corpo attorno al quale object orbita
    public float density; //densita' oggetto  
    public float volume; //volume oggetto (calcolato internamente)
    public float sup_temp; //temperatura superficiale oggetto (calcolato internamente)
    public (string, float) atm_comp; //composizione atmosfera --> (elemento, percentuale)
    public (string, float) terrain_comp; //composizione terreno --> (elemento, percentuale)
    public float age; //eta' oggetto
    public string type; //classificazione oggetto *
    public float period; //tempo di rivoluzione attorno alla stella (calcolato internamente)
    public float rot; //tempo di rotazione sul suo asse *
    public float g; //accelerazione gravita' (calcolato internamente)
    public float mass; //massa del corpo *
    public float distance; //distanza dalla stella madre *
    public float escape_vel; //velocita' di fuga (calcolato internamente)
    public float albedo; //percentuale di luce riflessa dal pianeta (no stelle)

    //SPECIFICO STELLE
    public float lum; //luminosita' stella
    public char spectre; //classe stella
 
}
