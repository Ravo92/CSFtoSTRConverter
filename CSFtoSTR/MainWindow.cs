using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CSFtoSTR
{
    public partial class MainWindow : Form
    {
        // Constants describing the binary markers used by the game's string format.
        private static readonly byte[] LabelMarker = { 0x20, 0x4C, 0x42, 0x4C }; // " LBL"
        private static readonly byte[] TextMarker = { 0x20, 0x52, 0x54, 0x53 }; // " RTS"

        // Output file encoding required by the game ("ANSI" in Windows == CP-1252 for Western European games).
        private static readonly Encoding OutputStrEncoding = Encoding.GetEncoding(1252);

        // Input key encoding (keys are stored as single-byte Western encoding; CP-1252 matches your files).
        private static readonly Encoding KeyEncoding = Encoding.GetEncoding(1252);

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// UI entry point: validates input/output paths, then converts the binary string file into a .str text file.
        /// The produced .str is written using Windows-1252 ("ANSI") so the game can read it.
        /// </summary>
        private void GoButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show("Select an input file");
                return;
            }

            if (!File.Exists(InputTextBox.Text))
            {
                MessageBox.Show("Can't find input file");
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputTextBox.Text))
            {
                MessageBox.Show("Select an output file");
                return;
            }

            try
            {
                using (var outputWriter = new StreamWriter(OutputTextBox.Text, false, OutputStrEncoding))
                using (var inputStream = File.Open(InputTextBox.Text, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var binaryReader = new BinaryReader(inputStream))
                {
                    ConsumeFileHeader(binaryReader);

                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        try
                        {
                            WriteNextRecordAsStr(binaryReader, outputWriter);
                        }
                        catch (InvalidDataException ex)
                        {
                            outputWriter.WriteLine("// ERROR: " + ex.Message);
                            break; // Stop on truncation/desync.
                        }
                    }
                }

                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Consumes the file header bytes. Your input files have a fixed header (24 bytes) plus one terminator byte.
        /// If this ever changes for another game/version, adjust the constants here.
        /// </summary>
        private static void ConsumeFileHeader(BinaryReader binaryReader)
        {
            long remainingBytes = binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
            if (remainingBytes < 25)
                throw new InvalidDataException("File too short for header.");

            if (!TryReadExactBytes(binaryReader, 24, out _))
                throw new InvalidDataException("Failed to read header bytes.");

            if (!TryReadSingleByte(binaryReader, out _))
                throw new InvalidDataException("Failed to read header terminator.");
        }

        /// <summary>
        /// Reads the next record from the binary file and writes it in .str format:
        /// KEY
        /// "VALUE"
        /// END
        ///
        /// Record layout (as observed from your file):
        ///   " LBL" (4 bytes)
        ///   uint32 labelId (4 bytes, unused)
        ///   uint32 keyLengthBytes (4 bytes)
        ///   keyBytes (keyLengthBytes bytes, CP-1252)
        ///   optionally: " RTS" (4 bytes)
        ///   optionally: uint32 valueLengthUtf16Units (4 bytes)
        ///   optionally: valueBytes (valueLengthUtf16Units * 2 bytes, bytewise-inverted UTF-16LE)
        ///
        /// If no " RTS" follows the key, the value is treated as empty.
        /// </summary>
        private static void WriteNextRecordAsStr(BinaryReader binaryReader, StreamWriter outputWriter)
        {
            // Re-sync: find the next " LBL" marker to avoid desync on padding/unknown bytes.
            if (!SeekToMarker(binaryReader, LabelMarker))
                throw new InvalidDataException("Desync: could not find ' LBL' marker before EOF.");

            // Consume " LBL"
            if (!TryReadExactBytes(binaryReader, 4, out _))
                throw new InvalidDataException("Truncated record (missing LBL tag).");

            // labelId/flags (unused, but must be consumed)
            if (!TryReadExactBytes(binaryReader, 4, out _))
                throw new InvalidDataException("Truncated record (missing LBL id).");

            // keyLengthBytes
            if (!TryReadExactBytes(binaryReader, 4, out var keyLengthBytesRaw))
                throw new InvalidDataException("Truncated record (missing key length).");

            uint keyLengthBytes = BitConverter.ToUInt32(keyLengthBytesRaw, 0);

            if (keyLengthBytes == 0 || keyLengthBytes > 1_000_000)
                throw new InvalidDataException("Unreasonable key length (desync).");

            // key bytes
            if (!TryReadExactBytes(binaryReader, (int)keyLengthBytes, out var keyRawBytes))
                throw new InvalidDataException("Truncated record (missing key).");

            string keyText = KeyEncoding.GetString(keyRawBytes);
            keyText = SanitizeDecodedText(keyText);

            // Peek next 4 bytes to determine whether an RTS block follows.
            long recordContinuationPosition = binaryReader.BaseStream.Position;
            byte[] nextFourBytes = binaryReader.ReadBytes((int)Math.Min(4, binaryReader.BaseStream.Length - binaryReader.BaseStream.Position));
            binaryReader.BaseStream.Position = recordContinuationPosition;

            if (nextFourBytes.Length < 4 || !MatchesMarker(nextFourBytes, TextMarker))
            {
                // No RTS value block -> empty value.
                WriteStrBlock(outputWriter, keyText, "");
                return;
            }

            // Consume " RTS"
            binaryReader.ReadBytes(4);

            // valueLengthUtf16Units
            if (!TryReadExactBytes(binaryReader, 4, out var valueLengthRaw))
                throw new InvalidDataException("Truncated record (missing value length).");

            uint valueLengthUtf16Units = BitConverter.ToUInt32(valueLengthRaw, 0);

            if (valueLengthUtf16Units > 5_000_000)
                throw new InvalidDataException("Unreasonable value length (desync).");

            int valueByteCount = checked((int)(valueLengthUtf16Units * 2));

            // valueBytes (bytewise inverted UTF-16LE)
            if (!TryReadExactBytes(binaryReader, valueByteCount, out var invertedUtf16LeBytes))
                throw new InvalidDataException("Truncated record (missing value payload).");

            // Undo inversion (b := 0xFF - b)
            for (int i = 0; i < invertedUtf16LeBytes.Length; i++)
                invertedUtf16LeBytes[i] = (byte)(0xFF - invertedUtf16LeBytes[i]);

            string valueText = Encoding.Unicode.GetString(invertedUtf16LeBytes);
            valueText = SanitizeDecodedText(valueText);

            WriteStrBlock(outputWriter, keyText, valueText);
        }

        /// <summary>
        /// Writes one .str block:
        /// KEY
        /// "VALUE"
        /// END
        ///
        /// Applies .str escaping rules:
        /// - real newlines in the decoded value become the literal sequence "\n"
        /// - quotes inside the value are doubled: " -> ""
        /// </summary>
        private static void WriteStrBlock(StreamWriter outputWriter, string key, string value)
        {
            outputWriter.WriteLine(key);
            outputWriter.WriteLine("\"" + EscapeValueForStr(value) + "\"");
            outputWriter.WriteLine("END");
            outputWriter.WriteLine();
        }

        /// <summary>
        /// Escapes a value so it is safe for the game's .str parser.
        /// - Converts real CRLF/LF into the literal text sequence '\n' (two characters).
        /// - Doubles quotes (CSV-style) because the game uses "" to represent a literal ".
        /// </summary>
        private static string EscapeValueForStr(string value)
        {
            if (value == null) return "";

            // Normalize newlines first, then turn them into the literal "\n" sequence.
            value = value.Replace("\r\n", "\n");
            value = value.Replace("\n", "\\n");

            // Quote escaping for this .str format: " becomes ""
            value = value.Replace("\"", "\"\"");

            return value;
        }

        /// <summary>
        /// Removes problematic Unicode code points produced during decode (non-characters, lone surrogates, format chars),
        /// and filters control characters except common whitespace. This prevents display/encoding issues in the output file.
        /// </summary>
        private static string SanitizeDecodedText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Remove non-characters that appear when decoding inverted UTF-16 blocks.
            text = text.Replace("\uFFFE", "").Replace("\uFFFF", "");

            var builder = new StringBuilder(text.Length);

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];

                // Remove "format" category (zero-width, BOM-like characters).
                if (char.GetUnicodeCategory(current) == UnicodeCategory.Format)
                    continue;

                // Remove unpaired surrogates.
                if (char.IsHighSurrogate(current))
                {
                    if (index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]))
                    {
                        builder.Append(current);
                        builder.Append(text[index + 1]);
                        index++;
                        continue;
                    }
                    continue;
                }

                if (char.IsLowSurrogate(current))
                    continue;

                // Remove control chars except common whitespace.
                if (char.IsControl(current) && current != '\r' && current != '\n' && current != '\t')
                    continue;

                builder.Append(current);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Searches the stream byte-by-byte until the given marker is found.
        /// Leaves the stream positioned at the first byte of the marker (does not consume it).
        /// Returns false if EOF is reached.
        /// </summary>
        private static bool SeekToMarker(BinaryReader binaryReader, byte[] marker)
        {
            if (marker == null || marker.Length == 0)
                return true;

            int matchedBytes = 0;

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                int currentByte = binaryReader.ReadByte();

                if (currentByte == marker[matchedBytes])
                {
                    matchedBytes++;

                    if (matchedBytes == marker.Length)
                    {
                        // Rewind to marker start.
                        binaryReader.BaseStream.Position -= marker.Length;
                        return true;
                    }
                }
                else
                {
                    // Simple partial-match fallback (sufficient for a 4-byte unique marker).
                    matchedBytes = (currentByte == marker[0]) ? 1 : 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the given byte array starts with the exact marker bytes.
        /// </summary>
        private static bool MatchesMarker(byte[] buffer, byte[] marker)
        {
            if (buffer == null || marker == null) return false;
            if (buffer.Length < marker.Length) return false;

            for (int i = 0; i < marker.Length; i++)
            {
                if (buffer[i] != marker[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to read exactly one byte from the stream. Returns false at EOF.
        /// </summary>
        private static bool TryReadSingleByte(BinaryReader binaryReader, out int value)
        {
            if (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                value = binaryReader.ReadByte();
                return true;
            }

            value = -1;
            return false;
        }

        /// <summary>
        /// Attempts to read an exact number of bytes from the stream.
        /// Returns false if fewer bytes than requested are available.
        /// </summary>
        private static bool TryReadExactBytes(BinaryReader binaryReader, int byteCount, out byte[] buffer)
        {
            buffer = binaryReader.ReadBytes(byteCount);
            return buffer != null && buffer.Length == byteCount;
        }

        /// <summary>
        /// Opens a file dialog and stores the selected input path in the UI.
        /// </summary>
        private void InputButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                InputTextBox.Text = openFileDialog.FileName;
        }

        /// <summary>
        /// Opens a save dialog and stores the selected output path in the UI.
        /// </summary>
        private void OutputButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                OutputTextBox.Text = saveFileDialog.FileName;
        }
    }
}