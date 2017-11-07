namespace PureMVC.UGFramework.Editor
{
    using LitJson;
    using PureMVC.UGFramework.Core;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public class BuildUtility
    {
        public static readonly string CsprojTemplatePath = Application.dataPath + "/../../Patch/Patch/Patch.csproj.tpl";
        public static readonly string CsprojPath = Application.dataPath + "/../../Patch/Patch/Patch.csproj";
        public static readonly string ConfigFilePath = Application.dataPath + "/../../Patch/Patch/PatchConfig.json";
        public static readonly string SrcPatchCodePath = Application.dataPath + "/Patch";
        public static readonly string DestPathCodePath = Application.dataPath + "/../../PatchSrc";
        public static readonly string DestPathCodeRelativePath = "..\\..\\PatchSrc";

        [MenuItem("UGFramework/Build/Gen .csproj Config")]
        public static void CreatePatchProjectConfig()
        {
            File.Delete(ConfigFilePath);

            JsonData _Content = new JsonData();
            _Content["UnityEngine"] = "[Relative path for UnityeEngine.dll]";
            _Content["UnityEngine.UI"] = "[Relative path for UnityEngine.UI.dll]";

            StringBuilder _Builder = new StringBuilder();

            JsonWriter _Writer = new JsonWriter(_Builder);
            _Writer.PrettyPrint = true;
            _Writer.IndentValue = 2;

            JsonMapper.ToJson(_Content, _Writer);

            File.WriteAllText(ConfigFilePath, _Builder.ToString());

            AssetDatabase.Refresh();
        }


        [MenuItem("UGFramework/Build/Prepare Build")]
        public static void PrepareBuild()
        {
            // Check and remove old csproj file
            FileInfo _CsprojFile = new FileInfo(CsprojPath);

            if (_CsprojFile != null && _CsprojFile.Exists)
            {
                _CsprojFile.Delete();
            }

            // Check Project Config Template file
            FileInfo _CsprojTemplate = new FileInfo(CsprojTemplatePath);

            if (_CsprojTemplate == null || !_CsprojTemplate.Exists)
            {
                Debug.LogError("Cannot find csproj template file.");
                return;
            }

            string _CsprojTemplateContent = string.Empty;

            try
            {
                _CsprojTemplateContent = File.ReadAllText(CsprojTemplatePath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError("Cannot read csproj template file.");
                return;
            }

            // Remove old patch src folder (for patch project)
            DirectoryInfo _DirInfo = new DirectoryInfo(DestPathCodePath);

            if (_DirInfo != null && !_DirInfo.Exists)
            {
                SystemUtility.DeleteDirectory(DestPathCodePath);
            }

            Directory.CreateDirectory(DestPathCodePath);

            // Copy patch files and generate file list
            List<string> _FileList = null;

            if (!CopyPatchSrcFile(out _FileList))
            {
                Debug.LogError("Fail to copy patch src file.");
                return;
            }
            if (_FileList.Count == 0)
            {
                Debug.LogWarning("No patch src file copied.");
                return;
            }

            // Read Project Config Setting
            Dictionary<string, string> _ProjectSetting = null;
            if (!LoadProjectSetting(out _ProjectSetting))
            {
                Debug.LogError("Fail to load project setting.");
                return;
            }

            // Genreate csproj file
            if (!GenerateProjectFile(_CsprojTemplateContent, _ProjectSetting, _FileList))
            {
                Debug.LogError("Fail to generate csproj file.");
                return;
            }

            // Rename .cs src file
            RenameSrcFiles(SrcPatchCodePath, ".cs", ".cs_");
            AssetDatabase.Refresh();
        }

        [MenuItem("UGFramework/Build/Post Build")]
        public static void PostBuild()
        {
            RenameSrcFiles(SrcPatchCodePath, ".cs_", ".cs");
            AssetDatabase.Refresh();
        }

        protected static void RenameSrcFiles(string SrcFilePath, string SrcExt, string DestExt)
        {
            if (string.IsNullOrEmpty(SrcFilePath))
            {
                return;
            }

            DirectoryInfo _DirInfo = new DirectoryInfo(SrcFilePath);

            if (_DirInfo == null || !_DirInfo.Exists)
            {
                return;
            }

            foreach (var _FileInfo in _DirInfo.GetFiles())
            {
                if (_FileInfo.Extension == SrcExt)
                {
                    string _DestPath = _FileInfo.FullName.Replace(SrcExt, DestExt);
                    _FileInfo.MoveTo(_DestPath);
                }
            }

            foreach (var _SubDirInfo in _DirInfo.GetDirectories())
            {
                RenameSrcFiles(_SubDirInfo.FullName, SrcExt, DestExt);
            }
        }

        protected static bool CopyPatchSrcFile(out List<string> RetFileList)
        {
            RetFileList = new List<string>();

            DirectoryInfo _DirInfo = new DirectoryInfo(SrcPatchCodePath);

            if (_DirInfo == null || !_DirInfo.Exists)
            {
                return false;
            }

            return CopyPatchSrcFileInternal(ref RetFileList, SrcPatchCodePath, DestPathCodePath, "");
        }

        protected static bool CopyPatchSrcFileInternal(ref List<string> FileList, string SrcFullPath, string DestFullPath, string DestRelativePath)
        {
            if (FileList == null || string.IsNullOrEmpty(SrcFullPath) || string.IsNullOrEmpty(DestFullPath))
            {
                return false;
            }

            DirectoryInfo _DirInfo = new DirectoryInfo(SrcFullPath);

            if (_DirInfo == null || !_DirInfo.Exists)
            {
                return false;
            }

            DirectoryInfo _DestDirInfo = new DirectoryInfo(DestFullPath);

            if (_DestDirInfo == null || !_DestDirInfo.Exists)
            {
                Directory.CreateDirectory(DestFullPath);
            }

            foreach (var _FileInfo in _DirInfo.GetFiles())
            {
                if (_FileInfo.Extension == ".cs")
                {
                    SystemUtility.CopyFile(SrcFullPath + "/" + _FileInfo.Name, DestFullPath + "/" + _FileInfo.Name, true);
                    string _RelativePath = DestRelativePath + "/" + _FileInfo.Name;
                    FileList.Add(_RelativePath);
                }
            }

            foreach (var _SubDirInfo in _DirInfo.GetDirectories())
            {
                CopyPatchSrcFileInternal(ref FileList, SrcFullPath + "/" + _SubDirInfo.Name, DestFullPath + "/" + _SubDirInfo.Name, DestRelativePath + "/" + _SubDirInfo.Name);
            }

            return true;
        }

        protected static bool GenerateProjectFile(string Template, Dictionary<string, string> ProjectConfig, List<string> FileList)
        {
            if (string.IsNullOrEmpty(Template) || ProjectConfig == null || FileList == null)
            {
                return false;
            }

            string _ConfigContent = string.Empty;

            foreach (var Iterator in ProjectConfig)
            {
                string _Key = Iterator.Key;
                string _Value = Iterator.Value;

                _ConfigContent += "\t\t<Reference Include=\"" + _Key +"\">\n";
                _ConfigContent += "\t\t\t<HintPath>" + _Value + "</HintPath>\n";
                _ConfigContent += "\t\t</Reference>\n";
            }

            string _FileContent = string.Empty;

            foreach (var _RelativePath in FileList)
            {
                string _tmp = _RelativePath.Replace('/', '\\');
                _FileContent += "\t\t<Compile Include=\"" + DestPathCodeRelativePath + _tmp + "\" />";
            }
            string _ProjectConfigFileContent = Template.Replace("{0}", _ConfigContent);            
            _ProjectConfigFileContent = _ProjectConfigFileContent.Replace("{1}", _FileContent);

            try
            {
                File.WriteAllText(CsprojPath, _ProjectConfigFileContent);
            }
            catch(Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
            
            return true;
        }

        protected static bool LoadProjectSetting(out Dictionary<string, string> ProjectSetting)
        {
            ProjectSetting = new Dictionary<string, string>();

            FileInfo _ProjectSettingFile = new FileInfo(ConfigFilePath);

            if (_ProjectSettingFile == null || !_ProjectSettingFile.Exists)
            {
                return false;
            }

            string _ConfigContent = string.Empty;

            try
            {
                _ConfigContent = File.ReadAllText(ConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }

            JsonData _ConfigContentObject = JsonMapper.ToObject(_ConfigContent);

            if (_ConfigContentObject == null || !_ConfigContentObject.IsObject)
            {
                return false;
            }

            IDictionary _ConfigContentDict = _ConfigContentObject as IDictionary;

            if (_ConfigContentDict == null)
            {
                return false;
            }

            foreach (string _Key in _ConfigContentDict.Keys)
            {
                if (string.IsNullOrEmpty(_Key))
                {
                    return false;
                }

                string _Value = _ConfigContentObject[_Key].ToString();

                if (string.IsNullOrEmpty(_Value))
                {
                    return false;
                }

                if (ProjectSetting.ContainsKey(_Key))
                {
                    Debug.LogWarning("Duplicate key [" + _Key + "] found. Ignored!");
                    continue;
                }

                ProjectSetting.Add(_Key, _Value);
            }

            return true;
        }
    }
}
