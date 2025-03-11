﻿using Foundations;
using UnityEngine;

namespace World.Projectiles
{
    public class ProjectileView : MonoBehaviour
    {
        public Projectile owner;
        public TrailRenderer trail;

        public void tick()
        {
            transform.localPosition = owner.position;
            transform.localRotation = EX_Utility.look_rotation_from_left(owner.direction);
        }

        public void Init(Projectile p)
        {
            owner = p;
            transform.localScale *= BattleContext.instance.projectile_scale_factor;
        }

        public void ResetPos()
        {
            if (trail != null)
            {

                Vector3[] trail_positions = new Vector3[trail.positionCount];
                trail.GetPositions(trail_positions);

                for (int i = 0; i < trail_positions.Length; i++)
                {
                    trail_positions[i] -= new Vector3(WorldContext.instance.reset_dis, 0);
                }

                transform.localPosition = new Vector3(owner.position.x, owner.position.y, 10f);
                transform.localRotation = EX_Utility.look_rotation_from_left(owner.direction);

                trail.Clear();
                trail.SetPositions(trail_positions);
            }
        }
    }
}

