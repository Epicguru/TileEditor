using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TileEditor
{
    public static class FileDialogeOpener
    {
        public static bool AnyOpen { get { return OpenCount > 0; } }
        public static int OpenCount { get; private set; }

        public static void OpenDialoge(ThreadedEvent uponCompleted)
        {
            OpenCount++;
            Thread t = new Thread(() =>
            {
                var d = new OpenFileDialog();

                d = new OpenFileDialog();
                d.Filter = "Tile Sprite |*.png";
                var result = d.ShowDialog();
                string path = d.FileName;

                Main.Dispatcher.AddNew(uponCompleted, result, path);
                OpenCount--;
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}
