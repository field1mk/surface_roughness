using System;
using System.Collections.Generic;
using System.Linq;

namespace surface_roughness {
    /// <summary>
    /// A wrapper class for the tests that are specified in the database (from a <see cref="Program" object./>"/>
    /// </summary>
    public class Test {
        public long test_uid { get; set; }
        public DateTime sTime { get; set; }
        public string PlaneID { get; set; }
        public string Operator { get; set; }
        /// <summary>
        /// A list of measurements that correspond to this particular test.
        /// </summary>
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();
        /// <summary>
        /// Creates a test object.
        /// </summary>
        /// <param name="test_uid"></param>
        /// <param name="sTime"></param>
        /// <param name="PlaneID"></param>
        /// <param name="Operator"></param>
        public Test(long test_uid, DateTime sTime, string PlaneID, string Operator) {
            this.test_uid = test_uid;
            this.sTime = sTime;
            this.PlaneID = PlaneID;
            this.Operator = Operator;
        }
        /// <summary>
        /// Gets the lowest height measurement for this object.
        /// </summary>
        /// <returns>The measurement containing the lowest height.</returns>
        public Measurement LowestMeasurment() {
            return Measurements.OrderBy(p => p.height).First();
        }
        /// <summary>
        /// Gets the heighest height measurement for this object.
        /// </summary>
        /// <returns>The measurement containing the heighest height.</returns>
        public Measurement HighestMeasurement() {
            return Measurements.OrderByDescending(p => p.height).First();
        }
        /// <summary>
        /// Gets the lowest average measurement for this test.
        /// </summary>
        public double MeasurementMean() {
            return Measurements.Average(p => p.height);
        }
        /// <summary>
        /// Gets the range of height measurements for this test.
        /// </summary>
        public double MeasurementRange() {
            return HighestMeasurement().height - LowestMeasurment().height;
        }

        /// <summary>
        /// Gets the average roughenss for this test.
        /// </summary>
        public double MeasurementAverageRoughness() {
            // Calculate the expression 1/(mn)
            int x_lower = 0;
            int y_lower = 0;
            int x_upper = Measurements.Count() - 1;
            int y_upper = Measurements.Count() - 1;
            double ratio = 1.0d / (x_upper * y_upper);

            //Calculate the "outer sum"
            double sum = 0;
            for (int ix = x_lower; ix <= x_upper; ix++) {
                //Calculate the "inner sum"
                double height = Math.Abs(Measurements[ix].height - Mu());
                sum += Program.Sigma(y_lower, y_upper, height);
            }

            // Calculate the total result and return the answer.
            double result = ratio * sum;
            return result;
        }

        /// <summary>
        /// Gets the root mean square roughenss for this test.
        /// </summary>
        public double MeasurementRootMeanSquareRoughness() {
            // Calculate the expression 1/(mn)
            int x_lower = 0;
            int y_lower = 0;
            int x_upper = Measurements.Count() - 1;
            int y_upper = Measurements.Count() - 1;
            double ratio = 1.0d / (x_upper * y_upper);

            //Calculate the "outer sum"
            double sum = 0;
            for (int ix = x_lower; ix <= x_upper; ix++) {
                //Calculate the "inner sum"
                double height = Math.Abs(Measurements[ix].height - Mu());
                double height_sqrd = Math.Pow(height, 2);
                sum += Program.Sigma(y_lower, y_upper, height_sqrd);
            }

            // Calculate the total result and return the answer.
            double result = ratio * sum;
            double resuilt_sqrt = Math.Sqrt(result);
            return resuilt_sqrt;
        }

        /// <summary>
        /// Performs a complex calculation involving the heights of this test's measurements and their locations. 
        /// </summary>
        /// <returns>The value of "mu" which is used for the value in a calculation from <see cref="MeasurementAverageRoughness"./></returns>
        public double Mu() {
            // Calculate the expression 1/(mn)
            int x_lower = 0;
            int y_lower = 0;
            int x_upper = Measurements.Count() - 1;
            int y_upper = Measurements.Count() - 1;
            double ratio = 1.0d / (x_upper * y_upper);

            //Calculate the "outer sum"
            double sum = 0;
            for (int ix = x_lower; ix <= x_upper; ix++) {
                //Calculate the "inner sum"
                sum += Program.Sigma(y_lower, y_upper, Measurements[ix].height);
            }


            // Calculate the total result and return the answer.
            double result = ratio * sum;
            return result;
        }
    }


}
