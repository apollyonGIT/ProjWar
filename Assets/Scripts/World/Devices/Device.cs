﻿using AutoCodes;
using Foundations.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Devices.DeviceUpgrades;
using World.Devices.Device_AI;
using World.Helpers;
using static World.WorldEnum;
using World.Devices.DeviceEmergencies;
using World.Enemys;

namespace World.Devices
{
    public interface IDeviceView : IModelView<Device>
    {
        void init();
        void init_pos();
        void notify_change_anim(string anim_name, bool loop);
        void notify_on_tick();
        void notify_open_collider(string _name, Action<ITarget> t1 = null, Action<ITarget> t2 = null, Action<ITarget> t3 = null);
        void notify_close_collider(string _name);
        void notify_change_anim_speed(float f);
        void notify_hurt(int dmg);
        void notify_set_station(DeviceModule dm);
        void notify_disable();
        void notify_enable();
        void notify_player_oper(bool oper);
        void notify_add_emergency(DeviceEmergency de);

        void notify_remove_emergency(DeviceEmergency de);
    }


    public class Device : Model<Device, IDeviceView>, ITarget
    {
        public enum DeviceType
        {
            melee,
            shooter,
            other,
        };

        public DeviceType device_type = DeviceType.other;

        public Dictionary<string, Vector2> bones_direction = new();
        public List<AnimEvent> anim_events = new();
        public Dictionary<string, Transform> key_points = new();
        public List<DeviceModule> module_list = new();
        public device_all desc => m_desc;

        Vector2 ITarget.Position => position;

        Faction ITarget.Faction => faction;

        float ITarget.Mass => 10;

        bool ITarget.is_interactive => true;

        int ITarget.hp => current_hp;

        Vector2 ITarget.acc_attacher { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private const string BONE_FOR_ROTATE = "roll_control";
        Vector2 ITarget.direction
        {
            get
            {
                if (bones_direction.TryGetValue(BONE_FOR_ROTATE, out var v2))
                {
                    return v2;
                }
                return Vector2.right;
            }
        }

        Vector2 ITarget.velocity
        {
            get
            {
                if (faction == Faction.player)
                    return WorldContext.instance.caravan_velocity;
                else
                    return velocity;
            }
        }

        private device_all m_desc;
        public Faction faction = Faction.player;
        public Vector2 velocity;
        protected float rotate_speed;
        public int current_hp;
        public Vector2 position;
        public List<ITarget> target_list = new List<ITarget>();
        public bool is_validate = true;
        public bool player_oper { get; protected set; }

        public List<DeviceUpgrade> upgrades = new();
        public List<DeviceEmergency> emergrences = new();

        Action<ITarget> m_tick_handle;


        //==================================================================================================


        protected virtual void rotate_bone_to_target(string bone_name)
        {
            if (target_list.Count == 0)
                return;

            var target = target_list[0];
            Vector2 target_v2;
            target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], target_v2, rotate_speed);
        }

        protected virtual void rotate_bone_to_dir(string bone_name, Vector2 dir)
        {
            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], dir, rotate_speed);
        }

        /// <summary>
        /// 判断target是否仍在范围内
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected virtual bool target_can_be_selected(ITarget t)
        {
            var tp = BattleUtility.get_target_colllider_pos(t);
            var t_distance = (tp - position).magnitude;
            return t.hp > 0 && t_distance >= desc.basic_range.Item1 && t_distance <= desc.basic_range.Item2;
        }

        protected virtual bool try_get_target()
        {
            return false;           //不知道进行哪个范围的搜索，先进行一个false的返回
        }

        public virtual void tick()
        {
            m_tick_handle?.Invoke(this);
            m_tick_handle = null;

            foreach (var module in module_list)
            {
                module.tick();
            }

            foreach(var em in emergrences)
            {
                em.tick();
            }

            for(int i = emergrences.Count - 1; i >= 0; i--)
            {
                foreach(var view in views)
                {
                    view.notify_remove_emergency(emergrences[i]);
                }
                if (emergrences[i].removed)
                {
                    emergrences.RemoveAt(i);
                }
            }


            for(int i = target_list.Count - 1; i >= 0; i--)         //每帧确认目标队列中的目标是否仍然有效
            {
                if (!target_can_be_selected(target_list[i]))
                {
                    if(target_list[i] is Enemy enemy)
                    {
                        enemy.Select(false);
                    }

                    target_list.RemoveAt(i);
                }
            }

            foreach (var view in views)
            {
                view.notify_on_tick();
            }
        }

        private void add_upgrades()
        {
            var records = device_upgrades.records.Where(t => t.Value.id == m_desc.device_upgrade);

            foreach (var record in records)
            {
                var upgrade = new DeviceUpgrade(record.Value);
                upgrades.Add(upgrade);
            }
        }

        public virtual void InitData(device_all rc)
        {
            m_desc = rc;
            current_hp = rc.hp;
            rotate_speed = desc.rotate_speed.Item1;
            
            add_upgrades();

            foreach (var view in views)
            {
                view.init();
            }
        }

        public virtual void InitPos()
        {
            foreach(var view in views)
            {
                view.init_pos();
            }
        }

        public virtual int Hurt(int dmg)
        {
            if (!is_validate)       //坏了就别受伤了
                return current_hp;
            current_hp = Mathf.Clamp(current_hp - dmg, 0, desc.hp);
            foreach (var view in views)
            {
                view.notify_hurt(dmg);
            }

            if (current_hp == 0)
            {
                is_validate = false;

                foreach (var view in views)
                {
                    view.notify_disable();
                }       
            }

            return current_hp;
        }

        public void ChangeAnim(string anim_name, bool loop)
        {
            foreach (var ae in anim_events)
            {
                ae.triggered = false;
            }
            foreach (var view in views)
            {
                view.notify_change_anim(anim_name, loop);
            }
        }

        void ITarget.impact(params object[] prms)
        {

        }

        void ITarget.attach_data(params object[] prms)
        {

        }

        void ITarget.detach_data(params object[] prms)
        {

        }

        void ITarget.hurt(Attack_Data attack_data)
        {
            var dmg_data = Hurt_Calc_Helper.dmg_to_device(attack_data, $"{m_desc.id},{m_desc.sub_id}");

            Hurt(dmg_data.dmg);
            if(attack_data.ignite > 0)
            {
                IgniteDevice((int)attack_data.ignite);
            }
        }

        public void OpenCollider(string collider_name, Action<ITarget> enter_e = null, Action<ITarget> stay_e = null, Action<ITarget> exit_e = null)
        {
            foreach (var view in views)
            {
                view.notify_open_collider(collider_name, enter_e, stay_e, exit_e);
            }
        }

        public void CloseCollider(string collider_name)
        {
            foreach (var view in views)
            {
                view.notify_close_collider(collider_name);
            }
        }

        public void SetModule(DeviceModule dm)
        {
            foreach (var view in views)
            {
                view.notify_set_station(dm);
            }
        }

        public void IgniteDevice(int fire_value)
        {
            foreach(var emer in emergrences)
            {
                if(emer is DeviceFired df)
                {
                    df.AddFire(fire_value);
                    return;
                }
            }

            var fire = new DeviceFired(fire_value);
            emergrences.Add(fire);

            foreach(var view in views)
            {
                view.notify_add_emergency(fire);
            }
        }

        float ITarget.distance(ITarget target)
        {
            return Vector2.Distance(position, target.Position);
        }

        void ITarget.tick_handle(Action<ITarget> outter_request, params object[] prms)
        {
            m_tick_handle += outter_request;
        }

        public virtual void StartControl()
        {
            player_oper = true;

            foreach(var view in views)
            {
                view.notify_player_oper(player_oper);
            }
        }

        /// <summary>
        /// 好像没用了
        /// </summary>
        public virtual void OperateClick()
        {

        }

        public virtual void OperateDrag(Vector2 dir)
        {

        }

        public virtual void EndControl()
        {
            player_oper = false;

            foreach (var view in views)
            {
                view.notify_player_oper(player_oper);
            }
        }

        public void Fix(int delta)
        {
            current_hp += delta;
            if (current_hp >= desc.hp)
            {
                current_hp = desc.hp;
            }
            if (is_validate == false)
            {
                var h = UnityEngine.Random.Range(0, desc.hp);
                if (h >= 0 && h < current_hp)
                {
                    is_validate = true;

                    foreach (var view in views)
                    {
                        view.notify_enable();
                    }
                }
            }

        }
    }


    public class AnimEvent
    {
        public string anim_name;
        public float percent;
        public bool triggered;
        public Action<Device> anim_event;
    }
}





