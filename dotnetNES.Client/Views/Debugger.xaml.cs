using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace dotnetNES.Client
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
            if (sender is ListView)
            {
                ListView grid = (sender as ListView);
                if (grid.SelectedItem != null)
                {                    

                    grid.Dispatcher.InvokeAsync(() =>
                    {                       
                        grid.ScrollIntoView(grid.SelectedItem);
                    });
                }
            }
        }
    }
}
