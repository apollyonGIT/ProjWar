using Commons;
using Foundations.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.Progresss
{
    public class ProgressUiView : MonoBehaviour, IProgressView
    {
        Progress owner;

        public Slider progress_slider;
        public TextMeshProUGUI progress_text;

        public GameObject notice;
        public TextMeshProUGUI notice_text;

        //==================================================================================================

        void IModelView<Progress>.attach(Progress owner)
        {
            this.owner = owner;
        }


        void IModelView<Progress>.detach(Progress owner)
        {
            this.owner = null;
        }


        void IProgressView.notify_on_tick()
        {
            progress_slider.maxValue = owner.total_progress;
            progress_slider.value = owner.current_progress;

            var distance_remaining = (owner.total_progress - owner.current_progress) * 2;  // unit to meters = 2.0;
            progress_text.text = $"{distance_remaining:F0} m";
        }

        void IProgressView.notify_notice_encounter(float p, bool b)
        {
            if (!b)
            {
                notice.gameObject.SetActive(false);
                progress_text.color = Color.white;
                return;
            }

            if(p - owner.current_progress < Config.current.notice_length_1 && p -owner.current_progress > Config.current.notice_length_2)
            {
                notice.gameObject.SetActive(true);
                var distance_remaining_noticed = p - owner.current_progress;
                notice_text.text = $"{distance_remaining_noticed * 2:F1} m";     // unit to meters = 2.0;
                var t = (distance_remaining_noticed - Config.current.notice_length_1) / (Config.current.notice_length_2 - Config.current.notice_length_1);
                notice_text.fontSize = Mathf.Lerp(36f, 108, t);
                progress_text.color = Color.red;
            }
            
        }

        void IProgressView.notify_add_progress_event(ProgressEvent pe)
        {
            
        }

        void IProgressView.notify_remove_progress_event(ProgressEvent pe)
        {
            
        }
    }
}

