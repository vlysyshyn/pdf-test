using PdfSharp.Fonts;

namespace pdf_test.Services
{
    public class FileFontResolver : IFontResolver // FontResolverBase
    {
        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            using (var ms = new MemoryStream())
            {
                using (var fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Roboto", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo("Fonts/Roboto-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo("Fonts/Roboto-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo("Fonts/Roboto-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo("Fonts/Roboto-Regular.ttf");
                }
            }

            return null;
        }
    }
}
