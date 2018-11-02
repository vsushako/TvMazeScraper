namespace TvMazeScraper.Repository
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork CreateNew();
    }
}
