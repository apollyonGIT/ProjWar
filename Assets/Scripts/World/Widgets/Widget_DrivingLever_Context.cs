using Foundations;
using Foundations.Tickers;
using UnityEngine;
using World.Caravans;
using World.Devices.Device_AI;

namespace World.Widgets
{
    public class Widget_DrivingLever_Context : Singleton<Widget_DrivingLever_Context>
    {
        const float LEVER_DROP_SPEED = 0.0001f;

        int freeze_time = FREEZE_TICK;

        const int FREEZE_TICK = 450;

        public float target_lever { get; private set; }

        public CaravanModule driving_module;

        //==================================================================================================

        public void attach()
        {
            Ticker.instance.do_when_tick_start += tick;

            freeze_time = FREEZE_TICK;
            driving_module = new CaravanModule();
        }


        public void detach()
        {
            Ticker.instance.do_when_tick_start -= tick;
        }


        private void tick()
        {
            ref var driving_lever = ref WorldContext.instance.driving_lever;

            if (freeze_time <= 0)
            {
                driving_lever -= LEVER_DROP_SPEED;
                driving_lever = Mathf.Max(driving_lever, 0);
            }
            else
            {
                freeze_time--;
            }

            driving_module.tick();
        }

        public void SetLever(float value, bool also_set_target_lever)
        {
            value = Mathf.Clamp01(value);

            if (also_set_target_lever)
                target_lever = value;
            
            WorldContext.instance.driving_lever = value;

            freeze_time = FREEZE_TICK;
        }

        public void Drag_Lever(bool can_drag, bool drag_to_up, bool also_set_target_lever)
        {
            var base_lever = WorldContext.instance.driving_lever;
            if (can_drag)
            {
                var dir_speed = drag_to_up ? 1.1f : -0.9f;
                base_lever += (dir_speed - base_lever) * CaravanUiView.LEVER_DELTA;
            }

            SetLever(base_lever, also_set_target_lever);

            if (WorldContext.instance.driving_lever != 0)
                WorldContext.instance.caravan_status_acc = WorldEnum.EN_caravan_status_acc.driving;
        }
    }
}

