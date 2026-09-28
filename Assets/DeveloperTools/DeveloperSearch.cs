#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Text;

namespace DeveloperTools
{
    internal static class DeveloperSearch
    {
        private const string Initials = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";

        internal static bool Matches(string value, string query)
        {
            query = Normalize(query);
            if (query.Length == 0) return true;
            value = Normalize(value);
            if (value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            // A consonant-only query can find Korean names without entering complete syllables.
            foreach (char letter in query)
                if (Initials.IndexOf(letter) < 0) return false;
            var initials = new StringBuilder(value.Length);
            foreach (char letter in value)
                initials.Append(letter >= '가' && letter <= '힣' ? Initials[(letter - '가') / 588] : letter);
            return initials.ToString().IndexOf(query, StringComparison.Ordinal) >= 0;
        }

        private static string Normalize(string value)
        {
            var normalized = new StringBuilder();
            foreach (char letter in (value ?? "").Normalize(NormalizationForm.FormC))
                if (!char.IsWhiteSpace(letter)) normalized.Append(letter);
            return normalized.ToString();
        }
    }
}
#endif
