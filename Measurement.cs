using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace surface_roughness {
    public class Measurement {
        public long measurement_uid { get; set; }
        public long test_uid { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double height { get; set; }
        public Measurement(long measurement_uid, long test_uid, double x, double y, double height) {
            this.measurement_uid = measurement_uid;
            this.test_uid = test_uid;
            this.x = x;
            this.y = y;
            this.height = height;
        }
    }
}
