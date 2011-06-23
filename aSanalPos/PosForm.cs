using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aSanalPos
{
    public class PosForm
    {
        public string kartSahibi { get; set; }
        public long kartNumarasi { get; set; }
        public int guvenlikKodu { get; set; }
        public int ay { get; set; }
        public int yil { get; set; }
        public double tutar { get; set; }
        public int taksit { get; set; }
    }
}
