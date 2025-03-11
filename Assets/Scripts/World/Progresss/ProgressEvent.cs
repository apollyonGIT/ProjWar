using AutoCodes;
using UnityEngine;
using World.Devices.NewDevice;

namespace World.Progresss
{
    public class ProgressEvent
    {
        public float trigger_progress;
        public bool notice_triggered = false;
        public bool event_triggered = false;

        public bool need_remove = false;

        public event_site record;

        public ProgressModule module;
        public int current_value = 0 ;
        public int max_value = 100;

        public Vector2 pos;

        public ProgressEvent(float v,event_site record,Vector2 pos)
        {
            trigger_progress = v;
            this.record = record;
            this.pos = pos;
        }

        public void Enter()
        {
            notice_triggered = true;
        }

        public void tick()
        {
            if (current_value >= max_value && event_triggered == false)
            {
                event_triggered = true;
                CompleteJob();
            }

            module.tick();
        }

        private void CompleteJob()
        {
            var dialog_path = record.dialogue_graph;
            Encounters.Dialogs.Encounter_Dialog.instance.init(this);
        }

        public void Exit()
        {
            need_remove = true;
            Encounters.Dialogs.Encounter_Dialog.instance.fini(null);
        }
    }
}
