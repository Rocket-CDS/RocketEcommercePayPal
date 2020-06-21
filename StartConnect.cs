using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;

namespace RocketEcommerce.PayPal
{
    public class StartConnect : DNNrocketAPI.APInterface
    {
        private static SimplisityInfo _postInfo;
        private static SimplisityInfo _paramInfo;
        private static CommandSecurity _commandSecurity;
        private static DNNrocketInterface _rocketInterface;
        private static string _tableName;
        private static string _currentLang;
        private static SystemData _systemInfoData;
        private static Dictionary<string, string> _passSettings;

        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return ERROR if not matching commands.
            var rtnDic = new Dictionary<string, object>();

            paramCmd = paramCmd.ToLower();

            _rocketInterface = new DNNrocketInterface(interfaceInfo);
            _systemInfoData = new SystemData(systemInfo);

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

        public static String EditData()
        {
            var objCtrl = new DNNrocketController();
            var razorTempl = DNNrocketUtils.GetRazorTemplateData(_rocketInterface.DefaultTemplate, _rocketInterface.TemplateRelPath, _rocketInterface.DefaultTheme , _currentLang, _rocketInterface.ThemeVersion, _systemInfoData.DebugMode);
            var guidKey = PortalUtils.GetPortalId() + "." +  _systemInfoData.SystemKey + "." + _rocketInterface.DefaultTemplate;
            var info = objCtrl.GetData(guidKey, _rocketInterface.EntityTypeCode, _currentLang, -1,  false, _rocketInterface.DatabaseTable);
            var strOut = DNNrocketUtils.RazorDetail(razorTempl, info, _passSettings, new SessionParams(_paramInfo), _systemInfoData.DebugMode);
            return strOut;
        }
        public static void SaveData()
        {
            _passSettings.Add("saved", "true");
            var objCtrl = new DNNrocketController();
            var guidKey = PortalUtils.GetPortalId() + "." + _systemInfoData.SystemKey + "." + _rocketInterface.DefaultTemplate;
            var info = objCtrl.GetData(guidKey, _rocketInterface.EntityTypeCode, _currentLang, -1, false, _rocketInterface.DatabaseTable);
            info.XMLData = _postInfo.XMLData;
            objCtrl.SaveData(info, _rocketInterface.DatabaseTable);
        }
        public static void DeleteData()
        {
            var objCtrl = new DNNrocketController();
            var guidKey = PortalUtils.GetPortalId() + "." + _systemInfoData.SystemKey + "." + _rocketInterface.DefaultTemplate;
            var info = objCtrl.GetData(guidKey, _rocketInterface.EntityTypeCode, _currentLang, -1, false, _rocketInterface.DatabaseTable);
            objCtrl.Delete(info.ItemID);
        }

    }
}
