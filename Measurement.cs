using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace surface_roughness {
    /// <summary>
    /// A wrapper class for the measurements that are specified in the database (from a <see cref="Program" object./>"/>
    /// </summary>
    public class Measurement {
        public long measurement_uid { get; set; }
        public long test_uid { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double height { get; set; }
        /// <summary>
        /// Create a Measurement object using the specified parameters.
        /// </summary>
        /// <param name="measurement_uid"></param>
        /// <param name="test_uid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="height"></param>
        public Measurement(long measurement_uid, long test_uid, double x, double y, double height) {
            this.measurement_uid = measurement_uid;
            this.test_uid = test_uid;
            this.x = x;
            this.y = y;
            this.height = height;
        }
    }
}
