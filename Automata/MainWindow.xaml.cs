using Automatas.POCO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Automata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        FiveTuple transformed;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ParseData(openFileDialog.FileName);
            }
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ParseData(((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
            }
        }

        private void ParseData(string filePath)
        {
            ResetData();
            string rawData;
            if (string.IsNullOrEmpty(filePath))
            {
                rawData = textBoxData.Text;
            }
            else
            {
                textBoxPath.Text = filePath;
                rawData = File.ReadAllText(filePath);
                textBoxData.Text = rawData;
            }
            var fiveTuple = new FiveTuple(rawData);
            if (fiveTuple.IsValid)
            {
                fiveTuple.Transform();
                textBoxQ.Text = string.Join("\n", fiveTuple.Q);
                textBoxF.Text = string.Join("\n", fiveTuple.F);
                textBoxA.Text = string.Join("\n", fiveTuple.A);
                itemsNFA.ItemsSource = fiveTuple.ToOutputMatrix();
                try
                {
                    SetUpGraph(fiveTuple, false);

                }
                catch (Exception)
                {

                }
                Transform(fiveTuple);
            }
            else
            {
                MessageBox.Show("Data invalida", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void SetUpGraph(FiveTuple fiveTuple, bool deterministic)
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
                    imageDFA.Source = image;
                }
                else
                {
                    imageNFA.Source = image;
                }
                File.Delete(result);
            }
            catch (Exception)
            {

            }
        }

        public async void Transform(FiveTuple fiveTuple)
        {
            var result = await Task.Run(() => fiveTuple.Transform());
            itemsDFA.ItemsSource = result.ToTransformedOutputMatrix();
            try
            {
                SetUpGraph(result, true);

            }
            catch (Exception)
            {

            }
            textBoxDataTransformed.Text = result.getTransformedData();
            transformed = result;
            textBoxIn.IsEnabled = true;
            buttonValidate.IsEnabled = true;
        }

        private void ButtonData_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxData.Text.Equals(string.Empty))
            {
                return;
            }
            ParseData(string.Empty);
        }

        private async void Validate()
        {
            var input = textBoxIn.Text;
            var result = await Task.Run(() => transformed.ValidateInput(input));
            if (result)
            {
                labelValidate.Content = "Cadena de Aceptacion";
                textBoxIn.Background = new SolidColorBrush(Color.FromRgb(198, 239, 206));
                SetUpGraph(transformed, true);
            }
            else
            {
                labelValidate.Content = "Cadena Invalida";
                textBoxIn.Background = new SolidColorBrush(Color.FromRgb(255, 199, 206));
                SetUpGraph(transformed, true);
            }
        }

        private void TextBoxIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private void ButtonValidate_Click(object sender, RoutedEventArgs e)
        {
            Validate();
        }

        private void TextBoxIn_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Validate();
            }
        }

        private void ResetData()
        {
            textBoxQ.Text = string.Empty;
            textBoxF.Text = string.Empty;
            textBoxA.Text = string.Empty;
            textBoxDataTransformed.Text = string.Empty;
            textBoxIn.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            textBoxIn.Clear();
            labelValidate.Content = string.Empty;
            itemsNFA.ClearValue(ItemsControl.ItemsSourceProperty);
            itemsDFA.ClearValue(ItemsControl.ItemsSourceProperty);
            imageNFA.Source = null;
            imageDFA.Source = null;
            textBoxIn.IsEnabled = false;
            buttonValidate.IsEnabled = false;
            transformed = null;
        }
    }
}
