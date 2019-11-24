using System.Runtime.Serialization;

namespace DataGenerator.Model.Sql
{
    public class ModelSamochodu
    {
        public int Id { get; set; }
        public string Marka { get; set; }
        public string Nazwa { get; set; }
        public int Generacja { get; set; }
        public string OznaczenieSilnika { get; set; }
        public int Moc { get; set; }
        public double SrednieSpalanie { get; set; }
        public string Naped { get; set; }
        public string TypSilnika { get; set; }
        public string Typ { get; set; }

        [IgnoreDataMember]
        public (int, int) PriceRange { get; set; }
    }
}
