namespace PureMVC.UGFramework.Editor
{
    using PureMVC.UGFramework.Core;
    using LitJson;
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System;
    using System.Collections;
    using SqlCipher4Unity3D;
    using ExcelDataReader;
    using System.Data;

    public class DaoUtility
    {
        //---------- Following defination should be customed according to project. ----------
        public static readonly string ConfigFilePath = Application.dataPath + "/export_db_config.json";
        public static readonly string DbExportPath = Application.dataPath + "/StreamingAssets/db";
        public static readonly string ExcelFilePath = Application.dataPath + "/../../../Design";
        public static readonly string CodeGeneratePath = Application.dataPath + "/Patch/Patch/Src/Dao";
        public static readonly string DbPassword = "@(7$$5)1";
        //---------- Above defination should be customed according to project. ----------

        public class ExcelTable
        {
            private DataTable m_Data = null;
            private string m_TableName = string.Empty;
            private bool m_IsValid = false;
            private int m_PrimaryKeyIndex = -1;
            private List<string> m_FieldNames = new List<string>();
            private List<string> m_FieldTypes = new List<string>();

            private string m_CreateTableSqlCmd = string.Empty;
            private List<string> m_UpdateTableSqlCmdList = new List<string>();

            public bool IsValid
            {
                get
                {
                    return m_IsValid;
                }
            }

            public string TableName
            {
                get
                {
                    return m_TableName;
                }
            }

            public DataTable Data
            {
                get
                {
                    return m_Data;
                }
            }

            public string CreateTableSqlCommand
            {
                get
                {
                    return m_CreateTableSqlCmd;
                }
            }

            public List<string> UpdateTableSqlCmdList
            {
                get
                {
                    return m_UpdateTableSqlCmdList;
                }
            }

            public ExcelTable(string TableName, DataTable TableData)
            {
                if (string.IsNullOrEmpty(TableName) || TableData == null)
                {
                    return;
                }

                int _Columns = TableData.Columns.Count;
                int _Rows = TableData.Rows.Count;

                if (_Columns <= 0 || _Rows < 2)
                {
                    Debug.LogError("[Export Data] Columns is 0 or rows is less than 2 in [" + TableName + "]");
                    return;
                }

                m_TableName = TableName;
                m_Data = TableData;

                for (int i = 0; i < _Columns; ++ i)
                {
                    string _FieldName = TableData.Rows[1][i].ToString();

                    if (string.IsNullOrEmpty(_FieldName))
                    {
                        Debug.LogError("[Export Data] Field name is null or empty in column " + i + "[" + TableName + "]");
                        return;
                    }

                    m_FieldNames.Add(_FieldName);

                    string _FieldType = TableData.Rows[0][i].ToString();

                    if (string.IsNullOrEmpty(_FieldType))
                    {
                        Debug.LogError("[Export Data] Field type is null or empty in column " + i + "[" + TableName + "]");
                        return;
                    }

                    _FieldType = _FieldType.ToLower().Trim();

                    if (_FieldType.Contains("primary key"))
                    {
                        if (m_PrimaryKeyIndex != -1)
                        {
                            Debug.LogError("[Export Data] More than one primary key is detected.");
                            return;
                        }

                        m_PrimaryKeyIndex = i;

                        _FieldType = _FieldType.Replace("primary key", "");
                        _FieldType = _FieldType.Replace(",", "");
                        _FieldType = _FieldType.Trim();
                    }

                    _FieldType = _FieldType.ToUpper();
                    m_FieldTypes.Add(_FieldType);
                }

                if (m_PrimaryKeyIndex == -1)
                {
                    Debug.LogError("[Export Data] No primary key is detected.");
                    return;
                }

                m_CreateTableSqlCmd = GenerateCreateTableSqlString();






                m_IsValid = true;
            }

            protected string GenerateCreateTableSqlString()
            {
                string _Ret = string.Empty;

                _Ret += "CREATE TABLE '"; // CREATE TABLE '
                _Ret += m_TableName; // CREATE TABLE '[Table Name]
                _Ret += "' ("; // CREATE TABLE '[Table Name]' (
                _Ret += "'" + m_FieldNames[m_PrimaryKeyIndex] + "' " + m_FieldTypes[m_PrimaryKeyIndex] + " PRIMARY KEY NOT NULL";

                for (int i = 0; i < m_Data.Columns.Count; ++i)
                {
                    if (i == m_PrimaryKeyIndex)
                    {
                        continue;
                    }

                    _Ret += ", '" + m_FieldNames[i] + "' " + m_FieldTypes[i] + " NOT NULL";
                }

                _Ret += ")"; // CREATE TABLE '[Table Name]' ()

                return string.Empty;
            }
        }

        [MenuItem("UGFramework/Config Data/Create Export Config")]
        public static void ConfigData_CreateExportConfig()
        {
            File.Delete(ConfigFilePath);

            JsonData _Content = new JsonData();
            _Content["[DB Name 1]"] = new JsonData();
            _Content["[DB Name 1]"]["[Excel Table Name 1]"] = "Excel File Name 1";
            _Content["[DB Name 1]"]["[Excel Table Name 2]"] = "Excel File Name 2";
            _Content["[DB Name 2]"] = new JsonData();
            _Content["[DB Name 2]"]["[Excel Table Name 3]"] = "Excel File Name 3";
            _Content["[DB Name 2]"]["[Excel Table Name 4]"] = "Excel File Name 4";

            StringBuilder _Builder = new StringBuilder();

            JsonWriter _Writer = new JsonWriter(_Builder);
            _Writer.PrettyPrint = true;
            _Writer.IndentValue = 2;

            JsonMapper.ToJson(_Content, _Writer);
                        
            File.WriteAllText(ConfigFilePath, _Builder.ToString());
        }

        [MenuItem("UGFramework/Config Data/Export Config Data")]
        public static void ConfigData_ExportConfigData()
        {
            Debug.Log("======> Valid Excel File Path <======");

            if (!Directory.Exists(ExcelFilePath))
            {
                Debug.LogError("[Export Data] Excel data source doesn't exist.");
                return;
            }

            Debug.Log("======> Remove Previous Export Data <======");

            if (Directory.Exists(DbExportPath))
            {
                SystemUtility.DeleteDirectory(DbExportPath);
            }

            Directory.CreateDirectory(DbExportPath);

            Debug.Log("======> Remove Previous Generated Code");

            if (Directory.Exists(CodeGeneratePath))
            {
                SystemUtility.DeleteDirectory(CodeGeneratePath);
            }

            Directory.CreateDirectory(CodeGeneratePath);

            Debug.Log("======> Load Export Config <======");

            FileInfo _ConfigFileInfo = new FileInfo(ConfigFilePath);

            if (_ConfigFileInfo == null || !_ConfigFileInfo.Exists)
            {
                Debug.LogError("[Export Data] Fail to read config file.");
                return;
            }

            Dictionary<string, Dictionary<string, string>> _ConfigSetting = LoadExportConfig();

            if (_ConfigSetting == null || _ConfigSetting.Count <= 0)
            {                
                return;
            }

            Debug.Log("======> Export Data <======");

            ExportData(_ConfigSetting);

            Debug.Log("======> Finish Excel Data Export <======");
        }

        protected static void ExportData(Dictionary<string, Dictionary<string, string>> ConfigSetting)
        {
            if (ConfigSetting == null)
            {
                return;
            }

            foreach (var _ConfigSettingIterator in ConfigSetting)
            {
                string _ExportDbName = _ConfigSettingIterator.Key;
                string _ExportDbPath = DbExportPath + "/" + _ExportDbName;

                SQLiteConnection _DbConnection = new SQLiteConnection(_ExportDbPath, DbPassword);

                if (_DbConnection == null)
                {
                    Debug.LogError("[Export Data] Cannot create " + _ExportDbName + ".");
                    continue;
                }

                Dictionary<string, string> _ExportSetting = _ConfigSettingIterator.Value;

                if (_ExportSetting == null)
                {
                    continue;
                }

                foreach (var _ExportSettingIterator in _ExportSetting)
                {
                    string _ExcelTableName = _ExportSettingIterator.Key;
                    string _ExcelFileName = _ExportSettingIterator.Value;

                    string _ExcelFilePath = ExcelFilePath + "/" + _ExcelFileName;

                    if (!File.Exists(_ExcelFilePath))
                    {
                        Debug.LogError("[Export Data] " + _ExcelFileName + " doesn't exist.");
                        continue;
                    }

                    FileStream _FileStream = null;

                    try
                    {
                        _FileStream = File.Open(_ExcelFilePath, FileMode.Open, FileAccess.Read);
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(ex.Message);
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    if (_FileStream == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    IExcelDataReader _Reader = ExcelReaderFactory.CreateReader(_FileStream);

                    if (_Reader == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    DataSet _DataSet = _Reader.AsDataSet();

                    if (_DataSet == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    DataTable _TableData = _DataSet.Tables[_ExcelTableName];

                    if (_TableData == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    int _Columns = _TableData.Columns.Count;
                    int _Rows = _TableData.Rows.Count;

                    if (_Rows < 2 || _Columns <= 0)
                    {
                        Debug.LogError("[Export Data] Rows are less than two or columns are less than one.");
                        continue;
                    }

                    ExcelTable _ExcelTableObject = new ExcelTable(_ExcelTableName, _TableData);


                }

                _DbConnection.Close();
                _DbConnection.Dispose();
                _DbConnection = null;
            }
        }

        protected static Dictionary<string, Dictionary<string, string>> LoadExportConfig()
        {
            string _ConfigContent = string.Empty;

            List<string> _ValidList = new List<string>();

            try
            {
                _ConfigContent = File.ReadAllText(ConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError("[Export Data] Fail to read config file.");
                return null;
            }

            JsonData _ConfigContentObject = JsonMapper.ToObject(_ConfigContent);

            if (_ConfigContentObject == null || !_ConfigContentObject.IsObject)
            {
                Debug.LogError("[Export Data] Fail to parse config file.");
                return null;
            }

            IDictionary _ConfigContentDict = _ConfigContentObject as IDictionary;

            if (_ConfigContentDict == null)
            {
                Debug.LogError("[Export Data] Fail to parse config file.");
                return null;
            }

            Dictionary<string, Dictionary<string, string>> _Ret = new Dictionary<string, Dictionary<string, string>>();

            foreach (string _ExportDbName in _ConfigContentDict.Keys)
            {
                if (string.IsNullOrEmpty(_ExportDbName))
                {
                    Debug.LogError("[Export Data] Export db name is null.");
                    return null;
                }

                if (!_Ret.ContainsKey(_ExportDbName))
                {
                    _Ret.Add(_ExportDbName, new Dictionary<string, string>());
                }

                JsonData _DbConfigObject = _ConfigContentObject[_ExportDbName];

                if (_DbConfigObject == null || !_DbConfigObject.IsObject)
                {
                    Debug.LogError("[Export Data] Fail to parse config file. [" + _ExportDbName + "]");
                    return null;
                }

                IDictionary _DbConfigDict = _DbConfigObject as IDictionary;

                if (_DbConfigDict == null)
                {
                    Debug.LogError("[Export Data] Fail to parse config file. [" + _ExportDbName + "]");
                    return null;
                }

                foreach (string _ExcelTableName in _DbConfigDict.Keys)
                {
                    if (string.IsNullOrEmpty(_ExcelTableName))
                    {
                        Debug.LogError("[Export Data] " + _ExportDbName + " refers to a null or empty excel table name.");
                        return null;
                    }

                    if (_ValidList.Contains(_ExcelTableName))
                    {
                        Debug.LogError("[Export Data] " + _ExportDbName + " - " + _ExcelTableName + " is already exported. Skipped.");
                        continue;
                    }

                    JsonData _ExcelFileNameObject = _DbConfigObject[_ExcelTableName];

                    if (_ExcelFileNameObject == null || !_ExcelFileNameObject.IsString)
                    {
                        Debug.LogError("[Export Data] Fail to parse config file. [" + _ExportDbName + " - " + _ExcelTableName + "]");
                        return null;
                    }

                    string _ExcelFileName = _ExcelFileNameObject.ToString();

                    if (string.IsNullOrEmpty(_ExcelFileName))
                    {
                        Debug.LogError("[Export Data] " + _ExportDbName + " - " + _ExcelTableName + " refers to a null or empty excel file name.");
                        return null;
                    }

                    _Ret[_ExportDbName].Add(_ExcelTableName, _ExcelFileName);
                }
            }

            return _Ret;
        } 

    }
}