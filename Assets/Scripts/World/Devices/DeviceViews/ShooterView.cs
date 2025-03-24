using UnityEngine;

namespace World.Devices.DeviceViews
{
    public class ShooterView : DeviceView
    {
        public LineRenderer aim_line;

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if (owner.player_oper && dkp.Count > 0 && aim_line != null)
            {
                aim_line.positionCount = 2;
                var cv = WorldContext.instance.caravan_dir.normalized;
                var bv = owner.bones_direction["roll_control"];
                var v1 = dkp[0].transform.position;
                var v2 = new Vector3(v1.x + (bv.x * cv.x - bv.y * cv.y) * 100, v1.y + (bv.x * cv.y + bv.y * cv.x) * 100, v1.z);
                aim_line.SetPosition(0, v1);
                aim_line.SetPosition(1, v2);
            }
        }

        public override void notify_player_oper(bool oper)
        {
            if (aim_line != null)
            {
                if (oper)
                    aim_line.enabled = true;
                else
                    aim_line.enabled = false;
            }
        }
    }
}
