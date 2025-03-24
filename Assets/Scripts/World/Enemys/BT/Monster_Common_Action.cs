using Commons;
using System.Collections.Generic;
using UnityEngine;
using World.Caravans;
using World.Helpers;


namespace World.Enemys.BT
{
    public interface IEnemy_Can_Shoot
    {
        int Shoot_CD { get; set; }
        int Shoot_CD_Max { get; set; }
        bool Shoot_Finished { get; set; }
        float Projectile_Speed { get; set; }
        IEnemy_Can_Shoot I_Shoot { get; set; }

        sealed void Init_Shooting_Data(Enemy e)
        {
            AutoCodes.monsters.TryGetValue($"{e._desc.id}", out var monster_r);
            if (AutoCodes.fire_logics.TryGetValue($"{monster_r.fire_logic}", out var fire_logic_r))
            {
                Shoot_CD_Max = fire_logic_r.cd;
                Shoot_CD = Random.Range(0, Shoot_CD_Max);
                if (fire_logic_r.speed.Item2 == 0)
                    Projectile_Speed = fire_logic_r.speed.Item1;
                else
                    Projectile_Speed = (fire_logic_r.speed.Item1 + fire_logic_r.speed.Item2) * 0.5f;
            }
        }

        sealed void Monster_Shoot(Enemy actor, string muzzle_bone_name, Vector2 expected_pos)
        {
            actor.try_get_bone_pos(muzzle_bone_name, out Vector2 muzzle_pos);
            Vector2 shoot_dir;
            if (!Monster_Common_Action.Parabolic_Trajectory_Prediction(muzzle_pos, expected_pos, Projectile_Speed, out Vector2 target_dir))
                target_dir.y = Mathf.Max(target_dir.y, Mathf.Abs(target_dir.x));
            shoot_dir = target_dir;
            Enemy_Shoot_Helper.do_shoot(actor, muzzle_pos, shoot_dir);
            Shoot_Finished = true;
            Shoot_CD = Shoot_CD_Max;
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------

    public interface IEnemy_Can_Jump
    {
        public enum Enemy_Jump_Mode
        {
            Jump_To,
            Jump_Away,
            Jump_Around,
        }
        bool Jump_Finished { get; set; }
        int Jump_CD_Ticks { get; set; }
        Enemy_Jump_Mode Jump_Mode { get; set; }
        IEnemy_Can_Jump I_Jump { get; set; }
        float Jump_Speed_Min { get; set; }
        float Jump_Speed_Max { get; set; }
        float Jump_Y_Speed_Min { get; set; }
        float X_Distance_Mod_Coef { get; set; }

        sealed void Jump_By_Mode(Enemy actor, Vector2 target_pos, int cd)
        {
            Vector2 v2;
            actor.mover.move_type = EN_enemy_move_type.Hover;

            switch (Jump_Mode)
            {
                case Enemy_Jump_Mode.Jump_To:
                    v2 = get_jump_v2(target_pos - actor.pos, Jump_Speed_Max);
                    break;
                case Enemy_Jump_Mode.Jump_Away:
                    v2 = get_jump_v2(actor.pos - target_pos, Jump_Speed_Max);
                    break;
                case Enemy_Jump_Mode.Jump_Around:
                default:
                    v2 = get_jump_v2(Random.Range(0, 2) == 1 ? Vector2.right : Vector2.left, Jump_Speed_Min);
                    break;
            }
            actor.velocity.x += v2.x;
            actor.velocity.y = v2.y;

            Jump_CD_Ticks = cd;
            Jump_Finished = true;

            Vector2 get_jump_v2(Vector2 jump_towards, float speed_max)
            {
                var x_dis = jump_towards.x;
                Vector2 jump_velocity = jump_towards.normalized * Jump_Speed_Min;
                jump_velocity += WorldContext.instance.caravan_velocity;
                jump_velocity.x += Mathf.Min(15f, x_dis) * X_Distance_Mod_Coef;
                if (jump_velocity.magnitude > speed_max)
                    jump_velocity = jump_velocity.normalized * speed_max;
                jump_velocity.y = Mathf.Max(jump_velocity.y, Jump_Y_Speed_Min);

                return jump_velocity;
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------

    public interface IEnemy_Can_Be_Separated
    {
        bool Separate_All { get; set; }
        Dictionary<string, int> Sub_Monsters { get; set; }

        sealed void Forced_Separate(Enemy cell)
        {
            cell.hp_self = 0;
            Separate_All = true;
        }

        sealed void Die_Of_Being_Seperated(Enemy cell)
        {
            // 代替 Monster_Common_Action.Basic_Die(cell);
            if (Separate_All)
            {
                foreach (var sub in Sub_Monsters)
                    cell.mgr.pd.add_enemy_directly_req(0, (uint)sub.Value, get_init_pos(sub.Key), "Default");
                Monster_Common_Action.Set_Death_VFX_By_Index(cell, 4);
            }
            else
            {
                // 0 = generate mount ; 1 = generate rider
                var r = Random.Range(0, 2);
                switch (r)
                {
                    case 0:
                        cell.mgr.pd.add_enemy_directly_req(0, (uint)Sub_Monsters["Mount"], get_init_pos("Mount"), "Default");
                        break;
                    case 1:
                        cell.mgr.pd.add_enemy_directly_req(0, (uint)Sub_Monsters["Rider"], get_init_pos("Rider"), "Default");
                        break;
                    default:
                        break;
                }
                Monster_Common_Action.Set_Death_VFX_By_General_Rule(cell);
            }

            cell.fini();

            Vector2 get_init_pos(string key)
            {
                switch (key)
                {
                    case "Rider":
                        return cell.pos - cell.mgr.ctx.caravan_pos;
                    case "Mount":
                    default:
                        return (Vector2)cell.collider_pos - cell.mgr.ctx.caravan_pos;
                }
            }

        }
    }

    // ----------------------------------------------------------------------------------------------------------------------

    public interface IEnemy_Can_Flyaround
    {
        (float, float) Flyaround_Deg { get; }
        (float, float) Flyaround_Radius { get; }
        float Flyaround_Radius_Ratio { get; }  // Vertical_Radius divided by Horizontal_Radius
        Vector2 Flyaround_Relative_Following_Pos { get; set; }

        void Init_Or_Reset_Relative_Following_Pos()
        {
            // also executed when initialized 
            var radius = Random.Range(Flyaround_Radius.Item1, Flyaround_Radius.Item2);
            var deg = Random.Range(Flyaround_Deg.Item1, Flyaround_Deg.Item2);
            var rad = deg * Mathf.Deg2Rad;
            var angle_fix = Mathf.Lerp(Flyaround_Radius_Ratio, 1f, Mathf.Abs(radius - 90f) / 90f);
            Flyaround_Relative_Following_Pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius * angle_fix;
        }

        void Flyaround_Per_Tick(Enemy actor, Vector2 target_pos)
        {
            actor.position_expt = Flyaround_Relative_Following_Pos + target_pos;

            float d2 = Mathf.Pow((actor.pos.x - actor.position_expt.x), 2) + Mathf.Pow((actor.pos.y - actor.position_expt.y), 2);
            if (d2 < 1f)
                Init_Or_Reset_Relative_Following_Pos();
        }

        void Set_Expt_Speed_By_HP(Enemy cell, float expt_speed, float speed_max)
        {
            cell.speed_expt = Mathf.Clamp(expt_speed, 0, speed_max);

            var t_hp = 0.8f * (cell.hp_max - cell.hp) / cell.hp_max;
            var t_v = (speed_max - t_hp * cell.speed_expt) / speed_max;
            cell.speed_expt *= t_v;
        }
    }

    // ======================================================================================================================

    public abstract class Monster_Basic_BT
    {
        protected ushort Ticks_In_Current_State;
        protected ushort Ticks_Target_Has_Been_Locked_On;
        protected ITarget Target_Locked_On;

        protected void Lock_Target()
        {
            // can only be used when target is null
            SeekTarget_Helper.random_player_part(out var target);
            Target_Locked_On = target;
            Ticks_Target_Has_Been_Locked_On = 0;
        }

        protected Vector2? Get_Target_Pos()
        {
            if (Target_Locked_On == null)
                return null;

            // 车体的坐标逻辑位置紧贴地面，需要添加 Y_offset 以修正瞄准位置。 
            if (Target_Locked_On is Caravan)
                return Target_Locked_On.Position + Vector2.up;

            return Target_Locked_On.Position;
        }

        protected bool Check_State_Time(ushort ticks)
        {
            return Ticks_In_Current_State >= ticks;
        }

        virtual protected void basic_die(Enemy self)
        {
            // 当实现了 IEnemy_Can_Be_Separated 接口时，需要重写此函数
            Monster_Common_Action.Set_Death_VFX_By_General_Rule(self);
            self.fini();
        }

        public void get_killed(Enemy self)
        {
            basic_die(self);
        }
    }

    // ======================================================================================================================

    internal static class Monster_Common_Action
    {
        internal static bool Parabolic_Trajectory_Prediction(Vector2 start_pos, Vector2 target_pos, float init_speed, out Vector2 target_dir, bool high_trajectory = false)
        {
            var L = target_pos.x - start_pos.x;
            var H = target_pos.y - start_pos.y;
            var to_left = L < 0;
            var xor = to_left ^ high_trajectory;
            if (xor)
                L = -L;
            var discriminant = (H - Config.current.gravity * Mathf.Pow(L / init_speed, 2)) / Mathf.Sqrt(L * L + H * H);
            if (Mathf.Abs(discriminant) > 1)
            {
                target_dir = target_pos - start_pos;
                return false;
            }
            var M = Mathf.Atan2(-H, L);
            var angle = (Mathf.Asin(discriminant) - M) * 0.5f;
            target_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (xor)
                target_dir.x = -target_dir.x;
            return true;
        }

        internal static float Get_Damage_Mod_By_Distance(float distance, float radius, float min_dmg_pcts)
        {
            // Mostly For Explosion
            return (min_dmg_pcts - 1) * Mathf.Pow(distance / radius, 2) + 1;
            // Radius >= Distance
        }

        internal static void Sync_Pos_By_World_Pos_Reset(ref Vector2 pos_to_be_sync)
        {
            // 当某一位置在一段时间内相对于场景不变时，需要调用此函数
            if (WorldContext.instance.is_need_reset)
                pos_to_be_sync.x += WorldContext.instance.reset_dis;
        }

        #region 设置死亡特效
        internal static void Set_Death_VFX_By_Index(Enemy cell, int index)
        {
            cell.death_vfx_index = index;
        }

        internal static void Set_Death_VFX_By_General_Rule(Enemy cell)
        {
            Set_Death_VFX_By_Index(cell, Random.Range(0, 4));
        }
        #endregion


        internal static float Get_Speed_Expt_By_Clamp(float v_expt, float v_limit_max)
        {
            return Mathf.Clamp(v_expt, 0, v_limit_max);
        }

        internal static void Set_Speed_Expt_By_Remaining_HP(Enemy cell, float v_expt, float v_limit_max)
        {
            // 使怪物移速随血量降低而降低
            cell.speed_expt = Get_Speed_Expt_By_Clamp(v_expt, v_limit_max);
            var t_hp = 0.8f * (cell.hp_max - cell.hp) / cell.hp_max;
            var t_v = (v_limit_max - t_hp * cell.speed_expt) / v_limit_max;
            cell.speed_expt *= t_v;
        }

    }
}
