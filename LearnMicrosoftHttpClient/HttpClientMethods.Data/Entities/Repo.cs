namespace HttpClientMethods.Data.Entities
{
    internal class Repo
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public int Owner { get; set; }
    }
}
