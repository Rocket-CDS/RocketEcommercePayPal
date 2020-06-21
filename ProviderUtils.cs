using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace RocketEcommerce.PayPal
{
    public class ProviderUtils
    {
        public static bool VerifyPayment(PayPalIpnParameters ipn, string verifyURL)
        {
            try
            {
                bool isVerified = false;

                if (ipn.IsValid)
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                    HttpWebRequest PPrequest = (HttpWebRequest)WebRequest.Create(verifyURL);
                    if ((PPrequest != null))
                    {
                        PPrequest.Method = "POST";
                        PPrequest.ContentLength = ipn.PostString.Length;
                        PPrequest.ContentType = "application/x-www-form-urlencoded";
                        StreamWriter writer = new StreamWriter(PPrequest.GetRequestStream());
                        writer.Write(ipn.PostString);
                        writer.Close();
                        HttpWebResponse response = (HttpWebResponse)PPrequest.GetResponse();
                        if ((response != null))
                        {
                            StreamReader reader = new StreamReader(response.GetResponseStream());
                            string responseString = reader.ReadToEnd();
                            reader.Close();
                            if (string.Compare(responseString, "VERIFIED", true) == 0)
                            {
                                isVerified = true;
                            }
                        }
                    }
                }
                return isVerified;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
