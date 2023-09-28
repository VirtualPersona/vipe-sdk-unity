using System;
using System.Linq;

namespace CA
{
    public static class Utility
    {
        public static bool IsPng(byte[] imageBytes)
        {
            if (imageBytes == null)
            {
                return false;
            }

            byte[] pngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            return imageBytes.Take(pngSignature.Length).SequenceEqual(pngSignature);
        }
        public static bool IsJpg(byte[] imageBytes)
        {
            if (imageBytes == null)
            {
                return false;
            }

            if (imageBytes.Length < 2)
            {
                return false;
            }

            byte[] jpgStartSignature = { 255, 216 };
            byte[] jpgEndSignature = { 255, 217 };

            bool startsWithJpgSignature = imageBytes.Take(jpgStartSignature.Length).SequenceEqual(jpgStartSignature);
            bool endsWithJpgSignature = imageBytes.Skip(imageBytes.Length - jpgEndSignature.Length).SequenceEqual(jpgEndSignature);

            return startsWithJpgSignature && endsWithJpgSignature;
        }
    }
}