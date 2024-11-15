using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterUpdates
{
    public partial class BetterUpdatesSettingsView : UserControl
    {
        private readonly BetterUpdates _plugin;

        public BetterUpdatesSettingsView(BetterUpdates plugin)
        {
            _plugin = plugin;
            InitializeComponent();
        }

        private void ButCheckNotif_Click(object sender, RoutedEventArgs e)
        {
            _plugin.CheckNotifications();
        }
    }
}