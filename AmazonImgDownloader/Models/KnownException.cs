using System;

namespace AmazonImgDownloader.Models
{
    public class KnownException : Exception
    {
        public KnownException(string s) : base(s)
        {
        }
    }
}