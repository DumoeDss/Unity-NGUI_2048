﻿using UnityEngine;
using System.Collections;
using System.IO;
using Mono.Data.Sqlite;

public class GameController : MonoBehaviour {

#if UNITY_EDITOR || !(UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
    private void GameMove()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                backMap = (int[,])iMap.Clone();
                SwipeRight();
                DrawMap(Theme);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                backMap = (int[,])iMap.Clone();
                SwipeLeft();
                DrawMap(Theme);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                backMap = (int[,])iMap.Clone();
                SwipeUp();
                DrawMap(Theme);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                backMap = (int[,])iMap.Clone();
                SwipeDown();
                DrawMap(Theme);
            }           
        }
    }
#elif UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
    private Vector2 firstPosition = Vector2.zero, lastPosition = Vector2.zero, offSet = Vector2.zero;
    private void GameMove()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                firstPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                lastPosition = touch.position;
                offSet = lastPosition - firstPosition;
                //if (offSet.x > offSet.y)
                if (Mathf.Abs(offSet.x) - Mathf.Abs(offSet.y) > 3)
                {
                    if (offSet.x >= 5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeRight();
                    }
                    else if (offSet.x <= -5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeLeft();

                    }
                }
                else if (Mathf.Abs(offSet.y) - Mathf.Abs(offSet.x) > 3)
                {
                    if (offSet.y >= 5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeUp();
                    }
                    else if (offSet.y <= -5.0f)
                    {
                        backMap = (int[,])iMap.Clone();
                        SwipeDown();
                    }
                }
                DrawMap(Theme);
            }
        }
    }
   
#endif

    public const int Cloume=4;    
    private int[,] iMap = new int[Cloume, Cloume];
    private int[,] backMap;
    public static int[,] saveMap = new int[Cloume, Cloume]; 
    private bool isMove = false;
    private static string Theme="Madoka";
    private static string BackGround = "默认";
    private int Score = 0;
    private int BestScore = 0;
    private bool isNewBestScore = false;
    private static string path;
    private string appDBPath;

    //游戏元素
    private UISprite[,] GameSprites = new UISprite[Cloume, Cloume];
    //游戏退出面板，游戏结束面板，游戏设置面板
    private UISprite TestSprite;
    //标题样式
    private UISprite TitleSprite;
    private UIWidget BGSprite;
    private UISprite SaveGamePanel;
    private UISprite SetGamePanel;
    private UISprite QuitGamePanel;
    private UISprite GameOverPanel;
    private UISprite GameMenuWidget;
    private UISprite StartGameWidget;
    private UIPopupList ThemeList;
    private UIPopupList BGList;

    private UILabel MainScoreLabel;
    private UILabel MainBestScoreLabel;
    private UILabel GameOverScoreLabel;
    private UILabel RandomLabel;
    private UILabel GameDataBtnName1;
    private UILabel GameDataBtnName2;
    private UILabel GameDataBtnName3;
    private UILabel SaveOrLoadLabel;
    private UILabel TestLabel;
    private UILabel[] GameDataBtnName = new UILabel[4];
    private UIButton QuitGameBtn;
    private UIButton StartGameBtn;
    private UIButton LoadGameBtn;
    private UIButton SetGameBtn;
    private UIButton ReturnSettingBtn;
    private UIButton ReturnMenuBtn;
    private UIButton BackUpBtn;
    private UIButton SaveGameBtn;
    private UIButton ReStartBtn;



    private string GameSettingDbName = "GameSetting";
    private string GameSaveDataDbName = "GameSaveData";
    private DbAccess db;
    private string dbName;

    void Start()
    {
        //TestSprite = this.transform.parent.Find("Test").GetComponent<UISprite>();
        //TestLabel = this.transform.parent.Find("Test/Label").GetComponent<UILabel>();

        appDBPath = Application.persistentDataPath + "/" + "Madoka.db";
        if (!File.Exists(appDBPath))
        {
            //用www先从Unity中下载到数据库  
            WWW loadDB = new WWW("jar:file://" + Application.dataPath + "!/assets/" + "Madoka.db");
            bool boo = true;
            while (boo)
            {
                if (loadDB.isDone)
                {
                    //拷贝至规定的地方  
                    File.WriteAllBytes(appDBPath, loadDB.bytes);
                    boo = false;
                }
            }

        }
        dbName = "URI=file:" + appDBPath;
#if UNITY_EDITOR
        dbName = "data source = Madoka.db";
#endif

        //try
        //{
        db = new DbAccess(dbName);
        IsSettingTableExist(db);
        IsGameSaveTableExist(db);
        db.DisConnectDb();

        path = Application.persistentDataPath;

        LoadSetting();
        InitGameSprite();


        //}
        //catch (System.Exception e)
        //{
        //    TestSprite.gameObject.SetActive(true);
        //    TestLabel.text = e.ToString();
        //}
    }
    
    private void IsSettingTableExist(DbAccess db)
    {
        if (!db.IsTableExist(GameSettingDbName))
        {
#if UNITY_EDITOR
            print("表不存在");
#endif
            db.CreateTable(GameSettingDbName, new string[] { "BestScore", "Background", "Theme" }, new string[] { "varchar(20)", "varchar(20)", "varchar(20)" });

            ChangeSetting();
        }
    }

    private void IsGameSaveTableExist(DbAccess db)
    {
        if (!db.IsTableExist(GameSaveDataDbName))
        {
#if UNITY_EDITOR
            print("表不存在");
#endif
            db.CreateTable(GameSaveDataDbName, new string[] {"TableIndex", "GameMap", "Score" }, new string[] { "varchar(2)","varchar(20)", "varchar(20)" });
            //SaveGame("2,0,0,0;0,2,0,0;0,0,0,0;0,0,0,0", "0");
        }
    }


    #region Update()
    //对玩家操作做出反应
    void Update()
    {
        GameMove();


        if ((Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Escape)) && StartGameWidget.gameObject.activeSelf == true) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (StartGameWidget.gameObject.activeSelf == true)
            {
                //&& 
                if(QuitGamePanel.gameObject.activeSelf == false&&GameOverPanel.gameObject.activeSelf == false&& SaveGamePanel.gameObject.activeSelf == false&& SetGamePanel.gameObject.activeSelf == false)
                {
                    //ReturnMenuBtnClick();
                    SetBtnState(false);
                    QuitGamePanel.gameObject.SetActive(true);
                    
                }
                else if (QuitGamePanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    QuitGamePanel.gameObject.SetActive(false);
                }
                else if (GameOverPanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    GameOverPanel.gameObject.SetActive(false);
                }
                else if (SaveGamePanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    SaveGamePanel.gameObject.SetActive(false);
                }
                else if (SetGamePanel.gameObject.activeSelf == true)
                {
                    SetBtnState(true);
                    SetGamePanel.gameObject.SetActive(false);
                }
                
            }
                //QuitPannel
            
            if (SaveGamePanel.gameObject.activeSelf == true)
            {
                SetBtnState(true);
                SaveGamePanel.gameObject.SetActive(false);
            }
            if (SetGamePanel.gameObject.activeSelf == true)
            {
                SetBtnState(true);
                SetGamePanel.gameObject.SetActive(false);
            }
        }
    }
    #endregion


    private void SelectTable(string table_name,out string[] strData)
    {
        DbAccess db = new DbAccess(dbName);
        SqliteDataReader SqliteReader = db.SelectTable(table_name);
        object[] objData = new object[SqliteReader.FieldCount];
        
        int fieldCount = SqliteReader.GetValues(objData);
        strData=new string[fieldCount];
        for (int i = 0; i < fieldCount; i++) {
            strData[i] = objData[i].ToString();
#if UNITY_EDITOR
            print(strData[i]);
#endif
        }
        db.DisConnectDb();
    }
    private void SelectTable(string table_name,string item, out string[] strData)
    {
        DbAccess db = new DbAccess(dbName);
        SqliteDataReader SqliteReader = db.SelectTable(table_name);
        strData = new string[4];
        if (SqliteReader.HasRows)
        {           
            while (SqliteReader.Read())
            {
                object[] objData = new object[SqliteReader.FieldCount];

                int fieldCount = SqliteReader.GetValues(objData);
                
                
                strData[int.Parse(objData[0].ToString())] = objData[2].ToString();
                
#if UNITY_EDITOR
                print(strData[int.Parse(objData[0].ToString())]);
#endif
                
            }
           
        }
        db.DisConnectDb();
    }
    private void SelectTable(string table_name,int keyvalue,out string[] strData,string keyfiled="TableIndex")
    {
        DbAccess db = new DbAccess(dbName);
        SqliteDataReader SqliteReader = db.SelectTable(table_name,keyfiled,keyvalue);
        object[] objData = new object[SqliteReader.FieldCount];

        int fieldCount = SqliteReader.GetValues(objData);
        strData = new string[fieldCount];
        for (int i = 0; i < fieldCount; i++)
        {
            strData[i] = objData[i].ToString();
#if UNITY_EDITOR
            print(strData[i]);
#endif

        }
        db.DisConnectDb();
    }

    private void InsertTable(string table_name,string[] strFiled, string[] strValue)
    {
        DbAccess db = new DbAccess(dbName);
        db.InsertTable(table_name, strFiled, strValue);
        db.DisConnectDb();
    }

    private void UpdateTable(string table_name, string strFiled, string strValue)
    {
        DbAccess db = new DbAccess(dbName);
        db.UpdateTable(table_name, strFiled, strValue);
        db.DisConnectDb();
    }
    private void UpdateTable(string table_name, string[] strFiled, string[] strValue, string strKeyFiled,string strKeyValue )
    {
        DbAccess db = new DbAccess(dbName);
        db.UpdateTable(table_name, strFiled, strValue, strKeyFiled, strKeyValue);
        db.DisConnectDb();
    }

    //读取游戏设置
    private void LoadSetting()
    {
        string[] strSetting;
        SelectTable(GameSettingDbName, out strSetting);       
        BestScore = int.Parse(strSetting[0]);       
        BackGround = strSetting[1];     
        Theme = strSetting[2];
    }

    private void LoadGame(int DataIndex)
    {
        string[] strData;
        SelectTable(GameSaveDataDbName, DataIndex, out strData);
        SplitStrMap(strData[1],out iMap);
        Score = int.Parse(strData[2]);
    }

    System.StringSplitOptions ssop = System.StringSplitOptions.RemoveEmptyEntries;
    private void SplitStrMap(string strMap,out int[,] a)
    {
        a = new int[Cloume,Cloume];
        string[] MapOrignX = strMap.Split(new char[] { ';' }, ssop);
        for (int i = 0; i < MapOrignX.Length; ++i)
        {
            string[] MapOrignY = MapOrignX[i].Split(new char[] { ',' }, ssop);
            for (int j = 0; j < MapOrignY.Length; ++j)
            {

                a[i, j] = int.Parse(MapOrignY[j]);
            }
        }
    }
    private void MapToString(int[,] intMap,out string outStrMap)
    {
        outStrMap = "";
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for(int iy = 0; iy < Cloume; ++iy)
            {
                outStrMap = outStrMap.Trim() + intMap[ix, iy];
                if (iy != Cloume - 1)
                    outStrMap += ",";
            }
            if (ix != Cloume - 1)
                outStrMap += ";";
        }
    }

    private void ChangeSetting()
    {
        string[] strFileds = new string[] { "BestScore", "Background", "Theme"};
        string[] strValues = new string[] { "0","默认","Madoka" };
        InsertTable(GameSettingDbName, strFileds, strValues);
    }

    public void ChangeSetting(string strFiled, string strValue)
    {
        UpdateTable(GameSettingDbName, strFiled, strValue);       
    }

    //保存游戏在空栏位
    private void NewSaveGame(string strGameMap, string strScore, int SaveIndex)
    {
        string[] strFileds = new string[] { "TableIndex","GameMap", "Score"};
        string[] strValues = new string[] { SaveIndex.ToString(), strGameMap, strScore };       
        InsertTable(GameSaveDataDbName, strFileds, strValues);
    }

    //保存游戏到已有栏位
    private void SaveGame(string strGameMap,string strScore,int SaveIndex)
    {
        string[] strFiled = new string[] { "TableIndex", "GameMap", "Score" };
        string[] strValue = new string[] { SaveIndex.ToString(), strGameMap, strScore };        
        UpdateTable(GameSaveDataDbName, strFiled, strValue, "TableIndex", SaveIndex.ToString());
    }

    public void CloseSaveGamePannelBtnClick()
    {
        SetBtnState(true);
        SaveGamePanel.gameObject.SetActive(false);
    }

    private void SetBtnState(bool isTrue)
    {
        StartGameBtn.isEnabled = isTrue;
        LoadGameBtn.isEnabled = isTrue;
        SetGameBtn.isEnabled = isTrue;
        QuitGameBtn.isEnabled = isTrue;
        ReturnSettingBtn.isEnabled = isTrue;
        ReturnMenuBtn.isEnabled = isTrue;
        BackUpBtn.isEnabled = isTrue;
        SaveGameBtn.isEnabled = isTrue;
        ReStartBtn.isEnabled = isTrue;
    }

    private bool isSaveOrLoad=true;

    


    private void InitIMap()
    {
        Score = 0;
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for(int iy = 0; iy < Cloume; ++iy)
            {
                iMap[ix, iy] = 0;
            }
        }
    }
    private void InitGameSprite()
    {
        GameMenuWidget = this.transform.parent.Find("GameMenu").GetComponent<UISprite>();
        StartGameWidget = this.transform.parent.Find("StartGame").GetComponent<UISprite>();
        StartGameWidget.spriteName = BackGround;
        SetGamePanel = this.transform.parent.Find("SetGamePanel").GetComponent<UISprite>();
        SaveGamePanel = this.transform.parent.Find("SaveGamePanel").GetComponent<UISprite>();
        QuitGamePanel = this.transform.parent.Find("QuitGamePanel").GetComponent<UISprite>();
        GameOverPanel = this.transform.parent.Find("GameOverPanel").GetComponent<UISprite>();
        QuitGameBtn = this.transform.parent.Find("GameMenu/QuitGameBtn").GetComponent<UIButton>();
        ReturnSettingBtn = this.transform.parent.Find("StartGame/ReturnSettingBtn").GetComponent<UIButton>();
        StartGameBtn = this.transform.parent.Find("GameMenu/StartGameBtn").GetComponent<UIButton>();
        LoadGameBtn = this.transform.parent.Find("GameMenu/LoadGameBtn").GetComponent<UIButton>();
        SetGameBtn = this.transform.parent.Find("GameMenu/SetGameBtn").GetComponent<UIButton>();
        ReturnMenuBtn = this.transform.parent.Find("StartGame/ReturnMenuBtn").GetComponent<UIButton>();
        BackUpBtn = this.transform.parent.Find("StartGame/BackUpBtn").GetComponent<UIButton>();
        SaveGameBtn = this.transform.parent.Find("StartGame/SaveGameBtn").GetComponent<UIButton>();
        ReStartBtn = this.transform.parent.Find("StartGame/ReStartBtn").GetComponent<UIButton>();

        ThemeList= this.transform.parent.Find("SetGamePanel/ThemeList").GetComponent<UIPopupList>();
        BGList = this.transform.parent.Find("SetGamePanel/BGList").GetComponent<UIPopupList>();
        TitleSprite = this.transform.parent.Find("StartGame/TitleSprite").GetComponent<UISprite>();
        MainScoreLabel = this.transform.parent.Find("StartGame/ScoreBg/Score").GetComponent<UILabel>();
        MainBestScoreLabel = this.transform.parent.Find("StartGame/BestScoreBg/BestScore").GetComponent<UILabel>();
        GameOverScoreLabel = this.transform.parent.Find("GameOverPanel/ScoreLabel").GetComponent<UILabel>();
        RandomLabel = this.transform.parent.Find("GameOverPanel/RandomLabel").GetComponent<UILabel>();
        SaveOrLoadLabel = this.transform.parent.Find("SaveGamePanel/Label").GetComponent<UILabel>();
        GameDataBtnName1 = this.transform.parent.Find("SaveGamePanel/GameDataBtn1/Label").GetComponent<UILabel>();
        GameDataBtnName2 = this.transform.parent.Find("SaveGamePanel/GameDataBtn2/Label").GetComponent<UILabel>();
        GameDataBtnName3 = this.transform.parent.Find("SaveGamePanel/GameDataBtn3/Label").GetComponent<UILabel>();
        GameDataBtnName[1] = GameDataBtnName1;
        GameDataBtnName[2] = GameDataBtnName2;
        GameDataBtnName[3] = GameDataBtnName3;

        for (int ix = 0; ix < Cloume; ++ix)
        {
            for (int iy = 0; iy < Cloume; ++iy)
            {
                GameSprites[ix, iy] = this.transform.parent.Find("StartGame/GameMap/Sprite" + ix + iy).GetComponent<UISprite>();

            }
        }
        #region 展开数组初始化
        //GameSprites[0, 0] = this.transform.Find("Sprite00").GetComponent<UISprite>();
        //GameSprites[0, 1] = this.transform.Find("Sprite01").GetComponent<UISprite>();
        //GameSprites[0, 2] = this.transform.Find("Sprite02").GetComponent<UISprite>();
        //GameSprites[0, 3] = this.transform.Find("Sprite03").GetComponent<UISprite>();
        //GameSprites[1, 0] = this.transform.Find("Sprite10").GetComponent<UISprite>();
        //GameSprites[1, 1] = this.transform.Find("Sprite11").GetComponent<UISprite>();
        //GameSprites[1, 2] = this.transform.Find("Sprite12").GetComponent<UISprite>();
        //GameSprites[1, 3] = this.transform.Find("Sprite13").GetComponent<UISprite>();
        //GameSprites[2, 0] = this.transform.Find("Sprite20").GetComponent<UISprite>();
        //GameSprites[2, 1] = this.transform.Find("Sprite21").GetComponent<UISprite>();
        //GameSprites[2, 2] = this.transform.Find("Sprite22").GetComponent<UISprite>();
        //GameSprites[2, 3] = this.transform.Find("Sprite23").GetComponent<UISprite>();
        //GameSprites[3, 0] = this.transform.Find("Sprite30").GetComponent<UISprite>();
        //GameSprites[3, 1] = this.transform.Find("Sprite31").GetComponent<UISprite>();
        //GameSprites[3, 2] = this.transform.Find("Sprite32").GetComponent<UISprite>();
        //GameSprites[3, 3] = this.transform.Find("Sprite33").GetComponent<UISprite>();
        #endregion
    }

    public void StartGameBtnClick()
    {
        InitIMap();
        AddSprite();
        AddSprite();
        DrawMap(Theme);
        MainBestScoreLabel.text = BestScore.ToString();
        GameMenuWidget.gameObject.SetActive(false);

        StartGameWidget.gameObject.SetActive(true);
    }

    private bool[] isGameDataNull = new bool[4];
    private void ShowGameData(UILabel[] GameDataBtnName)
    {
        string[] ScoreCount;
        SelectTable(GameSaveDataDbName, "TableIndex", out ScoreCount);
        for (int i = 1; i < ScoreCount.Length; ++i)
        {
            if (ScoreCount[i] != null)
            {
                GameDataBtnName[i].text = ScoreCount[i];
                isGameDataNull[i] = false;
            }
            else
            {
                GameDataBtnName[i].text = "无记录";
                isGameDataNull[i] = true;
            }
               
        }
    }

    
   
    private void InitGameDataBtn()
    {
        GameDataBtnName[1] = GameDataBtnName1;
        GameDataBtnName[2] = GameDataBtnName2;
        GameDataBtnName[3] = GameDataBtnName3;
    }
    public void LoadGameBtnClick()
    {
        isSaveOrLoad = false;
        SaveOrLoadLabel.text = "读取游戏";        
        ShowGameData(GameDataBtnName);
        SetBtnState(false);

        SaveGamePanel.gameObject.SetActive(true);
    }
    public void SaveGameBtnClick()
    {
        //if (QuitGamePanel.gameObject.activeSelf == true)
        //{
        //    QuitGamePanel.gameObject.SetActive(false);
        //}
        isSaveOrLoad = true;
        SaveOrLoadLabel.text = "保存游戏";       
        ShowGameData(GameDataBtnName);
        SetBtnState(false);
        SaveGamePanel.gameObject.SetActive(true);
    }
    public void GameDataBtn1Click()
    {
        GameDataSaveLoad(1, isSaveOrLoad, isGameDataNull[1]);
    }

    public void GameDataBtn2Click()
    {
        GameDataSaveLoad(2, isSaveOrLoad, isGameDataNull[2]);
    }

    public void GameDataBtn3Click()
    {
        GameDataSaveLoad(3, isSaveOrLoad, isGameDataNull[3]);
    }
    private void GameDataSaveLoad(int DataIndex, bool isSave, bool isDataNull)
    {
        if (isSave)
        {
            string strMap = "";

            MapToString(iMap, out strMap);

            if (isDataNull)
            {
                NewSaveGame(strMap, Score.ToString(), DataIndex);

            }
            else
            {
                SaveGame(strMap, Score.ToString(), DataIndex);

            }
            ShowGameData(GameDataBtnName);
            SetBtnState(true);
            SaveGamePanel.gameObject.SetActive(false);
        }
        else
        {
            if (isDataNull)
                return;
            else
            {
                InitGameSprite();
                LoadGame(DataIndex);
                DrawMap(Theme);
                MainScoreLabel.text = Score.ToString();
                MainBestScoreLabel.text = BestScore.ToString();
                SaveGamePanel.gameObject.SetActive(false);
                GameMenuWidget.gameObject.SetActive(false);
                SetBtnState(true);
                StartGameWidget.gameObject.SetActive(true);
            }

        }
    }

    public void SetGameBtnClick()
    {
        SetBtnState(false);
        SetGamePanel.gameObject.SetActive(true);
    }
    
    public void ChangeSetBtnClick()
    {
        //print(ThemeList.value.Trim());
        string strTheme="";
        string strBackground="";

        strBackground = BGList.value.Trim();
        switch (ThemeList.value.Trim())
        {
            case "魔法少女小圆":
                strTheme = "Madoka";
                break;
            case "小圆2":
                strTheme = "Homura";
                break;
            case "LoveLive":
                strTheme = "LoveLive";
                break;
            case "偶像大师":
                strTheme = "偶像大师";
                break;
        }
        StartGameWidget.spriteName = strBackground;
        ChangeSetting("Background", strBackground);
        if (strTheme != Theme)
        {
            Theme = strTheme;
            ChangeSetting("Theme", strTheme);
            if(StartGameWidget.gameObject.activeSelf==true)
                DrawMap(Theme);

        }
        SetBtnState(true);
        SetGamePanel.gameObject.SetActive(false);
        //ChangeSetting("Theme", strTheme);
        //ChangeSetting("BackGround", strBackground);


    }
    public void CloseSetGamePanelBtnClick()
    {
        SetBtnState(true);
        SetGamePanel.gameObject.SetActive(false);
    }
   
    public void ReturnMenuBtnClick()
    {
        SetBtnState(true);
        InitIMap();
        if (isNewBestScore)
        {
            ChangeSetting("BestScore", BestScore.ToString());
            isNewBestScore = false;
        }
        StartGameWidget.gameObject.SetActive(false);
        GameMenuWidget.gameObject.SetActive(true);        
    }

    public void ReturnGameBtnClick()
    {
        SetBtnState(true);
        QuitGamePanel.gameObject.SetActive(false);
    }

    public void ChangeTheme(string strTheme)
    {
        Theme = strTheme;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //向地图中随机添加一个元素
    private void AddSprite()
    {
        int rx, ry;
        rx = Random.Range(0, 4);
        ry = Random.Range(0, 4);
        if (iMap[rx, ry] == 0)
        {
            int ri = Random.Range(1, 101);
            iMap[rx, ry] =(ri > 90) ?  4 :  2;
        }
        else AddSprite();
        GameOver();
    }

    //检测游戏是否结束
    private bool isGameOver()
    {
        for (int ix = 0; ix < Cloume; ++ix)
        {
            for (int iy = 0; iy < Cloume; ++iy)
            {
                //1.如果还有位置为0，则游戏未结束。
                //2.如果所有的元素都不为0，判断是否有元素与上下左右相同，如果木有则游戏结束。
                if (iMap[ix, iy] == 0 || (ix > 0 && iMap[ix, iy] == iMap[ix - 1, iy]) || (ix < 3 && iMap[ix, iy] == iMap[ix + 1, iy])
                    || (iy > 0 && iMap[ix, iy] == iMap[ix, iy - 1]) || (iy < 3 && iMap[ix, iy] == iMap[ix, iy + 1]))
                {
                    return false;
                }

            }
        }
        return true;
    }

    //显示游戏结束面板
    private void GameOver()
    {
        if (isGameOver())
        {
            GameOverScoreLabel.text = Score+"";
            RandomLabel.text = "离最高分还差" + (BestScore - Score) + "分~";
            if (isNewBestScore)
            {
                //新记录
                RandomLabel.text = "新纪录~";
                ChangeSetting("BestScore", BestScore.ToString());
            }
            SetBtnState(false);
            GameOverPanel.gameObject.SetActive(true);
        }
    }

    //重新开始
    public void GameReStart()
    {
        
        if (isNewBestScore)
        {
            isNewBestScore = false;
            ChangeSetting("BestScore", BestScore.ToString());
        }
        for(int ix = 0; ix < Cloume; ++ix)
        {
            for(int iy = 0; iy < Cloume; ++iy)
            {
                iMap[ix, iy] = 0;
            }
        }
        Score = 0;
        MainScoreLabel.text = Score.ToString();
        AddSprite();
        AddSprite();
        DrawMap(Theme);
        if (GameOverPanel.gameObject.activeSelf == true)
        {
            GameOverPanel.gameObject.SetActive(false);
            SetBtnState(true);
        }
    }

    //后退一步
    public void backUp()
    {
        try
        {
            if (isMove)
            {
                iMap = (int[,])backMap.Clone();
                DrawMap(Theme);
                isMove = false;
            }
        }
        catch (System.Exception)
        {
            return;
        }
    }

    #region 绘制地图
    private void DrawMap(string theme)
    {
        for(int ix = 0; ix < Cloume; ++ix)
        {
            for(int iy = 0; iy < Cloume; ++iy)
            {
                if (iMap[ix, iy] == 0)
                {
                    GameSprites[ix, iy].alpha = 0.4f;
                }
                else GameSprites[ix, iy].alpha = 0.9f;
                switch (iMap[ix, iy])
                {
                    case 0:                      
                        GameSprites[ix, iy].spriteName = "0";
                        break;
                    case 2:
                        GameSprites[ix, iy].spriteName = theme + "2";
                        break;
                    case 4:
                        GameSprites[ix, iy].spriteName = theme + "4";
                        break;
                    case 8:
                        GameSprites[ix, iy].spriteName = theme + "8";
                        break;
                    case 16:
                        GameSprites[ix, iy].spriteName = theme + "16";
                        break;
                    case 32:
                        GameSprites[ix, iy].spriteName = theme + "32";
                        break;
                    case 64:
                        GameSprites[ix, iy].spriteName = theme + "64";
                        break;
                    case 128:
                        GameSprites[ix, iy].spriteName = theme + "128";
                        break;
                    case 256:
                        GameSprites[ix, iy].spriteName = theme + "256";
                        break;
                    case 512:
                        GameSprites[ix, iy].spriteName = theme + "512";
                        break;
                    case 1024:
                        GameSprites[ix, iy].spriteName = theme + "1024";
                        break;
                    case 2048:
                        GameSprites[ix, iy].spriteName = theme + "2048";
                        break;
                    case 4096:
                        GameSprites[ix, iy].spriteName = theme + "4096";
                        break;
                    case 8192:
                        GameSprites[ix, iy].spriteName = theme + "8192";
                        break;
                    case 16384:
                        GameSprites[ix, iy].spriteName = theme + "16384";
                        break;
                }
                
            }
        }
    }

    #endregion

    #region 移动
    private void SwipeRight()
    {
        isMove = false;
        for (int ix = 0; ix < Cloume; ix++)
        {
            for(int iy = Cloume-1; iy >= 0; iy--)
            {
                for(int ia = iy - 1; ia >= 0; ia--)
                {
                    if (iMap[ix, ia] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ix, ia];
                            iMap[ix, ia] = 0;
                            iy++;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ix, ia])
                        {
                            iMap[ix, iy] += iMap[ix, ia];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ix, ia] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }

    private void SwipeLeft()
    {
        isMove = false;
        for (int ix = 0; ix < Cloume; ix++)
        {
            for(int iy = 0; iy <Cloume; iy++)
            {
                for(int ia = iy + 1; ia < Cloume; ia++)
                {
                    if (iMap[ix, ia] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ix, ia];
                            iMap[ix, ia] = 0;
                            iy--;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ix, ia])
                        {
                            iMap[ix, iy] += iMap[ix, ia];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ix, ia] = 0;
                            isMove = true;

                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }

    private void SwipeUp()
    {
        isMove = false;
        for (int iy = 0; iy < Cloume; iy++)
        {
            for(int ix = 0; ix <Cloume; ix++)
            {
                for(int ib = ix + 1; ib <Cloume; ib++)
                {
                    if (iMap[ib, iy] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ib, iy];
                            iMap[ib, iy] = 0;
                            ix--;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ib, iy])
                        {
                            iMap[ix, iy] += iMap[ib, iy];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ib, iy] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }

    private void SwipeDown()
    {
        isMove = false;
        for (int iy = 0; iy < Cloume; iy++)
        {
            for (int ix = Cloume - 1; ix >= 0; ix--)
            {
                for (int ib = ix - 1; ib >= 0; ib--)
                {
                    if (iMap[ib, iy] > 0)
                    {
                        if (iMap[ix, iy] == 0)
                        {
                            iMap[ix, iy] = iMap[ib, iy];
                            iMap[ib, iy] = 0;
                            ix++;
                            isMove = true;
                        }
                        else if (iMap[ix, iy] == iMap[ib, iy])
                        {
                            iMap[ix, iy] += iMap[ib, iy];
                            Score += iMap[ix, iy];
                            MainScoreLabel.text = Score + "";
                            if (Score > BestScore)
                            {
                                BestScore = Score;
                                isNewBestScore = true;
                                MainBestScoreLabel.text = BestScore + "";
                            }
                            iMap[ib, iy] = 0;
                            isMove = true;
                        }
                        isGameOver();
                        break;
                    }
                }
            }
        }
        if (isMove)
            AddSprite();
    }
    #endregion

}
