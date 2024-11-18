using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 이미지 불러오기
        private void OpenImage(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            //if (openFileDialog.ShowDialog() == true)
            //{
            //    BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
            //    ImageDisplay.Source = bitmap; // 2번째 그리드에 표시
            //}
        }

        // 이미지 저장하기
        private void SaveImage(object sender, RoutedEventArgs e) { }

        // 다른 이름으로 저장하기
        private void SaveAsImage(object sender, RoutedEventArgs e) { }


    }
}