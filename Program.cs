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
        public static string WORKING_DIRECTORY = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static string input_file { get; set; }
        public static List<Measurement> AllMeasurements { get; set; } = new List<Measurement>();
        public static List<Test> AllTests { get; set; } = new List<Test>();
        public static void Main(string[] args) {
            while (!StartupCheck()) {
                StartupCheck();
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

        public static void DoStats() {
            var lines = new List<string>();
            lines.Add("test_uid,time,plane,operator,min_height,min_height_x,min_height_y,max_height,max_height_x,max_height_y,mean,range,roughness,roughness_squared");
            foreach (Test t in AllTests) {

                Console.WriteLine($"Test #{t.test_uid}: \n");
                if (t.Measurements.Count < 1000) {
                    Console.WriteLine("INVALID TEST. DOESN'T HAVE 1000 MEASUREMENTS.\n");
                } else {
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

                    Console.WriteLine(
                    $"Min height: {min_value}mm @ ({min_x},{min_y})\n" +
                    $"Max height: {max_value}mm @ ({max_x},{max_y})\n" +
                    $"Mean height: {avg}mm\n" +
                    $"Height range: {range}mm\n" +
                    $"Avg roughness: {roughness}mm\n" +
                    $"Avg Root mean suqare roughness: {roughness_sqrd}mm\n");
                }
            }
            string output = Path.Combine(WORKING_DIRECTORY, "summary_report.csv");
            File.WriteAllLines(output, lines.ToArray());
            Console.WriteLine("\nFinished. Saved summary report to: " + output);
        }

        public static List<Measurement> LoadMeasurements() {
            var data = SQLGetColumns("Measurements", "measurement_uid", "test_uid", "x", "y", "height");
            var measurements = new List<Measurement>();
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

        public static List<Test> LoadTests() {
            var data = SQLGetColumns("Tests", "test_uid", "sTime", "PlaneID", "Operator");
            var tests = new List<Test>();
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



        public static double Sigma(int lower_bound, int upper_bound, double expression_value) {
            double sum = 0;
            for (int i = lower_bound; i <= upper_bound; i++) {
                sum += expression_value;
            }
            return sum;
        }

        /// <summary>
        /// Mostly for debugging purposes since this program only reads from SQL and stores its results in the program memory instead of actually writing anything.
        /// </summary>
        /// <param name="string_query"></param>
        public static void SQLExecute(string string_query) {
            string connection_string = $"Data Source={input_file};Version=3;Read Only=True;";
            SQLiteConnection connection = new SQLiteConnection(connection_string);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand(string_query, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }


        public static List<object[]> SQLGetColumns(string table, params string[] columns) {
            string connection_string = $"Data Source={input_file};Version=3;Read Only=True;";
            SQLiteConnection connection = new SQLiteConnection(connection_string);
            connection.Open();
            SQLiteCommand command = new SQLiteCommand($"SELECT {ColumnsToStringList(columns)} FROM {table}", connection);
            SQLiteDataReader reader = command.ExecuteReader();
            var results = new List<object[]>();
            int row = 0;

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
