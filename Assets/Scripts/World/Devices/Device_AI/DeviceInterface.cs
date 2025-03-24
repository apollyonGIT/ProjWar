using UnityEngine;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public interface IAttack
    {
        float Damage_Increase { get; set; }
        float Knockback_Increase { get; set; }
        int Attack_Interval { get; set; }
        int Current_Interval { get; set; }
        public void TryToAutoShoot();
    }

    public interface ILoad
    {
        int Max_Ammo { get; set; }
        int Current_Ammo { get; set; }
        float Reloading_Process { get; set; }
        float Reload_Speed { get; set; }
        public void TryToAutoLoad();
    }

    public interface IRecycle
    {
        int Recycle_Interval { get; set; }
        int Current_Recycle_Interval { get; set; }
        public void TryToAutoRecycle();
    }

    public interface IShield
    {
        float Toughness_Current { get; set; }
        float Toughness_Max { get; set; }
        float Toughness_Recover { get; set; }
        float Def_Range { get; set; }
        bool Is_Defending { get; }
        Vector2 Shield_Dir { get; }
        bool Hitting_Time { get; set; }
        void Rebound_Projectile(Projectile proj, Vector2 proj_v)
        {

        }
    }
}
