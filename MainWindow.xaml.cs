using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace WpfTextTool
{
    public partial class MainWindow : Window
    {
        private string _scriptHeaderText = "int run_script = 0;\n\nmain\n{\n\tif (get_val(XB1_LS))\n\t{\n\t\trun_script = 1;\n\t}\n\n\tif (get_val(XB1_RS))\n\t{\n\t\trun_script = 0;\n\t}\n\n\tif (run_script == 1)\n\t{\n\t\tcombo_run(ExecuteScript);\n\t}\n}\n\ncombo ExecuteScript\n{";
        private string _scriptFooterText = "}\n";
        private string _scriptExecuteText = String.Empty;
        private DateTime _lastButtonPress = DateTime.MinValue;
        private string _inputCommands = "ABXYUDLR";

        public MainWindow()
        {
            InitializeComponent();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private int GetMsDelay()
        {
            return int.TryParse(MsTextBox.Text, out int ms) ? ms : 0;
        }

        private bool IsTooSoon(string buttonName)
        {
            int delay = GetMsDelay();
            if (delay <= 0) return false;

            double elapsed = (DateTime.Now - _lastButtonPress).TotalMilliseconds;
            if (elapsed < delay)
            {
                int remaining = (int)(delay - elapsed);
                MessageBox.Show(
                    $"Please wait {remaining} ms before pressing {buttonName} again.",
                    "Too Soon",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return true;
            }
            return false;
        }

        private bool IsValidInputCommand(char inputCommand)
        {
            if (inputCommand == 'A'
                || inputCommand == 'B'
                || inputCommand == 'X'
                || inputCommand == 'Y'
                || inputCommand == 'U'
                || inputCommand == 'D'
                || inputCommand == 'L'
                || inputCommand == 'R'
                )
            {
                return true;
            }

            return false;
        }

        private string CovertInputCommandToCronusCommand(char inputCommand)
        {
            string cronusCommand = "";

            if (inputCommand == 'A')
            {
                cronusCommand = "XB360_A";
            }
            else if (inputCommand == 'B')
            {
                cronusCommand = "XB360_B";
            }
            else if (inputCommand == 'X')
            {
                cronusCommand = "XB360_X";
            }
            else if (inputCommand == 'Y')
            {
                cronusCommand = "XB360_Y";
            }
            else if (inputCommand == 'U')
            {
                cronusCommand = "XB360_UP";
            }
            else if (inputCommand == 'D')
            {
                cronusCommand = "XB360_DOWN";
            }
            else if (inputCommand == 'L')
            {
                cronusCommand = "XB360_LEFT";
            }
            else if (inputCommand == 'R')
            {
                cronusCommand = "XB360_RIGHT";
            }

            return cronusCommand;
        }

        // ── Import ─────────────────────────────────────────────────────────────

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Import Text File",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _scriptHeaderText = File.ReadAllText(dialog.FileName);
                    ExportButton.IsEnabled = true;
                    _lastButtonPress = DateTime.Now;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read file:\n{ex.Message}",
                        "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Export ─────────────────────────────────────────────────────────────

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Generate Cronus Script",
                Filter = "GPC Files (*.gpc)|*.gpc|All Files (*.*)|*.*",
                DefaultExt = ".gpc",
                FileName = "script1"
            };

            if (dialog.ShowDialog() == true)
            {
                // Convert the input commands, if valid, to cronus script text
                char[] commands = _inputCommands.ToCharArray();
                for (int index = 0; index < commands.Length; index++)
                {
                    if (IsValidInputCommand(commands[index]) )
                    {
                        string cronusCommand = CovertInputCommandToCronusCommand(commands[index]);
                        if (cronusCommand != "")
                        {
                            _scriptExecuteText += "\tset_val(" + cronusCommand + ", 100);\n\twait(30);\n\tset_val(" + cronusCommand + ", 0);\n";

                            int delay = GetMsDelay();
                            if (delay > 0)
                            {
                                _scriptExecuteText += "\n\twait(" + delay + ");\n\n";
                            }
                        }
                    }
                }

                string scriptNameComment = "// " + dialog.SafeFileName + "\n\n";
                string[] fullScript = { scriptNameComment, _scriptHeaderText, _scriptExecuteText, _scriptFooterText };

                try
                {
                    File.WriteAllLines(dialog.FileName, fullScript);
                    _lastButtonPress = DateTime.Now;
                    MessageBox.Show("File exported successfully.",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to write file:\n{ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Numbers-only TextBox ───────────────────────────────────────────────

        private void MsTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void MsTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                foreach (char c in text)
                {
                    if (!char.IsDigit(c))
                    {
                        e.CancelCommand();
                        return;
                    }
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
