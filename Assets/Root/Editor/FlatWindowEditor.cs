#if UNITY_EDITOR
namespace FlatBuffers
{
    using UnityEditor;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Diagnostics;
    using System.Threading;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using Worldreaver.EditorUtility;
    using Worldreaver.Utility;

    internal class Colors
    {
        internal static readonly UnityEngine.Color LightBlue = new Color32(173, 216, 230, 255);
        internal static readonly UnityEngine.Color White = new Color32(255, 255, 255, 255);
        internal static readonly UnityEngine.Color LawnGreen = new Color32(124, 252, 0, 255);
        internal static readonly UnityEngine.Color SeaGreen = new Color32(46, 139, 87, 255);
        internal static readonly UnityEngine.Color Cornsilk = new Color32(255, 248, 220, 255);
        internal static readonly UnityEngine.Color Lavender = new Color32(230, 230, 250, 255);
        internal static readonly UnityEngine.Color Orangered = new Color32(255, 69, 0, 255);
        internal static readonly UnityEngine.Color LightSteelBlue = new Color32(176, 196, 222, 255);
        internal static readonly UnityEngine.Color YellowGreen = new Color32(154, 205, 50, 255);
        internal static readonly UnityEngine.Color PaleGreen = new Color32(152, 251, 152, 255);
        internal static readonly UnityEngine.Color Red = new Color32(255, 0, 0, 255);
    }

    internal class FlatWindowEditor : EditorWindow
    {
        public SubWindow[] windows;
        private SubWindow _currentWindow;
        private const string CURRENT_NAME_WINDOW = "current_name_window";

        /// <summary>
        /// ctrl = %
        /// shift = #
        /// alt = &
        /// _ = no key modifiers
        /// </summary>
        [MenuItem("Window/Flat Data &#F", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(FlatWindowEditor));
            window.titleContent = new GUIContent("Worldreaver", EditorHelper.GetIcon("Flat.png"));
            window.minSize = new Vector2(800, 450);
        }

        private void OnGUI()
        {
            Initialize();
            var style = EditorStyle.Get;

            if (_currentWindow == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            try
            {
                for (int i = 0; i < windows.Length; i++)
                {
                    if (_currentWindow == windows[i])
                    {
                        GUI.backgroundColor = Colors.LightBlue;
                        if (GUILayout.Button(windows[i].nameWindow, style.menuButtonSelected))
                        {
                            SetCurrentWindow(windows[i]);
                        }

                        GUI.backgroundColor = Colors.White;
                    }
                    else
                    {
                        if (GUILayout.Button(windows[i].nameWindow, style.menuButton))
                        {
                            SetCurrentWindow(windows[i]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                windows = null;
                Initialize();
            }

            GUI.backgroundColor = Colors.LawnGreen;
            if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(17)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Open Folder Persistent"), false, () => OsFileBrowser.Open(Application.persistentDataPath));
                menu.AddItem(new GUIContent("Delete All File Persistent"), false, () =>
                {
                    if (EditorUtility.DisplayDialog("Clear Persistent Data Path", "Are you sure you wish to clear the persistent data path?\nThis action cannot be reversed.", "Clear", "Cancel"))
                    {
                        var di = new DirectoryInfo(Application.persistentDataPath);

                        foreach (var file in di.GetFiles())
                        {
                            file.Delete();
                        }

                        foreach (var dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                    }
                });
                menu.AddItem(new GUIContent("Delete All PlayerPrefs"), false, () =>
                {
                    if (EditorUtility.DisplayDialog("Delete PlayerPrefs", "Are you sure you wish to clear PlayerPrefs?\nThis action cannot be reversed.", "Clear", "Cancel"))
                    {
                        PlayerPrefs.DeleteAll();
                    }
                });
                menu.AddItem(new GUIContent("Delete FlatData EditorPrefs"), false, () =>
                {
                    if (EditorUtility.DisplayDialog("Delete EditorPrefs", "Are you sure you wish to clear EditorPrefs of FlatData?\nThis action cannot be reversed.", "Clear", "Cancel"))
                    {
                        EditorPrefs.DeleteKey(CURRENT_NAME_WINDOW);
                        EditorPrefs.DeleteKey("_indexMenuSchema");
                    }
                });
                menu.ShowAsContext();
            }

            GUI.backgroundColor = Colors.White;
            EditorGUILayout.EndHorizontal();
            _currentWindow?.OnGUI();
        }

        private void OnLostFocus()
        {
            _currentWindow?.OnLostFocus();
        }

        private void OnDestroy()
        {
            _currentWindow?.OnDestroy();
        }

        private void Initialize()
        {
            if (windows.IsNullOrEmpty())
            {
                InitializeSubWindows();
            }

            var currentWindowName = EditorPrefs.GetString(CURRENT_NAME_WINDOW);
            if (string.IsNullOrEmpty(currentWindowName) && windows.Length > 0 && windows[0] != null)
            {
                currentWindowName = windows[0].nameWindow;
            }

            for (int i = 0; i < windows.Length; i++)
            {
                if (windows[i].nameWindow != currentWindowName) continue;

                SetCurrentWindow(windows[i]);
                break;
            }
        }

        private void InitializeSubWindows()
        {
            windows = new SubWindow[]
            {
                new SchemaWindow(this),
                /*new EditorSchema(this),*/
                /*new SpreadsheetWindow(this)*/
            };
        }

        private void SetCurrentWindow(SubWindow window)
        {
            _currentWindow = window;
            EditorPrefs.SetString(CURRENT_NAME_WINDOW, _currentWindow.nameWindow);
        }

        private void SetCurrentWindow(Type type)
        {
            _currentWindow.OnLostFocus();
            _currentWindow = windows.First(w => w.GetType() == type);
            EditorPrefs.SetString(CURRENT_NAME_WINDOW, _currentWindow.nameWindow);
        }
    }

    // internal class SpreadsheetWindow : SubWindow
    // {
    //     private const string SAVE_SHEET_KEY = "ss_key";
    //     private const string SAVE_LIST_SHEET_NAME = "ss_sheet_name";
    //     private string _pathGenerateBinary;
    //     private string _pathGenerateCode;
    //     private Type _creatorType;
    //     private bool _isFetchPath;
    //     private bool _isFetchPathBinary;
    //     private bool _isFetchingSheetName;
    //     private bool _isFetchSaveSheetName;
    //     private bool _isFetchSpreadSheetKey;
    //     private const string ROOT_TABLE_NAME = "root_table_name";
    //
    //     public SpreadsheetWindow(EditorWindow parent) : base("Spreadsheet", parent)
    //     {
    //     }
    //
    //     public SpreadsheetWindow(string name,
    //         EditorWindow parent) : base(name, parent)
    //     {
    //     }
    //
    //     #region -- properties ------------------------------------------------
    //
    //     private const string CLIENT_ID = "121683403714-fpqv8gipdkfivqdsi24olgqrtau94l4v.apps.googleusercontent.com";
    //     private const string CLIENT_SECRET = "ra-CeiVe7xRR1JT4kgub-VE6";
    //     private const string APP_NAME = "Fetch";
    //     private static readonly string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};
    //
    //     /// <summary>
    //     /// Key of the spreadsheet. Get from url of the spreadsheet.
    //     /// </summary>
    //     private string _spreadSheetKey = "";
    //
    //     /// <summary>
    //     /// List of sheet names which want to download
    //     /// </summary>
    //     private readonly List<string> _wantedSheetNames = new List<string>();
    //
    //     /// <summary>
    //     /// Position of the scroll view.
    //     /// </summary>
    //     private Vector2 _scrollPosition;
    //
    //     /// <summary>
    //     /// Progress of download and convert action. 100 is "completed".
    //     /// </summary>
    //     private float _progress = 100;
    //
    //     /// <summary>
    //     /// The message which be shown on progress bar when action is running.
    //     /// </summary>
    //     private string _progressMessage = "";
    //
    //     #endregion
    //
    //     #region -- function ------------------------------------------------
    //
    //     private void Initialized()
    //     {
    //         _progress = 100;
    //         _progressMessage = "";
    //         ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
    //     }
    //
    //     public override void OnGUI()
    //     {
    //         var style = EditorStyle.Get;
    //         Initialized();
    //         EditorUtil.DrawUiLine(Colors.SeaGreen);
    //         _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.scrollView);
    //
    //         EditorGUILayout.BeginHorizontal(style.areaHorizontal);
    //         {
    //             if (!_isFetchSpreadSheetKey)
    //             {
    //                 _isFetchSpreadSheetKey = true;
    //                 if (EditorPrefs.HasKey(SAVE_SHEET_KEY)) _spreadSheetKey = EditorPrefs.GetString(SAVE_SHEET_KEY);
    //             }
    //
    //             EditorGUILayout.LabelField(new GUIContent("API Key", "SpreadSheet Key or SpreadSheet Full Link"), style.widthLabel);
    //             _spreadSheetKey = EditorGUILayout.TextField(_spreadSheetKey);
    //             //edit spread sheet key if enter full link
    //             //https://docs.google.com/spreadsheets/d/..../edit#gid=1284357149
    //             if (_spreadSheetKey.Contains("docs.google.com"))
    //             {
    //                 _spreadSheetKey = _spreadSheetKey.Replace("https://docs.google.com/spreadsheets/d/", "");
    //                 var raws = _spreadSheetKey.Split('/');
    //                 _spreadSheetKey = raws[0];
    //             }
    //         }
    //
    //         EditorGUILayout.EndHorizontal();
    //         EditorGUILayout.HelpBox("These sheets below will be downloaded. Let the list blank (remove all items) if you want to download all sheets", MessageType.Info);
    //         var removeId = -1;
    //         if (!_isFetchSaveSheetName)
    //         {
    //             _isFetchSaveSheetName = true;
    //             if (EditorPrefs.HasKey(SAVE_LIST_SHEET_NAME))
    //             {
    //                 _wantedSheetNames.Clear();
    //                 var sheetCount = EditorPrefs.GetInt(SAVE_LIST_SHEET_NAME);
    //                 for (int i = 0; i < sheetCount; i++)
    //                 {
    //                     _wantedSheetNames.Add(EditorPrefs.GetString(SAVE_LIST_SHEET_NAME + i));
    //                 }
    //             }
    //         }
    //
    //         for (var i = 0; i < _wantedSheetNames.Count; i++)
    //         {
    //             EditorGUILayout.BeginHorizontal(style.areaHorizontal);
    //             EditorGUILayout.LabelField($"Sheet {i}", style.widthLabel);
    //             _wantedSheetNames[i] = EditorGUILayout.TextField(_wantedSheetNames[i]);
    //             if (GUILayout.Button("", style.searchCancelButton)) removeId = i;
    //             EditorGUILayout.EndHorizontal();
    //         }
    //
    //         if (removeId >= 0) _wantedSheetNames.RemoveAt(removeId);
    //         EditorGUILayout.Space(8);
    //         GUI.color = Colors.Cornsilk;
    //         GUILayout.Label(_wantedSheetNames.Count <= 0 ? "Status: Download all sheets" : $"Status: Download {_wantedSheetNames.Count} sheets");
    //         GUI.color = Colors.White;
    //
    //         GUI.backgroundColor = Colors.Lavender;
    //         if (GUILayout.Button("Fetch sheet name", GUILayout.Width(130)))
    //         {
    //             FetchSheetName();
    //         }
    //
    //         GUI.backgroundColor = Colors.White;
    //
    //         if (!_isFetchPath)
    //         {
    //             _isFetchPath = true;
    //             _pathGenerateCode = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH)) : EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH;
    //         }
    //
    //         GUILayout.Space(8);
    //         EditorGUILayout.BeginHorizontal();
    //         EditorGUILayout.LabelField("Generate path", style.widthLabel);
    //         GUI.enabled = false;
    //         EditorGUILayout.TextField(_pathGenerateCode);
    //         GUI.enabled = true;
    //         EditorHelper.PickFolderPath(ref _pathGenerateCode, nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH));
    //         EditorGUILayout.EndHorizontal();
    //
    //         GUI.backgroundColor = Colors.LawnGreen;
    //         if (GUILayout.Button("Fetch"))
    //         {
    //             FetchGenerate();
    //         }
    //
    //         void FetchSheetName()
    //         {
    //             if (string.IsNullOrEmpty(_spreadSheetKey))
    //             {
    //                 EditorUtility.DisplayDialog("Spreadsheet key", "Are you sure spreadsheet key is not empty?", "Ok");
    //                 return;
    //             }
    //
    //             if (_isFetchingSheetName)
    //             {
    //                 return;
    //             }
    //
    //             _isFetchingSheetName = true;
    //             _wantedSheetNames.Clear();
    //             var service = new SheetsService(new BaseClientService.Initializer()
    //             {
    //                 HttpClientInitializer = GetCredential(),
    //                 ApplicationName = APP_NAME,
    //             });
    //             var spreadSheetData = service.Spreadsheets.Get(_spreadSheetKey).Execute();
    //             var sheets = spreadSheetData.Sheets;
    //
    //             if (sheets != null && sheets.Count > 0)
    //             {
    //                 foreach (var sheet in sheets)
    //                 {
    //                     _wantedSheetNames.Add(sheet.Properties.Title);
    //                 }
    //
    //                 _isFetchingSheetName = false;
    //             }
    //         }
    //
    //         void FetchGenerate()
    //         {
    //             if (string.IsNullOrEmpty(_spreadSheetKey))
    //             {
    //                 EditorUtility.DisplayDialog("Spreadsheet key", "Are you sure spreadsheet key is not empty?", "Ok");
    //                 return;
    //             }
    //
    //             _progress = 0;
    //             var pathG = _pathGenerateCode;
    //             if (!EditorHelper.IsValidPath(pathG))
    //             {
    //                 pathG = pathG.Insert(0, Application.dataPath);
    //             }
    //
    //             GenerateDatabase(pathG);
    //             EditorPrefs.SetString(SAVE_SHEET_KEY, _spreadSheetKey);
    //             EditorPrefs.SetInt(SAVE_LIST_SHEET_NAME, _wantedSheetNames.Count);
    //             for (int i = 0; i < _wantedSheetNames.Count; i++)
    //             {
    //                 EditorPrefs.SetString(SAVE_LIST_SHEET_NAME + i, _wantedSheetNames[i]);
    //             }
    //         }
    //
    //         GUI.backgroundColor = Colors.White;
    //
    //         if (_creatorType == null) _creatorType = TypeUtil.GetTypeByName("GameDatabaseCreate");
    //         if (_creatorType != null)
    //         {
    //             if (!_isFetchPathBinary)
    //             {
    //                 _isFetchPathBinary = true;
    //                 _pathGenerateBinary = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH)) : EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH;
    //             }
    //
    //             GUILayout.Space(10);
    //             EditorUtil.DrawUiLine(Colors.SeaGreen);
    //             EditorGUILayout.BeginHorizontal();
    //             EditorGUILayout.LabelField("Generate path", style.widthLabel);
    //             GUI.enabled = false;
    //             EditorGUILayout.TextField(_pathGenerateBinary);
    //             GUI.enabled = true;
    //             EditorHelper.PickFolderPath(ref _pathGenerateBinary, nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH));
    //             EditorGUILayout.EndHorizontal();
    //
    //             if (EditorPrefs.HasKey(ROOT_TABLE_NAME))
    //             {
    //                 GUI.color = Colors.Cornsilk;
    //                 EditorGUILayout.LabelField($"Name binary file will is '{EditorPrefs.GetString(ROOT_TABLE_NAME)}_binary'");
    //                 GUI.color = Colors.White;
    //                 GUI.backgroundColor = Colors.LawnGreen;
    //                 if (GUILayout.Button("Generate binary")) GenerateBinary();
    //                 if (GUILayout.Button("Generate binary storage")) GenerateBinaryStorage();
    //                 GUI.backgroundColor = Colors.White;
    //             }
    //             else
    //             {
    //                 GUI.color = Colors.Orangered;
    //                 EditorGUILayout.LabelField("Can not detected name root table!", GUILayout.Width(200));
    //                 GUI.color = Colors.White;
    //                 if (GUILayout.Button("Get Name Root Table", GUILayout.Width(160))) OnGetNameRootTable();
    //             }
    //
    //             void OnGetNameRootTable()
    //             {
    //                 if (string.IsNullOrEmpty(_spreadSheetKey))
    //                 {
    //                     EditorUtility.DisplayDialog("Spreadsheet key", "Are you sure spreadsheet key is not empty?", "Ok");
    //                     return;
    //                 }
    //
    //                 if (_isFetchingSheetName)
    //                 {
    //                     return;
    //                 }
    //
    //                 _isFetchingSheetName = true;
    //                 var service = new SheetsService(new BaseClientService.Initializer()
    //                 {
    //                     HttpClientInitializer = GetCredential(),
    //                     ApplicationName = APP_NAME,
    //                 });
    //                 var spreadSheetData = service.Spreadsheets.Get(_spreadSheetKey).Execute();
    //                 EditorPrefs.SetString(ROOT_TABLE_NAME, spreadSheetData.Properties.Title);
    //                 _isFetchingSheetName = false;
    //             }
    //
    //             void GenerateBinary()
    //             {
    //                 var pathG = _pathGenerateBinary;
    //                 if (!EditorHelper.IsValidPath(pathG))
    //                 {
    //                     pathG = pathG.Insert(0, Application.dataPath);
    //                 }
    //
    //                 if (!Directory.Exists(pathG))
    //                 {
    //                     Directory.CreateDirectory(pathG);
    //                 }
    //
    //                 _creatorType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] {pathG, $"{EditorPrefs.GetString(ROOT_TABLE_NAME)}_binary"});
    //                 GUI.FocusControl(null);
    //             }
    //
    //             void GenerateBinaryStorage()
    //             {
    //                 var pathG = _pathGenerateBinary;
    //                 if (!EditorHelper.IsValidPath(pathG))
    //                 {
    //                     pathG = pathG.Insert(0, Application.dataPath);
    //                 }
    //
    //                 if (!Directory.Exists(pathG))
    //                 {
    //                     Directory.CreateDirectory(pathG);
    //                 }
    //
    //                 _creatorType.GetMethod("Run2", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] {pathG, $"{EditorPrefs.GetString(ROOT_TABLE_NAME)}_binary"});
    //                 GUI.FocusControl(null);
    //             }
    //         }
    //
    //         if ((_progress < 100) && (_progress > 0))
    //         {
    //             if (EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100))
    //             {
    //                 _progress = 100;
    //                 EditorUtility.ClearProgressBar();
    //             }
    //         }
    //         else
    //         {
    //             EditorUtility.ClearProgressBar();
    //         }
    //
    //         EditorGUILayout.Space(EditorStyle.LAST_SPACE_SCROLL);
    //         EditorGUILayout.EndScrollView();
    //     }
    //
    //     private void GenerateDatabase(string scriptsPath)
    //     {
    //         UnityEngine.Debug.Log("Downloading with sheet key: " + _spreadSheetKey);
    //
    //         //Authenticate
    //         _progressMessage = "Authenticating...";
    //         var service = new SheetsService(new BaseClientService.Initializer()
    //         {
    //             HttpClientInitializer = GetCredential(),
    //             ApplicationName = APP_NAME,
    //         });
    //
    //         _progress = 5;
    //         EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100);
    //         _progressMessage = "Get list of spreadsheets...";
    //         EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100);
    //
    //         var spreadSheetData = service.Spreadsheets.Get(_spreadSheetKey).Execute();
    //         var sheets = spreadSheetData.Sheets;
    //
    //         if ((sheets == null) || (sheets.Count <= 0))
    //         {
    //             UnityEngine.Debug.LogError("Not found any data!");
    //             _progress = 100;
    //             EditorUtility.ClearProgressBar();
    //             return;
    //         }
    //
    //         _progress = 15;
    //
    //         //For each sheet in received data, check the sheet name. If that sheet is the wanted sheet, add it into the ranges.
    //         var ranges = new List<string>();
    //         foreach (var sheet in sheets)
    //         {
    //             if ((_wantedSheetNames.Count <= 0) || (_wantedSheetNames.Contains(sheet.Properties.Title)))
    //             {
    //                 ranges.Add(sheet.Properties.Title);
    //             }
    //         }
    //
    //         var request = service.Spreadsheets.Values.BatchGet(_spreadSheetKey);
    //         request.Ranges = ranges;
    //         var response = request.Execute();
    //         var dataCreatorScript = EditorHelper.GetTemplateByName("GameDatabaseCreateTemplate");
    //         var appends = "";
    //         var fileNamespace = "";
    //         var prexMasterTable = "";
    //         var masterTitle = spreadSheetData.Properties.Title;
    //         EditorPrefs.SetString(ROOT_TABLE_NAME, masterTitle);
    //         //For each wanted sheet
    //         foreach (var valueRange in response.ValueRanges)
    //         {
    //             var sheetname = valueRange.Range.Split('!')[0];
    //             _progressMessage = $"Processing {sheetname}...";
    //             EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100);
    //
    //             string s;
    //             (s, fileNamespace) = GenerateDbCreatorFromTemplate(sheetname, valueRange);
    //             prexMasterTable += $"{sheetname.ToLower()}_sortedvector,";
    //             appends += $"{s}\n";
    //             if (_wantedSheetNames.Count <= 0)
    //                 _progress += 85f / (response.ValueRanges.Count);
    //             else
    //                 _progress += 85f / _wantedSheetNames.Count;
    //         }
    //
    //         appends += $"var root = {masterTitle}.Create{masterTitle}(builder, {prexMasterTable.Remove(prexMasterTable.Length - 1)});\nbuilder.Finish(root.Value);";
    //         dataCreatorScript = dataCreatorScript.Replace("__namespace__", fileNamespace.Contains("FlatBufferGenerated") ? fileNamespace : $"FlatBufferGenerated.{fileNamespace}").Replace("__data_replace__", appends);
    //         dataCreatorScript = Regex.Replace(dataCreatorScript, @"\r\n|\n\r|\r|\n", Environment.NewLine); // Normalize line endings
    //         if (!Directory.Exists(scriptsPath))
    //         {
    //             Directory.CreateDirectory(scriptsPath);
    //         }
    //
    //         EditorHelper.WriteToFile($"{scriptsPath}/GameDatabaseCreate.cs".Replace("/", "\\"), dataCreatorScript);
    //         _progress = 100;
    //         AssetDatabase.SaveAssets();
    //         AssetDatabase.Refresh();
    //         UnityEngine.Debug.Log($"<color=#25854B>Download completed!</color>");
    //     }
    //
    //     private static (string, string) GenerateDbCreatorFromTemplate(string fileName,
    //         ValueRange valueRange)
    //     {
    //         //Get properties's name, data type and sheet data
    //         IDictionary<int, string> propertyNames = new Dictionary<int, string>(); //Dictionary of (column index, property name of that column)
    //         IDictionary<int, Dictionary<int, string>> values = new Dictionary<int, Dictionary<int, string>>(); //Dictionary of (row index, dictionary of (column index, value in cell))
    //         var rowIndex = 0;
    //         foreach (var row in valueRange.Values)
    //         {
    //             var columnIndex = 0;
    //             foreach (string cellValue in row)
    //             {
    //                 var value = cellValue;
    //                 if (rowIndex == 0)
    //                 {
    //                     //This row is properties's name row
    //                     propertyNames.Add(columnIndex, value);
    //                 }
    //                 else
    //                 {
    //                     //Data rows
    //                     //Because first row is name row so we will minus 1 from rowIndex to make data index start from 0
    //                     if (!values.ContainsKey(rowIndex - 1))
    //                     {
    //                         values.Add(rowIndex - 1, new Dictionary<int, string>());
    //                     }
    //
    //                     values[rowIndex - 1].Add(columnIndex, value);
    //                 }
    //
    //                 columnIndex++;
    //             }
    //
    //             rowIndex++;
    //         }
    //
    //         var typeClass = TypeUtil.GetTypeByName(fileName);
    //         var props = typeClass.GetProperties();
    //         var methods = typeClass.GetMethods();
    //
    //         //Create list of Dictionaries (property name, value). Each dictionary represent for a object in a row of sheet.
    //         var sortedData = "";
    //         var dataAppend = "";
    //
    //         // avoid malloc many time
    //         // ReSharper disable once TooWideLocalVariableScope
    //         Type propertyType;
    //         string propName;
    //         foreach (var rowId in values.Keys)
    //         {
    //             var dataProperty = $"{fileName}.Create{fileName}(builder, __property_data__),\n";
    //             var tempProperty = "";
    //
    //             foreach (var columnId in propertyNames.Keys)
    //             {
    //                 if (!values[rowId].ContainsKey(columnId))
    //                 {
    //                     values[rowId].Add(columnId, "");
    //                 }
    //
    //                 bool isArray = false;
    //                 propName = propertyNames[columnId];
    //                 propertyType = props.FirstOrDefault(_ => _.Name == propName)?.PropertyType;
    //                 if (propertyType == null)
    //                 {
    //                     propertyType = methods.FirstOrDefault(_ => _.Name == propName)?.ReturnType;
    //                     if (propertyType == null) continue;
    //
    //                     isArray = true;
    //                 }
    //
    //                 if (isArray || EditorHelper.IsEnumerable(propertyType))
    //                 {
    //                     if (isArray && propertyType == typeof(string))
    //                     {
    //                         var offsets = values[rowId][columnId].Split('_');
    //                         var offsetVector = offsets.Aggregate("", (current,
    //                             offset) => current + $"builder.CreateString(\"{offset}\"),");
    //                         offsetVector = offsetVector.Remove(offsetVector.Length - 1);
    //                         tempProperty += $"{fileName}.Create{propName}Vector(builder, new []{{{offsetVector}}}),";
    //                     }
    //                     else
    //                     {
    //                         tempProperty += $"{fileName}.Create{propName}Vector(builder, new []{{{values[rowId][columnId].Replace("_", ",")}}}),";
    //                     }
    //                 }
    //                 else if (propertyType == typeof(string))
    //                 {
    //                     tempProperty += $"builder.CreateString(\"{values[rowId][columnId]}\"),";
    //                 }
    //                 else
    //                 {
    //                     tempProperty += $"{values[rowId][columnId]},";
    //                 }
    //             }
    //
    //             sortedData += $"{dataProperty.Replace("__property_data__", tempProperty.Remove(tempProperty.Length - 1))}";
    //         }
    //
    //         dataAppend += $"var {fileName.ToLower()}_sortedvector =  {fileName}.CreateSortedVectorOf{fileName}(builder, new[] {{{sortedData.Remove(sortedData.Length - 2)}}});";
    //         return (dataAppend, typeClass.Namespace);
    //     }
    //
    //     #region -- credential ------------------------------------------------
    //
    //     private static UserCredential GetCredential()
    //     {
    //         var fullPath = Path.GetFullPath(EditorHelper.PACKAGES_PATH);
    //         if (!Directory.Exists(fullPath))
    //         {
    //             fullPath = Path.Combine(Application.dataPath, "Root");
    //         }
    //
    //         fullPath += "/Spreadsheet/";
    //         var fi = new FileInfo(fullPath);
    //         var scriptFolder = fi.Directory?.ToString();
    //         var unused = scriptFolder?.Replace('\\', '/');
    //         UnityEngine.Debug.Log("Save Credential to: " + scriptFolder);
    //
    //         UserCredential credential = null;
    //         var clientSecrets = new ClientSecrets {ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET};
    //         try
    //         {
    //             credential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, Scopes, "user", CancellationToken.None, new FileDataStore(scriptFolder, true)).Result;
    //         }
    //         catch (Exception e)
    //         {
    //             UnityEngine.Debug.LogError(e.ToString());
    //         }
    //
    //         return credential;
    //     }
    //
    //     private static bool MyRemoteCertificateValidationCallback(object sender,
    //         X509Certificate certificate,
    //         X509Chain chain,
    //         SslPolicyErrors sslPolicyErrors)
    //     {
    //         var isOk = true;
    //         // If there are errors in the certificate chain, look at each error to determine the cause.
    //         // ReSharper disable once InvertIf
    //         if (sslPolicyErrors != SslPolicyErrors.None)
    //         {
    //             // ReSharper disable once ForCanBeConvertedToForeach
    //             for (var i = 0; i < chain.ChainStatus.Length; i++)
    //             {
    //                 // ReSharper disable once InvertIf
    //                 if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
    //                 {
    //                     chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
    //                     chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
    //                     chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
    //                     chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
    //                     var chainIsValid = chain.Build((X509Certificate2) certificate);
    //                     // ReSharper disable once InvertIf
    //                     if (!chainIsValid)
    //                     {
    //                         UnityEngine.Debug.LogError("certificate chain is not valid");
    //                         isOk = false;
    //                     }
    //                 }
    //             }
    //         }
    //
    //         return isOk;
    //     }
    //
    //     #endregion
    //
    //     #endregion
    // }

    internal class SchemaWindow : SubWindow
    {
        private SubWindow[] _subs;
        private SubWindow _sub;
        private readonly string[] _menuName = {"From fbs file"};
        private int _indexMenuSchema;
        private bool _isFetchIndex;

        public SchemaWindow(EditorWindow window) : base("Schema", window)
        {
        }

        public SchemaWindow(string name,
            EditorWindow parent) : base(name, parent)
        {
        }

        public override void OnGUI()
        {
            Initialize();
            if (_sub == null) return;
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Colors.LawnGreen;
            if (EditorGUILayout.DropdownButton(new GUIContent(" " + _menuName[_indexMenuSchema] + " "), FocusType.Passive, EditorStyles.toolbarDropDown, GUILayout.Width(120)))
            {
                var menu = new GenericMenu();
                for (int i = 0; i < _menuName.Length; i++)
                {
                    var index = i;
                    if (i != _indexMenuSchema)
                    {
                        menu.AddItem(new GUIContent(_menuName[i]), false, () =>
                        {
                            _indexMenuSchema = index;
                            SetCurrentWindow(_subs[_indexMenuSchema]);
                            EditorPrefs.SetInt(nameof(_indexMenuSchema), _indexMenuSchema);
                        });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent(_menuName[i]));
                    }
                }

                menu.ShowAsContext();
            }

            GUI.backgroundColor = Colors.White;
            EditorUtil.DrawUiLine(Colors.YellowGreen, padding: 15);
            EditorGUILayout.EndHorizontal();
            _sub?.OnGUI();
        }

        public override void OnLostFocus()
        {
            base.OnLostFocus();
            _sub?.OnLostFocus();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _sub?.OnDestroy();
        }

        private void Initialize()
        {
            if (!_isFetchIndex && EditorPrefs.HasKey(nameof(_indexMenuSchema)))
            {
                _isFetchIndex = true;
                _indexMenuSchema = EditorPrefs.GetInt(nameof(_indexMenuSchema));
            }

            if (_subs.IsNullOrEmpty())
            {
                InitializeSub();
            }

            for (int i = 0; i < _subs.Length; i++)
            {
                if (i != _indexMenuSchema) continue;

                SetCurrentWindow(_subs[i]);
                break;
            }
        }

        private void InitializeSub()
        {
            _subs = new SubWindow[]
            {
                new SchemaWindowB(parent)
            };
        }

        private void SetCurrentWindow(SubWindow window)
        {
            _sub = window;
        }
    }

    internal class SchemaWindowB : SubWindow
    {
        private string _pathGenerate = "";
        private string _pathSchemaFile = "";
        private readonly string[] _options = {"--gen-mutable", "--gen-object-api"};
        private bool _isFetchPath;
        private int _indexOption;

        public SchemaWindowB(EditorWindow window) : base("From fbs", window)
        {
        }

        public SchemaWindowB(string name,
            EditorWindow parent) : base(name, parent)
        {
        }

        public override void OnGUI()
        {
            var style = EditorStyle.Get;

            EditorGUILayout.BeginHorizontal(style.areaHorizontal);
            EditorGUILayout.LabelField("Fbs file", style.widthLabel);
            GUI.enabled = false;
            EditorGUILayout.TextField(_pathSchemaFile);
            GUI.enabled = true;
            EditorHelper.PickFilePath(ref _pathSchemaFile, "fbs");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(style.areaHorizontal);
            if (!_isFetchPath)
            {
                _isFetchPath = true;
                _pathGenerate = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_GENERATE_CODE_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_GENERATE_CODE_PATH)) : EditorHelper.DEFAULT_GENERATE_CODE_PATH;
            }

            EditorGUILayout.LabelField("Generate Path", style.widthLabel);
            GUI.enabled = false;
            EditorGUILayout.TextField(_pathGenerate);
            GUI.enabled = true;
            EditorHelper.PickFolderPath(ref _pathGenerate, nameof(EditorHelper.DEFAULT_GENERATE_CODE_PATH));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(style.areaHorizontal);
            EditorGUILayout.LabelField("Options", style.widthLabel);
            _indexOption = EditorGUILayout.Popup(_indexOption, _options, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            EditorHelper.Button("Generate code", style, GenerateSchema, Colors.Lavender);

            void GenerateSchema()
            {
                if (string.IsNullOrEmpty(_pathSchemaFile))
                {
                    EditorUtility.DisplayDialog("Path empty", "Are you sure path schema (fbs) file is not empty?", "Ok");
                    return;
                }

                UnityEngine.Debug.Log("Flatc generator : Start...");
                var pathG = _pathGenerate;
                if (!EditorHelper.IsValidPath(pathG))
                {
                    pathG = pathG.Insert(0, Application.dataPath);
                }

                var toolsPath = Path.GetFullPath(EditorHelper.PACKAGES_PATH);
                if (!Directory.Exists(toolsPath))
                {
                    toolsPath = Path.Combine(Application.dataPath, "Root");
                }

                var psi = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = $"{toolsPath}/FlatBuffer/GeneratorTools/flatc.exe",
                    Arguments = $@" --csharp -o ""{pathG}"" ""{Path.GetFullPath(_pathSchemaFile)}"" {_options[_indexOption]}",
                };

                var p = Process.Start(psi);
                if (p == null) return;
                p.EnableRaisingEvents = true;
                p.Exited += (sender,
                    e) =>
                {
                    UnityEngine.Debug.Log("Flatc generator : Complete!");
                    p?.Dispose();
                    p = null;
                };
            }
        }
    }

    internal class EditorSchema : SubWindow
    {
        private Vector2 _scrollPosition;
        private Type _typeParser;
        private string _masterName;
        private int _indexSelect;
        private Type[] _types;

        private ScriptableObject _data;

        public EditorSchema(EditorWindow parent) : base("Editor", parent)
        {
        }

        public EditorSchema(string name,
            EditorWindow parent) : base(name, parent)
        {
        }

        public override void OnGUI()
        {
            EditorUtil.DrawUiLine(Colors.Orangered);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.scrollView);
            var style = EditorStyle.Get;

            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scriptable", style.widthLabel);
            _data = (ScriptableObject) EditorGUILayout.ObjectField(_data, typeof(ScriptableObject), false);
            EditorGUILayout.EndHorizontal();

            if (_data != null)
            {
                EditorGUILayout.Space(8);
                if (GUILayout.Button("Make Creator")) OnCreator();
            }

            void OnCreator()
            {
                _types = ((IDatabaseInfo) _data).RootTypes;
                foreach (var type in _types)
                {
                    foreach (var item in type.GetFields())
                    {
                        var typeItem = item.FieldType;
                        if (EditorHelper.IsEnumerable(typeItem))
                        {
                            var elementType = typeItem.GetElementType();
                            if (elementType != null)
                            {
                                if (elementType.IsClass && !(elementType.IsPrimitive || elementType.IsValueType || elementType == typeof(string)) && !elementType.IsEnum)
                                {
                                    //class[]
                                }
                                else if (elementType.IsEnum)
                                {
                                    //enum[]
                                }
                                else if (elementType == typeof(string))
                                {
                                    //string[]
                                }
                                else if (typeItem == typeof(float))
                                {
                                    //float[]
                                }
                                else
                                {
                                    //int[]
                                }
                            }
                        }
                        else
                        {
                            if (typeItem.IsClass && !(typeItem.IsPrimitive || typeItem.IsValueType || typeItem == typeof(string)) && !typeItem.IsEnum)
                            {
                                //class
                            }
                            else if (typeItem.IsEnum)
                            {
                                //enum
                            }
                            else if (typeItem == typeof(string))
                            {
                                //string
                            }
                            else if (typeItem == typeof(float))
                            {
                                //float
                            }
                            else
                            {
                                //int
                            }
                        }
                    }
                }
            }

            EditorGUILayout.Space(EditorStyle.LAST_SPACE_SCROLL);
            EditorGUILayout.EndScrollView();
        }
    }

    internal static class OsFileBrowser
    {
        internal static bool MacEditor => Application.platform == RuntimePlatform.OSXEditor;

        private static void OpenInMac(string path)
        {
            var openInsidesOfFolder = false;

            // try mac
            var macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

            if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }

            if (!macPath.StartsWith("\""))
            {
                macPath = "\"" + macPath;
            }

            if (!macPath.EndsWith("\""))
            {
                macPath += "\"";
            }

            var arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

            try
            {
                Process.Start("open", arguments);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        private static void OpenInWin(string path)
        {
            var openInsidesOfFolder = false;

            // try windows
            var winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

            if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
            {
                openInsidesOfFolder = true;
            }

            try
            {
                Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        internal static void Open(string path)
        {
            if (!MacEditor)
            {
                OpenInWin(path);
            }
            else if (MacEditor)
            {
                OpenInMac(path);
            }
            else // couldn't determine OS
            {
                OpenInWin(path);
                OpenInMac(path);
            }
        }
    }

    internal abstract class SubWindow
    {
        public readonly string nameWindow;
        public EditorWindow parent;

        public abstract void OnGUI();

        public SubWindow(string name,
            EditorWindow parent)
        {
            nameWindow = name;
            this.parent = parent;
        }

        public virtual void OnLostFocus()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }

    internal class EditorStyle
    {
        private static EditorStyle style = null;
        internal readonly GUIStyle areaVertical;
        internal readonly GUIStyle areaHorizontal;
        internal readonly GUIStyle menuButton;
        internal readonly GUIStyle menuButtonSelected;
        internal readonly GUIStyle normalButton;
        internal readonly GUIStyle helpBox;
        internal readonly GUIStyle searchTextField;

        internal readonly GUIStyle searchCancelButton;

        //internal readonly GUIStyle buttonMin = "WinBtnMinMac";
        //internal readonly GUIStyle buttonInactive = "WinBtnInactiveMac";
        //internal readonly GUIStyle buttonClose = OsFileBrowser.MacEditor ? "WinBtnCloseMac" : "WinBtnClose";
        //internal readonly GUIStyle buttonMax = OsFileBrowser.MacEditor ? "WinBtnMaxMac" : "WinBtnMax";
        //internal readonly GUIStyle buttonRestore = OsFileBrowser.MacEditor ? "WinBtnRestoreMac" : "WinBtnRestore";
        internal readonly GUILayoutOption widthLabel = GUILayout.Width(92);
        internal const int LAST_SPACE_SCROLL = 20;

        public static EditorStyle Get => style ?? (style = new EditorStyle());

        public EditorStyle()
        {
            areaVertical = new GUIStyle {padding = new RectOffset(0, 0, 5, 5)};
            areaHorizontal = new GUIStyle {padding = new RectOffset(10, 10, 0, 0)};
            menuButton = new GUIStyle(EditorStyles.toolbarButton) {fontStyle = FontStyle.Bold, fontSize = 14, fixedHeight = 20};
            var selectLabel = new GUIStyleState {textColor = Colors.LightSteelBlue};
            menuButtonSelected = new GUIStyle(menuButton) {fontStyle = FontStyle.BoldAndItalic, active = selectLabel, normal = selectLabel, hover = selectLabel};
            normalButton = new GUIStyle(EditorStyles.miniButton) {fontStyle = FontStyle.Normal, fontSize = 12, fixedHeight = 20};
            helpBox = GUI.skin.GetStyle("HelpBox");
            helpBox.richText = true;
            helpBox.fontSize = 11;
            helpBox.wordWrap = true;
            searchTextField = new GUIStyle("ToolbarSeachTextField");
            searchCancelButton = new GUIStyle("ToolbarSeachCancelButton");
        }
    }

    internal static class EditorHelper
    {
        internal const string PACKAGES_PATH = "Packages/com.worldreaver.flatdata";
        internal const string DEFAULT_GENERATE_CODE_PATH = "/Root/Src/Scripts/Generated";
        internal const string DEFAULT_SPREADSHEET_GENERATE_CODE_PATH = "/Root/Src/Editor/Generated";
        internal const string DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH = "/Root/Src/Binary";
        internal const string DEFAULT_SCRIPTS_CREATOR_PATH = "/Root/Src/Scripts";

        internal static void Button(string name,
            EditorStyle style,
            Action action,
            Color32 color,
            int minWidth = -1)
        {
            GUI.backgroundColor = color;
            if (minWidth == -1)
            {
                if (GUILayout.Button(name, style.normalButton))
                {
                    action?.Invoke();
                }
            }
            else
            {
                if (GUILayout.Button(name, style.normalButton, GUILayout.Width(minWidth)))
                {
                    action?.Invoke();
                }
            }

            GUI.backgroundColor = Colors.White;
        }

        internal static void WriteToFile(string filePath,
            string content)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(fileStream, Encoding.ASCII))
                {
                    streamWriter.WriteLine(content);
                }
            }
        }

        internal static string GetTemplateByName(string templateName)
        {
            var fullPath = Path.GetFullPath(PACKAGES_PATH);
            if (!Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(Application.dataPath, "Root");
            }

            fullPath += $@"/Editor/Templates/{templateName}.txt";
            return File.ReadAllText(fullPath);
        }

        internal static Texture2D GetIcon(string name)
        {
            var path = Path.GetFullPath(PACKAGES_PATH) + $@"Root\Editor\Icons\{name}";
            Texture2D icon;
            if (Directory.Exists(path))
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            else
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("FlatWindowEditor")[0]).Split(new[] {"Editor"}, StringSplitOptions.RemoveEmptyEntries)[0] + $@"Editor\Icons\{name}");
            }

            return icon;
        }

        internal static bool IsValidPath(string path,
            bool allowRelativePaths = false)
        {
            bool isValid;

            try
            {
                var fullPath = Path.GetFullPath(path);

                if (allowRelativePaths)
                {
                    isValid = Path.IsPathRooted(path);
                }
                else
                {
                    var root = Path.GetPathRoot(path);
                    isValid = string.IsNullOrEmpty(root.Trim('\\', '/')) == false;
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

        internal static void PickFolderPath(ref string pathResult,
            string keySave = "")
        {
            GUI.backgroundColor = Colors.Cornsilk;
            if (GUILayout.Button(new GUIContent("", "Select folder"), EditorStyles.colorField, GUILayout.Width(18), GUILayout.Height(18)))
            {
                var path = EditorUtility.OpenFolderPanel("Select folder output", pathResult, "");
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave))
                    {
                        EditorPrefs.SetString(keySave, pathResult);
                    }
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Colors.White;
        }

        internal static void PickFilePath(ref string pathResult,
            string extension,
            string keySave = "")
        {
            GUI.backgroundColor = Colors.Cornsilk;
            if (GUILayout.Button(new GUIContent("", "Select file"), EditorStyles.colorField, GUILayout.Width(18), GUILayout.Height(18)))
            {
                var path = EditorUtility.OpenFilePanel("Select file", pathResult, extension);
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave))
                    {
                        EditorPrefs.SetString(keySave, pathResult);
                    }
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Colors.White;
        }

        internal static bool IsGenericEnumerable(Type type) => type.IsGenericType && type.GetInterfaces().Any(ti => (ti == typeof(IEnumerable<>) || ti.Name == "IEnumerable"));
        internal static bool IsEnumerable(Type type) => IsGenericEnumerable(type) || type.IsArray;
    }

    public interface IDatabaseInfo
    {
        string MasterName { get; }
        string NameSpace { get; }
        Type[] RootTypes { get; }
    }
}

#endif