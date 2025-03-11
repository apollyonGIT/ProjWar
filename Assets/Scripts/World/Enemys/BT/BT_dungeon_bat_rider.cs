using System.Collections.Generic;
using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_dungeon_bat_rider : Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Be_Separated, IEnemy_Can_Shoot, IEnemy_Can_Flyaround
    {
        #region CONST
        const float FLYAROUND_DEG_MIN = 40F;
        const float FLYAROUND_DEG_MAX = 140F;
        const float FLYAROUND_RADIUS_MIN = 4.5F;
        const float FLYAROUND_RADIUS_MAX = 7F;
        const float FLYAROUND_RADIUS_RATIO = 0.66F;

        const string MUZZLE_BONE_NAME_IN_SPINE = "proj_muzzle";

        const float FLY_SPEED_IDLE = 4F;
        const float FLY_SPEED_MAX = 22F;
        const float FLY_SPEED_V_COEF = 0.4F;
        const float FLY_SPEED_DX_TRIGGER_DISTANCE = 7F;
        const float FLY_SPEED_DX_COEF = 0.6F;

        const ushort TICKS_PER_SEC_IN_SPINE = 30;
        const ushort SHOOT_ATK_TICK_BY_SPINE = 21 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_END_TICK_BY_SPINE = 35 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        #endregion
        private enum EN_dungeon_bat_rider_FSM
        {
            Default,
            Ready,
            Shoot
        }
        EN_dungeon_bat_rider_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        #region Interface Separatable
        public bool Separate_All { get; set; }
        public Dictionary<string, int> Sub_Monsters { get; set; }
        #endregion

        #region Interface Shoot
        public int Shoot_CD { get; set; }
        public int Shoot_CD_Max { get; set; }
        public bool Shoot_Finished { get; set; }
        public float Projectile_Speed { get; set; }
        public IEnemy_Can_Shoot I_Shoot { get; set; }
        #endregion

        #region Interface Flyaround
        public (float, float) Flyaround_Deg { get; } = (FLYAROUND_DEG_MIN, FLYAROUND_DEG_MAX);
        public (float, float) Flyaround_Radius { get; } = (FLYAROUND_RADIUS_MIN, FLYAROUND_RADIUS_MAX);
        public float Flyaround_Radius_Ratio { get; } = FLYAROUND_RADIUS_RATIO;
        public Vector2 Flyaround_Relative_Following_Pos { get; set; }
        #endregion
        private IEnemy_Can_Flyaround I_Flyaround;

        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            FSM_change_to((EN_dungeon_bat_rider_FSM)System.Enum.Parse(typeof(EN_dungeon_bat_rider_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Fly;

            Sub_Monsters = cell._desc.sub_monsters;

            I_Shoot = cell.bt as IEnemy_Can_Shoot;
            I_Shoot.Init_Shooting_Data(cell);

            I_Flyaround = cell.bt as IEnemy_Can_Flyaround;
            I_Flyaround.Init_Or_Reset_Relative_Following_Pos();
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            var speed_expt = FLY_SPEED_IDLE
                + cell.mgr.ctx.caravan_velocity.x * FLY_SPEED_V_COEF
                + Mathf.Max(cell.mgr.ctx.caravan_pos.x - FLY_SPEED_DX_TRIGGER_DISTANCE - cell.pos.x, 0) * FLY_SPEED_DX_COEF;
            I_Flyaround.Set_Expt_Speed_By_HP(cell, speed_expt, FLY_SPEED_MAX);

            // Main FSM
            switch (m_state)
            {
                case EN_dungeon_bat_rider_FSM.Default:
                    Vector2 target_pos_focus = target_locked_on == null ? ctx.caravan_pos : target_locked_on.Position;
                    I_Flyaround.Flyaround_Per_Tick(cell, target_pos_focus);

                    if (target_locked_on == null)
                        lock_target();

                    if (target_locked_on != null)
                    {
                        if (Shoot_CD <= 0)
                            FSM_change_to(EN_dungeon_bat_rider_FSM.Ready);
                        else
                            Shoot_CD--;
                    }

                    cell.dir.x = target_pos_focus.x - cell.pos.x;
                    break;

                case EN_dungeon_bat_rider_FSM.Ready:
                    if (target_locked_on == null)
                    {
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Default);
                        break;
                    }

                    var delta_x = target_locked_on.Position.x - cell.pos.x;
                    if (Mathf.Abs(delta_x) >= 6f)
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Shoot);
                    else
                        cell.position_expt.x += delta_x >= 0 ? -6f : 6f;

                    cell.position_expt.y = Mathf.Min(cell.position_expt.y, target_locked_on.Position.y + 2f);
                    cell.dir.x = delta_x;

                    break;


                case EN_dungeon_bat_rider_FSM.Shoot:
                    ticks_in_current_state++;

                    if (ticks_in_current_state >= SHOOT_END_TICK_BY_SPINE)
                    {
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Default);
                        target_locked_on = null;
                        break;
                    }

                    if (target_locked_on != null)
                    {
                        var tp = target_locked_on.Position;
                        cell.position_expt.x = tp.x + (cell.pos.x > tp.x ? 8f : -8f);

                        if (ticks_in_current_state >= SHOOT_ATK_TICK_BY_SPINE && !Shoot_Finished)
                            I_Shoot.Monster_Shoot(cell, MUZZLE_BONE_NAME_IN_SPINE, target_locked_on.Position);
                        cell.dir.x = target_locked_on.Position.x - cell.pos.x;
                    }
                    else
                    {
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Default);
                        cell.dir.x = cell.velocity.x;
                    }
                    break;

                default:
                    break;
            }

            mover.move();
        }


        void FSM_change_to(EN_dungeon_bat_rider_FSM expected_fsm)
        {
            m_state = expected_fsm;
            ticks_in_current_state = 0;
            switch (expected_fsm)
            {
                case EN_dungeon_bat_rider_FSM.Shoot:
                    Shoot_Finished = false;
                    break;
                default:
                    break;
            }
        }

        override protected void basic_die(Enemy self)
        {
            IEnemy_Can_Be_Separated i_sepatate = self.bt as IEnemy_Can_Be_Separated;
            i_sepatate.Die_Of_Being_Seperated(self);
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

