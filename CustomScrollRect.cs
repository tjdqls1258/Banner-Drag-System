using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScrollRect : ScrollRect
{
    private UnityAction m_endDragAction;
    private UnityAction m_onDrageAction;

    public void AddOnDragAction(UnityAction action)
    {
        m_onDrageAction += action;
    }
    public void AddEndDragAction(UnityAction action)
    {
        m_endDragAction += action;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (m_endDragAction != null) m_endDragAction.Invoke();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        if (m_onDrageAction != null) m_onDrageAction.Invoke();
    }

}
