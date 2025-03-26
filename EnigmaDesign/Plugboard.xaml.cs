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
using System.Windows.Shapes;

namespace EnigmaDesign
{
    /// <summary>
    /// Interaction logic for Plugboard.xaml
    /// </summary>
    public partial class Plugboard : Window
    {
        public MainWindow machine { get; set; }
        bool key = false, value = false;
        int plugs = 10;
        Brush[] borderColor = new Brush[10]
        {Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green,
            Brushes.Red,Brushes.CornflowerBlue,Brushes.Violet,Brushes.Pink,Brushes.Coral,Brushes.MediumVioletRed};
        
        Button plugA, plugB;
        Button[] plugBTN = new Button[26];

        public Plugboard()
        {
            InitializeComponent();
            Counter.Content = plugs;
            GatherPlugs();
        }

        private void Complete(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Close(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GatherPlugs()
        {
            int acsii = 65;
            for (int i = 0; i < 26; i++)
            {
                plugBTN[i] = (Button)this.FindName($"Plug{(char)acsii}");
                acsii++;
            }
        }

        private void Configure(object sender, RoutedEventArgs e)
        {
            Button plug = sender as Button;

            if (plugs > 0)
            {
                if (!key && !value)
                {
                    plugA = plug;
                    key = true;
                    SelectedPlug.Content = $"Selected Plug: {plug.Content.ToString()}";
                }
                else if (key && !value)
                {
                    if (plug == plugA)
                    {
                        key = false;
                        plugA = null;
                        SelectedPlug.Content = $"Selected Plug: ";

                    }
                    else
                    {
                        plugB = plug;
                        value = true;
                        ConnectPlugs(plugA.ToString()[0], plugB.ToString()[0]);
                        Counter.Content = plugs;

                        plugA.BorderBrush = borderColor[plugs];
                        plugB.BorderBrush = borderColor[plugs];
                        plugA.IsEnabled = false;
                        plugB.IsEnabled = false;
                    }
                }
            }

            else if (plugs <= 0)
            {
                MessageBox.Show("You have reached the max no. of plugs to configure");
            }
        }

        private void ResetPlug(object sender, RoutedEventArgs e)
        {
            foreach (Button plug in plugBTN)
            {
                plug.IsEnabled = true;
            }

            SelectedPlug.Content = $"Selected Plug: ";
        }

        private void ConnectPlugs(char A, char B)
        {
            machine._plugboard[A] = B;
            machine._plugboard[B] = A;
            plugs--;
            NewPlug();
        }

        private void NewPlug()
        { 
            key = false;
            value = false;
            SelectedPlug.Content = $"Selected Plug: ";
        }
    }
}
