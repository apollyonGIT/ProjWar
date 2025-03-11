using AutoCodes;
using Commons;
using UnityEngine;
using World.Enemys.BT;
using World.Enemys;
using World.Helpers;
using UnityEngine.UIElements;

namespace World.Devices.NewDevice
{
    public class NewBasicHook : Device, IAttack, IRecycle, ILoad
    {
        #region Const
        private const string BONE_FOR_ROTATION = "roll_control";
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "shoot";
        private const string ANIM_HOOKING = "hanger_fired";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_NAME_FOR_MUZZEL = "proj_muzzel";
        private const float MAX_ROPE_DISTANCE = 10F;
        private const float SHOOT_ERROR_DEGREE = 5F;
        private const float AIMING_Y_OFFSET = 0.35F;

        #endregion

        private float move_speed_shooting;
        private float move_speed_hooked;
        private float move_speed_reeling_in;

        public Vector2 hook_position = Vector2.zero;
        Vector2 muzzel_pos;
        private Vector2 hook_v;

        private int dmg_init;
        private int dmg_by_period;
        private int dmg_interval_max;
        private int dmg_interval_current;

        public float rope_current_length;
        public float rope_limit_length;

        private float rope_elasticity;
        private float rope_breaking_overlength;

        public int rope_current_hp;
        public int rope_max_hp = 5;

        private float attack_range_max;
        private Vector2 self_traction = Vector2.zero;          //都做v2 处理，int时只处理x
        private Vector2 opposing_traction;

        public float rotate_angle = 0;

        public enum Device_FSM_Hook
        {
            Idle,
            Ready_to_Shoot,
            Shooting,
            Shooting_reeling_in,
            Hooked,
            Breaking_reeling_in,
            Broken,
        }

        private Device_FSM_Hook m_fsm;
        public Device_FSM_Hook fsm => m_fsm;

        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }       //把射击间隔当做 上弦来用

        public int Recycle_Interval { get; set; }
        public int Current_Recycle_Interval { get; set; }

        public int Max_Ammo { get; set; }
        public int Current_Ammo { get; set; }
        public float Reloading_Process { get; set; }
        public float Reload_Speed { get; set; }

        public override void InitData(device_all rc)
        {
            DeviceModule deviceModule = new DeviceModule();
            deviceModule.db_list.Add(new FireBehaviour()
            {
                module = deviceModule,
            });
            deviceModule.db_list.Add(new RecycleBehaviour()
            {
                module = deviceModule,
            });

            deviceModule.device = this;
            module_list.Add(deviceModule);

            base.InitData(rc);
            rotate_speed = desc.rotate_speed.Item1;
            attack_range_max = desc.basic_range.Item2;

            hook_logics.TryGetValue(desc.hook_logic.ToString(), out var record);

            Current_Interval = 0;
            Attack_Interval = record.cd;

            Recycle_Interval = record.cd;   //手动回收cd先也按射击cd来
            Current_Recycle_Interval = 0;

            move_speed_shooting = record.speed_shooting;
            move_speed_hooked = record.speed_hooked;
            move_speed_reeling_in = record.speed_reeling_in;

            dmg_init = record.damage_hit;
            dmg_by_period = record.damage_by_period;
            dmg_interval_max = record.damage_interval;

            rope_elasticity = record.elasticity;
            rope_breaking_overlength = record.breaking_overlength;
            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATION, Vector2.right);
            m_fsm = Device_FSM_Hook.Idle;
        }

        //====================================================================================================
        public override void tick()
        {
            muzzel_pos = key_points[KEY_POINT_NAME_FOR_MUZZEL].position;

            if (WorldContext.instance.is_need_reset)
            {
                hook_position -= new Vector2(WorldContext.instance.reset_dis, 0);
                muzzel_pos -= new Vector2(WorldContext.instance.reset_dis, 0);
            }
            if (!is_validate)
            {
                end_traction();
                FSM_change_to(Device_FSM_Hook.Broken);
            }

            switch (m_fsm)
            {
                case Device_FSM_Hook.Idle:
                    hook_position = muzzel_pos;
                    if (Current_Interval <= 0)
                        FSM_change_to(Device_FSM_Hook.Ready_to_Shoot);
                    else
                        Current_Interval--;
                    break;
                case Device_FSM_Hook.Ready_to_Shoot:
                    hook_position = muzzel_pos;
                    break;
                case Device_FSM_Hook.Shooting:
                    bones_direction[BONE_FOR_ROTATION] = hook_position - muzzel_pos;
                    if (hook_position.y <= Road_Info_Helper.try_get_altitude(hook_position.x))
                    {
                        target = null;
                        hook_v = Vector2.zero;
                        FSM_change_to(Device_FSM_Hook.Shooting_reeling_in);
                        break;
                    }

                    if ((hook_position - muzzel_pos).magnitude < MAX_ROPE_DISTANCE)
                    {
                        hook_v.y += Config.current.gravity * Config.PHYSICS_TICK_DELTA_TIME;
                        hook_position += hook_v * Config.PHYSICS_TICK_DELTA_TIME;
                    }
                    else
                    {
                        //判定为抓取失败
                        target = null;
                        hook_v = Vector2.zero;
                        FSM_change_to(Device_FSM_Hook.Shooting_reeling_in);
                    }
                    break;

                case Device_FSM_Hook.Hooked:
                    bones_direction[BONE_FOR_ROTATION] = hook_position - muzzel_pos;

                    if (target == null || target.hp <= 0 || !target.is_interactive)
                    {
                        end_traction();
                        FSM_change_to(Device_FSM_Hook.Breaking_reeling_in);
                    }
                    else
                    {
                        if (dmg_interval_current <= 0)
                        {
                            Attack_Data attack_data = new()
                            {
                                atk = dmg_by_period
                            };

                            target.hurt(attack_data);
                            dmg_interval_current = dmg_interval_max;
                        }
                        else
                        {
                            dmg_interval_current--;
                        }

                        var collider_pos_of_target = BattleUtility.get_target_colllider_pos(target);

                        if (WorldContext.instance.is_need_reset)
                        {
                            rope_current_length = (collider_pos_of_target - new Vector2(WorldContext.instance.reset_dis, 0) - muzzel_pos).magnitude;
                        }
                        else
                            rope_current_length = (collider_pos_of_target - muzzel_pos).magnitude;

                        if (rope_current_length > rope_limit_length)
                        {
                            var dl = rope_current_length - rope_limit_length;
                            var T = Mathf.Min(dl, rope_limit_length) * rope_elasticity;
                            var direction = (hook_position - muzzel_pos).normalized;
                            Road_Info_Helper.try_get_slope(WorldContext.instance.caravan_pos.x, out var slope);
                            var road_dir = new Vector2(1, slope).normalized;
                            var angle = Vector2.SignedAngle(direction, road_dir);

                            WorldContext.instance.tractive_force_max -= (int)self_traction.x;
                            WorldContext.instance.tractive_force_max += (int)(T * Mathf.Cos(angle));
                            self_traction = new Vector2((int)(T * Mathf.Cos(angle)), 0);

                            target.acc_attacher -= opposing_traction;
                            opposing_traction = -direction.normalized * T;
                            target.acc_attacher += opposing_traction;

                        }
                        if (WorldContext.instance.is_need_reset)
                        {
                            hook_position = collider_pos_of_target - new Vector2(WorldContext.instance.reset_dis, 0);
                        }
                        else
                        {
                            hook_position = collider_pos_of_target;
                        }
                    }
                    var mag = (hook_position - muzzel_pos).magnitude;
                    /*if (mag >= rope_limit_length + rope_breaking_overlength || rope_current_hp <= 0)
                    {
                        end_traction();
                        target = null;      //断了
                        FSM_change_to(Device_FSM_Hook.Breaking_reeling_in);
                    }*/
                    break;

                case Device_FSM_Hook.Shooting_reeling_in:
                case Device_FSM_Hook.Breaking_reeling_in:
                    var pos_relative = hook_position - muzzel_pos;
                    var new_length_limit = Mathf.Max(0f, pos_relative.magnitude - move_speed_reeling_in * Config.PHYSICS_TICK_DELTA_TIME);

                    hook_v.y += Config.current.gravity * Config.PHYSICS_TICK_DELTA_TIME;
                    hook_position += hook_v * Config.PHYSICS_TICK_DELTA_TIME;
                    var road_height = Road_Info_Helper.try_get_altitude(hook_position.x);
                    if (hook_position.y <= road_height)
                    {
                        hook_position.y = road_height;
                        hook_v.y = 0f;
                    }
                    var pos_relative_new = hook_position - muzzel_pos;
                    if (pos_relative_new.magnitude < 0.2f)
                    {
                        hook_position = muzzel_pos;
                        FSM_change_to(Device_FSM_Hook.Idle);
                    }
                    else
                    {
                        hook_position = muzzel_pos + pos_relative_new.normalized * Mathf.Min(pos_relative_new.magnitude, new_length_limit);
                        hook_position.x += pos_relative_new.x < 0 ? WorldContext.instance.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME : 0;
                        bones_direction[BONE_FOR_ROTATION] = pos_relative_new;
                    }
                    break;

                case Device_FSM_Hook.Broken:
                    if (is_validate)
                        FSM_change_to(Device_FSM_Hook.Idle);
                    break;

                default:
                    break;
            }

            base.tick();
        }

        private void FSM_change_to(Device_FSM_Hook target_fsm)
        {
            m_fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Hook.Idle:
                    ChangeAnim(ANIM_IDLE, true);
                    CloseCollider(COLLIDER_1);
                    break;
                case Device_FSM_Hook.Shooting_reeling_in:
                case Device_FSM_Hook.Shooting:
                    ChangeAnim(ANIM_ATTACK, true);
                    OpenCollider(COLLIDER_1, (ITarget t) =>
                    {
                        var e = t as Enemy;
                        if (e != null)
                        {
                            IEnemy_Can_Be_Separated separatable = e.bt as IEnemy_Can_Be_Separated;
                            if (separatable != null)
                                separatable.Forced_Separate(e);
                            else
                                hook();
                        }
                        else
                            hook();

                        void hook()
                        {
                            target = t;
                            var collider_pos_of_target = BattleUtility.get_target_colllider_pos(target);
                            rope_current_hp = rope_max_hp;
                            rope_current_length = (collider_pos_of_target - muzzel_pos).magnitude;
                            rope_limit_length = (collider_pos_of_target - muzzel_pos).magnitude;

                            int final_dmg = (int)(dmg_init * (1 + Damage_Increase));
                            Attack_Data attack_data = new()
                            {
                                atk = final_dmg
                            };

                            target.hurt(attack_data);
                            dmg_interval_current = dmg_interval_max;
                            FSM_change_to(Device_FSM_Hook.Hooked);
                        }
                    });
                    break;
                case Device_FSM_Hook.Hooked:
                    ChangeAnim(ANIM_HOOKING, true);
                    CloseCollider(COLLIDER_1);
                    break;
                case Device_FSM_Hook.Breaking_reeling_in:
                    hook_v = Vector2.zero;
                    ChangeAnim(ANIM_HOOKING, true);
                    CloseCollider(COLLIDER_1);
                    break;
                case Device_FSM_Hook.Broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    CloseCollider(COLLIDER_1);
                    break;
                default:
                    break;
            }
        }

        //----------------------------------------------------------------------------------------------------

        private void end_traction()
        {
            WorldContext.instance.tractive_force_max -= (int)self_traction.x;
            self_traction = Vector2.zero;

            if (target != null)
            {
                target.acc_attacher -= opposing_traction;
                opposing_traction = Vector2.zero;
            }
        }

        protected override bool target_can_be_selected(ITarget t)
        {
            //需要重写，因为索敌的起点不是position，而是muzzel_pos
            var tp = BattleUtility.get_target_colllider_pos(t);
            var t_distance = (tp - muzzel_pos).magnitude;
            return t.hp > 0 && t_distance >= desc.basic_range.Item1 && t_distance <= desc.basic_range.Item2;
        }

        protected override bool try_get_target()
        {
            target = BattleUtility.select_target_in_circle_min_angle(muzzel_pos, bones_direction[BONE_FOR_ROTATION], desc.basic_range.Item2, faction, (ITarget t) =>
            {
                return target_can_be_selected(t);
            });
            return target != null;
        }

        #region Check_If_Can_Shoot 2 Funcs
        private bool can_shoot(Vector2 target_dir)
        {
            return Current_Interval <= 0 && can_shoot_check_angle(target_dir);
        }

        private bool can_shoot_check_angle(Vector2 target_dir)
        {
            var delta_deg = Mathf.Abs(Vector2.SignedAngle(bones_direction[BONE_FOR_ROTATION], target_dir));
            return delta_deg <= SHOOT_ERROR_DEGREE;
        }
        #endregion

        private bool parabolic_trajectory_prediction(Vector2 muzzel_pos, Vector2 target_pos, float init_speed, out Vector2 target_dir)
        {
            var L = target_pos.x - muzzel_pos.x;
            var H = target_pos.y - muzzel_pos.y;
            var left = L < 0;
            if (left)
            {
                L = -L;
            }
            var discriminant = (H - Config.current.gravity * Mathf.Pow(L / init_speed, 2)) / Mathf.Sqrt(L * L + H * H);
            if (Mathf.Abs(discriminant) > 1)
            {
                target_dir = Vector2.zero;
                return false;
            }
            var M = Mathf.Atan2(-H, L);
            var angle = (Mathf.Asin(discriminant) - M) * 0.5f;
            target_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (left)
            {
                target_dir.x = -target_dir.x;
            }
            return true;
        }

        private void recycle_rope_by_relative_speed(float relative_coef)
        {
            if (Config.current.rope_min_length < rope_limit_length && Current_Recycle_Interval <= 0)
            {
                relative_coef = Mathf.Max(relative_coef, 0f);
                rope_limit_length = Mathf.Max(rope_limit_length - move_speed_hooked * Config.PHYSICS_TICK_DELTA_TIME * relative_coef, Config.current.rope_min_length);
            }
        }

        public void RotateRecycle(float angle)
        {
            rotate_angle += angle;
            if (fsm == Device_FSM_Hook.Hooked)
                recycle_rope_by_relative_speed(-angle * 0.02f);
        }

        #region PlayerControl

        public override void StartControl()
        {
            InputController.instance.right_hold_event += Aiming;
            InputController.instance.left_click_event += Fire;
            InputController.instance.left_hold_event += Load;

            base.StartControl();
        }

        public override void EndControl()
        {
            InputController.instance.right_hold_event -= Aiming;
            InputController.instance.left_click_event -= Fire;
            InputController.instance.left_hold_event -= Load;

            base.EndControl();
        }

        private void Aiming()
        {
            if (fsm == Device_FSM_Hook.Ready_to_Shoot)
            {
                var dir = InputController.instance.GetWorlMousePosition() - new Vector3(position.x, position.y, 10);
                rotate_bone_to_dir(BONE_FOR_ROTATION, dir);
            }
        }

        private void Fire()
        {
            if (InputController.instance.holding_right && Current_Interval <= 0 && fsm == Device_FSM_Hook.Ready_to_Shoot)
            {
                Vector2 expected_shoot_pos = InputController.instance.GetWorlMousePosition();
                FSM_change_to(Device_FSM_Hook.Shooting);
                Current_Interval = Attack_Interval;
                hook_v = move_speed_shooting * (expected_shoot_pos - muzzel_pos).normalized;
            }
        }

        private void Load()
        {
            Current_Interval--;
        }
        #endregion

        void IAttack.TryToAutoShoot()
        {
            if (!player_oper && fsm == Device_FSM_Hook.Ready_to_Shoot)
            {
                if (target == null || !target_can_be_selected(target))
                    try_get_target();

                if (target != null)
                    if (parabolic_trajectory_prediction(muzzel_pos, BattleUtility.get_target_colllider_pos(target), move_speed_shooting, out var expt_dir))
                    {
                        rotate_bone_to_dir(BONE_FOR_ROTATION, expt_dir);
                        if (can_shoot(expt_dir))
                        {
                            Current_Interval = Attack_Interval;
                            hook_v = move_speed_shooting * expt_dir.normalized;
                            FSM_change_to(Device_FSM_Hook.Shooting);
                        }
                    }
            }
        }

        void IRecycle.TryToAutoRecycle()
        {

            if (!player_oper && fsm == Device_FSM_Hook.Hooked)
            {
                rotate_angle -= 0.5f;
                recycle_rope_by_relative_speed(1f);
            }
        }

        void ILoad.TryToAutoLoad()
        {
            if (fsm == Device_FSM_Hook.Idle)
                Current_Interval--;          //有人的话 在自动上弦的情况下 再加快上弦
        }
    }
}
