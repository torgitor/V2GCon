﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VgcApis.Libs.Streams
{
    public class StringReader
    {
        private readonly Stream stream;

        public StringReader(Stream stream)
        {
            this.stream = stream;
        }

        #region private methods

        #endregion

        #region public methods
        public string Read()
        {
            var lenBuff = new byte[sizeof(int)];
            var readed = 0;
            while (readed < lenBuff.Length)
            {
                var c = stream.Read(lenBuff, readed, lenBuff.Length - readed);
                if (c == 0)
                {
                    return null;
                }
                readed += c;
            }
            var len = BitConverter.ToInt32(lenBuff, 0);
            if (len == 0)
            {
                return string.Empty;
            }
            var buff = new byte[len];
            readed = 0;
            while (readed < len)
            {
                var c = stream.Read(buff, readed, len - readed);
                if (c == 0 && readed < len)
                {
                    return null;
                }
                readed += c;
            }
            return Encoding.Unicode.GetString(buff);
        }
        #endregion
    }

}