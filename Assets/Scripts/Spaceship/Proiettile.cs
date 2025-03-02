using UnityEngine;
using static Functions;
public class Proiettile //CLASSE Arsenale nave
{
    public Functions fun = new Functions();//classe funzioni di supporto

    public void shoot_bullet(Vector3 ship_pos, float ship_dir, Quaternion ship_rot, Rigidbody2D proj_prefab, ShipWeaponConfig weapon) //mitraglietta classica
    {
        Rigidbody2D proiettile_clone; //clone del proiettile da creare
        proiettile_clone = Rigidbody2D.Instantiate(proj_prefab, ship_pos + fun.partition_vect(ship_dir) * weapon.bullet_spawn_vert, ship_rot); //creo il nuovo oggetto proiettile(con un offsett rispetto alla prua della nave)
        Vector3 bullet_direction = fun.partition_vect(ship_dir); //ottengo il vettore della direzione del proiettile(uso la funz. partition_vect per ottenere il vettore della direzione))
        proiettile_clone.linearVelocity = bullet_direction * weapon.bullet_speed; //calcolo la velocita' del proiettile e gliela assegno
        Rigidbody2D.Destroy(proiettile_clone.gameObject, weapon.time_to_destroy); //elimina il proiettile dopo time_to_destroy
    }
}