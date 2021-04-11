using System;
using System.Collections.Generic;
using System.Linq;

namespace surface_roughness {
    public class Test {
        public long test_uid { get; set; }
        public DateTime sTime { get; set; }
        public string PlaneID { get; set; }
        public string Operator { get; set; }
        public List<Measurement> Measurements { get; set; } = new List<Measurement>();
        public Test(long test_uid, DateTime sTime, string PlaneID, string Operator) {
            this.test_uid = test_uid;
            this.sTime = sTime;
            this.PlaneID = PlaneID;
            this.Operator = Operator;
        }

        public Measurement LowestMeasurment() {
            return Measurements.OrderBy(p => p.height).First();
        }
        public Measurement HighestMeasurement() {
            return Measurements.OrderByDescending(p => p.height).First();
        }

        public double MeasurementMean() {
            return Measurements.Average(p => p.height);
        }
        public double MeasurementRange() {
            return HighestMeasurement().height - LowestMeasurment().height;
        }
       
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
