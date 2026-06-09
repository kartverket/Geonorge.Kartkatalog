namespace Kartverket.Metadatakatalog.Models.Translations
{
    public class CodeListValue
    {
        public CodeListValue()
        {
        }

        public CodeListValue(string value, string description = null)
        {
            Value = value;
            Description = description;
        }

        public string Value { get; set; }
        public string Description { get; set; }

        // Allows existing code that expects the label string to keep working.
        public static implicit operator string(CodeListValue item) => item?.Value;

        public override string ToString() => Value;
    }
}
