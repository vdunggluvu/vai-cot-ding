using System;
using System.Threading;
using System.Threading.Tasks;
using MagicMouseClone.Core;

namespace MagicMouseClone.App
{
    public class FakeDeviceProvider
    {
        private readonly IGestureEngine _gestureEngine;
        private CancellationTokenSource? _cts;

        public FakeDeviceProvider(IGestureEngine gestureEngine)
        {
            _gestureEngine = gestureEngine;
        }

        public void SimulateConnection()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        // Simulate a scroll down gesture
                        // Frame 1: Finger down
                        _gestureEngine.ProcessTouchFrame(new TouchFrame
                        {
                            Points = new[] { new TouchPoint { Id = 1, X = 0.5f, Y = 0.5f, IsDown = true } },
                            Timestamp = DateTime.Now.Ticks
                        });

                        await Task.Delay(16); // 60fps

                        // Frame 2: Finger move
                        _gestureEngine.ProcessTouchFrame(new TouchFrame
                        {
                            Points = new[] { new TouchPoint { Id = 1, X = 0.5f, Y = 0.55f, IsDown = true } }, // moved down 0.05
                            Timestamp = DateTime.Now.Ticks
                        });

                         await Task.Delay(1000); // Wait 1 sec between gestures
                    }
                }
                catch (Exception) { }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }
    }
}
