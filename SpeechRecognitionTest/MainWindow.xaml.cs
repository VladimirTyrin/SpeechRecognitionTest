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

        private static void DoSafe(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Oops: {ex.Message}");
            }
        }

        private static void Say(string text) => DoSafe(() =>
        {
            var speachSynthesizer = BuildSpeachSynthesizer();
            speachSynthesizer.SpeakAsync(text);
            
        });

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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OutputTextBox.Text = args.Result.Text;
                    Say(args.Result.Text);
                    
                });
                
            };
            _recognitionEngine.RecognizeCompleted += (o, args) =>
            {
                if (_isRecognizing)
                    _recognitionEngine.RecognizeAsync();
            };
        }

        private void ToggleRecognize()
        {
            DoSafe(() =>
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
            });
            
        }

        private void SpeakButton_OnClick(object sender, RoutedEventArgs e)
        {
            var textToSpeak = string.IsNullOrWhiteSpace(InputTextBox.Text) ? "Boooooring" : InputTextBox.Text;
            Say(textToSpeak);
        }

        private void SaveSpeachToFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoSafe(() =>
            {
                var textToSpeak = string.IsNullOrWhiteSpace(InputTextBox.Text) ? "Boooooring" : InputTextBox.Text;
                var speachSynthesizer = BuildSpeachSynthesizer();
                speachSynthesizer.SetOutputToWaveFile("speech.wav");
                var prompt = speachSynthesizer.SpeakAsync(textToSpeak);
                Say("Done");
            });
        }

        private void RecognizeFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoSafe(() =>
            {
                var recognizer = BuildSpeachRecognizer();
                recognizer.SpeechRecognized += (o, args) =>
                {
                    var word = args.Result.Text;
                    var answer = _answers.ContainsKey(word) ? _answers[word] : "Wat";
                    Say(answer);
                };
                recognizer.SpeechRecognitionRejected += (o, args) =>
                {
                    Say("Fuck you");
                };
            });
        }

        private void RecognizeOtherWayFileButton_OnClick(object sender, RoutedEventArgs e) => DoSafe(ToggleRecognize);
    }
}
