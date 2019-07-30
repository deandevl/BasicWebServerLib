

using System;
using System.IO;
using System.Net.Sockets;

namespace BasicWebServerLib.WsCommon {
  public class WsFrameReader {
    private byte[] _buffer;
    
    public WsFrameReader() {
      _buffer = new byte[1024 * 64];
    }

    public WsFrame Read(Stream stream, Socket socket) {
      byte byte1;
      try {
        byte1 = (byte) stream.ReadByte();
      } catch (IOException) {
        if (socket.Connected) {
          throw;
        } else {
          return null;
        }
      }
      
      //process first first byte
      byte finBitFlag = 0x80;
      byte opCodeFlag = 0x0F;
      bool isFinBitSet = (byte1 & finBitFlag) == finBitFlag;
      WsOpCode opCode = (WsOpCode) (byte1 & opCodeFlag);
      
      //reade and process second byte
      byte byte2 = (byte) stream.ReadByte();
      byte maskFlag = 0x80;
      bool isMaskBitSet = (byte2 & maskFlag) == maskFlag;
      uint len = ReadLength(byte2, stream);
      byte[] decodePayload;
      
      //use the masking key to decode the data if needed
      if (isMaskBitSet) {
        const int maskKeyLen = 4;
        byte[] maskKey = BinaryReaderWriter.ReadExactly(maskKeyLen, stream);
        byte[] encodePayload = BinaryReaderWriter.ReadExactly((int) len, stream);
        decodePayload = new byte[len];
        
        //apply the mask key
        for (int i = 0; i < encodePayload.Length; i++) {
          decodePayload[i] = (byte) (encodePayload[i] ^ maskKey[i % maskKeyLen]);
        }
      } else {
        decodePayload = BinaryReaderWriter.ReadExactly((int) len, stream);
      }
      WsFrame frame = new WsFrame(isFinBitSet,opCode,decodePayload,true);
      return frame;
    }

    private static uint ReadLength(byte byte2, Stream stream) {
      byte payloadLenFlag = 0x7F;
      uint len = (uint) (byte2 & payloadLenFlag);
      
      //read a short length or a long length depending on the value of len
      if (len == 126) {
        len = BinaryReaderWriter.ReadUShortExactly(stream, false);
      }else if (len == 127) {
        len = (uint) BinaryReaderWriter.ReadULongExactly(stream, false);
        const uint maxLen = 2147483648; //2GB
        //protect ourselves against bad data
        if (len > maxLen) {
          throw new ArgumentOutOfRangeException(string.Format("Payload length out of range. Min 0 Max 2GB. Actual {0:#,##0} bytes.",len));
        }
      }
      return len;
    }
  }
}