using System ;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MonitorV3.Wondows
{
    /// <summary>
    /// AddCustomButton.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : Window
    {
        public string EditName { get; set; }
        public int EditID { get; set; }
        public event EventHandler ConfirmEvent;
        public EditWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmEvent?.Invoke(this, new EventArgs());
            this.Close();
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
