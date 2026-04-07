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

        public MainWindow()
        {
            InitializeComponent();
        }

        // Helpers ---------------------------------------------------------

        private int GetMsDelay()
        {
            return int.TryParse(InputDelayTextBox.Text, out int ms) ? ms : 0;
        }
        private int GetInputSequenceRepeatInstances()
        {
            return int.TryParse(RepeatSequenceTextBox.Text, out int repeats) ? repeats : 1;
        }

        private bool IsValidXboxInputCommand(string inputCommand)
        {
            if (inputCommand == "A"
                || inputCommand == "B"
                || inputCommand == "X"
                || inputCommand == "Y"
                || inputCommand == "U"
                || inputCommand == "D"
                || inputCommand == "L"
                || inputCommand == "R"
                )
            {
                return true;
            }

            return false;
        }

        private string CovertXboxInputToCronusCommand(string inputCommand)
        {
            string cronusCommand = "";

            if (inputCommand == "A")
            {
                cronusCommand = "XB360_A";
            }
            else if (inputCommand == "B")
            {
                cronusCommand = "XB360_B";
            }
            else if (inputCommand == "X")
            {
                cronusCommand = "XB360_X";
            }
            else if (inputCommand == "Y")
            {
                cronusCommand = "XB360_Y";
            }
            else if (inputCommand == "U")
            {
                cronusCommand = "XB360_UP";
            }
            else if (inputCommand == "D")
            {
                cronusCommand = "XB360_DOWN";
            }
            else if (inputCommand == "L")
            {
                cronusCommand = "XB360_LEFT";
            }
            else if (inputCommand == "R")
            {
                cronusCommand = "XB360_RIGHT";
            }

            return cronusCommand;
        }

        // UI Event Handlers -----------------------------------------------

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
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read file:\n{ex.Message}",
                        "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scriptExecuteText.Length > 0)
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
                    string scriptNameComment = "// " + dialog.SafeFileName + "\n\n";
                    string[] fullScript = { scriptNameComment, _scriptHeaderText, _scriptExecuteText, _scriptFooterText };

                    try
                    {
                        File.WriteAllLines(dialog.FileName, fullScript);
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
            else
            {
                MessageBox.Show($"Unable to export script. No input sequences were provided.",
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddInputSequenceButton_Click(object sender, RoutedEventArgs e)
        {
            // Input commands are separated by commas. Separate them out to process each input command individually. 
            string[] inputCommands = SequenceTextBox.Text.Split(',');

            // If this string array is empty, then the user didn't type out any input commands. 
            if (!String.IsNullOrEmpty(inputCommands[0]))
            {
                // Keep track of the new input sequences added locally in case there are issues with any of the input commands. 
                bool appendInputSequences = true;
                string additionalInputSequences = "";
                int delay = GetMsDelay();

                for (int i = 0; i < inputCommands.Length; i++)
                {
                    if (IsValidXboxInputCommand(inputCommands[i]))
                    {
                        string cronusCommand = CovertXboxInputToCronusCommand(inputCommands[i]);
                        if (cronusCommand != "")
                        {
                            additionalInputSequences += "\tset_val(" + cronusCommand + ", 100);\n\twait(30);\n\tset_val(" + cronusCommand + ", 0);\n";

                            if (delay > 0)
                            {
                                additionalInputSequences += "\n\twait(" + delay + ");\n\n";
                            }
                        }
                    }
                    else
                    {
                        appendInputSequences = false;
                        MessageBox.Show($"Unable to process input sequence: {inputCommands[i]}.",
                            "Add Input Sequence Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                }

                if (appendInputSequences && !String.IsNullOrEmpty(additionalInputSequences))
                {
                    _scriptExecuteText += additionalInputSequences;

                    int repeatInstances = GetInputSequenceRepeatInstances();
                    if (repeatInstances > 1)
                    {
                        for (int index = repeatInstances - 1; index > 0; index--)
                        {
                            _scriptExecuteText += additionalInputSequences;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"No input sequences were provided.",
                    "Add Input Sequence Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonSequenceTextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if ((c != ',') && !char.IsLetter(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void TimeDelayTextBox_TextInput(object sender, TextCompositionEventArgs e)
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

        private void TimeDelayTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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
