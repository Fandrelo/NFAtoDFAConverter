using Automata.Homebrew;
using Automata.Models;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Automata.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;

        private readonly DelegateCommand _changeNameCommand;
        public ICommand ChangeNameCommand => _changeNameCommand;

        private readonly DelegateCommand _openFileCommand;
        public ICommand OpenFileCommand => _openFileCommand;

        private readonly DelegateCommand _dropFileCommand;
        public ICommand DropFileCommand => _dropFileCommand;

        private readonly DelegateCommand _validateInput;
        public ICommand ValidateInput => _validateInput;

        private readonly DelegateCommand _parseManualDataCommand;
        public ICommand ParseManualDataCommand => _parseManualDataCommand;

        private readonly DelegateCommand _saveImageCommand;
        public ICommand SaveImageCommand => _saveImageCommand;

        private readonly DelegateCommand _flyoutOpenCommand;
        public ICommand FlyoutOpenCommand => _flyoutOpenCommand;

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            _changeNameCommand = new DelegateCommand(OnChangeName, CanChangeName);
            _openFileCommand = new DelegateCommand(OnOpenFile, null);
            _dropFileCommand = new DelegateCommand(OnDropFile, null);
            _validateInput = new DelegateCommand(OnValidateInput, null);
            _parseManualDataCommand = new DelegateCommand(OnParseManualData, null);
            _saveImageCommand = new DelegateCommand(OnSaveImage, null);
            _flyoutOpenCommand = new DelegateCommand(OnFlyoutOpen, null);
        }

        private void OnFlyoutOpen(object commandParameter)
        {
            IsFlyoutOpen = true;
        }

        private void OnSaveImage(object commandParameter)
        {
            if (!(commandParameter is BitmapImage image)) return;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Gif (.gif)|*.gif"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create)) encoder.Save(stream);
            }
        }

        private void OnParseManualData(object commandParameter)
        {
            ParseData(string.Empty);
        }

        private void OnValidateInput(object commandParameter)
        {
            var result = DFA.IsInputValid(Input);
            if (result)
            {
                ValidationStatus = "Ok";
                ValidationStatusBG = new SolidColorBrush(Color.FromRgb(198, 239, 206));
                SetUpGraphAsync(true);
            }
            else
            {
                ValidationStatus = "Error";
                ValidationStatusBG = new SolidColorBrush(Color.FromRgb(255, 199, 206));
                SetUpGraphAsync(true);
            }
        }

        private void OnDropFile(object commandParameter)
        {
            if (!(commandParameter is IDataObject ido)) return;
            if (ido.GetDataPresent(DataFormats.FileDrop))
            {
                ParseData(((string[])ido.GetData(DataFormats.FileDrop))[0]);
            }
        }

        private void OnChangeName(object commandParameter)
        {
            WindowTitle = "Automata";
            _changeNameCommand.InvokeCanExecuteChanged();
        }

        private bool CanChangeName(object commandParameter)
        {
            return WindowTitle != "Automata";
        }

        private void OnOpenFile(object commandParameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ParseData(openFileDialog.FileName);
            }
        }

        private string _windowTitle = "Automata";
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        private string _data;
        public string Data
        {
            get => _data;
            set
            {
                CanParse = !string.IsNullOrEmpty(value);
                SetProperty(ref _data, value);
            }
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private string _input;
        public string Input
        {
            get => _input;
            set {
                SetProperty(ref _input, value);
                if (DFA != null)
                {
                    OnValidateInput(null);
                }
            }
        }

        private bool _canType = false;
        public bool CanType
        {
            get => _canType;
            set => SetProperty(ref _canType, value);
        }

        private string _validationStatus;
        public string ValidationStatus
        {
            get => _validationStatus;
            set => SetProperty(ref _validationStatus, value);
        }

        private SolidColorBrush _validationStatusBG;
        public SolidColorBrush ValidationStatusBG
        {
            get => _validationStatusBG;
            set => SetProperty(ref _validationStatusBG, value);
        }

        private FiveTuple _nfa;
        public FiveTuple NFA
        {
            get => _nfa;
            set => SetProperty(ref _nfa, value);
        }

        private FiveTuple _dfa;
        public FiveTuple DFA
        {
            get => _dfa;
            set => SetProperty(ref _dfa, value);
        }

        private int _qFontSize;
        public int QFontSize
        {
            get => _qFontSize;
            set => SetProperty(ref _qFontSize, value);
        }

        private bool _canParse = false;
        public bool CanParse
        {
            get => _canParse;
            set => SetProperty(ref _canParse, value);
        }

        private bool _isFlyoutOpen = false;
        public bool IsFlyoutOpen
        {
            get => _isFlyoutOpen;
            set => SetProperty(ref _isFlyoutOpen, value);
        }

        private int _uiScale = 1;
        public int UIScale
        {
            get => _uiScale;
            set
            {
                SetProperty(ref _uiScale, value);
            }
        }

        private void ParseData(string filePath)
        {
            ResetData();
            string rawData;
            if (string.IsNullOrEmpty(filePath))
            {
                rawData = Data;
            }
            else
            {
                Path = filePath;
                rawData = File.ReadAllText(filePath);
                Data = rawData;
            }
            SetUpFiveTupleAsync(rawData);
        }

        private async void SetUpFiveTupleAsync(string rawData)
        {
            NFA = await Task.Run(() => new FiveTuple(rawData));
            if (NFA.IsValid)
            {
                if(NFA.Q.Length > 5)
                {
                    QFontSize = 15;
                } else
                {
                    QFontSize = 20;
                }
                NFA.SetupOutputMatrix();
                ForceNotification(nameof(NFA));
                try
                {
                    SetUpGraphAsync(false);
                }
                catch (Exception) { }
                ForceNotification(nameof(NFA));
                TransformFiveTupleAsync(NFA);
            }
            else
            {
                ShowMessageAsync("Error", "The input is not valid.");
            }
        }

        private void ResetData()
        {
            NFA = null;
            DFA = null;
            ValidationStatusBG = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Input = string.Empty;
            ValidationStatus = string.Empty;
            CanType = false;
        }

        public async void TransformFiveTupleAsync(FiveTuple fiveTuple)
        {
            DFA = await Task.Run(() => fiveTuple.Transform());
            DFA.SetupTransformedOutputMatrix();
            try
            {
                SetUpGraphAsync(true);
            }
            catch (Exception){}
            DFA.SetupTransformedData();
            CanType = true;
        }

        public async void SetUpGraphAsync(bool deterministic)
        {
            try
            {
                if (deterministic)
                {
                    await Task.Run(() => DFA.SetupGraph());
                    ForceNotification(nameof(DFA));
                }
                else
                {
                    await Task.Run(() => NFA.SetupGraph());
                    ForceNotification(nameof(NFA));
                }
            }
            catch (Exception){}
        }

        private async void ShowMessageAsync(string header, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, header, message);
        }
    }
}
