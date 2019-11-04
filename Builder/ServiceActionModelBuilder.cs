using System;
using DataGenerator.Model;

namespace DataGenerator.Builder
{
    public class ServiceActionModelBuilder : IBuilder<ServiceActionModel>
    {
        private ServiceActionModel Model { get; } = new ServiceActionModel();

        public ServiceActionModelBuilder WithDescription(string description)
        {
            Model.Opis = description;
            return this;
        }

        public ServiceActionModelBuilder WithWarranty(int warrantyDays)
        {
            Model.GwarancjaWDniach = warrantyDays;
            return this;
        }

        public ServiceActionModelBuilder WithCost(int cost)
        {
            Model.Cena = cost;
            return this;
        }

        public ServiceActionModel BuildModel()
        {
            if (Model.GwarancjaWDniach == default || Model.Opis == default || Model.Cena == default)
            {
                Console.WriteLine("Warning! Some ServiceActionModel field hasn't been set");
            }
            return Model;
        }
    }
}