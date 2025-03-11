using UnityEngine.UI;
using World.Devices.NewDevice;

namespace World.Devices.DeviceUiViews
{
    public class UIView_Unique_WarDrum : DeviceUiView
    {
        public Image indicator_left;
        public Image indicator_right;
        public Slider indicator_factor_expt;
        public override void init()
        {
            base.init();
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();
            var drum = (owner as Unique_War_Drum);

            indicator_left.gameObject.SetActive(drum.manual_left_ready);
            indicator_right.gameObject.SetActive(drum.manual_right_ready);
            indicator_factor_expt.value = drum.attack_factor_expt - 1f;
        }
        public void ControlDevice()
        {
            InputController.instance.SetDeviceControl(owner);
        }
    }
}
