namespace DataGenerator.Model
{
    public class ObszarDzialalnosci
    {
        public string Nazwa { get; set; }

        public (double, double) PunktCentralny { get; set; }
        public double Srednica { get; set; }


        public static ObszarDzialalnosci DefaultArea = new ObszarDzialalnosci
        {
            Nazwa = "Trojmiasto", PunktCentralny = (54.403367, 18.445529), Srednica = 30
        };
    }
}
