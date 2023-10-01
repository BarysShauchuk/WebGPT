namespace WebGPT.Data.SearchService
{
    public interface ISearchService
    {
        public Task<List<string>> SearchAsync(string? query);
        public Task<string> GetPageAsync(string? url);
    }
}
