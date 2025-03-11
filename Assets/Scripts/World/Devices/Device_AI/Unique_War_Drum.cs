using AutoCodes;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using World.Devices.NewDevice;
using World.Helpers;
using static World.WorldEnum;

namespace World.Devices
{
    public class Unique_War_Drum : Device, IAttack
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "attack";
        private const string ANIM_BROKEN = "idle";
        private const string BONE_FOR_ROTATION = "control";

        private const float EXPLOSION_EFFECT_MIN = 0.5F;

        private const float ATK_EVENT_TIME_1 = 1f / 20f;
        private const float ATK_EVENT_TIME_2 = 11f / 20f;
        private const float ATK_HALF_WAY = 0.5f;

        private const int ATTACK_DELAY = 20;
        #endregion

        private enum Device_War_Drum_FSM
        {
            idle,
            attack,
            broken,
        }
        private Device_War_Drum_FSM fsm;

        private List<ITarget> targets;
        private Request request;

        private float attack_factor_current = 1f;
        public float attack_factor_expt = 1f;
        private float attack_range;

        public bool manual_left_ready => fsm == Device_War_Drum_FSM.idle;
        public bool manual_right_ready => fsm == Device_War_Drum_FSM.attack && attack_factor_current == 0;
        private bool auto_left_ready => !player_oper && has_target();
        private bool auto_right_ready => !player_oper && has_target() && attack_factor_current == 0;

        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }

        public override void InitData(device_all rc)
        {
            DeviceModule deviceModule = new DeviceModule();

            var fb = new FireBehaviour() { module = deviceModule };
            deviceModule.db_list.Add(fb);

            deviceModule.device = this;
            module_list.Add(deviceModule);

            base.InitData(rc);

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATION, Vector2.up);

            attack_range = desc.basic_range.Item2;

            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = ATK_EVENT_TIME_1,
                anim_event = (Device d) =>
                {
                    BoardCast_Helper.to_all_target(explosion);
                }
            });

            // 敲鼓一下之后，动画停止
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = ATK_HALF_WAY,
                anim_event = (Device d) =>
                {
                    change_factor_of_anim_speed(0);
                }
            });

            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = ATK_EVENT_TIME_2,
                anim_event = (Device d) =>
                {
                    BoardCast_Helper.to_all_target(explosion);
                }
            });

            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = 1,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_War_Drum_FSM.idle);
                }
            });
            #region 子函数 explosion
            void explosion(ITarget target)
            {
                WorldSceneRoot.instance.sr.SetActive(true);

                if (request != null)
                    request.interrupt();

                request = Request_Helper.delay_do("close_screen_shock", 30, (req) =>
                {
                    WorldSceneRoot.instance.sr.SetActive(false);
                });

                var dis = Vector2.Distance(target.Position, position);
                if (dis >= attack_range)  //爆炸范围
                    return;

                if (target.Faction != Faction.player)
                {

                    // Make Damage
                    var dis_coef = (EXPLOSION_EFFECT_MIN - 1) / attack_range * dis + 1;  // Min Damage == 50%

                    Attack_Data attack_data = new()
                    {
                        atk = Mathf.CeilToInt(desc.basic_damage * dis_coef * (1 + Damage_Increase))
                    };

                    target.hurt(attack_data);

                    // Make KnockBack
                    target.impact(impact_source_type.melee, position, target.Position, 19f * dis_coef * (1 + Knockback_Increase));
                }
            }
            #endregion
        }

        public override void tick()
        {
            if (!is_validate)       //坏了
                FSM_change_to(Device_War_Drum_FSM.broken);

            switch (fsm)
            {
                case Device_War_Drum_FSM.idle:
                    attack_factor_expt -= (attack_factor_expt - 1) * 0.01f;
                    break;
                case Device_War_Drum_FSM.attack:
                    attack_factor_expt -= (attack_factor_expt - 1) * 0.01f;
                    break;

                case Device_War_Drum_FSM.broken:
                    if (is_validate)
                        FSM_change_to(Device_War_Drum_FSM.idle);
                    break;

                default:
                    break;
            }
            base.tick();
        }

        private void FSM_change_to(Device_War_Drum_FSM target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_War_Drum_FSM.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_War_Drum_FSM.attack:
                    ChangeAnim(ANIM_ATTACK, false);
                    break;
                case Device_War_Drum_FSM.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    break;
                default:
                    break;
            }
        }

        private void change_factor_of_anim_speed(float f)
        {
            attack_factor_current = f;
            foreach (var view in views)
            {
                view.notify_change_anim_speed(attack_factor_current);
            }
        }


        private void Auto_tick()
        {
            if (player_oper)
                return;

            switch (fsm)
            {
                case Device_War_Drum_FSM.idle:
                    if (auto_left_ready)
                        if (Current_Interval <= 0)
                        {
                            Attack_1();
                            Current_Interval = ATTACK_DELAY;
                        }
                        else
                            Current_Interval--;
                    break;

                case Device_War_Drum_FSM.attack:
                    if (auto_right_ready)
                        if (Current_Interval <= 0)
                        {
                            Attack_2();
                            Current_Interval = ATTACK_DELAY;
                        }
                        else
                            Current_Interval--;
                    break;

                default:
                    break;
            }
        }

        private bool has_target()
        {
            targets = BattleUtility.select_all_target_in_circle(position, attack_range, faction);
            return targets.Count > 0;
        }


        void IAttack.TryToAutoShoot()
        {
            Auto_tick();
        }



        #region PlayerControl
        public override void StartControl()
        {
            InputController.instance.left_click_event += Attack_1;
            InputController.instance.right_click_event += Attack_2;
            base.StartControl();
        }

        public override void EndControl()
        {
            InputController.instance.left_click_event -= Attack_1;
            InputController.instance.right_click_event -= Attack_2;
            base.EndControl();
        }

        private void Attack_1()
        {
            if (manual_left_ready)
            {
                FSM_change_to(Device_War_Drum_FSM.attack);
                attack_factor_expt += 0.5f;
            }
            else
            {
                attack_factor_expt = 1f;
            }
        }

        private void Attack_2()
        {
            if (manual_right_ready)
            {
                change_factor_of_anim_speed(attack_factor_expt);
                attack_factor_expt += 0.5f;
            }
            else
            {
                attack_factor_expt = 1f;
            }
        }
        #endregion
    }
}
