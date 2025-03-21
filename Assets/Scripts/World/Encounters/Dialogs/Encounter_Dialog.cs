﻿using Addrs;
using Foundations;
using Foundations.DialogGraphs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog : Singleton<Encounter_Dialog>
    {
        Dictionary<string, IDialogNode_Coder> coders_dic = new();
        public Dictionary<string, object> cache_dic = new();
        
        public bool is_open;
        public Encounter_Dialog_Window opening_window;

        public Progresss.ProgressEvent _event;

        //==================================================================================================

        public void init(Progresss.ProgressEvent _event)
        {
            if (is_open) return;
            is_open = true;

            this._event = _event;
            cache_dic = new();

            var path = _event.record.dialogue_graph;
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
            var uname = (string)args[0];

            var title = (string)args[1];
            var content = (string)args[2];
            var output_ac_list = (List<(string, Func<object>)>)args[3];
            var console = (string)args[4];

            Addressable_Utility.try_load_asset("Dialog_Window", out Encounter_Dialog_Window asset);
            var window = UnityEngine.Object.Instantiate(asset, WorldSceneRoot.instance.uiRoot.transform);
            window.init(uname, title, content, output_ac_list, console);

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

            if (coder is ParallelNode_Coder)
            {
                return;
            }
        }
    }
}

