using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace starlinktaxi.util
{
    public class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void ControlPropertyChanged([CallerMemberName] string tulajdonsagNev = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(tulajdonsagNev));
            }
        }
    }
}
