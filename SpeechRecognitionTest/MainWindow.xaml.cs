using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Speech;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace SpeechRecognitionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isRecognizing;
        private SpeechRecognitionEngine _recognitionEngine;

        private readonly Dictionary<string, string> _answers = new Dictionary<string, string>
        {
            {"Hello", "Hello" },
            {"Yes", "No" }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private static SpeechSynthesizer BuildSpeachSynthesizer() => new SpeechSynthesizer
        {
            Rate = 0,
            Volume = 100
        };

        private static void Say(string text)
        {
            var speachSynthesizer = BuildSpeachSynthesizer();
            speachSynthesizer.SpeakAsync(text);
        }

        private SpeechRecognizer BuildSpeachRecognizer()
        {
            var recognizer = new SpeechRecognizer();

            var choices = new Choices(_answers.Keys.ToArray());
            var grammarBuilder = new GrammarBuilder(choices);
            recognizer.LoadGrammar(new Grammar(grammarBuilder));

            return recognizer;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _recognitionEngine = new SpeechRecognitionEngine();
            _recognitionEngine.SetInputToDefaultAudioDevice();
            _recognitionEngine.LoadGrammar(new DictationGrammar());
            _recognitionEngine.SpeechRecognized += (o, args) =>
            {
                MessageBox.Show(args.Result.Text);
                _recognitionEngine.RecognizeAsync();
            };
        }

        private void ToggleRecognize()
        {
            try
            {
                if (_isRecognizing)
                {
                    _recognitionEngine.RecognizeAsyncCancel();
                    RecognizeOtherWayFileButton.Content = "Recognize";
                }
                else
                {
                    _recognitionEngine.RecognizeAsync();
                    RecognizeOtherWayFileButton.Content = "Stop";
                }

                _isRecognizing = !_isRecognizing;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oops: {ex.Message}");
            }
            
        }

        private void SpeakButton_OnClick(object sender, RoutedEventArgs e)
        {
            var textToSpeak = string.IsNullOrWhiteSpace(InputTextBox.Text) ? "Boooooring" : InputTextBox.Text;
            Say(textToSpeak);
        }

        private void SaveSpeachToFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textToSpeak = string.IsNullOrWhiteSpace(InputTextBox.Text) ? "Boooooring" : InputTextBox.Text;
                var speachSynthesizer = BuildSpeachSynthesizer();
                speachSynthesizer.SetOutputToWaveFile("speech.wav");
                var prompt = speachSynthesizer.SpeakAsync(textToSpeak);
                Say("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oops: {ex.Message}");
            }
        }

        private void RecognizeFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var recognizer = BuildSpeachRecognizer();
                recognizer.SpeechRecognized += (o, args) =>
                {
                    var word = args.Result.Text;
                    var answer = _answers.ContainsKey(word) ? _answers[word] : "Fuck you";
                    Say(answer);
                };
                recognizer.SpeechRecognitionRejected += (o, args) =>
                {
                    Say("Fuck you");
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oops: {ex.Message}");
            }
        }

        private void RecognizeOtherWayFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleRecognize();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oops: {ex.Message}");
            }

        }

        
    }
}
