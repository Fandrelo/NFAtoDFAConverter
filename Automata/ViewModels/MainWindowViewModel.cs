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
        private FiveTuple transformed;

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
            var result = transformed.ValidateInput(Input);
            if (result)
            {
                ValidationStatus = "Ok";
                ValidationStatusBG = new SolidColorBrush(Color.FromRgb(198, 239, 206));
                SetUpGraphAsync(transformed, true);
            }
            else
            {
                ValidationStatus = "Error";
                ValidationStatusBG = new SolidColorBrush(Color.FromRgb(255, 199, 206));
                SetUpGraphAsync(transformed, true);
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
            set => SetProperty(ref _data, value);
        }

        private string _path;
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        private string _q;
        public string Q
        {
            get => _q;
            set => SetProperty(ref _q, value);
        }

        private string _f;
        public string F
        {
            get => _f;
            set => SetProperty(ref _f, value);
        }

        private string _a;
        public string A
        {
            get => _a;
            set => SetProperty(ref _a, value);
        }

        private List<List<string>> _nfaMatrix;
        public List<List<string>> NFAMatrix
        {
            get => _nfaMatrix;
            set => SetProperty(ref _nfaMatrix, value);
        }

        private BitmapImage _nfaImage;
        public BitmapImage NFAImage
        {
            get => _nfaImage;
            set => SetProperty(ref _nfaImage, value);
        }

        private List<List<string>> _dfaMatrix;
        public List<List<string>> DFAMatrix
        {
            get => _dfaMatrix;
            set => SetProperty(ref _dfaMatrix, value);
        }

        private string _dataTransformed;
        public string DataTransformed
        {
            get => _dataTransformed;
            set => SetProperty(ref _dataTransformed, value);
        }

        private BitmapImage _dfaImage;
        public BitmapImage DFAImage
        {
            get => _dfaImage;
            set => SetProperty(ref _dfaImage, value);
        }

        private string _input;
        public string Input
        {
            get => _input;
            set {
                SetProperty(ref _input, value);
                if (transformed != null)
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
            var fiveTuple = await Task.Run(() => new FiveTuple(rawData));
            if (fiveTuple.IsValid)
            {
                Q = string.Join("\n", fiveTuple.Q);
                F = string.Join("\n", fiveTuple.F);
                A = string.Join("\n", fiveTuple.A);
                NFAMatrix = fiveTuple.ToOutputMatrix();
                try
                {
                    SetUpGraphAsync(fiveTuple, false);

                }
                catch (Exception) { }
                TransformFiveTupleAsync(fiveTuple);
            }
            else
            {
                MessageBox.Show("Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetData()
        {
            transformed = null;
            Q = string.Empty;
            F = string.Empty;
            A = string.Empty;
            DataTransformed = string.Empty;
            ValidationStatusBG = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Input = string.Empty;
            ValidationStatus = string.Empty;
            NFAMatrix = null;
            DFAMatrix = null;
            NFAImage = null;
            DFAImage = null;
            CanType = false;
        }

        public async void TransformFiveTupleAsync(FiveTuple fiveTuple)
        {
            var result = await Task.Run(() => fiveTuple.Transform());
            DFAMatrix = result.ToTransformedOutputMatrix();
            try
            {
                SetUpGraphAsync(result, true);

            }
            catch (Exception){}
            DataTransformed = result.GetTransformedData();
            transformed = result;
            CanType = true;
        }

        public async void SetUpGraphAsync(FiveTuple fiveTuple, bool deterministic)
        {
            try
            {
                var result = await Task.Run(() => fiveTuple.ToGraph());
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + result, UriKind.Absolute);
                image.EndInit();
                if (deterministic)
                {
                    DFAImage = image;
                }
                else
                {
                    NFAImage = image;
                }
                File.Delete(result);
            }
            catch (Exception){}
        }
    }
}
