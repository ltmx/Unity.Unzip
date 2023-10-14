using System.Diagnostics;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// AssetPostProcessor to automatically extract any archive file inside Unity
/// </summary>
public class Unzip : AssetPostprocessor
{
    private static string sevenZipExeLocation => FindExePath("7za");

    /// <summary>
    /// An array of common extensions (zip, 7z, rar, ect...)
    /// </summary>
    private static readonly string[] commonExtensions =
    {
        ".zip", ".7z", ".rar", ".gz", ".bz2", ".tar", ".tgz",
    };

    #region Hidden Extensions

    //https://7-zip.opensource.jp/chm/general/formats.htm
    
    /// <summary>
    /// All supported file extensions
    /// <li><a href="https://7-zip.opensource.jp/chm/general/formats.htm">Supported File Extensions</a></li>
    /// </summary>
    private static readonly string[] supportedExtensions = 
    {
        ".7z", // 7Z
        ".bz2", ".bzip2", ".tbz2", ".tbz", // BZIP2
        ".gz", ".gzip", ".tgz", // GZIP
        ".tar", // TAR
        ".wim", ".swm", ".esd", // WIM
        ".xz", ".txz", // XZ
        ".zip", ".zipx", ".jar", ".xpi", ".odt", ".ods", ".docx", ".xlsx", ".epub", // ZIP
        ".apm", // APM
        ".ar", ".a", ".deb", ".lib", // AR
        ".arj", // ARJ
        ".cab", // CAB
        ".chm", ".chw", ".chi", ".chq", ".chw", // CHM
        ".msi", ".msp", ".doc", ".xls", ".ppt", // COMPOUND
        ".cpio", // CPIO
        ".cramfs", // CramFS
        ".dmg", // DMG
        ".ext", ".ext2", ".ext3", ".ext4", ".img", // Ext
        ".fat", ".img", // FAT
        ".hfs", ".hfsx", // HFS
        ".hxs", ".hxi", ".hxr", ".hxq", ".hxw", ".lit", // HXS
        ".ihex", // iHex
        ".iso", ".img", // ISO
        ".lzh", ".lha", // LZH
        ".lzma", // LZMA
        ".mbr", // MBR
        ".mslz", // MsLZ
        ".mub", // Mub
        ".nsis", // NSIS
        ".ntfs", ".img", // NTFS
        ".mbr", // MBR
        ".rar", ".r00", // RAR
        ".rpm", // RPM
        ".ppmd", // PPMD
        ".qcow", ".qcow2", ".qcow2c", // QCOW2
        ".001", ".002", // SPLIT
        ".squashfs", // SquashFS
        ".udf", ".iso", ".img", // UDF
        ".scap", // UEFIc
        ".uefif", // UEFIs
        ".vdi", // VDI
        ".vhd", // VHD
        ".vmdk", // VMDK
        ".xar", ".pkg", // XAR
        ".z", ".taz", // Z
    };

    #endregion

    
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedAssetPaths)
    {
        foreach (string asset in importedAssets) {
            if (commonExtensions.Contains(Path.GetExtension(asset))) {
                string fullPath = Path.GetFullPath(asset);
                string newPath = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(asset));

                CreateDirectoryIfVoid(newPath);
                
                string arguments = $"e \"{fullPath}\" -o\"{newPath}\" -y";

                Process process = new Process();
                process.StartInfo.FileName = sevenZipExeLocation;
                process.StartInfo.Arguments = arguments;
                process.Start();
                process.WaitForExit();


                // Original archived file deletion after extraction
                AssetDatabase.DeleteAsset(asset);
            }

            // File.Delete... caused this script to be deleted by unity
            // File.Delete($"{fullPath}.meta");
        }
        AssetDatabase.Refresh();
    }
    
    private static string FindExePath(string fileName)
    {
        string[] guids = AssetDatabase.FindAssets(fileName);
        var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
        var selected = paths.FirstOrDefault(x => Path.GetFileName(x) == fileName + ".exe");
        return Path.Combine(ApplicationRootPath, selected);
    }
    
    private static readonly string ApplicationRootPath = Application.dataPath.Remove(Application.dataPath.Length - 6);
    public static void CreateDirectoryIfVoid(string path)
    {
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
}