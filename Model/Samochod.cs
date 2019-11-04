using System;
using System.Collections.Generic;

namespace DataGenerator.Model
{
    public class Samochod
    {
        public ModelSamochodu Model { get; set; }
        public string Vin { get; set; }
        public string Kolor { get; set; }
        public DateTime DataZakupu { get; set; }
        public int CenaZakupu { get; set; }
        public string ObszaryDzialalnosciNazwa { get; set; }
        public bool Dostepny { get; set; }
        public bool SkrzyniaAutomatyczna { get; set; }

        public IList<ServiceDataModel> Services = new List<ServiceDataModel>();
    }
}
