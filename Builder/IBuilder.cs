namespace DataGenerator.Builder
{
    public interface IBuilder<out T> where T : new()
    {
        T BuildModel();
    }
}