namespace ITWebNet.Food.Site.Models
{
    public class PostImageModel
    {
        public string Filename { get; set; }
        public byte[] Content { get; set; }
    }

    public class GetImageModel
    {

        public string Hash { get; set; }

        public GetImageModel SetHash(string value)
        {
            Hash = value;
            return this;
        }

        public string Extension { get; set; } = "png";

        public GetImageModel SetExtension(string value)
        {
            Extension = value;
            return this;
        }

        public int Height { get; set; }

        public GetImageModel SetHeight(int value)
        {
            Height = value;
            return this;
        }

        public int Width { get; set; }

        public GetImageModel SetWidth(int value)
        {
            Width = value;
            return this;
        }

        public ScaleImage Scale { get; set; }

        public GetImageModel SetScale(ScaleImage scale)
        {
            Scale = scale;
            return this;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Hash))
                return null;

            string srcPath;

            if (Height > 0 && Width > 0)
            {
                srcPath = string.Format(
                    "{0}_{1}x{2}.{3}", Hash, Width, Height, Extension);
                switch (Scale)
                {
                    case ScaleImage.Height:
                        srcPath = string.Format(
                            "{0}_{1}x{2}_{3}.{4}", Hash, Width, Height, "h", Extension);
                        break;
                    case ScaleImage.Width:
                        srcPath = string.Format(
                            "{0}_{1}x{2}_{3}.{4}", Hash, Width, Height, "w", Extension);
                        break;
                    default:
                        break;
                }
            }
            else
                srcPath = string.Format("{0}.{1}", Hash, Extension);

            return srcPath;
        }
    }

    public enum ScaleImage
    {
        Unknown = 0,
        Width = 1,
        Height = 2
    }
}