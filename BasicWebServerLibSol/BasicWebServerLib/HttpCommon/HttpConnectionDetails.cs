using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace BasicWebServerLib.HttpCommon {
  public class HttpConnectionDetails {
    public HttpListenerResponse Response {get;private set;}
    public string HttpPath {get;private set;}
    public string Method {get;private set;}
    public long ContentLength {get;private set;}
    public string ContentType {get;private set;}
    
    public HttpConnectionDetails(
      HttpListenerResponse response, 
      string httpPath, 
      string method,
      long contentLength,
      string contentType
      ) {
      Response = response;
      HttpPath = httpPath;
      Method = method;
      ContentLength = contentLength;
      ContentType = contentType;
    }
  }
}