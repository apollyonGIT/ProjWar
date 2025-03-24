﻿using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using World.Caravans;
using World.Devices;
using World.Devices.DeviceViews;
using World.Enemys;
using World.Helpers;

namespace World.Enemy_Cars
{
    public class Enemy_Car : Enemy
    {
        Caravan m_caravan;
        Dictionary<string, Device> m_devices = new();
        Dictionary<string, Vector3> m_caravan_slot_pos_dic = new();

        public Enemy_Car_Context ctx = new();
        public Enemy_CarMover car_mover = new();

        //==================================================================================================

        public Enemy_Car(uint id) : base(id)
        {
            var sub_desc = (AutoCodes.sub_monster_car)_sub_desc;
            var carbody_id = sub_desc.carbody;

            AutoCodes.caravan_bodys.TryGetValue($"{carbody_id},0", out var r_caravan_body);
            view_resource_name = r_caravan_body.view;

            var monster_behaviour_tree = sub_desc.ai;
            if (!string.IsNullOrEmpty(monster_behaviour_tree))
            {
                bt = (IEnemy_BT)Activator.CreateInstance(Assembly.Load("World").GetType($"World.Enemy_Cars.BT.{monster_behaviour_tree}"));
            }
        }


        protected override string calc_sub_table_key()
        {
            var cell_id = base.calc_sub_table_key();
            var r_expt_cells_0 = AutoCodes.sub_monster_cars.records.Where(t => $"{t.Value.id}" == cell_id);
            Dictionary<string, (int, int)> r_expt_cells_1 = new();
            
            foreach (var e_0 in r_expt_cells_0)
            {
                var sub_id_1 = $"{e_0.Value.sub_id}";
                var odds_1 = r_expt_cells_0.Where(t => t.Value.sub_id < e_0.Value.sub_id).Select(t => t.Value.odds).Sum();
                
                r_expt_cells_1.Add(sub_id_1, (odds_1, odds_1 + e_0.Value.odds));
            }

            var all_odds = r_expt_cells_0.Select(t => t.Value.odds).Sum();
            var random = UnityEngine.Random.Range(0, all_odds);

            var cell_sub_id = r_expt_cells_1.Where(t => random >= t.Value.Item1 && random <t.Value.Item2).First().Key;
            var ret = $"{cell_id},{cell_sub_id}";

            Debug.Log($"生成车类怪物, key: {ret}");
            return ret;
        }


        public override void do_after_init(params object[] args)
        {
            var go = (GameObject)args[0];
            init_caravan_and_device(go);

            ctx.caravan_pos = pos;
            car_mover.init();

            base.do_after_init();
        }


        public void init_caravan_and_device(GameObject obj)
        {
            var sub_desc = (AutoCodes.sub_monster_car)_sub_desc;
            var caravan_view = obj.GetComponent<CaravanView>();

            Caravan.load_caravan_slots(caravan_view, out m_caravan_slot_pos_dic);

            m_caravan = new($"{sub_desc.carbody},0")
            {
                faction = WorldEnum.Faction.opposite
            };

            m_caravan.add_view(caravan_view);

            foreach (var (slot_name, device_id) in sub_desc.device)
            {
                #region 创建device
                AutoCodes.device_alls.TryGetValue($"{device_id},0", out var r_device_all);
                Device device = DeviceMgr.create_device(r_device_all.behavior_script);

                Addressable_Utility.try_load_asset<DeviceView>(r_device_all.prefeb, out var device_view_asset);
                var device_view = UnityEngine.Object.Instantiate(device_view_asset, mgr.pd.transform, false);
                device.add_view(device_view);

                foreach (var kp in device_view.dkp)
                {
                    device.key_points.Add(kp.key_name, kp.transform);
                }
                device.InitData(r_device_all);

                m_devices.Add(slot_name, device);

                //车轮处理
                if (r_device_all.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Wheel)
                {
                    device_wheels.TryGetValue($"{r_device_all.id},{r_device_all.sub_id}", out var r_device_wheel);
                    ctx.tractive_force_max = r_device_wheel.tractive_force_max;
                    ctx.feedback_0 = r_device_wheel.feedback_0;
                    ctx.feedback_1 = r_device_wheel.feedback_1;
                }

                //重量
                ctx.total_weight += r_device_all.weight;
                #endregion

                #region 初始化device
                m_caravan_slot_pos_dic.TryGetValue(slot_name, out var slot_pos);

                var angle = Vector2.SignedAngle(Vector2.right, dir);
                var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * (Vector2)slot_pos;

                var flag = dir.x >= 0 ? 1 : -1;
                device.position = pos + (Vector2)new_v * flag;
                device.InitPos();
                device.faction = WorldEnum.Faction.opposite;

                device.tick();
                #endregion
            }
        }


        public override void tick()
        {
            base.tick();

            car_mover.move(this);
            car_mover.calc_and_set_caravan_leap_rad(this);

            load_data_from_ctx();

            if (WorldContext.instance.is_need_reset)
            {
                foreach (var (_, device) in m_devices)
                {
                    if (device == null)
                        continue;

                    device.position -= new Vector2(WorldContext.instance.reset_dis, 0);
                    device.velocity = velocity;
                }
            }

            foreach (var (slot_name, device) in m_devices.Where(t => t.Value != null))
            {
                m_caravan_slot_pos_dic.TryGetValue(slot_name, out var slot_pos);
                if (slot_pos == null) continue;

                var angle = Vector2.SignedAngle(Vector2.right, dir);
                var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * (Vector2)slot_pos;

                var flag = dir.x >= 0 ? 1 : -1;
                device.position = pos + (Vector2)new_v * flag;
                device.velocity = velocity;

                device.tick();
            }
        }


        public override void fini()
        {
            base.fini();

            foreach (var (_, device) in m_devices)
            {
                device.remove_all_views();
            }
        }


        protected override float calc_view_scaleX()
        {
            return dir.x >= 0 ? 1 : -1;
        }


        protected override Dictionary<string, object> calc_anim_info()
        {
            var sub_desc = (AutoCodes.sub_monster_car)_sub_desc;
            AutoCodes.spine_caravans.TryGetValue($"{sub_desc.carbody},0,{bt.state}", out var r_spine);

            Dictionary<string, object> ret = new()
            {
                { "anim_name", r_spine.anim_name },
                { "loop", r_spine.loop }
            };

            return ret;
        }


        protected override Dmg_Data calc_dmg(Attack_Data attack_data)
        {
            //临时：暂时使用车体的防御力
            return Hurt_Calc_Helper.dmg_to_caravan(attack_data, $"{m_caravan._desc.id},{m_caravan._desc.rank}");
        }


        public bool try_get_caravan(out Caravan caravan)
        {
            caravan = m_caravan;

            return m_caravan != null;
        }


        public bool try_get_device(string slot_name, out Device device)
        {
            return m_devices.TryGetValue(slot_name, out device);
        }


        void load_data_from_ctx()
        {
            pos = ctx.caravan_pos;
            velocity = ctx.caravan_velocity;

            dir = ctx.caravan_dir;
        }
    }


    public class Enemy_Car_Context 
    {
        public Vector2 caravan_pos;
        public Vector2 caravan_velocity;
        public Vector2 caravan_acc;

        public float caravan_rad;
        public Vector2 caravan_dir => EX_Utility.convert_rad_to_dir(caravan_rad);

        public float caravan_rotation_theta;
        public float caravan_rotation_omega;
        public float caravan_rotation_beta;

        public WorldEnum.EN_caravan_status_acc caravan_status_acc;
        public WorldEnum.EN_caravan_status_liftoff caravan_status_liftoff;

        public float caravan_vx_stored;

        //public Dictionary<string, Bone> caravan_bones;
        //public Dictionary<string, Vector3> caravan_slots;

        public float caravan_move_delta_dis;

        public int caravan_hp;
        public int caravan_hp_max;

        public float total_weight;
        public float driving_lever;
        public int tractive_force_max;
        public float feedback_0;
        public float feedback_1;
    }
}

