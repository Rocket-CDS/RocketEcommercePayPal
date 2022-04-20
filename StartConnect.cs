﻿using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketEcommerce.Components;
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
        private const string _systemkey = "rocketecommerce";
        private SessionParams _sessionParams;

        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return ERROR if not matching commands.
            var rtnDic = new Dictionary<string, object>();

            paramCmd = paramCmd.ToLower();

            _systemData = new SystemLimpet(_systemkey);
            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(paramInfo);
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            _securityData = new SecurityLimpet(PortalUtils.GetCurrentPortalId(), _systemData.SystemKey, _rocketInterface, -1, -1);
            _securityData.AddCommand("paypal_edit", true);
            _securityData.AddCommand("paypal_save", true);
            _securityData.AddCommand("paypal_delete", true);

            paramCmd = _securityData.HasSecurityAccess(paramCmd, "paypal_login");

            switch (paramCmd)
            {
                case "paypal_login":
                    strOut = LocalUtils.ReloadPage(PortalUtils.GetPortalId());
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
            var paypalData = new PayPalData(PortalUtils.GetPortalId(), _sessionParams.CultureCodeEdit);
            var appThemeSystem = new AppThemeSystemLimpet("RocketEcommercePayPal");
            var razorTempl = appThemeSystem.GetTemplate("settings.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, paypalData.Info, null, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public void SaveData()
        {
            var paypalData = new PayPalData(PortalUtils.GetPortalId(), _sessionParams.CultureCodeEdit);
            paypalData.Save(_postInfo);
        }
        public void DeleteData()
        {
            var paypalData = new PayPalData(PortalUtils.GetPortalId(), _sessionParams.CultureCodeEdit);
            paypalData.Delete();
        }

    }
}
