using UnityEngine;
using World.Devices.Device_AI;


namespace World.Devices
{
    public class Shooter_Trebuchet : NewBasicShooter
    {
        // Const
        private const float SHOOT_DEG_OFFSET = 72F;

        // Fields
        override protected float shoot_deg_offset => SHOOT_DEG_OFFSET;

        // ===================================================================================

        private void trebuchet_rotate(string bone_name, float dir_x)
        {
            bones_direction[bone_name] = new Vector2(dir_x, 0.001f);
        }

        // Auto
        override protected void rotate_bone_to_target(string bone_name)
        {
            if (target_list.Count == 0)
                return;

            float target_dx = BattleUtility.get_target_colllider_pos(target_list[0]).x - position.x;
            trebuchet_rotate(bone_name, target_dx);
        }

        override protected bool can_auto_shoot()
        {
            // Base:
            // return can_shoot_check_cd && can_shoot_check_error_angle() && can_shoot_check_ammo && can_shoot_check_post_cast;
            return can_shoot_check_cd && can_shoot_check_enemy_pos() && can_shoot_check_ammo && can_shoot_check_post_cast;

            bool can_shoot_check_enemy_pos()
            {
                // 抛石机有射界限制，无法命中自己头顶的敌人。
                if (target_list.Count == 0)
                    return false;

                var t_pos_delta = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
                var abs_dx = Mathf.Abs(t_pos_delta.x);
                return Mathf.Atan2(t_pos_delta.y, abs_dx) * Mathf.Rad2Deg < SHOOT_DEG_OFFSET;
            }
        }

        // ----------------------------------------------------------------------------------------

        // Player Control
        override protected void Aiming()
        {
            float dir_x = InputController.instance.GetWorlMousePosition().x - position.x;
            trebuchet_rotate(BONE_FOR_ROTATE, dir_x);
        }
    }
}

