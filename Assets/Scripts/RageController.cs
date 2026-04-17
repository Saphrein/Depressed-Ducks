using UnityEngine;
using UnityEngine.UI;

public class RageController : MonoBehaviour
{
    public Image rageDisplayImage;
    [Tooltip("Drag all 14 sliced sprites here in order (0 to 13)")]
    public Sprite[] allFrames;

    [HideInInspector] public int currentRageLevel = 0; // Set by DamageSystem

    private float animationTimer;
    private int animationFrameOffset = 0;

    void Update()
    {
        // 0 to 4 are static fill frames (0%, 20%, 40%, 60%, 80%)
        if (currentRageLevel < 5)
        {
            if (allFrames.Length > currentRageLevel)
            {
                rageDisplayImage.sprite = allFrames[currentRageLevel];
            }
        }
        else
        {
            // RAGE IS FULL (100%)! Loop frames 5 through 13 for fire effect
            animationTimer += Time.deltaTime;
            if (animationTimer >= 0.08f) // Speed of fire flicker
            {
                animationTimer = 0;
                animationFrameOffset++;

                // There are 9 frames of fire (Index 5 to 13)
                if (animationFrameOffset > 8) animationFrameOffset = 0;

                rageDisplayImage.sprite = allFrames[5 + animationFrameOffset];
            }
        }
    }
}