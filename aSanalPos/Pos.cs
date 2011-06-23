using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using _PosnetDotNetModule;

namespace aSanalPos
{
    public class Pos
    {
        #region Property
        private string sistemHatasi = "Bankayla bağlantı kurulamadı ! Lütfen daha sonra tekrar deneyin.";

        public bool sonuc { get; set; }
        public string hataMesaji { get; set; }
        public string hataKodu { get; set; }

        // Bankadan geri dönen değerler.
        public string code { get; set; }
        public string groupId { get; set; }
        public string transId { get; set; }
        public string referansNo { get; set; }
        #endregion

        #region Method
        public void GarantiBankasi(PosForm pf)
        {
            try
            {
                ePayment.cc5payment mycc5pay = new ePayment.cc5payment();
                mycc5pay.host = "https://ccpos.garanti.com.tr/servlet/cc5ApiServer";
                mycc5pay.name = "xxx";
                mycc5pay.password = "xxx";
                mycc5pay.clientid = "xxx";
                mycc5pay.orderresult = 0;
                mycc5pay.oid = Tool.RandomNumber();
                mycc5pay.currency = "949";
                mycc5pay.chargetype = "Auth";
                //gelenler
                mycc5pay.cardnumber = pf.kartNumarasi.ToString();
                mycc5pay.expmonth = string.Format("{0:00}", pf.ay);
                mycc5pay.expyear = pf.yil.ToString().Substring(2, 2);
                mycc5pay.cv2 = string.Format("{0:000}", pf.guvenlikKodu);
                mycc5pay.subtotal = pf.tutar.ToString();

                if (pf.taksit == -1)
                {
                    mycc5pay.taksit = "1";
                }
                else
                {
                    mycc5pay.taksit = pf.taksit.ToString();
                }

                //yedek bilgiler
                mycc5pay.bname = pf.kartSahibi;
                mycc5pay.phone = Tool.GetIp();
                string x = mycc5pay.processorder();
                if (x == "1" & mycc5pay.appr == "Approved")
                {
                    //bankadan geri dönen
                    this.sonuc = true;
                    this.groupId = mycc5pay.groupid;
                    this.referansNo = mycc5pay.refno;
                    this.transId = mycc5pay.transid;
                    this.code = mycc5pay.code;
                }
                else
                {
                    this.sonuc = false;
                    this.hataKodu = mycc5pay.err;
                    this.hataMesaji = mycc5pay.errmsg;
                }
            }
            catch (System.Exception)
            {
                this.sonuc = false;
                this.hataMesaji = this.sistemHatasi;
            }
        }
        public void YapiKredi(PosForm pf)
        {
            // Banka bilgileri.
            string mid = "xxx";
            string tid = "xxx";

            try
            {
                Random rnd = new Random();
                string ccno = pf.kartNumarasi.ToString(), expdate = pf.yil.ToString().Replace("20", string.Empty) + pf.ay, cvc = string.Format("{0:000}", pf.guvenlikKodu), orderid = "1234567890123456789" + rnd.Next(11111, 99999), amount = pf.tutar.ToString(), currencycode = "YT", instnumber = pf.taksit.ToString();

                C_Posnet posnetObj = new C_Posnet();
                bool result = false;
                posnetObj.SetURL("https://www.posnet.ykb.com/PosnetWebService/XML");
                posnetObj.SetMid(mid);
                posnetObj.SetTid(tid);
                result = posnetObj.DoSaleTran(ccno, expdate, cvc, orderid, amount, currencycode, instnumber, "", "");

                if (pf.taksit > 0) { posnetObj.SetKOICode(pf.taksit.ToString()); }

                if (posnetObj.GetApprovedCode() == "1")
                {
                    this.sonuc = true;
                    this.code = posnetObj.GetAuthcode();
                    this.referansNo = posnetObj.GetHostlogkey();
                }
                else
                {
                    this.sonuc = false;
                    this.hataMesaji = posnetObj.GetResponseText();
                }
            }
            catch (Exception)
            {
                this.sonuc = false;
                this.hataMesaji = this.sistemHatasi;
            }

        }
        public void VakifBank(PosForm pf)
        {
            // Banka bilgileri.
            string kullanici = "0001";
            string sifre = "xxx";
            string uyeno = "xxx";
            string posno = "xxx";
            string xcip = "xxx";

            try
            {
                byte[] b = new byte[5000];
                string sonuc;
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("ISO-8859-9");

                String tutarcevir = pf.tutar.ToString();
                tutarcevir = tutarcevir.Replace(".", "");
                tutarcevir = tutarcevir.Replace(",", "");
                tutarcevir = String.Format("{0:0000000000.00}", Convert.ToInt32(tutarcevir)).Replace(",", "");

                string taksitcevir = "";

                if (pf.taksit == -1)
                {
                    taksitcevir = "00";
                }
                else
                {
                    taksitcevir = String.Format("{0:00}", pf.taksit);
                }

                String yilcevir = pf.yil.ToString();
                yilcevir = yilcevir.Substring(2, 2);

                string aycevir = string.Format("{0:00}", pf.ay);

                string provizyonMesaji = "kullanici=" + kullanici + "&sifre=" + sifre + "&islem=PRO&uyeno=" + uyeno + "&posno=" + posno + "&kkno=" + pf.kartNumarasi + "&gectar=" + yilcevir + aycevir + "&cvc=" + string.Format("{0:000}", pf.guvenlikKodu) + "&tutar=" + tutarcevir + "&provno=000000&taksits=" + taksitcevir + "&islemyeri=I&uyeref=" + Tool.RandomNumber() + "&vbref=0&khip=" + Tool.GetIp() + "&xcip="+xcip;

                b.Initialize();
                b = Encoding.ASCII.GetBytes(provizyonMesaji);

                WebRequest h1 = (WebRequest)HttpWebRequest.Create("https://subesiz.vakifbank.com.tr/vpos724v3/?" + provizyonMesaji);
                h1.Method = "GET";

                WebResponse wr = h1.GetResponse();
                Stream s2 = wr.GetResponseStream();

                byte[] buffer = new byte[10000];
                int len = 0, r = 1;
                while (r > 0)
                {
                    r = s2.Read(buffer, len, 10000 - len);
                    len += r;
                }
                s2.Close();
                sonuc = encoding.GetString(buffer, 0, len).Replace("\r", "").Replace("\n", "");

                String gelenonaykodu, gelenrefkodu;
                XmlNode node = null;
                XmlDocument _msgTemplate = new XmlDocument();
                _msgTemplate.LoadXml(sonuc);
                node = _msgTemplate.SelectSingleNode("//Cevap/Msg/Kod");
                gelenonaykodu = node.InnerText.ToString();

                if (gelenonaykodu == "00")
                {
                    node = _msgTemplate.SelectSingleNode("//Cevap/Msg/ProvNo");
                    gelenrefkodu = node.InnerText.ToString();
                    this.sonuc = true;
                    this.referansNo = gelenrefkodu;
                }
                else
                {
                    this.sonuc = false;
                    this.hataMesaji = "";
                    this.hataKodu = gelenonaykodu;
                }
            }
            catch (Exception e)
            {
                this.sonuc = false;
                this.hataMesaji = this.sistemHatasi;
            }
        }
        public void Akbank(PosForm pf)
        {
            try
            {
                ePayment.cc5payment mycc5pay = new ePayment.cc5payment();

                mycc5pay.host = "https://www.sanalakpos.com/servlet/cc5ApiServer";
                mycc5pay.name = "xxx";
                mycc5pay.password = "xxx";
                mycc5pay.clientid = "xxx";
                mycc5pay.orderresult = 0;

                mycc5pay.oid = Tool.RandomNumber();

                mycc5pay.cardnumber = pf.kartNumarasi.ToString();
                mycc5pay.expmonth = pf.ay.ToString();
                mycc5pay.expyear = pf.yil.ToString().Substring(2, 2);
                mycc5pay.cv2 = pf.guvenlikKodu.ToString();

                mycc5pay.subtotal = string.Format("{0:0.00}", pf.tutar);
                mycc5pay.currency = "949";
                mycc5pay.chargetype = "Auth";

                if (pf.taksit == -1)
                {
                    mycc5pay.taksit = "1";
                }
                else
                {
                    mycc5pay.taksit = pf.taksit.ToString();
                }

                //fatura bilgileri
                mycc5pay.bname = pf.kartSahibi;
                mycc5pay.bcity = Tool.GetIp();

                string x = mycc5pay.processorder();

                if (x == "1" & mycc5pay.appr == "Approved")
                {
                    this.sonuc = true;
                    this.groupId = mycc5pay.groupid;
                    this.code = mycc5pay.code;
                    this.transId = mycc5pay.transid;
                    this.referansNo = mycc5pay.refno;
                }
                else
                {
                    this.hataMesaji = "";
                    this.hataKodu = mycc5pay.errmsg;
                    this.sonuc = false;
                }
            }
            catch (System.Exception)
            {
                this.hataMesaji = this.sistemHatasi;
                this.sonuc = false;
            }
        }
        public void IsBankasi(PosForm pf)
        {
            try
            {
                ePayment.cc5payment mycc5pay = new ePayment.cc5payment();

                mycc5pay.host = "https://spos.isbank.com.tr/servlet/cc5ApiServer";
                mycc5pay.name = "xxx";
                mycc5pay.password = "xxx";
                mycc5pay.clientid = "xxx";
                mycc5pay.orderresult = 0;

                mycc5pay.oid = Tool.RandomNumber();

                mycc5pay.cardnumber = pf.kartNumarasi.ToString();
                mycc5pay.expmonth = pf.ay.ToString();
                mycc5pay.expyear = pf.yil.ToString().Replace("20", string.Empty);
                mycc5pay.cv2 = pf.guvenlikKodu.ToString();

                mycc5pay.subtotal = pf.tutar.ToString();
                mycc5pay.currency = "949";
                mycc5pay.chargetype = "Auth";
                mycc5pay.taksit = pf.taksit.ToString();

                //fatura bilgileri
                mycc5pay.bname = pf.kartSahibi;
                mycc5pay.bcity = Tool.GetIp();

                string x = mycc5pay.processorder();

                if (x == "1" & mycc5pay.appr == "Approved")
                {
                    //bankadan geri dönen
                    this.sonuc = false;
                    this.groupId = mycc5pay.groupid;
                    this.transId = mycc5pay.transid;
                    this.code = mycc5pay.code;
                    this.referansNo = mycc5pay.refno;

                }
                else
                {                    
                    this.sonuc = false;
                    this.hataMesaji = mycc5pay.errmsg;
                    this.hataKodu = mycc5pay.errmsg;

                }
            }
            catch (Exception)
            {                
                this.sonuc = false;
                this.hataMesaji = this.sistemHatasi;
            }
        }
        #endregion
    }
}