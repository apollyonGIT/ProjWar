using TMPro;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class BasicShooterUiView : DeviceUiView
    {
        public TextMeshProUGUI ammoText;
        public Image ammo_progress;
        public Image manual_reload_indicator;
        public override void init()
        {
            base.init();
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();
            var shooter = (owner as NewBasicShooter);

            if (shooter.reloading)
            {
                ammoText.text = "Reloading:\n";
                if (shooter.reload_by_stage)
                    ammoText.text += ((float)shooter.reload_stage_current / shooter.reload_stage_max).ToString("P0");
                else
                    ammoText.text += $"{shooter.Current_Ammo}/{shooter.Max_Ammo}";
            }
            else
                ammoText.text = $"Ammo:\n{shooter.Current_Ammo}/{shooter.Max_Ammo}";

            ammo_progress.fillAmount = shooter.Reloading_Process;
            manual_reload_indicator.gameObject.SetActive(shooter.can_manual_reload);
        }
    }
}
