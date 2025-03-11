﻿using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Caravans.Mover
{
    public class CaravanMover_Land
    {
        public static void @do()
        {
            var mgr = CaravanMover.mgr;
            var ctx = mgr.ctx;

            ref var pos = ref ctx.caravan_pos;
            ref var velocity = ref ctx.caravan_velocity;
            ref var acc = ref ctx.caravan_acc;

            //1.着陆发生时，计算着陆点地形的方向向量和法向量
            Road_Info_Helper.try_get_leap_rad(pos.x, out var ground_rad);
            var sin_ground_rad = Mathf.Sin(ground_rad);
            var cos_ground_rad = Mathf.Cos(ground_rad);

            Vector2 grorund_slope = new(cos_ground_rad, sin_ground_rad);
            Vector2 ground_normal = new(-sin_ground_rad, cos_ground_rad);

            //2.着陆发生时，计算小车此时的“切向速度”与“法向速度”
            var v_landing_parallel = Vector2.Dot(velocity, grorund_slope) * grorund_slope;
            var v_landing_vertical = Vector2.Dot(velocity, ground_normal) * ground_normal;

            //3.计算小车“能否弹起”
            var can_bounce = false;
            var vtm_sqr = v_landing_vertical.sqrMagnitude;
            var vtm = v_landing_vertical.magnitude;

            if (vtm_sqr > Config.current.caravan_bounce_loss)
            {
                v_landing_vertical *= -Mathf.Sqrt(vtm_sqr - Config.current.caravan_bounce_loss) * Config.current.caravan_bounce_coefficient / vtm;

                can_bounce = v_landing_vertical.x + v_landing_parallel.x >= 0;
            }

            //4.接上一步，根据小车能否二次弹跳，对落地逻辑的后续处理
            pos.y = Road_Info_Helper.try_get_altitude(pos.x);

            if (can_bounce)
            {
                velocity.x = v_landing_parallel.x + v_landing_vertical.x;
                CaravanMover.do_jump_input_vy(Mathf.Max(0, v_landing_parallel.y + v_landing_vertical.y));
            }
            else
            {
                velocity = v_landing_parallel;
                CaravanMover.do_run();
                CaravanMover.move();
            }
        }
    }
}

