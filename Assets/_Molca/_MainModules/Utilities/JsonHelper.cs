using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Molca.Utils
{ 
    public static class JsonHelper
    {
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ToJson(object json)
        {
            return JsonConvert.SerializeObject(json);
        }

        public static bool TryGetValue<T>(string json, string key, out T value) where T:class
        {
            value = null;
            if (!IsValidJson(json))
                return false;
            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == key)
                    {
                        reader.Read();
                        if (reader.Value != null)
                        {
                            value = reader.Value as T;
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            return false;

            /*var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (data.ContainsKey(key))
            {
                value = data[key] as T;
                return value != null;
            }*/
        }

        public static T GetValue<T>(string json, string key) where T:class
        {
            if (!IsValidJson(json))
                return null;

            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == key)
                    {
                        reader.Read();
                        if (reader.Value != null)
                            return reader.Value as T;
                        else
                            return null;
                    }
                }
            }
            return null;

            /*var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (data.ContainsKey(key))
                return data[key] as T;
            else
                return null;*/
        }

        public static string ExtractBlock(string json, string blockName)
        {
            using (StringReader sr = new StringReader(json))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                StringBuilder blockContent = new StringBuilder();
                bool isTargetBlock = false;
                int depth = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName && (string)reader.Value == blockName)
                    {
                        isTargetBlock = true;
                        continue;
                    }

                    if (isTargetBlock)
                    {
                        if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
                        {
                            depth++;
                            if (depth == 1)
                            {
                                blockContent.Append(reader.TokenType == JsonToken.StartObject ? "{" : "[");
                                continue;
                            }
                        }
                        else if (reader.TokenType == JsonToken.EndObject || reader.TokenType == JsonToken.EndArray)
                        {
                            depth--;
                            if (depth == 0)
                            {
                                blockContent.Append(reader.TokenType == JsonToken.EndObject ? "}" : "]");
                                break;
                            }
                        }

                        if (depth > 0)
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.PropertyName:
                                    blockContent.Append($"\"{reader.Value}\":");
                                    break;
                                case JsonToken.String:
                                    blockContent.Append($"\"{reader.Value}\"");
                                    break;
                                case JsonToken.Integer:
                                case JsonToken.Float:
                                case JsonToken.Boolean:
                                case JsonToken.Null:
                                    blockContent.Append(reader.Value?.ToString() ?? "null");
                                    break;
                                case JsonToken.StartObject:
                                    blockContent.Append("{");
                                    break;
                                case JsonToken.EndObject:
                                    blockContent.Append("}");
                                    break;
                                case JsonToken.StartArray:
                                    blockContent.Append("[");
                                    break;
                                case JsonToken.EndArray:
                                    blockContent.Append("]");
                                    break;
                            }

                            if (reader.TokenType != JsonToken.PropertyName &&
                                reader.TokenType != JsonToken.StartObject &&
                                reader.TokenType != JsonToken.StartArray)
                            {
                                blockContent.Append(",");
                            }
                        }
                    }
                }

                // Remove the last comma if present
                if (blockContent.Length > 0 && blockContent[blockContent.Length - 1] == ',')
                {
                    blockContent.Length--;
                }

                return blockContent.ToString();
            }
        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
            {
                return false;
            }

            strInput = strInput.Trim();
            return (strInput.StartsWith("{") && strInput.EndsWith("}")) || // For object
                (strInput.StartsWith("[") && strInput.EndsWith("]")); // For array
        }
    }
}