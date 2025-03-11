using Addrs;
using Foundations;
using Foundations.DialogGraphs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.DiceGames.Dialogs
{
    public class DiceGame_Dialog : Singleton<DiceGame_Dialog>
    {
        Dictionary<string, IDialogNode_Coder> coders_dic = new();
        
        public bool is_open;
        public DiceGame_Dialog_Window opening_window;

        //==================================================================================================

        public void init(string path)
        {
            if (is_open) return;
            is_open = true;

            Addressable_Utility.try_load_asset(path, out DialogGraphAsset asset);
            DialogGraph_Utility.read_asset(asset, out coders_dic, out var entry_key);

            coders_dic.TryGetValue(entry_key, out IDialogNode_Coder first_coder);

            foreach (var (_, coder) in coders_dic)
            {
                load_input(coder);
            }

            first_coder.do_input();
        }


        public object fini(object[] args)
        {
            is_open = false;
            return null;
        }


        public object open_window(object[] args)
        {
            var title = (string)args[0];
            var content = (string)args[1];
            var output_ac_list = (List<(string, Func<object>)>)args[2];

            Addressable_Utility.try_load_asset("Dialog_Window", out DiceGame_Dialog_Window asset);
            var window = UnityEngine.Object.Instantiate(asset, WorldSceneRoot.instance.uiRoot.transform);
            window.init(title, content, output_ac_list);

            //规则：打开对话时，暂停游戏
            Time.timeScale = 0;

            return true;
        }


        void load_input(IDialogNode_Coder coder)
        {
            if (coder is DialogWindowNode_Coder)
            {
                coder.input = open_window;
                return;
            }

            if (coder is DestroyNode_Coder)
            {
                coder.input = (_) => {
                    //规则：关闭对话时，恢复游戏
                    Time.timeScale = 1; 

                    Debug.Log("对话结束!"); 
                    return null; 
                };

                return;
            }

            if (coder is JumpNode_Coder _coder)
            {
                _coder.coders = coders_dic;
                return;
            }
        }
    }
}

