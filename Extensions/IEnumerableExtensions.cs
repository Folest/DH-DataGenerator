using System.Collections.Generic;
using System.Text;
using DataGenerator.Model;
using DataGenerator.Model.Sql;

namespace DataGenerator.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string ToInsert(this IEnumerable<ModelSamochodu> models)
        {
            var sb = new StringBuilder(@"INSERT INTO ModeleSamochodow(id, marka, nazwa, generacja, oznaczenia_silnika, moc, srednie_spalanie, naped, typ_silnika) VALUES");

            foreach (var model in models)
            {
                sb.Append(
                    $"({model.Id},'{model.Marka}', '{model.Nazwa}', {model.Generacja}, '{model.OznaczenieSilnika}'," +
                    $" {model.Moc}, {model.SrednieSpalanie:F}, '{model.Naped}', '{model.TypSilnika}'),\n");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Samochod> cars)
        {
            var sb = new StringBuilder(@"INSERT INTO Samochody(vin, model_id, data_zakupu, cena_zakupu, obszary_dzialalnosci_nazwa, dostepny, kolor, skrzynia_automatyczna, szerokosc_geo, wysokosc_geo) VALUES");

            foreach (var car in cars)
            {
                sb.Append(
                    $"('{car.Vin}', {car.ModelId}, '{car.DataZakupu:d}',{car.CenaZakupu}, '{car.ObszaryDzialalnosciNazwa}'," +
                    $" {car.Dostepny}, '{car.Kolor}', {car.SkrzyniaAutomatyczna}, {car.LokalizacjaSzerokosc:0.#####}, {car.LokalizacjaWysokosc:0.#####}),\n");
            }
            sb.Remove(sb.Length - 2, 2);

            sb.Append(';');
            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Uzytkownik> users)
        {
            var sb = new StringBuilder(@"INSERT INTO Uzytkownicy(pesel, imie, nazwisko, plec, data_urodzenia, data_rejestracji) VALUES");

            foreach (var user in users)
            {
                sb.Append(
                    $"('{user.Pesel}', '{user.Imie}', '{user.Nazwisko}', '{(user.IsMale ? "m" : "k")}'," +
                    $" '{user.DataUrodzenia:O}', '{user.DataRejestracji:O}'),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Wynajem> rentals)
        {
            var sb = new StringBuilder("INSERT INTO Wynajmy(id, pesel, vin, cennik_id, czas_rozpoczecia, czas_zakonczenia, ocena_przejazdu, odleglosc_km, ilosc_zuzytego_paliwa, czas_postoju)\nVALUES");

            foreach (var rent in rentals)
            {
                sb.Append(
                    $"('{rent.Id}', '{rent.Pesel}', {rent.CennikId}, '{rent.CzasRozpoczecia:O}', '{rent.CzasZakonczenia:O}', {rent.OcenaPrzejazdu:F}," +
                    $" {rent.OdlegloscKm:F}, {rent.IloscZuzytegoPaliwa:F}, '{rent.CzasPostoju:c}'),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Cennik> rates)
        {
            var sb = new StringBuilder("INSERT INTO Cenniki\n VALUES ");

            foreach (var rate in rates)
            {
                sb.Append(
                    $"('{rate.Id}', '{rate.CenaPrzejazduKm}', {rate.CenaPrzejazduMinuta}, {rate.CenaPostojuMinuta}),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }
    }
}