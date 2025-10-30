namespace LibElectronAppApi.Shared.Abstract;

public interface IWebClient : IDisposable
{
    Task<T> GetDataAsync<T>(string url) where T : class;

    Task<byte[]> GetDataAsync(string url);
        
    Task<byte[]> PostDataAsync(string url, string contentType, byte[] data);

    Task<byte[]> PostAsync(string url);

    Task<byte[]> PostJsonDataAsync(string url, object data);

    Task DownloadFileAsync(
        string url,
        string filename,
        CancellationToken cancelToken,
        Action<long, int, long> progressAction);

    Task<long> DownloadPartialFileAsync(
        string url,
        string filename,
        long from,
        long to,
        Dictionary<string, string> requestHeaders,
        CancellationToken cancelToken,
        Action<long, int, long> progressAction,
        bool useRangeHeader);

    Task<long> GetContentLengthAsync(string url, CancellationToken cancelToken);

    void SetRequestHeaders(IReadOnlyDictionary<string, string> headers);

    void UpdateRequestHeaders(IReadOnlyDictionary<string, string> headers);

    void SetRequestHeader(string name, string value);
}