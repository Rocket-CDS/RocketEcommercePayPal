using DNNrocketAPI;
using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;

namespace RocketEcommerce.PayPal
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private SecurityLimpet _securityData;
        private RocketInterface _rocketInterface;
        private string _currentLang;
        private Dictionary<string, string> _passSettings;
        private SystemLimpet _systemData;

        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return ERROR if not matching commands.
            var rtnDic = new Dictionary<string, object>();

            paramCmd = paramCmd.ToLower();

            _systemData = new SystemLimpet(systemInfo);
            _rocketInterface = new RocketInterface(interfaceInfo);

            _postInfo = postInfo;
            _paramInfo = paramInfo;

            _currentLang = langRequired;
            if (_currentLang == "") _currentLang = DNNrocketUtils.GetCurrentCulture();

            _securityData = new SecurityLimpet(PortalUtils.GetCurrentPortalId(), _systemData.SystemKey, _rocketInterface, -1, -1);
            _securityData.AddCommand("paypal_edit", true);
            _securityData.AddCommand("paypal_save", true);
            _securityData.AddCommand("paypal_delete", true);

            paramCmd = _securityData.HasSecurityAccess(paramCmd, "paypal_login");

            switch (paramCmd)
            {
                case "paypal_login":
                    strOut = UserUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, UserUtils.GetCurrentUserId());
                    break;
                case "paypal_edit":
                    strOut = EditData();
                    break;
                case "paypal_save":
                    SaveData();
                    strOut = EditData();
                    break;
                case "paypal_delete":
                    DeleteData();
                    strOut = EditData();
                    break;
            }

            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);
            return rtnDic;
        }

        public String EditData()
        {
            var paypalData = new PayPalData(PortalUtils.SiteGuid());

            var razorTempl = RenderRazorUtils.GetRazorTemplateData(_rocketInterface.DefaultTemplate, _rocketInterface.TemplateRelPath, _rocketInterface.DefaultTheme , _currentLang, _rocketInterface.ThemeVersion, true);
            var strOut = RenderRazorUtils.RazorDetail(razorTempl, paypalData.Info, _passSettings, new SessionParams(_paramInfo), true);
            return strOut;
        }
        public void SaveData()
        {
            var paypalData = new PayPalData(PortalUtils.SiteGuid());
            paypalData.Save(_postInfo);
        }
        public void DeleteData()
        {
            var paypalData = new PayPalData(PortalUtils.SiteGuid());
            paypalData.Delete();
        }

    }
}
