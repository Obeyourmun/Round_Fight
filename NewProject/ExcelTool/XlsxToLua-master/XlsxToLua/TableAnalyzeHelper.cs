﻿using LitJson;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

public class TableAnalyzeHelper
{
    public static TableInfo AnalyzeTable(DataTable dt, string tableName, out string errorString)
    {
        if (dt.Rows.Count < AppValues.DATA_FIELD_DATA_START_INDEX)
        {
            errorString = "表格格式不符合要求，必须在表格前五行中依次声明字段描述、字段名、数据类型、检查规则、导出到MySQL数据库中的配置";
            return null;
        }
        if (dt.Columns.Count < 2)
        {
            errorString = "表格中至少需要配置2个字段";
            return null;
        }

        TableInfo tableInfo = new TableInfo();
        tableInfo.TableName = tableName;

        // 当前解析到的列号
        int curColumnIndex = 0;

        // 解析第一列（主键列，要求数据类型为int、long或string且数据非空、唯一）
        DataType primaryKeyColumnType = _AnalyzeDataType(dt.Rows[AppValues.DATA_FIELD_DATA_TYPE_INDEX][0].ToString().Trim());
        if (!(primaryKeyColumnType == DataType.Int || primaryKeyColumnType == DataType.Long || primaryKeyColumnType == DataType.String))
        {
            errorString = _GetTableAnalyzeErrorString(tableName, 0) + "主键列的类型只能为int、long或string";
            return null;
        }
        else
        {
            // 解析出主键列，然后检查是否非空、唯一，如果是string型主键还要检查是否符合变量名的规范（只能由英文字母、数字、下划线组成）
            FieldInfo primaryKeyField = _AnalyzeOneField(dt, tableInfo, 0, null, out curColumnIndex, out errorString);
            if (errorString != null)
            {
                errorString = _GetTableAnalyzeErrorString(tableName, 0) + "主键列解析错误\n" + errorString;
                return null;
            }
            // 主键列字段名不允许为空
            else if (primaryKeyField.IsIgnoreClientExport == true)
            {
                errorString = _GetTableAnalyzeErrorString(tableName, 0) + "主键列必须指定字段名\n" + errorString;
                return null;
            }
            else
            {
                // 唯一性检查
                FieldCheckRule uniqueCheckRule = new FieldCheckRule();
                uniqueCheckRule.CheckType = TableCheckType.Unique;
                uniqueCheckRule.CheckRuleString = "unique";
                TableCheckHelper.CheckUnique(primaryKeyField, uniqueCheckRule, out errorString);
                if (errorString != null)
                {
                    errorString = _GetTableAnalyzeErrorString(tableName, 0) + "主键列存在重复错误\n" + errorString;
                    return null;
                }

                // string型主键检查是否符合变量名的规范
                if (primaryKeyColumnType == DataType.String)
                {
                    StringBuilder errorStringBuilder = new StringBuilder();
                    for (int row = 0; row < primaryKeyField.Data.Count; ++row)
                    {
                        string tempError = null;
                        TableCheckHelper.CheckFieldName(primaryKeyField.Data[row].ToString(), out tempError);
                        if (tempError != null)
                            errorStringBuilder.AppendFormat("第{0}行所填主键{1}\n", row + AppValues.DATA_FIELD_DATA_START_INDEX + 1, tempError);
                    }
                    if (!string.IsNullOrEmpty(errorStringBuilder.ToString()))
                    {
                        errorString = _GetTableAnalyzeErrorString(tableName, 0) + "string型主键列存在非法数据\n" + errorStringBuilder.ToString();
                        return null;
                    }
                }

                // 非空检查（因为string型检查是否符合变量名规范时以及未声明数值型字段中允许空时，均已对主键列进行过非空检查，这里只需对数据值字段且声明数值型字段中允许空的情况下进行非空检查）
                if (AppValues.IsAllowedNullNumber == true && (primaryKeyColumnType == DataType.Int || primaryKeyColumnType == DataType.Long))
                {
                    FieldCheckRule notEmptyCheckRule = new FieldCheckRule();
                    uniqueCheckRule.CheckType = TableCheckType.NotEmpty;
                    uniqueCheckRule.CheckRuleString = "notEmpty";
                    TableCheckHelper.CheckNotEmpty(primaryKeyField, notEmptyCheckRule, out errorString);
                    if (errorString != null)
                    {
                        errorString = _GetTableAnalyzeErrorString(tableName, 0) + "主键列存在非空错误\n" + errorString;
                        return null;
                    }
                }

                tableInfo.AddField(primaryKeyField);
            }
        }

        // 存储定义过的字段名，不允许有同名字段（key：字段名， value：列号）
        Dictionary<string, int> fieldNames = new Dictionary<string, int>();
        // 先加入主键列
        fieldNames.Add(tableInfo.GetKeyColumnFieldInfo().FieldName, 0);
        // 解析剩余的列
        while (curColumnIndex < dt.Columns.Count)
        {
            int nextColumnIndex = curColumnIndex;
            FieldInfo oneField = _AnalyzeOneField(dt, tableInfo, nextColumnIndex, null, out curColumnIndex, out errorString);
            if (errorString != null)
            {
                errorString = _GetTableAnalyzeErrorString(tableName, nextColumnIndex) + errorString;
                return null;
            }
            else
            {
                // 跳过未声明变量名以及数据库导出信息的无效列
                if (oneField != null)
                {
                    // 检查字段名是否重复
                    if (fieldNames.ContainsKey(oneField.FieldName))
                    {
                        errorString = _GetTableAnalyzeErrorString(tableName, nextColumnIndex) + string.Format("表格中存在字段名同为{0}的字段，分别位于第{1}列和第{2}列", oneField.FieldName, Utils.GetExcelColumnName(fieldNames[oneField.FieldName] + 1), Utils.GetExcelColumnName(oneField.ColumnSeq + 1));
                        return null;
                    }
                    else
                    {
                        tableInfo.AddField(oneField);
                        fieldNames.Add(oneField.FieldName, oneField.ColumnSeq);
                    }
                }
            }
        }

        errorString = null;
        return tableInfo;
    }

    /// <summary>
    /// 解析一列的数据结构及数据，返回FieldInfo
    /// </summary>
    private static FieldInfo _AnalyzeOneField(DataTable dt, TableInfo tableInfo, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        // 判断列号是否越界
        if (columnIndex >= dt.Columns.Count)
        {
            errorString = "需要解析的列号越界，可能因为dict或array中实际的子元素个数低于声明的个数";
            nextFieldColumnIndex = columnIndex + 1;
            return null;
        }

        FieldInfo fieldInfo = new FieldInfo();
        // 所在表格
        fieldInfo.TableName = tableInfo.TableName;
        // 字段描述中的换行全替换为空格
        fieldInfo.Desc = dt.Rows[AppValues.DATA_FIELD_DESC_INDEX][columnIndex].ToString().Trim().Replace(System.Environment.NewLine, " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
        // 字段所在列号
        fieldInfo.ColumnSeq = columnIndex;
        // 检查规则字符串
        string checkRuleString = dt.Rows[AppValues.DATA_FIELD_CHECK_RULE_INDEX][columnIndex].ToString().Trim().Replace(System.Environment.NewLine, " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
        fieldInfo.CheckRule = string.IsNullOrEmpty(checkRuleString) ? null : checkRuleString;
        // 导出到数据库中的字段名及类型
        string databaseInfoString = dt.Rows[AppValues.DATA_FIELD_EXPORT_DATABASE_FIELD_INFO][columnIndex].ToString().Trim();
        if (string.IsNullOrEmpty(databaseInfoString))
        {
            fieldInfo.DatabaseFieldName = null;
            fieldInfo.DatabaseFieldType = null;
        }
        else
        {
            int leftBracketIndex = databaseInfoString.IndexOf('(');
            int rightBracketIndex = databaseInfoString.LastIndexOf(')');
            if (leftBracketIndex == -1 || rightBracketIndex == -1 || leftBracketIndex > rightBracketIndex)
            {
                errorString = "导出到数据库中表字段信息声明错误，必须在字段名后的括号中声明对应数据库中的数据类型";
                nextFieldColumnIndex = columnIndex + 1;
                return null;
            }

            fieldInfo.DatabaseFieldName = databaseInfoString.Substring(0, leftBracketIndex);
            fieldInfo.DatabaseFieldType = databaseInfoString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
        }
        // 引用父FileInfo
        fieldInfo.ParentField = parentField;

        // 如果该字段是array类型的子元素，则不填写变量名（array下属元素的变量名自动顺序编号）
        // 并且如果子元素不是集合类型，也不需配置数据类型（直接使用array声明的子元素数据类型，子元素列不再单独配置），直接依次声明各子元素列
        string inputFieldName = dt.Rows[AppValues.DATA_FIELD_NAME_INDEX][columnIndex].ToString().Trim();
        if (parentField != null && parentField.DataType == DataType.Array)
        {
            // array的子元素如果为array或dict，则必须像array、dict定义格式那样通过一列声明子元素类型（目的为了校验，防止类似array声明子元素为dict[2]，而实际子元素为dict[3]，导致程序仍能正确解析之后的字段，但逻辑上却把后面的独立字段误认为是声明的array的子元素），但变量名仍旧不需填写
            if (parentField.ArrayChildDataType == DataType.Array)
            {
                string inputDataTypeString = dt.Rows[AppValues.DATA_FIELD_DATA_TYPE_INDEX][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputDataTypeString))
                {
                    errorString = "array的子元素若为array型，则必须在每个子元素声明列中声明具体的array类型";
                    nextFieldColumnIndex = columnIndex + 1;
                    return null;
                }
                // 判断本列所声明的类型是否为array型
                DataType inputDataType = _AnalyzeDataType(inputDataTypeString);
                if (inputDataType != DataType.Array)
                {
                    errorString = string.Format("父array声明的子元素类型为array，但第{0}列所填的类型为{1}", Utils.GetExcelColumnName(columnIndex + 1), inputDataType.ToString());
                    nextFieldColumnIndex = columnIndex + 1;
                    return null;
                }
                // 解析本列实际填写的array的子元素数据类型定义
                string childDataTypeString;
                DataType childDataType;
                int childCount;
                _GetArrayChildDefine(inputDataTypeString, out childDataTypeString, out childDataType, out childCount, out errorString);
                if (errorString != null)
                {
                    errorString = string.Format("解析第{0}列定义的array类型声明错误，{1}", Utils.GetExcelColumnName(columnIndex + 1), errorString);
                    nextFieldColumnIndex = columnIndex + 1;
                    return null;
                }
                // 解析父array声明的子类型数据类型定义（之前经过了检测，这里无需进行容错）
                string defineDataTypeString;
                DataType defineDataType;
                int defineCount;
                _GetArrayChildDefine(parentField.ArrayChildDataTypeString, out defineDataTypeString, out defineDataType, out defineCount, out errorString);
                if (childDataType != defineDataType)
                {
                    errorString = string.Format("父array的array类型子元素所声明的子元素为{0}型，但你填写的经过解析为{1}型", defineDataType.ToString(), childDataType.ToString());
                    nextFieldColumnIndex = columnIndex + 1;
                    return null;
                }
                if (childCount != defineCount)
                {
                    errorString = string.Format("父array的array类型子元素所声明的子元素共有{0}个，但你填写的经过解析为{1}个", defineCount, childCount);
                    nextFieldColumnIndex = columnIndex + 1;
                    return null;
                }
            }
            else if (parentField.ArrayChildDataType == DataType.Dict)
            {
                string inputDataTypeString = dt.Rows[AppValues.DATA_FIELD_DATA_TYPE_INDEX][columnIndex].ToString().Trim();
                // dict类型子元素定义列中允许含有无效列
                if (string.IsNullOrEmpty(inputDataTypeString))
                {
                    errorString = null;
                    nextFieldColumnIndex = columnIndex + 1;
                    return null;
                }
                else
                {
                    // 判断本列所声明的类型是否为dict型
                    DataType inputDataType = _AnalyzeDataType(inputDataTypeString);
                    if (inputDataType != DataType.Dict)
                    {
                        errorString = string.Format("父array声明的子元素类型为dict，但第{0}列所填的类型为{1}", Utils.GetExcelColumnName(columnIndex + 1), inputDataType.ToString());
                        nextFieldColumnIndex = columnIndex + 1;
                        return null;
                    }
                    // 解析本列实际填写的dict的子元素格式
                    int childCount;
                    _GetDictChildCount(inputDataTypeString, out childCount, out errorString);
                    if (errorString != null)
                    {
                        errorString = string.Format("解析第{0}列定义的dict类型声明错误，{1}", Utils.GetExcelColumnName(columnIndex + 1), errorString);
                        nextFieldColumnIndex = columnIndex + 1;
                        return null;
                    }
                    // 解析父array声明的dict子元素个数（之前经过了检测，这里无需进行容错）
                    int defineCount;
                    _GetDictChildCount(parentField.ArrayChildDataTypeString, out defineCount, out errorString);
                    if (childCount != defineCount)
                    {
                        errorString = string.Format("父array的dict类型子元素所声明的子元素共有{0}个，但你填写的经过解析为{1}个", defineCount, childCount);
                        nextFieldColumnIndex = columnIndex + 1;
                        return null;
                    }
                }
            }

            // 检查array下属子元素列，不允许填写变量名（如果不进行此强制限制，当定表时没有空出足够的array子元素列数，并且后面为独立字段时，解析array型时会把后面的独立字段当成array子元素声明列，很可能造成格式上可以正确解析，但逻辑上和定表者想法不符但不易发觉的问题）
            if (!string.IsNullOrEmpty(inputFieldName))
            {
                errorString = string.Format("array下属的子元素列不允许填写变量名，第{0}列错误地填写了变量名{1}", Utils.GetExcelColumnName(columnIndex + 1), inputFieldName);
                nextFieldColumnIndex = columnIndex + 1;
                return null;
            }
            fieldInfo.FieldName = null;
            fieldInfo.DataType = parentField.ArrayChildDataType;
            // array类型的ArrayChildDataTypeString中还包含了子元素个数，需要去除
            int lastColonIndex = parentField.ArrayChildDataTypeString.LastIndexOf(':');
            if (lastColonIndex == -1)
            {
                Utils.LogErrorAndExit("错误：用_AnalyzeOneField函数解析array子元素类型时发现array类型的ArrayChildDataTypeString中不含有子元素个数");
                errorString = null;
                nextFieldColumnIndex = columnIndex + 1;
                return null;
            }
            else
                fieldInfo.DataTypeString = parentField.ArrayChildDataTypeString.Substring(0, lastColonIndex);
        }
        else if (string.IsNullOrEmpty(inputFieldName))
        {
            // dict下属字段的变量名为空的列，视为无效列，直接忽略
            if (fieldInfo.ParentField != null && fieldInfo.ParentField.DataType == DataType.Dict)
            {
                Utils.LogWarning(string.Format("警告：第{0}列为dict下属字段，但未填写变量名，将被视为无效列而忽略", Utils.GetExcelColumnName(fieldInfo.ColumnSeq + 1)));
                errorString = null;
                nextFieldColumnIndex = columnIndex + 1;
                return null;
            }
            // 独立字段未填写字段名以及导出数据库信息，视为无效列，直接忽略
            else if (string.IsNullOrEmpty(databaseInfoString))
            {
                Utils.LogWarning(string.Format("警告：第{0}列未填写变量名，也未填写导出数据库信息，将被视为无效列而忽略", Utils.GetExcelColumnName(fieldInfo.ColumnSeq + 1)));
                errorString = null;
                nextFieldColumnIndex = columnIndex + 1;
                return null;
            }
            // 未填写字段名但填写了导出数据库的信息，视为只进行MySQL导出的字段，自动进行字段命名
            else
            {
                Utils.LogWarning(string.Format("警告：第{0}列未填写变量名，仅填写了导出数据库信息，将不会进行lua、csv、json等客户端形式导出", Utils.GetExcelColumnName(fieldInfo.ColumnSeq + 1)));
                fieldInfo.FieldName = string.Concat(AppValues.AUTO_FIELD_NAME_PREFIX, fieldInfo.ColumnSeq);
                fieldInfo.IsIgnoreClientExport = true;

                // 读取填写的数据类型
                fieldInfo.DataTypeString = dt.Rows[AppValues.DATA_FIELD_DATA_TYPE_INDEX][columnIndex].ToString().Trim();
                fieldInfo.DataType = _AnalyzeDataType(fieldInfo.DataTypeString);
            }
        }
        else
        {
            // 检查字段名是否合法
            if (!TableCheckHelper.CheckFieldName(inputFieldName, out errorString))
            {
                nextFieldColumnIndex = columnIndex + 1;
                return null;
            }
            else
                fieldInfo.FieldName = inputFieldName;

            // 读取填写的数据类型
            fieldInfo.DataTypeString = dt.Rows[AppValues.DATA_FIELD_DATA_TYPE_INDEX][columnIndex].ToString().Trim();
            fieldInfo.DataType = _AnalyzeDataType(fieldInfo.DataTypeString);
        }

        switch (fieldInfo.DataType)
        {
            case DataType.Int:
                {
                    _AnalyzeIntType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Long:
                {
                    _AnalyzeLongType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Float:
                {
                    _AnalyzeFloatType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Bool:
                {
                    _AnalyzeBoolType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.String:
                {
                    _AnalyzeStringType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Lang:
                {
                    _AnalyzeLangType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Date:
                {
                    _AnalyzeDateType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Time:
                {
                    _AnalyzeTimeType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Json:
                {
                    _AnalyzeJsonType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.TableString:
                {
                    _AnalyzeTableStringType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.MapString:
                {
                    _AnalyzeMapStringType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Array:
                {
                    _AnalyzeArrayType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            case DataType.Dict:
                {
                    _AnalyzeDictType(fieldInfo, tableInfo, dt, columnIndex, parentField, out nextFieldColumnIndex, out errorString);
                    break;
                }
            default:
                {
                    errorString = string.Format("数据类型无效，填写的为{0}", fieldInfo.DataTypeString);
                    nextFieldColumnIndex = columnIndex + 1;
                    break;
                }
        }

        if (errorString == null)
            return fieldInfo;
        else
            return null;
    }

    /// <summary>
    /// 将填写的数据类型字符串解析为DataType的枚举
    /// </summary>
    private static DataType _AnalyzeDataType(string inputTypeString)
    {
        if (string.IsNullOrEmpty(inputTypeString))
            return DataType.Invalid;

        string typeString = inputTypeString.Trim();

        if (typeString.StartsWith("int", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Int;
        if (typeString.StartsWith("long", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Long;
        else if (typeString.StartsWith("float", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Float;
        else if (typeString.StartsWith("string", StringComparison.CurrentCultureIgnoreCase))
            return DataType.String;
        else if (typeString.StartsWith("lang", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Lang;
        else if (typeString.StartsWith("bool", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Bool;
        else if (typeString.StartsWith("date", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Date;
        else if (typeString.StartsWith("time", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Time;
        else if (typeString.StartsWith("json", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Json;
        else if (typeString.StartsWith("tableString", StringComparison.CurrentCultureIgnoreCase))
            return DataType.TableString;
        else if (typeString.StartsWith("mapString", StringComparison.CurrentCultureIgnoreCase))
            return DataType.MapString;
        else if (typeString.StartsWith("array", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Array;
        else if (typeString.StartsWith("dict", StringComparison.CurrentCultureIgnoreCase))
            return DataType.Dict;
        else
            return DataType.Invalid;
    }

    private static bool _AnalyzeIntType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        fieldInfo.Data = new List<object>();
        // 记录非法数据的行号以及数据值（key：行号， value：数据值）
        Dictionary<int, string> invalidInfo = new Dictionary<int, string>();

        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                fieldInfo.Data.Add(null);
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputData))
                {
                    if (AppValues.IsAllowedNullNumber == true)
                        fieldInfo.Data.Add(null);
                    else
                        invalidInfo.Add(row, inputData);
                }
                else
                {
                    int intValue;
                    bool isValid = int.TryParse(inputData, out intValue);
                    if (isValid)
                        fieldInfo.Data.Add(intValue);
                    else
                        invalidInfo.Add(row, inputData);
                }
            }
        }

        if (invalidInfo.Count > 0)
        {
            StringBuilder invalidDataInfo = new StringBuilder();
            invalidDataInfo.Append("以下行中数据不是合法的int类型的值：\n");

            foreach (var item in invalidInfo)
                invalidDataInfo.AppendFormat("第{0}行，错误地填写数据为\"{1}\"\n", item.Key + 1, item.Value);

            errorString = invalidDataInfo.ToString();
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        else
        {
            errorString = null;
            nextFieldColumnIndex = columnIndex + 1;
            return true;
        }
    }

    private static bool _AnalyzeLongType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        fieldInfo.Data = new List<object>();
        // 记录非法数据的行号以及数据值（key：行号， value：数据值）
        Dictionary<int, string> invalidInfo = new Dictionary<int, string>();

        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                fieldInfo.Data.Add(null);
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputData))
                {
                    if (AppValues.IsAllowedNullNumber == true)
                        fieldInfo.Data.Add(null);
                    else
                        invalidInfo.Add(row, inputData);
                }
                else
                {
                    long longValue;
                    bool isValid = long.TryParse(inputData, out longValue);
                    if (isValid)
                        fieldInfo.Data.Add(longValue);
                    else
                        invalidInfo.Add(row, inputData);
                }
            }
        }

        if (invalidInfo.Count > 0)
        {
            StringBuilder invalidDataInfo = new StringBuilder();
            invalidDataInfo.Append("以下行中数据不是合法的long类型的值：\n");

            foreach (var item in invalidInfo)
                invalidDataInfo.AppendFormat("第{0}行，错误地填写数据为\"{1}\"\n", item.Key + 1, item.Value);

            errorString = invalidDataInfo.ToString();
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        else
        {
            errorString = null;
            nextFieldColumnIndex = columnIndex + 1;
            return true;
        }
    }

    private static bool _AnalyzeFloatType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        fieldInfo.Data = new List<object>();
        // 记录非法数据的行号以及数据值（key：行号， value：数据值）
        Dictionary<int, string> invalidInfo = new Dictionary<int, string>();

        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                fieldInfo.Data.Add(null);
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputData))
                {
                    if (AppValues.IsAllowedNullNumber == true)
                        fieldInfo.Data.Add(null);
                    else
                        invalidInfo.Add(row, inputData);
                }
                else
                {
                    double floatValue;
                    bool isValid = double.TryParse(inputData, out floatValue);
                    if (isValid)
                        fieldInfo.Data.Add(floatValue);
                    else
                        invalidInfo.Add(row, inputData);
                }
            }
        }

        if (invalidInfo.Count > 0)
        {
            StringBuilder invalidDataInfo = new StringBuilder();
            invalidDataInfo.Append("以下行中数据不是合法的float类型的值：\n");

            foreach (var item in invalidInfo)
                invalidDataInfo.AppendFormat("第{0}行，错误地填写数据为\"{1}\"\n", item.Key + 1, item.Value);

            errorString = invalidDataInfo.ToString();
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        else
        {
            errorString = null;
            nextFieldColumnIndex = columnIndex + 1;
            return true;
        }
    }

    private static bool _AnalyzeBoolType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        fieldInfo.Data = new List<object>();
        // 记录非法数据的行号以及数据值（key：行号， value：数据值）
        Dictionary<int, string> invalidInfo = new Dictionary<int, string>();

        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                fieldInfo.Data.Add(null);
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if ("1".Equals(inputData) || "true".Equals(inputData, StringComparison.CurrentCultureIgnoreCase))
                    fieldInfo.Data.Add(true);
                else if ("0".Equals(inputData) || "false".Equals(inputData, StringComparison.CurrentCultureIgnoreCase))
                    fieldInfo.Data.Add(false);
                else
                    invalidInfo.Add(row, inputData);
            }
        }

        if (invalidInfo.Count > 0)
        {
            StringBuilder invalidDataInfo = new StringBuilder();
            invalidDataInfo.Append("以下行中数据不是合法的bool值，正确填写bool值方式为填1或true代表真，0或false代表假：\n");

            foreach (var item in invalidInfo)
                invalidDataInfo.AppendFormat("第{0}行，错误地填写数据为\"{1}\"\n", item.Key + 1, item.Value);

            errorString = invalidDataInfo.ToString();
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        else
        {
            errorString = null;
            nextFieldColumnIndex = columnIndex + 1;
            return true;
        }
    }

    private static bool _AnalyzeStringType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        // 检查string型字段数据格式声明是否正确
        if (!"string(trim)".Equals(fieldInfo.DataTypeString) && !"string".Equals(fieldInfo.DataTypeString))
        {
            errorString = string.Format("错误：string型字段定义非法，若要自动去除输入字符串的首尾空白字符请将数据类型声明为\"string(trim)\"，否则声明为\"string\"，而你输入的为\"{0}\"", fieldInfo.DataTypeString);
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }

        fieldInfo.Data = new List<object>();

        if ("string(trim)".Equals(fieldInfo.DataTypeString, StringComparison.CurrentCultureIgnoreCase))
        {
            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                // 如果本行该字段的父元素标记为无效则该数据也标为无效
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                    fieldInfo.Data.Add(dt.Rows[row][columnIndex].ToString().Trim());
            }
        }
        else
        {
            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                    fieldInfo.Data.Add(dt.Rows[row][columnIndex].ToString());
            }
        }

        errorString = null;
        nextFieldColumnIndex = columnIndex + 1;
        return true;
    }

    private static bool _AnalyzeLangType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        fieldInfo.LangKeys = new List<object>();
        fieldInfo.Data = new List<object>();

        // 如果是统一指定key名规则，需要解析替换规则
        Dictionary<string, List<object>> replaceInfo = null;
        string configString = null;
        if (!fieldInfo.DataTypeString.Equals("lang", StringComparison.CurrentCultureIgnoreCase))
        {
            replaceInfo = _GetLangKeyReplaceInfo(fieldInfo.DataTypeString, tableInfo, out configString, out errorString);
            if (errorString != null)
            {
                errorString = string.Format("lang型格式定义错误，{0}", errorString);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }

            if (replaceInfo.Count < 1)
                Utils.LogWarning(string.Format("警告：表格{0}中的Lang型字段{1}（列号：{2}）进行统一配置但使用的是完全相同的key名（{3}），请确定是否要这样设置", tableInfo.TableName, fieldInfo.FieldName, Utils.GetExcelColumnName(fieldInfo.ColumnSeq + 1), configString));
        }

        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
            {
                fieldInfo.LangKeys.Add(null);
                fieldInfo.Data.Add(null);
                continue;
            }
            string inputData = null;
            // 未统一指定key名规则的Lang型数据，需在单元格中填写key
            if (replaceInfo == null)
                inputData = dt.Rows[row][columnIndex].ToString().Trim();
            // 统一指定key名规则的Lang型数据，根据规则生成在Lang文件中的具体key名
            else
            {
                inputData = configString;
                foreach (var item in replaceInfo)
                {
                    if (item.Value[row - AppValues.DATA_FIELD_DATA_START_INDEX] != null)
                        inputData = configString.Replace(item.Key, item.Value[row - AppValues.DATA_FIELD_DATA_START_INDEX].ToString());
                }
            }

            if (string.IsNullOrEmpty(inputData))
            {
                fieldInfo.LangKeys.Add(string.Empty);
                fieldInfo.Data.Add(null);
            }
            else
            {
                fieldInfo.LangKeys.Add(inputData);
                if (AppValues.LangData.ContainsKey(inputData))
                    fieldInfo.Data.Add(AppValues.LangData[inputData]);
                else
                    fieldInfo.Data.Add(null);
            }
        }

        errorString = null;
        nextFieldColumnIndex = columnIndex + 1;
        return true;
    }

    /// <summary>
    /// 获取统一配置的Lang型字段定义在合成key时需进行替换的信息（key：要替换的字符串形如{fieldName}， value：对应的字段数据列表）
    /// </summary>
    private static Dictionary<string, List<object>> _GetLangKeyReplaceInfo(string defineString, TableInfo tableInfo, out string configString, out string errorString)
    {
        Dictionary<string, List<object>> replaceInfo = new Dictionary<string, List<object>>();

        // 取出括号中key的配置
        int leftBracketIndex = defineString.IndexOf('(');
        int rightBracketIndex = defineString.LastIndexOf(')');
        if (leftBracketIndex == -1 || rightBracketIndex == -1 || leftBracketIndex > rightBracketIndex)
        {
            errorString = "lang类型统一key格式声明错误，必须形如lang(xxx{fieldName}xxx)，其中xxx可为任意内容，花括号中为拼成key的该行数据取哪个字段名下的值";
            configString = null;
            return null;
        }
        configString = defineString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
        if (string.IsNullOrEmpty(configString))
        {
            errorString = "lang类型统一key格式声明错误，括号内声明的key名规则不能为空";
            return null;
        }

        int leftBraceIndex = -1;
        for (int i = 0; i < defineString.Length; ++i)
        {
            if (defineString[i] == '{' && leftBraceIndex == -1)
                leftBraceIndex = i;
            else if (defineString[i] == '}' && leftBraceIndex != -1)
            {
                // 取出花括号中包含的字段名检查是否存在
                string refFieldName = defineString.Substring(leftBraceIndex + 1, i - leftBraceIndex - 1).Trim();
                FieldInfo refFielfInfo = tableInfo.GetFieldInfoByFieldName(refFieldName);
                if (refFielfInfo != null)
                    replaceInfo.Add(defineString.Substring(leftBraceIndex, i - leftBraceIndex + 1), refFielfInfo.Data);
                else
                {
                    errorString = string.Format("lang类型统一key格式声明错误，找不到名为{0}的字段，注意所引用的字段必须在这个lang字段之前声明且不为集合类型子元素，否则无法找到", refFieldName);
                    return null;
                }

                leftBraceIndex = -1;
            }
        }

        errorString = null;
        return replaceInfo;
    }

    /// <summary>
    /// 解析date型的格式类型
    /// </summary>
    public static DateFormatType GetDateFormatType(string formatString)
    {
        formatString = formatString.Trim();
        if ("#1970sec".Equals(formatString, StringComparison.CurrentCultureIgnoreCase))
            return DateFormatType.ReferenceDateSec;
        else if ("#1970msec".Equals(formatString, StringComparison.CurrentCultureIgnoreCase))
            return DateFormatType.ReferenceDateMsec;
        else if ("#dateTable".Equals(formatString, StringComparison.CurrentCultureIgnoreCase))
            return DateFormatType.DataTable;
        else
            return DateFormatType.FormatString;
    }

    /// <summary>
    /// 解析time型的格式类型
    /// </summary>
    public static TimeFormatType GetTimeFormatType(string formatString)
    {
        formatString = formatString.Trim();
        if ("#sec".Equals(formatString, StringComparison.CurrentCultureIgnoreCase))
            return TimeFormatType.ReferenceTimeSec;
        else
            return TimeFormatType.FormatString;
    }

    /// <summary>
    /// 解析date型数据的定义
    /// </summary>
    private static bool _AnalyzeDateType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        const string DEFINE_START_STRING = "date";
        // 定义date型输入导出格式的key
        const string INPUT_PARAM_KEY = "input";
        const string TO_LUA_PARAM_KEY = "toLua";
        const string TO_DATABASE_PARAM_KEY = "toDatabase";
        // 解析date型输入导出格式的声明
        if (!DEFINE_START_STRING.Equals(fieldInfo.DataTypeString, StringComparison.CurrentCultureIgnoreCase))
        {
            int leftBracketIndex = fieldInfo.DataTypeString.IndexOf('(');
            int rightBracketIndex = fieldInfo.DataTypeString.LastIndexOf(')');
            if (leftBracketIndex == -1 && rightBracketIndex == -1)
            {
                errorString = "date型格式定义错误";
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            if (!(leftBracketIndex != -1 || rightBracketIndex > leftBracketIndex))
            {
                errorString = "date型格式定义错误，括号不匹配";
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }

            // 解析声明的时间格式
            string defineString = fieldInfo.DataTypeString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
            if (string.IsNullOrEmpty(defineString))
            {
                errorString = "date型格式定义错误，若要声明时间格式就必须在括号中填写，否则不要加括号，本工具会采用config配置文件中设置的默认时间格式";
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            // 通过|分隔各个时间参数
            string[] defineParams = defineString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder paramDefineErrorStringBuilder = new StringBuilder();
            const string ERROR_STRING_FORMAT = "配置项\"{0}\"设置的格式\"{1}\"错误：{2}\n";
            foreach (string defineParam in defineParams)
            {
                // 通过=分隔参数项的key和value
                string[] paramKeyAndValue = defineParam.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (paramKeyAndValue.Length != 2)
                {
                    paramDefineErrorStringBuilder.AppendFormat("配置参数\"{0}\"未正确用=分隔配置项的key和value\n", defineParam);
                    continue;
                }
                string paramKey = paramKeyAndValue[0].Trim();
                string paramValue = paramKeyAndValue[1].Trim();

                errorString = null;
                switch (paramKey)
                {
                    case INPUT_PARAM_KEY:
                        {
                            if (TableCheckHelper.CheckDateInputDefine(paramValue, out errorString) == true)
                                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT] = paramValue;
                            else
                                paramDefineErrorStringBuilder.AppendFormat(ERROR_STRING_FORMAT, paramKey, paramValue, errorString);

                            break;
                        }
                    case TO_LUA_PARAM_KEY:
                        {
                            if (TableCheckHelper.CheckDateToLuaDefine(paramValue, out errorString) == true)
                                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_LUA_FORMAT] = paramValue;
                            else
                                paramDefineErrorStringBuilder.AppendFormat(ERROR_STRING_FORMAT, paramKey, paramValue, errorString);

                            break;
                        }
                    case TO_DATABASE_PARAM_KEY:
                        {
                            if (TableCheckHelper.CheckDateToDatabaseDefine(paramValue, out errorString) == true)
                                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_DATABASE_FORMAT] = paramValue;
                            else
                                paramDefineErrorStringBuilder.AppendFormat(ERROR_STRING_FORMAT, paramKey, paramValue, errorString);

                            break;
                        }
                    default:
                        {
                            paramDefineErrorStringBuilder.AppendFormat("存在非法配置项key\"{0}\"\n", paramKey);
                            break;
                        }
                }
            }
            string paramDefineErrorString = paramDefineErrorStringBuilder.ToString();
            if (!string.IsNullOrEmpty(paramDefineErrorString))
            {
                errorString = string.Format("date型格式定义存在以下错误：\n{0}", paramDefineErrorString);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
        }

        // 检查date型输入格式、导出至lua文件格式、导出至MySQL数据库格式是否都已声明，没有则分别采用config文件的默认设置
        if (!fieldInfo.ExtraParam.ContainsKey(AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT))
        {
            if (AppValues.DefaultDateInputFormat == null)
            {
                errorString = string.Format("未声明date型的输入格式，在config配置文件中也未定义名为\"{0}\"的默认格式配置项", AppValues.APP_CONFIG_KEY_DEFAULT_DATE_INPUT_FORMAT);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            else
                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT] = AppValues.DefaultDateInputFormat;
        }
        if (!fieldInfo.ExtraParam.ContainsKey(AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_LUA_FORMAT))
        {
            if (AppValues.DefaultDateToLuaFormat == null)
            {
                errorString = string.Format("未声明date型导出至lua文件的格式，在config配置文件中也未定义名为\"{0}\"的默认格式配置项", AppValues.APP_CONFIG_KEY_DEFAULT_DATE_TO_LUA_FORMAT);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            else
                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_LUA_FORMAT] = AppValues.DefaultDateToLuaFormat;
        }
        if (!fieldInfo.ExtraParam.ContainsKey(AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_DATABASE_FORMAT))
        {
            if (AppValues.DefaultDateToDatabaseFormat == null)
            {
                errorString = string.Format("未声明date型导出至MySQL数据库的格式，在config配置文件中也未定义名为\"{0}\"的默认格式配置项", AppValues.APP_CONFIG_KEY_DEFAULT_DATE_TO_DATABASE_FORMAT);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            else
                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_DATABASE_FORMAT] = AppValues.DefaultDateToDatabaseFormat;
        }

        DateFormatType dateFormatType = GetDateFormatType((string)fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT]);
        fieldInfo.Data = new List<object>();
        // 记录非法数据的行号以及数据值（key：行号， value：数据值）
        Dictionary<int, object> invalidInfo = new Dictionary<int, object>();

        if (dateFormatType == DateFormatType.FormatString)
        {
            // 用于对时间格式进行转换
            DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
            dateTimeFormat.ShortDatePattern = (string)fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT];

            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                // 如果本行该字段的父元素标记为无效则该数据也标为无效
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                {
                    string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                    // 忽略未填写的数据
                    if (string.IsNullOrEmpty(inputData))
                        fieldInfo.Data.Add(null);
                    else
                    {
                        try
                        {
                            DateTime dateTime = Convert.ToDateTime(inputData, dateTimeFormat);
                            fieldInfo.Data.Add(dateTime);
                        }
                        catch
                        {
                            invalidInfo.Add(row, inputData);
                        }
                    }
                }
            }
        }
        else if (dateFormatType == DateFormatType.ReferenceDateSec)
        {
            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                // 如果本行该字段的父元素标记为无效则该数据也标为无效
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                {
                    string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                    ulong inputLongValue = 0;
                    if (ulong.TryParse(inputData, out inputLongValue) == false)
                        invalidInfo.Add(row, inputData);
                    else
                    {
                        DateTime dateTime = AppValues.REFERENCE_DATE_LOCAL.AddSeconds((double)inputLongValue);
                        fieldInfo.Data.Add(dateTime);
                    }
                }
            }
        }
        else if (dateFormatType == DateFormatType.ReferenceDateMsec)
        {
            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                // 如果本行该字段的父元素标记为无效则该数据也标为无效
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                {
                    string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                    ulong inputLongValue = 0;
                    if (ulong.TryParse(inputData, out inputLongValue) == false)
                        invalidInfo.Add(row, inputData);
                    else
                    {
                        DateTime dateTime = AppValues.REFERENCE_DATE_LOCAL.AddMilliseconds((double)inputLongValue);
                        fieldInfo.Data.Add(dateTime);
                    }
                }
            }
        }
        else
        {
            errorString = "错误：用_AnalyzeDateType函数处理非法的DateFormatType类型";
            Utils.LogErrorAndExit(errorString);
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }

        if (invalidInfo.Count > 0)
        {
            StringBuilder invalidDataInfo = new StringBuilder();
            invalidDataInfo.AppendFormat("以下行中的数据无法按指定的输入格式（{0}）进行读取\n", fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT]);
            foreach (var item in invalidInfo)
                invalidDataInfo.AppendFormat("第{0}行，错误地填写数据为\"{1}\"\n", item.Key + 1, item.Value);

            errorString = invalidDataInfo.ToString();
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        else
        {
            errorString = null;
            nextFieldColumnIndex = columnIndex + 1;
            return true;
        }
    }

    /// <summary>
    /// 解析time型数据的定义
    /// </summary>
    private static bool _AnalyzeTimeType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        const string DEFINE_START_STRING = "time";

        // 定义time型输入导出格式的key
        const string INPUT_PARAM_KEY = "input";
        const string TO_LUA_PARAM_KEY = "toLua";
        const string TO_DATABASE_PARAM_KEY = "toDatabase";
        // 解析time型输入导出格式的声明
        if (!DEFINE_START_STRING.Equals(fieldInfo.DataTypeString, StringComparison.CurrentCultureIgnoreCase))
        {
            int leftBracketIndex = fieldInfo.DataTypeString.IndexOf('(');
            int rightBracketIndex = fieldInfo.DataTypeString.LastIndexOf(')');
            if (leftBracketIndex == -1 && rightBracketIndex == -1)
            {
                errorString = "time型格式定义错误";
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            if (!(leftBracketIndex != -1 || rightBracketIndex > leftBracketIndex))
            {
                errorString = "time型格式定义错误，括号不匹配";
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }

            // 解析声明的时间格式
            string defineString = fieldInfo.DataTypeString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
            if (string.IsNullOrEmpty(defineString))
            {
                errorString = "time型格式定义错误，若要声明时间格式就必须在括号中填写，否则不要加括号，本工具会采用config配置文件中设置的默认时间格式";
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            // 通过|分隔各个时间参数
            string[] defineParams = defineString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder paramDefineErrorStringBuilder = new StringBuilder();
            const string ERROR_STRING_FORMAT = "配置项\"{0}\"设置的格式\"{1}\"错误：{2}\n";
            foreach (string defineParam in defineParams)
            {
                // 通过=分隔参数项的key和value
                string[] paramKeyAndValue = defineParam.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (paramKeyAndValue.Length != 2)
                {
                    paramDefineErrorStringBuilder.AppendFormat("配置参数\"{0}\"未正确用=分隔配置项的key和value\n", defineParam);
                    continue;
                }
                string paramKey = paramKeyAndValue[0].Trim();
                string paramValue = paramKeyAndValue[1].Trim();

                errorString = null;
                switch (paramKey)
                {
                    case INPUT_PARAM_KEY:
                        {
                            if (TableCheckHelper.CheckTimeDefine(paramValue, out errorString) == true)
                                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT] = paramValue;
                            else
                                paramDefineErrorStringBuilder.AppendFormat(ERROR_STRING_FORMAT, paramKey, paramValue, errorString);

                            break;
                        }
                    case TO_LUA_PARAM_KEY:
                        {
                            if (TableCheckHelper.CheckTimeDefine(paramValue, out errorString) == true)
                                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_LUA_FORMAT] = paramValue;
                            else
                                paramDefineErrorStringBuilder.AppendFormat(ERROR_STRING_FORMAT, paramKey, paramValue, errorString);

                            break;
                        }
                    case TO_DATABASE_PARAM_KEY:
                        {
                            if (TableCheckHelper.CheckTimeDefine(paramValue, out errorString) == true)
                                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_DATABASE_FORMAT] = paramValue;
                            else
                                paramDefineErrorStringBuilder.AppendFormat(ERROR_STRING_FORMAT, paramKey, paramValue, errorString);

                            break;
                        }
                    default:
                        {
                            paramDefineErrorStringBuilder.AppendFormat("存在非法配置项key\"{0}\"\n", paramKey);
                            break;
                        }
                }
            }
            string paramDefineErrorString = paramDefineErrorStringBuilder.ToString();
            if (!string.IsNullOrEmpty(paramDefineErrorString))
            {
                errorString = string.Format("time型格式定义存在以下错误：\n{0}", paramDefineErrorString);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
        }

        // 检查time型输入格式、导出至lua文件格式、导出至MySQL数据库格式是否都已声明，没有则分别采用config文件的默认设置
        if (!fieldInfo.ExtraParam.ContainsKey(AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT))
        {
            if (AppValues.DefaultTimeInputFormat == null)
            {
                errorString = string.Format("未声明time型的输入格式，在config配置文件中也未定义名为\"{0}\"的默认格式配置项", AppValues.APP_CONFIG_KEY_DEFAULT_TIME_INPUT_FORMAT);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            else
                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT] = AppValues.DefaultTimeInputFormat;
        }
        if (!fieldInfo.ExtraParam.ContainsKey(AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_LUA_FORMAT))
        {
            if (AppValues.DefaultTimeToLuaFormat == null)
            {
                errorString = string.Format("未声明time型导出至lua文件的格式，在config配置文件中也未定义名为\"{0}\"的默认格式配置项", AppValues.APP_CONFIG_KEY_DEFAULT_TIME_TO_LUA_FORMAT);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            else
                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_LUA_FORMAT] = AppValues.DefaultTimeToLuaFormat;
        }
        if (!fieldInfo.ExtraParam.ContainsKey(AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_DATABASE_FORMAT))
        {
            if (AppValues.DefaultTimeToDatabaseFormat == null)
            {
                errorString = string.Format("未声明time型导出至MySQL数据库的格式，在config配置文件中也未定义名为\"{0}\"的默认格式配置项", AppValues.APP_CONFIG_KEY_DEFAULT_TIME_TO_DATABASE_FORMAT);
                nextFieldColumnIndex = columnIndex + 1;
                return false;
            }
            else
                fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_DATABASE_FORMAT] = AppValues.DefaultTimeToDatabaseFormat;
        }

        TimeFormatType timeFormatType = GetTimeFormatType((string)fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT]);
        fieldInfo.Data = new List<object>();
        // 记录非法数据的行号以及数据值（key：行号， value：数据值）
        Dictionary<int, object> invalidInfo = new Dictionary<int, object>();

        if (timeFormatType == TimeFormatType.FormatString)
        {
            // 用于对时间格式进行转换
            DateTimeFormatInfo dateTimeFormat = new DateTimeFormatInfo();
            dateTimeFormat.ShortTimePattern = (string)fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT];

            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                // 如果本行该字段的父元素标记为无效则该数据也标为无效
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                {
                    string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                    // 忽略未填写的数据
                    if (string.IsNullOrEmpty(inputData))
                        fieldInfo.Data.Add(null);
                    else
                    {
                        try
                        {
                            // 此函数会将DateTime的日期部分自动赋值为当前时间
                            DateTime tempDateTime = Convert.ToDateTime(inputData, dateTimeFormat);
                            fieldInfo.Data.Add(new DateTime(AppValues.REFERENCE_DATE.Year, AppValues.REFERENCE_DATE.Month, AppValues.REFERENCE_DATE.Day, tempDateTime.Hour, tempDateTime.Minute, tempDateTime.Second));
                        }
                        catch
                        {
                            invalidInfo.Add(row, inputData);
                        }
                    }
                }
            }
        }
        else if (timeFormatType == TimeFormatType.ReferenceTimeSec)
        {
            for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
            {
                // 如果本行该字段的父元素标记为无效则该数据也标为无效
                if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                    fieldInfo.Data.Add(null);
                else
                {
                    string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                    uint inputIntValue = 0;
                    if (uint.TryParse(inputData, out inputIntValue) == false)
                        invalidInfo.Add(row, inputData);
                    else
                    {
                        if (inputIntValue >= 86400)
                            invalidInfo.Add(row, inputData);
                        else
                        {
                            DateTime dateTime = AppValues.REFERENCE_DATE.AddSeconds(inputIntValue);
                            fieldInfo.Data.Add(dateTime);
                        }
                    }
                }
            }
        }
        else
        {
            errorString = "错误：用_AnalyzeTimeType函数处理非法的TimeFormatType类型";
            Utils.LogErrorAndExit(errorString);
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }

        if (invalidInfo.Count > 0)
        {
            StringBuilder invalidDataInfo = new StringBuilder();
            invalidDataInfo.AppendFormat("以下行中的数据无法按指定的输入格式（{0}）进行读取\n", fieldInfo.ExtraParam[AppValues.TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT]);
            foreach (var item in invalidInfo)
                invalidDataInfo.AppendFormat("第{0}行，错误地填写数据为\"{1}\"\n", item.Key + 1, item.Value);

            errorString = invalidDataInfo.ToString();
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        else
        {
            errorString = null;
            nextFieldColumnIndex = columnIndex + 1;
            return true;
        }
    }

    /// <summary>
    /// 解析json型数据的定义，将json通过LitJson库解析出来
    /// </summary>
    private static bool _AnalyzeJsonType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        StringBuilder errorStringBuilder = new StringBuilder();
        fieldInfo.Data = new List<object>();
        fieldInfo.JsonString = new List<string>();
        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
            {
                fieldInfo.Data.Add(null);
                fieldInfo.JsonString.Add(null);
            }
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputData))
                {
                    fieldInfo.Data.Add(null);
                    fieldInfo.JsonString.Add(null);
                }
                else
                {
                    fieldInfo.JsonString.Add(inputData);
                    try
                    {
                        JsonData jsonData = JsonMapper.ToObject(inputData);
                        fieldInfo.Data.Add(jsonData);
                    }
                    catch (JsonException exception)
                    {
                        errorStringBuilder.AppendFormat("第{0}行所填json字符串（{1}）非法，原因为：{2}\n", row + AppValues.DATA_FIELD_DATA_START_INDEX + 1, inputData, exception.Message);
                    }
                }
            }
        }

        nextFieldColumnIndex = columnIndex + 1;
        errorString = errorStringBuilder.ToString();
        if (string.IsNullOrEmpty(errorString))
        {
            errorString = null;
            return true;
        }
        else
        {
            errorString = string.Concat("以下行中所填json字符串非法：\n", errorString);
            return false;
        }
    }

    /// <summary>
    /// 解析tableString型数据的定义，将其格式解析为TableStringFormatDefine类，但填写的数据直接以字符串形式存在FieldInfo的Data变量中
    /// </summary>
    private static bool _AnalyzeTableStringType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        // 检查定义字符串是否合法并转为TableStringFormatDefine的定义结构
        fieldInfo.TableStringFormatDefine = _GetTableStringFormatDefine(fieldInfo.DataTypeString, out errorString);
        if (errorString != null)
        {
            errorString = string.Format("tableString格式定义错误，{0}，你输入的类型定义字符串为{1}", errorString, fieldInfo.DataTypeString);
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        // 将填写的数据直接以字符串形式存在FieldInfo的Data变量中
        fieldInfo.Data = new List<object>();
        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
                fieldInfo.Data.Add(null);
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputData))
                    fieldInfo.Data.Add(null);
                else
                    fieldInfo.Data.Add(inputData);
            }
        }

        errorString = null;
        nextFieldColumnIndex = columnIndex + 1;
        return true;
    }

    private static bool _AnalyzeMapStringType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        StringBuilder errorStringBuilder = new StringBuilder();

        fieldInfo.MapStringFormatDefine = MapStringAnalyzeHelper.GetMapStringFormatDefine(fieldInfo.DataTypeString, out errorString);
        if (errorString != null)
        {
            errorString = string.Format("{0}，你输入的类型定义字符串为{1}", errorString, fieldInfo.DataTypeString);
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        // 将填写的数据存在FieldInfo的JsonString变量中，然后将数据转为JsonData存在Data变量中
        fieldInfo.JsonString = new List<string>();
        fieldInfo.Data = new List<object>();
        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            // 如果本行该字段的父元素标记为无效则该数据也标为无效
            if (parentField != null && (bool)parentField.Data[row - AppValues.DATA_FIELD_DATA_START_INDEX] == false)
            {
                fieldInfo.JsonString.Add(null);
                fieldInfo.Data.Add(null);
            }
            else
            {
                string inputData = dt.Rows[row][columnIndex].ToString().Trim();
                if (string.IsNullOrEmpty(inputData))
                {
                    fieldInfo.JsonString.Add(null);
                    fieldInfo.Data.Add(null);
                }
                else
                {
                    fieldInfo.JsonString.Add(inputData);

                    // 将输入的数据转为JsonData
                    JsonData jsonData = MapStringAnalyzeHelper.GetMapStringData(inputData, fieldInfo.MapStringFormatDefine, out errorString);
                    if (errorString == null)
                        fieldInfo.Data.Add(jsonData);
                    else
                    {
                        errorStringBuilder.AppendFormat("第{0}行填写的数据（{1}）非法，{2}\n", row + 1, inputData, errorString);
                        fieldInfo.Data.Add(null);
                    }
                }
            }
        }

        nextFieldColumnIndex = columnIndex + 1;
        errorString = errorStringBuilder.ToString();
        if (string.IsNullOrEmpty(errorString))
        {
            errorString = null;
            return true;
        }
        else
        {
            errorString = string.Format("以下行中的数据不符合该字段mapString类型（{0}）的格式要求：\n{1}", fieldInfo.DataTypeString, errorString);
            return false;
        }
    }

    private static TableStringFormatDefine _GetTableStringFormatDefine(string dataTypeString, out string errorString)
    {
        TableStringFormatDefine formatDefine = new TableStringFormatDefine();

        // 必须在tableString[]中声明格式
        const string DEFINE_START_STRING = "tableString[";
        if (!(dataTypeString.StartsWith(DEFINE_START_STRING, StringComparison.CurrentCultureIgnoreCase) && dataTypeString.EndsWith("]")))
        {
            errorString = "必须在tableString[]中声明，即以\"tableString[\"开头，以\"]\"结尾";
            return formatDefine;
        }
        // 去掉外面的tableString[]，取得中间定义内容
        int startIndex = DEFINE_START_STRING.Length;
        string formatString = dataTypeString.Substring(startIndex, dataTypeString.Length - startIndex - 1).Trim();
        // 通过|分离key和value的声明
        string[] keyAndValueFormatString = formatString.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (keyAndValueFormatString.Length != 2)
        {
            errorString = "必须用|分隔key和value";
            return formatDefine;
        }
        // 解析key声明
        string keyFormatString = keyAndValueFormatString[0].Trim();
        const string KEY_START_STRING = "k:";
        if (!keyFormatString.StartsWith(KEY_START_STRING, StringComparison.CurrentCultureIgnoreCase))
        {
            errorString = "key的声明必须以k:开头";
            return formatDefine;
        }
        else
        {
            // 去除开头的k:
            keyFormatString = keyFormatString.Substring(KEY_START_STRING.Length).Trim();

            // 按数据顺序自动编号
            if (keyFormatString.Equals("#seq", StringComparison.CurrentCultureIgnoreCase))
            {
                formatDefine.KeyDefine.KeyType = TableStringKeyType.Seq;
            }
            // 以数据组中指定索引位置的数据为key
            else if (keyFormatString.StartsWith("#"))
            {
                formatDefine.KeyDefine.KeyType = TableStringKeyType.DataInIndex;
                formatDefine.KeyDefine.DataInIndexDefine = _GetDataInIndexDefine(keyFormatString, out errorString);
                if (errorString != null)
                {
                    errorString = "key的声明未符合形如#1(int)\n" + errorString;
                    return formatDefine;
                }
                // 只有int、long或string型数据才能作为key
                if (!(formatDefine.KeyDefine.DataInIndexDefine.DataType == DataType.Int || formatDefine.KeyDefine.DataInIndexDefine.DataType == DataType.Long || formatDefine.KeyDefine.DataInIndexDefine.DataType == DataType.String))
                {
                    errorString = string.Format("key只允许为int、long或string型，你定义的类型为{0}\n", formatDefine.KeyDefine.DataInIndexDefine.DataType.ToString());
                    return formatDefine;
                }
            }
            else
            {
                errorString = "key声明非法";
                return formatDefine;
            }
        }

        // 解析value声明
        string valueFormatString = keyAndValueFormatString[1].Trim();
        const string VALUE_START_STRING = "v:";
        if (!valueFormatString.StartsWith(VALUE_START_STRING, StringComparison.CurrentCultureIgnoreCase))
        {
            errorString = "value的声明必须以v:开头";
            return formatDefine;
        }
        else
        {
            // 去除开头的v:
            valueFormatString = valueFormatString.Substring(VALUE_START_STRING.Length).Trim();

            // value始终为true
            if (valueFormatString.Equals("#true", StringComparison.CurrentCultureIgnoreCase))
            {
                formatDefine.ValueDefine.ValueType = TableStringValueType.True;
            }
            // value为table类型
            else if (valueFormatString.StartsWith("#table", StringComparison.CurrentCultureIgnoreCase))
            {
                formatDefine.ValueDefine.ValueType = TableStringValueType.Table;
                // 判断是否形如#table(xxx)
                int leftBracketIndex = valueFormatString.IndexOf('(');
                int rightBracketIndex = valueFormatString.LastIndexOf(')');
                if (leftBracketIndex == -1 || rightBracketIndex != valueFormatString.Length - 1)
                {
                    errorString = "table类型value格式声明错误，必须形如#table(xxx)";
                    return formatDefine;
                }
                else
                {
                    // 去掉#table(xxx)外面，只保留括号中的内容
                    string tableDefineString = valueFormatString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
                    // table中每个键值对的声明用英文逗号隔开
                    string[] tableElementDefine = tableDefineString.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    // 解析每个键值对的定义，形如type=#1(int)
                    formatDefine.ValueDefine.TableValueDefineList = new List<TableElementDefine>();
                    // 记录每个键值对的key，不允许重复（key：key名， value：第几组键值对，从0开始记）
                    Dictionary<string, int> tableKeys = new Dictionary<string, int>();
                    for (int i = 0; i < tableElementDefine.Length; ++i)
                    {
                        TableElementDefine oneTableElementDefine = _GetTableElementDefine(tableElementDefine[i].Trim(), out errorString);
                        if (errorString != null)
                        {
                            errorString = string.Format("table类型值声明错误，无法解析{0}，", tableElementDefine[i].Trim()) + errorString;
                            return formatDefine;
                        }
                        else
                        {
                            // 检查定义的key是否重复
                            if (tableKeys.ContainsKey(oneTableElementDefine.KeyName))
                            {
                                errorString = string.Format("table类型的第{0}个与第{1}个子元素均为相同的key（{2}）", tableKeys[oneTableElementDefine.KeyName] + 1, i + 1, oneTableElementDefine.KeyName);
                                return formatDefine;
                            }
                            else
                            {
                                tableKeys.Add(oneTableElementDefine.KeyName, i + 1);
                                formatDefine.ValueDefine.TableValueDefineList.Add(oneTableElementDefine);
                            }
                        }
                    }
                }
            }
            // 以数据组中指定索引位置的数据为value
            else if (valueFormatString.StartsWith("#"))
            {
                formatDefine.ValueDefine.ValueType = TableStringValueType.DataInIndex;
                formatDefine.ValueDefine.DataInIndexDefine = _GetDataInIndexDefine(valueFormatString, out errorString);
                if (errorString != null)
                {
                    errorString = "value的声明未符合形如#1(int)\n" + errorString;
                    return formatDefine;
                }
            }
            else
            {
                errorString = "value声明非法";
                return formatDefine;
            }
        }

        errorString = null;
        return formatDefine;
    }

    /// <summary>
    /// 将形如type=#1(int)的格式定义字符串转为TableElementDefine定义
    /// </summary>
    private static TableElementDefine _GetTableElementDefine(string tableElementDefine, out string errorString)
    {
        TableElementDefine elementDefine = new TableElementDefine();

        // 判断是否用=分隔key与value定义
        string[] keyAndValueString = tableElementDefine.Trim().Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (keyAndValueString.Length != 2)
        {
            errorString = "必须用=分隔键值对";
            return elementDefine;
        }
        else
        {
            // 取得并检查key名
            elementDefine.KeyName = keyAndValueString[0].Trim();
            TableCheckHelper.CheckFieldName(elementDefine.KeyName, out errorString);
            if (errorString != null)
            {
                errorString = "键值对中键名非法，" + errorString;
                return elementDefine;
            }
            // 解析value的定义
            elementDefine.DataInIndexDefine = _GetDataInIndexDefine(keyAndValueString[1].Trim(), out errorString);
            if (errorString != null)
            {
                errorString = "键值对中value的声明未符合形如#1(int)\n" + errorString;
                return elementDefine;
            }
        }

        errorString = null;
        return elementDefine;
    }

    /// <summary>
    /// 将形如#1(int)的格式定义字符串转为DataInIndexDefine定义
    /// </summary>
    private static DataInIndexDefine _GetDataInIndexDefine(string defineString, out string errorString)
    {
        DataInIndexDefine dataInIndexDefine = new DataInIndexDefine();

        // 检查#后是否跟合法数字，且数字后面用括号注明类型
        defineString = defineString.Substring(1).Trim();
        // 检查类型是否合法
        int leftBracketIndex = defineString.IndexOf('(');
        int rightBracketIndex = defineString.LastIndexOf(')');
        if (leftBracketIndex == -1 || rightBracketIndex != defineString.Length - 1)
        {
            errorString = "未注明格式类型，需要形如：#1(int)";
            return dataInIndexDefine;
        }
        else
        {
            string dataTypeString = defineString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
            dataInIndexDefine.DataType = _AnalyzeDataType(dataTypeString);
            if (!(dataInIndexDefine.DataType == DataType.Int || dataInIndexDefine.DataType == DataType.Long || dataInIndexDefine.DataType == DataType.Float || dataInIndexDefine.DataType == DataType.Bool || dataInIndexDefine.DataType == DataType.String || dataInIndexDefine.DataType == DataType.Lang))
            {
                errorString = "格式类型非法，只支持int、long、float、bool、string、lang这几种类型";
                return dataInIndexDefine;
            }
        }
        // 检查数据索引值是否合法
        string dataIndexString = defineString.Substring(0, leftBracketIndex).Trim();
        int dataIndex;
        if (int.TryParse(dataIndexString, out dataIndex))
        {
            if (dataIndex > 0)
                dataInIndexDefine.DataIndex = dataIndex;
            else
            {
                errorString = "数据索引值编号最小要从1开始";
                return dataInIndexDefine;
            }
        }
        else
        {
            errorString = string.Format("数据索引值不是合法数字，你的输入值为:{0}", dataIndexString);
            return dataInIndexDefine;
        }

        errorString = null;
        return dataInIndexDefine;
    }

    private static bool _AnalyzeArrayType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        // dict或array集合类型中，如果定义列中的值填-1代表这行数据的该字段不生效，Data中用true和false代表该集合字段的某行数据是否生效
        fieldInfo.Data = _GetValidInfoForSetData(dt, columnIndex, fieldInfo.TableName);
        // 为了在多重嵌套的集合结构中快速判断出是不是在任意上层集合中已经被标为无效，这里只要在上层标为无效，在其所有下层都会进行进行标记，从而在判断数据是否有效时只要判断向上一层是否标记为无效即可
        if (parentField != null)
        {
            int dataCount = Math.Min(fieldInfo.Data.Count, parentField.Data.Count);
            for (int i = 0; i < dataCount; ++i)
            {
                if ((bool)parentField.Data[i] == false)
                    fieldInfo.Data[i] = false;
            }
        }
        // 解析array声明的子元素的数据类型和个数
        string childDataTypeString;
        DataType childDataType;
        int childCount;
        _GetArrayChildDefine(fieldInfo.DataTypeString, out childDataTypeString, out childDataType, out childCount, out errorString);
        if (errorString != null)
        {
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        // 存储子类型的类型以及类型定义字符串
        fieldInfo.ArrayChildDataTypeString = childDataTypeString;
        fieldInfo.ArrayChildDataType = childDataType;
        // 解析之后的几列作为array的下属元素
        fieldInfo.ChildField = new List<FieldInfo>();
        nextFieldColumnIndex = columnIndex + 1;
        int tempCount = childCount;
        int seq = 1;
        while (tempCount > 0)
        {
            int nextColumnIndex = nextFieldColumnIndex;
            FieldInfo childFieldInfo = _AnalyzeOneField(dt, tableInfo, nextColumnIndex, fieldInfo, out nextFieldColumnIndex, out errorString);
            if (errorString != null)
            {
                errorString = string.Format("array类型数据下属元素（列号：{0}）的解析存在错误\n{1}", Utils.GetExcelColumnName(nextColumnIndex + 1), errorString);
                return false;
            }
            else
            {
                // 忽略无效列
                if (childFieldInfo != null)
                {
                    // 将array下属子元素的fieldName改为顺序编号
                    childFieldInfo.FieldName = string.Format("[{0}]", seq);
                    ++seq;

                    fieldInfo.ChildField.Add(childFieldInfo);
                    --tempCount;
                }
            }
        }

        // 如果array的子元素为array或dict类型，当前面的子元素用-1标识为无效后，后面的数据也必须声明为无效的，比如用array[dict[3]:5]表示一场推图战斗胜利最多可获得的5种奖励物，如果某一行对应的关卡至多只有3种奖励物，则必须填在前3个子元素列中，后面2个声明为无效
        if (fieldInfo.ArrayChildDataType == DataType.Array || fieldInfo.ArrayChildDataType == DataType.Dict)
        {
            for (int i = 0; i < fieldInfo.Data.Count; ++i)
            {
                // 如果本行数据中array下属的集合型子元素已经读取到-1标识的无效数据，记录其是第几个子元素
                int invalidDataIndex = 0;
                for (int j = 0; j < fieldInfo.ChildField.Count; ++j)
                {
                    if ((bool)fieldInfo.ChildField[j].Data[i] == true)
                    {
                        if (invalidDataIndex != 0)
                        {
                            errorString = string.Format("array的子元素为array或dict类型时，当前面的子元素用-1标识为无效后，后面的数据也必须声明为无效的。而第{0}行第{1}个子元素声明为无效，而后面第{2}个子元素却有效\n", i + AppValues.DATA_FIELD_DATA_START_INDEX + 1, invalidDataIndex, j + 1);
                            return false;
                        }
                    }
                    else
                        invalidDataIndex = j + 1;
                }
            }
        }

        errorString = null;
        return true;
    }

    /// <summary>
    /// 解析形如array[type:n]（type为类型，n为array中元素个数）的array类型的声明字符串，获得子元素的类型以及子元素的个数
    /// </summary>
    private static bool _GetArrayChildDefine(string defineString, out string childDataTypeString, out DataType childDataType, out int childCount, out string errorString)
    {
        childDataTypeString = null;
        childDataType = DataType.Invalid;
        childCount = 0;
        errorString = null;

        int leftBracketIndex = defineString.IndexOf('[');
        int rightBracketIndex = defineString.LastIndexOf(']');
        if (leftBracketIndex != -1 && rightBracketIndex != -1 && leftBracketIndex < rightBracketIndex)
        {
            // 去掉首尾包裹的array[]
            string typeAndCountString = defineString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1).Trim();
            childDataTypeString = typeAndCountString;
            // 通过冒号分离类型和个数（注意不能用Split分隔，可能出现array[array[int:2]:3]这种嵌套结构，必须去找最后一个冒号的位置）
            int lastColonIndex = typeAndCountString.LastIndexOf(':');
            if (lastColonIndex == -1)
            {
                errorString = string.Format("array类型数据声明不合法，请采用array[type:n]的形式，冒号分隔类型与个数的定义，你填写的类型为{0}", defineString);
                return false;
            }
            else
            {
                string typeString = typeAndCountString.Substring(0, lastColonIndex).Trim();
                string countString = typeAndCountString.Substring(lastColonIndex + 1, typeAndCountString.Length - lastColonIndex - 1).Trim();

                // 判断类型是否合法
                DataType inputChildDataType = _AnalyzeDataType(typeString);
                if (inputChildDataType == DataType.Invalid)
                {
                    errorString = string.Format("array类型数据声明不合法，子类型错误，你填写的类型为{0}", defineString);
                    return false;
                }
                childDataType = inputChildDataType;
                // 判断个数是否合法
                int count;
                if (!int.TryParse(countString, out count))
                {
                    errorString = string.Format("array类型数据声明不合法，声明的下属元素个数不是合法的数字，你填写的个数为{0}", countString);
                    return false;
                }
                if (count < 1)
                {
                    errorString = string.Format("array类型数据声明不合法，声明的下属元素个数不能低于1个，你填写的个数为{0}", countString);
                    return false;
                }
                childCount = count;
                return true;
            }
        }
        else
        {
            errorString = string.Format("array类型数据声明不合法，请采用array[type:n]的形式，其中type表示类型，n表示array中元素个数，你填写的类型为{0}", defineString);
            return false;
        }
    }

    private static bool _AnalyzeDictType(FieldInfo fieldInfo, TableInfo tableInfo, DataTable dt, int columnIndex, FieldInfo parentField, out int nextFieldColumnIndex, out string errorString)
    {
        // dict或array集合类型中，如果定义列中的值填-1代表这行数据的该字段不生效，Data中用true和false代表该集合字段的某行数据是否生效
        fieldInfo.Data = _GetValidInfoForSetData(dt, columnIndex, fieldInfo.TableName);
        // 为了在多重嵌套的集合结构中快速判断出是不是在任意上层集合中已经被标为无效，这里只要在上层标为无效，在其所有下层都会进行进行标记，从而在判断数据是否有效时只要判断向上一层是否标记为无效即可
        if (parentField != null)
        {
            int dataCount = Math.Min(fieldInfo.Data.Count, parentField.Data.Count);
            for (int i = 0; i < dataCount; ++i)
            {
                if ((bool)parentField.Data[i] == false)
                    fieldInfo.Data[i] = false;
            }
        }
        // 记录dict中子元素的字段名，dict中不允许同名字段
        List<string> inputFieldNames = new List<string>();
        // 解析dict声明的子元素个数
        int childCount;
        _GetDictChildCount(fieldInfo.DataTypeString, out childCount, out errorString);
        if (errorString != null)
        {
            nextFieldColumnIndex = columnIndex + 1;
            return false;
        }
        // 解析之后的几列作为array的下属元素
        fieldInfo.ChildField = new List<FieldInfo>();
        nextFieldColumnIndex = columnIndex + 1;
        int tempCount = childCount;
        while (tempCount > 0)
        {
            int nextColumnIndex = nextFieldColumnIndex;
            FieldInfo childFieldInfo = _AnalyzeOneField(dt, tableInfo, nextColumnIndex, fieldInfo, out nextFieldColumnIndex, out errorString);
            if (errorString != null)
            {
                errorString = string.Format("dict类型数据下属元素（列号：{0}）的解析存在错误\n{1}", Utils.GetExcelColumnName(nextColumnIndex + 1), errorString);
                return false;
            }
            else
            {
                // 忽略无效列
                if (childFieldInfo != null)
                {
                    // 检查dict子元素中是否已经存在同名的字段
                    if (inputFieldNames.Contains(childFieldInfo.FieldName))
                    {
                        errorString = string.Format("dict类型的子元素中不允许含有同名字段（{0}）\n", childFieldInfo.FieldName);
                        return false;
                    }
                    else
                        inputFieldNames.Add(childFieldInfo.FieldName);

                    fieldInfo.ChildField.Add(childFieldInfo);
                    --tempCount;
                }
            }
        }

        errorString = null;
        return true;
    }

    /// <summary>
    /// 解析形如dict[n]（n为子元素个数）的dict类型的声明字符串，获得子元素的个数
    /// </summary>
    private static bool _GetDictChildCount(string defineString, out int childCount, out string errorString)
    {
        childCount = 0;
        errorString = null;

        int leftBracketIndex = defineString.IndexOf('[');
        int rightBracketIndex = defineString.LastIndexOf(']');
        if (leftBracketIndex != -1 && rightBracketIndex != -1 && leftBracketIndex < rightBracketIndex)
        {
            // 取出括号中的数字，判断是否合法
            string countString = defineString.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
            int count;
            if (!int.TryParse(countString, out count))
            {
                errorString = string.Format("dict类型数据声明不合法，声明的下属元素个数不是合法的数字，你填写的个数为{0}", countString);
                return false;
            }
            if (count < 1)
            {
                errorString = string.Format("dict类型数据声明不合法，声明的下属元素个数不能低于1个，你填写的个数为{0}", countString);
                return false;
            }
            childCount = count;
            return true;
        }
        else
        {
            errorString = string.Format("dict类型数据声明不合法，请采用dict[n]的形式，其中n表示下属元素个数，你填写的类型为{0}", defineString);
            return false;
        }
    }

    /// <summary>
    /// 获取dict或array数据类型中，每一行数据的有效性（填-1表示本行的这个数据无效）
    /// </summary>
    private static List<object> _GetValidInfoForSetData(DataTable dt, int columnIndex, string tableName)
    {
        List<object> validInfo = new List<object>();

        for (int row = AppValues.DATA_FIELD_DATA_START_INDEX; row < dt.Rows.Count; ++row)
        {
            string inputData = dt.Rows[row][columnIndex].ToString().Trim();
            if ("-1".Equals(inputData))
                validInfo.Add(false);
            else if (string.IsNullOrEmpty(inputData))
                validInfo.Add(true);
            else
            {
                Utils.LogWarning(string.Format("警告：表格{0}的第{1}列的第{2}行数据有误，array或dict类型中若某行不需要此数据请填-1，否则留空，你填写的为\"{3}\"，本工具按有效值进行处理，但请按规范更正", tableName, Utils.GetExcelColumnName(columnIndex + 1), row, inputData));
                validInfo.Add(true);
            }
        }

        return validInfo;
    }

    /// <summary>
    /// 当表格存在错误无法继续时，输出内容前统一加上表格名和列名
    /// </summary>
    private static string _GetTableAnalyzeErrorString(string tableName, int columnIndex)
    {
        return string.Format("表格{0}中列号为{1}的字段存在以下严重错误，导致无法继续，请修正错误后重试\n", tableName, Utils.GetExcelColumnName(columnIndex + 1));
    }

    /// <summary>
    /// 获取某张表格的配置参数（key：参数名， value：参数列表）
    /// </summary>
    public static Dictionary<string, List<string>> GetTableConfig(DataTable dt, out string errorString)
    {
        Dictionary<string, List<string>> config = new Dictionary<string, List<string>>();
        int rowCount = dt.Rows.Count;
        int columnCount = dt.Columns.Count;

        for (int column = 0; column < columnCount; ++column)
        {
            // 取参数名
            string paramName = dt.Rows[AppValues.CONFIG_FIELD_DEFINE_INDEX][column].ToString().Trim();
            if (string.IsNullOrEmpty(paramName))
                continue;
            else
            {
                if (config.ContainsKey(paramName))
                {
                    errorString = string.Format("错误：配置表中存在相同的参数名{0}，请修正错误后重试\n", paramName);
                    return null;
                }
                else
                    config.Add(paramName, new List<string>());
            }

            // 取具体参数配置
            List<string> inputParams = config[paramName];
            for (int row = AppValues.CONFIG_FIELD_PARAM_START_INDEX; row < rowCount; ++row)
            {
                string param = dt.Rows[row][column].ToString();
                if (!string.IsNullOrEmpty(param))
                {
                    if (inputParams.Contains(param))
                        Utils.LogWarning(string.Format("警告：配置项（参数名为{0}）中存在相同的参数\"{1}\"，请确认是否填写错误\n", paramName, param));

                    inputParams.Add(param);
                }
            }
        }

        errorString = null;
        if (config.Count > 0)
            return config;
        else
            return null;
    }
}
