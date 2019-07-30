using System;
using BasicWebServerLib.WsCommon;


namespace BasicWebServerLib.Events {
  public class WsTextFrameEventArgs : EventArgs {
    public object Message {get;private set;}
    public WsConnectionDetails Details { get; private set; }
    
    public WsTextFrameEventArgs(object message, WsConnectionDetails details) {
      Message = message;
      Details = details;
    }
  }
}