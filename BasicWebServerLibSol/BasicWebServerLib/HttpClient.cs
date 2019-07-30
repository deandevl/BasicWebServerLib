

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using BasicWebServerLib.Events;
using BasicWebServerLib.HttpCommon;
using Microsoft.Win32;

namespace BasicWebServerLib {
  public class HttpClient {
    private readonly Action<object, HttpRequestEventArgs> _onHttpRequestChanged;
    private readonly Helpers _helpers;
    
    public HttpClient(Action<object, HttpRequestEventArgs> onHttpRequestChanged) {
      _onHttpRequestChanged = onHttpRequestChanged;
      _helpers = new Helpers();
    }

    public void Process(HttpListenerContext context, string baseFolderPath,string[] httpMethods) {
      HttpListenerRequest request = context.Request;
      HttpListenerResponse response = context.Response;
      string httpMethod = request.HttpMethod.ToLower();
      string httpPath = request.RawUrl.Split('?')[0];
      string foundMethod = Array.Find(httpMethods, method => method == httpMethod);
      if(foundMethod != null){
        if(httpPath == "/") {
          string indexPath = Path.Combine(baseFolderPath, "index.html");
          if(File.Exists(indexPath)) {
            string bufferStr = File.ReadAllText(indexPath);
            byte[] buffer = Encoding.UTF8.GetBytes(bufferStr);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            output.Close();
            response.Close();
          }
        } else {
          httpPath = httpPath.Substring(1).Replace("/",@"\");
          string filePath = Path.Combine(baseFolderPath, httpPath);
          string contentType = "";
          if(File.Exists(filePath)) {
            string ext = Path.GetExtension(filePath).TrimStart(".".ToCharArray());
            if(_helpers.MimeTypes.ContainsKey(ext)) {
              contentType = _helpers.MimeTypes[ext];
            } else {
              contentType = GetContentType(ext);
            }
            byte[] buffer = File.ReadAllBytes(filePath);
            response.ContentLength64 = buffer.Length;
            response.ContentType = contentType;
            Stream output = response.OutputStream;
            output.Write(buffer,0,buffer.Length);
            output.Close();
            response.Close();
          } else {
            HttpConnectionDetails details = new HttpConnectionDetails(
              response,
              httpPath,
              httpMethod,
              request.ContentLength64,
              request.ContentType);
            StreamReader reader = new StreamReader(request.InputStream,request.ContentEncoding);
            string body = reader.ReadToEnd();
            request.InputStream.Close();
            reader.Close();
            HttpRequestEventArgs args = new HttpRequestEventArgs(body,details);
            _onHttpRequestChanged(this, args);
          }
        }
      }
    }
    //get mime type from a file extension
    private string GetContentType(string ext) {
      if (Regex.IsMatch(ext, "^[a-z0-9]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled))
        return (Registry.GetValue(@"HKEY_CLASSES_ROOT\." + ext, "Content Type", null) as string) ??
               "application/octet-stream";
      return "application/octet-stream";
    }
  }
}