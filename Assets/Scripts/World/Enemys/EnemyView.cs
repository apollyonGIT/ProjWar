using Commons;
using Foundations.MVVM;
using Spine.Unity;
using System;
using System.Linq;
using UnityEngine;

namespace World.Enemys
{
    public class EnemyView : MonoBehaviour, IEnemyView
    {
        public SkeletonAnimation anim;

        public Enemy owner;
        public Action tick1_outter;

        //==================================================================================================

        void IModelView<Enemy>.attach(Enemy owner)
        {
            this.owner = owner;

            var anim_info = owner.anim_info;
            var anim_name = (string)anim_info["anim_name"];
            var anim_loop = (bool)anim_info["loop"];

            anim.state.SetAnimation(0, anim_name, anim_loop);

            owner.bones = anim.skeleton.Bones.ToDictionary(k => k.Data.Name, v => v);
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
    }
}

