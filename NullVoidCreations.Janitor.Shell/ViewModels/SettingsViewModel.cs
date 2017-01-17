﻿using System;
using System.Collections.ObjectModel;
using Microsoft.Win32.TaskScheduler;
using NullVoidCreations.Janitor.Shared.Base;
using NullVoidCreations.Janitor.Shell.Commands;
using NullVoidCreations.Janitor.Shell.Core;
using NullVoidCreations.Janitor.Shell.Models;

namespace NullVoidCreations.Janitor.Shell.ViewModels
{
    public enum ScheduleType : byte
    {
        None,
        Once,
        Daily,
        Weekly
    }

    public class SettingsViewModel : ViewModelBase
    {
        readonly ObservableCollection<bool> _weekDays;
        readonly CommandBase _scheduleSilentRun, _skipUac, _saveSchedule;
        bool _isScheduleDisabled, _isScheduleOnce, _isScheduleDaily, _isScheduleWeekly;
        DateTime _date, _time;

        public SettingsViewModel()
        {
            // load schedule
            _weekDays = new ObservableCollection<bool>();
            for (var index = 0; index < 7; index++)
                _weekDays.Add(SettingsManager.Instance.ScheduleDays[index]);
            _date = SettingsManager.Instance.ScheduleDate;
            _time = SettingsManager.Instance.ScheduleTime;
            _isScheduleDisabled = SettingsManager.Instance.ScheduleType == ScheduleType.None;
            _isScheduleOnce = SettingsManager.Instance.ScheduleType == ScheduleType.Once;
            _isScheduleDaily = SettingsManager.Instance.ScheduleType == ScheduleType.Daily;
            _isScheduleWeekly = SettingsManager.Instance.ScheduleType == ScheduleType.Weekly;

            // commands
            _scheduleSilentRun = new ScheduleSilentRunCommand(this);
            _skipUac = new SkipUacCommand(this);
            _saveSchedule = new AsyncDelegateCommand(this, null, ExecuteSaveSchedule, SaveScheduleExecuted);
            _scheduleSilentRun.IsEnabled = _skipUac.IsEnabled = _saveSchedule.IsEnabled = true;
        }

        #region properties

        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (value == _date)
                    return;

                _date = value;
                RaisePropertyChanged("Date");
            }
        }

        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (value == _time)
                    return;

                _time = value;
                RaisePropertyChanged("Time");
            }
        }

        public bool IsScheduleDisabled
        {
            get { return _isScheduleDisabled; }
            set
            {
                if (value == IsScheduleDisabled)
                    return;

                _isScheduleDisabled = value;
                RaisePropertyChanged("IsScheduleDisabled");

                if (IsScheduleDisabled)
                    IsScheduleOnce = IsScheduleDaily = IsScheduleWeekly = false;
            }
        }

        public bool IsScheduleOnce
        {
            get { return _isScheduleOnce; }
            set
            {
                if (value == IsScheduleOnce)
                    return;

                _isScheduleOnce = value;
                RaisePropertyChanged("IsScheduleOnce");

                if (IsScheduleOnce)
                    IsScheduleDisabled = IsScheduleDaily = IsScheduleWeekly = false;
            }
        }

        public bool IsScheduleDaily
        {
            get { return _isScheduleDaily; }
            set
            {
                if (value == IsScheduleDaily)
                    return;

                _isScheduleDaily = value;
                RaisePropertyChanged("IsScheduleDaily");

                if (IsScheduleDaily)
                    IsScheduleDisabled = IsScheduleOnce = IsScheduleWeekly = false;
            }
        }

        public bool IsScheduleWeekly
        {
            get { return _isScheduleWeekly; }
            set
            {
                if (value == IsScheduleWeekly)
                    return;

                _isScheduleWeekly = value;
                RaisePropertyChanged("IsScheduleWeekly");

                if (IsScheduleWeekly)
                    IsScheduleDisabled = IsScheduleOnce = IsScheduleDaily = false;
            }
        }

        public ObservableCollection<bool> WeekDays
        {
            get { return _weekDays; }
        }

        public bool RunAtBoot
        {
            get { return SettingsManager.Instance.RunAtBoot; }
            set
            {
                if (value == SettingsManager.Instance.RunAtBoot)
                    return;

                SettingsManager.Instance.RunAtBoot = value;
                RaisePropertyChanged("RunAtBoot");

                // enable UAC skipping
                if (RunAtBoot)
                    SkipUac = true;
            }
        }

        public bool RunScanAtLaunch
        {
            get { return SettingsManager.Instance.RunScanAtLaunch; }
            set
            {
                if (value == SettingsManager.Instance.RunScanAtLaunch)
                    return;

                SettingsManager.Instance.RunScanAtLaunch = value;
                RaisePropertyChanged("RunScanAtLaunch");
            }
        }

        public bool RunPluginUpdateAtLaunch
        {
            get { return SettingsManager.Instance.RunPluginUpdateAtLaunch; }
            set
            {
                if (value == SettingsManager.Instance.RunPluginUpdateAtLaunch)
                    return;

                SettingsManager.Instance.RunPluginUpdateAtLaunch = value;
                RaisePropertyChanged("RunPluginUpdateAtLaunch");
            }
        }

        public bool RunProgramUpdateAtLaunch
        {
            get { return SettingsManager.Instance.RunProgramUpdateAtLaunch; }
            set
            {
                if (value == SettingsManager.Instance.RunProgramUpdateAtLaunch)
                    return;

                SettingsManager.Instance.RunProgramUpdateAtLaunch = value;
                RaisePropertyChanged("RunProgramUpdateAtLaunch");
            }
        }

        public bool SkipUac
        {
            get { return SettingsManager.Instance.SkipUac; }
            set
            {
                if (value == SettingsManager.Instance.SkipUac)
                    return;

                SettingsManager.Instance.SkipUac = value;
                RaisePropertyChanged("SkipUac");

                // disable boot time execution when UAC is disabled
                if (!SkipUac)
                {
                    RunAtBoot = false;
                    ScheduleSilentRun.Execute(RunAtBoot);
                }
            }
        }

        public bool ExitOnClose
        {
            get { return SettingsManager.Instance.ExitOnClose; }
            set
            {
                if (value == SettingsManager.Instance.ExitOnClose)
                    return;

                SettingsManager.Instance.ExitOnClose = value;
                RaisePropertyChanged("ExitOnClose");
            }
        }

        public bool CloseAfterFixing
        {
            get { return SettingsManager.Instance.CloseAfterFixing; }
            set
            {
                if (value == SettingsManager.Instance.CloseAfterFixing)
                    return;

                SettingsManager.Instance.CloseAfterFixing = value;
                RaisePropertyChanged("CloseAfterFixing");
            }
        }

        public bool ShutdownAfterFixing
        {
            get { return SettingsManager.Instance.ShutdownAfterFixing; }
            set
            {
                if (value == SettingsManager.Instance.ShutdownAfterFixing)
                    return;

                SettingsManager.Instance.ShutdownAfterFixing = value;
                RaisePropertyChanged("ShutdownAfterFixing");
            }
        }

        #endregion

        #region commands

        public CommandBase ScheduleSilentRun
        {
            get { return _scheduleSilentRun; }
        }

        public CommandBase SkipUserAccountControl
        {
            get { return _skipUac; }
        }

        public CommandBase SaveSchedule
        {
            get { return _saveSchedule; }
        }

        #endregion

        object ExecuteSaveSchedule(object parameter)
        {
            var task = new TaskModel();
            task.Name = string.Format("{0}AutomaticSmartScan", SettingsManager.Instance.CodeName);
            if (IsScheduleDisabled)
                return task.Delete();

            task.ExecutablePath = SettingsManager.Instance.ExecutablePath;
            task.CommandLineArguments = string.Format("/{0} /{1}", CommandLineManager.CommandLineArgument.Silent, CommandLineManager.CommandLineArgument.SmartScan);


            var schedule = new DateTime(Date.Year, Date.Month, Date.Day, Time.Hour, Time.Minute, Time.Second);
            if (IsScheduleOnce)
            {
                task.Schedule = new TimeTrigger(schedule);
            }
            else if (IsScheduleDaily)
            {
                task.Schedule = new DailyTrigger();
                task.Schedule.StartBoundary = schedule;
            }
            else if (IsScheduleWeekly)
            {
                DaysOfTheWeek days = DaysOfTheWeek.AllDays;
                for (short index = 0; index < WeekDays.Count; index++)
                {
                    if (!WeekDays[index])
                        continue;

                    DaysOfTheWeek day = DaysOfTheWeek.AllDays;
                    switch (index)
                    {
                        case 0:
                            day = DaysOfTheWeek.Sunday;
                            break;

                        case 1:
                            day = DaysOfTheWeek.Monday;
                            break;

                        case 2:
                            day = DaysOfTheWeek.Tuesday;
                            break;

                        case 3:
                            day = DaysOfTheWeek.Wednesday;
                            break;

                        case 4:
                            day = DaysOfTheWeek.Thursday;
                            break;

                        case 5:
                            day = DaysOfTheWeek.Friday;
                            break;

                        case 6:
                            day = DaysOfTheWeek.Saturday;
                            break;
                    }

                    if (days == DaysOfTheWeek.AllDays)
                        days = day;
                    else
                        days |= day;
                }
                task.Schedule = new WeeklyTrigger(days);
                task.Schedule.StartBoundary = schedule;
            }


            var result = task.Create();
            if (result)
            {
                Date = task.Schedule.StartBoundary;
                Time = task.Schedule.StartBoundary;
            }
            return result;
        }

        void SaveScheduleExecuted(object result)
        {
            if ((bool)result)
            {
                // weekdays
                var d = new bool[WeekDays.Count];
                for (var i = 0; i < WeekDays.Count; i++)
                    d[i] = WeekDays[i];
                SettingsManager.Instance.ScheduleDays = d;

                if (IsScheduleOnce)
                    SettingsManager.Instance.ScheduleType = ScheduleType.Once;
                else if (IsScheduleDaily)
                    SettingsManager.Instance.ScheduleType = ScheduleType.Daily;
                else if (IsScheduleWeekly)
                    SettingsManager.Instance.ScheduleType = ScheduleType.Weekly;
                else
                    SettingsManager.Instance.ScheduleType = ScheduleType.None;

                UiHelper.Instance.Alert("Smart scan schedule saved successfully.");
            }
            else
                UiHelper.Instance.Error("Failed to save smart scan schedule.");
        }
    }
}
