# Third-Party Notices

Alternative Timer is based on LiveSplit.DetailedTimer and uses LiveSplit libraries at build time. The repository includes small compile-time DLL references in `packages` so it can build without cloning sibling LiveSplit repositories. These DLLs are not part of the normal release package; LiveSplit supplies the runtime assemblies.

## LiveSplit / LiveSplit.DetailedTimer

- Source: https://github.com/LiveSplit/LiveSplit
- Source: https://github.com/LiveSplit/LiveSplit.DetailedTimer
- Source: https://github.com/LiveSplit/LiveSplit.Timer
- License: MIT
- Copyright: Copyright (c) 2013 Christopher Serr and Sergey Papushin

Alternative Timer modifies code originally derived from LiveSplit.DetailedTimer. The Alternative Timer license in `LICENSE.txt` uses the MIT terms and preserves the original copyright notice. The `packages/LICENSE` file also carries the LiveSplit MIT license text for the checked-in compile-time references.

## SpeedrunComSharp

- Source: https://github.com/LiveSplit/SpeedrunComSharp
- License: MIT
- Copyright: Copyright (c) 2015 Christopher Serr

SpeedrunComSharp is included only as a compile-time reference because `LiveSplit.Timer.dll` references it.

## CustomFontDialog and WinFormsColor

These assemblies are taken from the LiveSplit source tree and are included only as compile-time references for settings UI compatibility.

## Microsoft .NET Framework Reference Assemblies

The checked-in `microsoft.netframework.referenceassemblies` and `microsoft.netframework.referenceassemblies.net481` package folders are used only for local/offline build compatibility. Their package metadata lists Microsoft as the author and links the license at https://github.com/Microsoft/dotnet/blob/master/LICENSE.
