using DG.Tweening;
using TMPro;

public static class Extension_TextMeshProUGUI
{
    public static Tweener DOFade(this TextMeshProUGUI tmp, float value, float duration)
    {
        return DOVirtual.Float(tmp.alpha, value, duration, (alpha) =>
        {
            tmp.alpha = alpha;
        }).SetTarget(tmp);
    }
}