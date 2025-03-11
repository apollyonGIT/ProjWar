using Foundations;
using Foundations.Tickers;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using World;
using World.BackPack;
using World.Characters;
using World.Devices;

public class InputController : MonoBehaviourSingleton<InputController>
{
    public Control_1 c1;
    private const int input_priority = 0;
    private const string input_name = "input";
    public event Action tick_action;

    public event Action right_click_event, right_hold_event, right_release_event;
    public event Action left_click_event, left_hold_event, left_release_event;
    public event Action esc_event;

    public Device control_device;

    public Texture2D aim_cursor;


    protected override void on_init()
    {
        base.on_init();
        {
            c1 = new Control_1();
            c1.GamePlay.SelectCharacter_1.started += SelectCharacter_1;
            c1.GamePlay.SelectCharacter_2.started += SelectCharacter_2;
            c1.GamePlay.SelectCharacter_3.started += SelectCharacter_3;
            c1.GamePlay.SelectCharacter_4.started += SelectCharacter_4;
            c1.GamePlay.RightClick.started += RightClick;
            c1.GamePlay.RightClick.performed += RightHold;
            c1.GamePlay.RightClick.canceled += RightRelease;
            c1.GamePlay.LeftClick.started += LeftClick;
            c1.GamePlay.LeftClick.performed += LeftHold;
            c1.GamePlay.LeftClick.canceled += LeftRelease;
            c1.GamePlay.Esc.started += Esc;
            c1.GamePlay.Del.started += Del;
        }
    }
    private void Del(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
        
        if(bmgr.select_slot!= null)
        {
            if (bmgr.select_slot.loot == null)
                return;
            else
            {
                bmgr.RemoveLoot(bmgr.select_slot.loot);
                bmgr.CancelSelectSlot();
            }
        }
    }
    private void SelectCharacter_4(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
        if (cmgr.characters.Count < 4)
        {
            return;
        }

        cmgr.SelectCharacter(cmgr.characters[3]);
    }

    private void SelectCharacter_3(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
        if (cmgr.characters.Count < 3)
        {
            return;
        }

        cmgr.SelectCharacter(cmgr.characters[2]);
    }

    private void SelectCharacter_2(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
        if (cmgr.characters.Count < 2)
        {
            return;
        }

        cmgr.SelectCharacter(cmgr.characters[1]);
    }

    private void SelectCharacter_1(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
        if (cmgr.characters.Count < 1)
        {
            return;
        }

        cmgr.SelectCharacter(cmgr.characters[0]);
    }


    #region RightButton
    [HideInInspector]
    public bool holding_right;
    
    private void RightClick(InputAction.CallbackContext context)
    {
        right_click_event?.Invoke();

        Debug.Log("RightClick");
    }

    private void RightHold(InputAction.CallbackContext context)
    {
        holding_right = true;
        tick_action += right_hold_event;
        Debug.Log("StartRightHold");
    }

    private void RightRelease(InputAction.CallbackContext context)
    {
        if (holding_right)
        {
            holding_right = false;
            tick_action -= right_hold_event;
            Debug.Log("EndRightHold");
        }
        right_release_event?.Invoke();
        Debug.Log("RightRelease");
    }
    #endregion

    #region LeftButton
    [HideInInspector]
    public bool holding_left;

    private void LeftClick(InputAction.CallbackContext context)
    {
        left_click_event?.Invoke();
    }

    private void LeftHold(InputAction.CallbackContext context)
    {
        holding_left = true;
        tick_action += left_hold_event;
    }

    private void LeftRelease(InputAction.CallbackContext context)
    {
        if (holding_left)
        {
            holding_left = false;
            tick_action -= left_hold_event;
        }
        left_release_event?.Invoke();
    }
    #endregion

    private void Esc(InputAction.CallbackContext context)
    {
        EndDeviceControl();
    }

    public void SetDeviceControl(Device d)
    {
        if (control_device != null)
            EndDeviceControl();
        d.StartControl();
        control_device = d;

        Cursor.SetCursor(aim_cursor,Vector2.zero, CursorMode.Auto);
    }

    public void EndDeviceControl()
    {
        if (control_device == null)
            return;

        tick_action -= left_hold_event;
        tick_action -= right_hold_event;

        control_device.EndControl();
        control_device = null;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public Vector2 GetScreenMousePosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public Vector3 GetWorlMousePosition()
    {
        var mousePosition = GetScreenMousePosition();
        var pos = WorldSceneRoot.instance.mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10 - WorldSceneRoot.instance.mainCamera.transform.position.z));

        return pos;
    }

    public void tick()
    {
        tick_action?.Invoke();
    }

    public void OnEnable()
    {
        c1.Enable();
        Ticker.instance.add_tick(input_priority, input_name, tick);
    }
    private void OnDisable()
    {
        c1.Disable();
        Ticker.instance.remove_tick(input_name);
    }
}
