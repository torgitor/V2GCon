using NDesk.Options;
using System;
using System.Collections.Generic;

namespace DyFetch.Models
{
    internal class Configs
    {
        public bool headless = false;
        public string pipeIn = null;
        public string pipeOut = null;
        public bool help = false;
        public bool ignoreCertError = false;
        public string proxy = null;
        public List<string> csses = new List<string>();
        public int timeout = -1;
        public string url = null;
        public int wait = -1;

        readonly OptionSet opts = null;

        public Configs(string[] args)
            : this()
        {
            Parse(args);
        }

        public Configs()
        {
            opts = new OptionSet()
            {
                { "pipein=", "anonymous input-pipe handle", v => pipeIn = v },
                { "pipeout=", "anonymous output-pipe handle", v => pipeOut = v },
                { "s|headless", "headless mode", v => headless = v != null },
                { "i|ignore", "ignore certificate errors", v => ignoreCertError = v != null },
                { "u|url=", "the URL to download", v => url = v },
                { "p|proxy=", "HTTP proxy in host:port format", v => proxy = v },
                { "t|timeout=", "wait timeout in milliseconds", (int v) => timeout = v },
                { "c|css=", "wait until one of the css selectors match", v => csses.Add(v) },
                { "w|wait=", "wait after match in milliseconds", (int v) => wait = v },
                { "h|help", "show help", v => help = v != null },
            };
        }

        #region properties

        #endregion

        #region public methods
        public void Parse(string[] args)
        {
            try
            {
                opts.Parse(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void ShowHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("DyFetch v0.0.1 2023-08-21");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine(
                "DyFetch.exe -proxy=\"127.0.0.1:8080\" -t 20000 -u \"https://www.bing.com\""
            );
            Console.WriteLine("");
            Console.WriteLine("Arguments:");
            opts?.WriteOptionDescriptions(Console.Out);
            Console.WriteLine("");
        }
        #endregion

        #region private methods

        #endregion

        #region protected methods

        #endregion
    }
}
