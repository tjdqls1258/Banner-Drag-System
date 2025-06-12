using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollBannerItem<T> : CacheObject
{//��ʾ����� ����

    //���� ����� ��ġ
    public enum BannerPosition
    {
        None = -1,
        FirstPos,
        Center,
        LastPos,
        Reset
    }

    //1,2,3�� ��ġ, �ӵ�, ������ �ε��� ( ��� ��ũ�ѿ��� �������ִ� �� )
    private Vector2 m_firstPos;
    private Vector2 m_lastPos;
    private Vector2 m_centerPos;
    private BannerPosition m_currentPosition;
    protected int m_pageIndex = 0;
    private float m_moveSpeed = 0.5f;

    private UnityAction<int> m_clickAction;
    private Func<int, T> getDataAction;
    [SerializeField] private Button m_btn;
    protected T m_currentData;
    private bool m_onSlide = false;

    public void Awake()
    {
        m_btn.onClick.AddListener(OnClickAction);
    }

    public virtual void InitBannerItem(int centerPageIndex, int Max, T data)
    {
        if (m_currentPosition == BannerPosition.Center)
            m_pageIndex = centerPageIndex;
        else if (m_currentPosition == BannerPosition.FirstPos)
            m_pageIndex = Max < centerPageIndex + 1 ? 0 : centerPageIndex + 1;
        else if (m_currentPosition == BannerPosition.LastPos)
            m_pageIndex = 1 > centerPageIndex ? Max : centerPageIndex - 1;

        if(getDataAction != null) 
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void SetPosition(int index, Vector2 first, Vector2 last, Vector2 center, float moveSpeed)
    {
        m_pageIndex = index;
        m_currentPosition = index == 0 ? BannerPosition.LastPos : index == 1 ? BannerPosition.Center : BannerPosition.FirstPos;
        m_firstPos = first;
        m_lastPos = last; 
        m_centerPos = center;
        m_moveSpeed = moveSpeed;
    }
    public void SetSpeed(float slideSpeed)
    {
        m_moveSpeed = slideSpeed;
    }
    private void MoveBanner(BannerPosition position, float moveSpeed)
    {
        switch (position)
        {
            case BannerPosition.FirstPos:
                MoveLeft(moveSpeed);
                break;
            case BannerPosition.LastPos:
                MoveRight(moveSpeed);
                break;
            case BannerPosition.Center:
                MoveCenter(moveSpeed);
                break;
        }
    }

    private void MoveRight(float moveSpeed) => DoMove(m_lastPos, moveSpeed);
    private void MoveCenter(float moveSpeed) => DoMove(m_centerPos, moveSpeed);

    private void MoveLeft(float moveSpeed) => DoMove(m_firstPos, moveSpeed);

    private void DoMove(Vector2 Pos, float moveSpeed)
    {
        gameObject.SetActive(true);
        MyRT.DOKill();
        MyRT.DOAnchorPos(Pos, moveSpeed).OnComplete((ScrollEndEvent)).OnKill(DoKillCon);
    }

    public void MoveNext()
    {
        if(CheckNext())
            MoveBanner(m_currentPosition, m_moveSpeed);
    }

    public void MoveBeforePage()
    {
        if(CheckBefore())
            MoveBanner(m_currentPosition, m_moveSpeed);
    }

    public virtual void UpdateIndex(int centerPageIndex, int Max, T data)
    {
        if (m_currentPosition == BannerPosition.Center)
            m_pageIndex = centerPageIndex;
        else if (m_currentPosition == BannerPosition.FirstPos)
            m_pageIndex = Max < centerPageIndex + 1 ? 0 : centerPageIndex + 1;
        else if (m_currentPosition == BannerPosition.LastPos)
            m_pageIndex = 1 > centerPageIndex ? Max : centerPageIndex-1;

        if (getDataAction != null)
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void SetAction(UnityAction<int> action, Func<int, T> data)
    {
        m_clickAction = action;
        getDataAction = data;
    }

    public void OnClickAction()
    {
        if(m_clickAction != null)
        {
            m_clickAction.Invoke(m_pageIndex);
        }
    }

    private void OnDestroy()
    {
        if(MyRT != null)
            MyRT.DOKill();
    }

    public void SlideNext(float speed)
    {
        m_onSlide = false;
        if (CheckNext())
            MoveBanner(m_currentPosition, speed);
    }

    public void SlidBeforePage(float speed)
    {
        m_onSlide = false; 
        if (CheckBefore())
            MoveBanner(m_currentPosition, speed);
    }

    public void OnSlide()
    {
        m_onSlide = true;
        MyRT.DOKill();

        if (getDataAction != null)
            m_currentData = getDataAction.Invoke(m_pageIndex);
        UpdateBanner();
    }

    public void MoveCurrent()
    {
        m_onSlide = false;
        gameObject.SetActive(m_currentPosition == BannerPosition.Center);
        MoveBanner(m_currentPosition, 0.1f);
    }

    protected virtual void ScrollEndEvent()
    {
        gameObject.SetActive(BannerPosition.Center == m_currentPosition);
    }

    protected virtual void UpdateBanner() { }

    //�������� �̵��ϴµ� Reset(���������� ��ũ�Ѹ���)�� ��� ù��° ��ġ�� �̵� �����ְ�
    //�߾����� ���� �������� ��� ��ʸ� ������Ʈ ���ݴϴ�.
    private bool CheckNext()
    {
        m_currentPosition = (BannerPosition)((int)m_currentPosition + 1);
        if (m_currentPosition == BannerPosition.Reset)
        {
            gameObject.SetActive(false);
            m_currentPosition = BannerPosition.FirstPos;
            MyRT.anchoredPosition = m_firstPos;
            return false;
        }
        if (m_currentPosition != BannerPosition.LastPos)
        {
            if (getDataAction != null)
                m_currentData = getDataAction.Invoke(m_pageIndex);
            UpdateBanner();
        }
        return true;
    }

    //�������� �̵��ϴµ� �ش� �������� None(ù��°���� �ڷ� ��ũ�Ѹ���) �� ��� ����Ʈ�� ������ ��ġ�� �̵������ְ�
    //�߾����� ���� �������� ��� ��ʸ� ������Ʈ ���ݴϴ�.
    private bool CheckBefore()
    {
        m_currentPosition = (BannerPosition)((int)m_currentPosition - 1);
        if (m_currentPosition == BannerPosition.None)
        {
            gameObject.SetActive(false);
            m_currentPosition = BannerPosition.LastPos;
            MyRT.anchoredPosition = m_lastPos;

            return false;
        }

        if (m_currentPosition != BannerPosition.FirstPos)
        {
            if (getDataAction != null)
                m_currentData = getDataAction.Invoke(m_pageIndex);
            UpdateBanner();
        }
        return true;
    }

    //DoTween�� �Ϸ�Ǳ� ���� Kill ���Ұ�� ��ġ�� ������ �����ݴϴ�.
    private void DoKillCon()
    {
        if (m_onSlide) return;

        switch (m_currentPosition)
        {
            case BannerPosition.FirstPos:
                MyRT.anchoredPosition = m_firstPos;
                break;
            case BannerPosition.Center:
                MyRT.anchoredPosition = m_centerPos;
                break;
            case BannerPosition.LastPos:
                MyRT.anchoredPosition = m_lastPos;
                break;
        }

    }

    //3���� ��ʸ� ���� �׷��ִ°� �ƴ� �߾ӿ� �ִ� ��ʸ� �׷��ݴϴ�.
    //�� ��ũ�Ѹ� �ɶ��� 3�� ���� �׷��ݴϴ�.
    public void BannerSetting(int index)
    {
        if ((BannerPosition)index == BannerPosition.Center)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
