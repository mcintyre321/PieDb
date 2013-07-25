namespace PieDb
{
    public abstract class DatabaseTransactionAction
    {
        public abstract void Apply(DataStore target);
    }
}