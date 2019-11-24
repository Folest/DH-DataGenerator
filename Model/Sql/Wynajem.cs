using System;

namespace DataGenerator.Model.Sql
{
    public class Wynajem
    {
        public static long CurrentId { get; private set; } = 1;

        public Wynajem()
        {
            Id = CurrentId++;
        }

        public long Id { get; }
        public string Pesel { get; set; }
        public string Vin { get; set; }
        public long CennikId => Cennik?.Id ?? -1;
        public DateTime CzasRozpoczecia { get; set; }
        public DateTime CzasZakonczenia { get; set; }
        public float OcenaPrzejazdu { get; set; }
        public double OdlegloscKm { get; set; }
        public double IloscZuzytegoPaliwa { get; set; }
        public int CzasPostojuMin { get; set; }

        public Cennik Cennik { get; set; }
    }
}
