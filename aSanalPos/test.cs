using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aSanalPos
{
    public class test
    {
        public void Cekim() {

            // kullanıcıdan kart bilgilerini alıyoruz.
            PosForm pf = new PosForm { 
                ay = 1,
                yil=2011,
                guvenlikKodu = 123,
                kartNumarasi = 1234567891234568,
                kartSahibi = "kart sahibi",
                taksit = 3,
                tutar = 1.00
            };
            
            // Poslarımıza yukarıdaki bilgileri gönderiyoruz.
            Pos p = new Pos();

            // Örnek gönderim;
            p.Akbank(pf);
            //p.GarantiBankasi(pf);
            //p.VakifBank(pf);
            //p.YapiKredi(pf);
            //p.IsBankasi(pf);


            // Poslardan geriye dönen bilgileri alıyoruz.

            if (p.sonuc)
            {
                // Çekim işlemi başarılı ise, geri dönen bilgileri alıyoruz. 
                // Genellikle bu bilgiler veritabanında saklanır.
                // Bankadan bankaya değişiklik göstereceği için, alanlardan bazıları boş gelebilir.
                Console.Write(p.referansNo);
                Console.Write(p.groupId);
                Console.Write(p.transId);
                Console.Write(p.code);
            }
            else
            {
                // Çekim işlemi herhangi bir sebepden dolayı olumsuz sonuçlanmışsa, bankadan dönen hatayı alıyoruz.
                // Hata kodlarının açıklamaları ilgili banka dökümantasyonunda belirtilmiştir.
                Console.Write(p.sonuc);
                Console.Write(p.hataMesaji);
                Console.Write(p.hataKodu);
            }
        
        }
    }
}
