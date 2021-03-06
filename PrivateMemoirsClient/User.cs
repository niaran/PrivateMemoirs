﻿using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using static PrivateMemoirs.General;

namespace PrivateMemoirsUser
{
    public class User
    {
        public ObservableCollection<Memoir> Memoirs { get; set; } = new ObservableCollection<Memoir>();
        public int TotalMemoir { get; set; } = -1;
        public CurrentMemoirField CurrentField { get; set; } = CurrentMemoirField.NONE;
    }
    public class Memoir : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string text;
        private string title;
        private string date;
        private string id;


        public string MEMOIR_TITLE
        {
            get
            {
                if (title != null && title.Length > 20)
                    return title.Remove(20, title.Length - 20);
                else
                    return title;
            }
            set { title = value; RaisePropertyChanged("MEMOIR_TITLE"); }
        }
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
        public string MEMOIR_ID
        {
            get { return id; }
            set { id = value; }
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