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
using System.Windows.Threading;
using System.Timers;
using System.IO.Packaging;
using System.Windows.Interop;

namespace EnigmaDesign
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Border[] lampboard = new Border[26];
        Plugboard configPlug = new Plugboard();
        Display msg;

        public string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Standard alphabet for reference
        string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV"; // Rotor 1 wiring (Hours)
        string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX"; // Rotor 2 wiring (Minutes)
        string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA"; // Rotor 3 wiring (Seconds)
        string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT"; // Reflector wiring

        int[] _keyOffset = { 0, 0, 0 }; // Current rotor offsets (H, M, S)
        int[] _initOffset = { 0, 0, 0 }; // Initial rotor offsets (H, M, S)

        // Rotor state flag
        bool _rotor = false;

        // Plugboard setup
        public Dictionary<char, char> _plugboard = new Dictionary<char, char>(); // Plugboard dictionary
        private bool _plugboardSet = false; // Flag to indicate if plugboard is set

        public MainWindow()
        {
            InitializeComponent();

            this.Left = 40;
            this.Top = 20;

            SetDefaults(); // Initialize default values
            GatherLabel();
            InitializePlugboard();

            _rotor = false; // Initially rotor is off
            RotorBTN.IsEnabled = false;

            msg = new Display
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = this.Left + 800,
                Top = 20
            };
            
            msg.Show();
            msg.typing = this;
        }

        //initate list of labels
        private void GatherLabel()
        {
            for (int i = 0; i < _control.Length; i++)
            {
                lampboard[i] = (Border)this.FindName($"Lamp{_control[i]}");
            }
        }

        private void InitializePlugboard()
        {
            for (int i = 0; i < _control.Length; i++)
            {
                _plugboard[_control[i]] = _control[i];
            }
        }

        private void LightLamp(char letter)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
           
            for (int x = 0; x < lampboard.Length; x++)
            {
                if (letter == _control[x])
                {
                    lampboard[x].Background = Brushes.Yellow;
                    timer.Start();
                    
                    timer.Tick += (s, args) =>
                    {
                        lampboard[x].Background = Brushes.FloralWhite;
                        timer.Stop();
                    }; 
                    
                    break;
                }
            }
        }

        // Find the index of a character in a string
        private int IndexSearch(string ring, char letter)
        {
            int index = 0;
            for (int x = 0; x < ring.Length; x++)
            {
                if (ring[x] == letter)
                {
                    index = x;
                    break;
                }
            }
            return index;
        }

        private void Type(object sender, KeyEventArgs e)
        {
            if (RotorBTN.Content.ToString() == "Settings Lock")
            {
                // Check for uppercase letters and message length
                if (e.Key.ToString().Length == 1)
                {
                    if ((int)e.Key.ToString()[0] >= 65 && (int)e.Key.ToString()[0] <= 90)
                    {
                        // Rotate rotors if enabled
                        if (_rotor)
                        {
                            Rotate(true);
                            DisplayMessage(msg.MessageTB, e.Key.ToString()[0]);
                            DisplayMessage(msg.EncryptedTB, Encrypt(e.Key.ToString()[0]));
                            LightLamp(Encrypt(e.Key.ToString()[0]));
                            DisplayMessage(msg.MirroredTB, Mirror(e.Key.ToString()[0]));
                        }
                    }
                }
                // Handle space key
                else if (e.Key == Key.Space)
                {
                    DisplayMessage(msg.MessageTB,' ');
                    DisplayMessage(msg.EncryptedTB, ' ');
                    DisplayMessage(msg.MirroredTB, ' ');
                }
                // Handle backspace key
                else if (e.Key == Key.Back)
                {
                    Rotate(false); // Rotate rotors backward
                    RemoveLetter(msg.MessageTB);
                    RemoveLetter(msg.EncryptedTB);
                    RemoveLetter(msg.MirroredTB);
                }
            }
        }

        private void DisplayMessage(TextBlock message, char letter)
        {
            message.Text += letter;
        }

        private void RemoveLetter(TextBlock message)
        {
            if (!string.IsNullOrEmpty(message.Text))
            {
                message.Text = message.Text.Remove(message.Text.Length - 1);
            }
        }

        private char Encrypt(char letter)
        {
            char newChar = letter;

            // Plugboard pass (before rotors)
            newChar = _plugboard[newChar];

            // Rotor pass forward
            newChar = _ring1[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring3[IndexSearch(_control, newChar)];

            // Reflector pass
            newChar = _reflector[IndexSearch(_control, newChar)];
            reflectorDisplay.Content = newChar.ToString();

            // Rotor pass backward
            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            // Plugboard pass (after rotors)
            newChar = _plugboard[newChar];

            return newChar;
        }

        // Mirror a character (encrypt and pass back through rotors)
        private char Mirror(char letter)
        {
            char newChar = Encrypt(letter);

            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            return newChar;
        }

        // Set default values
        private void SetDefaults()
        {
            _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
            _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
            _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
            _keyOffset = new int[] { 0, 0, 0 };
        }

        // Rotate rotors
        private void Rotate(bool forward)
        {
            if (forward)
            {
                _keyOffset[2]++;
                _ring3 = MoveValues(forward, _ring3);

                if (_keyOffset[2] / _control.Length >= 1)
                {
                    _keyOffset[2] = 0;
                    _keyOffset[1]++;
                    _ring2 = MoveValues(forward, _ring2);
                    if (_keyOffset[1] / _control.Length >= 1)
                    {
                        _keyOffset[1] = 0;
                        _keyOffset[0]++;
                        _ring1 = MoveValues(forward, _ring1);
                    }
                }
            }
            else
            {
                if (_keyOffset[2] > 0 || _keyOffset[1] > 0)
                {
                    _keyOffset[2]--;
                    _ring3 = MoveValues(forward, _ring3);
                    if (_keyOffset[2] < 0)
                    {
                        _keyOffset[2] = 25;
                        _keyOffset[1]--;
                        _ring2 = MoveValues(forward, _ring2);
                        if (_keyOffset[1] < 0)
                        {
                            _keyOffset[1] = 25;
                            _keyOffset[0]--;
                            _ring1 = MoveValues(forward, _ring1);
                            if (_keyOffset[0] < 0)
                                _keyOffset[0] = 25;
                        }
                    }
                }
            }

            DisplayRing(ringDisplay3, _ring3);
            DisplayRing(ringDisplay2, _ring2);
            DisplayRing(ringDisplay1, _ring1);
        }

        // Move rotor values
        private string MoveValues(bool forward, string ring)
        {
            char movingValue = ' ';
            string newRing = "";

            if (forward)
            {
                movingValue = ring[0];
                for (int x = 1; x < ring.Length; x++)
                    newRing += ring[x];
                newRing += movingValue;
            }
            else
            {
                movingValue = ring[25];
                for (int x = 0; x < ring.Length - 1; x++)
                    newRing += ring[x];
                newRing = movingValue + newRing;
            }

            return newRing;
        }

        private void RotorBTN_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();

            if (int.TryParse(ringDisplay1.Text, out _initOffset[0]) &&
                int.TryParse(ringDisplay2.Text, out _initOffset[1]) &&
                int.TryParse(ringDisplay3.Text, out _initOffset[2]))
            {
                if (_initOffset[0] >= 0 && _initOffset[0] <= 25 &&
                    _initOffset[1] >= 0 && _initOffset[1] <= 25 &&
                    _initOffset[2] >= 0 && _initOffset[2] <= 25)
                {
                    ringDisplay1.IsEnabled = false;
                    ringDisplay2.IsEnabled = false;
                    ringDisplay3.IsEnabled = false;
                    RotorBTN.IsEnabled = false;

                    _rotor = true;
                    RotorBTN.Content = "Settings Lock";

                    _ring1 = InitializeRotors(_initOffset[0], _ring1);
                    _ring2 = InitializeRotors(_initOffset[1], _ring2);
                    _ring3 = InitializeRotors(_initOffset[2], _ring3);

                    DisplayRing(ringDisplay3, _ring3);
                    DisplayRing(ringDisplay2, _ring2);
                    DisplayRing(ringDisplay1, _ring1);
                }
            }
        }

        private string InitializeRotors(int initial, string ring)
        {
            string newRing = ring;
            for (int x = 0; x < initial; x++)
                newRing = MoveValues(true, newRing);
            return newRing;
        }

        private void DisplayRing(TextBox display, string ring)
        {
            display.Text = ring[0].ToString(); 
        }

        private void PlugboardBTN_Click(object sender, RoutedEventArgs e)
        {
            configPlug.machine = this;
            configPlug.Show();

            RotorBTN.IsEnabled = true;
            PlugboardBTN.IsEnabled = false;
        }

        private void CloseAll(object sender, EventArgs e)
        {
            this.Close();
            msg.Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            InitializePlugboard();
            SetDefaults();

            configPlug = new Plugboard();

            PlugboardBTN.IsEnabled = true;
            RotorBTN.IsEnabled = false;
            ringDisplay1.IsEnabled = true;
            ringDisplay2.IsEnabled = true;
            ringDisplay3.IsEnabled = true;

            RotorBTN.Content = "Configure Rotors";
            reflectorDisplay.Content = "0";
            msg.MessageTB.Text = String.Empty;
            msg.EncryptedTB.Text = String.Empty;
            msg.MirroredTB.Text = String.Empty;


            DisplayRing(ringDisplay1, "0");
            DisplayRing(ringDisplay2, "0");
            DisplayRing(ringDisplay3, "0");
        }
    }
}
