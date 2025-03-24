using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.DeviceUiViews;
using World.Devices.DeviceViews;

namespace World.Devices
{
    public class DevicePD : Producer
    {
        public override IMgr imgr => mgr;
        public Transform ui_content;
        DeviceMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            var ctx = WorldContext.instance;

            mgr = new(Config.DeviceMgr_Name, priority);
            mgr.pd = this;

            if (!Config.current.is_load_devices) return;

            // 这里把轮子放在第一个，使为了使其在下面的UI面板中靠前
            device_alls.TryGetValue(Config.current.init_wheel.ToString() + ",0", out var rc0);
            var w1 = mgr.GetDevice(rc0.behavior_script);
            Addressable_Utility.try_load_asset<DeviceView>(rc0.prefeb, out var wview);
            var wv = Instantiate(wview, transform, false);
            w1.add_view(wv);

            Addressable_Utility.try_load_asset<DeviceUiView>(rc0.ui_prefab, out var wui);
            var w_ui_view = Instantiate(wui, ui_content, false);
            w1.add_view(w_ui_view);
           
            w1.InitData(rc0);
            mgr.AddDevice("device_slot_wheel", w1);

            device_wheels.TryGetValue(Config.current.init_wheel.ToString() + ",0", out var wheel_rc);
            ctx.tractive_force_max = wheel_rc.tractive_force_max;
            ctx.feedback_0 = wheel_rc.feedback_0;
            ctx.feedback_1 = wheel_rc.feedback_1;

            //规则：设备安装时，添加其重量
            ctx.total_weight += rc0.weight;

            Device d1 = null;
            if (Config.current.init_device_in_slot_top != 0)
            {
                device_alls.TryGetValue(Config.current.init_device_in_slot_top.ToString() + ",0", out var rc1);
                d1 = mgr.GetDevice(rc1.behavior_script);
                Addressable_Utility.try_load_asset<DeviceView>(rc1.prefeb, out var d1view);
                var gunview = Instantiate(d1view, transform, false);
                d1.add_view(gunview);

                Addressable_Utility.try_load_asset<DeviceUiView>(rc1.ui_prefab, out var d1ui);
                var d1_ui_view = Instantiate(d1ui, ui_content, false);
                d1.add_view(d1_ui_view);

                foreach (var kp in gunview.dkp)
                {
                    d1.key_points.Add(kp.key_name, kp.transform);
                }
                d1.InitData(rc1);

                //规则：设备安装时，添加其重量
                ctx.total_weight += rc1.weight;
            }
            mgr.AddDevice("slot_top", d1);

            Device d_ft = null;
            if (Config.current.init_device_in_slot_front_top != 0)
            {
                device_alls.TryGetValue(Config.current.init_device_in_slot_front_top.ToString() + ",0", out var rc_ft);
                d_ft = mgr.GetDevice(rc_ft.behavior_script);
                Addressable_Utility.try_load_asset<DeviceView>(rc_ft.prefeb, out var d_ft_view);
                var d_ft_v = Instantiate(d_ft_view, transform, false);
                d_ft.add_view(d_ft_v);

                Addressable_Utility.try_load_asset<DeviceUiView>(rc_ft.ui_prefab, out var d_ft_ui);
                var ft_ui_view = Instantiate(d_ft_ui, ui_content, false);
                d_ft.add_view(ft_ui_view);


                foreach (var kp in d_ft_v.dkp)
                {
                    d_ft.key_points.Add(kp.key_name, kp.transform);
                }
                d_ft.InitData(rc_ft);                   //数据部分(待修改)    

                //规则：设备安装时，添加其重量
                ctx.total_weight += rc_ft.weight;
            }
            mgr.AddDevice("slot_front_top", d_ft);

            Device d_bt = null;
            if (Config.current.init_device_in_slot_back_top != 0)
            {
                device_alls.TryGetValue(Config.current.init_device_in_slot_back_top.ToString() + ",0", out var rc_bt);
                d_bt = mgr.GetDevice(rc_bt.behavior_script);
                Addressable_Utility.try_load_asset<DeviceView>(rc_bt.prefeb, out var d_bt_view);
                var d_bt_v = Instantiate(d_bt_view, transform, false);
                d_bt.add_view(d_bt_v);

                Addressable_Utility.try_load_asset<DeviceUiView>(rc_bt.ui_prefab, out var d_bt_ui_view);
                var bt_ui_view = Instantiate(d_bt_ui_view, ui_content, false);
                d_bt.add_view(bt_ui_view);

                foreach (var kp in d_bt_v.dkp)
                {
                    d_bt.key_points.Add(kp.key_name, kp.transform);
                }
                d_bt.InitData(rc_bt);                   //数据部分(待修改)

                //规则：设备安装时，添加其重量
                ctx.total_weight += rc_bt.weight;
            }
            mgr.AddDevice("slot_back_top", d_bt);

            Device d2 = null;
            if (Config.current.init_device_in_slot_front != 0)
            {
                device_alls.TryGetValue(Config.current.init_device_in_slot_front.ToString() + ",0", out var rc2);
                d2 = mgr.GetDevice(rc2.behavior_script);        //代码部分
                Addressable_Utility.try_load_asset<DeviceView>(rc2.prefeb, out var d2view);
                var dv = Instantiate(d2view, transform, false);
                d2.add_view(dv);                    //外观部分

                Addressable_Utility.try_load_asset<DeviceUiView>(rc2.ui_prefab, out var d2_ui_view);
                var d2_ui = Instantiate(d2_ui_view, ui_content, false);
                d2.add_view(d2_ui);

                foreach (var kp in dv.dkp)
                {
                    d2.key_points.Add(kp.key_name, kp.transform);
                }
                d2.InitData(rc2);                   //数据部分(待修改)

                //规则：设备安装时，添加其重量
                ctx.total_weight += rc2.weight;
            }
            mgr.AddDevice("slot_front", d2);

            Device d4 = null;
            if (Config.current.init_device_in_slot_back != 0)
            {
                device_alls.TryGetValue(Config.current.init_device_in_slot_back.ToString() + ",0", out var rc4);
                d4 = mgr.GetDevice(rc4.behavior_script);
                Addressable_Utility.try_load_asset<DeviceView>(rc4.prefeb, out var d4view);
                var d_4_v = Instantiate(d4view, transform, false);
                d4.add_view(d_4_v);

                Addressable_Utility.try_load_asset<DeviceUiView>(rc4.ui_prefab, out var d4_ui_view);
                var d4_ui = Instantiate(d4_ui_view, ui_content, false);
                d4.add_view(d4_ui);

                foreach (var kp in d_4_v.dkp)
                {
                    d4.key_points.Add(kp.key_name, kp.transform);
                }
                d4.InitData(rc4);                   //数据部分(待修改)

                //规则：设备安装时，添加其重量
                ctx.total_weight += rc4.weight;
            }
            mgr.AddDevice("slot_back", d4);

            mgr.Init();


            SortUi();
        }

        private void SortUi()
        {
            if (ui_content != null)
            {
                var start_pos = new Vector2(-600, 0);
                for(int i =0;i< ui_content.childCount - 1; i++)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_content.GetChild(i).GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_content.GetChild(i+1).GetComponent<RectTransform>());

                    var rect = ui_content.GetChild(i).GetComponent<RectTransform>();
                    start_pos = new Vector2(start_pos.x, 0 + rect.rect.height / 2);
                    rect.anchoredPosition = start_pos;

                    start_pos = new Vector2(start_pos.x + rect.rect.width / 2 + ui_content.GetChild(i + 1).GetComponent<RectTransform>().rect.width / 2, start_pos.y);
                }

                if(ui_content.childCount >= 1)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ui_content.GetChild(ui_content.childCount-1).GetComponent<RectTransform>());
                    ui_content.GetChild(ui_content.childCount - 1).GetComponent<RectTransform>().anchoredPosition = start_pos;
                }
            }
        }

        public override void call()
        {
        }
    }
}