using MainSpace.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MainSpace.Controllers
{
    public static class MainTextBoxController
    {
        public static Dispatcher mainWindowDispatcher;
        public static TextBox _mainBox;

        public static void Reset()
        {
            mainWindowDispatcher.Invoke(new Action(() => _mainBox.Background = Brushes.White));
            mainWindowDispatcher.Invoke(new Action(() => _mainBox.Text = TimeSpan.Zero.ToString(@"hh\:mm\:ss\.fff")));
        }

        public static void InvokeMain(TimeSpan activeTime, TimeSpan timeLeft)
        {
            InvokeTimeBoxColor(activeTime, timeLeft);
            InvokeTimeBoxText(timeLeft);
        }

        public static void InvokeTimeBoxText(TimeSpan timeLeft) => mainWindowDispatcher.Invoke(new Action(() => _mainBox.Text = timeLeft.ToString(@"hh\:mm\:ss\.fff")));
        public static void InvokeTimeBoxColor(TimeSpan activeTime, TimeSpan timeLeft) => mainWindowDispatcher.Invoke(new Action(() => _mainBox.Background = BrushController.MakeBrush(activeTime, timeLeft)));

        public static void SetBrush(ActionType mode)
        {
            BrushController.SetTargetBrush(mode);
        }
    }

    internal static class BrushController
    {
        private static SolidColorBrush _targetBrush;
        private static List<SolidColorBrush> targetBrushes = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Yellow),
            new SolidColorBrush(Colors.Green)
        };

        public static SolidColorBrush MakeBrush(TimeSpan activeTime, TimeSpan timeLeft)
        {
            byte div = (byte)(255 - (activeTime - timeLeft) / activeTime * 255);
            if (div > 180)
            {
                return Brushes.White;
            }

            var returnBrush = new SolidColorBrush(Color.Subtract(_targetBrush.Color, Color.FromArgb(div, 0, 0, 0)));

            return returnBrush;
        }

        public static void SetTargetBrush(ActionType mode)
        {
            _targetBrush = targetBrushes[(int)mode];
        }
    }
}
