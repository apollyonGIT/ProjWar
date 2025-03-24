using Foundations.Tickers;
using UnityEngine;
using World.Caravans;
using World.Enemys;

namespace World.Enemy_Cars.BT
{
    public class Car_AI_Basic : IEnemy_BT
    {
        EN_caravan_move_type m_state = EN_caravan_move_type.Run;

        string IEnemy_BT.state => $"{m_state}";

        //==================================================================================================

        void IEnemy_BT.init(Enemy _cell, params object[] prms)
        {
            var cell = (Enemy_Car)_cell;
            ref var ctx = ref cell.ctx;

            ctx.driving_lever = 0.5f;
        }

        int cd = 5;
        void IEnemy_BT.tick(Enemy _cell)
        {
            var cell = (Enemy_Car)_cell;
            var car_mover = cell.car_mover;

            if (cd-- == 0)
            {
                cd = 300;
                car_mover.do_jump_input_h(cell, 3);
            }
        }


        void IEnemy_BT.notify_on_enter_die(Enemy cell)
        {
            cell.fini();
        }


        void IEnemy_BT.notify_on_dying(Enemy cell)
        {

        }


        void IEnemy_BT.notify_on_dead(Enemy cell)
        {

        }
    }
}

