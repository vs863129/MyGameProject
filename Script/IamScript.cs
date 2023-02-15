using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using System;

public class IamScript : MonoBehaviour
{
    public List<string> Lists = new List<string>();
    public List<string> Files = new List<string>();
    public static readonly string M_FOLDER = Application.dataPath + "/Mod";
    public static readonly string P_FOLDER = Application.dataPath + "/Pack";
    public static readonly string C_FOLDER = Application.dataPath + "/Console";
    [SerializeField] Button Button;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Got_EN_Date GotDate; //取得模組的資料並提取需要的內容
    public enum GotWhat
    {
        ItemName = 0,
        ContextMenu = 1,
    }
    public void Awake() //創建預設資料夾
    {
        Directory.CreateDirectory(M_FOLDER);
        Directory.CreateDirectory(P_FOLDER);
        Directory.CreateDirectory(C_FOLDER);
    }
    public void GotENDate(int what) //因變動較大,只在編輯器內用
    {
        switch (what)
        {
            case 0:
                GotDate.GotItemNameDate();
                break;
            case 1:
                GotDate.GotContextMenuDate();
                break;
        }

    }
    public void Exit() //關閉軟體用
    {
        Application.Quit();
    }
    #region 統合檔案用
    public void Action() //執行按鈕使用
    {
        Button.interactable = false;
        text.text = "Loading...";
        Invoke(nameof(Load),1); //1秒的延遲執行,使按鈕和文字顯示有時間切換
    }
    public void SearchFolder() //檢查CH資料夾內是否有其他資料夾.(少部分模組才會抓到資料夾)
    {
        List <DirectoryInfo> Found_Folders=new List<DirectoryInfo>();
        DirectoryInfo directoryInfo = new DirectoryInfo(M_FOLDER);
        DirectoryInfo[] MainFolders = directoryInfo.GetDirectories();
        foreach(DirectoryInfo Folder in MainFolders)
        {
            string Path = Folder.FullName + "/media" + "/lua" + "/shared" + "/Translate" + "/CH";
            DirectoryInfo AllFolders = new DirectoryInfo(Path);
            if(AllFolders.GetDirectories()!=null)
            {
                foreach (DirectoryInfo Found in AllFolders.GetDirectories())
                {
                    Found_Folders.Add(Found);
                }
            }
        }
        if(Found_Folders.Count!=0)
        {
            foreach (DirectoryInfo Folder in Found_Folders)
            {
                Debug.Log(Folder.FullName);
            }
        }
    }
    void Load() //開始統整
    {
        try
        {
            ClearFolderDate();
            DirectoryInfo directoryInfo = new DirectoryInfo(M_FOLDER);
            DirectoryInfo[] Folders = directoryInfo.GetDirectories();
            foreach (DirectoryInfo FolderInfo in Folders)
            {
                string Path = FolderInfo.FullName + "/media" + "/lua" + "/shared" + "/Translate" + "/CH";
                DirectoryInfo CH_Folder = new DirectoryInfo(Path);
                FileInfo[] Files = CH_Folder.GetFiles("*.txt");
                foreach (FileInfo fileInfo in Files)
                {
                    TextWriter writer = new StreamWriter(P_FOLDER + "/" + fileInfo.Name, true);
                    Write(fileInfo, writer);
                    writer.Close();
                }
            }
            WriterLastText();
            text.text = "Done";
        }
        catch(Exception Error) //如果出錯則打印資料並存在Console裡面,按鈕則會顯示ERROR且不可按
        {
            TextWriter writer = new StreamWriter(C_FOLDER+"/Log.txt", true);
            writer.WriteLine("[" + DateTime.Now + "]" + Error.Message);
            writer.Close();
            text.text = "ERROR";
            Button.interactable = true;
        }
    }
    bool FileDone(string FileName) //用於為新檔寫開頭的固定模板用(例如ItemName_CH = {),如果已寫過則跳過
    {

        foreach(string File in Files)
        {
            if(File==FileName)
            {
                return true;
            }
        }
        return false;
    }
    void Write(FileInfo file, TextWriter Writer)
    {
        if (!FileDone(file.Name))//確認檔案是否有被創建過
        {
            string[] date = File.ReadAllLines(file.FullName);
            if (file.Name != "Recorded_Media_CH.txt") //該檔案本身沒有類似( ItemName_CH = { )的模板,故然跳過
            {
                Writer.WriteLine(date[0]);
            }
            Files.Add(file.Name);
        }
        WriteText(file, Writer);
    }
    static void WriteText(FileInfo file,TextWriter Writer)//寫內文
    {
        int i = 0;
        while (i< w_date(file).Count)
        {
            Writer.WriteLine(w_date(file)[i]);
            i++;
        }
    }
    void WriterLastText() //寫收尾的 }
    {
        foreach (string FileName in Files)
        {
            TextWriter writer = new StreamWriter(P_FOLDER + "/" + FileName,true);
            if (FileName != "Recorded_Media_CH.txt")
            {
                writer.WriteLine("}");
            }
            writer.Close();
        }
    }
    static void ClearFolderDate() //清除空資料夾
    {
        DirectoryInfo Folder= new DirectoryInfo(P_FOLDER);
        FileInfo[] files = Folder.GetFiles();
        foreach(FileInfo file in files)
        {
            file.Delete();
        }
    }
    static List<string> w_date(FileInfo file) //專門寫內文的程式碼
    {
        string[] W_DATE = File.ReadAllLines(file.FullName);
        List<string> date = new List<string>();
        int i = 1; //通常每個檔案,第一行固定有重複性模板,且一個檔案只需要一個,故然跳過
        if(file.Name== "Recorded_Media_CH.txt") //通常都直接接內容,固然不跳過並全寫入
        {
            i = 0;
        }
        while (i < W_DATE.Length)
        {
            if (W_DATE[i].Contains('}'))
            {
                i++;
                continue;
            }
            if (W_DATE[i]=="")
            {
                i++;
                continue;
            }
            date.Add(W_DATE[i]);
            i++;
        }
        return date;
    }
    #endregion
}
[System.Serializable]
public class Got_EN_Date //在編輯器內使用,因未適用於大多數模組提取資料用,導致會經常變換,Build出來的時候,按鈕是鎖定的
{
    [SerializeField] string TESTDATE;
    public static readonly string D_FOLDER = Application.dataPath + "/AllDate";

    [SerializeField]List<string> All_DATE = new List<string>();

    [SerializeField] List<string> ALL_DATE = new List<string>();
    [SerializeField] List<string> ALL_DATENAME = new List<string>();
    public void GotContextMenuDate()
    {
        DirectoryInfo Path = new DirectoryInfo(D_FOLDER + "/ContextMenu");
        FileInfo[] files = Path.GetFiles("*.txt");
        List<string> GOTDATE = new List<string>();
        foreach(FileInfo file in files)
        {
            string[] _Date = File.ReadAllLines(file.FullName);
            foreach(string Check in _Date)
            {
                if(ContextMenuDate(Check) !=null)
                {
                    GOTDATE.Add(ContextMenuDate(Check));
                }
            }
        }
        WriteText(GOTDATE, "/ContextMenu/");
    }
    public void GotItemNameDate()
    {
        List<string> ALLDATE = new List<string>();
        List<string> DATENAME = new List<string>();
        DirectoryInfo Path = new DirectoryInfo(D_FOLDER + "/ItemName");
        FileInfo[] Files = Path.GetFiles("*.txt");
        foreach (FileInfo fileInfo in Files)
        {
            string[] W_DATE = File.ReadAllLines(fileInfo.FullName);
            foreach (string Check in W_DATE)
            {
                if (ItemNameDate(Check) != null)
                {
                    ALLDATE.Add(ItemNameDate(Check));
                }
                if(Date_ItemName(Check)!=null)
                {
                    DATENAME.Add(Date_ItemName(Check));
                }
            }
        }
        if (ALLDATE.Count == DATENAME.Count)
        {

            WriteText(ALLDATE, DATENAME);

        }
        else
        {
            ALL_DATE = ALLDATE;
            ALL_DATENAME = DATENAME;
            Debug.LogError("¨ú±oªºDATE¤£¥¿½T");
        }
    }
    string ContextMenuDate(string DATE)
    {
        if(DATE.Contains("ClothingItemExtraOption"))
        {
            int startIndex = DATE.IndexOf('=') + 2;
            int endIndex = DATE.LastIndexOf(',');
            int length = endIndex - startIndex;
            string date = DATE.Substring(startIndex, length);
            return "	ContextMenu_" + date + " = " + '"' + date + '"'+',';
        }
        return null;
    }
    string ItemNameDate(string DATE)
    {
        if(DATE.Contains("item")||DATE.Contains("item	"))
        {
            int found = 0;
            found = DATE.IndexOf("item");
            return "	ItemName_Base." + DATE.Substring(found + 4)+" = ";
        }
        return null;
    }
    string Date_ItemName(string DATE)
    {
        if (DATE.Contains("DisplayName"))
        {
            int startIndex = DATE.IndexOf('=')+1;
            int endIndex = DATE.LastIndexOf(',');
            int length = endIndex - startIndex;
            return '"'+DATE.Substring(startIndex, length)+'"'+",";
        }
        return null;
    }
    void WriteText(List<string> Date, List<string> Name)
    {
        TextWriter writer = new StreamWriter(D_FOLDER + "/Done/" +"/ItemName/"+ "ItemName_CH.txt", false);
        int i = 0;
        while(i<Date.Count)
        {
            writer.WriteLine(Date[i]+Name[i]);
            All_DATE.Add(Date[i] + Name[i]);
            i++;
        }
        writer.Close();
    }
    void WriteText(List<string> DATE,string Path)
    {
        TextWriter writer = new StreamWriter(D_FOLDER + "/Done/" + Path + "ContextMenu_CH.txt", false);
        foreach(string AllDate in DATE)
        {
            writer.WriteLine(AllDate);
            All_DATE.Add(AllDate);
        }
    }
}
