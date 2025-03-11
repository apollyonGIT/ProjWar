using AutoCodes;
using Commons;
using UnityEngine;
using World.Caravans;
using World.Widgets;

namespace World.Devices
{
    public class BasicWheel : Device
    {
        #region Const
        private const string BONE_FOR_ROTATE = "roll_control";

        private const float ANGLE_LIMIT = 1919810F;
        private const float ANGLE_RESET = 114514F * Mathf.PI;

        private const int BROKEN_MALFUNC_TICKS_RAND_MIN = 300;
        private const int BROKEN_MALFUNC_TICKS_RAND_MAX = 2000;
        #endregion

        private enum FSM_Wheel
        {
            Running,
            Braking,
            Jumping,
            Broken,
        }
        private FSM_Wheel fsm;

        private float angle_rotated;
        private float wheel_radius_reciprocal;
        private float wheel_jumping_visual_speed;

        private int broken_malfunc_ticks;
        private int broken_malfunc_ticks_current;

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            device_wheels.TryGetValue(desc.id.ToString() + ",0", out var wheel_rc);
            wheel_radius_reciprocal = 1f / wheel_rc.wheel_radius_visual;

            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            generate_broken_malfunc_ticks();
            FSM_change_to(FSM_Wheel.Braking);
        }

        public override void tick()
        {
            var vel = (this as ITarget).velocity;
            var wctx = WorldContext.instance;

            switch (fsm)
            {
                case FSM_Wheel.Running:
                    if (!is_validate)
                        FSM_change_to(FSM_Wheel.Broken);
                    else
                    {
                        if (faction == WorldEnum.Faction.player)
                            if (wctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.braking)
                                FSM_change_to(FSM_Wheel.Braking);
                            else if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                                FSM_change_to(FSM_Wheel.Jumping);
                            else
                                wheel_rotate(vel.magnitude);
                        else
                            wheel_rotate(vel.magnitude);
                    }
                    break;

                case FSM_Wheel.Braking:
                    // 只有玩家可以进入这一状态，所以不需要检查faction
                    if (!is_validate)
                        FSM_change_to(FSM_Wheel.Broken);
                    else if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                        FSM_change_to(FSM_Wheel.Jumping);
                    else if (wctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.driving)
                        FSM_change_to(FSM_Wheel.Running);
                    break;

                case FSM_Wheel.Jumping:
                    // 只有玩家可以进入这一状态，所以不需要检查faction
                    if (!is_validate)
                        FSM_change_to(FSM_Wheel.Broken);
                    else if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.ground)
                        FSM_change_to(wctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.driving ? FSM_Wheel.Running : FSM_Wheel.Braking);
                    else
                        wheel_rotate(wheel_jumping_visual_speed);
                    break;

                case FSM_Wheel.Broken:
                    if (is_validate)
                        FSM_change_to(FSM_Wheel.Running);
                    else
                    {
                        if (faction == WorldEnum.Faction.player)
                        {
                            if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                                wheel_rotate(wheel_jumping_visual_speed);
                            else
                            {
                                if (vel.magnitude > 1f)
                                {
                                    if (broken_malfunc_ticks_current >= broken_malfunc_ticks)
                                    {
                                        broken_malfunc_ticks_current = 0;
                                        generate_broken_malfunc_ticks();
                                        
                                        //跳起
                                        CaravanMover.do_jump_input_vy(vel.magnitude * 0.5f);

                                        //刹车，且不重置target_lever高度
                                        //CaravanMover.mgr.cell.Brake();
                                        Widget_DrivingLever_Context.instance.SetLever(0, false);
                                        WorldContext.instance.caravan_status_acc = WorldEnum.EN_caravan_status_acc.braking;
                                    }
                                    else
                                        broken_malfunc_ticks_current++;                                   
                                }
                                wheel_rotate(vel.magnitude);
                            }                           
                        }
                        else
                            wheel_rotate(vel.magnitude);
                    }
                    break;

                default:
                    break;
            }


            base.tick();
        }

        private void FSM_change_to(FSM_Wheel expected_fsm)
        {
            switch (expected_fsm)
            {
                case FSM_Wheel.Running:
                    wheel_jumping_visual_speed = 10f;
                    break;
                case FSM_Wheel.Braking:
                    wheel_jumping_visual_speed = 0f;
                    break;
                case FSM_Wheel.Broken:
                    wheel_jumping_visual_speed = 1f;
                    broken_malfunc_ticks_current = 0;
                    break;
                default:
                    break;
            }
            fsm = expected_fsm;
        }

        private void wheel_rotate(float v)
        {
            angle_rotated -= v * wheel_radius_reciprocal * Config.PHYSICS_TICK_DELTA_TIME;   //Caculate in Rad

            if (Mathf.Abs(angle_rotated) > ANGLE_LIMIT)
                angle_rotated -= Mathf.Sign(angle_rotated) * ANGLE_RESET;

            bones_direction[BONE_FOR_ROTATE] = new Vector2(Mathf.Cos(angle_rotated), Mathf.Sin(angle_rotated));
        }

        private void generate_broken_malfunc_ticks()
        {
            broken_malfunc_ticks = Random.Range(BROKEN_MALFUNC_TICKS_RAND_MIN, BROKEN_MALFUNC_TICKS_RAND_MAX);
        }   


    }
}
