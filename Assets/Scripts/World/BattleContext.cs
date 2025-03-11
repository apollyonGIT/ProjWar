using Foundations;

namespace World
{
    public class BattleContext : Singleton<BattleContext>
    {
        public float melee_scale_factor = 1f;           //近战设备大小
        public float projectile_scale_factor = 1f;      //设备子弹大小
        public int projectile_repeate_amount = 0;       //设备重复射击增量
        public int projectile_salvo_amount = 0;         //设备散射增量
        public float enemy_scale_factor = 1f;           //怪物体型系数
        public float enemy_attack_factor = 1f;          //怪物攻击力系数
        public float enemy_def_factor = 1f;             //怪物防御力系数
        public float enemy_vel_factor = 1f;             //怪物速度系数
        public float enemy_mass_factor = 1f;            //怪物重量系数
        public float enemy_hp_factor = 1f;              //怪物hp系数
        public float drop_loot_delta = 0f;              //掉落物品增量    

        public void Init()
        {
            projectile_scale_factor = 1f;
            projectile_repeate_amount = 0;
            projectile_salvo_amount = 0;
            melee_scale_factor = 1f;

            enemy_scale_factor = 1f;
            enemy_attack_factor = 1f;
            enemy_def_factor = 1f;
            enemy_vel_factor = 1f;
            enemy_mass_factor = 1f;
            enemy_hp_factor = 1f;
            drop_loot_delta = 0f;
        }
    }
}
