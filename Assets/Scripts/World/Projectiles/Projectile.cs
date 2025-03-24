using AutoCodes;
using Commons;
using Foundations;
using System;
using UnityEngine;
using World.VFXs;
using static World.WorldEnum;

namespace World.Projectiles
{
    public enum MovementStatus
    {
        normal,
        in_object,
        in_ground,
    }


    public class Projectile : ITarget
    {
        public projectile desc;

        public Vector2 position;
        public Vector2 velocity;
        public Vector2 direction;

        public float mass;
        public int life_ticks;
        protected int life_ticks_init;
        public float radius;
        public Attack_Data attack_data;
        public MovementStatus movement_status = MovementStatus.normal;
        public Faction faction;

        public bool validate = true;

        public ITarget in_target = null;
        protected Vector2 pos_offset_in_object = Vector2.zero;
        protected Vector2 direction_in_object = Vector2.zero;

        public ITarget last_hit = null;

        protected float init_speed;  // For Caculating Actual Damage When Hit Target
        protected float rot_speed;
        protected float rot_propulsion;

        public Action<ITarget> hit_target_event;

        Vector2 ITarget.Position => position;

        Faction ITarget.Faction => faction;

        float ITarget.Mass => mass;

        bool ITarget.is_interactive => true;

        int ITarget.hp => validate ? 1 : 0;

        Vector2 ITarget.acc_attacher { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        Vector2 ITarget.direction => direction;

        Vector2 ITarget.velocity => velocity;

        private const float VELOCITY_IN_GROUND_REMAINING_PER_TICK = 0.2f;
        private const float VALIDATE_DISTANCE = 80F;                //当飞射物距离车距离超过一定值 判定为飞射物没有维护的价值


        public virtual void Init(Vector2 dir, Vector2 position,
            float rnd_angle_1, float rnd_angle_2, float speed, float init_speed,
            Faction f, projectile _desc, int life_ticks,Attack_Data atk_data, float rot_speed, float rot_propulsion,Action<ITarget> hit_event = null)
        {

            desc = _desc;
            mass = desc.mass;
            this.life_ticks = life_ticks;
            life_ticks_init = life_ticks;

            radius = desc.radius;
            faction = f;
            this.attack_data = atk_data;
            this.rot_speed = rot_speed;
            this.rot_propulsion = rot_propulsion;
            hit_target_event = hit_event;   

            var rnd = UnityEngine.Random.Range(rnd_angle_1, rnd_angle_2);
            direction = (Quaternion.AngleAxis(rnd, Vector3.forward) * dir).normalized;
            velocity = direction * speed + WorldContext.instance.caravan_velocity;

            this.init_speed = init_speed;

            this.position = position;
        }

        public virtual void tick()
        {
            switch (movement_status)
            {
                case MovementStatus.normal:
                    var acc_x = -velocity.x * desc.k_feedback / desc.mass;
                    var acc_y = Config.current.gravity;
                    Vector2 ammo_acc = new Vector2(acc_x, acc_y);
                    if (desc.propulsion_force > 0)
                    {
                        ammo_acc += direction.normalized * desc.propulsion_force / desc.mass;
                        rot_speed += rot_propulsion * Config.PHYSICS_TICK_DELTA_TIME;
                    }

                    velocity += ammo_acc * Config.PHYSICS_TICK_DELTA_TIME;
                    position += velocity * Config.PHYSICS_TICK_DELTA_TIME;  // Move
                    rotate(); // Rotate
                    HitGround();
                    HitEnemy();
                    break;
                case MovementStatus.in_object:
                    movement_in_object();
                    if (in_target.hp <= 0)
                        RemoveSelf();
                    break;
                case MovementStatus.in_ground:
                    if (--life_ticks <= 0)
                    {
                        if (desc.exploded_by_lifetime)
                        {
                            //生成逻辑爆炸
                            var targets = BattleUtility.select_all_target_in_circle(position, desc.explode_radius, faction);
                            foreach (var t in targets)
                            {
                                if (t != null && t.is_interactive)
                                {
                                    Attack_Data attack_data = new()
                                    {
                                        atk = desc.explode_dmg
                                    };

                                    t.hurt(attack_data);
                                    t.impact(impact_source_type.melee, position, BattleUtility.get_target_colllider_pos(t), desc.explode_ft);
                                }
                            }
                            //生成特效 仅外观
                            Mission.instance.try_get_mgr("VFX", out VFXMgr vmgr);
                            vmgr.AddVFX(desc.explode_vfx, Config.PHYSICS_TICKS_PER_SECOND, position);
                        }
                        RemoveSelf();
                    }
                    else if (velocity.magnitude > 0.1f)
                    {
                        velocity *= VELOCITY_IN_GROUND_REMAINING_PER_TICK;
                        position += velocity * Config.PHYSICS_TICK_DELTA_TIME;  // Move
                        rotate(); // Rotate
                    }
                    break;
            }

            var dis = (WorldContext.instance.caravan_pos - position).magnitude;
            if (dis > VALIDATE_DISTANCE)
                RemoveSelf();
        }

        public virtual void tick1()
        {
            // Nothing for now
        }

        protected void movement_status_change_to(MovementStatus expected_movement_status)
        {
            movement_status = expected_movement_status;
            switch (expected_movement_status)
            {
                case MovementStatus.in_object:
                    var ts = in_target as Devices.Device_AI.IShield;

                    // Countered by Shield
                    if (ts != null && ts.Hitting_Time)
                        ts.Rebound_Projectile(this, velocity);

                    // temp: Arrows should not stick in device or carbody
                    else if (!(in_target is Enemys.Enemy))
                        RemoveSelf();

                    break;
            }
        }

        protected virtual void rotate()
        {

        }

        protected virtual void movement_in_object()
        {

        }

        public virtual void HitEnemy()
        {

        }
        public virtual void HitGround()
        {

        }
        public virtual void RemoveSelf()
        {
            validate = false;
        }
        public virtual void ResetPos()
        {
            position -= new Vector2(WorldContext.instance.reset_dis, 0);
        }

        public virtual void ResetProjectile(Vector2 vel, Vector2 dir, Faction f, MovementStatus movement_status)
        {
            life_ticks = life_ticks_init;
            velocity = vel;
            direction = dir;
            faction = f;
            this.movement_status = movement_status;
        }

        #region ITargetFunc
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

        }
        float ITarget.distance(ITarget target)
        {
            return Vector2.Distance(position, target.Position);
        }
        void ITarget.tick_handle(System.Action<ITarget> outter_request, params object[] prms)
        {

        }
        #endregion
    }
}
