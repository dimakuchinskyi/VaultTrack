using System.Windows;
using System.Windows.Media.Animation;

namespace VaultTrack.Views
{
    public partial class ConfirmDialog : Window
    {
        public bool Result { get; private set; } = false;

        public ConfirmDialog(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = (Storyboard)FindResource("FadeInStoryboard");
            fadeIn.Begin(this);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var fadeOut = (Storyboard)FindResource("FadeOutStoryboard");
            fadeOut.Begin(this);
            base.OnClosing(e);
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.DialogResult = true;
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.DialogResult = false;
            this.Close();
        }
    }
} 