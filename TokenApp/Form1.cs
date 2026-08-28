using CsvHelper;
using DataFactory.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model.Base;
using Model.TechCenter.JJT;
using Service.Struct;
using Service.TechCenter;
using System;
using Tool;

namespace TokenApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SHJUserInfo user = new SHJUserInfo();
                user.UserCode = "2010149446";
                user.RealName = "郑维尧";
                user.UserName = "ZHENG WEI YAO";
                user.Depart = "三航局-技术中心-总部-BIM中心-";
                user.Job = "研发工程师";
                user.Mobile = "13611963917";
                user.SoftwareID = 4;

                string GUID = Guid.NewGuid().ToString();

                var tokenData = new EncryptData<SHJUserInfo>()
                {
                    AGuid = GUID,
                    Data = user,
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ZGuid = GUID
                };

                string EncryptKey = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:Key");
                string EncryptIV = AppSettingUtils.GetSetting("AppSettings:TripleEncrypt:IV");

                // 使用TripleDES加密JSON数据
                string encryptedToken = CryptographyUtils.TripleDESEncrypt(JsonUtils.Serialize(tokenData), EncryptKey, EncryptIV);


                textBox1.Text = encryptedToken;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
