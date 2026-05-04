# Tic Tac Toe

Two versions of the game are included:

---

## 1. HTML Version (`index.html`)
Open directly in any browser — no setup needed.
- Dark purple/neon theme with animations
- Win overlay popup
- 2 Players or vs Computer (minimax AI)

---

## 2. C# WinForms Version (`TicTacToe.cs` + `TicTacToe.csproj`)

### Requirements
- [.NET 6 SDK](https://dotnet.microsoft.com/download) (Windows only for WinForms)

### Run it
```bash
dotnet run
```

### Build an .exe
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```
The `.exe` will be in `bin/Release/net6.0-windows/win-x64/publish/`.

---

## File Structure
```
TicTacToe/
├── index.html        ← HTML/CSS/JS version (open in browser)
├── TicTacToe.cs      ← C# WinForms game (all-in-one file)
├── TicTacToe.csproj  ← .NET project file
└── README.md
```
