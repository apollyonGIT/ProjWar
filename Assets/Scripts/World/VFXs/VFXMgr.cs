using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.VFXs
{
    public class VFX
    {
        public string path;
        public int duration;
        public Vector2 pos;

        public VFX(string path, int duration, Vector2 pos)
        {
            this.path = path;
            this.duration = duration;
            this.pos = pos;
        }
    }

    public interface IVFXMgrView : IModelView<VFXMgr>
    {
        void notify_add_vfx(VFX vfx);

        void notify_remove_vfx(VFX vfx);

        void notify_reset_vfx();
    }
    public class VFXMgr :Model<VFXMgr,IVFXMgrView>,IMgr
    {
        public List<VFX> vfx_list = new();

        private List<VFX> vfx_need_remove = new();

        string IMgr.name => m_mgr_name;

        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_prority;

        readonly int m_mgr_prority;

        public VFXMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_prority = priority;

            (this as IMgr).init(args);
        }

        public VFX AddVFX(string path,int duration,Vector2 pos)
        {
            var vfx = new VFX(path, duration, pos);
            vfx_list.Add(vfx);

            foreach (var view in views)
            {
                view.notify_add_vfx(vfx);
            }

            return vfx;
        }

        public void AddVFX(VFX vfx)
        {
            vfx_list.Add(vfx);

            foreach (var view in views)
            {
                view.notify_add_vfx(vfx);
            }
        }

        public void tick()
        {
            foreach (var vfx in vfx_list)
            {
                vfx.duration--;
                if (vfx.duration <= 0)
                {
                    vfx_need_remove.Add(vfx);
                }
            }

            foreach(var vfx in vfx_need_remove)
            {
                vfx_list.Remove(vfx);

                foreach(var view in views)
                {
                    view.notify_remove_vfx(vfx);
                }
            }

            vfx_need_remove.Clear();

            if (WorldContext.instance.is_need_reset)
            {
                foreach(var vfx in vfx_list)
                {
                    vfx.pos -= new Vector2(WorldContext.instance.reset_dis,0);
                }

                foreach(var view in views)
                {
                    view.notify_reset_vfx();
                }
            }

        }

        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Foundations.Tickers.Ticker.instance;
            ticker.remove_tick(m_mgr_name);
        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Foundations.Tickers.Ticker.instance;
            ticker.add_tick(m_mgr_prority, m_mgr_name, tick);

        }
    }
}
