using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EmotionType
{
    VeryPositive, Positive, SlightlyPositive, Nuetral, SlightlyNegative, Negative, VeryNegative
}

public class SpeechManager : MonoBehaviour
{
    [SerializeField] GameObject speechBubble;
    [SerializeField] TMP_Text speechText;
    [SerializeField] Image speechIcon;

    [Header("Emotion Images")]
    [SerializeField] Sprite veryPositiveIcon;
    [SerializeField] Sprite positiveIcon;
    [SerializeField] Sprite slightlyPositiveIcon;
    [SerializeField] Sprite nuetralIcon;
    [SerializeField] Sprite slightlyNegativeIcon;
    [SerializeField] Sprite negativeIcon;
    [SerializeField] Sprite veryNegativeIcon;
    


    private float hideTimer;

    void Update()
    {
        if (hideTimer <= 0)
            return;

        hideTimer -= Time.deltaTime;

        if (hideTimer <= 0)
            speechBubble.SetActive(false);
    }
    

    public void Say(string speech, EmotionType emotion, float duration)
    {
        speechBubble.SetActive(true);
        speechText.text = speech;
        speechIcon.sprite = GetSpriteFromEmotionType(emotion);

        hideTimer = duration;
    }

    private Sprite GetSpriteFromEmotionType(EmotionType emotion)
    {
        switch(emotion)
        {
            case EmotionType.VeryPositive: return veryPositiveIcon;
            case EmotionType.Positive: return positiveIcon;
            case EmotionType.SlightlyPositive: return slightlyPositiveIcon;
            case EmotionType.Nuetral: return nuetralIcon;
            case EmotionType.SlightlyNegative: return slightlyNegativeIcon;
            case EmotionType.Negative: return negativeIcon;
            case EmotionType.VeryNegative: return veryNegativeIcon;
        }

        Debug.LogError("Forgot to identify an emotion");
        return positiveIcon;
    }

}
