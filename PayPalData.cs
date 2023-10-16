using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketEcommerceAPI.Components;
using Simplisity;
using RocketPortal.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.PayPal
{
    public class PayPalData
    {
        private const string _entityTypeCode = "PAYPAL";
        private const string _tableName = "RocketEcommerceAPI";
        private const string _systemKey = "rocketecommerceapi";
        private string _guidKey;
        private DNNrocketController _objCtrl;
        public PayPalData(int portalid, string cultureCode)
        {
            PortalShop = new PortalShopLimpet(portalid, cultureCode);
            PortalData = new PortalLimpet(portalid);
            _guidKey = portalid + "_" + _systemKey + "_" + _entityTypeCode;
            _objCtrl = new DNNrocketController();
            Info = _objCtrl.GetData(_guidKey, _entityTypeCode, cultureCode, -1, true, _tableName);
            if (Info == null)
            {
                Info = new SimplisityInfo();
                Info.TypeCode = _entityTypeCode;
                Info.GUIDKey = _guidKey;
                Info.PortalId = portalid;
                Info.Lang = cultureCode;
            }
            if (Info.Lang == "") Info.Lang = cultureCode; // incase we have a missing language record.
        }
        public void Save(SimplisityInfo postInfo)
        {
            Info.XMLData = postInfo.XMLData;
            _objCtrl.SaveData(Info, _tableName);
            LogUtils.LogTracking("Save - UserId: " + UserUtils.GetCurrentUserId() + " " + postInfo.XMLData,"PayPal");
        }
        public void Delete()
        {
            if (Info.ItemID > 0) _objCtrl.Delete(Info.ItemID);
        }

        public SimplisityInfo Info { get; set; }
        public string NotifyUrl
        {
            get { return PortalData.EngineUrlWithProtocol.TrimEnd('/') + "/Desktopmodules/dnnrocket/api/rocket/actioncontent"; }
        }
        public string ReturnUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/returnurl"); }
            set { Info.SetXmlProperty("genxml/textbox/returnurl", value); }
        }
        public string ReturnCmd
        {
            get { return Info.GetXmlProperty("genxml/textbox/returncommand"); }
            set { Info.SetXmlProperty("genxml/textbox/returncommand", value); }
        }
        public string TestPostUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/testposturl"); }
            set { Info.SetXmlProperty("genxml/textbox/testposturl", value); }
        }
        public string LivePostUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/liveposturl"); }
            set { Info.SetXmlProperty("genxml/textbox/liveposturl", value); }
        }
        public string PayPalId
        {
            get { return Info.GetXmlProperty("genxml/textbox/paypalid"); }
            set { Info.SetXmlProperty("genxml/textbox/paypalid", value); }
        }
        public string CharSet
        {
            get { return Info.GetXmlProperty("genxml/textbox/charset"); }
            set { Info.SetXmlProperty("genxml/textbox/charset", value); }
        }
        public string CurrencyCode
        {
            get { return Info.GetXmlProperty("genxml/textbox/currencycode"); }
            set { Info.SetXmlProperty("genxml/textbox/currencycode", value); }
        }
        public string ExtraFields
        {
            get { return Info.GetXmlProperty("genxml/textbox/extrafields"); }
            set { Info.SetXmlProperty("genxml/textbox/extrafields", value); }
        }
        public bool PreProduction
        {
            get { return Info.GetXmlPropertyBool("genxml/checkbox/preproduction"); }
            set { Info.SetXmlProperty("genxml/checkbox/preproduction", value.ToString()); }
        }
        public bool DebugMode
        {
            get { return Info.GetXmlPropertyBool("genxml/checkbox/debugmode"); }
            set { Info.SetXmlProperty("genxml/checkbox/debugmode", value.ToString()); }
        }
        public bool Active
        {
            get { return Info.GetXmlPropertyBool("genxml/checkbox/active"); }
            set { Info.SetXmlProperty("genxml/checkbox/active", value.ToString()); }
        }

        public PortalShopLimpet PortalShop { get; set; }
        public PortalLimpet PortalData { get; set; }

        public string PayButtonText
        {
            get
            {
                var rtn = Info.GetXmlProperty("genxml/lang/genxml/textbox/paybuttontext");
                if (rtn == "") rtn = DNNrocketUtils.GetResourceString("/DesktopModules/DNNrocketModules/RocketEcommerce/App_LocalResources/", "RE.bankpayment", "Text", "");
                return rtn;
            }
            set { Info.SetXmlProperty("genxml/lang/genxml/textbox/paybuttontext", value); }
        }
        public string PayMsg
        {
            get { return Info.GetXmlProperty("genxml/lang/genxml/textbox/paymsg"); }
            set { Info.SetXmlProperty("genxml/lang/genxml/textbox/paymsg", value); }
        }

        public string PaymentProvKey { get { return "paypal"; } }

    }
}
