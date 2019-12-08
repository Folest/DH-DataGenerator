using System.Collections.Generic;
using System.Text;
using DataGenerator.Model;
using DataGenerator.Model.Sql;

namespace DataGenerator.Extensions
{
    public static class InsertBuilders
    {
        public static string ToInsert(this IEnumerable<ModelSamochodu> models)
        {
            var sb = new StringBuilder("INSERT INTO ModeleSamochodow(id, marka, nazwa, generacja, oznaczenie_silnika, moc, srednie_spalanie, naped, typ_silnika, typ)\nVALUES");

            foreach (var model in models)
            {
                sb.Append(
                    $"({model.Id},'{model.Marka}', '{model.Nazwa}', {model.RokRozpoczeciaProdukcji}, '{model.OznaczenieSilnika}'," +
                    $" {model.Moc}, {model.SrednieSpalanie:F}, '{model.Naped}', '{model.TypSilnika}', '{model.Typ}'),\n");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Samochod> cars)
        {
            var sb = new StringBuilder("INSERT INTO Samochody(vin, FK_Modele_id, data_zakupu, cena_zakupu, FK_Obszary_dzialalnosci_nazwa, dostepny, kolor, skrzynia_automatyczna, lokalizacja_szerokosc, lokalizacja_dlugosc) \nVALUES");

            foreach (var car in cars)
            {
                sb.Append(
                    $"('{car.Vin}', {car.ModelId}, '{car.DataZakupu:O}',{car.CenaZakupu}, '{car.ObszaryDzialalnosciNazwa}'," +
                    $" {(car.Dostepny ? 1 : 0)}, '{car.Kolor}'," +
                    $" {(car.SkrzyniaAutomatyczna ? 1 : 0)}, {car.LokalizacjaSzerokosc:0.#####}, {car.LokalizacjaWysokosc:0.#####}),\n");
            }
            sb.Remove(sb.Length - 2, 2);

            sb.Append(';');
            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Uzytkownik> users)
        {
            var sb = new StringBuilder("INSERT INTO Uzytkownicy(pesel, imie, nazwisko, plec, data_urodzenia, data_rejestracji)\n VALUES");

            foreach (var user in users)
            {
                sb.Append(
                    $"('{user.Pesel}', '{user.Imie}', '{user.Nazwisko}', '{(user.IsMale ? "mezczyzna" : "kobieta")}'," +
                    $" '{user.DataUrodzenia:O}', '{user.DataRejestracji:O}'),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Wynajem> rentals)
        {
            var sb = new StringBuilder("INSERT INTO Wynajmy(id, FK_Uzytkownicy_pesel,FK_Cenniki_id, FK_Samochody_vin, czas_rozpoczecia, czas_zakonczenia, ocena_przejazdu, odleglosc_km, ilosc_zuzytego_paliwa, czas_postoju)\nVALUES");

            foreach (var rent in rentals)
            {
                sb.Append(
                    $"({rent.Id}, '{rent.Pesel}', {rent.CennikId}, " +
                    $"'{rent.Vin}' ,'{rent.CzasRozpoczecia:O}', '{rent.CzasZakonczenia:O}', {rent.OcenaPrzejazdu:F}," +
                    $" {rent.OdlegloscKm:F}, {rent.IloscZuzytegoPaliwa:F}, '{rent.CzasPostojuMin}'),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<Cennik> rates)
        {
            var sb = new StringBuilder("INSERT INTO Cenniki\nVALUES ");

            foreach (var rate in rates)
            {
                sb.Append(
                    $"({rate.Id}," +
                    $" {(rate.CenaPrzejazduKm > 100 ? $"1.{rate.CenaPrzejazduKm}" : $"0.{rate.CenaPrzejazduKm}")}," +
                    $" 0.{rate.CenaPrzejazduMinuta}, 0.{rate.CenaPostojuMinuta}),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<ObszarDzialalnosci> operationArea)
        {
            var sb = new StringBuilder("INSERT INTO ObszaryDzialalnosci\nVALUES ");

            foreach (var area in operationArea)
            {
                sb.Append($"('{area.Nazwa}', '{area.PunktCentralny.Item1}','{area.PunktCentralny.Item2}', '{area.Srednica}'),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }

        public static string ToInsert(this IEnumerable<PrawoJazdy> licenses)
        {
            var sb = new StringBuilder("INSERT INTO PrawaJazdy\nVALUES ");

            foreach (var license in licenses)
            {
                sb.Append($"({license.Id}, '{license.DataWydania:O}','{license.Pesel}'),\n");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(';');

            return sb.ToString();
        }
    }
}