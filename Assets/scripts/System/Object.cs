using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour //SUPERCLASSE DEGLI OGGETTI PIANETA, STELLA DOPO LA GENERAZIONE CON ObjectGenerator e Generator
{
    public float radius; //raggio oggetto *
    public float density; //densita' oggetto  
    public float volume; //volume oggetto (calcolato internamente)
    public float age; //eta' oggetto
    public string type; //tipo oggetto (Oggetto planetario o stellare)
    public float rot; //tempo di rotazione sul suo asse *
    public float g; //accelerazione gravita' (calcolato internamente)
    public float mass; //massa del corpo *
    public float escape_vel; //velocita' di fuga (calcolato internamente)
}
