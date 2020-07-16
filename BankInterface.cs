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
        public override string GetBankRemotePost(PaymentLimpet paymentData, SystemData systemInfoData)
        {
            var rocketInterface = systemInfoData.GetInterface(paymentData.PaymentProvider);
            if (rocketInterface != null)
            {
                var rPost = new RemotePost();
                var paypalData = new PayPalData(PortalUtils.SiteGuid());
                var appliedtotal = paymentData.Amount.ToString("0.00").Replace(",", "");
                var postUrl = paypalData.LivePostUrl;
                if (paypalData.PreProduction) postUrl = paypalData.TestPostUrl;

                // get the order data

                rPost.Url = postUrl;

                rPost.Add("cmd", "_xclick");
                rPost.Add("item_number", paymentData.PaymentId.ToString(""));
                rPost.Add("return", paypalData.ReturnUrl + "?status=1&key=" + paymentData.PaymentGuid);
                rPost.Add("currency_code", paypalData.CurrencyCode);
                rPost.Add("cancel_return", paypalData.ReturnUrl + "?status=0&key=" + paymentData.PaymentGuid);
                rPost.Add("notify_url", paypalData.NotifyUrl + "?systemprovider=rocketecommerce&cmd=rocketecommerce_notify&key=" + paymentData.PaymentGuid);
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
        public override int GetOrderRecordId(SimplisityInfo postInfo, SimplisityInfo paramInfo)
        {
            var ipn = new PayPalIpnParameters(postInfo);
            return ipn.item_number;
        }
        public override PaymentLimpet NotifyEvent(PaymentLimpet paymentData, SimplisityInfo postInfo, SimplisityInfo paramInfo, SystemData systemInfoData)
        {
            var rtnMsg = "";
            var rocketInterface = systemInfoData.GetInterface("paypal");
            if (rocketInterface != null)
            {
                var ipn = new PayPalIpnParameters(postInfo);
                if (paymentData.Status == PaymentStatus.WaitingForBank) // Only process if we are waiting for bank.
                {
                    paymentData.BankMessage = "version=2" + Environment.NewLine + "cdr=1";
                    var paypalData = new PayPalData(PortalUtils.SiteGuid());
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

            return paymentData;
        }
        public override PaymentLimpet ReturnEvent(PaymentLimpet paymentData, SimplisityInfo postInfo, SimplisityInfo paramInfo, SystemData systemInfoData)
        {

            if (paymentData.Status == PaymentStatus.WaitingForBank)
            {
                if (paramInfo.GetXmlPropertyInt("genxml/urlparams/status") == 0)
                    paymentData.Status = PaymentStatus.PaymentFailed;
                else
                    paymentData.Status = PaymentStatus.PaymentNotVerified;
                paymentData.Update();
            }
            return paymentData;
        }
    }
}
