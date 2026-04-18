using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlRandomizer : MonoBehaviour
{
    private PlayerController player;
    public TextMeshProUGUI statusText;

    private enum PlayerAction { Dash, DropDown, MoveLeft, MoveRight }
    private Dictionary<string, PlayerAction> currentMapping = new Dictionary<string, PlayerAction>();

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        ResetControls();
    }

    public void ResetControls()
    {
        currentMapping["Dash"] = PlayerAction.Dash;
        currentMapping["DropDown"] = PlayerAction.DropDown;
        currentMapping["MoveLeft"] = PlayerAction.MoveLeft;
        currentMapping["MoveRight"] = PlayerAction.MoveRight;
        if (statusText) statusText.text = "Controls: Normal";
    }

    public void ShuffleControls()
    {
        ResetControls();

        List<string> keys = new List<string>(currentMapping.Keys);

        int indexA = Random.Range(0, keys.Count);
        int indexB;
        do { indexB = Random.Range(0, keys.Count); } while (indexB == indexA);

        string keyA = keys[indexA];
        string keyB = keys[indexB];

        PlayerAction temp = currentMapping[keyA];
        currentMapping[keyA] = currentMapping[keyB];
        currentMapping[keyB] = temp;

        // Helper function to get the key name even for nested Move composites
        string bindingA = GetKeyName(keyA);
        string bindingB = GetKeyName(keyB);

        if (statusText)
        {
            statusText.text = $"<color=red>SWAPPED!\n{keyA} ({bindingA}) \u2194 {keyB} ({bindingB})</color>";
        }
    }

    private string GetKeyName(string actionName)
    {
        var inputActions = player.GetComponent<PlayerInput>().actions;
        var moveAction = inputActions["Move"];

        if (actionName == "MoveLeft" || actionName == "MoveRight")
        {
            // We look for "a" for Left and "d" for Right based on your screenshot
            string targetKey = actionName == "MoveLeft" ? "/a" : "/d";

            for (int i = 0; i < moveAction.bindings.Count; i++)
            {
                // This checks if the physical key path contains "a" or "d"
                if (moveAction.bindings[i].path.Contains(targetKey, System.StringComparison.OrdinalIgnoreCase))
                {
                    return moveAction.GetBindingDisplayString(i);
                }
            }
            return "Key Not Set";
        }
        else
        {
            return inputActions[actionName].GetBindingDisplayString();
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 raw = value.Get<Vector2>();
        Vector2 remapped = raw;

        if (raw.x < 0) remapped.x = GetRemappedAxis(currentMapping["MoveLeft"]);
        else if (raw.x > 0) remapped.x = GetRemappedAxis(currentMapping["MoveRight"]);

        player.MoveInput = remapped;
    }

    private float GetRemappedAxis(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.MoveLeft: return -1f;
            case PlayerAction.MoveRight: return 1f;
            case PlayerAction.Dash: player.TriggerDash(); return 0f;
            case PlayerAction.DropDown: player.TriggerDropDown(); return 0f;
            default: return 0f;
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed) player.TriggerJump();
        else player.TriggerStopJump();
    }

    public void OnDash(InputValue value) { if (value.isPressed) ExecuteAction(currentMapping["Dash"]); }
    public void OnDropDown(InputValue value) { if (value.isPressed) ExecuteAction(currentMapping["DropDown"]); }

    private void ExecuteAction(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.Dash: player.TriggerDash(); break;
            case PlayerAction.DropDown: player.TriggerDropDown(); break;
            case PlayerAction.MoveLeft: player.MoveInput = Vector2.left; break;
            case PlayerAction.MoveRight: player.MoveInput = Vector2.right; break;
        }
    }
}