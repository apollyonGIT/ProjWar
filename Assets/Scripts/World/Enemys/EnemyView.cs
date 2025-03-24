using Commons;
using Foundations.MVVM;
using Foundations.Tickers;
using Spine.Unity;
using System;
using System.Linq;
using UnityEngine;

namespace World.Enemys
{
    public class EnemyView : MonoBehaviour, IEnemyView
    {
        public SkeletonAnimation anim;
        Renderer m_anim_renderer;

        public Enemy owner;
        public Action tick1_outter;

        public GameObject pre_aim;
        public GameObject aim;

        //==================================================================================================

        void IModelView<Enemy>.attach(Enemy owner)
        {
            this.owner = owner;

            var anim_info = owner.anim_info;
            var anim_name = (string)anim_info["anim_name"];
            var anim_loop = (bool)anim_info["loop"];

            anim.state.SetAnimation(0, anim_name, anim_loop);

            owner.bones = anim.skeleton.Bones.ToDictionary(k => k.Data.Name, v => v);

            m_anim_renderer = anim.transform.GetComponent<MeshRenderer>();
        }


        void IModelView<Enemy>.detach(Enemy owner)
        {
            this.owner = null;

            DestroyImmediate(gameObject);
        }


        void IEnemyView.notify_on_tick1()
        {
            transform.localPosition = owner.view_pos;
            transform.localRotation = owner.view_rotation;
            transform.localScale = Vector3.one * owner.battle_ctx.enemy_scale_factor;

            anim.skeleton.ScaleX = owner.view_scaleX;
            anim.skeleton.ScaleY = owner.view_scaleY;

            var anim_info = owner.anim_info;
            var anim_name = (string)anim_info["anim_name"];
            var anim_loop = (bool)anim_info["loop"];

            if (anim.AnimationName != anim_name)
            {
                anim.state.SetAnimation(0, anim_name, anim_loop);
            }

            anim.Update(Config.PHYSICS_TICK_DELTA_TIME);

            tick1_outter?.Invoke();
        }


        void IEnemyView.notify_on_hurt()
        {
            var req_name = $"enemy_{owner.GUID}_hurt_fillPhase_stop";

            foreach (var req in Request_Helper.query_request(req_name))
            {
                req.interrupt();
            }

            if (m_anim_renderer == null) return;

            m_anim_renderer.material.SetFloat("_FillPhase", 1f);
            Request_Helper.delay_do(req_name, 15, (_) => 
            {
                if (m_anim_renderer == null) return;
                m_anim_renderer.material.SetFloat("_FillPhase", 0f); 
            });
        }

        void IEnemyView.notify_on_pre_aim(bool ret)
        {
            pre_aim.SetActive(ret);
        }

        void IEnemyView.notify_on_aim(bool ret)
        {
            aim.SetActive(ret);
        }
    }
}

