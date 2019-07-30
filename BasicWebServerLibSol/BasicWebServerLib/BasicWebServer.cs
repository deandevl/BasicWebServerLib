
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BasicWebServerLib.Events;
using BasicWebServerLib.WsCommon;

namespace BasicWebServerLib {
  public class BasicWebServer {
    private Socket _tcpSocket;
    private TcpListener _tcpListener;
    private HttpListener _httpListener;
    private CancellationTokenSource _cts;
    private CancellationToken _ct;
    private readonly int? _tcpPort;
    private readonly string _httpPrefix;
    private readonly string _baseFolderPath;
    private readonly string[] _httpMethods;
    private readonly IDictionary<string, NetworkStream> _wsClients;
    
    public BasicWebServer(string baseFolderPath="", int? tcpPort=54001,string httpPrefix="http://localhost:8080/") {
      _tcpPort = tcpPort;
      _httpPrefix = httpPrefix;
      _wsClients = new Dictionary<string, NetworkStream>();
      _httpMethods = new string[4] {"get", "post", "put", "delete"};
      _cts = new CancellationTokenSource();
      _ct = _cts.Token;
      _ct.Register(() => {
        _httpListener.Stop();
        if(_tcpPort != null) {
          _tcpListener.Stop();

          if(_tcpSocket != null && _tcpSocket.Connected) {
            //if we have active web socket clients send them a close message
            foreach(KeyValuePair<string, NetworkStream> entry in _wsClients) {
              WsFrameWriter frameWriter = new WsFrameWriter(entry.Value);
              //set the close reason to Normal
              BinaryReaderWriter.WriteUShort((ushort)WebSocketCloseCode.Normal, entry.Value, false);
              frameWriter.Write(WsOpCode.ConnectionClose, new byte[1], true);
            }
            _tcpSocket.Shutdown(SocketShutdown.Both);
            _tcpSocket.Close();
          }
        }
        Console.WriteLine("Cancellation has been requested.");
      });
     
      //check baseFolderPath
      if(!Directory.Exists(baseFolderPath)) {
        Console.WriteLine(baseFolderPath + " does not exist.");
        Console.WriteLine("Do you want to create it? (y/n)");
        ConsoleKeyInfo consoleKey = Console.ReadKey();
        if(consoleKey.KeyChar == 'y') {
          Directory.CreateDirectory(baseFolderPath);
          Console.WriteLine();
          Console.WriteLine("Created base folder " + baseFolderPath);
          Console.WriteLine("Press any key to exit server.");
          Console.ReadKey();
        }
      }
      else {
        _baseFolderPath = baseFolderPath;
      }
    }

    public void Start() {
      if(_tcpPort != null) {
        _tcpListener = new TcpListener(IPAddress.Any, (int)_tcpPort);
        _tcpListener.Start();
      }

      _httpListener = new HttpListener();
      _httpListener.Prefixes.Add(_httpPrefix);
      _httpListener.Start();
      
      //start parallel tasks to listen for incoming tcp/http requests
      Task.Factory.StartNew(() => StartTasks(),_ct);
      Console.WriteLine("Listening on http prefix " + _httpPrefix + " ;web socket port " + _tcpPort);
      Console.WriteLine("Press any key to exit.");
      Console.ReadKey();
      _cts.Cancel();
    }
    public void Cancel() {
      _cts.Cancel();
    }
    
    public event EventHandler<WsTextFrameEventArgs> WsTextFrameChanged;
    void OnTextFrameChanged(object sender, WsTextFrameEventArgs e) {
      EventHandler<WsTextFrameEventArgs> handler = WsTextFrameChanged;
      handler?.Invoke(this,e);
    }

    public event EventHandler<HttpRequestEventArgs> HttpRequestChanged;
    void OnHttpRequestChanged(object sender, HttpRequestEventArgs e) {
      EventHandler<HttpRequestEventArgs> handler = HttpRequestChanged;
      handler?.Invoke(this,e);
    }
    
    private async Task StartTasks() {
      ArrayList tasks = new ArrayList();
      if(_tcpPort != null) {
        tasks.Add(TcpListenLoop());
      }
      
      tasks.Add(HttpListenLoop());
      Task[] taskArray = tasks.ToArray(typeof(Task)) as Task[];
      await Task.WhenAll(taskArray);
    }
    
    private async Task TcpListenLoop(){
      while (true) {
        //wait for connection
        _tcpSocket = await _tcpListener.AcceptSocketAsync();
        //got new tcp connection, create a client handler for it  
        WsClient wsClient = new WsClient(_tcpSocket, _wsClients, OnTextFrameChanged);
        await wsClient.ProcessAsync();  
        _ct.ThrowIfCancellationRequested();
      }
    }
    
    private async Task HttpListenLoop() {
      while(true) {
        //wait for connection
        HttpListenerContext httpContext = await _httpListener.GetContextAsync();
        //got a new http connection, create a client handler for it
        HttpClient httpClient = new HttpClient(OnHttpRequestChanged);
        httpClient.Process(httpContext, _baseFolderPath,_httpMethods);
        _ct.ThrowIfCancellationRequested();
      }
    }
  }
}