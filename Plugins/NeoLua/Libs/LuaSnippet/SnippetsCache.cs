﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoLuna.Libs.LuaSnippet
{
    internal sealed class SnippetsCache : VgcApis.BaseClasses.Disposable
    {
        List<LuaKeywordSnippets> keywordCache;
        List<LuaFuncSnippets> functionCache;
        List<LuaSubFuncSnippets> subFunctionCache;
        List<ApiFunctionSnippets> apiFunctionCache;

        List<Dictionary<string, string>> webUiLuaSnippetsCache =
            new List<Dictionary<string, string>>();

        public SnippetsCache()
        {
            GenSnippetCaches();
        }

        #region public methods
        public List<Dictionary<string, string>> GetWebUiLuaStaticSnippets() =>
            webUiLuaSnippetsCache;

        public BestMatchSnippets CreateBestMatchSnippets(ScintillaNET.Scintilla editor)
        {
            return new BestMatchSnippets(
                editor,
                apiFunctionCache,
                functionCache,
                keywordCache,
                subFunctionCache
            );
        }

        #endregion

        #region private methods

        List<string> GenKeywords(IEnumerable<string> initValues) =>
            new StringBuilder(Models.Consts.Lua.LuaModules)
                .Append(@" ")
                .Append(Models.Consts.Lua.NeoLuaKeyWords)
                .ToString()
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Union(initValues)
                .Union(new string[] { "setmetatable(o, {__index = mn})" })
                .OrderBy(e => e)
                .ToList();

        List<LuaFuncSnippets> GenLuaFunctionSnippet()
        {
            var funcs =
                string.Join(" ", Models.Consts.Lua.NeoLuaPredefinedFunctions)
                + " "
                + Models.Consts.Lua.LuaFunctions;

            var r = funcs
                .Replace("dofile", "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .OrderBy(s => s)
                .Select(e =>
                {
                    try
                    {
                        return new LuaFuncSnippets($"{e}()");
                    }
                    catch { }
                    return null;
                })
                .Where(e => e != null)
                .ToList();
            return r;
        }

        List<LuaSubFuncSnippets> GenLuaPredefinedFuncSnippets(
            IEnumerable<LuaSubFuncSnippets> append
        ) =>
            Models.Consts.Lua.LuaPredefinedSubFunctions
                .Select(fn => new LuaSubFuncSnippets(fn, "."))
                .Union(append)
                .ToList();

        List<Dictionary<string, string>> GenWebUiSnippets(List<Tuple<string, Type>> apis)
        {
            /*
            {
                caption: 'kvp', // 匹配关键词
                value: 'for k, v in ipairs(t) do\n    print(k, v)\nend',   // 把匹配到的替换为这个
                score: 100, // 越大越靠前
                meta: "snippet" // 随便写
            }
            */

            Dictionary<string, string> ToSnippetDict1(string s)
            {
                return new Dictionary<string, string>
                {
                    { "caption", s },
                    { "value", s },
                    { "meta", "snippet" },
                };
            }

            Dictionary<string, string> ToKeywordDict(string s)
            {
                return new Dictionary<string, string>
                {
                    { "caption", s },
                    { "value", s },
                    { "meta", "keyword" },
                };
            }

            Dictionary<string, string> ToSnippetDict2(string caption, string value)
            {
                return new Dictionary<string, string>
                {
                    { "caption", caption },
                    { "value", value },
                    { "meta", "snippet" },
                };
            }

            var apiNames = apis.Select(tp => ToKeywordDict(tp.Item1));
            // math.floor()
            var luaSubFunctions = GetLuaSubFunctions();
            var predefinedFunctions = Models.Consts.Lua.LuaPredefinedSubFunctions.Concat(
                Models.Consts.Lua.NeoLuaPredefinedFunctions
            );
            var apiEvents = apis.SelectMany(
                api =>
                    VgcApis.Misc.Utils
                        .GetPublicEventsInfoOfType(api.Item2)
                        .Select(infos => $"{api.Item1}.{infos.Item2}")
            );
            var apiProps = apis.SelectMany(
                api =>
                    VgcApis.Misc.Utils
                        .GetPublicPropsInfoOfType(api.Item2)
                        .Select(infos => $"{api.Item1}.{infos.Item2}")
            );

            var apiFuncs = apis.SelectMany(
                api =>
                    VgcApis.Misc.Utils
                        .GetPublicMethodNameAndParam(api.Item2)
                        .Select(info =>
                        {
                            // void Misc:Stop(int ms)
                            var t1 = $"{info.Item1} {api.Item1}:{info.Item2}({info.Item4})";
                            // Misc:Stop(ms)
                            var t2 = $"{api.Item1}:{info.Item2}({info.Item3})";
                            return new Tuple<string, string>(t1, t2);
                        })
            );

            var snippets = (
                new List<IEnumerable<string>>
                {
                    luaSubFunctions,
                    predefinedFunctions,
                    apiEvents,
                    apiProps,
                }
            )
                .SelectMany(el => el.Select(s => ToSnippetDict1(s)))
                .Concat(apiNames)
                .Concat(apiFuncs.Select(tp => ToSnippetDict2(tp.Item1, tp.Item2)))
                .ToList();

            return snippets;
        }

        IEnumerable<string> GetLuaSubFunctions() =>
            Models.Consts.Lua.LuaSubFunctions
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .OrderBy(s => s)
                .Select(s => $"{s}()");

        List<LuaSubFuncSnippets> GenLuaSubFunctionSnippet() =>
            GetLuaSubFunctions()
                .Select(e =>
                {
                    try
                    {
                        return new LuaSubFuncSnippets(e, ".");
                    }
                    catch { }
                    return null;
                })
                .Where(e => e != null)
                .ToList();

        void GenSnippetCaches()
        {
            var apis = new List<Tuple<string, Type>>
            {
                new Tuple<string, Type>(
                    "mailbox",
                    typeof(VgcApis.Interfaces.PostOfficeComponents.ILuaMailBox)
                ),
                new Tuple<string, Type>(
                    "mail",
                    typeof(VgcApis.Interfaces.PostOfficeComponents.ILuaMail)
                ),
                new Tuple<string, Type>("std.Sys", typeof(Interfaces.ILuaSys)),
                new Tuple<string, Type>("std.Misc", typeof(Interfaces.ILuaMisc)),
                new Tuple<string, Type>("std.Server", typeof(Interfaces.ILuaServer)),
                new Tuple<string, Type>("std.Web", typeof(Interfaces.ILuaWeb)),
                new Tuple<string, Type>("std.Signal", typeof(Interfaces.ILuaSignal)),
                // 2023-08-01 测试结果 Wrap()开销9ns，Invoke()开销2ns，还可以接受。
                new Tuple<string, Type>("wserv", typeof(VgcApis.Interfaces.IWrappedCoreServCtrl)),
                new Tuple<string, Type>("coreServ", typeof(VgcApis.Interfaces.ICoreServCtrl)),
                new Tuple<string, Type>(
                    "coreConfiger",
                    typeof(VgcApis.Interfaces.CoreCtrlComponents.IConfiger)
                ),
                new Tuple<string, Type>(
                    "coreCtrl",
                    typeof(VgcApis.Interfaces.CoreCtrlComponents.ICoreCtrl)
                ),
                new Tuple<string, Type>(
                    "coreState",
                    typeof(VgcApis.Interfaces.CoreCtrlComponents.ICoreStates)
                ),
                new Tuple<string, Type>(
                    "coreLogger",
                    typeof(VgcApis.Interfaces.CoreCtrlComponents.ILogger)
                ),
            };

            webUiLuaSnippetsCache = GenWebUiSnippets(apis);

            keywordCache = GenKeywordSnippetItems(GenKeywords(apis.Select(e => e.Item1)));
            functionCache = GenLuaFunctionSnippet();

            var orgLuaSubFuncSnippet = GenLuaSubFunctionSnippet();

            var apiEvSnippets = apis.SelectMany(api => GenApisEventSnippet(api.Item1, api.Item2));
            var apiPropSnippets = apis.SelectMany(api => GenApisPropSnippet(api.Item1, api.Item2));

            subFunctionCache = apiPropSnippets
                .Concat(apiEvSnippets)
                .Concat(GenLuaPredefinedFuncSnippets(orgLuaSubFuncSnippet))
                .ToList();

            apiFunctionCache = apis.SelectMany(
                    api => GenApiFunctionSnippetItems(api.Item1, api.Item2)
                )
                .ToList();
        }

        List<LuaKeywordSnippets> GenKeywordSnippetItems(IEnumerable<string> keywords) =>
            keywords.OrderBy(k => k).Select(e => new LuaKeywordSnippets(e)).ToList();

        IEnumerable<LuaSubFuncSnippets> GenApisPropSnippet(string apiName, Type type) =>
            VgcApis.Misc.Utils
                .GetPublicPropsInfoOfType(type)
                .OrderBy(infos => infos.Item2)
                .Select(infos => new LuaSubFuncSnippets($"{apiName}.{infos.Item2}", "."));

        IEnumerable<LuaSubFuncSnippets> GenApisEventSnippet(string apiName, Type type) =>
            VgcApis.Misc.Utils
                .GetPublicEventsInfoOfType(type)
                .OrderBy(infos => infos.Item2)
                .Select(infos => new LuaSubFuncSnippets($"{apiName}.{infos.Item2}", "."));

        IEnumerable<ApiFunctionSnippets> GenApiFunctionSnippetItems(string apiName, Type type) =>
            VgcApis.Misc.Utils
                .GetPublicMethodNameAndParam(type)
                .OrderBy(info => info.Item2) // item2 = method name
                .Select(
                    info =>
                        new ApiFunctionSnippets(
                            info.Item1, // return type
                            apiName,
                            info.Item2, // methodName,
                            info.Item3, // paramStr,
                            info.Item4, // paramWithType,
                            @""
                        )
                );

        #endregion

        #region protected methods
        protected override void Cleanup()
        {
            // acm will dispose it self.
            // acm?.Dispose();
        }
        #endregion
    }
}
