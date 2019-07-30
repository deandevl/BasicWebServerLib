using System.IO;
using System.Text;

namespace BasicWebServerLib.WsCommon {
  /*
   * see http://tools.ietf.org/html/rfc6455 for specification
   * see fragmentation section for sending multipart messages
   * EXAMPLE: For a text message sent as three segments,
   *   the first fragment would have an opcode of TextFrame and isLastFrame false,
   *   the second fragment would have an opcode of ContinuationFrame and isLastFrame false,
   *   the third fragment would have an opcode of ContinuationFrame and isLastFrame true.
   */
  public class WsFrameWriter {
    private readonly Stream _stream;

    public WsFrameWriter(Stream stream) {
      _stream = stream;
    }

    public void Write(WsOpCode opCode, byte[] payload, bool isLastFrame) {
      //best to write everything to a memory stream before we push it onto the wire
      //not really necessary but good advice
      using (MemoryStream memoryStream = new MemoryStream()) {
        byte finBitSetAsByte = isLastFrame ? (byte) 0x80 : (byte) 0x00;
        byte byte1 = (byte) (finBitSetAsByte | (byte) opCode);
        memoryStream.WriteByte(byte1);
        
        //Don't set the mask flag.  No need to mask data from server to client
        //Depending on the size of the length we want to write it as a byte, ushort, or ulong.
        if (payload.Length < 126) {
          memoryStream.WriteByte((byte) payload.Length);
        }else if (payload.Length <= ushort.MaxValue) {
          memoryStream.WriteByte(126);
          BinaryReaderWriter.WriteUShort((ushort)payload.Length,memoryStream,false);
        }else {
          memoryStream.WriteByte(127);
          BinaryReaderWriter.WriteULong((ulong)payload.Length,memoryStream,false);
        }

        memoryStream.Write(payload, 0, payload.Length);
        byte[] buffer = memoryStream.ToArray();
        _stream.Write(buffer, 0, buffer.Length);
      }
    }

    public void Write(WsOpCode opCode, byte[] payload) {
      Write(opCode, payload, true);
    }

    public void WriteText(string text) {
      byte[] responseBytes = Encoding.UTF8.GetBytes(text);
      Write(WsOpCode.TextFrame, responseBytes);
    }
  }
}