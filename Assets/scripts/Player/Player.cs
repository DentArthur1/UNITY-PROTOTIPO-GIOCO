using UnityEngine;
public class Player : MonoBehaviour
{
    [Range(0f, 20f)] public float walking_speed; //velocità di camminata
    [Range(0f, 50f)] public float running_speed; //velocità di corsa
    [Range(0f, 20f)] public float chrouching_speed; //velocità di chrouch
    [Range(0f, 1000f)] public float jump_speed; //forza salto

    //Variabile di stato
    public int moving_state; //1Running, 2Crouching, 3Walking, 4Idle
    public bool on_air;
    public bool jumping;

    Rigidbody2D rb;
    SpriteRenderer player_sprite;
    CapsuleCollider2D player_collider;
    Vector2 player_direction;
    Vector2 base_sprite_size;
    float actual_speed; //velocità attuale

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player_sprite = GetComponent<SpriteRenderer>();
        player_collider = GetComponent<CapsuleCollider2D>();
        base_sprite_size = player_sprite.size;
    }

    void FixedUpdate() //Gestisco oggetti fisici
    {
        update_player();
    }
    void Update()
    {
        handle_inputs();
    }

    void handle_inputs() //Gestisco gli input 
    {
        player_direction.x = Input.GetAxisRaw("Horizontal"); //Ottengo input direzionale

        if (Input.GetKey(KeyCode.LeftShift)) //Corsa
        {
            moving_state = 1;
        }else if (Input.GetKey(KeyCode.LeftControl)) //Crouch
        {
            moving_state = 2;
        } else if (player_direction.x != 0) //Walking
        {
            moving_state = 3;
        } else //Idle
        {
            moving_state = 4;
        }

        //Jumping
        if(Input.GetKeyDown(KeyCode.Space) && !on_air)
        {
            jumping = true;
        }

    }

    void update_player() //Aggiorno il rigidbody del player
    {
        handle_moving_state();
        manage_jump();
        manage_on_air();
    }

    void manage_jump() //Gestisco il salto
    {
        if (jumping)
        {
            rb.AddForce(Vector2.up * jump_speed);
            jumping = false;
        }  
    }

    void manage_on_air() //Gestisco la varaibile on_air
    {
        if (rb.velocity.y != 0)
        {
            on_air = true;
        } else
        {
            on_air= false;
        }
    }

    void crouch() //Attiva modalità di crouch
    {
        actual_speed = chrouching_speed;
        player_sprite.size = new Vector2(base_sprite_size.x, base_sprite_size.y / 2);
        player_collider.size = new Vector2(base_sprite_size.x, base_sprite_size.y / 2);
    }

    void handle_moving_state() //Esegui lo stato ottenuto dagli input
    {
        //Attivo i valori di base del player
        player_sprite.size = base_sprite_size;
        player_collider.size = base_sprite_size;
        switch (moving_state) 
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
        rb.AddForce(player_direction * actual_speed);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, actual_speed);

    }
}
