using Automata.Homebrew;
using Automata.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Automata
{
    class MainWindowViewModel : ViewModelBase
    {
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

        public MainWindowViewModel()
        {
            _changeNameCommand = new DelegateCommand(OnChangeName, CanChangeName);
            _openFileCommand = new DelegateCommand(OnOpenFile, null);
            _dropFileCommand = new DelegateCommand(OnDropFile, null);
            _validateInput = new DelegateCommand(OnValidateInput, null);
            _parseManualDataCommand = new DelegateCommand(OnParseManualData, null);
        }

        private void OnParseManualData(object obj)
        {
            ParseData(string.Empty);
        }

        private void OnValidateInput(object commandParameter)
        {
            var result = DFA.ValidateInput(Input);
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

        private string Data { get; set; }

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
                NFA.ToOutputMatrix();
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
                MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            DFA.ToTransformedOutputMatrix();
            try
            {
                SetUpGraphAsync(true);
            }
            catch (Exception){}
            DFA.GetTransformedData();
            CanType = true;
        }

        public async void SetUpGraphAsync(bool deterministic)
        {
            try
            {
                if (deterministic)
                {
                    await Task.Run(() => DFA.ToGraph());
                    ForceNotification(nameof(DFA));
                }
                else
                {
                    await Task.Run(() => NFA.ToGraph());
                    ForceNotification(nameof(NFA));
                }
            }
            catch (Exception){}
        }
    }
}
