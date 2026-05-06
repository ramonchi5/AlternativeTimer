# LiveSplit.AlternativeTimer

Alternative Timer is an unofficial LiveSplit component based on LiveSplit's Detailed Timer. It keeps the big timer and the current segment name, but removes the segment timer and comparison rows entirely.

Subsplit-aware timer, subsplits must begin with a certain format for their number it to be displayed on the current segment name (check README), designed to be used in while also using the Split Detail, but works on its own.

It is intended to sit beside SplitDetail. SplitDetail can show the current segment time, while Alternative Timer stays focused on the run timer and the name of the segment you are currently playing.

## Features

- Shows the main LiveSplit timer with normal size, height, color, gradient, decimal-size, and timing-method controls.
- Shows the current segment name without the old Detailed Timer segment-time block.
- Strips subsplit prefixes from the displayed segment name.
- Optionally turns a leading segment counter like `5/5 Geni` into a separate `5 of 5` label, while the main name becomes `Geni`.
- Lets the `1 of 5` label appear above or below the segment name, with its own color, bold option, left padding, vertical move, and adjustable name gap.
- Allows the name gap to be positive, zero, or negative for tighter stacked layouts.
- Lets you choose whether the `1 of 5` label or the segment name has priority when the component is too short for both.
- Includes optional disabled-by-default backgrounds: plain, vertical gradient, horizontal gradient, 2 or 3 colors, and rounded all/top/bottom corners.

## Segment Number Convention

Alternative Timer does not auto-count subsplit groups. If you want the separate `1 of 5` display, write the number directly at the beginning of the split name:

```text
1/5 Gourd
2/5 Window
3/13 Genichiro
1/1 Tutorial
```

The component accepts any numbers in the `number/number` format. It displays the number as `1 of 5`, `2 of 5`, and so on, and removes that prefix from the main segment name.

## Install

Download the latest release DLL:

```text
LiveSplit.AlternativeTimer.dll
```

Before replacing files, make a backup of your current LiveSplit `Components` folder or at least the DLL you are replacing, and keep a backup of the layout you are editing. Then copy `LiveSplit.AlternativeTimer.dll` into LiveSplit's `Components` folder and add `Alternative Timer` from the layout editor.

For normal use, install the DLL from the latest GitHub release. The source code, `bin`, `obj`, `.vs`, and PDB files are not needed.

## Build

The repository is self-contained for normal builds. Compile-time reference DLLs live in `packages`; LiveSplit provides the matching runtime assemblies when the component runs inside LiveSplit.

Build the solution in Release mode:

```powershell
dotnet build .\LiveSplit.AlternativeTimer.sln -c Release
```

The release DLL is produced at:

```text
src\LiveSplit.AlternativeTimer\bin\Release\net481\LiveSplit.AlternativeTimer.dll
```

If a sibling `LiveSplit` repository exists, you can also copy the DLL and PDB there during build:

```powershell
dotnet build .\LiveSplit.AlternativeTimer.sln -c Release -p:CopyToLiveSplitArtifacts=true
```

That opt-in copy writes to:

```text
..\LiveSplit\artifacts\bin\AlternativeTimer\release\
```

## Release Package

For a GitHub release, upload:

```text
LiveSplit.AlternativeTimer.dll
README.md
LICENSE.txt
THIRD_PARTY_NOTICES.md
```

The PDB is only useful for debugging. Do not upload `bin`, `obj`, `.vs`, or source dependency folders as part of the normal release package.

The `packages` folder contains compile-time references only. Do not include it in the release package.

## License

MIT. See [LICENSE.txt](LICENSE.txt) and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
