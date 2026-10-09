using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UISystem
{
    // One row per successful registration; no network or Android storage permission required.
    public sealed class RegistrationCsvStore
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(true);
        public string FilePath { get; }

        public RegistrationCsvStore(string directory)
        {
            FilePath = Path.Combine(directory, "registrations.csv");
        }

        public void Save(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Name and email are required.");

            string row = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) + "," +
                CsvCell(name.Trim()) + "," + CsvCell(email.Trim()) + "\r\n";
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            using (var stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                long originalLength = stream.Length;
                stream.Position = originalLength;
                try
                {
                    if (originalLength == 0)
                    {
                        byte[] bom = Encoding.GetPreamble();
                        stream.Write(bom, 0, bom.Length);
                        Write(stream, "RegisteredAtUtc,Name,Email\r\n");
                    }
                    Write(stream, row);
                    stream.Flush(true);
                }
                catch
                {
                    // Restore the previous file if an append fails part way through.
                    try { stream.SetLength(originalLength); stream.Flush(true); }
                    catch (IOException) { }
                    throw;
                }
            }
        }

        public IReadOnlyList<RegistrationRecord> ReadAll()
        {
            var records = new List<RegistrationRecord>();
            if (!File.Exists(FilePath)) return records;
            using (var reader = new StreamReader(FilePath, Encoding, true))
            {
                List<string[]> rows = ReadRows(reader);
                if (rows.Count == 0) return records;
                if (rows[0].Length != 3 || rows[0][0] != "RegisteredAtUtc" ||
                    rows[0][1] != "Name" || rows[0][2] != "Email")
                    throw new InvalidDataException("Unrecognized registration CSV header.");
                for (int i = 1; i < rows.Count; i++)
                {
                    string[] row = rows[i];
                    if (row.Length != 3 || !DateTime.TryParse(row[0], CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out DateTime date))
                        throw new InvalidDataException("Invalid registration CSV row " + (i + 1) + ".");
                    records.Add(new RegistrationRecord(date, DisplayCell(row[1]), DisplayCell(row[2])));
                }
            }
            return records;
        }

        private static string DisplayCell(string value)
        {
            // Undo only the spreadsheet protection prefix written by CsvCell.
            return value.Length > 1 && value[0] == '\'' && "=+-@".IndexOf(value[1]) >= 0
                ? value.Substring(1) : value;
        }

        private static List<string[]> ReadRows(TextReader reader)
        {
            var rows = new List<string[]>();
            var cells = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            bool closedQuote = false;
            bool started = false;
            int next;
            while ((next = reader.Read()) != -1)
            {
                char c = (char)next;
                if (quoted)
                {
                    if (c != '"') cell.Append(c);
                    else if (reader.Peek() == '"') { reader.Read(); cell.Append('"'); }
                    else { quoted = false; closedQuote = true; }
                    continue;
                }
                if (c == ',' || c == '\r' || c == '\n')
                {
                    cells.Add(cell.ToString()); cell.Clear(); closedQuote = false;
                    if (c == ',') { started = true; continue; }
                    if (c == '\r' && reader.Peek() == '\n') reader.Read();
                    // Ignore genuinely empty lines, but never discard an empty CSV field.
                    if (started || cells.Count > 1 || cells[0].Length > 0) rows.Add(cells.ToArray());
                    cells.Clear(); started = false;
                    continue;
                }
                if (closedQuote) throw new InvalidDataException("Unexpected text after a CSV quote.");
                if (c == '"')
                {
                    if (cell.Length > 0) throw new InvalidDataException("Unexpected CSV quote.");
                    quoted = true;
                }
                else cell.Append(c);
                started = true;
            }
            if (quoted) throw new InvalidDataException("Incomplete quoted CSV field.");
            if (started || cells.Count > 0)
            {
                cells.Add(cell.ToString()); rows.Add(cells.ToArray());
            }
            return rows;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static string CsvCell(string value)
        {
            // Excel must treat user input as text, including formula-like names.
            if ("=+-@".IndexOf(value[0]) >= 0)
                value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }

    public sealed class RegistrationRecord
    {
        public DateTime RegisteredAtUtc { get; }
        public string Name { get; }
        public string Email { get; }

        public RegistrationRecord(DateTime registeredAtUtc, string name, string email)
        {
            RegisteredAtUtc = registeredAtUtc;
            Name = name;
            Email = email;
        }
    }
}
