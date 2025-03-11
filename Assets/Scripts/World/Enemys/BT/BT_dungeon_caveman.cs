using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_dungeon_caveman : Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Jump, IEnemy_Can_Shoot
    {
        #region CONST
        const int JUMP_CD = 7;

        const int TARGET_RESELECTED_TICK = 360;

        const float MELEE_DISTANCE = 2F;
        const float SHOOT_DISTANCE_MIN = 4.5F;
        const float SHOOT_DISTANCE_MAX = 11F;
        const string MUZZLE_BONE_NAME_IN_SPINE = "proj_muzzle";

        const float JUMP_SPEED_X_MIN = 2F;
        const float JUMP_SPEED_X_MAX = 4F;
        const float JUMP_SPEED_Y_MIN = 2F;
        const float JUMP_X_DISTANCE_MOD_COEF = 0.1F;

        const ushort TICKS_PER_SEC_IN_SPINE = 30;
        const ushort MELEE_ATK_TICK_IN_SPINE = 12 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort MELEE_END_TICK_IN_SPINE = 29 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_ATK_TICK_IN_SPINE = 27 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_END_TICK_IN_SPINE = 60 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        #endregion

        private enum EN_dungeon_caveman_FSM
        {
            Default,    // Init State
            Idle,
            Walk_1,
            Walk_2,
            Atk_Melee,
            Atk_Shoot,
            Falling,
        }
        private EN_dungeon_caveman_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        private bool walk_left_right_change;

        private bool atk_finished;
        private ushort melee_combo;

        private ushort state_event_tick;
        private ushort state_over_tick;

        Enemy self;

        #region Interface Jump
        public bool Jump_Finished { get; set; }
        public int Jump_CD_Ticks { get; set; } = JUMP_CD;
        public IEnemy_Can_Jump.Enemy_Jump_Mode Jump_Mode { get; set; }
        public IEnemy_Can_Jump I_Jump { get; set; }
        public float Jump_Speed_Min { get; set; } = JUMP_SPEED_X_MIN;
        public float Jump_Speed_Max { get; set; } = JUMP_SPEED_X_MAX;
        public float Jump_Y_Speed_Min { get; set; } = JUMP_SPEED_Y_MIN;
        public float X_Distance_Mod_Coef { get; set; } = JUMP_X_DISTANCE_MOD_COEF;
        #endregion

        #region Interface Shoot
        public int Shoot_CD { get; set; }
        public int Shoot_CD_Max { get; set; }
        public bool Shoot_Finished { get; set; }
        public float Projectile_Speed { get; set; }
        public IEnemy_Can_Shoot I_Shoot { get; set; }
        #endregion

        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            FSM_change_to((EN_dungeon_caveman_FSM)System.Enum.Parse(typeof(EN_dungeon_caveman_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Hover;
            I_Jump = cell.bt as IEnemy_Can_Jump;
            self = cell;

            I_Shoot = cell.bt as IEnemy_Can_Shoot;
            I_Shoot.Init_Shooting_Data(cell);
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            if (cell.acc_attacher != Vector2.zero)
                FSM_change_to(EN_dungeon_caveman_FSM.Default);

            Shoot_CD = Shoot_CD <= 0 ? 0 : --Shoot_CD;

            switch (m_state)
            {
                case EN_dungeon_caveman_FSM.Default:
                    if (mover.move_type == EN_enemy_move_type.Slide && cell.acc_attacher == Vector2.zero)
                        FSM_change_to(EN_dungeon_caveman_FSM.Idle);
                    break;

                case EN_dungeon_caveman_FSM.Idle:
                    if (target_locked_on == null || ticks_target_has_been_locked_on >= TARGET_RESELECTED_TICK)
                        lock_target();
                    else
                        ticks_target_has_been_locked_on++;

                    if (target_locked_on != null)
                    {
                        var delta_pos = target_locked_on.Position - cell.pos;
                        var distance = delta_pos.magnitude;

                        if (Shoot_CD <= 0 && distance <= SHOOT_DISTANCE_MAX && distance >= SHOOT_DISTANCE_MIN)
                        {
                            FSM_change_to(EN_dungeon_caveman_FSM.Atk_Shoot);
                            cell.dir.x = delta_pos.x;
                            break;
                        }

                        if (check_melee_atk_range(distance) && Random.Range(1, 10) > melee_combo)
                        {
                            FSM_change_to(EN_dungeon_caveman_FSM.Atk_Melee);
                            break;
                        }

                        if (Jump_CD_Ticks <= 0)
                        {
                            Jump_Mode = check_melee_atk_range(distance) ? IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Around : IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_To;
                            FSM_change_to(walk_left_right_change ? EN_dungeon_caveman_FSM.Walk_1 : EN_dungeon_caveman_FSM.Walk_2);
                        }
                        else
                            Jump_CD_Ticks--;  //cool down only if has target                  
                    }

                    break;

                case EN_dungeon_caveman_FSM.Walk_1:
                case EN_dungeon_caveman_FSM.Walk_2:
                    if (mover.move_type == EN_enemy_move_type.Slide)
                        m_state = EN_dungeon_caveman_FSM.Idle;
                    break;

                case EN_dungeon_caveman_FSM.Atk_Melee:
                    ticks_in_current_state++;

                    if (check_state_time(state_over_tick))
                    {
                        m_state = EN_dungeon_caveman_FSM.Idle;
                        target_locked_on = null;
                        break;
                    }

                    if (target_locked_on != null)
                    {
                        var target_delta_pos = target_locked_on.Position - cell.pos;
                        cell.dir.x = target_delta_pos.x;

                        if (check_melee_atk_range(target_delta_pos.magnitude))
                        {
                            if (!atk_finished && check_state_time(state_event_tick))
                            {
                                Attack_Data attack_data = new()
                                {
                                    atk = cell._desc.basic_atk
                                };

                                target_locked_on.hurt(attack_data);
                                atk_finished = true;
                                melee_combo++;
                            }
                            break;
                        }
                        target_locked_on = null;
                    }

                    FSM_change_to(EN_dungeon_caveman_FSM.Idle);
                    break;

                case EN_dungeon_caveman_FSM.Atk_Shoot:
                    ticks_in_current_state++;

                    if (check_state_time(state_over_tick))
                    {
                        FSM_change_to(EN_dungeon_caveman_FSM.Idle);
                        break;
                    }

                    if (target_locked_on != null)
                    {
                        cell.dir.x = target_locked_on.Position.x - cell.pos.x;
                        if (!Shoot_Finished && check_state_time(state_event_tick))
                            I_Shoot.Monster_Shoot(cell, MUZZLE_BONE_NAME_IN_SPINE, target_locked_on.Position);
                    }
                    else
                        FSM_change_to(EN_dungeon_caveman_FSM.Idle);
                    break;

                default:
                    break;
            }

            mover.move();

        }

        private void FSM_change_to(EN_dungeon_caveman_FSM expected_FSM)
        {
            m_state = expected_FSM;
            ticks_in_current_state = 0;
            switch (expected_FSM)
            {
                case EN_dungeon_caveman_FSM.Atk_Melee:
                    state_event_tick = MELEE_ATK_TICK_IN_SPINE;
                    state_over_tick = MELEE_END_TICK_IN_SPINE;
                    atk_finished = false;
                    break;
                case EN_dungeon_caveman_FSM.Atk_Shoot:
                    state_event_tick = SHOOT_ATK_TICK_IN_SPINE;
                    state_over_tick = SHOOT_END_TICK_IN_SPINE;
                    Shoot_Finished = false;
                    break;
                case EN_dungeon_caveman_FSM.Walk_1:
                case EN_dungeon_caveman_FSM.Walk_2:
                    I_Jump.Jump_By_Mode(self, target_locked_on.Position, JUMP_CD);
                    walk_left_right_change = !walk_left_right_change;
                    melee_combo = 0;
                    self.dir.x = self.velocity.x;
                    break;
                default:
                    break;
            }
        }

        private bool check_melee_atk_range(float distance_to_target)
        {
            return distance_to_target <= MELEE_DISTANCE;
        }

        private bool check_state_time(ushort ticks)
        {
            return ticks_in_current_state >= ticks;
        }



        void IEnemy_BT.notify_on_enter_die(Enemy cell)
        {
            basic_die(cell);
        }


        void IEnemy_BT.notify_on_dying(Enemy cell)
        {

        }


        void IEnemy_BT.notify_on_dead(Enemy cell)
        {

        }
    }
}

