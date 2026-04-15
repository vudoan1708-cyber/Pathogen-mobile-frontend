using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pathogen
{
    public class RecallButtonHandler : MonoBehaviour, IPointerDownHandler
    {
        public Action onPointerDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown?.Invoke();
        }
    }
}
