using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public interface IEncounter_Dialog_CPN
    {
        string key_name { set; }

        void @do(Encounter_Dialog_Window_Btn_Option owner, string[] args);
    }
}

