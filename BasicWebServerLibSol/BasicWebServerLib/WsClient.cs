
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BasicWebServerLib.WsCommon;
using BasicWebServerLib.Events;

namespace BasicWebServerLib {
  public class WsClient {
    private readonly Socket _tcpSocket;
    private readonly IDictionary<string, NetworkStream> _wsClients;
    private readonly NetworkStream _networkStream;
    private readonly WsFrameReader _frameReader;
    private readonly Helpers _helpers;
    private readonly Action<object, WsTextFrameEventArgs> _onTextFrameChanged;

    public WsClient(Socket tcpSocket, IDictionary<string, NetworkStream> wsClients, Action<object,WsTextFrameEventArgs> onTextFrameChanged) {
      _tcpSocket = tcpSocket;
      _wsClients = wsClients;
      _networkStream = new NetworkStream(_tcpSocket,true);
      _onTextFrameChanged = onTextFrameChanged;
      _frameReader = new WsFrameReader();
      _helpers = new Helpers();
    }
    
    public async Task ProcessAsync() {
      string header = _helpers.ReadHeader(_networkStream);
      
      Regex webSocketUpgradeRegex = new Regex("Upgrade: websocket", RegexOptions.IgnoreCase);
      Match webSocketUpgradeRegexMatch = webSocketUpgradeRegex.Match(header);

      if (webSocketUpgradeRegexMatch.Success) {
        await PerformHandshake(header);
        string wspath = header.Split(' ')[1].TrimStart('/');
        WsConnectionDetails details = new WsConnectionDetails(wspath,_networkStream);
        _wsClients.Add(wspath,_networkStream);
        
        while (_tcpSocket.Connected) {
          while (!_networkStream.DataAvailable);
          WsFrame frame = _frameReader.Read(_networkStream, _tcpSocket);
          if (frame.OpCode == WsOpCode.ConnectionClose) {
            _tcpSocket.Close();
            _wsClients.Remove(wspath);
          }else {
            string message = Encoding.UTF8.GetString(frame.DecodePayload);
            WsTextFrameEventArgs args = new WsTextFrameEventArgs(message,details);
            _onTextFrameChanged(this, args);
          }
        }
      }
    }
    
    private async Task PerformHandshake(string header) {
      try {
        Regex webSocketKeyRegex = new Regex("Sec-WebSocket-Key: (.*)");
        Regex webSocketVersionRegex = new Regex("Sec-WebSocket-Version: (.*)");

        // check the version. Support version 13 and above
        const int webSocketVersion = 13;
        int secWebSocketVersion = Convert.ToInt32(webSocketVersionRegex.Match(header).Groups[1].Value.Trim());
        if (secWebSocketVersion < webSocketVersion) {
          throw new Exception(string.Format("WebSocket Version {0} not suported. Must be {1} or above", secWebSocketVersion, webSocketVersion));
        }

        string secWebSocketKey = webSocketKeyRegex.Match(header).Groups[1].Value.Trim();
        string setWebSocketAccept = ComputeSocketAcceptString(secWebSocketKey);
        string response = ("HTTP/1.1 101 Switching Protocols\r\n"
                           + "Connection: Upgrade\r\n"
                           + "Upgrade: websocket\r\n"
                           + "Sec-WebSocket-Accept: " + setWebSocketAccept);

        await _helpers.SendHttpHeaderAsync(response, _networkStream);
        Console.WriteLine("Web Socket handshake sent");
      } catch (Exception) {
        await _helpers.SendHttpHeaderAsync("HTTP/1.1 400 Bad Request", _networkStream);
      }
    }
    
    /// <summary>
    /// Combines the key supplied by the client with a guid and returns the sha1 hash of the combination
    /// </summary>
    private string ComputeSocketAcceptString(string secWebSocketKey) {
      // this is a guid as per the web socket spec
      const string webSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

      string concatenated = secWebSocketKey + webSocketGuid;
      byte[] concatenatedAsBytes = Encoding.UTF8.GetBytes(concatenated);
      byte[] sha1Hash = SHA1.Create().ComputeHash(concatenatedAsBytes);
      string secWebSocketAccept = Convert.ToBase64String(sha1Hash);
      return secWebSocketAccept;
    }
  }
}