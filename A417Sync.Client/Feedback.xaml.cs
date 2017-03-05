namespace A417Sync.Client
{
    using System.Diagnostics;
    using System.Windows;

    using Microsoft.HockeyApp;

    /// <summary>
    /// Interaction logic for Feedback.xaml
    /// </summary>
    public partial class Feedback : Window
    {
        public Feedback()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs e) => Close();

        private async void Send(object sender, RoutedEventArgs e)
        {
            await HockeyClient.Current.CreateFeedbackThread()
                .PostFeedbackMessageAsync(this.Message.Text, this.Email.Text, this.Subject.Text, this.Name.Text)
                .ContinueWith(
                    task =>
                        {
                            Serilog.Log.ForContext<Feedback>()
                                .Information("Feedback sent. Info received: {@info}", task.Result);
                            Process.Start(task.Result.CreatedAt);
                        });
            Cancel(sender, e);
        }
    }
}