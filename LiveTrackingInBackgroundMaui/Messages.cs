using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveTrackingInBackgroundMaui
{
    public class StartServiceMessage
    {
    }

    public class StopServiceMessage
    {
    }

    public class LocationMessage
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LocationErrorMessage
    {
    }
}
