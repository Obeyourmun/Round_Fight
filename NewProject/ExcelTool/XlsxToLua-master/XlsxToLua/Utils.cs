﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class Utils
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr _lopen(string lpPathName, int iReadWrite);
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);
    private const int OF_READWRITE = 2;
    private const int OF_SHARE_DENY_NONE = 0x40;
    private static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

    /// <summary>
    /// 获取某个文件的状态
    /// </summary>
    public static FileState GetFileState(string filePath)
    {
        if (File.Exists(filePath))
        {
            IntPtr vHandle = _lopen(filePath, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR)
                return FileState.IsOpen;

            CloseHandle(vHandle);
            return FileState.Available;
        }
        else
            return FileState.Inexist;
    }

    /// <summary>
    /// 将Excel中的列编号转为列名称（第1列为A，第28列为AB）
    /// </summary>
    public static string GetExcelColumnName(int columnNumber)
    {
        string result = string.Empty;
        int temp = columnNumber;
        int quotient;
        int remainder;
        do
        {
            quotient = temp / 26;
            remainder = temp % 26;
            if (remainder == 0)
            {
                remainder = 26;
                --quotient;
            }

            result = (char)(remainder - 1 + 'A') + result;
            temp = quotient;
        }
        while (quotient > 0);

        return result;
    }

    /// <summary>
    /// 将List中的所有数据用指定分隔符连接为一个新字符串
    /// </summary>
    public static string CombineString<T>(IList<T> list, string separateString)
    {
        if (list == null || list.Count < 1)
            return null;
        else
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
                builder.Append(list[i]).Append(separateString);

            string result = builder.ToString();
            // 去掉最后多加的一次分隔符
            if (separateString != null)
                return result.Substring(0, result.Length - separateString.Length);
            else
                return result;
        }
    }

    /// <summary>
    /// 获取有效值声明{1,5,10}或数值范围声明[1,5]中符合要求的值集合（有效值声明支持int、float、string型，数值范围声明仅支持int型）
    /// </summary>
    public static List<object> GetEffectiveValue(string inputDefineString, DataType dataType, out string errorString)
    {
        if (string.IsNullOrEmpty(inputDefineString))
        {
            errorString = "未声明获取值集合的规则";
            return null;
        }

        List<object> result = new List<object>();
        string defineString = inputDefineString.Trim();

        List<FieldCheckRule> fieldCheckRules = TableCheckHelper.GetCheckRules(defineString, out errorString);
        if (errorString != null)
        {
            errorString = "获取值集合规则声明错误，" + errorString;
            return null;
        }
        if (fieldCheckRules.Count > 1)
        {
            errorString = "获取值集合规则声明错误，仅支持从一个有效值声明或数值范围声明中取符合要求的值集合，不支持用&&组合的规则";
            return null;
        }

        FieldCheckRule checkRule = fieldCheckRules[0];
        switch (checkRule.CheckType)
        {
            case TableCheckType.Effective:
                {
                    if (dataType == DataType.Int || dataType == DataType.Float)
                    {
                        // 去除首尾花括号后，通过英文逗号分隔每个有效值即可
                        if (!(checkRule.CheckRuleString.StartsWith("{") && checkRule.CheckRuleString.EndsWith("}")))
                        {
                            errorString = "获取值集合规则所采用的值有效性声明错误：必须在首尾用一对花括号包裹整个定义内容";
                            return null;
                        }
                        string temp = checkRule.CheckRuleString.Substring(1, checkRule.CheckRuleString.Length - 2).Trim();
                        if (string.IsNullOrEmpty(temp))
                        {
                            errorString = "获取值集合规则所采用的值有效性声明错误：至少需要输入一个有效值";
                            return null;
                        }

                        string[] values = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (dataType == DataType.Int)
                        {
                            for (int i = 0; i < values.Length; ++i)
                            {
                                string oneValueString = values[i].Trim();
                                int oneValue;
                                if (int.TryParse(oneValueString, out oneValue) == true)
                                {
                                    if (result.Contains(oneValue))
                                        Utils.LogWarning(string.Format("警告：获取值集合规则所采用的值有效性声明（\"{0}\"）中，出现了相同的有效值\"{1}\"，本工具忽略此问题，需要你之后修正错误\n", checkRule.CheckRuleString, oneValue));
                                    else
                                        result.Add(oneValue);
                                }
                                else
                                {
                                    errorString = string.Format("获取值集合规则所采用的值有效性声明错误：出现了非int型有效值定义，其为\"{0}\"", oneValueString);
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < values.Length; ++i)
                            {
                                string oneValueString = values[i].Trim();
                                float oneValue;
                                if (float.TryParse(oneValueString, out oneValue) == true)
                                {
                                    if (result.Contains(oneValue))
                                        Utils.LogWarning(string.Format("警告：获取值集合规则所采用的值有效性声明（\"{0}\"）中，出现了相同的有效值\"{1}\"，本工具忽略此问题，需要你之后修正错误\n", checkRule.CheckRuleString, oneValue));
                                    else
                                        result.Add(oneValue);
                                }
                                else
                                {
                                    errorString = string.Format("获取值集合规则所采用的值有效性声明错误：出现了非float型有效值定义，其为\"{0}\"", oneValueString);
                                    return null;
                                }
                            }
                        }
                    }
                    else if (dataType == DataType.String)
                    {
                        // 用于分隔有效值声明的字符，默认为英文逗号
                        char separator = ',';
                        // 去除首尾花括号后整个有效值声明内容
                        string effectiveDefineString = checkRule.CheckRuleString;

                        // 右边花括号的位置
                        int rightBraceIndex = checkRule.CheckRuleString.LastIndexOf('}');
                        if (rightBraceIndex == -1)
                        {
                            errorString = "获取值集合规则所采用的string型值有效性声明错误：必须用一对花括号包裹整个定义内容";
                            return null;
                        }
                        // 如果声明了分隔有效值的字符
                        if (rightBraceIndex != checkRule.CheckRuleString.Length - 1)
                        {
                            int leftBracketIndex = checkRule.CheckRuleString.LastIndexOf('(');
                            int rightBracketIndex = checkRule.CheckRuleString.LastIndexOf(')');
                            if (leftBracketIndex < rightBraceIndex || rightBracketIndex < leftBracketIndex)
                            {
                                errorString = "获取值集合规则所采用的string型值有效性声明错误：需要在最后面的括号中声明分隔各个有效值的一个字符，如果使用默认的英文逗号作为分隔符，则不必在最后面用括号声明自定义分隔字符";
                                return null;
                            }
                            string separatorString = checkRule.CheckRuleString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
                            if (separatorString.Length > 1)
                            {
                                errorString = string.Format("获取值集合规则所采用的string型值有效性声明错误：自定义有效值的分隔字符只能为一个字符，而你输入的为\"{0}\"", separatorString);
                                return null;
                            }
                            separator = separatorString[0];

                            // 取得前面用花括号包裹的有效值声明
                            effectiveDefineString = checkRule.CheckRuleString.Substring(0, rightBraceIndex + 1).Trim();
                        }

                        // 去除花括号
                        effectiveDefineString = effectiveDefineString.Substring(1, effectiveDefineString.Length - 2);
                        if (string.IsNullOrEmpty(effectiveDefineString))
                        {
                            errorString = "获取值集合规则所采用的string型值有效性声明错误：至少需要输入一个有效值";
                            return null;
                        }

                        string[] effectiveValueDefine = effectiveDefineString.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                        if (effectiveValueDefine.Length == 0)
                        {
                            errorString = "获取值集合规则所采用的string型值有效性声明错误：至少需要输入一个有效值";
                            return null;
                        }

                        foreach (string effectiveValue in effectiveValueDefine)
                        {
                            if (result.Contains(effectiveValue))
                                Utils.LogWarning(string.Format("警告：获取值集合规则所采用的string型值有效性声明（\"{0}\"）中，出现了相同的有效值\"{1}\"，本工具忽略此问题，需要你之后修正错误\n", checkRule.CheckRuleString, effectiveValue));
                            else
                                result.Add(effectiveValue);
                        }
                    }
                    else
                    {
                        errorString = string.Format("获取值集合规则所采用的值有效性声明只能用于int、float或string型的字段，而该字段为{0}型", dataType);
                        return null;
                    }

                    break;
                }
            case TableCheckType.Range:
                {
                    if (dataType == DataType.Int)
                    {
                        bool isIncludeFloor;
                        bool isIncludeCeil;
                        int floorValue = 0;
                        int ceilValue = 0;

                        // 规则首位必须为方括号或者圆括号
                        if (checkRule.CheckRuleString.StartsWith("("))
                            isIncludeFloor = false;
                        else if (checkRule.CheckRuleString.StartsWith("["))
                            isIncludeFloor = true;
                        else
                        {
                            errorString = "获取值集合规则所采用的数值范围声明错误：必须用英文(或[开头，表示有效范围是否包含等于下限的情况";
                            return null;
                        }
                        // 规则末位必须为方括号或者圆括号
                        if (checkRule.CheckRuleString.EndsWith(")"))
                            isIncludeCeil = false;
                        else if (checkRule.CheckRuleString.EndsWith("]"))
                            isIncludeCeil = true;
                        else
                        {
                            errorString = "获取值集合规则所采用的数值范围声明错误，采用的数值范围声明错误：必须用英文)或]结尾，表示有效范围是否包含等于上限的情况";
                            return null;
                        }
                        // 去掉首尾的括号
                        string temp = checkRule.CheckRuleString.Substring(1, checkRule.CheckRuleString.Length - 2);
                        // 通过英文逗号分隔上下限
                        string[] floorAndCeilString = temp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (floorAndCeilString.Length != 2)
                        {
                            errorString = "获取值集合规则所采用的数值范围声明错误：必须用英文逗号分隔值范围的上下限";
                            return null;
                        }
                        string floorString = floorAndCeilString[0].Trim();
                        string ceilString = floorAndCeilString[1].Trim();
                        // 提取上下限数值
                        if (int.TryParse(ceilString, out ceilValue) == false)
                        {
                            errorString = string.Format("获取值集合规则所采用的数值范围声明错误：上限不是合法的数字，你输入的为{0}", ceilString);
                            return null;
                        }
                        if (int.TryParse(floorString, out floorValue) == false)
                        {
                            errorString = string.Format("获取值集合规则所采用的数值范围声明错误：下限不是合法的数字，你输入的为{0}", floorString);
                            return null;
                        }
                        // 判断上限是否大于下限
                        if (floorValue >= ceilValue)
                        {
                            errorString = string.Format("获取值集合规则所采用的数值范围声明错误：上限值必须大于下限值，你输入的下限为{0}，上限为{1}", floorValue, ceilString);
                            return null;
                        }
                        if (!isIncludeFloor)
                            ++floorValue;
                        if (!isIncludeCeil)
                            --ceilValue;

                        for (int i = floorValue; i <= ceilValue; ++i)
                            result.Add(i);
                    }
                    else
                    {
                        errorString = string.Format("获取值集合规则所采用的数值范围声明只能用于int型的字段，而该字段为{0}型", dataType);
                        return null;
                    }

                    break;
                }
            default:
                {
                    errorString = "获取值集合规则声明错误，仅支持从一个有效值声明或数值范围声明中取符合要求的值集合";
                    break;
                }
        }

        if (errorString == null)
            return result;
        else
            return null;
    }

    /// <summary>
    /// 解析在英文小括号内用|分隔的Excel文件名
    /// </summary>
    public static string[] GetExcelFileNames(string paramString, out string errorString)
    {
        int leftBracketIndex = paramString.IndexOf('(');
        int rightBracketIndex = paramString.LastIndexOf(')');
        if (leftBracketIndex == -1 || rightBracketIndex == -1 || leftBracketIndex > rightBracketIndex)
        {
            errorString = "必须在英文小括号内声明Excel文件名";
            return null;
        }
        else
        {
            string fileNameString = paramString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
            string[] fileNames = fileNameString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (fileNames.Length < 1)
            {
                errorString = "必须在英文小括号内声明至少一个Excel文件名";
                return null;
            }
            else
            {
                errorString = null;
                return fileNames;
            }
        }
    }

    public static void Log(string logString, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(logString);
        AppValues.LogContent.AppendLine(logString);
    }

    public static void LogWarning(string warningString)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(warningString);
        AppValues.LogContent.AppendLine(warningString);
    }

    public static void LogError(string errorString)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorString);
        AppValues.LogContent.AppendLine(errorString);
    }

    /// <summary>
    /// 输出错误信息并在用户按任意键后退出
    /// </summary>
    public static void LogErrorAndExit(string errorString)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorString);
        Console.WriteLine("程序被迫退出，请修正错误后重试");
        Console.ReadKey();
        Environment.Exit(-1);
    }

    /// <summary>
    /// 将程序运行中检查出的所有错误保存到文本文件中，存储目录为本工具所在目录
    /// </summary>
    public static bool SaveErrorInfoToFile()
    {
        try
        {
            string fileName = string.Format("表格检查结果 {0}", string.Format("{0:yyyy年MM月dd日 HH时mm分ss秒}.txt", DateTime.Now));
            string savePath = Utils.CombinePath(AppValues.PROGRAM_FOLDER_PATH, fileName);
            StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
            writer.WriteLine(AppValues.LogContent.ToString().Replace("\n", System.Environment.NewLine));
            writer.Flush();
            writer.Close();

            Log(string.Format("全部错误信息已导出文本文件，文件名为\"{0}\"，存储在本工具所在目录下", fileName));
            return true;
        }
        catch
        {
            LogError("全部错误信息导出到文本文件失败");
            return false;
        }
    }

    /// <summary>
    /// 二分查找
    /// </summary>
    public static int binarySearch(List<int> dataList, int key)
    {
        int low = 0;
        int high = dataList.Count - 1;
        while (low <= high)
        {
            int middle = (high + low) / 2;
            if (dataList[middle] == key)
                return middle;
            else if (dataList[middle] < key)
                low = middle + 1;
            else
                high = middle - 1;
        }

        return -1;
    }

    public static string GetCamelCaseString(string inputString)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (inputString.IndexOf('_') != -1)
        {
            bool isUnderlineOfLastChar = false;
            for (int i = 0; i < inputString.Length; ++i)
            {
                char c = inputString[i];
                if (c == '_')
                {
                    isUnderlineOfLastChar = true;
                    continue;
                }
                if (isUnderlineOfLastChar == true)
                {
                    stringBuilder.Append(char.ToUpper(c));
                    isUnderlineOfLastChar = false;
                }
                else
                    stringBuilder.Append(char.ToLower(c));
            }

            string tempClassName = stringBuilder.ToString();
            return char.ToLower(tempClassName[0]) + tempClassName.Substring(1);
        }
        else
            return inputString;
    }

    /// <summary>
    /// 获取导出某种生成文件的存储目录
    /// </summary>
    public static string GetExportDirectoryPath(string tableName, string exportRootPath)
    {
        if (AppValues.IsExportKeepDirectoryStructure == true)
        {
            // 获取表格相对于Excel文件夹的相对路径
            string excelFolderPath = AppValues.ExcelFolderPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            if (!excelFolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                excelFolderPath = excelFolderPath + Path.DirectorySeparatorChar;

            Uri excelFolderUri = new Uri(excelFolderPath);
            Uri fileUri = new Uri(AppValues.ExportTableNameAndPath[tableName]);
            Uri relativeUri = excelFolderUri.MakeRelativeUri(fileUri);
            // 注意：Uri转为的字符串中会将中文转义为%xx，需要恢复为非转义形式
            return Path.GetDirectoryName(CombinePath(exportRootPath, Uri.UnescapeDataString(relativeUri.ToString())));
        }
        else
            return exportRootPath;
    }

    /// <summary>
    /// 将某张Excel表格转换为lua table内容保存到文件
    /// </summary>
    public static bool SaveLuaFile(string tableName, string fileName, string content)
    {
        try
        {
            string exportDirectoryPath = GetExportDirectoryPath(tableName, AppValues.ExportLuaFilePath);
            if (Directory.Exists(exportDirectoryPath) == false)
                Directory.CreateDirectory(exportDirectoryPath);

            string savePath = Utils.CombinePath(exportDirectoryPath, fileName + ".lua");
            StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
            writer.Write(content);
            writer.Flush();
            writer.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool SaveCsvFile(string tableName, List<StringBuilder> rowContentList)
    {
        try
        {
            string exportDirectoryPath = GetExportDirectoryPath(tableName, AppValues.ExportCsvPath);
            if (Directory.Exists(exportDirectoryPath) == false)
                Directory.CreateDirectory(exportDirectoryPath);

            string fileName = string.Concat(tableName, ".", AppValues.ExportCsvExtension);
            string savePath = Utils.CombinePath(exportDirectoryPath, fileName);
            StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
            foreach (StringBuilder stringBuilder in rowContentList)
                writer.WriteLine(stringBuilder);

            writer.Flush();
            writer.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool SaveCsClassFile(string tableName, string className, string content)
    {
        try
        {
            string exportDirectoryPath = GetExportDirectoryPath(tableName, AppValues.ExportCsClassPath);
            if (Directory.Exists(exportDirectoryPath) == false)
                Directory.CreateDirectory(exportDirectoryPath);

            string fileName = string.Concat(className, ".", AppValues.EXPORT_CS_CLASS_FILE_EXTENSION);
            string savePath = Utils.CombinePath(exportDirectoryPath, fileName);
            StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
            writer.Write(content);
            writer.Flush();
            writer.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool SaveJavaClassFile(string tableName, string className, string content)
    {
        try
        {
            string exportDirectoryPath = GetExportDirectoryPath(tableName, AppValues.ExportJavaClassPath);
            if (Directory.Exists(exportDirectoryPath) == false)
                Directory.CreateDirectory(exportDirectoryPath);

            string fileName = string.Concat(className, ".", AppValues.EXPORT_JAVA_CLASS_FILE_EXTENSION);
            string savePath = Utils.CombinePath(exportDirectoryPath, fileName);
            StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
            writer.Write(content);
            writer.Flush();
            writer.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool SaveJsonFile(string tableName, string content)
    {
        try
        {
            string exportDirectoryPath = GetExportDirectoryPath(tableName, AppValues.ExportJsonPath);
            if (Directory.Exists(exportDirectoryPath) == false)
                Directory.CreateDirectory(exportDirectoryPath);

            string fileName = string.Concat(tableName, ".", AppValues.ExportJsonExtension);
            string savePath = Utils.CombinePath(exportDirectoryPath, fileName);
            StreamWriter writer = new StreamWriter(savePath, false, new UTF8Encoding(false));
            writer.Write(content);
            writer.Flush();
            writer.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 合并两个路径字符串，与.Net类库中的Path.Combine不同，本函数不会因为path2以目录分隔符开头就认为是绝对路径，然后直接返回path2
    /// </summary>
    public static string CombinePath(string path1, string path2)
    {
        path1 = path1.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        if (!path1.EndsWith(Path.DirectorySeparatorChar.ToString()))
            path1 = path1 + Path.DirectorySeparatorChar;

        path2 = path2.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        if (path2.StartsWith(Path.DirectorySeparatorChar.ToString()))
            path2 = path2.Substring(1, path2.Length - 1);

        return Path.Combine(path1, path2);
    }
}

public enum FileState
{
    Inexist,     // 不存在
    IsOpen,      // 已被打开
    Available,   // 当前可用
}
