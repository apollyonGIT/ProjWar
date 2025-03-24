using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;
using World.Devices.Device_AI;
using World.Environments;
using World.Helpers;

namespace World.Progresss
{
    public interface IProgressView : IModelView<Progress>
    {
        void notify_on_tick();
        /// <summary>
        /// 提醒玩家 已经接近奇遇了
        /// </summary>
        /// <param name="p"></param>
        /// <param name="b"></param>
        void notify_notice_encounter(float p, bool b);

        void notify_add_progress_event(ProgressEvent pe);

        void notify_remove_progress_event(ProgressEvent pe);
    }


    public class Progress : Model<Progress, IProgressView>
    {
        WorldContext ctx;

        //==================================================================================================

        public float total_progress => m_total_progress;
        public float m_total_progress;

        public float current_progress
        {
            get
            {
                return m_current_progress;
            }
            set
            {
                m_current_progress = Mathf.Clamp(value, 0f, m_total_progress);
            }
        }
        private float m_current_progress;

        public Dictionary<uint, plot_paragraph> plots = new();
        public (uint, float) current_order;

        public List<ProgressEvent> progress_events = new();

        public void tick()
        {
            if (ctx.is_need_reset)
            {
                foreach (var pe in progress_events)
                {
                    pe.pos -= new Vector2(ctx.reset_dis, 0);
                }
            }

            foreach (var view in views)
            {
                view.notify_on_tick();
            }

            for (int i = progress_events.Count - 1; i >= 0; i--)
            {
                progress_events[i].tick();

                if (progress_events[i].need_remove == true)
                {
                    foreach (var view in views)
                    {
                        view.notify_remove_progress_event(progress_events[i]);
                    }

                    progress_events.RemoveAt(i);
                }
            }

            //录入当前进度
            ctx.scene_remain_progress = m_total_progress - current_progress;
        }


        public void tick1()
        {

        }

        public void Init(uint map_id)
        {
            ctx = WorldContext.instance;

            foreach (var rc in plot_paragraphs.records)
            {
                if (rc.Value.map_id == map_id)
                {
                    plots.Add(rc.Value.site_order, rc.Value);

                    m_total_progress += rc.Value.distance_to_next;
                }
            }

            current_order = (plots[0].site_order, plots[0].distance_to_next);

            //剩余进度
            ctx.scene_remain_progress = m_total_progress;
        }

        public void Move(float dis)
        {
            m_current_progress += dis;
            foreach (var view in views)
            {
                view.notify_notice_encounter(0, false);
            }

            foreach (var pe in progress_events)
            {
                if (m_current_progress > pe.trigger_progress - Config.current.notice_length_1 && m_current_progress < pe.trigger_progress - Config.current.notice_length_2)
                {
                    foreach (var view in views)
                    {
                        view.notify_notice_encounter(pe.trigger_progress, true);
                    }
                    var x = pe.pos.x;
                    Road_Info_Helper.try_get_altitude(x, out var altitude);
                    pe.pos.y = altitude + 2;
                }

                if (m_current_progress > pe.trigger_progress - Config.current.trigger_length && m_current_progress < pe.trigger_progress + Config.current.trigger_length && pe.notice_triggered == false)
                {
                    pe.Enter();
                }
                else if (m_current_progress > pe.trigger_progress + Config.current.trigger_length)
                {
                    pe.Exit();
                }

            }

            current_order.Item2 -= dis;

            if (current_order.Item2 <= 0)
            {
                trigger_event();

                var new_order = current_order.Item1 + 1;

                if (!plots.TryGetValue(new_order, out var _plot)) return;
                current_order = (new_order, _plot.distance_to_next);
            }
        }

        private void trigger_event()
        {
            var es = plots[current_order.Item1].event_site;
            if (es == null || es.Count == 0)
                return;

            var index = Random.Range(0, es.Count);
            event_sites.TryGetValue(es[index].ToString(), out var event_site_record);

            Mission.instance.try_get_mgr(Config.EnvironmentMgr_Name, out EnvironmentMgr emgr);
            if (event_site_record.prefeb != null)
            {
                Debug.Log(event_site_record.prefeb);
                Addressable_Utility.try_load_asset(event_site_record.prefeb, out EncounterObjects objs);            //奇遇这个为纯外观 不带一丝逻辑
                if (objs != null)
                {
                    emgr.AddEncounterObj(objs, new Vector2(WorldContext.instance.caravan_pos.x + event_site_record.distance, 0));
                }
            }

            var pos = new Vector2(WorldContext.instance.caravan_pos.x + event_site_record.distance, 3);
            var pe = new ProgressEvent(current_progress + event_site_record.distance, event_site_record, pos);
            progress_events.Add(pe);
            ProgressModule pm = new ProgressModule()
            {
                pe = pe,
            };
            pe.module = pm;
            foreach (var view in views)
            {
                view.notify_add_progress_event(pe);
            }
        }
    }
}





