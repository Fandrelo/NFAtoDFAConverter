using Automata.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Automata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(DialogCoordinator.Instance);
        public MainWindow()
        {
            RightWindowCommandsOverlayBehavior = WindowCommandsOverlayBehavior.Never;
            IconOverlayBehavior = WindowCommandsOverlayBehavior.Never;
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }
    }
}
