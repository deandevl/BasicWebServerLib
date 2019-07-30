namespace BasicWebServerLib.WsCommon {
  public enum WebSocketCloseCode {
    Normal = 1000,
    GoingAway = 1001,
    ProtocolError = 1002,
    DataTypeNotSupported = 1003,
    Reserved = 1004,
    ReservedNoStatusCode = 1005,
    ReservedAbnormalClosure = 1006,
    MismatchDataNonUtf8 = 1007,
    ViolationOfPolicy = 1008,
    MessageTooLarge = 1009,
    EndpointExpectsExtension = 1010,
    ServerUnexpectedCondition = 1011,
    ServerRegectTlsHandshake = 1015,
  }
}