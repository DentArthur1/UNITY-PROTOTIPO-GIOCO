using UnityEngine;
using static Functions;
public class Player : IsoPhysicsObject //Classe per gestire il movimento del player
{

    [Range(0f, 20f)] public float walking_speed; //velocit� di camminata
    [Range(0f, 50f)] public float running_speed; //velocit� di corsa
    [Range(0f, 20f)] public float chrouching_speed; //velocit� di chrouch
    [Range(0f, 1000f)] public float jump_speed; //forza salto

    //Variabile di stato
    public int moving_state; //1Running, 2Crouching, 3Walking, 4Idle
    public bool jumping;
    public bool crouching;

    //Componenti oggetto player
    public GameObject Gravitator_obj;
    IsometricGravity IsoGravity;
    Rigidbody2D rb;
    SpriteRenderer player_sprite;
    CapsuleCollider2D player_collider;
    Vector2 base_sprite_size;

    //Variabili player
    Vector2 player_direction;
    float actual_speed; //velocit� attuale

    void Start()
    {
        IsoGravity = Gravitator_obj.GetComponent<IsometricGravity>();
        rb = GetComponent<Rigidbody2D>();
        fall_point = rb.position;
        player_sprite = GetComponent<SpriteRenderer>();
        player_collider = GetComponent<CapsuleCollider2D>();
        base_sprite_size = player_sprite.size;
    }

    void FixedUpdate() //Gestisco oggetti fisici
    {
        update_player();
    }
    void Update() //Gestisco gli input dell'utente
    {
        handle_inputs();
    }

    void handle_inputs() //Gestisco gli input 
    {
        player_direction.x = Input.GetAxisRaw("Horizontal"); //Ottengo input direzionale orizzontale
        player_direction.y = Input.GetAxisRaw("Vertical"); //Ottengo input direzionale verticale
        //Azioni da eseguire continuamente
        if (Input.GetKey(KeyCode.LeftShift)) //Corsa
        {
            moving_state = 1;
        }
        else if (Input.GetKey(KeyCode.LeftControl))//Holding del crouch
        {
            moving_state = 2;
        }
        else if (player_direction.x != 0 || player_direction.y != 0) //Walking
        {
            moving_state = 3;
        }
        else //Idle
        {
            moving_state = 4;
        }

        //Azioni da eseguire solo una volta, al momento della pressione del tasto
        if (Input.GetKeyDown(KeyCode.Space) && !on_air)//Jumping
        {
            jumping = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))//Crouching
        {
            crouching = true;
        }

    }

    void update_player() //Aggiorno il rigidbody del player
    {
        handle_moving_state();
        manage_jump();
    }

    void manage_jump() //Gestisco il salto
    {
        if (jumping)
        {
            IsoGravity.physics_call(this, rb.position, rb.linearVelocity); //physics call
            jumping = false;
            rb.AddForce(Vector3.up * jump_speed);
        } 
    }

    void crouch() //Attiva modalit� di crouch
    {
        if (crouching)
        {
            IsoGravity.physics_call(this, new Vector2(rb.position.x, rb.position.y - (player_sprite.size.y / 2)), rb.linearVelocity); //physics call
            crouching = false;
        }
        actual_speed = chrouching_speed;
        player_sprite.size = new Vector2(base_sprite_size.x, base_sprite_size.y / 2); //dimezzo le dimensioni della sprite del player
        player_collider.size = new Vector2(base_sprite_size.x, base_sprite_size.y / 2); //dimezzo anche il collider
    }

    void handle_moving_state() //Esegui lo stato ottenuto dagli input
    {
        //Attivo i valori di base del player (dimensioni)
        player_sprite.size = base_sprite_size;
        player_collider.size = base_sprite_size;

        switch (moving_state)//Applico lo stato al player
        {
            case 1: //Corsa
                actual_speed = running_speed;
                break;
            case 2: //Crouch
                crouch();
                break;
            case 3: //walking
                actual_speed = walking_speed;
                break;
        }
        if (!on_air) //se il player non e' in aria o non e' gia fermo gli applico la sua velocita' attuale
        {
            rb.linearVelocity = (player_direction * actual_speed);
        }
    }
}
