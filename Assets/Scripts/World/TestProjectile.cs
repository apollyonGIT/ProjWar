using Commons;
using UnityEngine;

namespace World
{
    public enum Projectile_Movement_Status
    {
        normal,
        embed_object,
        embed_earth,
    }

    public class TestProjectile : MonoBehaviour
    {
        public bool is_arrow;
        public float mass;

        public Vector2 position;
        public Vector2 velocity;
        public Vector2 direction;

        public int life_ticks;
        public float radius;

        public Projectile_Movement_Status status;

        public void Init(Vector2 dir,Vector2 position,float random_angle,float speed)
        {
            var rnd = Random.Range(-random_angle,random_angle);
            direction = (Quaternion.AngleAxis(rnd,Vector3.forward) * dir).normalized;
            velocity = direction * speed;

            this.position = position;

            transform.position = new Vector3(position.x, position.y, 10);

            Destroy(gameObject,5f);
        }

        public void FixedUpdate()
        {
            if (status == Projectile_Movement_Status.normal)
            {
                var acc = Config.current.gravity;
                velocity += new Vector2(0, acc) * Time.deltaTime;
                position += velocity * Time.deltaTime;
            }

            transform.position = new Vector3(position.x, position.y, 10);
        }
    }
}
