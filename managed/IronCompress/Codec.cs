namespace IronCompress {
    public enum Codec {
        /// <summary>
        /// Google Snappy.
        /// </summary>
        Snappy = 1,

        /// <summary>
        /// Facebook Zstandard.
        /// </summary>
        Zstd = 2,

        /// <summary>
        /// Managed only.
        /// </summary>
        Gzip = 3,

        /// <summary>
        /// Brotli is part of .NET Standard 2.1 and up. Native version fallback on .NET Standard 2.0.
        /// </summary>
        Brotli = 4,

        LZO = 5,

        LZ4 = 6
    }
}