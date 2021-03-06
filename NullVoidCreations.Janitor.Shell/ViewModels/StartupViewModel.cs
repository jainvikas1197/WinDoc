﻿using System.Collections.ObjectModel;
using NullVoidCreations.Janitor.Shared.Base;
using NullVoidCreations.Janitor.Shell.Core;
using NullVoidCreations.Janitor.Shell.Models;

namespace NullVoidCreations.Janitor.Shell.ViewModels
{
    public class StartupViewModel : ViewModelBase, ISignalObserver
    {
        readonly CommandBase _refresh, _delete;

        public StartupViewModel()
        {
            _refresh = new AsyncDelegateCommand(this, null, ExecuteRefresh, RefreshCompleted);
            _delete = new AsyncDelegateCommand(this, CanDelete, ExecuteDelete, DeleteComplete, ConfirmDelete);
            _refresh.IsEnabled = _delete.IsEnabled = true;

            SignalHost.Instance.AddObserver(this);
        }

        ~StartupViewModel()
        {
            SignalHost.Instance.RemoveObserver(this);
        }

        #region properties

        public ObservableCollection<StartupEntryModel> Entries
        {
            get { return GetValue<ObservableCollection<StartupEntryModel>>("Entries"); }
            private set { this["Entries"] = value;}
        }

        public StartupEntryModel SelectedEntry
        {
            get { return GetValue<StartupEntryModel>("SelectedEntry"); }
            set
            {
                this["SelectedEntry"] = value;
                Delete.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region commands

        public CommandBase Refresh
        {
            get { return _refresh; }
        }

        public CommandBase Delete
        {
            get { return _delete; }
        }

        #endregion

        bool CanDelete(object parameter)
        {
            return SelectedEntry != null;
        }

        bool ConfirmDelete(object parameter)
        {
            return UiHelper.Instance.Question("Are you sure you want to remove program {0} from startup?", SelectedEntry.Name);
        }

        void DeleteComplete(object result)
        {
            if ((bool)result)
            {
                Entries.Remove(SelectedEntry);
                SelectedEntry = null;
            }
            else
                UiHelper.Instance.Error("An error occured while removing program {0} from startup.", SelectedEntry.Name);
        }

        object ExecuteDelete(object parameter)
        {
            var entry = parameter as StartupEntryModel;
            if (entry == null)
                return false;

            return entry.RemoveEntry();
        }

        object ExecuteRefresh(object parameter)
        {
            SignalHost.Instance.RaiseSignal(Signal.StartupEntriesLoadStarted);

            var entries = new ObservableCollection<StartupEntryModel>();
            foreach (var entry in StartupEntryModel.GetStartupEntries())
                entries.Add(entry);

            SignalHost.Instance.RaiseSignal(Signal.StartupEntriesLoadStopped);

            return entries;
        }

        void RefreshCompleted(object startupEntries)
        {
            SelectedEntry = null;
            Entries = startupEntries as ObservableCollection<StartupEntryModel>;
        }

        public void SignalReceived(Signal signal, params object[] data)
        {
            switch (signal)
            {
                case Signal.Initialized:
                    Refresh.Execute(Entries);
                    break;
            }
        }
    }
}
