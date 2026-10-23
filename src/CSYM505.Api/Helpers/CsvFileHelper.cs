namespace CSYM505.Api.Helpers
{
    using Microsoft.VisualBasic.FileIO;

    public static class CsvFileHelper
    {
        public static async Task<IEnumerable<string[]>> ReadRowsAsync(string filePath, string[] expectedColumns)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var rows = ParseRows(content, filePath, expectedColumns);

            return rows;
        }

        private static IEnumerable<string[]> ParseRows(string content, string filePath, string[] expectedColumns)
        {
            using var reader = new StringReader(content);
            using var parser = new TextFieldParser(reader);

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = false;

            var columns = parser.ReadFields();

            if (columns == null || !columns.SequenceEqual(expectedColumns))
            {
                throw new FormatException($"The CSV columns do not match the expected columns: {filePath}");
            }

            while (!parser.EndOfData)
            {
                var lineNumber = parser.LineNumber;
                var fields = parser.ReadFields();

                if (fields == null || fields.Length != expectedColumns.Length)
                {
                    throw new FormatException($"Invalid number of CSV fields at line {lineNumber}: {filePath}");
                }

                yield return fields;
            }
        }
    }
}
