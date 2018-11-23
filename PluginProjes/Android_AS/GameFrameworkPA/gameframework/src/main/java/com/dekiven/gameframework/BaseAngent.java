package com.dekiven.gameframework;


//渠道基类
public class BaseAngent {

    /**
     * 请求支付
     * @param pid 商品id
     * @param externalData 额外数据
     */
    public void startPurchase(String pid, String externalData)
    {

    }

    /**
     * 返回渠道是否有自己的退出对话框
     * @return
     */
    public boolean hasAngentExitDialog()
    {
        return false;
    }
}
