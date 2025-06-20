using System.Windows;
using VaultTrack.ViewModels;

namespace VaultTrack.Views
{
    /// <summary>
    /// Interaction logic for TransactionWindow.xaml
    /// </summary>
    public partial class TransactionWindow : Window
    {
        public TransactionWindow()
        {
            InitializeComponent();
        }

        public TransactionWindow(TransactionViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.RequestClose += (s, e) => this.DialogResult = viewModel.DialogResult;
        }
    }
} 