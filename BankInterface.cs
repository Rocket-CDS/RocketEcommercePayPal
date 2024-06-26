﻿using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RocketEcommerceAPI;
using DNNrocketAPI.Components;
using RocketEcommerceAPI.Interfaces;
using RocketEcommerceAPI.Components;
using System.Globalization;

namespace RocketEcommerceAPI.PayPal
{
    public class BankInterface : PaymentInterface
    {
        public override bool Active()
        {
            var systemData = new SystemLimpet("rocketecommerceapi");
            var rocketInterface = systemData.GetInterface("paypal");
            if (rocketInterface != null)
            {
                var payData = new PayPalData(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
                return payData.Active;
            }
            return false;
        }

        public override string GetBankRemotePost(PaymentLimpet paymentData)
        {
            var systemData = new SystemLimpet("rocketecommerceapi");
            var rocketInterface = systemData.GetInterface(paymentData.PaymentProvider);
            if (rocketInterface != null)
            {
                var rPost = new RemotePost();
                var paypalData = new PayPalData(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
                var postUrl = paypalData.LivePostUrl;
                if (paypalData.PreProduction) postUrl = paypalData.TestPostUrl;

                var rtnUrl = paypalData.ReturnUrl + "?cmd=" + paypalData.ReturnCmd;

                rPost.Url = postUrl;

                rPost.Add("cmd", "_xclick");
                rPost.Add("item_number", paymentData.PaymentId.ToString(""));
                rPost.Add("return", rtnUrl + "&status=1&s=rocketecommerceapi&key=" + paymentData.PaymentGuid);
                rPost.Add("currency_code", paypalData.CurrencyCode);
                rPost.Add("cancel_return", rtnUrl + "&status=0&s=rocketecommerceapi&key=" + paymentData.PaymentGuid);
                rPost.Add("notify_url", paypalData.NotifyUrl + "?cmd=remote_publicnotify&s=rocketecommerceapi&p=paypal&key=" + paymentData.PaymentKey);
                rPost.Add("custom", paypalData.PortalShop.CurrencyCultureCode);
                rPost.Add("business", paypalData.PayPalId);
                rPost.Add("item_name", paymentData.PaymentId.ToString(""));
                var aPay = paymentData.AmountPayCents.ToString();
                rPost.Add("amount", aPay.Substring(0, aPay.Length - 2) + "." + aPay.Substring(aPay.Length - 2)); // use en-US format, regardless of currency. (Seems Odd and even wrong!!!) 
                rPost.Add("shipping", "0");
                rPost.Add("tax", "0");
                rPost.Add("lc", DNNrocketUtils.GetCurrentCulture().Substring(3, 2));

                var extrafields = paypalData.ExtraFields;
                var fields = extrafields.Split(',');
                foreach (var f in fields)
                {
                    var ary = f.Split('=');
                    if (ary.Count() == 2)
                    {
                        var n = ary[0];
                        var v = ary[1];
                        rPost.Add(n, v);
                    }
                }

                //Build the re-direct html 
                var rtnStr = rPost.GetPostHtml();

                if (paypalData.DebugMode)
                {
                    FileUtils.SaveFile(PortalUtils.TempDirectoryMapPath() + "\\debug_rPost.html", rtnStr);
                }

                return rtnStr;
            }
            return "";
        }
        public override string NotifyEvent(SimplisityInfo paramInfo)
        {
            LogUtils.LogSystem("PayPal NotifyEvent: " + paramInfo.XMLData);

            var paymentKey = ProviderUtils.GetParam(paramInfo, "key");
            PaymentLimpet paymentData = new PaymentLimpet(PortalUtils.GetPortalId(), paymentKey);
            if (paymentData.Exists && paymentData.BankAction == PaymentAction.BankPost)
            {
                // update bank action to IPN, so the return does not update the paymentData with a race condition
                var ipn = new PayPalIpnParameters(paramInfo);
                paymentData.BankMessage = "version=2" + Environment.NewLine + "cdr=1";
                var paypalData = new PayPalData(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
                var postUrl = paypalData.LivePostUrl;
                if (paypalData.PreProduction) postUrl = paypalData.TestPostUrl;
                var validateUrl = postUrl + "?" + ipn.PostString;

                if (ProviderUtils.VerifyPayment(ipn, validateUrl))
                {
                    paymentData.Paid(true);
                }
                else
                {
                    if (ipn.IsValid)
                    {
                        paymentData.Paid(false);
                    }
                    else
                    {
                        paymentData.PaymentFailed();
                    }
                }
                paymentData.Update("paypal IPN");
            }
            return "OK";
        }

        public override void ReturnEvent(SimplisityInfo paramInfo)
        {
            LogUtils.LogSystem("PayPal ReturnEvent: " + paramInfo.XMLData);

            var paymentKey = ProviderUtils.GetParam(paramInfo, "key");
            PaymentLimpet paymentData = new PaymentLimpet(PortalUtils.GetPortalId(), paymentKey);
            if (paymentData.Exists && paymentData.BankAction == PaymentAction.BankPost)
            {
                var status = ProviderUtils.GetParam(paramInfo, "status");
                if (status == "0")
                    paymentData.PaymentFailed();
                else
                {
                    paymentData.Paid(false);
                }
            }
        }
        public override string PayButtonText()
        {
            var payData = new PayPalData(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
            return payData.PayButtonText;
        }

        public override string PayMsg()
        {
            var payData = new PayPalData(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
            return payData.PayMsg;
        }

        public override string PaymentProvKey()
        {
            var payData = new PayPalData(PortalUtils.GetCurrentPortalId(), DNNrocketUtils.GetCurrentCulture());
            return payData.PaymentProvKey;
        }
    }
}
