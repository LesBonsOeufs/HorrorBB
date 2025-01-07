using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class Extension_RawImage
{
    public static void Fade(this RawImage rawImage, float value)
    {
        Color lColor = rawImage.color;
        lColor.a = value;
        rawImage.color = lColor;
    }

    public static Tweener DOFade(this RawImage rawImage, float endValue, float duration)
    {
        return DOVirtual.Float(rawImage.color.a, endValue, duration, rawImage.Fade)
                .SetTarget(rawImage);
    }
}
