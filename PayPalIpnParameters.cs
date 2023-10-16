using DNNrocketAPI.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketEcommerceAPI.PayPal
{

    public class PayPalIpnParameters
    {

        public PayPalIpnParameters(SimplisityInfo paramInfo)
        {
            _postString = paramInfo.GetXmlProperty("genxml/requestcontent");
            _payment_status = ProviderUtils.GetParam(paramInfo, "payment_status");
            var strItem = ProviderUtils.GetParam(paramInfo, "item_name");
            if (GeneralUtils.IsNumeric(strItem)) _item_number = Convert.ToInt32(strItem);
            _custom = ProviderUtils.GetParam(paramInfo, "custom");

            _postString = "cmd=_notify-validate&" + _postString;
        }

        private string _postString = string.Empty;
        private string _payment_status = string.Empty;
        private string _txn_id = string.Empty;
        private string _receiver_email = string.Empty;
        private string _email = string.Empty;
        private string _custom = "";
        private int _item_number = -1;
        private decimal _mc_gross = -1;
        private decimal _shipping = -1;

        private decimal _tax = -1;

        public string PostString
        {
            get { return _postString; }
            set { _postString = value; }
        }

        public string payment_status
        {
            get { return _payment_status; }
            set { _payment_status = value; }
        }

        public string txn_id
        {
            get { return _txn_id; }
            set { _txn_id = value; }
        }

        public string receiver_email
        {
            get { return _receiver_email; }
            set { _receiver_email = value; }
        }

        public string email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string custom
        {
            get { return _custom; }
            set { _custom = value; }
        }

        public int item_number
        {
            get { return _item_number; }
            set { _item_number = value; }
        }

        public decimal mc_gross
        {
            get { return _mc_gross; }
            set { _mc_gross = value; }
        }

        public decimal shipping
        {
            get { return _shipping; }
            set { _shipping = value; }
        }

        public decimal tax
        {
            get { return _tax; }
            set { _tax = value; }
        }

        public int CartID
        {
            get { return _item_number; }
        }

        public bool IsValid
        {
            get
            {
                if (_payment_status != "Completed" & _payment_status != "Pending")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

    }
}
