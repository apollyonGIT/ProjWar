using AutoCodes;
using Commons;
using UnityEngine;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class BasicShield : Device, IAttack, IShield
    {

        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_BLOCK = "idle";
        private const string ANIM_MELEE = "attack";
        private const string ANIM_BROKEN = "destroying";
        protected const string BONE_FOR_ROTATION = "control";   // Temp：不规范命名
        private const string COLLIDER_FOR_ATK = "collider_1";
        private const string KEY_POINT_FOR_ATK = "collider_1";

        private const float SHOOT_ERROR_DEGREE = 5F;
        #endregion

        #region IAttack
        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }
        #endregion

        #region IShield
        public float Toughness_Current { get; set; }
        public float Toughness_Max { get; set; }
        public float Toughness_Recover { get; set; }
        public float Def_Range { get; set; }
        public bool Is_Defending => player_oper;
        public Vector2 Shield_Dir => bones_direction[BONE_FOR_ROTATION];
        public bool Hitting_Time { get; set; }
        #endregion

        private enum Device_FSM_Shield
        {
            idle,
            block,
            melee_attack,
            broken,
        }
        private Device_FSM_Shield fsm;


        #region shield_melee_attack
        private int atk_dmg;
        private float atk_ft;
        private bool can_blaze; //能否连续射击
        protected bool can_attack_check_cd => Current_Interval <= 0;
        private Vector2 atk_offset_pos; // Temp:攻击时产生的位移应当做到动画里
        #endregion

        #region shield_rotate
        private bool can_rotate_by_player_control => player_oper;
        private bool can_rotate_by_auto_control;
        private bool can_rotate => can_rotate_by_player_control || can_rotate_by_auto_control;
        private float get_rotate_speed => Is_Defending ? desc.rotate_speed.Item2 : desc.rotate_speed.Item1;
        #endregion

        #region shield_move
        private bool can_move_forward_by_player_control => Is_Defending;
        private bool can_move_forward_by_auto_control;
        private bool can_move_forward => can_move_forward_by_player_control || can_move_forward_by_auto_control;
        private float move_speed_base;
        private float move_speed;

        public Vector2 position_last_tick = Vector2.zero;
        private Vector2 expt_position;
        #endregion

        //============================================================================================================

        public override void InitData(device_all rc)
        {
            DeviceModule deviceModule = new DeviceModule();
            var fb = new FireBehaviour() { module = deviceModule };
            deviceModule.db_list.Add(fb);
            deviceModule.device = this;
            module_list.Add(deviceModule);

            base.InitData(rc);

            shield_logics.TryGetValue(desc.shield_logic.ToString(), out var record);

            Def_Range = record.def_range;

            atk_dmg = record.damage;
            atk_ft = record.knockback_ft;
            can_blaze = record.can_blaze;
            Attack_Interval = record.cd;
            Current_Interval = record.cd;

            move_speed_base = record.atk_part_speed;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATION, Vector2.right);

            FSM_change_to(Device_FSM_Shield.idle);

            #region AnimEvent
            var counterattack_tick_on = new AnimEvent()
            {
                anim_name = ANIM_MELEE,
                percent = record.hit_period.Item1,
                anim_event = (Device d) =>
                {
                    Hitting_Time = true;
                    d.OpenCollider(COLLIDER_FOR_ATK, (ITarget t) =>
                    {
                        int final_atk_dmg = (int)(atk_dmg * (1 + Damage_Increase));
                        float final_atk_ft = (atk_ft * (1 + Knockback_Increase));

                        Attack_Data attack_data = new()
                        {
                            atk = final_atk_dmg
                        };

                        t.hurt(attack_data);
                        t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, Shield_Dir, final_atk_ft);
                    });
                }
            };

            var counterattack_tick_off = new AnimEvent()
            {
                anim_name = ANIM_MELEE,
                percent = record.hit_period.Item2,
                anim_event = (Device d) =>
                {
                    Hitting_Time = false;
                    d.CloseCollider(COLLIDER_FOR_ATK);
                }
            };

            var counterattack_back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_MELEE,
                percent = 1f,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Shield.idle);
                }
            };

            var block_back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_BLOCK,
                percent = 1f,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Shield.idle);
                }
            };
            #endregion

            anim_events.Add(counterattack_tick_on);
            anim_events.Add(counterattack_tick_off);
            anim_events.Add(counterattack_back_to_idle);
            anim_events.Add(block_back_to_idle);
        }

        public override void InitPos()
        {
            position_last_tick = position;
            expt_position = position;

            base.InitPos();
        }

        // ------------------------------------------------------------------------------------------------------------

        public override void tick()
        {
            if (WorldContext.instance.is_need_reset)
            {
                position_last_tick -= new Vector2(WorldContext.instance.reset_dis, 0);
            }

            if (!is_validate && fsm != Device_FSM_Shield.broken)
                FSM_change_to(Device_FSM_Shield.broken);

            switch (fsm)
            {
                case Device_FSM_Shield.idle:
                    if (Current_Interval > 0)
                        Current_Interval--;
                    break;

                case Device_FSM_Shield.block:
                case Device_FSM_Shield.melee_attack:
                    expt_position = atk_offset_pos;
                    break;

                case Device_FSM_Shield.broken:
                    expt_position = position;
                    if (is_validate)
                        FSM_change_to(Device_FSM_Shield.idle);
                    break;
                default:
                    break;
            }

            expt_position += new Vector2(WorldContext.instance.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME, 0);
            shield_device_move_to_expected_pos();
            
            base.tick();
        }

        private void FSM_change_to(Device_FSM_Shield target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case Device_FSM_Shield.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    rotate_speed = get_rotate_speed;
                    move_speed = move_speed_base;
                    Hitting_Time = false;
                    CloseCollider(COLLIDER_FOR_ATK);
                    break;
                case Device_FSM_Shield.block:
                    ChangeAnim(ANIM_BLOCK, false);
                    rotate_speed = get_rotate_speed;
                    move_speed = move_speed_base;
                    break;
                case Device_FSM_Shield.melee_attack:
                    Current_Interval = Attack_Interval;
                    ChangeAnim(ANIM_MELEE, false);
                    rotate_speed = 0;
                    move_speed = move_speed_base * 3f;
                    atk_offset_pos = position_last_tick + Shield_Dir.normalized * 0.5f;
                    break;
                case Device_FSM_Shield.broken:
                    ChangeAnim(ANIM_BROKEN, false);
                    rotate_speed = 0;
                    move_speed = move_speed_base;
                    Hitting_Time = false;
                    CloseCollider(COLLIDER_FOR_ATK);
                    break;
                default:
                    break;
            }
        }

        //------------------------------------------------------------------------------------------------------------

        void IAttack.TryToAutoShoot()
        {
            if (player_oper) //有玩家操控时 以玩家操控优先
                return;

            switch (fsm)
            {
                case Device_FSM_Shield.idle:
                case Device_FSM_Shield.block:
                    Current_Interval--;

                    if (target_list.Count ==0)
                        try_get_target();

                    rotate_bone_to_target(BONE_FOR_ROTATION);

                    if (can_auto_attack())
                        FSM_change_to(Device_FSM_Shield.melee_attack);
                    break;

                default:
                    break;
            }
        }

        //------------------------------------------------------------------------------------------------------------

        protected virtual bool can_auto_attack()
        {
            return can_attack_check_cd && can_shoot_check_error_angle();
        }
        protected virtual bool can_manual_attack()
        {
            return can_attack_check_cd;
        }

        protected bool can_shoot_check_error_angle()
        {
            if (target_list.Count == 0)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            var current_v2 = Shield_Dir;
            var delta_deg = Mathf.Abs(Vector2.SignedAngle(current_v2, target_v2));
            return delta_deg <= SHOOT_ERROR_DEGREE;
        }

        protected override bool try_get_target()
        {
            var target = BattleUtility.select_target_in_circle_min_angle(position, bones_direction[BONE_FOR_ROTATION], desc.basic_range.Item2, faction, (ITarget t) =>
            {
                return target_can_be_selected(t);
            });

            if(target!=null)
                target_list.Add(target);

            //这里即使返回null也是可以接受的，无需排除此结果
            return target != null;
        }

        // ------------------------------------------------------------------------------------------------------------
        private void shield_device_rotate_to(Vector2 expected_dir)
        {
            rotate_bone_to_dir(BONE_FOR_ROTATION, expected_dir);
        }

        private void shield_device_move_to_expected_pos()
        {
            // 需要保证在每个状态下都被调用，否则move_position会被重置到position
            var distance = expt_position - position_last_tick;
            var move_distance_per_tick = move_speed * Config.PHYSICS_TICK_DELTA_TIME;
            if (distance.magnitude > move_distance_per_tick)
                expt_position = position_last_tick + distance.normalized * move_distance_per_tick;
            position_last_tick = expt_position;
        }


        public override int Hurt(int dmg)
        {
            if (Toughness_Current > dmg)
                Toughness_Current -= dmg;
            else
                Toughness_Current = 0;

            return base.Hurt(dmg);
        }


        void IShield.Rebound_Projectile(Projectile proj, Vector2 proj_vel)
        {
            var v_new = (proj_vel - velocity).magnitude * Shield_Dir * 2f;
            proj.ResetProjectile(v_new, Shield_Dir, faction, MovementStatus.normal);
        }

        #region PlayerControl
        public override void StartControl()
        {
            InputController.instance.left_hold_event += TryToAttack;
            InputController.instance.left_release_event += ResetFire;

            base.StartControl();
        }

        public override void EndControl()
        {
            InputController.instance.RemoveLeftHold(TryToAttack);
            InputController.instance.left_release_event -= ResetFire;

            base.EndControl();
        }

        public override void OperateDrag(Vector2 dir)
        {
            Vector2 pos_to_pointer = dir;
            var ptp_n = pos_to_pointer.normalized;
            var ptp_m = pos_to_pointer.magnitude;

            if (can_rotate)
                shield_device_rotate_to(ptp_n);

            if (can_move_forward)
            {
                expt_position = position + ptp_n * Mathf.Clamp(ptp_m - 1f, 0, Def_Range);
            }
            else
                expt_position = position;
        }

        private bool fired = false;

        private void ResetFire()
        {
            fired = false;
        }
        private void TryToAttack()
        {
            if (!fired)
            {
                if (Melee_Attack())
                    fired = true;
            }
            else if (can_blaze)
            {
                Melee_Attack();
            }
        }
        private bool Melee_Attack()
        {
            if (can_manual_attack())
                if (fsm == Device_FSM_Shield.idle || fsm == Device_FSM_Shield.block)
                {
                    FSM_change_to(Device_FSM_Shield.melee_attack);
                    return true;
                }
            return false;
        }

        #endregion
    }
}
