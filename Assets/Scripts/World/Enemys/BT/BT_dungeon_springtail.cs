using Commons;
using System;
using UnityEngine;
using World.Helpers;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static UnityEngine.GraphicsBuffer;

namespace World.Enemys.BT
{



    public class BT_dungeon_springtail : Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Jump
    {
        #region CONST
        const int JUMP_CD = 15;
        const int MELEE_ATK_CD = 240;

        const int TARGET_RESELECTED_TICK = 360;

        const float JUMP_SPEED_X_MIN = 1F;
        const float JUMP_SPEED_X_MAX = 30F;

        const float JUMP_SPEED_Y_MIN = 3.5F;
        const float JUMP_X_DISTANCE_MOD_COEF = 0.8F;

        const float BITE_ATTACK_DISTANCE = 1F;

        const short TICKS_PER_SEC_IN_SPINE = 30;
        const short JUMP_TICK_BY_SPINE = 12 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const short ATK_DMG_TICK_BY_SPINE = 4 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const short ATK_END_TICK_BY_SPINE = 8 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        #endregion

        private enum EN_dungeon_springtail_FSM
        {
            Default,    // Init State
            Idle,
            Jumping,
            Atk_Melee,
            Falling,    // Die and Fall
        }
        private EN_dungeon_springtail_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        private int melee_atk_cd = UnityEngine.Random.Range(0, MELEE_ATK_CD);
        private bool melee_atk_finished;

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

        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            FSM_change_to((EN_dungeon_springtail_FSM)Enum.Parse(typeof(EN_dungeon_springtail_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Hover;

            I_Jump = cell.bt as IEnemy_Can_Jump;
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            if (cell.acc_attacher != Vector2.zero)
                FSM_change_to(EN_dungeon_springtail_FSM.Default);

            switch (m_state)
            {
                case EN_dungeon_springtail_FSM.Default:
                    if (mover.move_type == EN_enemy_move_type.Slide && cell.acc_attacher == Vector2.zero)
                        FSM_change_to(EN_dungeon_springtail_FSM.Idle);
                    break;

                case EN_dungeon_springtail_FSM.Idle:
                    if (target_locked_on == null || ticks_target_has_been_locked_on >= TARGET_RESELECTED_TICK)
                        lock_target();

                    if (target_locked_on != null)
                    {
                        if (Jump_CD_Ticks <= 0)
                        {
                            var delta_x = target_locked_on.Position.x - cell.pos.x;
                            cell.dir.x = delta_x;
                            Jump_Mode = Mathf.Abs(delta_x) < BITE_ATTACK_DISTANCE ? IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Around : IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_To;
                            FSM_change_to(EN_dungeon_springtail_FSM.Jumping);
                        }
                        else
                            Jump_CD_Ticks--;   //cool down only if has target
                    }

                    break;

                case EN_dungeon_springtail_FSM.Jumping:
                    ticks_in_current_state++;

                    if (Jump_Finished && mover.move_type == EN_enemy_move_type.Slide)
                    {
                        FSM_change_to(EN_dungeon_springtail_FSM.Idle);
                        target_locked_on = null;
                        break;
                    }
                    else if (target_locked_on != null)
                    {
                        var dis = target_locked_on.Position - cell.pos;
                        if (melee_atk_cd <= 0 && dis.magnitude < BITE_ATTACK_DISTANCE)
                        {
                            FSM_change_to(EN_dungeon_springtail_FSM.Atk_Melee);
                            cell.dir.x = dis.x;
                        }
                        else if (!Jump_Finished && ticks_in_current_state >= JUMP_TICK_BY_SPINE)
                            I_Jump.Jump_By_Mode(cell, target_locked_on.Position, JUMP_CD);
                    }

                    break;

                case EN_dungeon_springtail_FSM.Atk_Melee:
                    ticks_in_current_state++;
                    if (!melee_atk_finished && ticks_in_current_state > ATK_DMG_TICK_BY_SPINE)
                    {
                        melee_atk_finished = true;

                        Attack_Data attack_data = new()
                        {
                            atk = cell._desc.basic_atk
                        };

                        target_locked_on.hurt(attack_data);
                        melee_atk_cd = MELEE_ATK_CD;
                    }
                    else if (ticks_in_current_state > ATK_END_TICK_BY_SPINE)
                        FSM_change_to(mover.move_type == EN_enemy_move_type.Slide ? EN_dungeon_springtail_FSM.Idle : EN_dungeon_springtail_FSM.Default);
                    break;

                default:
                    break;
            }


            if (melee_atk_cd > 0)
                melee_atk_cd--;

            if (target_locked_on != null)
                ticks_target_has_been_locked_on++;

            mover.move();

        }


        private void FSM_change_to(EN_dungeon_springtail_FSM expected_FSM)
        {
            m_state = expected_FSM;
            ticks_in_current_state = 0;
            switch (expected_FSM)
            {
                case EN_dungeon_springtail_FSM.Default:
                    break;
                case EN_dungeon_springtail_FSM.Idle:
                    break;
                case EN_dungeon_springtail_FSM.Jumping:
                    Jump_Finished = false;
                    break;
                case EN_dungeon_springtail_FSM.Atk_Melee:
                    melee_atk_finished = false;
                    break;
                case EN_dungeon_springtail_FSM.Falling:
                    break;
                default:
                    break;
            }
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

