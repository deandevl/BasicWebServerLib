
using System.Net.Sockets;

namespace BasicWebServerLib.WsCommon {
  public class WsConnectionDetails {
    public string PathOrFileName { get; private set; }
    public NetworkStream Stream { get; private set; }

    public WsConnectionDetails(string pathOrFileName, NetworkStream stream) {
      PathOrFileName = pathOrFileName;
      Stream = stream;
    }
  }
}