﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace V2RayGCon.Services.ShareLinkComponents
{
    internal sealed class VeeDecoder :
        VgcApis.BaseClasses.ComponentOf<Codecs>,
        VgcApis.Interfaces.IShareLinkDecoder
    {
        Cache cache;
        Settings setting;


        public VeeDecoder(
            Cache cache,
            Settings setting)
        {
            this.cache = cache;
            this.setting = setting;
        }

        #region properties

        #endregion

        #region public methods
        public override void Prepare()
        {
            var v0a = new VeeCodecs.Vee0a(cache);
            var v1a = new VeeCodecs.Vee1a(cache);

            AddChild(v0a);
            AddChild(v1a);
        }

        public Tuple<JObject, JToken> Decode(string shareLink)
        {
            string message = null;
            try
            {
                return DecodeWorker(shareLink);
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            if (!string.IsNullOrEmpty(message))
            {
                setting.SendLog(message);
            }
            return null;
        }

        public string Encode(string config)
        {
            string message = null;
            try
            {
                return EncodeWorker(config);
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            if (!string.IsNullOrEmpty(message))
            {
                setting.SendLog(message);
            }
            return null;
        }

        public List<string> ExtractLinksFromText(string text) =>
            Misc.Utils.ExtractLinks(
                text,
                VgcApis.Models.Datas.Enum.LinkTypes.v);
        #endregion

        #region private methods
        Tuple<JObject, JToken> DecodeWorker(string shareLink)
        {
            var bytes = VeeLink2Bytes(shareLink);
            var linkVersion = VgcApis.Libs.Streams.BitStream.ReadVersion(bytes);

            foreach (var component in GetChildren())
            {
                var decoder = component as VeeCodecs.IVeeDecoder;
                if (decoder.GetSupportedVersion() == linkVersion)
                {
                    return decoder.Bytes2Config(bytes);
                }
            }

            throw new NotSupportedException(
                $"Not supported vee share link version {linkVersion}");
        }

        string EncodeWorker(string config)
        {
            if (!VgcApis.Misc.Utils.TryParseJObject(
                config, out JObject json))
            {
                return null;
            }

            VeeCodecs.IVeeDecoder encoder;
            var protocol = Misc.Utils.GetProtocolFromConfig(json);
            switch (protocol)
            {
                case VgcApis.Models.Consts.Config.ProtocolNameVmess:
                    encoder = GetChild<VeeCodecs.Vee0a>();
                    break;
                case VgcApis.Models.Consts.Config.ProtocolNameSs:
                    encoder = GetChild<VeeCodecs.Vee1a>();
                    break;
                default:
                    return null;
            }

            var bytes = encoder?.Config2Bytes(json);
            return Bytes2VeeLink(bytes);
        }

        byte[] VeeLink2Bytes(string veeLink)
        {
            // Do not use Misc.Utils.Base64Decode() 
            // Unicode encoder can not handle all possible byte values.

            string b64Body = Misc.Utils.GetLinkBody(veeLink);
            string b64Padded = Misc.Utils.Base64PadRight(b64Body);
            return Convert.FromBase64String(b64Padded);
        }

        string Bytes2VeeLink(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(
                    @"Bytes is null!");
            }

            var b64Str = Convert.ToBase64String(bytes);
            return Misc.Utils.AddLinkPrefix(
                b64Str,
                VgcApis.Models.Datas.Enum.LinkTypes.v);
        }

        #endregion

        #region protected methods
        #endregion
    }
}
