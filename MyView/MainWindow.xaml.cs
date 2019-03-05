using System.Windows;

namespace MyView
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyPoker.ITable Table = new MyPoker.Table();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Table;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
