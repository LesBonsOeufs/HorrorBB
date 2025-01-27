using UnityEngine;

public abstract class InteractableWSign : Interactable
{
    [SerializeField] private Highlighter highlighter;
    [SerializeField] private CanvasGroup inputCanvasGroup;

    public override Interactor ActiveInteractor
    {
        get { return base.ActiveInteractor; }

        protected set
        {
            base.ActiveInteractor = value;

            inputCanvasGroup.alpha = ActiveInteractor ? 0.5f : 1f;
        }
    }

    public override void InteractorEnter()
    {
        highlighter.IsOn = true;
        inputCanvasGroup.alpha = 1f;
        inputCanvasGroup.gameObject.SetActive(true);
    }

    public override void InteractorExit()
    {
        highlighter.IsOn = false;
        inputCanvasGroup.alpha = 0f;
        inputCanvasGroup.gameObject.SetActive(false);
    }
}