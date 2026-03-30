using UnityEngine;

/// <summary>
/// Reads the character selection from PlayerPrefs and swaps the player's
/// SpriteRenderer and Animator to match the chosen character.
///
/// Attach this to the Player GameObject in the MainIsland scene.
/// Assign the blue and red animator controllers and idle sprites in the Inspector.
/// </summary>
public class CharacterLoader : MonoBehaviour
{
    [Header("Blue Character")]
    [SerializeField] private RuntimeAnimatorController blueAnimator;
    [SerializeField] private Sprite blueIdleSprite;

    [Header("Red Character")]
    [SerializeField] private RuntimeAnimatorController redAnimator;
    [SerializeField] private Sprite redIdleSprite;

    private void Start()
    {
        int selected = PlayerPrefs.GetInt(CharacterSelectManager.CHARACTER_PREF_KEY, 0);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Animator animator = GetComponent<Animator>();

        if (selected == 0) // Blue
        {
            if (sr && blueIdleSprite) sr.sprite = blueIdleSprite;
            if (animator && blueAnimator) animator.runtimeAnimatorController = blueAnimator;
        }
        else if (selected == 1) // Red
        {
            if (sr && redIdleSprite) sr.sprite = redIdleSprite;
            if (animator && redAnimator) animator.runtimeAnimatorController = redAnimator;
        }
    }
}
