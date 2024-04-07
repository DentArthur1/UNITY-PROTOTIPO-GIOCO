using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoPhysicsObject : MonoBehaviour //Classe per gestire gli oggetti simulati con la fisica di IsometricGravity
{
    public Vector2 fall_point; //punto di caduta oggetto aggiornato dinamicamente da IsometricGravity ma settato inizialmente dalla classa utente
    public bool on_air; //variabile attivazione fisica
    public Vector2 initial_vel; //velocita' iniziale oggetto al momento della chiamata
    public string on_tile; //Tile su cui l'oggetto e' poggiato
}
