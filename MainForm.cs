using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeneticAlgorithmApp
{
    public partial class MainForm : Form
    {
        private List<Sample> samples = new List<Sample>();
        private Random random = new Random();

        // Genetic algorithm parameters
        private const int PopulationSize = 13;
        private const int TournamentSize = 3;
        private const int ChromosomeBitsPerParameter = 8; // At least 4 bits per parameter
        private const int NumberOfParameters = 3; // pa, pb, pc
        private const int MaxIterations = 100; // At least 100 iterations
        private const double MutationRate = 0.1;
        private const double CrossoverRate = 0.7;
        private const double ParameterMin = 0.0;
        private const double ParameterMax = 3.0;
        
        private List<Individual> population = new List<Individual>();
        private int currentIteration = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Setup interface
            txtIterations.Text = MaxIterations.ToString();
            txtChromosomeBits.Text = ChromosomeBitsPerParameter.ToString();
            
            // Load data button click event
            btnLoadData.Click += (s, args) => LoadData();
            
            // Start button click event
            btnStart.Click += (s, args) => StartAlgorithm();
        }

        private void InitializeComponent()
        {
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnLoadData = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtIterations = new System.Windows.Forms.TextBox();
            this.txtChromosomeBits = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 90);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(776, 348);
            this.txtLog.TabIndex = 0;
            // 
            // btnLoadData
            // 
            this.btnLoadData.Location = new System.Drawing.Point(12, 12);
            this.btnLoadData.Name = "btnLoadData";
            this.btnLoadData.Size = new System.Drawing.Size(131, 23);
            this.btnLoadData.TabIndex = 1;
            this.btnLoadData.Text = "Load Data";
            this.btnLoadData.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(149, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(131, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start Algorithm";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // txtIterations
            // 
            this.txtIterations.Location = new System.Drawing.Point(149, 50);
            this.txtIterations.Name = "txtIterations";
            this.txtIterations.Size = new System.Drawing.Size(100, 20);
            this.txtIterations.TabIndex = 3;
            // 
            // txtChromosomeBits
            // 
            this.txtChromosomeBits.Location = new System.Drawing.Point(401, 50);
            this.txtChromosomeBits.Name = "txtChromosomeBits";
            this.txtChromosomeBits.Size = new System.Drawing.Size(100, 20);
            this.txtChromosomeBits.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Number of Iterations (min 100):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(255, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(140, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Chromosome Bits (min 4):";
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtChromosomeBits);
            this.Controls.Add(this.txtIterations);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnLoadData);
            this.Controls.Add(this.txtLog);
            this.Name = "MainForm";
            this.Text = "Genetic Algorithm - Parameter Optimization";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private TextBox txtLog;
        private Button btnLoadData;
        private Button btnStart;
        private TextBox txtIterations;
        private TextBox txtChromosomeBits;
        private Label label1;
        private Label label2;

        private void LoadData()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Text Files|*.txt",
                    Title = "Select Sinusik Data File"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    LoadSamplesFromFile(filePath);
                    txtLog.AppendText($"Loaded {samples.Count} samples from {filePath}\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSamplesFromFile(string filePath)
        {
            samples.Clear();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] values = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length >= 2)
                {
                    if (double.TryParse(values[0], out double x) && double.TryParse(values[1], out double y))
                    {
                        samples.Add(new Sample { X = x, Y = y });
                    }
                }
            }
        }

        private void StartAlgorithm()
        {
            if (samples.Count == 0)
            {
                MessageBox.Show("Please load the sample data first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse iterations and chromosome bits
            if (!int.TryParse(txtIterations.Text, out int iterations) || iterations < 100)
            {
                MessageBox.Show("Number of iterations must be at least 100.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtChromosomeBits.Text, out int bits) || bits < 4)
            {
                MessageBox.Show("Chromosome bits per parameter must be at least 4.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Initialize the algorithm
            InitializePopulation(bits);

            // Evaluate initial population
            EvaluatePopulation();
            
            // Print initial best and average fitness
            Individual bestIndividual = GetBestIndividual();
            double avgFitness = GetAverageFitness();
            txtLog.AppendText($"Initial - Best Fitness: {bestIndividual.Fitness:F6}, Avg Fitness: {avgFitness:F6}, " +
                               $"Parameters: pa={bestIndividual.Parameters[0]:F6}, pb={bestIndividual.Parameters[1]:F6}, pc={bestIndividual.Parameters[2]:F6}\r\n");

            // Run the algorithm iteratively
            for (int i = 0; i < iterations; i++)
            {
                // Perform one generation
                PerformGeneration();
                
                // Update best and average fitness
                bestIndividual = GetBestIndividual();
                avgFitness = GetAverageFitness();
                
                // Log results
                txtLog.AppendText($"Iteration {i+1} - Best Fitness: {bestIndividual.Fitness:F6}, Avg Fitness: {avgFitness:F6}, " +
                                   $"Parameters: pa={bestIndividual.Parameters[0]:F6}, pb={bestIndividual.Parameters[1]:F6}, pc={bestIndividual.Parameters[2]:F6}\r\n");
                
                // Auto-scroll to bottom
                txtLog.SelectionStart = txtLog.Text.Length;
                txtLog.ScrollToCaret();
                
                // Process UI events to keep the form responsive
                Application.DoEvents();
            }

            // Final results
            bestIndividual = GetBestIndividual();
            txtLog.AppendText($"\r\n--- Final Results ---\r\n");
            txtLog.AppendText($"Best Parameters: pa={bestIndividual.Parameters[0]:F6}, pb={bestIndividual.Parameters[1]:F6}, pc={bestIndividual.Parameters[2]:F6}\r\n");
            txtLog.AppendText($"Best Fitness: {bestIndividual.Fitness:F6}\r\n");
        }

        private void InitializePopulation(int bitsPerParameter)
        {
            population = new List<Individual>();
            
            // Create initial population with random chromosomes
            for (int i = 0; i < PopulationSize; i++)
            {
                bool[] chromosome = new bool[bitsPerParameter * NumberOfParameters];
                
                // Initialize with random bits
                for (int j = 0; j < chromosome.Length; j++)
                {
                    chromosome[j] = random.Next(2) == 1;
                }
                
                Individual individual = new Individual
                {
                    Chromosome = chromosome,
                    Parameters = new double[NumberOfParameters],
                    BitsPerParameter = bitsPerParameter
                };
                
                population.Add(individual);
            }
        }

        private void EvaluatePopulation()
        {
            foreach (Individual individual in population)
            {
                // Decode chromosome to parameters
                DecodeChromosome(individual);
                
                // Calculate fitness
                individual.Fitness = CalculateFitness(individual.Parameters);
            }
        }

        private void DecodeChromosome(Individual individual)
        {
            int bitsPerParameter = individual.BitsPerParameter;
            
            for (int i = 0; i < NumberOfParameters; i++)
            {
                int startBit = i * bitsPerParameter;
                int value = 0;
                
                // Convert binary to decimal
                for (int j = 0; j < bitsPerParameter; j++)
                {
                    if (individual.Chromosome[startBit + j])
                    {
                        value |= (1 << j);
                    }
                }
                
                // Map to parameter range [0, 3]
                double normalizedValue = (double)value / ((1 << bitsPerParameter) - 1);
                individual.Parameters[i] = ParameterMin + normalizedValue * (ParameterMax - ParameterMin);
            }
        }

        private double CalculateFitness(double[] parameters)
        {
            double pa = parameters[0];
            double pb = parameters[1];
            double pc = parameters[2];
            double sumSquaredError = 0;
            
            foreach (Sample sample in samples)
            {
                double predictedY = CalculateFunction(sample.X, pa, pb, pc);
                double error = sample.Y - predictedY;
                sumSquaredError += error * error;
            }
            
            return sumSquaredError;
        }

        private double CalculateFunction(double x, double pa, double pb, double pc)
        {
            // This is our function f(x; pa, pb, pc)
            // Based on context, it looks like we need to find a function to fit the data
            // A common approach is to use a parametric function like:
            // f(x) = pa * sin(pb * x + pc)
            return pa * Math.Sin(pb * x + pc);
        }

        private void PerformGeneration()
        {
            List<Individual> newPopulation = new List<Individual>();
            
            // Get best individual for hot deck (elitism)
            Individual bestIndividual = GetBestIndividual();
            
            // Selection and crossover - 4 individuals
            for (int i = 0; i < 2; i++)
            {
                Individual parent1 = TournamentSelection();
                Individual parent2 = TournamentSelection();
                
                // Perform crossover
                var (child1, child2) = Crossover(parent1, parent2);
                
                // Add children to new population
                newPopulation.Add(child1);
                newPopulation.Add(child2);
            }
            
            // Selection and mutation - 4 individuals
            for (int i = 0; i < 4; i++)
            {
                Individual parent = TournamentSelection();
                Individual child = DeepCopy(parent);
                
                // Perform mutation
                Mutate(child);
                
                // Add child to new population
                newPopulation.Add(child);
            }
            
            // Selection, crossover and mutation - 4 individuals
            for (int i = 0; i < 2; i++)
            {
                Individual parent1 = TournamentSelection();
                Individual parent2 = TournamentSelection();
                
                // Perform crossover
                var (child1, child2) = Crossover(parent1, parent2);
                
                // Perform mutation
                Mutate(child1);
                Mutate(child2);
                
                // Add children to new population
                newPopulation.Add(child1);
                newPopulation.Add(child2);
            }
            
            // Add best individual from old population (hot deck)
            newPopulation.Add(DeepCopy(bestIndividual));
            
            // Evaluate new population
            foreach (Individual individual in newPopulation)
            {
                DecodeChromosome(individual);
                individual.Fitness = CalculateFitness(individual.Parameters);
            }
            
            // Replace old population
            population = newPopulation;
        }

        private Individual TournamentSelection()
        {
            // Randomly select tournament participants
            List<Individual> tournament = new List<Individual>();
            
            for (int i = 0; i < TournamentSize; i++)
            {
                int index = random.Next(population.Count);
                tournament.Add(population[index]);
            }
            
            // Return the best individual from the tournament
            return tournament.OrderBy(i => i.Fitness).First();
        }

        private (Individual, Individual) Crossover(Individual parent1, Individual parent2)
        {
            Individual child1 = DeepCopy(parent1);
            Individual child2 = DeepCopy(parent2);
            
            // Perform single-point crossover
            if (random.NextDouble() < CrossoverRate)
            {
                int crossoverPoint = random.Next(1, parent1.Chromosome.Length - 1);
                
                for (int i = crossoverPoint; i < parent1.Chromosome.Length; i++)
                {
                    // Swap genes
                    bool temp = child1.Chromosome[i];
                    child1.Chromosome[i] = child2.Chromosome[i];
                    child2.Chromosome[i] = temp;
                }
            }
            
            return (child1, child2);
        }

        private void Mutate(Individual individual)
        {
            for (int i = 0; i < individual.Chromosome.Length; i++)
            {
                if (random.NextDouble() < MutationRate)
                {
                    // Flip bit
                    individual.Chromosome[i] = !individual.Chromosome[i];
                }
            }
        }

        private Individual GetBestIndividual()
        {
            return population.OrderBy(i => i.Fitness).First();
        }

        private double GetAverageFitness()
        {
            return population.Average(i => i.Fitness);
        }

        private Individual DeepCopy(Individual source)
        {
            Individual copy = new Individual
            {
                Chromosome = new bool[source.Chromosome.Length],
                Parameters = new double[source.Parameters.Length],
                Fitness = source.Fitness,
                BitsPerParameter = source.BitsPerParameter
            };
            
            // Copy chromosome
            Array.Copy(source.Chromosome, copy.Chromosome, source.Chromosome.Length);
            
            // Copy parameters
            Array.Copy(source.Parameters, copy.Parameters, source.Parameters.Length);
            
            return copy;
        }
    }

    public class Individual
    {
        public bool[] Chromosome { get; set; } = Array.Empty<bool>();
        public double[] Parameters { get; set; } = Array.Empty<double>();
        public double Fitness { get; set; }
        public int BitsPerParameter { get; set; }
    }

    public class Sample
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}