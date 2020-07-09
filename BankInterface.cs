using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RocketEcommerce;
using DNNrocketAPI.Componants;
using RocketEcommerce.Interfaces;
using RocketEcommerce.Componants;

namespace RocketEcommerce.PayPal
{
    public class BankInterface : PaymentInterface
    {
        public override string GetBankRemotePost(PaymentData paymentData, SystemData systemInfoData)
        {
            var rocketInterface = systemInfoData.GetInterface(paymentData.PaymentProvider);
            if (rocketInterface != null)
            {
                var rPost = new RemotePost();
                var paypalData = new PayPalData(PortalUtils.SiteGuid(), systemInfoData.SystemKey);
                var appliedtotal = paymentData.Amount.ToString("0.00").Replace(",", "");
                var postUrl = paypalData.LivePostUrl;
                if (paypalData.PreProduction) postUrl = paypalData.TestPostUrl;

                // get the order data

                rPost.Url = postUrl;

                rPost.Add("cmd", "_xclick");
                rPost.Add("item_number", paymentData.PaymentId.ToString(""));
                rPost.Add("return", paypalData.ReturnUrl + "?status=1&code=" + paymentData.PaymentGuid);
                rPost.Add("currency_code", paypalData.CurrencyCode);
                rPost.Add("cancel_return", paypalData.ReturnUrl + "?status=0&code=" + paymentData.PaymentGuid);
                rPost.Add("notify_url", paypalData.NotifyUrl + "?systemprovider=RocketEcommerce&cmd=RocketEcommerce_notify&code=" + paymentData.PaymentGuid);
                rPost.Add("custom", DNNrocketUtils.GetCurrentCulture());
                rPost.Add("business", paypalData.PayPalId);
                rPost.Add("item_name", paymentData.PaymentId.ToString(""));
                rPost.Add("amount", appliedtotal);
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
                        var d = paymentData.Info.GetXmlProperty(v);
                        rPost.Add(n, d);
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
        public override int GetOrderRecordId(SimplisityInfo postInfo, SimplisityInfo paramInfo)
        {
            var ipn = new PayPalIpnParameters(postInfo);
            return ipn.item_number;
        }
        public override string NotifyEvent(SimplisityInfo postInfo, SimplisityInfo paramInfo, SystemData systemInfoData)
        {
            var rtnMsg = "";
            var paymentData = new PaymentData(PortalUtils.GetPortalId(), paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid"), DNNrocketUtils.GetCurrentCulture());
            var rocketInterface = systemInfoData.GetInterface(paymentData.PaymentProvider);
            if (rocketInterface != null)
            {
                rtnMsg = "version=2" + Environment.NewLine + "cdr=1";

                var ipn = new PayPalIpnParameters(postInfo);
                if (paymentData.Status == PaymentStatus.WaitingForBank) // Only process if we are waiting for bank.
                {
                    var paypalData = new PayPalData(PortalUtils.SiteGuid(), systemInfoData.SystemKey);
                    var postUrl = paypalData.LivePostUrl;
                    if (paypalData.PreProduction) postUrl = paypalData.TestPostUrl;
                    var validateUrl = postUrl + "?" + ipn.PostString;

                    if (ProviderUtils.VerifyPayment(ipn, validateUrl))
                    {
                        paymentData.Status  = PaymentStatus.PaymentOK;
                    }
                    else
                    {
                        if (ipn.IsValid)
                        {
                            paymentData.Status = PaymentStatus.PaymentNotVerified;
                        }
                        else
                        {
                            paymentData.Status = PaymentStatus.PaymentFailed;
                        }
                    }
                    paymentData.Update();
                }
            }

            return rtnMsg;
        }
        public override int ReturnEvent(SimplisityInfo postInfo, SimplisityInfo paramInfo, SystemData systemInfoData)
        {
            var paymentData = new PaymentData(PortalUtils.GetPortalId(), paramInfo.GetXmlPropertyInt("genxml/hidden/paymentid"), DNNrocketUtils.GetCurrentCulture());
            var rocketInterface = systemInfoData.GetInterface(paymentData.PaymentProvider);
            if (rocketInterface != null)
            {
                if (paymentData.Status == PaymentStatus.WaitingForBank) // Only process if we are waiting for bank.
                {
                    if (paramInfo.GetXmlProperty("genxml/hidden/status") == "1")
                    {
                        return Convert.ToInt32(PaymentStatus.PaymentOK);
                    }
                    if (paramInfo.GetXmlProperty("genxml/hidden/status") == "0")
                    {
                        return Convert.ToInt32(PaymentStatus.PaymentFailed);
                    }
                }
            }

            return -1;
        }
    }
}
