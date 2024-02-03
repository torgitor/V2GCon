﻿using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace V2RayGCon.Services.ShareLinkComponents
{
    public sealed class Codecs : VgcApis.BaseClasses.ComponentOf<Codecs>
    {
        Settings settings;

        public Codecs() { }

        #region vless and trojan
        public string TrojanToConfig(Models.Datas.SharelinkMetadata vee)
        {
            if (vee == null)
            {
                return null;
            }

            var outbSs = Misc.Caches.Jsons.LoadTemplate("outbTrojan");

            outbSs["streamSettings"] = Comm.GenStreamSetting(vee);
            var node = outbSs["settings"]["servers"][0];
            node["address"] = vee.host;
            node["port"] = vee.port;
            node["password"] = vee.auth1;
            node["flow"] = vee.auth2;

            var tpl = Misc.Caches.Jsons.LoadTemplate("tplLogWarn") as JObject;
            return GenerateJsonConfing(tpl, outbSs);
        }

        public string VlessToConfig(Models.Datas.SharelinkMetadata vee)
        {
            if (vee == null)
            {
                return null;
            }

            var outVmess = Misc.Caches.Jsons.LoadTemplate("outbVless");
            outVmess["streamSettings"] = Comm.GenStreamSetting(vee);

            outVmess["protocol"] = "vless";
            var node = outVmess["settings"]["vnext"][0];
            node["address"] = vee.host;
            node["port"] = vee.port;
            node["users"][0]["id"] = vee.auth1;
            if (!string.IsNullOrEmpty(vee.auth2))
            {
                node["users"][0]["flow"] = vee.auth2;
            }
            node["users"][0]["encryption"] = "none";
            var tpl = Misc.Caches.Jsons.LoadTemplate("tplLogWarn") as JObject;
            return GenerateJsonConfing(tpl, outVmess);
        }
        #endregion

        #region public methods

        public string Encode<TDecoder>(string name, string config)
            where TDecoder : VgcApis.BaseClasses.ComponentOf<Codecs>,
                VgcApis.Interfaces.IShareLinkDecoder
        {
            try
            {
                return GetChild<TDecoder>()?.Encode(name, config);
            }
            catch { }
            return null;
        }

        public VgcApis.Models.Datas.DecodeResult Decode(
            string shareLink,
            VgcApis.Interfaces.IShareLinkDecoder decoder
        )
        {
            try
            {
                return decoder.Decode(shareLink);
            }
            catch { }
            return null;
        }

        public VgcApis.Models.Datas.DecodeResult Decode<TDecoder>(string shareLink)
            where TDecoder : VgcApis.BaseClasses.ComponentOf<Codecs>,
                VgcApis.Interfaces.IShareLinkDecoder
        {
            return GetChild<TDecoder>()?.Decode(shareLink);
        }

        public void Run(Settings settings)
        {
            this.settings = settings;
            var ssDecoder = new SsDecoder();
            var v2cfgDecoder = new V2cfgDecoder();
            var vmessDecoder = new VmessDecoder();
            var trojanDecoder = new TrojanDecoder();
            var vlessDecoder = new VlessDecoder();
            var socksDecoder = new SocksDecoder();

            AddChild(vlessDecoder);
            AddChild(trojanDecoder);
            AddChild(ssDecoder);
            AddChild(socksDecoder);
            AddChild(v2cfgDecoder);
            AddChild(vmessDecoder);
        }

        public List<VgcApis.Interfaces.IShareLinkDecoder> GetDecoders(bool isIncludeV2cfgDecoder)
        {
            var r = new List<VgcApis.Interfaces.IShareLinkDecoder>();

            if (settings.CustomDefImportVlessShareLink)
            {
                r.Add(GetChild<VlessDecoder>());
            }

            if (settings.CustomDefImportVmessShareLink)
            {
                r.Add(GetChild<VmessDecoder>());
            }

            if (settings.CustomDefImportSocksShareLink)
            {
                r.Add(GetChild<SocksDecoder>());
            }

            if (settings.CustomDefImportTrojanShareLink)
            {
                r.Add(GetChild<TrojanDecoder>());
            }

            if (settings.CustomDefImportSsShareLink)
            {
                r.Add(GetChild<SsDecoder>());
            }

            if (isIncludeV2cfgDecoder)
            {
                r.Add(GetChild<V2cfgDecoder>());
            }

            return r;
        }
        #endregion

        #region private methods
        public string GenerateJsonConfing(JObject template, JToken outbound)
        {
            if (template == null || outbound == null)
            {
                return null;
            }

            var inb = VgcApis.Misc.Utils.CreateJObject(
                "inbounds.0",
                Misc.Caches.Jsons.LoadTemplate("inbSimSock")
            );

            var outb = VgcApis.Misc.Utils.CreateJObject("outbounds.0", outbound);

            VgcApis.Misc.Utils.MergeJson(template, inb);
            VgcApis.Misc.Utils.MergeJson(template, outb);
            var key = VgcApis.Models.Consts.Config.SectionKeyV2rayGCon;
            template.Remove(key);
            return VgcApis.Misc.Utils.FormatConfig(template);
        }
        #endregion
    }
}
