namespace PureMVC.UGFramework.Core
{
    using System.IO;

    public class SystemUtility
    {
        public static DirectoryInfo CreateDirectory(string DirectoryPath)
        {
            if (string.IsNullOrEmpty(DirectoryPath))
            {
                return null;
            }

            DirectoryInfo _DirInfo = new DirectoryInfo(DirectoryPath);

            if (_DirInfo == null)
            {
                _DirInfo = Directory.CreateDirectory(DirectoryPath);
            }

            return _DirInfo;
        }

        public static void CopyFile(string SrcPath, string DestPath, bool bOverrite)
        {
            if (string.IsNullOrEmpty(SrcPath) || string.IsNullOrEmpty(DestPath))
            {
                return;
            }

            FileInfo _FileInfo = new FileInfo(SrcPath);

            if (_FileInfo == null || !_FileInfo.Exists || _FileInfo.Attributes != FileAttributes.Normal)
            {
                return;
            }

            File.Copy(SrcPath, DestPath, bOverrite);
        }

        public static void DeleteFile(string SrcPath)
        {
            if (string.IsNullOrEmpty(SrcPath))
            {
                return;
            }

            FileInfo _FileInfo = new FileInfo(SrcPath);

            if (_FileInfo == null || !_FileInfo.Exists || _FileInfo.Attributes != FileAttributes.Normal)
            {
                return;
            }

            _FileInfo.Delete();
        }

        public static void DeleteDirectory(string DirPath)
        {
            if (string.IsNullOrEmpty(DirPath))
            {
                return;
            }

            DirectoryInfo _DirInfo = new DirectoryInfo(DirPath);

            if (_DirInfo == null || !_DirInfo.Exists)
            {
                return;
            }

            foreach (var _FileInfo in _DirInfo.GetFiles())
            {
                if (_FileInfo != null)
                {
                    _FileInfo.Delete();
                }
            }

            foreach (var _SubDirInfo in _DirInfo.GetDirectories())
            {
                if (_SubDirInfo != null)
                {
                    DeleteDirectory(_SubDirInfo.FullName);
                }
            }

            _DirInfo.Delete();
        }

        public static bool IsFileExist(string FilePath)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return false;
            }

            FileInfo _FileInfo = new FileInfo(FilePath);

            if (_FileInfo == null || !_FileInfo.Exists)
            {
                return false;
            }

            return true;
        }
    }
}