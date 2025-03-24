using System.Collections.Generic;
using UnityEngine;

namespace Commons
{
    [CreateAssetMenu(menuName = "GameConfig", fileName = "GameConfig")]
    public class Config : ScriptableObject
    {
        #region codes
        public static Config current
        {
            get
            {
                if (s_current == null)
                {
                    s_current = CreateInstance<Config>();
                }
                return s_current;
            }
        }
        private static Config s_current;
        private void OnEnable()
        {
            s_current = this;
        }
        private void OnDisable()
        {
            if (ReferenceEquals(s_current, this))
            {
                s_current = null;
            }
        }
        #endregion

        [Range(1, 256)]
        public int pixelPerUnit = 100;

        public float scaled_pixel_per_unit { get; set; } = 100;

        public Vector2Int desiredResolution = new Vector2Int(1920, 1080);
        public float desiredPerspectiveFOV = 60;

        [Header("YH Game Init 初始化")] 
        public string first_load_scene = "InitScene"; //首次加载scene

        #region const
        //帧率
        public const int PHYSICS_TICKS_PER_SECOND = 120;
        public const float PHYSICS_TICK_DELTA_TIME = 1f / PHYSICS_TICKS_PER_SECOND;
        #endregion

        #region internal_setting
        //Mgr tick优先级
        public const int CaravanMgr_Priority = 1;
        public const int DeviceMgr_Priority = 2;
        public const int EnemyMgr_Priority = 3;
        public const int New_EnemyMgr_Priority = 3;
        public const int EnvironmentMgr_Priority = 4;
        public const int MapMgr_Priority = 5;
        public const int CardMgr_Priority = 6;
        public const int ProjectileMgr_Priority = 7;
        public const int ProjectileMgr_Enemy_Priority = 8;
        public const int SisterMgr_Priority = 9;
        public const int BrotherMgr_Priority = 10;
        public const int Base_SlotMgr_Priority = 11;
        public const int Vfx_Priority = 99;

        public const int BuffMgr_Priority = 0;

        public const int CoreMgr_Priority = 20;

        //Mgr注册name(tick)
        public const string PlayerMgr_Name = "Player";
        public const string AdventureMgr_Name = "Adventure";
        public const string ProjectileMgr_Name = "ProjectileMgr";
        public const string GarageMgr_Name = "GarageMgr";
        public const string CaravanMgr_Name = "CaravanMgr";
        public const string DeviceMgr_Name = "DeviceMgr";
        public const string CoreMgr_Name = "CoreMgr";
        public const string EnemyMgr_Name = "EnemyMgr";
        public const string New_EnemyMgr_Name = "New_EnemyMgr";
        public const string BuffMgr_Name = "BuffMgr";
        public const string WorkMgr_Name = "WorkMgr";
        public const string CharacterMgr_Name = "CharacterMgr";
        public const string RelicMgr_Name = "RelicMgr";

        public const string EnvironmentMgr_Name = "enviroment";
        public const string CardMgr_Name = "CardMgr";
        public const string WareHouseMgr_Name = "WareHouse";
        public const string MapMgr_Name = "MapMgr";
        public const string ProjectileMgr_Enemy_Name = "ProjectileMgr_Enemy";
        public const string SisterMgr_Name = "SisterMgr";
        public const string BrotherMgr_Name = "BrotherMgr";
        public const string BgmMgr_Name = "BgmMgr";
        public const string HoldingDeviceMgr_Name = "HoldingDeviceMgr";
        public const string Vfx_Name = "Vfx";

        //Mgr注册name(普通)
        public const string RewardHouse_Name = "RewardHouse";
        public const string Elite_Battle_TimerMgr_Name = "Elite_Battle_TimerMgr";
        public const string SelectLevelMgr_Name = "SelectLevel";

        #endregion
    }

}
