using System.Windows.Controls;

namespace dotnetNES.Client.Views
{
    /// <summary>
    /// Interaction logic for NameTables.xaml
    /// </summary>
    public partial class Debugger
    {      
        public Debugger()
        {
            InitializeComponent();
        }

        private void OutputLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = sender as ListView;
            if (grid?.SelectedItem != null)
            {                    

                grid.Dispatcher.InvokeAsync(() =>
                {                       
                    grid.ScrollIntoView(grid.SelectedItem);
                });
            }
        }
    }
}
