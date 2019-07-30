using System;
using BasicWebServerLib.HttpCommon;

namespace BasicWebServerLib.Events {
  public class HttpRequestEventArgs : EventArgs {
    public object Body  {get;private set;}
    public HttpConnectionDetails Details {get;private set;}

    public HttpRequestEventArgs(object body, HttpConnectionDetails details) {
      Body = body;
      Details = details;
    }
  }
}