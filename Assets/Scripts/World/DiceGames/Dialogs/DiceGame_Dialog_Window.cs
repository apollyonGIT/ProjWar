using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace World.DiceGames.Dialogs
{
    public class DiceGame_Dialog_Window : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI content;

        public DiceGame_Dialog_Window_Btn_Option btn_option_model;

        List<DiceGame_Dialog_Window_Btn_Option> m_btn_options = new();

        //==================================================================================================

        public void init(params object[] args)
        {
            title.text = (string)args[0];
            content.text = (string)args[1];
            var btn_option_ac_list = (List<(string, Func<object>)>)args[2];

            for (int i = 0; i < btn_option_ac_list.Count; i++)
            {
                var btn_option = Instantiate(btn_option_model, btn_option_model.transform.parent);
                btn_option.gameObject.SetActive(true);
                m_btn_options.Add(btn_option);
            }

            int option_index = 0;
            foreach (var (option_name, option_ac) in btn_option_ac_list)
            {
                var btn_info = m_btn_options[option_index];
                btn_info.btn_option_name.text = option_name;
                btn_info.btn_option.onClick.AddListener(
                    () => {
                        fini();
                        option_ac?.Invoke(); 
                    });

                option_index++;
            }

            DiceGame_Dialog.instance.opening_window = this;
        }


        public void fini()
        {
            DestroyImmediate(gameObject);

            DiceGame_Dialog.instance.opening_window = null;
        }
    }
}

