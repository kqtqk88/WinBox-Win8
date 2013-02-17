
namespace WinBox.Utility
{
    public static class Extensions
    {
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
