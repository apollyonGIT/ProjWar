using Commons;
using ExcelDataReader;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using World.Helpers;

namespace World.Enemys.BT
{

    public class BT_dungeon_beatle : Monster_Basic_BT, IEnemy_BT
    {
        #region CONST
        const float FLY_SPEED_TAKE_OFF = 12F;
        const float FLY_SPEED_CHARGE_BEGIN = 8F;
        const float FLY_SPEED_MAX = 30F;

        const int AWARENESS_CAPACITY_MIN = 480;
        const int AWARENESS_CAPACITY_MAX = 960;
        const int AWARENESS_DACAY_PER_TICK = 9;
        const float AWARENESS_TRIGGER_DISTANCE_THRESHOLD = 10F;
        const float AWARENESS_TRIGGER_DISTANCE_COEF = 15F;

        const float CHASING_FOCUS_Y_COEF_MIN = 0.15F;
        const float CHASING_FOCUS_Y_COEF_MAX = 0.35F;

        const float EXPLOSION_RADIUS = 1.2F;
        const float EXPLOSION_EFFECT_MIN = 0.25F;
        const float EXPLOSION_KNOCKBACK_FT = 20F;

        const int DEATH_VFX_EXPLOSION_INDEX = 4;
        #endregion

        private enum EN_dungeon_beatle_FSM
        {
            Default,    // Init State 1
            Default_Awaken,  // Init State 2
            Fly_Wait,
            Fly_Charge
        }
        private EN_dungeon_beatle_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        private int awareness;
        private Vector2 pos_expct;
        private float chasing_focus_y_coef;

        private bool explode;

        private int awareness_capacity = Random.Range(AWARENESS_CAPACITY_MIN, AWARENESS_CAPACITY_MAX);


        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            var state = (string)prms[0];
            m_state = (EN_dungeon_beatle_FSM)System.Enum.Parse(typeof(EN_dungeon_beatle_FSM), state);

            switch (m_state)
            {
                case EN_dungeon_beatle_FSM.Default:
                    cell.mover.move_type = EN_enemy_move_type.Slide;
                    cell.dir = Random.value > 0.5f ? Vector2.right : Vector2.left;
                    break;
                // Only Initialized in Beast Tide Mode
                case EN_dungeon_beatle_FSM.Default_Awaken:
                    cell.mover.move_type = EN_enemy_move_type.Fly;
                    chasing_focus_y_coef = Random.Range(CHASING_FOCUS_Y_COEF_MIN, CHASING_FOCUS_Y_COEF_MAX);
                    m_state = EN_dungeon_beatle_FSM.Fly_Charge;
                    break;
                default:
                    break;
            }
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            switch (m_state)
            {
                case EN_dungeon_beatle_FSM.Default:
                    var dx = Mathf.Abs(ctx.caravan_pos.x - cell.pos.x);
                    if (dx < AWARENESS_TRIGGER_DISTANCE_THRESHOLD)
                        awareness += (int)(ctx.caravan_velocity.magnitude * AWARENESS_TRIGGER_DISTANCE_COEF);

                    if (awareness < awareness_capacity)
                        awareness -= AWARENESS_DACAY_PER_TICK;
                    else
                    {
                        m_state = EN_dungeon_beatle_FSM.Fly_Wait;   // 飞起来，但是懵逼
                        mover.move_type = EN_enemy_move_type.Fly;
                        pos_expct = cell.pos + new Vector2(0, Random.Range(2.5f, 3f));
                    }
                    break;


                case EN_dungeon_beatle_FSM.Fly_Wait:
                    ticks_in_current_state++;

                    Monster_Common_Action.Sync_Pos_By_World_Pos_Reset(ref pos_expct);
                    cell.position_expt = pos_expct;

                    set_speed_expt(cell, Mathf.Max(FLY_SPEED_TAKE_OFF - ticks_in_current_state * 0.03f, 3f));

                    if (ticks_in_current_state >= 100)
                        cell.dir.x = ctx.caravan_pos.x - cell.pos.x;    //朝向

                    if (ticks_in_current_state >= 380)
                    {
                        ticks_in_current_state = 0;
                        m_state = EN_dungeon_beatle_FSM.Fly_Charge;
                        chasing_focus_y_coef = Random.Range(CHASING_FOCUS_Y_COEF_MIN, CHASING_FOCUS_Y_COEF_MAX);
                    }
                    break;


                case EN_dungeon_beatle_FSM.Fly_Charge:
                    ticks_in_current_state++;

                    set_speed_expt(cell, FLY_SPEED_CHARGE_BEGIN + ticks_in_current_state * 0.012f);

                    if (ticks_in_current_state > 10)
                        explode = true;

                    var distance_fall_behind = Mathf.Clamp(ctx.caravan_pos.x - cell.pos.x, 3f, 8.5f);
                    var y_feedback = -cell.velocity.y > (cell.pos.y - ctx.caravan_pos.y) ? -cell.velocity.y : 0;
                    var caravan_pos_focus = ctx.caravan_pos + new Vector2(0, distance_fall_behind * chasing_focus_y_coef + y_feedback);
                    cell.position_expt = caravan_pos_focus;

                    cell.dir.x = cell.velocity.x;

                    if ((cell.position_expt - cell.pos).magnitude < 0.8f)
                        cell.hp_self = -1;
                    break;


                default:
                    break;
            }

            mover.move();

        }

        private void set_speed_expt(Enemy cell, float v)
        {
            cell.speed_expt = Monster_Common_Action.Get_Speed_Expt_By_Clamp(v, FLY_SPEED_MAX);
        }


        private void die_of_explosion(Enemy cell)
        {
            Monster_Common_Action.Set_Death_VFX_By_Index(cell, DEATH_VFX_EXPLOSION_INDEX);
            // 自爆！！
            BoardCast_Helper.to_all_target(explosion_dungeon_beatle);
            cell.fini();

            #region 子函数 explosion_dungeon_beatle
            void explosion_dungeon_beatle(ITarget target)
            {
                var dis = Vector2.Distance(target.Position, cell.pos);
                if (dis >= EXPLOSION_RADIUS)  //爆炸范围
                    return;

                // Make Damage
                var dmg_dis_coef = Monster_Common_Action.Get_Damage_Mod_By_Distance(dis, EXPLOSION_RADIUS, EXPLOSION_EFFECT_MIN);
                Attack_Data attack_data = new()
                {
                    atk = Mathf.CeilToInt(cell._desc.basic_atk * dmg_dis_coef)
                };

                target.hurt(attack_data);

                // Make KnockBack
                if (target.Faction != WorldEnum.Faction.player)
                    target.impact(WorldEnum.impact_source_type.melee, cell.pos, target.Position, EXPLOSION_KNOCKBACK_FT * dmg_dis_coef);
            }
            #endregion
        }



        void IEnemy_BT.notify_on_enter_die(Enemy cell)
        {
            if (explode)
                die_of_explosion(cell);
            else
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

