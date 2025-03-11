using AutoCodes;
using Commons;
using UnityEngine;

namespace World.Devices.NewDevice
{
    public class NewBasicMelee : Device, IAttack
    {
        #region CONST
        private const float MOVE_SPEED_SLOW_COEF = 0.5F;

        private const string BONE_FOR_ROTATION = "roll_control";
        private const float ATK_ERROR_DEGREE = 5F;

        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK_1 = "attack_1";
        private const string ANIM_ATTACK_2 = "attack_2";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_1 = "collider_1";
        #endregion

        private int melee_atk_dmg;
        private float basic_knockback;

        private float hit_box_offset;
        private float distance_can_attack;

        private bool can_move = true;
        public float move_speed;
        private float move_speed_fast;
        private float move_speed_slow;

        private float rotation_speed_fast;
        private float rotation_speed_slow;

        public float attack_factor = 1f;

        public Vector2 sub_position = Vector2.zero;

        private enum Device_FSM_Melee
        {
            idle,
            attacking,
            broken,
        }
        private Device_FSM_Melee fsm;

        private bool can_blaze;  //按住鼠标可以连续攻击
        protected bool can_attack_check_post_cast = true;  //判定攻击后摇是否结束；


        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }

        public override void InitData(device_all rc)
        {
            DeviceModule deviceModule = new DeviceModule();
            deviceModule.db_list.Add(new FireBehaviour()
            {
                module = deviceModule,
            });

            deviceModule.device = this;
            module_list.Add(deviceModule);

            base.InitData(rc);
            device_type = DeviceType.melee;

            rotation_speed_fast = rc.rotate_speed.Item1;
            rotation_speed_slow = rc.rotate_speed.Item2;

            melee_logics.TryGetValue(rc.melee_logic.ToString(), out var logic);
            melee_atk_dmg = logic.damage;
            can_blaze = logic.can_blaze;
            basic_knockback = logic.knockback_ft;
            hit_box_offset = logic.hit_box_offset;
            distance_can_attack = hit_box_offset + desc.basic_range.Item2;
            move_speed_fast = logic.atk_part_speed;
            move_speed_slow = logic.atk_part_speed * MOVE_SPEED_SLOW_COEF;
            Current_Interval = logic.cd;
            Attack_Interval = logic.cd;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATION, Vector2.up);

            FSM_change_to(Device_FSM_Melee.idle);

            var anim_1_col_turn_on_time = logic.hit_period.Item1;
            var anim_1_col_turn_off_time = logic.hit_period.Item2;
            var anim_2_col_turn_on_time = logic.hit_period_2 == null ? anim_1_col_turn_on_time : logic.hit_period_2.Value.Item1;
            var anim_2_col_turn_off_time = logic.hit_period_2 == null ? anim_1_col_turn_off_time : logic.hit_period_2.Value.Item2;

            #region Anim_Event_Attack_1
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = anim_1_col_turn_on_time,
                anim_event = (Device d) =>
                {
                    d.OpenCollider(COLLIDER_1, (ITarget t) =>
                    {
                        int final_dmg = (int)(melee_atk_dmg * (1 + Damage_Increase));
                        float final_ft = basic_knockback * (1 + Knockback_Increase);

                        Attack_Data attack_data = new()
                        {
                            atk = final_dmg
                        };

                        t.hurt(attack_data);
                        t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);
                    });
                    can_move = false;
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = anim_1_col_turn_off_time,
                anim_event = (Device d) =>
                {
                    d.CloseCollider(COLLIDER_1);
                    can_move = true;
                    can_attack_check_post_cast = true;
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = 1,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Melee.idle);
                }
            });
            #endregion

            #region Anim_Event_Attack_2
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = anim_2_col_turn_on_time,
                anim_event = (Device d) =>
                {
                    d.OpenCollider(COLLIDER_1, (ITarget t) =>
                    {
                        int final_dmg = (int)(melee_atk_dmg * (1 + Damage_Increase));
                        float final_ft = basic_knockback * (1 + Knockback_Increase);

                        Attack_Data attack_data = new()
                        {
                            atk = final_dmg
                        };

                        t.hurt(attack_data);
                        t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);
                    });
                    can_move = false;
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = anim_2_col_turn_off_time,
                anim_event = (Device d) =>
                {
                    d.CloseCollider(COLLIDER_1);
                    can_move = true;
                    can_attack_check_post_cast = true;
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = 1,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Melee.idle);
                }
            });

            #endregion
        }

        // ========================================================================================

        public override void tick()
        {
            if (WorldContext.instance.is_need_reset)
            {
                sub_position -= new Vector2(WorldContext.instance.reset_dis, 0);
            }

            if (!is_validate && fsm != Device_FSM_Melee.broken)       //坏了
            {
                FSM_change_to(Device_FSM_Melee.broken);
                can_move = true;
            }

            switch (fsm)
            {
                case Device_FSM_Melee.idle:
                    if (!player_oper)
                    {
                        melee_device_move_to(position);
                    }
                    break;
                case Device_FSM_Melee.attacking:
                    break;
                case Device_FSM_Melee.broken:
                    target = null;
                    melee_device_move_to(position);

                    if (is_validate)
                        FSM_change_to(Device_FSM_Melee.idle);
                    break;

                default:
                    break;
            }

            //sub_position = position;
            base.tick();
        }

        private void FSM_change_to(Device_FSM_Melee target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Melee.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    move_speed = move_speed_fast;
                    break;
                case Device_FSM_Melee.attacking:
                    ChangeAnim(Random.Range(0, 2) == 0 ? ANIM_ATTACK_1 : ANIM_ATTACK_2, false);
                    move_speed = move_speed_slow;
                    can_attack_check_post_cast = false;
                    break;
                case Device_FSM_Melee.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    move_speed = move_speed_slow;
                    break;
                default:
                    break;
            }
        }

        // ----------------------------------------------------------------------------------------

        public override void InitPos()
        {
            sub_position = position;

            base.InitPos();
        }

        public void ChangeFactor(float f)
        {
            attack_factor = f;
            Current_Interval = (int)(Current_Interval / attack_factor);

            foreach (var view in views)
            {
                view.notify_change_anim_speed(attack_factor);
            }
        }

        protected override bool target_can_be_selected(ITarget t)
        {
            var t_distance = BattleUtility.get_v2_to_target_collider_pos(t, position).magnitude;
            return t.hp > 0 && t_distance <= desc.basic_range.Item2;
        }
        protected override bool try_get_target()
        {
            target = try_select_nearest_target();
            // 这里即使返回null也是可以接受的，无需排除此结果
            return target != null;

            ITarget try_select_nearest_target()
            {
                // 以槽位位置为中心寻找半径内的目标，返回其中最近的目标。最近 = 到自身当前位置的距离最近
                var ts = BattleUtility.select_all_target_in_circle(position, desc.basic_range.Item2, faction, (ITarget ts) =>
                {
                    return target_can_be_selected(ts);
                });

                if (ts != null && ts.Count != 0)
                {
                    ts.Sort((ITarget t1, ITarget t2) =>
                    {
                        if (BattleUtility.get_v2_to_target_collider_pos(t1, sub_position).magnitude > BattleUtility.get_v2_to_target_collider_pos(t2, sub_position).magnitude)
                            return 1;
                        else if (BattleUtility.get_v2_to_target_collider_pos(t1, sub_position).magnitude == BattleUtility.get_v2_to_target_collider_pos(t2, sub_position).magnitude)
                            return 0;
                        return -1;
                    });
                    return ts[0];
                }
                return null;
            }
        }


        private void melee_device_rotate_to(Vector2 expected_dir, float rotation_speed)
        {
            bones_direction[BONE_FOR_ROTATION] = BattleUtility.rotate_v2(bones_direction[BONE_FOR_ROTATION], expected_dir, rotation_speed);
        }

        private void melee_device_move_to(Vector2 expected_pos)
        {
            if (can_move)
            {
                var distance = expected_pos - sub_position;
                var move_distance_per_tick = move_speed * Config.PHYSICS_TICK_DELTA_TIME;
                if (distance.magnitude > move_distance_per_tick)
                    expected_pos = sub_position + distance.normalized * move_distance_per_tick;

                var t_distance = expected_pos - position;
                if(t_distance.magnitude > 3f)                   //此处3为近战设备的移动范围（随便填的)
                {
                    expected_pos = (expected_pos - position).normalized * 3f + position;
                }

                sub_position = expected_pos;
            }
        }

        protected bool check_error_angle()
        {
            if (target == null)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            var current_v2 = bones_direction[BONE_FOR_ROTATION];
            var delta_deg = Mathf.Abs(Vector2.SignedAngle(current_v2, target_v2));
            return delta_deg <= ATK_ERROR_DEGREE;
        }

        #region PlayerControl
        public override void StartControl()
        {
            Current_Interval = 0;
            //InputController.instance.right_hold_event += Aiming;
            if (can_blaze)
            {
                InputController.instance.left_click_event += Attack;
                InputController.instance.left_hold_event += Attack;
            }
            else
                InputController.instance.left_click_event += Attack;

            base.StartControl();
        }

        public override void EndControl()
        {
            //InputController.instance.right_hold_event -= Aiming;
            if (can_blaze)
            {
                InputController.instance.left_click_event -= Attack;
                InputController.instance.left_hold_event -= Attack;
            }
            else
                InputController.instance.left_click_event -= Attack;

            base.EndControl();
        }

        public override void OperateDrag(Vector2 dir)
        {
            Debug.Log($"dir:{dir}");
            can_move = true;
            melee_device_rotate_to(dir, Current_Interval <= 0 ? rotation_speed_fast : rotation_speed_slow);
            melee_device_move_to(position + dir * 10);//这里可能需要乘一个常量K
        }

        private void Aiming()
        {
            can_move = true;
            var dir = InputController.instance.GetWorlMousePosition() - new Vector3(position.x, position.y, 10);
            melee_device_rotate_to(dir, Current_Interval <= 0 ? rotation_speed_fast : rotation_speed_slow);
            var pos = InputController.instance.GetWorlMousePosition();
            melee_device_move_to(pos);
        }

        private void Attack()           //管他这的那的 砍
        {
            if (InputController.instance.holding_right && can_attack_check_post_cast)
                if (fsm != Device_FSM_Melee.broken)
                    FSM_change_to(Device_FSM_Melee.attacking);
        }

        #endregion

        void IAttack.TryToAutoShoot()
        {
            if (!player_oper && fsm != Device_FSM_Melee.broken)
            {
                Current_Interval--;

                if (target == null || !target_can_be_selected(target))
                    try_get_target();

                Vector2 expected_pos_attacking;
                if (target == null)
                {
                    expected_pos_attacking = sub_position;
                    melee_device_rotate_to(Vector2.up, Current_Interval <= 0 ? rotation_speed_fast : rotation_speed_slow);
                }
                else
                {
                    var delta_pos = BattleUtility.get_v2_to_target_collider_pos(target, position);
                    expected_pos_attacking = position
                                + delta_pos.normalized
                                * Mathf.Min(desc.basic_range.Item2, delta_pos.magnitude - hit_box_offset);
                    melee_device_rotate_to(delta_pos.normalized, Current_Interval <= 0 ? rotation_speed_fast : rotation_speed_slow);
                }
                melee_device_move_to(expected_pos_attacking);

                if (Current_Interval <= 0 && (BattleUtility.get_v2_to_target_collider_pos(target, sub_position)).magnitude <= distance_can_attack && check_error_angle())
                {
                    Current_Interval = Attack_Interval;
                    FSM_change_to(Device_FSM_Melee.attacking);
                }
            }
        }
    }
}
