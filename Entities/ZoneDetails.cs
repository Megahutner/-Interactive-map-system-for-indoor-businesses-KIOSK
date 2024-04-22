using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWEKiosk.Entities
{
    public class DataResponse
    {
        public string message { get; set; }
        public int code { get; set; }
        public ZoneDetails data { get; set; }
    }
    public class ZoneDetails
    {
        public int Id { get; set; }
        public string ZoneId { get; set; }
        public string ImgUrl { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public List<ZoneObject> ObjectList { get; set; }
    }

    public class ZoneObject
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string ObjectId { get; set; }
        public string Name { get; set; }
        public int Front { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public virtual double Width { get; set; }
        public virtual double Height { get; set; }
        public virtual string Color { get; set; }
        public virtual string Category { get; set; }
    }
}
