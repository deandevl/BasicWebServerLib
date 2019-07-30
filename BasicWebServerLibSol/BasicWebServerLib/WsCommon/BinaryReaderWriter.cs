using System;
using System.IO;

namespace BasicWebServerLib.WsCommon {
  public class BinaryReaderWriter {
    
    public static byte[] ReadExactly(int length, Stream stream) {
      byte[] buffer = new byte[length];
      if (length == 0) {
        return buffer;
      }

      int offset = 0;
      do {
        int bytes_read = stream.Read(buffer, offset, length - offset);
        if (bytes_read == 0) {
          throw new EndOfStreamException(
            string.Format("Unexpected endo of stream encountered whilst attempting to read {0:#,##0} bytes", length));
        }

        offset += bytes_read;
      } while (offset < length);

      return buffer;
    }

    public static ushort ReadUShortExactly(Stream stream, bool isLittleEndian) {
      byte[] len_buffer = ReadExactly(2, stream);
      if (!isLittleEndian) {
        Array.Reverse(len_buffer); //big endian
      }

      return BitConverter.ToUInt16(len_buffer, 0);
    }

    public static ulong ReadULongExactly(Stream stream, bool isLittleEndian) {
      byte[] len_buffer = ReadExactly(8, stream);
      if (!isLittleEndian) {
        Array.Reverse(len_buffer); //big endian
      }

      return BitConverter.ToUInt64(len_buffer, 0);
    }

    public static long ReadLongExactly(Stream stream, bool isLittleEndian) {
      byte[] len_buffer = ReadExactly(8, stream);
      if (!isLittleEndian) {
        Array.Reverse(len_buffer);
      }

      return BitConverter.ToInt64(len_buffer, 0);
    }

    public static void WriteULong(ulong value, Stream stream, bool isLittleEndian) {
      byte[] buffer = BitConverter.GetBytes(value);
      if (BitConverter.IsLittleEndian && !isLittleEndian) {
        Array.Reverse(buffer);
      }
      stream.Write(buffer,0,buffer.Length);
    }

    public static void WriteUShort(ushort value, Stream stream, bool isLittleEndian) {
      byte[] buffer = BitConverter.GetBytes(value);
      if (BitConverter.IsLittleEndian && !isLittleEndian) {
        Array.Reverse(buffer);
      }
      stream.Write(buffer,0,buffer.Length);
    }
  }
}