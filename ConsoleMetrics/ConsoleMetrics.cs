using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace ConsoleMetrics
{
    public class ConsoleMetrics : RocketPlugin
    {
        public string InitialTitle;
        public bool IsLoading = true;

        public override void LoadPlugin()
        {
            base.LoadPlugin();
            InitialTitle = Console.Title;

            var t = new Thread(async () => await TitleUpdate());
            t.IsBackground = true;
            t.Start();

            Level.onLevelLoaded += level;
        }

        public void level(int l)
        {
            if (l > 0)
            {
                IsLoading = false;
            }
        }

        private int c = 0;

        public void FixedUpdate()
        {
            c++;
        }

        public int MB
        {
            get
            {
                long priv = Profiler.GetMonoHeapSizeLong() + Profiler.usedHeapSizeLong;
                return (int)Math.Round(priv / (1024 * 1024f), 0);
            }
        }

        public int MB_Mono
        {
            get
            {
                long priv = Profiler.GetMonoHeapSizeLong();
                return (int)Math.Round(priv / (1024 * 1024f), 0);
            }
        }

        public int MB_Framework
        {
            get
            {
                long priv = Profiler.usedHeapSizeLong;
                return (int)Math.Round(priv / (1024 * 1024f), 0);
            }
        }

        private List<int> m_Times = new List<int>();

        private int SU()
        {
            m_Times.Add(c);
            c = 0;

            if (m_Times.Count > 2)
            {
                m_Times.RemoveAt(0);
            }

            return (int)Math.Round(m_Times.Average(), 0);
        }

        private async Task TitleUpdate()
        {
            while (true)
            {
                await Task.Delay(1000);
                Console.Title = $"{InitialTitle}   {(IsLoading ? "[Loading]" : "[Loaded]")}   Players: {Provider.clients.Count}   TPS: {SU()}   Memory: {MB}mb (Mono: {MB_Mono}mb, .NET: {MB_Framework}mb)";
            }
        }
    }
}