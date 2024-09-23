using System.Windows.Media.Imaging;

namespace Hackathon1
{
    public abstract class AppDataItem
    {
        // Abstract method to handle the data representation
        public abstract string GetValue();
        public abstract override string ToString();
    }


    public class StringData : AppDataItem
    {
        public string Value { get; set; }

        public StringData(string value)
        {
            Value = value;
        }
        public override string ToString()
        {
            return "Text: " + GetValue();
        }

        public override string GetValue()
        {
            return Value;
        }
    }

    public class ImageData : AppDataItem
    {
        public BitmapImage Value { get; set; }
        public string Location { get; set; }
        public ImageData(string loc, BitmapImage value)
        {
            Location = loc;
            Value = value;
        }
        public override string ToString()
        {
            return "Image: " + GetValue();
        }

        public override string GetValue()
        {
            return Location;
        }
    }
}
