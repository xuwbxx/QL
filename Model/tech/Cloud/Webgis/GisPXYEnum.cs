namespace Model.Tech.Cloud.Webgis
{
    /// <summary>
    /// 船类型
    /// </summary>
    public enum PXYShipTypeEnum
    {
        引渡船 = 50,
        搜救船 = 51,
        拖船 = 52,
        港口供应船 = 53,
        载有防污染装置和设备的船舶 = 54,
        执法艇 = 55,
        医疗船 = 58,
        捕捞 = 30,
        拖引 = 31,
        拖引并且船长大于200m或船宽大于25m = 32,
        疏浚或水下作业 = 33,
        潜水作业 = 34,
        参与军事行动 = 35,
        帆船航行 = 36,
        娱乐船 = 37,
        集装箱 = 100
    }

    public enum PXYShipTypeIDEnum
    {
        支腿船 = 1,
        起重船 = 2,
        交通船 = 3,
        驳船 = 4,
        打桩船 = 5,
        拖轮 = 6,
        搅拌船 = 7,
        锚艇 = 8,
        座底安装船 = 9,
        其他 = 10
    }

    /// <summary>
    /// 航行状态
    /// </summary>
    public enum PXYNaviStat
    {
        在航 = 0,
        锚泊 = 1,
        失控 = 2,
        操纵受限 = 3,
        吃水受限 = 4,
        靠泊 = 5,
        搁浅 = 6,
        捕捞作业 = 7,
        靠帆船提供动力 = 8,

    }

    public enum PXYShipUseType
    {
        三航租赁 = 1,
        三航自有 = 2,
        外部使用 = 3,
        分包自带 = 4
    }

    public enum PXYShipUseStatus
    {
        作业 = 1,
        闲置 = 0
    }

    public enum PXYWindStatus
    {
        完工 = 0,
        停工 = 1,
        在建 = 2,
        中标 = 3,
        水工工程 = 4,
        陆上工程 = 5,
        境外工程 = 6
    }

    public enum PXYShipWeight
    {
        无 = 0,
        小于500吨 = 1,
        大于500吨小于1000吨 = 2,
        大于1000吨 = 3
    }
}
