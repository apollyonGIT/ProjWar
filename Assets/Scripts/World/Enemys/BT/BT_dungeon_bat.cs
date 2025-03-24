using Commons;
using UnityEngine;

namespace World.Enemys.BT
{
    public class BT_dungeon_bat : Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Flyaround
    {
        #region CONST
        const float FLYAROUND_DEG_MIN = 10F;
        const float FLYAROUND_DEG_MAX = 170F;
        const float FLYAROUND_RADIUS_MIN = 5F;
        const float FLYAROUND_RADIUS_MAX = 14F;
        const float FLYAROUND_RADIUS_RATIO = 0.2F;

        const ushort CHARGE_CD = 6 * Config.PHYSICS_TICKS_PER_SECOND;
        const int MAX_CHARGE_TICKS = 180;

        const float FLY_SPEED_IDLE = 9F;
        const float FLY_SPEED_BURST = 16F;
        const float FLY_SPEED_MAX = 25F;
        #endregion

        private enum EN_dungeon_bat_FSM
        {
            Default,
            Move,
            Charge,
        }
        EN_dungeon_bat_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        private bool charge_to_right;
        private bool can_bite;
        private bool charge_over_distance;

        Enemy self;
        Vector2 caravan_pos_focus;

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
            self = cell;
            FSM_change_to((EN_dungeon_bat_FSM)System.Enum.Parse(typeof(EN_dungeon_bat_FSM), (string)prms[0]));

            caravan_pos_focus = cell.mgr.ctx.caravan_pos + Vector2.up;

            I_Flyaround = this;
            I_Flyaround.Init_Or_Reset_Relative_Following_Pos();
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            // Main FSM
            switch (m_state)
            {
                case EN_dungeon_bat_FSM.Default:
                case EN_dungeon_bat_FSM.Move:
                    if (Target_Locked_On == null)
                        Lock_Target();

                    if (Target_Locked_On == null)
                        caravan_pos_focus = cell.mgr.ctx.caravan_pos + Vector2.up;
                    else
                    {
                        caravan_pos_focus = Get_Target_Pos().Value;

                        var abs_dis_x = Mathf.Abs(cell.pos.x - caravan_pos_focus.x);
                        if (Check_State_Time(CHARGE_CD))
                        {
                            if (abs_dis_x > 3f && abs_dis_x < 12f && cell.pos.y < caravan_pos_focus.y + abs_dis_x * 0.6f)
                                FSM_change_to(EN_dungeon_bat_FSM.Charge);
                        }
                        else
                            Ticks_In_Current_State++;
                    }

                    set_speed_expt(cell, FLY_SPEED_IDLE);
                    I_Flyaround.Flyaround_Per_Tick(cell, caravan_pos_focus);

                    break;

                case EN_dungeon_bat_FSM.Charge:
                    // Back to Move State if Condition Met
                    if (Target_Locked_On == null || Check_State_Time(MAX_CHARGE_TICKS))
                    {
                        FSM_change_to(EN_dungeon_bat_FSM.Move);
                        break;
                    }

                    Ticks_In_Current_State++;
                    caravan_pos_focus = Get_Target_Pos().Value;

                    set_speed_expt(cell, FLY_SPEED_BURST);

                    var dis_to_focus_pos = caravan_pos_focus - cell.pos;

                    if (!charge_over_distance)
                    {
                        var pos_y_feedback = -Mathf.Min(0, cell.pos.y - caravan_pos_focus.y - 1.5f);
                        cell.position_expt = 2 * caravan_pos_focus + new Vector2(charge_to_right ? 3f : -3f, pos_y_feedback) - cell.pos;

                        if ((dis_to_focus_pos.x + (charge_to_right ? 1f : -1f) < 0) == charge_to_right)
                        {
                            bat_end_charge_earlier(30);
                            charge_over_distance = true;
                        }
                    }
                    else
                    {
                        cell.position_expt = new Vector2(caravan_pos_focus.x + (charge_to_right ? 1 : -1), 10f);
                    }

                    if (can_bite && dis_to_focus_pos.magnitude < 0.5f)
                    {
                        Attack_Data attack_data = new()
                        {
                            atk = cell._desc.basic_atk
                        };

                        Target_Locked_On.hurt(attack_data);
                        can_bite = false;
                        bat_end_charge_earlier(120);
                    }
                    break;

                default:
                    break;
            }

            //朝向
            cell.dir.x = cell.velocity.x;
            mover.move();
        }

        private void FSM_change_to(EN_dungeon_bat_FSM state)
        {
            m_state = state;
            Ticks_In_Current_State = 0;
            switch (state)
            {
                case EN_dungeon_bat_FSM.Default:
                    self.mover.move_type = EN_enemy_move_type.Fly;
                    Ticks_In_Current_State = (ushort)Random.Range(0, CHARGE_CD);
                    break;
                case EN_dungeon_bat_FSM.Move:
                    break;
                case EN_dungeon_bat_FSM.Charge:
                    charge_to_right = (caravan_pos_focus - self.pos).x >= 0;
                    can_bite = true;
                    charge_over_distance = false;
                    break;
                default:
                    break;
            }
        }

        private void set_speed_expt(Enemy cell, float v)
        {
            var v_expt = v + cell.mgr.ctx.caravan_velocity.x * 0.2f;
            cell.speed_expt = Monster_Common_Action.Get_Speed_Expt_By_Clamp(v_expt, FLY_SPEED_MAX);
        }

        private void bat_end_charge_earlier(int ticks_earlier)
        {
            Ticks_In_Current_State = (ushort)Mathf.Max(MAX_CHARGE_TICKS - ticks_earlier, Ticks_In_Current_State);
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

