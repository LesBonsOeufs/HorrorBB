using UnityEngine;

public class LegController : MonoBehaviour
{
    public float maxTipWait = 0.7f;

    private bool readySwitchOrder = false;
    private bool stepOrder = true;

    public Leg[] Legs => _legs;
    [SerializeField] private Leg[] _legs;

    private void Update()
    {
        if (_legs.Length < 2) return;

        // If tip is not in current order but it's too far from target position, Switch the order
        for (int i = 0; i < _legs.Length; i++)
        {
            if (_legs[i].TipDistance > maxTipWait)
            {
                stepOrder = i % 2 == 0;
                break;
            }
        }

        // Ordering steps
        foreach (Leg leg in _legs)
        {
            leg.Movable = stepOrder;
            stepOrder = !stepOrder;
        }

        int lIndex = stepOrder ? 0 : 1;

        // If the opposite foot step completes, switch the order to make a new step
        if (readySwitchOrder && !_legs[lIndex].Animating)
        {
            stepOrder = !stepOrder;
            readySwitchOrder = false;
        }

        if (!readySwitchOrder && _legs[lIndex].Animating)
        {
            readySwitchOrder = true;
        }
    }
}
