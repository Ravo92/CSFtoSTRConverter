# CSFtoSTR Converter

**CSFtoSTR Converter** is a Windows tool that converts proprietary game binary string files  
(commonly known as **CSF**) into human-readable **".str" files** that are fully compatible with the game engine.

Supportet games:  
[The Lord of the Rings™: Battle for Middle Earth™](https://en.wikipedia.org/wiki/The_Lord_of_the_Rings:_The_Battle_for_Middle-earth)  
[The Lord of the Rings™: Battle for Middle Earth II™](https://en.wikipedia.org/wiki/The_Lord_of_the_Rings:_The_Battle_for_Middle-earth_II)  
[The Lord of the Rings™: The Battle for Middle-earth II™: The Rise of the Witch-king™](https://en.wikipedia.org/wiki/The_Lord_of_the_Rings:_The_Battle_for_Middle-earth_II:_The_Rise_of_the_Witch-king)  

The generated ".str" files can be edited safely and loaded by the game **without crashes**,  
while preserving all formatting rules required by the original parser.

---

## ✨ What This Tool Does

- Reads the game’s **binary string format** ("LBL" / "RTS" records)
- Correctly decodes:
  - **Keys** using Windows-1252 (ANSI)
  - **Values** using inverted UTF-16LE
- Exports strings into classic **".str" files**
- Applies **all required escaping rules** for game compatibility
- Produces output suitable for:
  - modding
  - translation
  - debugging
  - verification / round-trip testing

---

## 📄 Output Format (".str")

Each record is written in the engine’s native string format:

>CONTROLBAR:ToolTipConstructMenFarm
>"Generates resources depending on the available terrain\nIncreases command point limit by 50"
>END

---

## 📌 Important Format Rules

This tool enforces the exact rules expected by the game:

### Line breaks
- **No real line breaks inside strings**
- All line breaks are written as the literal sequence:
  """
  \n
  """

### Quotation marks
- Quotes inside strings use **CSV-style escaping**
- A literal quote is written as:
  """
  ""
  """
- ❌ Backslash escapes ("\"") are **not allowed**

### Encoding
- Output encoding is **ANSI / Windows-1252**
- UTF-8 is **not safe** and may break the game's ability to proberly display strings

---

## 🔒 Why This Tool Is Necessary

The game’s string parser is extremely strict:

- A single unescaped """ inside a string can fail the game's ability to proberly display strings
- UTF-8 multi-byte characters break internal length assumptions
- Real line breaks inside quoted strings are invalid
- This tool removes the need of manually converting the CSF files via Hex-Editor (more user friendly)

---

## 🧠 How It Works (Internals)

### Binary format (simplified)

"""
" LBL"
uint32 labelId
uint32 keyLength
byte[] key (Windows-1252)

optional:
" RTS"
uint32 valueLength (UTF-16 code units)
byte[] value (UTF-16LE, bytewise inverted)
"""

### Conversion steps

1. Scan for "LBL" markers to maintain sync
2. Decode keys using Windows-1252
3. Decode values by:
   - inverting each byte ("0xFF - b")
   - decoding UTF-16LE
4. Remove invalid Unicode remnants
5. Escape the value for ".str" output
6. Write the final ".str" block

---

## ✅ Guarantees

- Output ".str" files:
  - load correctly in-game
  - support umlauts and special characters
  - support embedded quotes
  - support multi-line tooltips via "\n"

---

## 🖥 Requirements

- Windows
- .NET 10 (WinForms)
- Input files using the "*.CSF" format

---

## ⚠ Notes

- This tool currently focuses on **binary → ".str" export**
- The ".str" output is intentionally conservative and parser-safe
- Only "\n" is treated as a line break escape