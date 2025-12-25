using System.Windows;
using System.Windows.Media.Animation;

namespace VaultTrack.Views
{
    public partial class SuccessDialog : Window
    {
        public SuccessDialog(string message)
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

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
} 