namespace DataGenerator.Model
{
    public class Cennik
    {
        public int Id { get; set; }
        public int CenaPrzejazduKm { get; set; }
        public int CenaPrzejazduMinuta { get; set; }
        public int CenaPostojuMinuta { get; set; }
        public string ObszarDzialalnosciNazwa { get; set; }

        public static Cennik DefaultPriceList = new Cennik { Id = 0, CenaPostojuMinuta = 10, CenaPrzejazduKm = 50, CenaPrzejazduMinuta = 50, ObszarDzialalnosciNazwa = ObszarDzialalnosci.DefaultArea.Nazwa};
    }
}

