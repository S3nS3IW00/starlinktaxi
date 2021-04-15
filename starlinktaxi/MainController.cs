using starlinktaxi.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace starlinktaxi
{
    public class MainController : Bindable
    {

        private bool hasSaveGame = false;

        public bool HasSaveGame { get => hasSaveGame; set { hasSaveGame = value; ControlPropertyChanged(); } }

        public void CheckSaveGame()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".starlinktaxi", "save.json");
            FileInfo fi = new FileInfo(path);
            HasSaveGame = fi.Exists;
        }

    }
}
