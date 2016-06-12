using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using static PrivateMemoirs.General;

namespace PrivateMemoirsUser
{
    public class User
    {
        public ObservableCollection<Memoir> Memoirs { get; set; } = new ObservableCollection<Memoir>();
        public int CurentMemoir { get; set; } = -1;
        public CurrentMemoirField CurrentField { get; set; } = CurrentMemoirField.NONE;
    }
    public class Memoir : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string text;
        private string date;

        public string MEMOIR_TEXT
        {
            get { return text; }
            set { text = value; RaisePropertyChanged("MEMOIR_TEXT"); }
        }
        public string MEMOIR_DATE_CHANGE
        {
            get { return date; }
            set { date = value; RaisePropertyChanged("MEMOIR_DATE_CHANGE"); }
        }

        protected virtual void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}