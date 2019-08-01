## BasicWebServer

**BasicWebServer** is a C# dll library that can provide basic web server for local rendering of html/script files and accept/respond to web socket requests.  The example below shows how a instance of **BasicWebServer** is performed:

```
BasicWebServer server = new BasicWebServer(baseFolderPath: 'c:\\server',tcpPort: 54001, httpPrefix: 'http://localhost:8081');
      server.WsTextFrameChanged += WsTextFrameChanged;
      server.HttpRequestChanged += HttpRequestChanged;
      
      server.Start();
```

With the following parameters:

- baseFolderPath -- the folder where you will be running the server from
- tcpPort -- the port number for the web socket (default is 54001)
- httpPrefix -- the http prefix for the server to listen to (default is `http://localhost:8080`)

After creating an instance, call the **BasicWebServer's** `Start()` function to start listening for client requests.  

The assignment of optional callback functions for the web socket (`server.WsTextFrameChanged`) and http (`server.HttpRequestChanged`) requests is central in configuring the server.  The callbacks are responding to a custom event whose arguments extend the [EventArgs](https://docs.microsoft.com/en-us/dotnet/api/system.eventargs?view=netframework-4.7.1) class.  For example the `HttpRequestChanged` in the above example is defined as follows for the **EmployeeWebServerApp**:

```
    private void HttpRequestChanged(object sender, EventArgs args) {
      HttpRequestEventArgs httpArgs = (HttpRequestEventArgs) args;
      _httpDetails = httpArgs.Details;
      string body = (string)httpArgs.Body;
      _requestDictionary = _serializer.Deserialize<Dictionary<string, object>>(body);
      
      if(_httpDetails.HttpPath == "employee") {
        _actions[(string)_requestDictionary["action"]]();
      }
    }
```

The `args` parameter is an instance of the custom event argument `HttpRequestEventArgs` that contains two important pieces of information:

- `HttpRequestEventArgs.Details` -- a `HttpConnectionDetails` class with the following public members about the http request:

  ```
  public class HttpConnectionDetails {
      public HttpListenerResponse Response {get;private set;}
      public string HttpPath {get;private set;}
      public string Method {get;private set;}
      public long ContentLength {get;private set;}
      public string ContentType {get;private set;}
  }    
  ```

  - `HttpRequestEventArgs.Body` --  contains the body of the http request.  The `body` variable is a Json string which in this **EmployeeWebServerApp**:we are using an instance of the [JavaScriptSerializer Class](https://docs.microsoft.com/en-us/dotnet/api/system.web.script.serialization.javascriptserializer?view=netframework-4.8) to deserialize to a `<Dictionary<string, object>` type.

The [**EmployeeWebServer**](https://github.com/deandevl/EmployeeWebServerApp) console application demos the server with a [LiteDB](https://github.com/mbdavid/LiteDB) database and simple html on the client side with CRUD operations via http and web socket.

A `setup.exe` file is provided for installing/uninstalling the library in a folder of your choice.