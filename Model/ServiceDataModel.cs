using System;
using System.Collections.Generic;

namespace DataGenerator.Model
{
    public class ServiceDataModel
    {
        public string NumerRachunku { get; set; }
        public string VIN { get; set; }
        public IEnumerable<ServiceActionModel> OpisCzynnosciNaprawczych { get; set; }
        public int LacznyKoszt { get; set; }
        public DateTime DataPrzyjecia { get; set; }
        public DateTime DataRealizacji { get; set; }
        public string NazwaSerwisu { get; set; }
    }
}