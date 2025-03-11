using Commons;
using UnityEngine;
using World.Enemys;
using World.Helpers;

namespace World.Projectiles
{
    public class ArrowProjectile : Projectile
    {
        private const int MAX_IN_OBJECT_TICKS = 10;

        private int in_object_ticks = 0;


        protected override void movement_in_object()
        {
            if (in_object_ticks < 0)
            {
                var current_dir = in_target.direction;
                var angel = Vector2.SignedAngle(direction_in_object, current_dir);
                Vector2 current_offset = Quaternion.AngleAxis(angel, Vector3.forward) * pos_offset_in_object;

                position = in_target.Position + current_offset;
            }
            else
            {
                var acc_y = Config.current.gravity;
                Vector2 ammo_acc = new Vector2(0, acc_y);
                if (desc.propulsion_force > 0)
                {
                    ammo_acc += direction.normalized * desc.propulsion_force / desc.mass;
                    rot_speed += rot_propulsion * Config.PHYSICS_TICK_DELTA_TIME;
                }

                velocity += ammo_acc * Config.PHYSICS_TICK_DELTA_TIME;
                velocity *= 0.95f;  //降低速度
                position += velocity * Config.PHYSICS_TICK_DELTA_TIME;  // Move

                if (--in_object_ticks >= 0)
                    return;

                // last，check if is still collided with the same target
                if (target_select(out var target, true))
                {
                    //记录插入时 物体的 position 与 direction
                    pos_offset_in_object = position - in_target.Position;
                    direction_in_object = in_target.direction;
                }
                else
                {
                    movement_status = MovementStatus.normal;
                }
            }
        }


        protected override void rotate()
        {
            direction = velocity;
        }

        private bool target_select(out ITarget target_selected, bool same_last_target = false)
        {
            target_selected = BattleUtility.select_target_in_circle(position, radius, faction, (ITarget t) =>
            {
                return (t != last_hit) ^ same_last_target && t.is_interactive;           //此处其实这个函数没有意义,只是用于示范
            });
            return target_selected != null;
        }


        public override void HitEnemy()
        {
            if (target_select(out var target))
            {
                //1.对目标造成伤害与击退
                var target_v = Vector2.zero;
                if (target is Enemy)
                {
                    var e = target as Enemy;
                    target_v = e.velocity;
                }
                var dv_between_self_and_target = (velocity - target_v).magnitude;

                var dmg = init_speed == 0 ? damage : damage * Mathf.Pow(dv_between_self_and_target / init_speed, 2);

                Attack_Data attack_data = new()
                {
                    atk = (int)dmg
                };

                target.hurt(attack_data);
                target.impact(WorldEnum.impact_source_type.projectile, velocity, mass, Config.current.arrow_penetration_loss);

                //2.根据剩余动能，判定飞射物自身的后续运动方式
                last_hit = target;
                var ek_mul_2 = mass * Mathf.Pow(velocity.magnitude, 2);
                var delta_ek = ek_mul_2 - Config.current.arrow_penetration_loss * target.Mass;

                if (delta_ek > 0)
                {
                    velocity *= Mathf.Sqrt(delta_ek / mass) / velocity.magnitude;
                    direction = velocity.normalized;
                }
                else
                {
                    in_object_ticks = MAX_IN_OBJECT_TICKS;
                    in_target = target;
                    movement_status_change_to(MovementStatus.in_object);

                    //规则：附加自身在目标上的重量
                    target.attach_data(mass);
                }
            }
        }
        public override void HitGround()
        {
            var road_height = Road_Info_Helper.try_get_altitude(position.x);
            if (road_height >= position.y)
            {
                movement_status = MovementStatus.in_ground;
            }
        }

        public override void HitDevice()
        {
            if (target_select(out var target))
            {
                //1.对目标造成伤害与击退
                var target_v = WorldContext.instance.caravan_velocity;
                var dv_between_self_and_target = (velocity - target_v).magnitude;

                var dmg = init_speed == 0 ? damage : damage * Mathf.Pow(dv_between_self_and_target / init_speed, 2);

                Attack_Data attack_data = new()
                {
                    atk = (int)dmg
                };

                target.hurt(attack_data);
                target.impact(WorldEnum.impact_source_type.projectile, velocity, mass, Config.current.arrow_penetration_loss);

                //2.根据剩余动能，判定飞射物自身的后续运动方式
                last_hit = target;
                //车体  设备 必定插
                in_target = target;
                movement_status_change_to(MovementStatus.in_object);

                //规则：附加自身在目标上的重量
                target.attach_data(mass);
            }
        }
    }
}
