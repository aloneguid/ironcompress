using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace IronCompress {
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

            string prefix, suffix, arch;

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                prefix = "";
                suffix = ".dll";
                arch = RuntimeInformation.ProcessArchitecture switch {
                    Architecture.X64 => "",
                    _ => throw new NotSupportedException("Only x64 is supported on Windows"),
                };
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                prefix = "lib";
                suffix = ".so";
                arch = RuntimeInformation.ProcessArchitecture switch {
                    Architecture.X64 => "",
                    Architecture.Arm64 => "arm64",
                    _ => throw new NotSupportedException("Only x64 and ARM 64 Linux is supported."),
                };
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                prefix = "lib";
                suffix = ".dylib";
                arch = RuntimeInformation.ProcessArchitecture switch {
                    Architecture.X64 => "",
                    Architecture.Arm64 => "arm64",
                    _ => throw new NotSupportedException("Only x64 and ARM 64 MacOSX is supported."),
                };
            }
            else {
                throw new NotSupportedException($"'{Environment.OSVersion.Platform}' OS is not supported");
            }

            if(arch != "")
                arch = "-" + arch;
            string nativeName = $"{prefix}{LibName}{arch}{suffix}";
            return NativeLibrary.Load(nativeName, assembly, searchPath);
        }
#endif

        [DllImport(LibName)]
        internal static extern unsafe bool compress(
           bool compress,
           int codec,
           byte* inputBuffer,
           int inputBufferSize,
           byte* outputBuffer,
           int* outputBufferSize);
    }
}
