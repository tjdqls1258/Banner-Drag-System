using UnityEngine.UI;

public class DemoBannerItem : ScrollBannerItem<DemoBannerData>
{
    public Text text;
    public override void InitBannerItem(int centerPageIndex, int Max, DemoBannerData data)
    {
        base.InitBannerItem(centerPageIndex, Max, data);
        if (data != null)
            text.text = data.demoID.ToString();
    }

    protected override void UpdateBanner()
    {
        if (m_currentData != null)
            text.text = m_currentData.demoID.ToString();
    }
}
