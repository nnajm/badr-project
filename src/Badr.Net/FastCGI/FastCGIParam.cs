using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badr.Net.FastCGI
{
    public class FastCGIParam
    {
        public readonly string Name;
        public readonly string Value;
        public readonly int ParamEndOffset;

        public FastCGIParam(byte[] buffer, int offset)
        {
            int nameLength = GetNextLength(buffer, ref offset);
            int valueLength = GetNextLength(buffer, ref offset);

            Name = Encoding.ASCII.GetString(buffer, offset, nameLength);
            if (valueLength > 0)
                Value = Encoding.ASCII.GetString(buffer, offset + nameLength, valueLength);
            else
                Value = "";

            ParamEndOffset = offset + nameLength + valueLength;
        }

        private int GetNextLength(byte[] buffer, ref int offset)
        {
            int length = buffer[offset];
            if (length >> 7 == 1)
            {
                length = ((length & 0x7F) << 24) + (buffer[offset + 1] << 16) + (buffer[offset + 2] << 8) + (buffer[offset + 3]);
                offset += 4;
            }
            else
                offset += 1;

            return length;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, Value);
        }
    }
}
