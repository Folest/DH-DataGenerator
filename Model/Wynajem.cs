using System;

namespace DataGenerator.Model
{
    public class Wynajem
    {
        public Guid Id { get; set; }
        public string Pesel { get; set; }
        public string Vin { get; set; }
        public int CennikId { get; set; }
        public DateTime CzasRozpoczecia { get; set; }
        public DateTime CzasZakonczenia { get; set; }
        public float OcenaPrzejazdu { get; set; }
        public double OdlegloscKm { get; set; }
        public double IloscZuzytegoPaliwa { get; set; }
        public TimeSpan CzasPostoju { get; set; }
        public int CenaZl { get; set; }
        public int CenaGr { get; set; }
    }
}
