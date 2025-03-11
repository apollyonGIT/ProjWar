﻿using Foundations;
using System.Collections.Generic;
using World.Progresss;
using World.Widgets;

namespace World.Devices.NewDevice
{
    public class BasicModule
    {
        public bool has_worker { get; protected set; }

        public virtual void SetWorker(bool ret)
        {
            has_worker = ret;
        }

        public virtual void tick()
        {

        }

        /// <summary>
        /// 用于设置对应的模块外观贴图
        /// </summary>
        public virtual void SetModule()
        {

        }
    }

    public class ProgressModule : BasicModule
    {
        public ProgressEvent pe;

        public override void tick()
        {
            base.tick();
            if (has_worker)
            {
                if (pe != null)
                {
                    pe.current_value += 1;
                }
            }
        }
    }
    public class CaravanModule : BasicModule
    {
        private bool worker_draging;
        private bool need_draging_lever => Widget_DrivingLever_Context.instance.target_lever - WorldContext.instance.driving_lever > 0.05f;
        private bool can_end_draging => WorldContext.instance.driving_lever == 1f || Widget_DrivingLever_Context.instance.target_lever - WorldContext.instance.driving_lever < -0.025f;

        public override void tick()
        {
            base.tick();

            if (has_worker)
            {
                if (need_draging_lever)
                    worker_draging = true;

                if (worker_draging)
                {
                    Widget_DrivingLever_Context.instance.Drag_Lever(true, true, false);
                    if (can_end_draging)
                        worker_draging = false;
                }
            }
            else
            {
                worker_draging = false;
            }
        }
    }

    public class CaravanFixModule : BasicModule
    {
        public override void tick()
        {
            base.tick();

            if (has_worker)
            {
                float percent = (float)WorldContext.instance.caravan_hp / WorldContext.instance.caravan_hp_max;
                if (percent < 0.5f)
                {
                    Widget_Fix_Context.instance.TryToFixCaravan();
                    return;
                    //车都半血了 别管设备了
                }
                else
                {
                    Device d = null;
                    Mission.instance.try_get_mgr(Commons.Config.DeviceMgr_Name, out DeviceMgr dmgr);
                    foreach (var (_, device) in dmgr.slots_device)
                    {
                        if(percent >= (float)device.current_hp / device.desc.hp)
                        {
                            percent = (float)device.current_hp / device.desc.hp;
                            d = device;

                            if (percent == 0)       //都坏了后面的还比啥呢 赶紧修吧
                                break;
                        }
                    }
                      if(d!=null)
                        Widget_Fix_Context.instance.TryToFix(d);
                    else
                        Widget_Fix_Context.instance.TryToFixCaravan();
                }
            }
        }
    }

    public class CaravanPushModule :BasicModule
    {
        public override void tick()
        {
            base.tick();
            if (has_worker)
            {
                if (Widget_PushCar_Context.instance.AbleToPush_CheckCD && Widget_PushCar_Context.instance.AbleToPush())
                    Widget_PushCar_Context.instance.PushCaravan();
            }
        }
    }

    public class DeviceModule : BasicModule
    {
        public Device device;
        public List<DeviceBehaviour> db_list = new();

        public override void SetWorker(bool ret)
        {
            base.SetWorker(ret);

            foreach (DeviceBehaviour db in db_list)
            {
                db.is_auto = ret;
            }
        }
        public override void tick()
        {
            base.tick();

            if (has_worker)
            {
                //判断要不要修一下
            }

            foreach (var db in db_list)
            {
                db.tick();
            }
        }
        public override void SetModule()
        {
            base.SetModule();

            device.SetModule(this);
        }
    }

    public abstract class DeviceBehaviour
    {
        public DeviceModule module;
        public bool is_auto;
        public abstract void tick();
    }

    public class FireBehaviour : DeviceBehaviour
    {
        public override void tick()
        {
            if (is_auto)
                (module.device as IAttack).TryToAutoShoot();
        }
    }

    public class LoadBehaviour : DeviceBehaviour
    {
        public override void tick()
        {
            if (is_auto)
                (module.device as ILoad).TryToAutoLoad();
        }
    }

    public class RecycleBehaviour : DeviceBehaviour
    {
        public override void tick()
        {
            if (is_auto)
            {
                (module.device as IRecycle).TryToAutoRecycle();
            }
        }
    }
}
