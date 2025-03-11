using AutoCodes;
using Commons;
using UnityEngine;
using static World.WorldEnum;

namespace World.Devices
{
    public class BasicMelee : Device
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
        private int melee_atk;
        private float basic_knockback;

        private int cd_default;
        private int cd_actual;

        private float hit_box_offset;
        private float distance_can_attack;

        private bool can_move = true;
        public float move_speed;
        private float move_speed_fast;
        private float move_speed_slow;

        private float rotation_speed_fast;
        private float rotation_speed_slow;

        public float attack_factor = 1f;
        public Vector2 move_position = Vector2.zero;
        private Vector2 position_last_tick = Vector2.zero;

        private enum Device_FSM_Melee
        {
            idle,
            attacking,
            moving,
            broken,
        }

        private Device_FSM_Melee fsm;

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            device_type = DeviceType.melee;

            rotation_speed_fast = rc.rotate_speed.Item1;
            rotation_speed_slow = rc.rotate_speed.Item2;

            melee_logics.TryGetValue(rc.melee_logic.ToString(), out var logic);
            cd_default = logic.cd;
            cd_actual = logic.cd;
            melee_atk = logic.damage;
            basic_knockback = logic.knockback_ft;
            hit_box_offset = logic.hit_box_offset;
            distance_can_attack = hit_box_offset + desc.basic_range.Item2;

            move_speed_fast = logic.atk_part_speed;
            move_speed_slow = logic.atk_part_speed * MOVE_SPEED_SLOW_COEF;

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
                        Attack_Data attack_data = new()
                        {
                            atk = melee_atk
                        };

                        t.hurt(attack_data);
                        t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), basic_knockback);
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
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = 1,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Melee.moving);
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_IDLE, false);
                    }
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
                        Attack_Data attack_data = new()
                        {
                            atk = melee_atk
                        };

                        t.hurt(attack_data);
                        t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), basic_knockback);
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
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = 1,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_FSM_Melee.moving);
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_IDLE, false);
                    }
                }
            });

            #endregion
        }

        public override void tick()
        {
            if (WorldContext.instance.is_need_reset)
            {
                move_position -= new Vector2(WorldContext.instance.reset_dis, 0);
                position_last_tick -= new Vector2(WorldContext.instance.reset_dis, 0);
            }

            if (!is_validate)       //坏了
            {
                FSM_change_to(Device_FSM_Melee.broken);
                can_move = true;
            }

            switch (fsm)
            {
                case Device_FSM_Melee.idle:
                    cd_actual--;

                    target = try_select_nearest_target();
                    if (target != null)
                    {
                        FSM_change_to(Device_FSM_Melee.moving);
                    }

                    //返回默认位置ing
                    move_to(position);

                    rotate_to(Vector2.up, cd_actual <= 0 ? rotation_speed_fast : rotation_speed_slow);
                    break;

                case Device_FSM_Melee.attacking:
                    Vector2 expected_pos_attacking;
                    if (target == null || !target_can_be_selected(target))
                    {
                        expected_pos_attacking = position_last_tick;
                    }
                    else
                    {
                        var delta_pos_1 = BattleUtility.get_v2_to_target_collider_pos(target, position);
                        expected_pos_attacking = position
                            + delta_pos_1.normalized
                            * Mathf.Min(desc.basic_range.Item2, delta_pos_1.magnitude - hit_box_offset);
                    }
                    move_to(expected_pos_attacking);
                    break;

                case Device_FSM_Melee.moving:
                    cd_actual--;

                    if (target == null || !target_can_be_selected(target))     //无目标或者目标不符合条件就重新索敌
                    {
                        target = try_select_nearest_target();
                        if (target == null)
                        {
                            FSM_change_to(Device_FSM_Melee.idle);
                            move_to(position);
                            rotate_to(Vector2.up, cd_actual <= 0 ? rotation_speed_fast : rotation_speed_slow);
                            break;
                        }
                    }

                    var delta_pos = BattleUtility.get_v2_to_target_collider_pos(target, position);
                    var normal = delta_pos.normalized;
                    var expected_pos_moving = position_last_tick;

                    if ((cd_actual <= 0) && (BattleUtility.get_v2_to_target_collider_pos(target, position_last_tick)).magnitude <= distance_can_attack && check_error_angle()) //走到了再砍,但是保持距离免得贴上去了
                    {
                        cd_actual = (int)(cd_default / attack_factor);
                        FSM_change_to(Device_FSM_Melee.attacking);
                        expected_pos_moving = position + normal * Mathf.Min(desc.basic_range.Item2, delta_pos.magnitude - hit_box_offset);
                    }
                    rotate_to(normal, cd_actual <= 0 ? rotation_speed_fast : rotation_speed_slow);
                    move_to(expected_pos_moving);

                    break;

                case Device_FSM_Melee.broken:
                    target = null;
                    move_to(position);

                    if (is_validate)
                        FSM_change_to(Device_FSM_Melee.idle);
                    break;

                default:
                    break;
            }

            position_last_tick = position + new Vector2(WorldContext.instance.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME, 0);
            base.tick();
        }

        public override void InitPos()
        {
            move_position = position;
            position_last_tick = position;

            base.InitPos();
        }

        public void ChangeFactor(float f)
        {
            attack_factor = f;
            cd_actual = (int)(cd_actual / attack_factor);

            foreach (var view in views)
            {
                view.notify_change_anim_speed(attack_factor);
            }
        }

        private ITarget try_select_nearest_target()
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
                    if (BattleUtility.get_v2_to_target_collider_pos(t1, position_last_tick).magnitude > BattleUtility.get_v2_to_target_collider_pos(t2, position_last_tick).magnitude)
                        return 1;
                    else if (BattleUtility.get_v2_to_target_collider_pos(t1, position_last_tick).magnitude == BattleUtility.get_v2_to_target_collider_pos(t2, position_last_tick).magnitude)
                        return 0;
                    return -1;
                });
                return ts[0];
            }

            return null;
        }

        protected override bool target_can_be_selected(ITarget t)
        {
            var t_distance = BattleUtility.get_v2_to_target_collider_pos(t, position).magnitude;
            return t.hp > 0 && t_distance <= desc.basic_range.Item2;
        }

        private void rotate_to(Vector2 expected_dir, float rotation_speed)
        {
            bones_direction[BONE_FOR_ROTATION] = BattleUtility.rotate_v2(bones_direction[BONE_FOR_ROTATION], expected_dir, rotation_speed);
        }

        private void move_to(Vector2 expected_pos)
        {
            if (can_move)
            {
                var distance = expected_pos - position_last_tick;
                var move_distance_per_tick = move_speed * Config.PHYSICS_TICK_DELTA_TIME;
                if (distance.magnitude > move_distance_per_tick)
                {
                    expected_pos = position_last_tick
                        + distance.normalized * move_distance_per_tick;
                }
                position = expected_pos;
            }
            else
            {
                position = position_last_tick;
            }

        }

        protected bool check_error_angle()
        {
            Vector2 target_v2;
            if (target != null)
            {
                target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            }
            else
            {
                return false;
            }
            var current_v2 = bones_direction[BONE_FOR_ROTATION];
            var delta_deg = Mathf.Abs(Vector2.SignedAngle(current_v2, target_v2));
            return delta_deg <= ATK_ERROR_DEGREE;
        }


        private void FSM_change_to(Device_FSM_Melee target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Melee.idle:
                    move_speed = move_speed_fast;
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_IDLE, true);
                    }
                    break;
                case Device_FSM_Melee.attacking:
                    move_speed = move_speed_slow;
                    var i = Random.Range(0, 2);
                    if (i == 0)
                        foreach (var view in views)
                        {
                            view.notify_change_anim(ANIM_ATTACK_1, false);
                        }
                    else
                        foreach (var view in views)
                        {
                            view.notify_change_anim(ANIM_ATTACK_2, false);
                        }
                    break;
                case Device_FSM_Melee.moving:
                    move_speed = move_speed_fast;
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_IDLE, true);
                    }
                    break;
                case Device_FSM_Melee.broken:
                    move_speed = move_speed_slow;
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_BROKEN, true);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
