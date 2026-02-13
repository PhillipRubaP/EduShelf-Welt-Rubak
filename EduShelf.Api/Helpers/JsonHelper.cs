using System;
using System.Text.Json;

namespace EduShelf.Api.Helpers
{
    public static class JsonHelper
    {
        /// <summary>
        /// Extracts the first valid JSON object or array from a string, handling Markdown code blocks.
        /// </summary>
        /// <param name="response">The raw AI response string.</param>
        /// <returns>The extracted JSON string, or the original string if no JSON structure is found.</returns>
        public static string ExtractJson(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return "{}";
            }

            var cleaned = response.Trim();

            // 1. Remove Markdown Code Blocks (```json ... ``` or ``` ... ```)
            if (cleaned.StartsWith("```"))
            {
                var firstNewline = cleaned.IndexOf('\n');
                if (firstNewline > -1)
                {
                    cleaned = cleaned.Substring(firstNewline + 1);
                }
                else 
                {
                    // Fallback if no newline found after ``` (edge case)
                     cleaned = cleaned.TrimStart('`').Trim();
                }

                if (cleaned.EndsWith("```"))
                {
                    cleaned = cleaned.Substring(0, cleaned.Length - 3);
                }
            }

            // 2. Find outermost brackets/braces
            // We want to find the first occurrence of '[' or '{'
            var firstBracketIndex = cleaned.IndexOfAny(new[] { '[', '{' });
            
            // If we found a start, try to find the matching end
            if (firstBracketIndex >= 0)
            {
                var lastBracketIndex = cleaned.LastIndexOfAny(new[] { ']', '}' });
                
                if (lastBracketIndex > firstBracketIndex)
                {
                    return cleaned.Substring(firstBracketIndex, lastBracketIndex - firstBracketIndex + 1);
                }
            }

            return cleaned.Trim();
        }

        public static T? TryDeserialize<T>(string json)
        {
             try
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return default;
            }
        }
    }
}
