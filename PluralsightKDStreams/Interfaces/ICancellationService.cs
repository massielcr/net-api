namespace PluralsightKDStreams.Interfaces
{
    public interface ICancellationService
    {
        CancellationToken GetToken(string key, int? seconds = null);

        void Cancel(string key);
    }
}
