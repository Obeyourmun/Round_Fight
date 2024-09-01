﻿using System.Collections.Generic;
using System.IO;
using System.Text;

public class TxtConfigReader
{
    /// <summary>
    /// 将文本文件中的键值对读取并加入Dictionary
    /// </summary>
    public static Dictionary<string, string> ParseTxtConfigFile(string filePath, string separator, out string errorString)
    {
        if (!File.Exists(filePath))
        {
            errorString = string.Format("错误：TxtConfig文件不存在，输入路径为{0}", filePath);
            return null;
        }

        Dictionary<string, string> result = new Dictionary<string, string>();

        using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
        {
            string line = null;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null)
            {
                ++lineNumber;

                // 以#开头的注释行和空行忽略
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                // 找到key、value
                int separatorIndex = line.IndexOf(separator);
                if (separatorIndex == -1)
                {
                    errorString = string.Format("错误：第{0}行不合法不包含分隔符{1}", lineNumber, separator);
                    return null;
                }
                string key = line.Substring(0, separatorIndex);
                string value = line.Substring(separatorIndex + 1);
                if (result.ContainsKey(key))
                {
                    errorString = string.Format("错误：第{0}行中的key({1})重复定义", lineNumber, key);
                    return null;
                }

                result.Add(key, value);
            }
        }

        errorString = null;
        return result;
    }
}
