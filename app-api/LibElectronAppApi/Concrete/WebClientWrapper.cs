using System.Net;
using System.Net.Http.Headers;
using System.Text;
using LibElectronAppApi.Abstract;
using Newtonsoft.Json;

namespace LibElectronAppApi.Concrete;

public class WebClientWrapper : IWebClient
{
#pragma warning disable SYSLIB0014
    private class CustomWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;

            request.Headers.Clear();
            foreach (var header in RequestHeaders.Keys)
            {
                if (request.Headers.AllKeys.Contains(header))
                    request.Headers.Set(header, RequestHeaders[header]);
                else
                    request.Headers.Add(header, RequestHeaders[header]);
            }

            return request;
        }

        public Dictionary<string, string> RequestHeaders =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
#pragma warning restore SYSLIB0014

    private readonly IFileOperations fileOperations;
    private readonly CustomWebClient innerWebClient;
    private bool disposed;

    public WebClientWrapper(IFileOperations fileOperations)
    {
        this.fileOperations = fileOperations;
#pragma warning disable SYSLIB0014
        innerWebClient = new CustomWebClient();
#pragma warning restore SYSLIB0014
        innerWebClient.Proxy = null;
    }

    public void Dispose()
    {
        if (!disposed)
        {
            innerWebClient.Dispose();
            disposed = true;
        }
    }

    public async Task<byte[]> GetDataAsync(string url)
    {
        return await innerWebClient.DownloadDataTaskAsync(url);
    }

    public async Task DownloadFileAsync(
        string url,
        string filename,
        CancellationToken cancelToken,
        Action<long, int, long> progressAction)
    {

        long streamSize = -1;
        if (progressAction != null)
        {
            streamSize = await GetContentLengthAsync(url, cancelToken);
        }

        using (var inputStream = innerWebClient.OpenRead(url))
        {
            if (inputStream != null)
            {
                using (var outputStream = fileOperations.CreateFileStream(filename, FileMode.Create))
                {
                    var buffer = new byte[65535];
                    int byteCountRead;
                    long totalByteCountRead = 0;
                    do
                    {
                        byteCountRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancelToken);
                        if (byteCountRead > 0)
                        {
                            await outputStream.WriteAsync(buffer, 0, byteCountRead, cancelToken);
                            totalByteCountRead += byteCountRead;
                            progressAction?.Invoke(streamSize, byteCountRead, totalByteCountRead);
                        }
                    } while (!cancelToken.IsCancellationRequested && byteCountRead > 0);
                }

                if (cancelToken.IsCancellationRequested)
                {
                    fileOperations.DeleteFile(filename);
                }
            }
        }
    }

    public async Task<byte[]> PostDataAsync(string url, string contentType, byte[] data)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            SetRequestHeader("Content-Type", contentType);
            SetRequestHeader("Content-Length", data.Length.ToString());
        }

        return await innerWebClient.UploadDataTaskAsync(new Uri(url), HttpMethod.Post.Method, data);
    }

    public async Task<byte[]> PostAsync(string url)
    {
        return await PostJsonDataAsync(url, new object());
    }

    public async Task<byte[]> PostJsonDataAsync(string url, object data)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        return await PostDataAsync(url, "application/json", bytes);
    }

    public async Task<T> GetDataAsync<T>(string url) where T : class
    {
        var rawData = await innerWebClient.DownloadStringTaskAsync(url);
        if (!string.IsNullOrWhiteSpace(rawData))
        {
            return JsonConvert.DeserializeObject<T>(rawData);
        }

        return null;
    }

    public async Task<long> DownloadPartialFileAsync(
        string url,
        string filename,
        long from,
        long to,
        Dictionary<string, string> requestHeaders,
        CancellationToken cancelToken,
        Action<long, int, long> progressAction,
        bool useRangeHeader)
    {
        long result = -1;
        var streamSize = to - from + 1;
        using (var handler = new HttpClientHandler())
        {
            handler.UseCookies = false;
            using (var httpClient = new HttpClient(handler, false))
            {
                httpClient.Timeout = TimeSpan.FromMinutes(2);
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    if (requestHeaders != null)
                    {
                        foreach (var header in requestHeaders)
                        {
                            request.Headers.Add(header.Key, new[] { header.Value });
                        }
                    }
                    else
                    {
                        foreach (var key in innerWebClient.Headers.AllKeys)
                        {
                            request.Headers.Add(key, innerWebClient.Headers[key]);
                        }
                    }

                    if (useRangeHeader)
                    {
                        request.Headers.Range = new RangeHeaderValue(from, to);
                    }

                    using (HttpResponseMessage response = await httpClient.SendAsync(
                               request,
                               HttpCompletionOption.ResponseHeadersRead,
                               cancelToken))
                    {
                        if (response.Headers.TryGetValues("Content-Range",
                                out IEnumerable<string> contentRange))
                        {
                            var contentRangeText = contentRange.FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(contentRangeText))
                            {
                                var contentRangeParts = contentRangeText.Split('/');
                                if (contentRangeParts.Length == 2)
                                {
                                    result = long.Parse(contentRangeParts[1].Trim());
                                }
                            }
                        }

                        using (var inputStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var outputStream = fileOperations.CreateFileStream(
                                       filename,
                                       FileMode.Create))
                            {
                                var buffer = new byte[65535];
                                int byteCountRead;
                                long totalByteCountRead = 0;
                                do
                                {
                                    byteCountRead = await inputStream.ReadAsync(
                                        buffer,
                                        0,
                                        buffer.Length,
                                        cancelToken);
                                    if (byteCountRead > 0)
                                    {
                                        await outputStream.WriteAsync(buffer, 0, byteCountRead, cancelToken);
                                        totalByteCountRead += byteCountRead;
                                        if (totalByteCountRead > streamSize)
                                        {
                                            throw new Exception(
                                                "Server returned too much data for partial range request.");
                                        }

                                        progressAction?.Invoke(streamSize, byteCountRead, totalByteCountRead);
                                    }
                                } while (!cancelToken.IsCancellationRequested && byteCountRead > 0);

                                if (totalByteCountRead < streamSize)
                                {
                                    throw new Exception(
                                        $"Failed to read entire stream from server: {totalByteCountRead} of {streamSize} retrieved.");
                                }
                            }

                            if (cancelToken.IsCancellationRequested)
                            {
                                fileOperations.DeleteFile(filename);
                            }

                            return result;
                        }
                    }
                }
            }
        }
    }

    public async Task<long> GetContentLengthAsync(string url, CancellationToken cancelToken)
    {
        long streamSize;
        try
        {
            using (var handler = new HttpClientHandler())
            {
                handler.UseCookies = false;
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, url))
                    {
                        using (HttpResponseMessage response = await httpClient.SendAsync(
                                   request,
                                   HttpCompletionOption.ResponseHeadersRead,
                                   cancelToken))
                        {
                            streamSize = response.Content.Headers.ContentLength.GetValueOrDefault(-1);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            streamSize = -1;
        }

        return streamSize;
    }

    public void SetRequestHeaders(IReadOnlyDictionary<string, string> headers)
    {
        innerWebClient.RequestHeaders.Clear();
        UpdateRequestHeaders(headers);
    }

    public void UpdateRequestHeaders(IReadOnlyDictionary<string, string> headers)
    {
        var existingKeys = innerWebClient.RequestHeaders.Keys;
        foreach (var kvp in headers)
        {
            try
            {
                if (!IsRestrictedHeader(kvp.Key))
                {
                    if (!existingKeys.Contains(kvp.Key))
                    {
                        innerWebClient.RequestHeaders.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        innerWebClient.RequestHeaders[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing - some headers can't be set - this "cleans" them out.
            }
        }
    }

    public void SetRequestHeader(string name, string value)
    {
        if (!IsRestrictedHeader(name))
        {
            innerWebClient.RequestHeaders[name] = value;
        }
    }

    private bool IsRestrictedHeader(string name)
    {
        //var doNotCopyHeaders = new[] { "host", "connection", "range", "accept-encoding", "accept-language", "content-length" }; //"accept"
        //return doNotCopyHeaders.FirstOrDefault(x => x == name.ToLower()) != null;
        return false;
    }
}