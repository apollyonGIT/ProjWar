using UnityEngine;
using UnityEngine.UI;
using World.Helpers;

namespace World.Progresss
{
    public class ProgressEventView : MonoBehaviour
    {
        private const float FIXED_TRANSFORM_Z = 9.9F;

        public ProgressEvent pe;
        public Slider progress_slider;
        public Image icon;

        public ProgressStationView station;

        public void Init(ProgressEvent pe)
        {
            var pos = pe.pos;
            this.pe = pe;
            transform.position = new Vector3(pos.x, pos.y, FIXED_TRANSFORM_Z);
            station.Init(pe.module);
            progress_slider.maxValue = pe.max_value;
        }

        public void Destroy()
        {
            //清空module
            Character_Module_Helper.EmptyModule(station.module);
            Destroy(gameObject);
        }

        public void tick()
        {
            var pos = pe.pos;
            progress_slider.value = pe.current_value;
            transform.position = new Vector3(pos.x, pos.y, FIXED_TRANSFORM_Z);
            Vector3 delta = transform.position - (Vector3)WorldContext.instance.caravan_pos;
            delta.y = 0;
            delta.z += 2;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, delta.normalized);

            station.tick();
        }

        public void AddValue()
        {
            pe.current_value = pe.max_value;
        }
    }
}
