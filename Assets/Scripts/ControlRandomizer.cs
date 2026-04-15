using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ControlRandomizer : MonoBehaviour
{
    private PlayerController player;
    public TextMeshProUGUI statusText;

    private enum PlayerAction { Jump, Dash, DropDown }
    private Dictionary<string, PlayerAction> currentMapping = new Dictionary<string, PlayerAction>();

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        ResetControls();
    }

    public void ResetControls()
    {
        currentMapping["Jump"] = PlayerAction.Jump;
        currentMapping["Dash"] = PlayerAction.Dash;
        currentMapping["DropDown"] = PlayerAction.DropDown;
        if (statusText) statusText.text = "Controls: Normal";
    }

    public void ShuffleControls()
    {
        currentMapping["Jump"] = PlayerAction.Jump;
        currentMapping["Dash"] = PlayerAction.Dash;
        currentMapping["DropDown"] = PlayerAction.DropDown;

        string[] keys = { "Jump", "Dash", "DropDown" };
        int firstIndex = Random.Range(0, 3);
        int secondIndex = (firstIndex + Random.Range(1, 3)) % 3;

        string keyA = keys[firstIndex];
        string keyB = keys[secondIndex];

        PlayerAction temp = currentMapping[keyA];
        currentMapping[keyA] = currentMapping[keyB];
        currentMapping[keyB] = temp;

        if (statusText) statusText.text = $"<color=red>SWAPPED!</color>\n{keyA} ↔ {keyB}";
    }

    public void OnMove(InputValue value) => player.MoveInput = value.Get<Vector2>();

    public void OnJump(InputValue value)
    {
        ExecuteAction(currentMapping["Jump"], value.isPressed);
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed) ExecuteAction(currentMapping["Dash"], true);
    }

    public void OnDropDown(InputValue value)
    {
        if (value.isPressed) ExecuteAction(currentMapping["DropDown"], true);
    }

    private void ExecuteAction(PlayerAction action, bool isPressed)
    {
        switch (action)
        {
            case PlayerAction.Jump:
                if (isPressed) player.TriggerJump();
                else player.TriggerStopJump();
                break;
            case PlayerAction.Dash:
                if (isPressed) player.TriggerDash();
                break;
            case PlayerAction.DropDown:
                if (isPressed) player.TriggerDropDown();
                break;
        }
    }
}