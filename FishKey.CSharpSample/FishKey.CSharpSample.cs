using System.Windows.Forms;
using static FishKey.Core.Types;

namespace FishKey.CSharpSample
{
    public static class Hotkeys
    {
        [Hotkey("Alt+N")]
        public static void csharpHotkey()
        {
            MessageBox.Show("Alt+N was triggered from a C# hotkey!");
        }
    }
}
