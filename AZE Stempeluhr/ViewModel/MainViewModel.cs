using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AZE.Impl;
using AZE.Properties;
using Microsoft.Win32;
using WpfBasics;

namespace AZE.ViewModel
{
    internal class MainViewModel : NotifyPropertyChanged
    {
        private const int ReloadInterval = 1;  //s
        private const int TimerInterval = 500;  //ms

        #region Fields

        private Timer timer;

        private double timeoutValue;

        private DateTime last;

        private string fileName;

        private AzeData currentAzeData;

        private int pauseMinutes;

        private bool running;

        private ActionCommand selectFileCommand;

        private ActionCommand setBeginCommand;

        private ActionCommand setEndCommand;

        private ActionCommand addPauseCommand;

        private int precision = 5;

        #endregion

        #region Binding properties

        public bool Running
        {
            get => this.running;
            set
            {
                this.SetValue(ref this.running, value);

                this.setBeginCommand?.TriggerExecuteChange();
                this.setEndCommand?.TriggerExecuteChange();
                this.addPauseCommand?.TriggerExecuteChange();
                this.selectFileCommand?.TriggerExecuteChange();
            }
        }

        public string FileName
        {
            get => this.fileName;
            set => this.SetValue(ref this.fileName, value);
        }

        public string Date { get; set; }

        public string Time { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public AzeData CurrentAzeData
        {
            get => this.currentAzeData;
            set => this.SetValue(ref this.currentAzeData, value);
        }

        public int PauseMinutes
        {
            get => this.pauseMinutes;
            set
            {
                this.SetValue(ref this.pauseMinutes, value);
                this.StoreSettings();
            }
        }

        #endregion

        #region Commands

        public ICommand SelectFileCommand
        {
            get
            {
                return this.selectFileCommand ?? (this.selectFileCommand = new ActionCommand(() => this.ExecuteSelectFileCommand(), () => !this.Running));
            }
        }

        public ICommand SetBeginCommand
        {
            get
            {
                return this.setBeginCommand ?? (this.setBeginCommand = new ActionCommand(() => this.ExecuteSetBeginCommand(), () => !this.Running));
            }
        }

        public ICommand SetEndCommand
        {
            get
            {
                return this.setEndCommand ?? (this.setEndCommand = new ActionCommand(() => this.ExecuteSetEndCommand(), () => !this.Running));
            }
        }

        public ICommand AddPauseCommand
        {
            get
            {
                return this.addPauseCommand ?? (this.addPauseCommand = new ActionCommand(() => this.ExecuteAddPauseCommand(), () => !this.Running));
            }
        }

        #endregion

        public MainViewModel()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadSettings();
            this.TimeOut();
            this.timer = new Timer(o => this.TimingStep(), null, MainViewModel.TimerInterval, MainViewModel.TimerInterval);
        }

        private void TimeOut()
        {
            this.timeoutValue = MainViewModel.ReloadInterval * 1000;

            DateTime now = DateTime.Now;
            this.Date = now.ToShortDateString();
            this.Time = now.ToShortTimeString();

            this.CalculateBeginAndEnd(now, out DateTime begin, out DateTime end);
            this.BeginTime = begin.ToShortTimeString();
            this.EndTime = end.ToShortTimeString();

            this.OnPropertyChanged(() => this.Date);
            this.OnPropertyChanged(() => this.Time);
            this.OnPropertyChanged(() => this.BeginTime);
            this.OnPropertyChanged(() => this.EndTime);

            if (now.Date > this.last.Date)
            {
                this.LoadAzeData();
            }
            this.last = now;
        }

        private void TimingStep()
        {
            this.timeoutValue -= MainViewModel.TimerInterval;

            if (this.timeoutValue <= 0)
            {
                this.TimeOut();
            }
        }

        #region Command implementation

        private void ExecuteSelectFileCommand()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel AZE Tabelle|*.xls?";
            if (!string.IsNullOrEmpty(this.FileName))
            {
                ofd.FileName = this.FileName;
            }

            bool? result = ofd.ShowDialog();
            if (result.GetValueOrDefault())
            {
                this.FileName = ofd.FileName;

                this.StoreSettings();
                this.LoadAzeData();
            }
        }

        private void ExecuteSetBeginCommand()
        {
            this.CalculateBeginAndEnd(DateTime.Now, out DateTime begin, out _);

            this.StoreTime(AzeTimeValueEnum.Begin, begin);
        }

        private void ExecuteSetEndCommand()
        {
            this.CalculateBeginAndEnd(DateTime.Now, out _, out DateTime end);

            this.StoreTime(AzeTimeValueEnum.End, end);
        }

        private void ExecuteAddPauseCommand()
        {
            this.AddPause(this.PauseMinutes);
        }

        #endregion

        #region Methods

        public void LoadSettings()
        {
            Settings settings = Properties.Settings.Default;
            if (settings.AzeFileName != null)
            {
                this.FileName = settings.AzeFileName;
            }

            this.precision = settings.Precision;
            if (this.precision < 1)
            {
                this.precision = 1;
            }
            else if (this.precision > 30)
            {
                this.precision = 30;
            }

            this.PauseMinutes = settings.LastPauseDuration;
        }

        private void StoreSettings()
        {
            Settings settings = Properties.Settings.Default;

            settings.AzeFileName = this.FileName;
            settings.Precision = this.precision;
            settings.LastPauseDuration = this.PauseMinutes;

            settings.Save();
        }

        internal void CalculateBeginAndEnd(DateTime time, out DateTime begin, out DateTime end)
        {
            int plus = 0, minus = 0;
            if (time.Minute % this.precision != 0)
            {
                minus = time.Minute % this.precision;
                plus = this.precision - minus;
            }

            begin = time.AddMinutes(-minus);
            end = time.AddMinutes(plus);
        }

        private void LoadAzeData()
        {
            if (!File.Exists(this.FileName))
            {
                this.CurrentAzeData = null;
                return;
            }

            Task loadTask = new Task(
                () =>
                {
                    try
                    {
                        this.Running = true;
                        using (ExcelFileAccess aze = ExcelFileAccess.OpenReadOnly(this.FileName))
                        {
                            this.CurrentAzeData = aze.FindData(DateTime.Today);
                        }
                    }
                    finally
                    {
                        this.Running = false;
                    }
                });
            loadTask.Start();
        }

        internal void StoreTime(AzeTimeValueEnum timeValue, DateTime time, bool unattended = false)
        {
            if (!File.Exists(this.FileName) || this.CurrentAzeData == null)
            {
                this.CurrentAzeData = null;
                return;
            }

            if (timeValue == AzeTimeValueEnum.Begin && this.CurrentAzeData.Begin.HasValue
                || timeValue == AzeTimeValueEnum.End && this.CurrentAzeData.End.HasValue)
            {
                if (unattended && timeValue == AzeTimeValueEnum.Begin
                    || !unattended && MessageBox.Show(Properties.Resources.MessageTextConfirm, Properties.Resources.MessageTitleConfirm, MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }

            Task storeTask = new Task(
                () =>
                {
                    try
                    {
                        this.Running = true;
                        using (ExcelFileAccess aze = ExcelFileAccess.OpenFullAccess(this.FileName))
                        {
                            if (aze.SetTime(this.CurrentAzeData.RowNumber, timeValue, time))
                            {
                                this.CurrentAzeData = aze.FindData(DateTime.Today, this.CurrentAzeData.RowNumber);
                            }
                        }
                    }
                    finally
                    {
                        this.Running = false;
                    }
                });
            storeTask.Start();
        }

        private void AddPause(int minutes)
        {
            if (!File.Exists(this.FileName) || this.CurrentAzeData == null)
            {
                this.CurrentAzeData = null;
                return;
            }

            Task storeTask = new Task(
                () =>
                {
                    try
                    {
                        this.Running = true;
                        using (ExcelFileAccess aze = ExcelFileAccess.OpenFullAccess(this.FileName))
                        {
                            if (aze.AddPause(this.CurrentAzeData.RowNumber, minutes))
                            {
                                this.CurrentAzeData = aze.FindData(DateTime.Today, this.CurrentAzeData.RowNumber);
                            }
                        }
                    }
                    finally
                    {
                        this.Running = false;
                    }
                });
            storeTask.Start();
        }

        #endregion
    }
}