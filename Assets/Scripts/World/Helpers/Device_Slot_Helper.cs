using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using UnityEngine;
using World.Devices;
using World.Devices.DeviceViews;

namespace World.Helpers
{
    public class Device_Slot_Helper
    {
        public static void InstallDevice(string device_id,string slot)
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            device_alls.TryGetValue(device_id, out var rc1);
            var device = dmgr.GetDevice(rc1.behavior_script);
            Addressable_Utility.try_load_asset<DeviceView>(rc1.prefeb, out var d1view);
            var device_view = GameObject.Instantiate(d1view, dmgr.pd.transform, false);
            device.add_view(device_view);
            foreach (var kp in device_view.dkp)
            {
                device.key_points.Add(kp.key_name, kp.transform);
            }
            device.InitData(rc1);
            dmgr.InstallDevice(slot, device);

            WorldContext.instance.caravan_slots.TryGetValue(slot, out var slot_t);
            if (slot_t != null)
            {
                var v = new Vector2(slot_t.x, slot_t.y);
                var angle = Vector2.SignedAngle(Vector2.right, WorldContext.instance.caravan_dir);
                var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * v;

                var cp = WorldContext.instance.caravan_pos;

                device.position = WorldContext.instance.caravan_pos + new Vector2(new_v.x, new_v.y);
            }
            device.InitPos();
        }

        public static Device RemoveDevice(string slot)
        {
            Device remove_device = null;
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            dmgr.slots_device.TryGetValue(slot, out remove_device);
            if (remove_device != null)
            {
              
                dmgr.slots_device[slot] = null;
                remove_device.remove_all_views();
            }
            return remove_device;
        }

        public static Device InstallDevice(Device device,string slot)
        {
            var remove_device = RemoveDevice(slot);
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            Addressable_Utility.try_load_asset<DeviceView>(device.desc.prefeb, out var d1view);
            var device_view = GameObject.Instantiate(d1view, dmgr.pd.transform, false);
            device.add_view(device_view);

            device.key_points.Clear();
            foreach (var kp in device_view.dkp)
            {
                device.key_points.Add(kp.key_name, kp.transform);
            }
            device.InitData(device.desc);
            dmgr.InstallDevice(slot, device);

            WorldContext.instance.caravan_slots.TryGetValue(slot, out var slot_t);
            if (slot_t != null)
            {
                var v = new Vector2(slot_t.x, slot_t.y);
                var angle = Vector2.SignedAngle(Vector2.right, WorldContext.instance.caravan_dir);
                var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * v;

                var cp = WorldContext.instance.caravan_pos;

                device.position = WorldContext.instance.caravan_pos + new Vector2(new_v.x, new_v.y);
            }
            device.InitPos();

            return remove_device;
        }

        public static Device GetDevice(string slot)
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            dmgr.slots_device.TryGetValue(slot, out var device);
            return device;
        }
    }
}
