using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace GeneticAlgorithmApp
{
    public partial class MainForm : Form
    {
        // Parametry algorytmu (mo¿na zmieniaæ przez interfejs)
        private int bitsPerParameter = 7;
        private int numParameters = 2;
        private double paramMin = 0;
        private double paramMax = 100;
        private int populationSize = 11;
        private int tournamentSize = 2;
        private int maxIterations = 30;
        private double mutationProbability = 0.15;

        // Dane do wykresów
        private List<double> bestFitnessList = new List<double>();
        private List<double> averageFitnessList = new List<double>();

        private Random random = new Random();
        private bool isRunning = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeChart();
            UpdateParameterLabels();
        }

        private void InitializeComponent()
        {
            // G³ówny formularz
            this.Text = "Algorytm Genetyczny";
            this.Width = 850;
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel parametrów
            Panel paramPanel = new Panel
            {
                Left = 10,
                Top = 10,
                Width = 280,
                Height = 280,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(paramPanel);

            // Etykiety i kontrolki dla parametrów
            Label titleLabel = new Label
            {
                Text = "Parametry algorytmu",
                Left = 10,
                Top = 10,
                Width = 260,
                Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
            };
            paramPanel.Controls.Add(titleLabel);

            // Liczba bitów na parametr
            Label bitsLabel = new Label
            {
                Text = "Liczba bitów na parametr:",
                Left = 10,
                Top = 40,
                Width = 160
            };
            paramPanel.Controls.Add(bitsLabel);

            TrackBar bitsTrackBar = new TrackBar
            {
                Minimum = 3,
                Maximum = 10,
                Value = bitsPerParameter,
                Left = 170,
                Top = 35,
                Width = 100,
                TickFrequency = 1,
                Name = "bitsTrackBar"
            };
            bitsTrackBar.ValueChanged += ParameterChanged;
            paramPanel.Controls.Add(bitsTrackBar);

            // Rozmiar populacji
            Label popSizeLabel = new Label
            {
                Text = "Rozmiar populacji:",
                Left = 10,
                Top = 80,
                Width = 160
            };
            paramPanel.Controls.Add(popSizeLabel);

            TrackBar popSizeTrackBar = new TrackBar
            {
                Minimum = 9,
                Maximum = 51,
                Value = populationSize,
                Left = 170,
                Top = 75,
                Width = 100,
                TickFrequency = 2,
                Name = "popSizeTrackBar"
            };
            popSizeTrackBar.ValueChanged += ParameterChanged;
            paramPanel.Controls.Add(popSizeTrackBar);

            // Rozmiar turnieju
            Label tournamentLabel = new Label
            {
                Text = "Rozmiar turnieju:",
                Left = 10,
                Top = 120,
                Width = 160
            };
            paramPanel.Controls.Add(tournamentLabel);

            TrackBar tournamentTrackBar = new TrackBar
            {
                Minimum = 2,
                Maximum = 10,
                Value = tournamentSize,
                Left = 170,
                Top = 115,
                Width = 100,
                TickFrequency = 1,
                Name = "tournamentTrackBar"
            };
            tournamentTrackBar.ValueChanged += ParameterChanged;
            paramPanel.Controls.Add(tournamentTrackBar);

            // Liczba iteracji
            Label iterationsLabel = new Label
            {
                Text = "Liczba iteracji:",
                Left = 10,
                Top = 160,
                Width = 160
            };
            paramPanel.Controls.Add(iterationsLabel);

            TrackBar iterationsTrackBar = new TrackBar
            {
                Minimum = 20,
                Maximum = 100,
                Value = maxIterations,
                Left = 170,
                Top = 155,
                Width = 100,
                TickFrequency = 10,
                Name = "iterationsTrackBar"
            };
            iterationsTrackBar.ValueChanged += ParameterChanged;
            paramPanel.Controls.Add(iterationsTrackBar);

            // Prawdopodobieñstwo mutacji
            Label mutationLabel = new Label
            {
                Text = "Prawdopodobieñstwo mutacji:",
                Left = 10,
                Top = 200,
                Width = 160
            };
            paramPanel.Controls.Add(mutationLabel);

            TrackBar mutationTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 50,
                Value = (int)(mutationProbability * 100),
                Left = 170,
                Top = 195,
                Width = 100,
                TickFrequency = 5,
                Name = "mutationTrackBar"
            };
            mutationTrackBar.ValueChanged += ParameterChanged;
            paramPanel.Controls.Add(mutationTrackBar);

            // Przyciski
            Button startButton = new Button
            {
                Text = "Start",
                Left = 20,
                Top = 240,
                Width = 100,
                Height = 30,
                Name = "startButton"
            };
            startButton.Click += StartButtonClick;
            paramPanel.Controls.Add(startButton);

            Button resetButton = new Button
            {
                Text = "Reset",
                Left = 150,
                Top = 240,
                Width = 100,
                Height = 30,
                Name = "resetButton"
            };
            resetButton.Click += ResetButtonClick;
            paramPanel.Controls.Add(resetButton);

            // Panel wyników
            Panel resultsPanel = new Panel
            {
                Left = 300,
                Top = 10,
                Width = 520,
                Height = 280,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(resultsPanel);

            // Etykiety dla wyników
            Label resultsTitleLabel = new Label
            {
                Text = "Wyniki",
                Left = 10,
                Top = 10,
                Width = 500,
                Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
            };
            resultsPanel.Controls.Add(resultsTitleLabel);

            RichTextBox resultsTextBox = new RichTextBox
            {
                Left = 10,
                Top = 40,
                Width = 500,
                Height = 230,
                ReadOnly = true,
                Name = "resultsTextBox"
            };
            resultsPanel.Controls.Add(resultsTextBox);

            // Panel wykresu
            Panel chartPanel = new Panel
            {
                Left = 10,
                Top = 300,
                Width = 810,
                Height = 300,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(chartPanel);

            // Wykres
            Chart fitnessChart = new Chart
            {
                Left = 10,
                Top = 10,
                Width = 790,
                Height = 280,
                Name = "fitnessChart"
            };
            chartPanel.Controls.Add(fitnessChart);

            // Etykiety parametrów (aktualne wartoœci)
            Label bitsValueLabel = new Label
            {
                Left = 10,
                Top = 60,
                Width = 250,
                Name = "bitsValueLabel"
            };
            paramPanel.Controls.Add(bitsValueLabel);

            Label popSizeValueLabel = new Label
            {
                Left = 10,
                Top = 100,
                Width = 250,
                Name = "popSizeValueLabel"
            };
            paramPanel.Controls.Add(popSizeValueLabel);

            Label tournamentValueLabel = new Label
            {
                Left = 10,
                Top = 140,
                Width = 250,
                Name = "tournamentValueLabel"
            };
            paramPanel.Controls.Add(tournamentValueLabel);

            Label iterationsValueLabel = new Label
            {
                Left = 10,
                Top = 180,
                Width = 250,
                Name = "iterationsValueLabel"
            };
            paramPanel.Controls.Add(iterationsValueLabel);

            Label mutationValueLabel = new Label
            {
                Left = 10,
                Top = 220,
                Width = 250,
                Name = "mutationValueLabel"
            };
            paramPanel.Controls.Add(mutationValueLabel);
        }

        private void InitializeChart()
        {
            Chart chart = (Chart)Controls.Find("fitnessChart", true)[0];

            chart.Titles.Add("Wykres wartoœci funkcji przystosowania");

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "Iteracja";
            chartArea.AxisY.Title = "Wartoœæ funkcji przystosowania";
            chartArea.AxisX.Minimum = 0;
            chart.ChartAreas.Add(chartArea);

            Series bestSeries = new Series("Najlepszy osobnik");
            bestSeries.ChartType = SeriesChartType.Line;
            bestSeries.Color = Color.Red;
            bestSeries.BorderWidth = 2;

            Series avgSeries = new Series("Œrednia populacji");
            avgSeries.ChartType = SeriesChartType.Line;
            avgSeries.Color = Color.Blue;
            avgSeries.BorderWidth = 2;

            chart.Series.Add(bestSeries);
            chart.Series.Add(avgSeries);

            Legend legend = new Legend();
            chart.Legends.Add(legend);
        }

        private void UpdateParameterLabels()
        {
            Label bitsValueLabel = (Label)Controls.Find("bitsValueLabel", true)[0];
            Label popSizeValueLabel = (Label)Controls.Find("popSizeValueLabel", true)[0];
            Label tournamentValueLabel = (Label)Controls.Find("tournamentValueLabel", true)[0];
            Label iterationsValueLabel = (Label)Controls.Find("iterationsValueLabel", true)[0];
            Label mutationValueLabel = (Label)Controls.Find("mutationValueLabel", true)[0];

            bitsValueLabel.Text = $"Aktualnie: {bitsPerParameter} bitów";
            popSizeValueLabel.Text = $"Aktualnie: {populationSize} osobników";
            tournamentValueLabel.Text = $"Aktualnie: {tournamentSize} osobników";
            iterationsValueLabel.Text = $"Aktualnie: {maxIterations} iteracji";
            mutationValueLabel.Text = $"Aktualnie: {mutationProbability:P0}";
        }

        private void ParameterChanged(object sender, EventArgs e)
        {
            TrackBar trackBar = (TrackBar)sender;

            switch (trackBar.Name)
            {
                case "bitsTrackBar":
                    bitsPerParameter = trackBar.Value;
                    break;
                case "popSizeTrackBar":
                    // Upewniamy siê, ¿e wartoœæ jest nieparzysta
                    populationSize = trackBar.Value % 2 == 0 ? trackBar.Value + 1 : trackBar.Value;
                    break;
                case "tournamentTrackBar":
                    // Maksymalnie 20% populacji
                    tournamentSize = Math.Min(trackBar.Value, (int)(populationSize * 0.2));
                    break;
                case "iterationsTrackBar":
                    maxIterations = trackBar.Value;
                    break;
                case "mutationTrackBar":
                    mutationProbability = trackBar.Value / 100.0;
                    break;
            }

            UpdateParameterLabels();
        }

        private void StartButtonClick(object sender, EventArgs e)
        {
            if (isRunning) return;

            isRunning = true;
            Button startButton = (Button)Controls.Find("startButton", true)[0];
            startButton.Enabled = false;

            // Resetujemy dane wykresów
            bestFitnessList.Clear();
            averageFitnessList.Clear();

            RichTextBox resultsTextBox = (RichTextBox)Controls.Find("resultsTextBox", true)[0];
            resultsTextBox.Clear();

            // Uruchamiamy algorytm asynchronicznie
            Task.Run(() => RunGeneticAlgorithm());
        }

        private void ResetButtonClick(object sender, EventArgs e)
        {
            RichTextBox resultsTextBox = (RichTextBox)Controls.Find("resultsTextBox", true)[0];
            resultsTextBox.Clear();

            Chart chart = (Chart)Controls.Find("fitnessChart", true)[0];
            foreach (Series series in chart.Series)
            {
                series.Points.Clear();
            }

            bestFitnessList.Clear();
            averageFitnessList.Clear();
        }
        // Funkcja celu, któr¹ optymalizujemy
        private double TargetFunction(double x1, double x2)
        {
            return Math.Sin(x1 * 0.05) + Math.Sin(x2 * 0.05) + 0.4 * Math.Sin(x1 * 0.15) * Math.Sin(x2 * 0.15);
        }
        // Tworzenie losowego osobnika
        private List<int> CreateRandomIndividual()
        {
            int chromosomeLength = bitsPerParameter * numParameters;
            List<int> individual = new List<int>();

            for (int i = 0; i < chromosomeLength; i++)
            {
                individual.Add(random.NextDouble() < 0.5 ? 0 : 1);
            }

            return individual;
        }
        // Dekodowanie osobnika do wartoœci parametrów
        private List<double> DecodeIndividual(List<int> individual)
        {
            List<double> parameters = new List<double>();

            for (int paramIdx = 0; paramIdx < numParameters; paramIdx++)
            {
                int startIdx = paramIdx * bitsPerParameter;
                int endIdx = startIdx + bitsPerParameter;
                List<int> paramBits = individual.GetRange(startIdx, bitsPerParameter);

                // Konwersja z binarnego do dziesiêtnego
                int value = 0;
                for (int i = 0; i < paramBits.Count; i++)
                {
                    if (paramBits[i] == 1)
                    {
                        value += (int)Math.Pow(2, paramBits.Count - i - 1);
                    }
                }

                // Mapowanie wartoœci do przedzia³u [paramMin, paramMax]
                double maxBinaryValue = Math.Pow(2, bitsPerParameter) - 1;
                double normalizedValue = paramMin + (value / maxBinaryValue) * (paramMax - paramMin);
                parameters.Add(normalizedValue);
            }

            return parameters;
        }