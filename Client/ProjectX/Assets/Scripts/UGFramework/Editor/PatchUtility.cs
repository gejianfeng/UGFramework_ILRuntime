namespace PureMVC.UGFramework.Editor
{
    using PureMVC.UGFramework.Core;
    using UnityEditor;
    using UnityEngine;

    public class PatchUtility
    {
        [MenuItem("UGFramework/Patch/Remove Patch Code")]
        public static void Patch_RemovePatchCode()
        {
            SystemUtility.DeleteDirectory(Application.dataPath + "/Patch");
        }

    }
}
