using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace IronCompress {

    [SuppressUnmanagedCodeSecurity]
    static class Native {
        const string LibName = "nironcompress";

        static Native() {

            // lower versions will just have to rely on 64-bit only version, which is the default with no arch suffixes.

#if NETCOREAPP3_1_OR_GREATER
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
            if(libraryName != LibName)
                return IntPtr.Zero;

#if NET6_0_OR_GREATER
            string ri = RuntimeInformation.RuntimeIdentifier;
#else
            string ri = "<not supported>";
#endif

            try {
                return NativeLibrary.Load(libraryName, assembly, searchPath);
            }
            catch(DllNotFoundException ex) {
                throw new DllNotFoundException($"Unable to load {libraryName} ({RuntimeInformation.ProcessArchitecture}/{ri}/{searchPath}). CD: {Environment.CurrentDirectory}", ex);
            }
        }
#endif

        [DllImport(LibName)]
        internal static extern unsafe bool iron_compress(bool compress,
            int codec,
            byte* inputBuffer,
            int inputBufferSize,
            byte* outputBuffer,
            int* outputBufferSize,
            int compressionLevel);

        [DllImport(LibName)]
        internal static extern bool iron_ping();
    }
}
