using System.Collections.Generic;
using System.Text;
using DataGenerator.Model;

namespace DataGenerator.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string ToInsert(this IEnumerable<ModelSamochodu> models)
        {
            var sb = new StringBuilder(@"INSERT INTO Modele
                                       (id, marka, nazwa, generacja, oznaczenia_silnika, moc, srednie_spalanie, naped, typ_silnika) 
                                        VALUES");

            foreach (var model in models)
            {
                sb.Append(
                    $"({model.Id}, {model.Nazwa}, {model.Generacja}, {model.OznaczenieSilnika}," +
                    $" {model.Moc}, {model.SrednieSpalanie}, {model.TypSilnika})\n");
            }
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Samochod> cars)
        {
            var sb = new StringBuilder(@"INSERT INTO Samochody
                                       (vin, model_id, data_zakupu, cena_zakupu, obszary_dzialalnosci_nazwa, dostepny, kolor, skrzynia_automatyczna) 
                                        VALUES");

            foreach (var car in cars)
            {
                sb.Append(
                    $"({car.Vin}, {car.ModelId}, {car.CenaZakupu}, {car.ObszaryDzialalnosciNazwa}," +
                    $" {car.Dostepny}, {car.Kolor}, {car.SkrzyniaAutomatyczna})\n");
            }

            sb.Append(';');
            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Uzytkownik> users)
        {
            var sb = new StringBuilder(@"INSERT INTO Uzytkownicy
                                       (pesel, imie, nazwisko, plec, data_urodzenia, data_rejestracji) 
                                        VALUES");

            foreach (var user in users)
            {
                sb.Append(
                    $"({user.Pesel}, {user.Imie}, {user.Nazwisko}, {(user.IsMale ? "m" : "k")}," +
                    $" {user.DataUrodzenia}, {user.DataRejestracji})\n");
            }
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Wynajem> rentals)
        {
            var sb = new StringBuilder(@"INSERT INTO Wynajmy
                                       (id, pesel, vin, cennik_id, czas_rozpoczecia, czas_zakonczenia, ocena_przejazdu, odleglosc_km,
                                        ilosc_zuzytego_paliwa, czas_postoju, cena) 
                                        VALUES");

            foreach (var rent in rentals)
            {
                sb.Append(
                    $"({rent.Id}, {rent.Pesel}, {rent.CennikId}, {rent.CzasRozpoczecia}, {rent.CzasZakonczenia}, {rent.OcenaPrzejazdu}," +
                    $" {rent.OdlegloscKm}, {rent.IloscZuzytegoPaliwa}, {rent.CzasPostoju}, {rent.CenaZl}.{rent.CenaGr})\n");
            }
            sb.Append(';');

            return sb.ToString();
        }

    }
}