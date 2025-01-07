using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SwitchToggle : MonoBehaviour
{
	[SerializeField] private RectTransform uiHandleRectTransform;
	[SerializeField] private Color backgroundActiveColor;
	[SerializeField] private Color handleActiveColor;

	private Image backgroundImage, handleImage;
	private Color backgroundDefaultColor, handleDefaultColor;
	private Toggle toggle;
	private Vector2 handlePosition;

	private void Awake()
	{
		toggle = GetComponent<Toggle>();

		handlePosition = uiHandleRectTransform.anchoredPosition;

		backgroundImage = uiHandleRectTransform.parent.GetComponent<Image>();
		handleImage = uiHandleRectTransform.GetComponent<Image>();

		backgroundDefaultColor = backgroundImage.color;
		handleDefaultColor = handleImage.color;

		toggle.onValueChanged.AddListener(OnSwitch);

		if (toggle.isOn)
			OnSwitch(true);
	}

	private void OnSwitch(bool on)
	{
		//uiHandleRectTransform.anchoredPosition = on ? handlePosition * -1 : handlePosition ; // no anim
		uiHandleRectTransform.DOAnchorPos(on ? handlePosition * -1 : handlePosition, .4f).SetEase(Ease.InOutBack);

		//backgroundImage.color = on ? backgroundActiveColor : backgroundDefaultColor ; // no anim
		backgroundImage.DOColor(on ? backgroundActiveColor : backgroundDefaultColor, .6f);

		//handleImage.color = on ? handleActiveColor : handleDefaultColor ; // no anim
		handleImage.DOColor(on ? handleActiveColor : handleDefaultColor, .4f);
	}

	private void OnDestroy()
	{
		toggle.onValueChanged.RemoveListener(OnSwitch);
	}
}
