﻿using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Characters
{
    public class CharacterMgrView : MonoBehaviour, ICharacterMgrView
    {
        CharacterMgr owner;
        public Transform content;
        public List<CharacterView> characters_view = new List<CharacterView>();
        public CharacterView prefab;

        //==================================================================================================

        void IModelView<CharacterMgr>.attach(CharacterMgr owner)
        {
            this.owner = owner;
        }


        void IModelView<CharacterMgr>.detach(CharacterMgr owner)
        {
            this.owner = null;
        }

        void ICharacterMgrView.notify_add_character(Character c)
        {
            var cv = Instantiate(prefab, content, false);
            cv.init(c);
            cv.gameObject.SetActive(true);
            characters_view.Add(cv);
        }

        void ICharacterMgrView.notify_on_tick()
        {
        }

        void ICharacterMgrView.notify_select_character(Character c)
        {
            foreach(var cv  in characters_view)
            {
                if(cv.character == c)
                {
                    cv.focus.gameObject.SetActive(true);
                }
            }
        }

        void ICharacterMgrView.notify_cancel_select_character(Character c)
        {
            foreach (var cv in characters_view)
            {
                if (cv.character == c)
                {
                    cv.focus.gameObject.SetActive(false);
                }
            }
        }

        void ICharacterMgrView.notify_character_is_working(Character c, bool working)
        {
            foreach (var cv in characters_view)
            {
                if (cv.character == c)
                {
                    if (working)
                    {
                        cv.working.gameObject.SetActive(true);
                        cv.rest.gameObject.SetActive(false);
                    }
                    else
                    {
                        cv.working.gameObject.SetActive(false);
                        cv.rest.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}

