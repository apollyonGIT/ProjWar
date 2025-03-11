using AutoCodes;
using Commons;
using Foundations;
using UnityEngine;
using World.Projectiles;
using static World.WorldEnum;

namespace World.Devices
{
    public interface IAim
    {
        public Vector2 Aim { get; set; }
    }
    public class BasicShooter : Device, IAim
    {
        public Vector2 m_aim;
        Vector2 IAim.Aim
        {
            get
            {
                return m_aim;
            }

            set
            {
                m_aim = value;
            }
        }

        private enum Device_FSM_Shooter
        {
            idle,
            shoot,
            broken,
            repeat,
        }

        private Device_FSM_Shooter fsm;

        private int basic_interval;
        private int interval;

        private bool repeating = false;
        private int repeat_shoot = 0;

        private const string ANIM_IDLE = "idle";
        private const string ANIM_SHOOT = "shoot";
        private const string ANIM_BROKEN = "idle";
        private const string BONE_FOR_ROTATE = "roll_control";

        private const float SHOOT_ERROR_DEGREE = 5F;
        protected virtual float shoot_deg_offset => 0f;
        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            fire_logics.TryGetValue(desc.fire_logic.ToString(), out var record);

            basic_interval = record.cd;
            interval = record.cd;
            rotate_speed = desc.rotate_speed.Item1;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            FSM_change_to(Device_FSM_Shooter.idle);

            var shoot = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.tick_percent,
                anim_event = (Device d) =>
                {

                    int repeat = (int)record.repeat + BattleContext.instance.projectile_repeate_amount;
                    Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
                    if (repeat > 1)
                    {
                        if (repeating == false)              //第一次重复射击
                        {
                            repeating = true;
                            repeat_shoot = repeat;          //宣告开始重复

                            single_shoot(record);
                            repeat_shoot--;
                        }
                        else
                        {
                            if (repeat_shoot > 1)            //重复中
                            {
                                single_shoot(record);
                                repeat_shoot--;
                            }
                            else
                            {
                                repeating = false;
                                single_shoot(record);                            //重复的最后一轮
                            }
                        }
                    }
                    else
                    {
                        single_shoot(record);
                    }
                }
            };
            anim_events.Add(shoot);

            var break_event = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.rapid_fire_tick_percent,
                anim_event = (Device d) =>
                {
                    if (repeating)
                    {
                        ChangeAnim(ANIM_SHOOT, false);
                    }
                }
            };
            anim_events.Add(break_event);

            var back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = 1f,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Shooter.idle);
                }
            };
            anim_events.Add(back_to_idle);
        }
        private void single_shoot(fire_logic record)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            var salvo = record.salvo + BattleContext.instance.projectile_salvo_amount;

            for (int i = 0; i < salvo; i++)
            {
                var angle = 2 * record.angle;
                var ave_a = angle / salvo;
                var angle_1 = -record.angle + (salvo - i - 1) * ave_a;
                var angle_2 = record.angle - i * ave_a;

                float speed;
                float init_speed;
                if (record.speed.Item2 == 0)
                {
                    speed = record.speed.Item1;
                    init_speed = record.speed.Item1;
                }
                else
                {
                    speed = Random.Range(record.speed.Item1, record.speed.Item2);
                    init_speed = (record.speed.Item1 + record.speed.Item2) * 0.5f;
                }

                var plt = record.projectile_life_ticks.Item2 == 0 ? record.projectile_life_ticks.Item1 : Random.Range(record.projectile_life_ticks.Item1, record.projectile_life_ticks.Item2);

                projectiles.TryGetValue(record.projectile_id.ToString(), out var projectile_record);
                var p = new Projectile();
                float rot_speed = projectile_record.inertia_moment > 0 ? projectile_record.mass * init_speed / projectile_record.inertia_moment : 0;
                rot_speed *= Random.Range(-1f, 1f);

                switch (projectile_record.ammo_type)
                {
                    case "Bullet":
                        p = new BulletProjectile();
                        break;
                    case "Arrow":
                        p = new ArrowProjectile();
                        break;
                    default:
                        
                        break;
                }

                var shoot_bone_dir = bones_direction[BONE_FOR_ROTATE];
                var shoot_dir = Quaternion.AngleAxis((shoot_bone_dir.x >= 0 ? 1 : -1) * shoot_deg_offset, Vector3.forward) * shoot_bone_dir;

                var rot_propulsion = (Random.value + Random.value - 1f) * projectile_record.propulsion_error;  // 取两次value相加是为了改变概率密度，不能改成乘2

                p.Init(shoot_dir, key_points[record.bone_name].position, angle_1, angle_2, speed, init_speed, Faction.player, projectile_record, plt, record.damage, rot_speed, rot_propulsion);
                pmgr.AddProjectile(p);
            }
        }
        protected override bool try_get_target()
        {
            var t = BattleUtility.select_target_in_circle_min_angle(position, bones_direction[BONE_FOR_ROTATE], desc.basic_range.Item2, faction, (ITarget t) =>
            {
                return target_can_be_selected(t);
            });
            if (t != null)
            {
                target = t;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 旋转骨骼去瞄准敌人
        /// </summary>
        /// <param name="bone_name"></param>
        protected override void rotate_bone_to_target(string bone_name)
        {
            Vector2 target_v2;

            if (m_aim != Vector2.zero)
            {
                target_v2 = m_aim - position;
            }
            else if (target != null)
            {
                target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            }
            else
            {
                return;
            }

            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], target_v2, rotate_speed);
        }
        /// <summary>
        /// 判断target是否仍在范围内
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected override bool target_can_be_selected(ITarget t)
        {
            var t_distance = (t.Position - position).magnitude;
            return t.hp > 0 && t_distance >= desc.basic_range.Item1 && t_distance <= desc.basic_range.Item2;
        }

        protected virtual bool can_shoot()
        {
            return can_shoot_check_cd() && can_shoot_check_error_angle();
        }

        protected bool can_shoot_check_cd()
        {
            return interval <= 0;
        }

        protected bool can_shoot_check_error_angle()
        {
            Vector2 target_v2;
            if (m_aim != Vector2.zero)
            {
                target_v2 = m_aim - position;
            }
            else if (target != null)
            {
                target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            }
            else
            {
                return false;
            }
            var current_v2 = bones_direction[BONE_FOR_ROTATE];
            var delta_deg = Mathf.Abs(Vector2.SignedAngle(current_v2, target_v2));
            return delta_deg <= SHOOT_ERROR_DEGREE;
        }

        private void FSM_change_to(Device_FSM_Shooter target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Shooter.idle:
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_IDLE, true);
                    }
                    rotate_speed = desc.rotate_speed.Item1;
                    break;
                case Device_FSM_Shooter.shoot:
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_SHOOT, false);
                    }
                    rotate_speed = desc.rotate_speed.Item2;
                    break;
                case Device_FSM_Shooter.broken:
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_BROKEN, true);
                    }
                    break;
                default:
                    break;
            }
        }

        public override void tick()
        {
            if (!is_validate)
            {
                FSM_change_to(Device_FSM_Shooter.broken);
            }
            else
            {
                rotate_bone_to_target(BONE_FOR_ROTATE);
            }

            switch (fsm)
            {
                case Device_FSM_Shooter.idle:
                    if (target == null || !target_can_be_selected(target))
                    {
                        target = null;
                        try_get_target();
                    }
                    interval--;
                    if (can_shoot())
                    {
                        interval = basic_interval;
                        FSM_change_to(Device_FSM_Shooter.shoot);
                    }
                    break;
                case Device_FSM_Shooter.shoot:
                    if (target == null || !target_can_be_selected(target))
                    {
                        target = null;
                        try_get_target();
                    }
                    break;
                case Device_FSM_Shooter.broken:
                    if (is_validate)
                    {
                        FSM_change_to(Device_FSM_Shooter.idle);
                    }
                    break;
                default:
                    break;
            }
            base.tick();
        }
    }
}