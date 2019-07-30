
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BasicWebServerLib.WsCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BasicWebServerLib {
  public class Helpers {
    private readonly Dictionary<int, string> _responseStatus;

    public Helpers() {
      _responseStatus = new Dictionary<int, string>() {
        {200, "HTTP/1.1 200 OK\r\n"},
        {400, "HTTP/1.1 400 Bad Request\r\n"},
        {404, "HTTP/1.1 404 Not Found\r\n"},
        {500, "HTTP/1.1 500  Internal Server Error\r\n"}
      };
    }

    public Dictionary<string, string> MimeTypes = new Dictionary<string, string>() {
      {"js", "text/javascript"}
    };
    
    public string ReadHeader(Stream network_stream) {
      int length = 1024 * 16; // 16KB buffer more than enough for http header
      byte[] buffer = new byte[length];
      int offset = 0;
      int bytesRead = 0;
      do {
        if (offset >= length) {
          throw new Exception("Http header message too large to fit in buffer (16KB)");
        }

        bytesRead = network_stream.Read(buffer, offset, length - offset);
        offset += bytesRead;
        string header = Encoding.UTF8.GetString(buffer, 0, offset);

        // as per http specification, all headers should end this this
        if (header.Contains("\r\n\r\n")) {
          return header;
        }
      } while (bytesRead > 0);
      return string.Empty;
    }

    public async Task SendHttpHeaderAsync(string response, Stream stream) {
      response = response.Trim() + Environment.NewLine + Environment.NewLine;
      Byte[] bytes = Encoding.UTF8.GetBytes(response);
      await stream.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task SendResponseHttpAsync(int status, string response, HttpListenerContext context) {
      HttpListenerResponse http_response = context.Response;
      http_response.AppendHeader("Access-Control-Allow-Origin", "*");
      http_response.StatusCode = status;
      http_response.ContentType = "text/html";
      using (StreamWriter writer = new StreamWriter(http_response.OutputStream, Encoding.UTF8))
        await writer.WriteLineAsync(response);
      http_response.Close();
    }

    public void SendHttpResponse(int status,string statusText, byte[] contentBuffer, string contentType,string serverInfo, HttpListenerResponse httpResponse) {
      httpResponse.StatusCode = status;
      httpResponse.StatusDescription = statusText;
      httpResponse.ContentType = contentType;
      httpResponse.ContentLength64 = contentBuffer.Length;
      httpResponse.Headers.Add("Server",serverInfo);
      httpResponse.Headers.Add("Date",DateTime.Now.ToString());
      httpResponse.Headers.Add("Connection", "close");

      Stream outputStream = httpResponse.OutputStream;
      outputStream.Write(contentBuffer,0,contentBuffer.Length);
      outputStream.Close();
      httpResponse.Close();
    }

    public void SendHttpTextResponse(HttpListenerResponse response, string responseStr) {
      byte[] buffer = Encoding.UTF8.GetBytes(responseStr);
      response.ContentType = "text/plain";
      response.ContentLength64 = buffer.Length;
      Stream output = response.OutputStream;
      output.Write(buffer,0,buffer.Length);
      output.Close();
      response.Close();
    }

    public void SendHttpJsonResponse(HttpListenerResponse response, Dictionary<string, object> responseDic) {
      string responseStr = DictionaryToJson(responseDic);
      byte[] buffer = Encoding.UTF8.GetBytes(responseStr);
      response.ContentType = "application/json";
      response.ContentLength64 = buffer.Length;
      Stream output = response.OutputStream;
      output.Write(buffer,0,buffer.Length);
      output.Close();
      response.Close();
    }
    
    public void SendResponse(int status, string response, Stream stream) {
      string header = _responseStatus[status];
      header += "Date: " + String.Format("{0:ddd, dd MMM yy HH:mm:ss} GMT\r\n", DateTime.Now);
      header += "Server: BasicWebServer\r\n";
      header += "Cache-Control: none\r\n";
      header += "Access-Control-Allow-Origin: *\r\n";
      header += "Content-Type: text/html\r\n";
      header += "Connection: close\r\n";
      header += string.Format("Content-Length: {0}\r\n\r\n", response.Length);
      header += response;

      byte[] bytes = Encoding.UTF8.GetBytes(header);

      stream.Write(bytes, 0, bytes.Length);
      stream.Flush();
      
    }

    public void SendWsText(WsFrameWriter wsFrameWriter, Dictionary<string, object> responseDict) {
      string responseJson = DictionaryToJson(responseDict);
      wsFrameWriter.Write(WsOpCode.TextFrame,Encoding.UTF8.GetBytes(responseJson),true);
    }

    public string ArrayToJson(ArrayList list) {
      return JsonConvert.SerializeObject(list);
    }
    public ArrayList JArrayToArrayList(object jArray) {
      return ((JArray)jArray).ToObject<ArrayList>();
    }
    
    public string DictionaryToJson(Dictionary<string, object> dict) {
      return JsonConvert.SerializeObject(dict);
    }

    public Dictionary<string,object> JsonToDictionary(string json_str) {
      return JsonConvert.DeserializeObject<Dictionary<string, object>>(json_str);
    }

    
  }
}