# Advanced Calculator Pro

A modern, fast, and feature-rich desktop calculator built with **C# 12**, **.NET 8**, **WPF**, and **MVVM**, following **Clean Architecture** principles.

---

## Screenshots

| Standard Calculator | Programmer Calculator |
| :---: | :---: |
| ![Standard Calculator](src/screenshot_standard.png) | ![Programmer Calculator](src/screenshot_programmer.png) |

| Scientific Calculator | Unit Converter |
| :---: | :---: |
| ![Scientific Calculator](src/screenshot_scientific.png) | ![Unit Converter](src/screenshot_converter.png) |

---

## Features

* **Standard Calculator**:
  * Arithmetic operations (`+`, `-`, `*`, `/`, `%`, `±`, `√`, `1/x`, `x²`).
  * Dedicated memory system (`MC`, `MR`, `M+`, `M-`, `MS`).
  * Expression display with live operator precedence and evaluation status.

* **Scientific Calculator**:
  * Trigonometric functions: `sin`, `cos`, `tan`, `asin`, `acos`, `atan`.
  * Hyperbolic functions: `sinh`, `cosh`, `tanh`, `asinh`, `acosh`, `atanh`.
  * Logarithmic & Exponential: `log10`, `ln`, `exp`, `10^x`, `x^y`.
  * Constants: `π`, `e`, `phi`.
  * Angle modes: `DEG`, `RAD`, `GRAD`.
  * Special functions: Factorial (`n!`), Absolute value (`|x|`), `cbrt`.

* **Programmer Calculator**:
  * Simultaneous real-time 4-base representation: **HEX**, **DEC**, **OCT**, **BIN**.
  * Bit sizes: `QWORD` (64-bit), `DWORD` (32-bit), `WORD` (16-bit), `BYTE` (8-bit).
  * Bitwise logic: `AND`, `OR`, `XOR`, `NOT`, `NAND`, `NOR`.
  * Bit shifts: `Shift Left (Lsh)`, `Shift Right (Rsh)`.

* **Unit Converter**:
  * 10 Categories: Length, Weight, Temperature, Area, Volume, Speed, Time, Data Storage, Energy, Pressure.
  * Instant 2-way conversion with swap support.

* **Currency Converter**:
  * Supports major world currencies (USD, EUR, GBP, SAR, AED, EGP, KWD, JPY, etc.).
  * Offline-capable rate engine with decoupled service design.

* **Calculation History**:
  * Persistent storage via **SQLite** and **Dapper**.
  * Search, filter, pin/star favorite calculations, copy results, and delete records.
  * Export history to **CSV** or **Plain Text**.

* **Settings & Customization**:
  * Light & Dark themes.
  * Configurable decimal precision (0–15 digits).
  * Auto-copy result to clipboard.
  * Always on Top mode.

* **Keyboard Support**:
  * Number keypad, operators, Enter (`=`), Escape (`Clear`), Backspace (`Delete`), `Ctrl+C` (Copy), `Ctrl+V` (Paste), `F1`–`F6` (Quick Mode Navigation).

---

## Architecture

The solution follows Clean Architecture boundaries:

* `AdvancedCalculator.Core`: Domain entities, value objects, interfaces, and domain exceptions without external framework dependencies.
* `AdvancedCalculator.Application`: Custom math parser (Lexer + Shunting-Yard AST evaluator) and application services.
* `AdvancedCalculator.Infrastructure`: SQLite database access via Dapper and local JSON configuration persistence.
* `AdvancedCalculator.UI`: WPF presentation layer with MVVM, Microsoft DI, and responsive modern styles.
* `AdvancedCalculator.Tests`: Unit test suite using xUnit and FluentAssertions.

---

## Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
* Windows 10/11 (WPF runtime).

### Build and Run
```bash
# Clone the repository
git clone https://github.com/XREFS0/AdvancedCalculator.git
cd AdvancedCalculator

# Restore & Build
dotnet build

# Run Unit Tests
dotnet test

# Launch the Application
dotnet run --project AdvancedCalculator.UI/AdvancedCalculator.UI.csproj
```

---

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
