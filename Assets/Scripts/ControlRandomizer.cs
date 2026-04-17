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
        // Reset first so swaps are always from a clean state
        ResetControls();

        List<string> keys = new List<string>(currentMapping.Keys);

        // Pick 2 different random keys and swap only them
        int indexA = Random.Range(0, keys.Count);
        int indexB;
        do { indexB = Random.Range(0, keys.Count); } while (indexB == indexA);

        string keyA = keys[indexA];
        string keyB = keys[indexB];

        PlayerAction temp = currentMapping[keyA];
        currentMapping[keyA] = currentMapping[keyB];
        currentMapping[keyB] = temp;

        if (statusText) statusText.text = $"<color=red>SWAPPED!\n{keyA} \u2194 {keyB}</color>";
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