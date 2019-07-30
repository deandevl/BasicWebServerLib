

namespace BasicWebServerLib.WsCommon {
  public class WsFrame {
    public bool IsFinBitSet { get; private set; }
    public WsOpCode OpCode { get; private set; }
    public byte[] DecodePayload { get; private set; }
    public bool IsValid { get; private set; }

    public WsFrame(bool isFinBitSet, WsOpCode wsOpCode, byte[] decodePayload, bool isValid) {
      IsFinBitSet = isFinBitSet;
      OpCode = wsOpCode;
      DecodePayload = decodePayload;
      IsValid = isValid;
    }
  }
}