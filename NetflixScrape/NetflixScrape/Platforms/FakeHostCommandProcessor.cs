using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JBlam.NetflixScrape.Core.Models;

namespace JBlam.NetflixScrape.Server.Platforms
{
    class FakeHostCommandProcessor : HostCommandProcessor
    {
        public static FakeHostCommandProcessor TryCreate()
        {
            try
            {
                NativeMethods.DoSomethingToDemonstrateExistence();
                return new FakeHostCommandProcessor();
            }
            catch (DllNotFoundException)
            {
                return null;
            }
        }

        public override void ClickMouse(MouseButton button) => NativeMethods.DoEverything();

        public override void MoveMouse(int x, int y) => NativeMethods.DoEverything();

        public override void SetMouse(int x, int y) => NativeMethods.DoEverything();

        public override void WheelMouse(MouseWheelDirection direction, int amount) => NativeMethods.DoEverything();

        static class NativeMethods
        {
            [DllImport("fake.dll")]
            public static extern void DoEverything();

            [DllImport("fake.dll")]
            public static extern void DoSomethingToDemonstrateExistence();
        }
    }
}
