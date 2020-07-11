using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;

namespace RocketEcommerce.PayPal
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private CommandSecurity _commandSecurity;
        private DNNrocketInterface _rocketInterface;
        private string _currentLang;
        private Dictionary<string, string> _passSettings;
        private SystemData _systemData;

        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return ERROR if not matching commands.
            var rtnDic = new Dictionary<string, object>();

            paramCmd = paramCmd.ToLower();

            _systemData = new SystemData(systemInfo);
            _rocketInterface = new DNNrocketInterface(interfaceInfo);

            _postInfo = postInfo;
            _paramInfo = paramInfo;

            _currentLang = langRequired;
            if (_currentLang == "") _currentLang = DNNrocketUtils.GetCurrentCulture();

            _commandSecurity = new CommandSecurity(-1, -1, _rocketInterface);
            _commandSecurity.AddCommand("paypal_edit", true);
            _commandSecurity.AddCommand("paypal_save", true);
            _commandSecurity.AddCommand("paypal_delete", true);

            if (!_commandSecurity.HasSecurityAccess(paramCmd))
            {
                strOut = UserUtils.LoginForm(systemInfo, postInfo, _rocketInterface.InterfaceKey, UserUtils.GetCurrentUserId());
            }
            else
            {
                switch (paramCmd)
                {
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
            }

            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);
            return rtnDic;
        }

        public String EditData()
        {
            var paypalData = new PayPalData(PortalUtils.SiteGuid());

            var razorTempl = DNNrocketUtils.GetRazorTemplateData(_rocketInterface.DefaultTemplate, _rocketInterface.TemplateRelPath, _rocketInterface.DefaultTheme , _currentLang, _rocketInterface.ThemeVersion, true);
            var strOut = DNNrocketUtils.RazorDetail(razorTempl, paypalData.Info, _passSettings, new SessionParams(_paramInfo), true);
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
