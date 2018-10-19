using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using AZE.Impl;
using AZE.Properties;
using AZE.Resources;
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

        private ActionCommand switchLanguageCommand;

        private int precision = 5;

        private ActionCommand openFileCommand;

        private double taskBarProgressValue = 0;

        private TaskbarItemProgressState taskBarProgressState;

        private ICommand activatedCommand;

        #endregion

        #region Binding properties


        /// <summary>
        ///     Progress value for task bar item
        /// </summary>
        public double TaskBarProgressValue
        {
            get => this.taskBarProgressValue;
            set => this.SetValue(ref this.taskBarProgressValue, value);
        }

        /// <summary>
        ///     Progress value for task bar item
        /// </summary>
        public TaskbarItemProgressState TaskBarProgressState
        {
            get => this.taskBarProgressState;
            set => this.SetValue(ref this.taskBarProgressState, value);
        }

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
                this.switchLanguageCommand?.TriggerExecuteChange();
            }
        }

        public string FileName
        {
            get => this.fileName;
            set
            {
                this.SetValue(ref this.fileName, value);
                this.OnPropertyChanged(() => this.FileNameShort);
            }
        }

        public string FileNameShort => Path.GetFileName(this.FileName);

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

        public ImageSource SwitchLanguageFlag
        {
            get
            {
                if (Settings.Default.Culture.Contains("de"))
                {
                    return FlagIcons.English.Source;
                }

                return FlagIcons.German.Source;
            }
        }

        #endregion

        #region Commands

        public ICommand SelectFileCommand 
            => this.selectFileCommand ?? (this.selectFileCommand = new ActionCommand(this.ExecuteSelectFileCommand, () => !this.Running));

        public ICommand SetBeginCommand 
            => this.setBeginCommand ?? (this.setBeginCommand = new ActionCommand(this.ExecuteSetBeginCommand, () => !this.Running));

        public ICommand SetEndCommand 
            => this.setEndCommand ?? (this.setEndCommand = new ActionCommand(this.ExecuteSetEndCommand, () => !this.Running));

        public ICommand AddPauseCommand 
            => this.addPauseCommand ?? (this.addPauseCommand = new ActionCommand(this.ExecuteAddPauseCommand, () => !this.Running));

        public ICommand SwitchLanguageCommand 
            => this.switchLanguageCommand ?? (this.switchLanguageCommand = new ActionCommand(this.ExecuteSwitchLanguageCommand, () => !this.Running));

        public ICommand OpenFileCommand 
            => this.openFileCommand ?? (this.openFileCommand = new ActionCommand(this.ExecuteOpenFileCommand, () => !string.IsNullOrWhiteSpace(this.FileName)));

        public ICommand ActivatedCommand
            => this.activatedCommand ?? (this.activatedCommand = new ActionCommand(this.ExecuteActivatedCommand, () => !this.Running));

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

        private void ExecuteOpenFileCommand()
        {
            //System.Diagnostics.Process.Start("explorer.exe", $"/select,{this.FileName}");
            System.Diagnostics.Process.Start(this.FileName);
        }

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

        private void ExecuteSwitchLanguageCommand()
        {
            if ((Settings.Default.Culture?.ToLower().Contains("de")).GetValueOrDefault())
            {
                Settings.Default.Culture = "en";
            }
            else
            {
                Settings.Default.Culture = "de";
            }

            Settings.Default.Save();

            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }

        private void ExecuteActivatedCommand()
        {
            if (this.Running)
            {
                return;
            }

            this.UpdateTaskBar(TaskbarItemProgressState.None);
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

        private void UpdateTaskBar(TaskbarItemProgressState state)
        {
            Debug.WriteLine($"{state} ({this.Running})");

            switch (state)
            {
                case TaskbarItemProgressState.None:
                case TaskbarItemProgressState.Indeterminate:
                    this.TaskBarProgressValue = 0;
                    break;
                case TaskbarItemProgressState.Normal:
                case TaskbarItemProgressState.Error:
                    this.TaskBarProgressValue = 1;
                    break;
                case TaskbarItemProgressState.Paused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            this.TaskBarProgressState = state;
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
                        this.UpdateTaskBar(TaskbarItemProgressState.Indeterminate);

                        using (ExcelFileAccess aze = ExcelFileAccess.OpenReadOnly(this.FileName))
                        {
                            this.CurrentAzeData = aze.FindData(DateTime.Today);
                        }

                        this.UpdateTaskBar(TaskbarItemProgressState.Normal);
                    }
                    catch (Exception)
                    {
                        this.UpdateTaskBar(TaskbarItemProgressState.Error);
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
                        this.UpdateTaskBar(TaskbarItemProgressState.Indeterminate);

                        using (ExcelFileAccess aze = ExcelFileAccess.OpenFullAccess(this.FileName))
                        {
                            if (aze.SetTime(this.CurrentAzeData.RowNumber, timeValue, time))
                            {
                                this.CurrentAzeData = aze.FindData(DateTime.Today, this.CurrentAzeData.RowNumber);
                            }
                        }

                        this.UpdateTaskBar(TaskbarItemProgressState.Normal);
                    }
                    catch (Exception)
                    {
                        this.UpdateTaskBar(TaskbarItemProgressState.Error);
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
                        this.UpdateTaskBar(TaskbarItemProgressState.Indeterminate);

                        using (ExcelFileAccess aze = ExcelFileAccess.OpenFullAccess(this.FileName))
                        {
                            if (aze.AddPause(this.CurrentAzeData.RowNumber, minutes))
                            {
                                this.CurrentAzeData = aze.FindData(DateTime.Today, this.CurrentAzeData.RowNumber);
                            }
                        }

                        this.UpdateTaskBar(TaskbarItemProgressState.Normal);
                    }
                    catch (Exception)
                    {
                        this.UpdateTaskBar(TaskbarItemProgressState.Error);
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