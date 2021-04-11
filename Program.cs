using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace surface_roughness {



    class Program {
        /// <summary>
        /// The exeutable path.
        /// </summary>
        public static string WORKING_DIRECTORY = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        /// <summary>
        /// The path for the database this program uses to work with as input.
        /// </summary>
        public static string input_file { get; set; }
        /// <summary>
        /// A collection of Measurement objects that are used for calculations.
        /// </summary>
        public static List<Measurement> AllMeasurements { get; set; } = new List<Measurement>();
        /// <summary>
        /// A collection of Test objects that are used for calculations.
        /// </summary>
        public static List<Test> AllTests { get; set; } = new List<Test>();
        public static void Main(string[] args) {
            var success = false;
            while (!success) {
                success = StartupCheck();
            }
            // Get and parse the tests from the database in a list of concrete models
            AllTests = LoadTests();

            // Same for the measurements.
            AllMeasurements = LoadMeasurements();
            foreach (Measurement m in AllMeasurements) {
                Test t = AllTests.First(p => p.test_uid == m.test_uid);
                t.Measurements.Add(m);
            }

            //Console.WriteLine("Z(Z(1)) from 0 -> 100 is: "+Sigma(0,1000-1,Sigma(0, 1000-1, 1)));

            DoStats();
            
        }

        /// <summary>
        /// Perform all calculations, output to console, and output a summary report as a csv file.
        /// </summary>
        public static void DoStats() {
            var lines = new List<string>();
            lines.Add("test_uid,time,plane,operator,min_height,min_height_x,min_height_y,max_height,max_height_x,max_height_y,mean,range,roughness,roughness_squared");
            foreach (Test t in AllTests) {

                Console.WriteLine($"Test #{t.test_uid}: \n");
                // Evaluate the validity of a test.
                if (t.Measurements.Count < 1000) {
                    Console.WriteLine("INVALID TEST. DOESN'T HAVE 1000 MEASUREMENTS.\n");
                } else {
                    //Calculate each statistic
                    Measurement min = t.LowestMeasurment();
                    double min_value = min.height;
                    double min_x = min.x;
                    double min_y = min.y;

                    Measurement max = t.HighestMeasurement();
                    double max_value = max.height;
                    double max_x = max.x;
                    double max_y = max.y;

                    double avg = t.MeasurementMean();
                    double range = t.MeasurementRange();
                    double roughness = t.MeasurementAverageRoughness();
                    double roughness_sqrd = t.MeasurementRootMeanSquareRoughness();

                    lines.Add($"{t.test_uid},{t.sTime},{t.PlaneID},{t.Operator},{min_value},{min_x},{min_y},{max_value},{max_x},{max_y},{avg},{range},{roughness},{roughness_sqrd}");
                    // Debugging/output
                    Console.WriteLine(
                    $"Min height: {min_value}mm @ ({min_x},{min_y})\n" +
                    $"Max height: {max_value}mm @ ({max_x},{max_y})\n" +
                    $"Mean height: {avg}mm\n" +
                    $"Height range: {range}mm\n" +
                    $"Avg roughness: {roughness}mm\n" +
                    $"Avg Root mean suqare roughness: {roughness_sqrd}mm\n");
                }
            }
            // Create a summary file and save it to the working directory.
            string output = Path.Combine(WORKING_DIRECTORY, "summary_report.csv");
            File.WriteAllLines(output, lines.ToArray());
            Console.WriteLine("\nFinished. Saved summary report to: " + output);
        }

        /// <summary>
        /// Read from the <see cref="input_file" database and store these Measurements in <see cref="AllMeasurements"./>/>
        /// </summary>
        /// <returns>A list of <see cref="Measurement" objects./></returns>
        public static List<Measurement> LoadMeasurements() {
            var data = SQLGetColumns("Measurements", "measurement_uid", "test_uid", "x", "y", "height");
            var measurements = new List<Measurement>();
            // Parse the text and create individual Measurement objects and store them into a list.
            foreach (object[] entry in data) {
                Measurement m = new Measurement(
                    (long)entry[0],
                    (long)entry[1],
                    (double)entry[2],
                    (double)entry[3],
                    (double)entry[4]);
                measurements.Add(m);
            }
            return measurements;
        }

        /// <summary>
        /// Read from the <see cref="input_file" database and store these Tests in <see cref="AllTests"./>/>
        /// </summary>
        /// <returns>A list of <see cref="Test" objects./></returns>
        public static List<Test> LoadTests() {
            var data = SQLGetColumns("Tests", "test_uid", "sTime", "PlaneID", "Operator");
            var tests = new List<Test>();

            //Parse the string into individual Test objects, and store them into a list.
            foreach (object[] entry in data) {
                string op = entry[3] != DBNull.Value && entry[3] != null ? (string)entry[3] : "";
                Test t = new Test(
                    (long)entry[0],
                    (DateTime)entry[1],
                    (string)entry[2],
                    op);
                tests.Add(t);
            }
            return tests;
        }

        /// <summary>
        /// Form an expert system to ensure that the user correctly inputs a path to a database file.
        /// </summary>
        /// <returns>Whether or not the user successfully input a path.</returns>
        public static bool StartupCheck() {
            Console.Write("Enter a full path for a database as input: ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) {
                Console.Write("You didn't provide a response. ");
                return false;
            }

            input_file = input;

            if (!File.Exists(input_file)) {
                Console.Write("This is not a valid path to a database. ");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Perform a sum on a value for a set amount of repetitions.
        /// </summary>
        /// <param name="lower_bound">The starting point. Inclusive.</param>
        /// <param name="upper_bound">The ending point. Inclusive.</param>
        /// <param name="expression_value">The value to "repeate and sum".</param>
        /// <returns></returns>
        public static double Sigma(int lower_bound, int upper_bound, double expression_value) {
            double sum = 0;
            for (int i = lower_bound; i <= upper_bound; i++) {
                sum += expression_value;
            }
            return sum;
        }
        /// <summary>
        /// Get a table of values through SQLite, from the specified columns.
        /// </summary>
        /// <param name="table">The name of the table from <see cref="input_file"/></param>
        /// <param name="columns">The columns to retreive.</param>
        /// <returns>A table (list of rows) of the database.</returns>
        public static List<object[]> SQLGetColumns(string table, params string[] columns) {
            // Connect to the database.
            string connection_string = $"Data Source={input_file};Version=3;Read Only=True;";
            SQLiteConnection connection = new SQLiteConnection(connection_string);
            connection.Open();

            // From a command using a list of strings specified by the arguments.
            SQLiteCommand command = new SQLiteCommand($"SELECT {ColumnsToStringList(columns)} FROM {table}", connection);
            SQLiteDataReader reader = command.ExecuteReader();
            var results = new List<object[]>();
            int row = 0;

            // Scan the contents of the sqlite data reader and store them in a table, and close when finished.
            while (reader.Read()) {
                results.Add(new object[columns.Length]);
                for (int val = 0; val < columns.Length; val++) {
                    results[row][val] = reader[columns[val]];
                }
                row++;
            }
            connection.Close();
            return results;
        }
        /// <summary>
        /// Helper method to perform a string SELECT query. Inputs a list of columns to convert to a comma-separated list.
        /// </summary>
        /// <param name="columns">The list of columns to convert.</param>
        /// <returns>The string comma-seperated list of columns.</returns>
        public static string ColumnsToStringList(params string[] columns) {
            string text = "";
            for (int i = 0; i < columns.Length; i++) {
                text += columns[i];
                if (i < columns.Length - 1) {
                    text += ",";
                }
            }

            return text;
        }


    }
}
