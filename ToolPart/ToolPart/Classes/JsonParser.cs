using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolPart.Classes;

public class JsonParser
{
    public static object ParseJson(string jsonString)
    {
        jsonString = jsonString.Trim();

        if (jsonString.StartsWith("[") && jsonString.EndsWith("]")) return ParseArray(jsonString);

        if (jsonString.StartsWith("{") && jsonString.EndsWith("}")) return ParseObject(jsonString);

        return ParseValue(jsonString);
    }

    public static List<object> ParseArray(string arrayString)
    {
        arrayString = arrayString.Trim();
        if (!arrayString.StartsWith("[") || !arrayString.EndsWith("]"))
            throw new Exception("Phải là một array JSON hợp lệ");
        var content = arrayString.Substring(1, arrayString.Length - 2).Trim();
        if (string.IsNullOrEmpty(content)) return [];
        var elements = ParseArrayElements(content);
        return elements.Select(element => ParseElement(element.Trim())).ToList();
    }

    public static Dictionary<string, object> ParseObject(string objectString)
    {
        objectString = objectString.Trim();
        if (!objectString.StartsWith("{") || !objectString.EndsWith("}"))
            throw new Exception("Phải là một object JSON hợp lệ");
        var content = objectString.Substring(1, objectString.Length - 2).Trim();
        if (string.IsNullOrEmpty(content)) return new Dictionary<string, object>();
        var result = new Dictionary<string, object>();
        var pairs = ParseKeyValuePairs(content);
        foreach (var keyValue in pairs.Select(ParseKeyValue)) result[keyValue.Key] = keyValue.Value;
        return result;
    }

    public static object ParseElement(string elementString)
    {
        elementString = elementString.Trim();

        if (elementString.StartsWith("{")) return ParseObject(elementString);

        return elementString.StartsWith("[") ? ParseArray(elementString) : ParseValue(elementString);
    }

    public static List<string> ParseArrayElements(string content)
    {
        var elements = new List<string>();
        var i = 0;
        var bracketCount = 0;
        var squareBracketCount = 0;
        var inString = false;
        var currentElement = new StringBuilder();

        while (i < content.Length)
        {
            var ch = content[i];

            if (ch == '"' && (i == 0 || content[i - 1] != '\\')) inString = !inString;

            if (!inString)
                switch (ch)
                {
                    case '{':
                        bracketCount++;
                        break;
                    case '}':
                        bracketCount--;
                        break;
                    case '[':
                        squareBracketCount++;
                        break;
                    case ']':
                        squareBracketCount--;
                        break;
                    case ',' when bracketCount == 0 && squareBracketCount == 0:
                        elements.Add(currentElement.ToString().Trim());
                        currentElement.Clear();
                        i++;
                        continue;
                }

            currentElement.Append(ch);
            i++;
        }

        if (currentElement.Length > 0) elements.Add(currentElement.ToString().Trim());

        return elements;
    }

    public static List<string> ParseKeyValuePairs(string content)
    {
        var pairs = new List<string>();
        var i = 0;
        var inString = false;
        var bracketCount = 0;
        var squareBracketCount = 0;
        var currentPair = new StringBuilder();

        while (i < content.Length)
        {
            var ch = content[i];

            if (ch == '"' && (i == 0 || content[i - 1] != '\\')) inString = !inString;

            if (!inString)
                switch (ch)
                {
                    case '{':
                    case '[':
                    {
                        if (ch == '{') bracketCount++;
                        else squareBracketCount++;
                        break;
                    }
                    case '}':
                    case ']':
                    {
                        if (ch == '}') bracketCount--;
                        else squareBracketCount--;
                        break;
                    }
                    case ',' when bracketCount == 0 && squareBracketCount == 0:
                        pairs.Add(currentPair.ToString().Trim());
                        currentPair.Clear();
                        i++;
                        continue;
                }

            currentPair.Append(ch);
            i++;
        }

        if (currentPair.Length > 0) pairs.Add(currentPair.ToString().Trim());

        return pairs;
    }

    public static KeyValuePair<string, object> ParseKeyValue(string pairString)
    {
        var colonIndex = -1;
        var inString = false;

        for (var i = 0; i < pairString.Length; i++)
            if (pairString[i] == '"' && (i == 0 || pairString[i - 1] != '\\'))
            {
                inString = !inString;
            }
            else if (pairString[i] == ':' && !inString)
            {
                colonIndex = i;
                break;
            }

        if (colonIndex == -1) throw new Exception("Không tìm thấy dấu : trong cặp key-value");

        var keyPart = pairString.Substring(0, colonIndex).Trim();
        var valuePart = pairString.Substring(colonIndex + 1).Trim();

        var key = ParseString(keyPart);
        var value = ParseValue(valuePart);

        return new KeyValuePair<string, object>(key, value);
    }

    public static string ParseString(string str)
    {
        str = str.Trim();
        if (str.StartsWith("\"") && str.EndsWith("\"")) return str.Substring(1, str.Length - 2);
        return str;
    }

    public static object ParseValue(string valueStr)
    {
        valueStr = valueStr.Trim();

        switch (valueStr)
        {
            case "null":
                return null;
            case "true":
                return true;
            case "false":
                return false;
        }

        if (valueStr.StartsWith("\"") && valueStr.EndsWith("\"")) return valueStr.Substring(1, valueStr.Length - 2);

        if (double.TryParse(valueStr, out var doubleResult))
        {
            if (valueStr.Contains('.')) return doubleResult;

            return (int)doubleResult;
        }

        if (valueStr.StartsWith("{")) return ParseObject(valueStr);

        if (valueStr.StartsWith("[")) return ParseArray(valueStr);

        throw new Exception($"Không thể parse giá trị: {valueStr}");
    }
}