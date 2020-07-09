using DNNrocketAPI;
using DNNrocketAPI.Componants;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerce.PayPal
{
    public class PayPalData
    {
        private const string _entityTypeCode = "PAYPAL";
        private const string _tableName = "RocketEcommerce";
        private string _guidKey;
        private DNNrocketController _objCtrl;
        public PayPalData(string siteGuid, string systemKey)
        {
            _guidKey = siteGuid + "_" + systemKey;
            _objCtrl = new DNNrocketController();
            Info = _objCtrl.GetData(_guidKey, _entityTypeCode, DNNrocketUtils.GetCurrentCulture(), -1, true, _tableName);
            if (Info == null)
            {
                var portalId = PortalUtils.GetPortalIdBySiteKey(siteGuid);
                Info = new SimplisityInfo();
                Info.TypeCode = _entityTypeCode;
                Info.GUIDKey = _guidKey;
                Info.PortalId = portalId;
            }
        }
        public void Save(SimplisityInfo postInfo)
        {
            Info.XMLData = postInfo.XMLData;
            _objCtrl.SaveData(Info, _tableName);
        }
        public void Delete()
        {
            if (Info.ItemID > 0) _objCtrl.Delete(Info.ItemID);
        }

        public SimplisityInfo Info { get; set; }
        public string NotifyUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/notifyurl"); }
            set { Info.SetXmlProperty("genxml/textbox/notifyurl", value); }
        }
        public string ReturnUrl
        {
            get { return Info.GetXmlProperty("genxml/textbox/returnurl"); }
            set { Info.SetXmlProperty("genxml/textbox/returnurl", value); }
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



    }
}
