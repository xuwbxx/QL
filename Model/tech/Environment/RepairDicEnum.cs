namespace Model.Tech.Environment
{
    public enum SoilRepairDicEnum
    {
        重金属浓度字典 = 1,
        土壤性质字典 = 2,
        修复剂组分列名字典 = 3,
        修复剂组分名称字典 = 4,
        修复效率字段 = 5
    }

    public enum DictionaryDataTypeEnum
    {
        数字型 = 1,
        字符型 = 2
    }

    public enum SoilRepairFileEnum
    {
        土壤修复剂关联文件 = 1,
        项目工程结题报告 = 2
    }

    public enum UserViewRightEnum
    {
        读写 = 1,
        只读 = 2
    }

    public enum UserDataRightEnum
    {
        一级 = 1,
        二级 = 2
    }

    public enum UserRoleEnum
    {
        管理员 = 1,
        工作人员 = 2,
        普通用户 = 3
    }

    public enum ResourceTypeNum
    {
        科技论文 = 1,
        中外专利 = 2,
        实验报告 = 3,
        中试报告 = 4,
        工程总结 = 5,
        法律法规 = 6,
        中外标准 = 7

    }

    public enum ResearcherNum
    {
        研发人员 = 1,
        录入人员 = 2
    }

}
