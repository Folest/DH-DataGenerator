using System;
using System.Collections.Generic;
using System.Linq;
using DataGenerator.Model;

namespace DataGenerator.Builder
{
    public class ServiceDataModelBuilder
    {
        private ServiceDataModel Model { get; } = new ServiceDataModel();

        public ServiceDataModelBuilder()
        {
        }

        public ServiceDataModelBuilder WithInvoiceNumber(string number)
        {
            Model.NumerRachunku = number;
            return this;
        }

        public ServiceDataModelBuilder WithVin(string vin)
        {
            Model.VIN = vin;
            return this;
        }

        public ServiceDataModelBuilder WithTotalCost(int cost)
        {
            Model.LacznyKoszt = cost;
            return this;
        }

        public ServiceDataModelBuilder WithServiceName(string serviceName)
        {
            Model.NazwaSerwisu = serviceName;
            return this;
        }

        public ServiceDataModelBuilder WithServiceActions(IEnumerable<ServiceActionModel> serviceActions)
        {
            Model.OpisCzynnosciNaprawczych = serviceActions;
            return this;
        }

        public ServiceDataModelBuilder WithAdmittanceDate(DateTime admittanceDate)
        {
            Model.DataPrzyjecia = admittanceDate;
            return this;
        }

        public ServiceDataModelBuilder WithReturnDate(DateTime returnDate)
        {
            Model.DataRealizacji = returnDate;
            return this;
        }

        public ServiceDataModelBuilder WithDuration(int days)
        {
            Model.DataPrzyjecia = Model.DataPrzyjecia.AddDays(days);
            return this;
        }

        public ServiceDataModel CalculateCostAndBuildModel()
        {
            Model.LacznyKoszt = Model.OpisCzynnosciNaprawczych.Aggregate(0, (sum, model) => sum + model.Cena);
            return Model;
        }

    }
}