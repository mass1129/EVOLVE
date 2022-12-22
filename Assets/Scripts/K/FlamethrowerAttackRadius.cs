using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class FlamethrowerAttackRadius : MonoBehaviour
{
    public delegate void AssultEnteredEvent(GM_Hunter assult);
    public delegate void AssultExitedEvent(GM_Hunter assult);

    public event AssultEnteredEvent OnAssultEnter;
    public event AssultEnteredEvent OnAssultExit;

    private List<GM_Hunter>AssultsInRadius = new List<GM_Hunter>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<GM_Hunter>(out GM_Hunter assult))
        {
            AssultsInRadius.Add(assult);
            OnAssultEnter?.Invoke(assult);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<GM_Hunter>(out GM_Hunter assult))
        {
            AssultsInRadius.Remove(assult);
            OnAssultExit?.Invoke(assult);
        }
    }

    private void OnDisable()
    {
        foreach (GM_Hunter assult in AssultsInRadius)
        {
            OnAssultExit?.Invoke(assult);
        }

        AssultsInRadius.Clear();
    }
}