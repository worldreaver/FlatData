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
    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Services;
    using Google.Apis.Sheets.v4;
    using Google.Apis.Sheets.v4.Data;
    using System.Text;
    using System.Diagnostics;
    using System.Threading;
    using Google.Apis.Util.Store;
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
            window.titleContent = new GUIContent("GodMod", EditorHelper.GetIcon("Flat.png"));
            window.minSize = new Vector2(500, 450);
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
                menu.AddItem(new GUIContent("Open Scripts Creator"), false, CreatorWindow.ShowWindow);
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
                new SpreadsheetWindow(this),
                new EditorSchema(this),
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

    internal class SpreadsheetWindow : SubWindow
    {
        private const string SAVE_SHEET_KEY = "ss_key";
        private const string SAVE_LIST_SHEET_NAME = "ss_sheet_name";
        private string _pathGenerateBinary;
        private string _pathGenerateCode;
        private Type _creatorType;
        private bool _isFetchPath;
        private bool _isFetchPathBinary;
        private bool _isFetchingSheetName;
        private bool _isFetchSaveSheetName;
        private bool _isFetchSpreadSheetKey;
        private const string ROOT_TABLE_NAME = "root_table_name";

        public SpreadsheetWindow(EditorWindow parent) : base("Spreadsheet", parent)
        {
        }

        public SpreadsheetWindow(string name,
            EditorWindow parent) : base(name, parent)
        {
        }

        #region -- properties ------------------------------------------------

        private const string CLIENT_ID = "121683403714-fpqv8gipdkfivqdsi24olgqrtau94l4v.apps.googleusercontent.com";
        private const string CLIENT_SECRET = "ra-CeiVe7xRR1JT4kgub-VE6";
        private const string APP_NAME = "Fetch";
        private static readonly string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};

        /// <summary>
        /// Key of the spreadsheet. Get from url of the spreadsheet.
        /// </summary>
        private string _spreadSheetKey = "";

        /// <summary>
        /// List of sheet names which want to download
        /// </summary>
        private readonly List<string> _wantedSheetNames = new List<string>();

        /// <summary>
        /// Position of the scroll view.
        /// </summary>
        private Vector2 _scrollPosition;

        /// <summary>
        /// Progress of download and convert action. 100 is "completed".
        /// </summary>
        private float _progress = 100;

        /// <summary>
        /// The message which be shown on progress bar when action is running.
        /// </summary>
        private string _progressMessage = "";

        #endregion

        #region -- function ------------------------------------------------

        private void Initialized()
        {
            _progress = 100;
            _progressMessage = "";
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        }

        public override void OnGUI()
        {
            var style = EditorStyle.Get;
            Initialized();
            EditorUtil.DrawUiLine(Colors.SeaGreen);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.scrollView);

            EditorGUILayout.BeginHorizontal(style.areaHorizontal);
            {
                if (!_isFetchSpreadSheetKey)
                {
                    _isFetchSpreadSheetKey = true;
                    if (EditorPrefs.HasKey(SAVE_SHEET_KEY)) _spreadSheetKey = EditorPrefs.GetString(SAVE_SHEET_KEY);
                }

                EditorGUILayout.LabelField(new GUIContent("API Key", "SpreadSheet Key or SpreadSheet Full Link"), style.widthLabel);
                _spreadSheetKey = EditorGUILayout.TextField(_spreadSheetKey);
                //edit spread sheet key if enter full link
                //https://docs.google.com/spreadsheets/d/..../edit#gid=1284357149
                if (_spreadSheetKey.Contains("docs.google.com"))
                {
                    _spreadSheetKey = _spreadSheetKey.Replace("https://docs.google.com/spreadsheets/d/", "");
                    var raws = _spreadSheetKey.Split('/');
                    _spreadSheetKey = raws[0];
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("These sheets below will be downloaded. Let the list blank (remove all items) if you want to download all sheets", MessageType.Info);
            var removeId = -1;
            if (!_isFetchSaveSheetName)
            {
                _isFetchSaveSheetName = true;
                if (EditorPrefs.HasKey(SAVE_LIST_SHEET_NAME))
                {
                    _wantedSheetNames.Clear();
                    var sheetCount = EditorPrefs.GetInt(SAVE_LIST_SHEET_NAME);
                    for (int i = 0; i < sheetCount; i++)
                    {
                        _wantedSheetNames.Add(EditorPrefs.GetString(SAVE_LIST_SHEET_NAME + i));
                    }
                }
            }

            for (var i = 0; i < _wantedSheetNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(style.areaHorizontal);
                EditorGUILayout.LabelField($"Sheet {i}", style.widthLabel);
                _wantedSheetNames[i] = EditorGUILayout.TextField(_wantedSheetNames[i]);
                if (GUILayout.Button("", style.searchCancelButton)) removeId = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeId >= 0) _wantedSheetNames.RemoveAt(removeId);
            EditorGUILayout.Space(8);
            GUI.color = Colors.Cornsilk;
            GUILayout.Label(_wantedSheetNames.Count <= 0 ? "Status: Download all sheets" : $"Status: Download {_wantedSheetNames.Count} sheets");
            GUI.color = Colors.White;

            GUI.backgroundColor = Colors.Lavender;
            if (GUILayout.Button("Fetch sheet name", GUILayout.Width(130)))
            {
                FetchSheetName();
            }

            GUI.backgroundColor = Colors.White;

            if (!_isFetchPath)
            {
                _isFetchPath = true;
                _pathGenerateCode = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH)) : EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH;
            }

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generate path", style.widthLabel);
            GUI.enabled = false;
            EditorGUILayout.TextField(_pathGenerateCode);
            GUI.enabled = true;
            EditorHelper.PickFolderPath(ref _pathGenerateCode, nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH));
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Colors.LawnGreen;
            if (GUILayout.Button("Fetch"))
            {
                FetchGenerate();
            }

            void FetchSheetName()
            {
                if (string.IsNullOrEmpty(_spreadSheetKey))
                {
                    EditorUtility.DisplayDialog("Spreadsheet key", "Are you sure spreadsheet key is not empty?", "Ok");
                    return;
                }

                if (_isFetchingSheetName)
                {
                    return;
                }

                _isFetchingSheetName = true;
                _wantedSheetNames.Clear();
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = GetCredential(),
                    ApplicationName = APP_NAME,
                });
                var spreadSheetData = service.Spreadsheets.Get(_spreadSheetKey).Execute();
                var sheets = spreadSheetData.Sheets;

                if (sheets != null && sheets.Count > 0)
                {
                    foreach (var sheet in sheets)
                    {
                        _wantedSheetNames.Add(sheet.Properties.Title);
                    }

                    _isFetchingSheetName = false;
                }
            }

            void FetchGenerate()
            {
                if (string.IsNullOrEmpty(_spreadSheetKey))
                {
                    EditorUtility.DisplayDialog("Spreadsheet key", "Are you sure spreadsheet key is not empty?", "Ok");
                    return;
                }

                _progress = 0;
                var pathG = _pathGenerateCode;
                if (!EditorHelper.IsValidPath(pathG))
                {
                    pathG = pathG.Insert(0, Application.dataPath);
                }

                GenerateDatabase(pathG);
                EditorPrefs.SetString(SAVE_SHEET_KEY, _spreadSheetKey);
                EditorPrefs.SetInt(SAVE_LIST_SHEET_NAME, _wantedSheetNames.Count);
                for (int i = 0; i < _wantedSheetNames.Count; i++)
                {
                    EditorPrefs.SetString(SAVE_LIST_SHEET_NAME + i, _wantedSheetNames[i]);
                }
            }

            GUI.backgroundColor = Colors.White;

            if (_creatorType == null) _creatorType = TypeUtil.GetTypeByName("GameDatabaseCreate");
            if (_creatorType != null)
            {
                if (!_isFetchPathBinary)
                {
                    _isFetchPathBinary = true;
                    _pathGenerateBinary = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH)) : EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH;
                }

                GUILayout.Space(10);
                EditorUtil.DrawUiLine(Colors.SeaGreen);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Generate path", style.widthLabel);
                GUI.enabled = false;
                EditorGUILayout.TextField(_pathGenerateBinary);
                GUI.enabled = true;
                EditorHelper.PickFolderPath(ref _pathGenerateBinary, nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_BINARY_PATH));
                EditorGUILayout.EndHorizontal();

                if (EditorPrefs.HasKey(ROOT_TABLE_NAME))
                {
                    GUI.color = Colors.Cornsilk;
                    EditorGUILayout.LabelField($"Name binary file will is '{EditorPrefs.GetString(ROOT_TABLE_NAME)}_binary'");
                    GUI.color = Colors.White;
                    GUI.backgroundColor = Colors.LawnGreen;
                    if (GUILayout.Button("Generate binary")) GenerateBinary();
                    if (GUILayout.Button("Generate binary storage")) GenerateBinaryStorage();
                    GUI.backgroundColor = Colors.White;
                }
                else
                {
                    GUI.color = Colors.Orangered;
                    EditorGUILayout.LabelField("Can not detected name root table!", GUILayout.Width(200));
                    GUI.color = Colors.White;
                    if (GUILayout.Button("Get Name Root Table", GUILayout.Width(160))) OnGetNameRootTable();
                }

                void OnGetNameRootTable()
                {
                    if (string.IsNullOrEmpty(_spreadSheetKey))
                    {
                        EditorUtility.DisplayDialog("Spreadsheet key", "Are you sure spreadsheet key is not empty?", "Ok");
                        return;
                    }

                    if (_isFetchingSheetName)
                    {
                        return;
                    }

                    _isFetchingSheetName = true;
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = GetCredential(),
                        ApplicationName = APP_NAME,
                    });
                    var spreadSheetData = service.Spreadsheets.Get(_spreadSheetKey).Execute();
                    EditorPrefs.SetString(ROOT_TABLE_NAME, spreadSheetData.Properties.Title);
                    _isFetchingSheetName = false;
                }

                void GenerateBinary()
                {
                    var pathG = _pathGenerateBinary;
                    if (!EditorHelper.IsValidPath(pathG))
                    {
                        pathG = pathG.Insert(0, Application.dataPath);
                    }

                    if (!Directory.Exists(pathG))
                    {
                        Directory.CreateDirectory(pathG);
                    }

                    _creatorType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] {pathG, $"{EditorPrefs.GetString(ROOT_TABLE_NAME)}_binary"});
                    GUI.FocusControl(null);
                }
                
                void GenerateBinaryStorage()
                {
                    var pathG = _pathGenerateBinary;
                    if (!EditorHelper.IsValidPath(pathG))
                    {
                        pathG = pathG.Insert(0, Application.dataPath);
                    }

                    if (!Directory.Exists(pathG))
                    {
                        Directory.CreateDirectory(pathG);
                    }

                    _creatorType.GetMethod("Run2", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] {pathG, $"{EditorPrefs.GetString(ROOT_TABLE_NAME)}_binary"});
                    GUI.FocusControl(null);
                }
            }

            if ((_progress < 100) && (_progress > 0))
            {
                if (EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100))
                {
                    _progress = 100;
                    EditorUtility.ClearProgressBar();
                }
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }

            EditorGUILayout.Space(EditorStyle.LAST_SPACE_SCROLL);
            EditorGUILayout.EndScrollView();
        }

        private void GenerateDatabase(string scriptsPath)
        {
            UnityEngine.Debug.Log("Downloading with sheet key: " + _spreadSheetKey);

            //Authenticate
            _progressMessage = "Authenticating...";
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GetCredential(),
                ApplicationName = APP_NAME,
            });

            _progress = 5;
            EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100);
            _progressMessage = "Get list of spreadsheets...";
            EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100);

            var spreadSheetData = service.Spreadsheets.Get(_spreadSheetKey).Execute();
            var sheets = spreadSheetData.Sheets;

            if ((sheets == null) || (sheets.Count <= 0))
            {
                UnityEngine.Debug.LogError("Not found any data!");
                _progress = 100;
                EditorUtility.ClearProgressBar();
                return;
            }

            _progress = 15;

            //For each sheet in received data, check the sheet name. If that sheet is the wanted sheet, add it into the ranges.
            var ranges = new List<string>();
            foreach (var sheet in sheets)
            {
                if ((_wantedSheetNames.Count <= 0) || (_wantedSheetNames.Contains(sheet.Properties.Title)))
                {
                    ranges.Add(sheet.Properties.Title);
                }
            }

            var request = service.Spreadsheets.Values.BatchGet(_spreadSheetKey);
            request.Ranges = ranges;
            var response = request.Execute();
            var dataCreatorScript = EditorHelper.GetTemplateByName("GameDatabaseCreateTemplate");
            var appends = "";
            var fileNamespace = "";
            var prexMasterTable = "";
            var masterTitle = spreadSheetData.Properties.Title;
            EditorPrefs.SetString(ROOT_TABLE_NAME, masterTitle);
            //For each wanted sheet
            foreach (var valueRange in response.ValueRanges)
            {
                var sheetname = valueRange.Range.Split('!')[0];
                _progressMessage = $"Processing {sheetname}...";
                EditorUtility.DisplayCancelableProgressBar("Processing", _progressMessage, _progress / 100);

                string s;
                (s, fileNamespace) = GenerateDbCreatorFromTemplate(sheetname, valueRange);
                prexMasterTable += $"{sheetname.ToLower()}_sortedvector,";
                appends += $"{s}\n";
                if (_wantedSheetNames.Count <= 0)
                    _progress += 85f / (response.ValueRanges.Count);
                else
                    _progress += 85f / _wantedSheetNames.Count;
            }

            appends += $"var root = {masterTitle}.Create{masterTitle}(builder, {prexMasterTable.Remove(prexMasterTable.Length - 1)});\nbuilder.Finish(root.Value);";
            dataCreatorScript = dataCreatorScript.Replace("__namespace__", fileNamespace.Contains("FlatBufferGenerated") ? fileNamespace : $"FlatBufferGenerated.{fileNamespace}").Replace("__data_replace__", appends);
            dataCreatorScript = Regex.Replace(dataCreatorScript, @"\r\n|\n\r|\r|\n", Environment.NewLine); // Normalize line endings
            if (!Directory.Exists(scriptsPath))
            {
                Directory.CreateDirectory(scriptsPath);
            }

            EditorHelper.WriteToFile($"{scriptsPath}/GameDatabaseCreate.cs".Replace("/", "\\"), dataCreatorScript);
            _progress = 100;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"<color=#25854B>Download completed!</color>");
        }

        private static (string, string) GenerateDbCreatorFromTemplate(string fileName,
            ValueRange valueRange)
        {
            //Get properties's name, data type and sheet data
            IDictionary<int, string> propertyNames = new Dictionary<int, string>(); //Dictionary of (column index, property name of that column)
            IDictionary<int, Dictionary<int, string>> values = new Dictionary<int, Dictionary<int, string>>(); //Dictionary of (row index, dictionary of (column index, value in cell))
            var rowIndex = 0;
            foreach (var row in valueRange.Values)
            {
                var columnIndex = 0;
                foreach (string cellValue in row)
                {
                    var value = cellValue;
                    if (rowIndex == 0)
                    {
                        //This row is properties's name row
                        propertyNames.Add(columnIndex, value);
                    }
                    else
                    {
                        //Data rows
                        //Because first row is name row so we will minus 1 from rowIndex to make data index start from 0
                        if (!values.ContainsKey(rowIndex - 1))
                        {
                            values.Add(rowIndex - 1, new Dictionary<int, string>());
                        }

                        values[rowIndex - 1].Add(columnIndex, value);
                    }

                    columnIndex++;
                }

                rowIndex++;
            }

            var typeClass = TypeUtil.GetTypeByName(fileName);
            var props = typeClass.GetProperties();
            var methods = typeClass.GetMethods();

            //Create list of Dictionaries (property name, value). Each dictionary represent for a object in a row of sheet.
            var sortedData = "";
            var dataAppend = "";

            // avoid malloc many time
            // ReSharper disable once TooWideLocalVariableScope
            Type propertyType;
            string propName;
            foreach (var rowId in values.Keys)
            {
                var dataProperty = $"{fileName}.Create{fileName}(builder, __property_data__),\n";
                var tempProperty = "";

                foreach (var columnId in propertyNames.Keys)
                {
                    if (!values[rowId].ContainsKey(columnId))
                    {
                        values[rowId].Add(columnId, "");
                    }

                    bool isArray = false;
                    propName = propertyNames[columnId];
                    propertyType = props.FirstOrDefault(_ => _.Name == propName)?.PropertyType;
                    if (propertyType == null)
                    {
                        propertyType = methods.FirstOrDefault(_ => _.Name == propName)?.ReturnType;
                        if (propertyType == null) continue;

                        isArray = true;
                    }

                    if (isArray || EditorHelper.IsEnumerable(propertyType))
                    {
                        if (isArray && propertyType == typeof(string))
                        {
                            var offsets = values[rowId][columnId].Split('_');
                            var offsetVector = offsets.Aggregate("", (current,
                                offset) => current + $"builder.CreateString(\"{offset}\"),");
                            offsetVector = offsetVector.Remove(offsetVector.Length - 1);
                            tempProperty += $"{fileName}.Create{propName}Vector(builder, new []{{{offsetVector}}}),";
                        }
                        else
                        {
                            tempProperty += $"{fileName}.Create{propName}Vector(builder, new []{{{values[rowId][columnId].Replace("_", ",")}}}),";
                        }
                    }
                    else if (propertyType == typeof(string))
                    {
                        tempProperty += $"builder.CreateString(\"{values[rowId][columnId]}\"),";
                    }
                    else
                    {
                        tempProperty += $"{values[rowId][columnId]},";
                    }
                }

                sortedData += $"{dataProperty.Replace("__property_data__", tempProperty.Remove(tempProperty.Length - 1))}";
            }

            dataAppend += $"var {fileName.ToLower()}_sortedvector =  {fileName}.CreateSortedVectorOf{fileName}(builder, new[] {{{sortedData.Remove(sortedData.Length - 2)}}});";
            return (dataAppend, typeClass.Namespace);
        }

        #region -- credential ------------------------------------------------

        private static UserCredential GetCredential()
        {
            var fullPath = Path.GetFullPath(EditorHelper.PACKAGES_PATH);
            if (!Directory.Exists(fullPath))
            {
                fullPath = Path.Combine(Application.dataPath, "Root");
            }

            fullPath += "/Spreadsheet/";
            var fi = new FileInfo(fullPath);
            var scriptFolder = fi.Directory?.ToString();
            var unused = scriptFolder?.Replace('\\', '/');
            UnityEngine.Debug.Log("Save Credential to: " + scriptFolder);

            UserCredential credential = null;
            var clientSecrets = new ClientSecrets {ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET};
            try
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets, Scopes, "user", CancellationToken.None, new FileDataStore(scriptFolder, true)).Result;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }

            return credential;
        }

        private static bool MyRemoteCertificateValidationCallback(object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            var isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            // ReSharper disable once InvertIf
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < chain.ChainStatus.Length; i++)
                {
                    // ReSharper disable once InvertIf
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        var chainIsValid = chain.Build((X509Certificate2) certificate);
                        // ReSharper disable once InvertIf
                        if (!chainIsValid)
                        {
                            UnityEngine.Debug.LogError("certificate chain is not valid");
                            isOk = false;
                        }
                    }
                }
            }

            return isOk;
        }

        #endregion

        #endregion
    }

    internal class SchemaWindow : SubWindow
    {
        private SubWindow[] _subs;
        private SubWindow _sub;
        private readonly string[] _menuName = {"Form namespace", "From fbs file"};
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
                new SchemaWindowA(parent),
                new SchemaWindowB(parent)
            };
        }

        private void SetCurrentWindow(SubWindow window)
        {
            _sub = window;
        }
    }

    internal class SchemaWindowA : SubWindow
    {
        private readonly List<Type> _types = new List<Type>();
        private bool _isFoldout;
        private string _namespaceSchema = "";
        private bool _isFindSuccess;
        private bool _isFoldoutType = true;
        private Vector2 _scrollPosition;
        private string _pathGenerate = "";
        private bool _isFetchPath;
        private bool _isFetchSaveNamespace;
        private int _indexOption;
        private readonly string _pathSchema = $"{Application.dataPath}/../Schemas";
        private readonly string[] _options = {"--gen-mutable"};

        public SchemaWindowA(EditorWindow window) : base("From namespace", window)
        {
        }

        public SchemaWindowA(string name,
            EditorWindow parent) : base(name, parent)
        {
        }

        public override void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUI.skin.scrollView);
            var style = EditorStyle.Get;
            EditorGUILayout.BeginHorizontal();
            if (!_isFetchSaveNamespace)
            {
                _isFetchSaveNamespace = true;
                if (EditorPrefs.HasKey(nameof(_namespaceSchema))) _namespaceSchema = EditorPrefs.GetString(nameof(_namespaceSchema));
            }

            EditorGUILayout.LabelField(new GUIContent("Namespace", "Namespace of raw file code"), style.widthLabel);
            _namespaceSchema = EditorGUILayout.TextField(_namespaceSchema);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUI.backgroundColor = Colors.Lavender;
            if (GUILayout.Button("Find all type of namespace")) OnTaskFind();
            GUI.backgroundColor = Colors.White;

            if (_isFindSuccess)
            {
                GUILayout.Space(8);
                _isFoldoutType = EditorGUILayout.Foldout(_isFoldoutType, "Types", EditorStyles.foldoutHeader);
                if (_isFoldoutType)
                {
                    EditorGUILayout.TextArea($"Below are all existing type in namespace. <b>Total {_types.Count} element.</b>", style.helpBox);
                    for (var i = 0; i < _types.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal(style.areaHorizontal);
                        EditorGUILayout.LabelField($"{GetTypeDefine(_types[i])} name", style.widthLabel);
                        EditorGUILayout.LabelField("", _types[i].Name, EditorStyles.textField);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
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

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Options", style.widthLabel);
                _indexOption = EditorGUILayout.Popup(_indexOption, _options, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);
                EditorHelper.Button("Generate code", style, GenerateSchema, Colors.Lavender);

                string GetTypeDefine(Type type)
                {
                    if (type.IsAbstract && !type.IsInterface)
                    {
                        return "Abstract class";
                    }

                    if (type.IsClass)
                    {
                        return "Class";
                    }

                    if (type.IsInterface)
                    {
                        return "Interface";
                    }

                    if (type.IsEnum)
                    {
                        return "Enum";
                    }

                    return type.IsValueType ? "Struct" : "";
                }

                void GenerateSchema()
                {
                    var pathG = _pathGenerate;
                    if (!EditorHelper.IsValidPath(pathG))
                    {
                        pathG = pathG.Insert(0, Application.dataPath);
                    }

                    UnityEngine.Debug.Log("Flatc generator : Start...");
                    var generator = new FlatBuffersSchemaGenerator();
                    var schema = generator.Create();
                    foreach (var type in _types)
                    {
                        schema.AddType(type);
                    }

                    var builder = new StringBuilder();
                    builder.AppendLine($"namespace FlatBufferGenerated.{_namespaceSchema};");
                    using (var writer = new StringWriter(builder))
                    {
                        schema.WriteTo(writer);
                    }

                    if (!Directory.Exists(_pathSchema))
                    {
                        Directory.CreateDirectory(_pathSchema);
                    }

                    var schemaFilePath = $"{_pathSchema}/{_namespaceSchema}_Schema.fbs";
                    File.WriteAllText(schemaFilePath, builder.ToString());

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
                        Arguments = $@" --csharp -o ""{pathG}"" ""{Path.GetFullPath(schemaFilePath)}"" {_options[_indexOption]}",
                    };

                    var p = Process.Start(psi);
                    if (p == null) return;
                    p.EnableRaisingEvents = true;
                    p.Exited += (sender,
                        e) =>
                    {
                        Directory.Delete(_pathSchema, true);
                        UnityEngine.Debug.Log("Flatc generator : Complete!");
                        p?.Dispose();
                        p = null;
                    };
                }
            }

            void OnTaskFind()
            {
                if (string.IsNullOrEmpty(_namespaceSchema))
                {
                    EditorUtility.DisplayDialog("Namespace Error", "Are you sure namespace is not empty?", "Ok");
                    return;
                }

                _isFindSuccess = false;
                _types.Clear();
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in assemblys)
                {
                    foreach (var type in item.Modules.First().GetTypes())
                    {
                        if (type.Namespace != null && type.Namespace.Equals(_namespaceSchema) && !type.IsInterface && !type.IsEnum)
                        {
                            _isFindSuccess = true;
                            _types.Add(type);
                        }
                    }
                }

                if (!_isFindSuccess)
                {
                    EditorUtility.DisplayDialog("Namespace Error", $"Are you sure namespace is correct or namespace now is empty?\nPlease check namespace again.", "Ok");
                }
                else
                {
                    EditorPrefs.SetString(nameof(_namespaceSchema), _namespaceSchema);
                }
            }

            EditorGUILayout.Space(EditorStyle.LAST_SPACE_SCROLL);
            EditorGUILayout.EndScrollView();
        }
    }

    internal class SchemaWindowB : SubWindow
    {
        private string _pathGenerate = "";
        private string _pathSchemaFile = "";
        private readonly string[] _options = {"--gen-mutable"};
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
        private string _searchFilter = "";
        private bool _searching;
        private string _pathBinaryFile;
        private Vector2 _scrollPosition;
        private bool _isFetchPath;
        private bool _isFetchSaveNamespace;
        private string _namespaceSchema;
        private Type _typeParser;
        private readonly List<ItemInfoCollapse> _itemInfo = new List<ItemInfoCollapse>();
        private string _masterName;
        private int _indexSelect;
        private string[] _menuItems;

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

            EditorGUILayout.BeginHorizontal();
            //drop down select class.
            if (EditorGUILayout.DropdownButton(new GUIContent(" " + (_menuItems != null ? _menuItems[_indexSelect] : "") + " "), FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                if (_menuItems != null)
                {
                    var menu = new GenericMenu();
                    for (int i = 0; i < _menuItems.Length; i++)
                    {
                        var index = i;
                        if (i != _indexSelect)
                            menu.AddItem(new GUIContent(_menuItems[i]), false, () =>
                            {
                                _indexSelect = index;
                                //todo
                            });
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(_menuItems[i]));
                        }
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.Space(6);
            _searchFilter = EditorGUILayout.TextField(_searchFilter, style.searchTextField, GUILayout.MinWidth(0));
            if (GUILayout.Button("", style.searchCancelButton))
            {
                _searchFilter = "";
                GUI.FocusControl(null);
            }

            _searching = _searchFilter != "";

            if (GUILayout.Button(" Save ", EditorStyles.toolbarButton))
            {
                // EditorLocalPrefs.Save(files[selectedFile]);
                // Refresh();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (!_isFetchPath)
            {
                _isFetchPath = true;
                if (EditorPrefs.HasKey(nameof(_pathBinaryFile))) _pathBinaryFile = EditorPrefs.GetString(nameof(_pathBinaryFile));
            }

            GUI.enabled = false;
            EditorGUILayout.LabelField(".wr binary file", style.widthLabel);
            EditorGUILayout.LabelField("", _pathBinaryFile, EditorStyles.textField);
            GUI.enabled = true;
            GUI.backgroundColor = Colors.Cornsilk;
            if (GUILayout.Button(new GUIContent("", "Select file"), EditorStyles.colorField, GUILayout.Width(18), GUILayout.Height(18)))
            {
                var path = EditorUtility.OpenFilePanel("Select file", _pathBinaryFile, "wr");
                if (!string.IsNullOrEmpty(path))
                {
                    _pathBinaryFile = path;
                    _masterName = "";
                    if (!string.IsNullOrEmpty(nameof(_pathBinaryFile)))
                    {
                        EditorPrefs.SetString(nameof(_pathBinaryFile), _pathBinaryFile);
                    }
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Colors.White;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (!_isFetchSaveNamespace)
            {
                _isFetchSaveNamespace = true;
                if (EditorPrefs.HasKey(nameof(_namespaceSchema))) _namespaceSchema = EditorPrefs.GetString(nameof(_namespaceSchema));
            }

            EditorGUILayout.LabelField(new GUIContent("Namespace", "Namespace of raw file code"), style.widthLabel);
            _namespaceSchema = EditorGUILayout.TextField(_namespaceSchema);
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_pathBinaryFile))
            {
                if (string.IsNullOrEmpty(_masterName))
                {
                    _masterName = Path.GetFileNameWithoutExtension(_pathBinaryFile).Split('_')[0];
                    _typeParser = TypeUtil.GetTypeByName($"{_masterName}Edit");
                    var assemblys = AppDomain.CurrentDomain.GetAssemblies();

                    Type GetTypeRawMaster()
                    {
                        foreach (var item in assemblys)
                        {
                            foreach (var type in item.Modules.First().GetTypes())
                            {
                                if (type.Namespace == null || !type.Namespace.Equals(_namespaceSchema) || type.IsInterface || type.IsEnum) continue;
                                if (type.Name.Equals(_masterName))
                                {
                                    return type;
                                }
                            }
                        }

                        return null;
                    }

                    string NameFieldInMaster(Type type,
                        ref Type masterTable)
                    {
                        if (masterTable == null)
                        {
                            return "";
                        }

                        foreach (var info in masterTable.GetProperties())
                        {
                            if (info.PropertyType == type || EditorHelper.IsEnumerable(info.PropertyType) && type.IsAssignableFrom(info.PropertyType.GetElementType()))
                            {
                                return info.Name;
                            }
                        }

                        return "";
                    }

                    var typeRawMaster = GetTypeRawMaster();

                    foreach (var item in assemblys)
                    {
                        foreach (var type in item.Modules.First().GetTypes())
                        {
                            if (type.Namespace == null || !type.Namespace.Equals(_namespaceSchema) || type.IsInterface || type.IsEnum) continue;
                            if (type.Name.Equals(_masterName)) continue;

                            var propInfos = type.GetProperties();
                            var propertyData = new KeyValuePair<string, Type>[propInfos.Length];
                            for (int i = 0; i < propInfos.Length; i++)
                            {
                                propertyData[i] = new KeyValuePair<string, Type>(propInfos[i].Name, propInfos[i].PropertyType);
                            }

                            _itemInfo.Add(new ItemInfoCollapse(type.Name, propertyData, NameFieldInMaster(type, ref typeRawMaster)));
                        }
                    }

                    _menuItems = new string[_itemInfo.Count];
                    for (int i = 0; i < _itemInfo.Count; i++)
                    {
                        _menuItems[i] = _itemInfo[i].Name;
                    }
                }

                if (GUILayout.Button("Make Viewer")) OnTaskMakeViewer();

                if (_typeParser != null)
                {
                    if (GUILayout.Button("Parser")) OnTaskParser();
                }

                void OnTaskMakeViewer()
                {
                    if (string.IsNullOrEmpty(_pathBinaryFile))
                    {
                        EditorUtility.DisplayDialog("Path Error", "Are you sure path is not empty?", "Ok");
                        return;
                    }

                    var scriptViewer = EditorHelper.GetTemplateByName("GameDatabaseViewTemplate");
                    var scriptsPath = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH)) : EditorHelper.DEFAULT_SPREADSHEET_GENERATE_CODE_PATH;

                    if (!EditorHelper.IsValidPath(scriptsPath))
                    {
                        scriptsPath = scriptsPath.Insert(0, Application.dataPath);
                    }

                    if (!Directory.Exists(scriptsPath))
                    {
                        Directory.CreateDirectory(scriptsPath);
                    }

                    var dataAppend = "";
                    for (int i = 0; i < _itemInfo.Count; i++)
                    {
                        dataAppend += $"if(itemInfoCollapse.Name == \"{_itemInfo[i].Name}\"){{\n";
                        dataAppend += $"results = new object[dataTable.{_itemInfo[i].NameMasterField}Length, itemInfoCollapse.NameTypeField.Length];\n";
                        dataAppend += $"for (int i = 0; i < dataTable.{_itemInfo[i].NameMasterField}Length; i++){{\n";
                        dataAppend += $"var idata = dataTable.{_itemInfo[i].NameMasterField}(i);\n";
                        dataAppend += "if (idata != null){\n";

                        int j = 0;
                        foreach (var pair in _itemInfo[i].NameTypeField)
                        {
                            if (EditorHelper.IsEnumerable(pair.Value))
                            {
                                dataAppend += $"var __{pair.Key.ToLower()} = new object[idata.Value.{pair.Key}Length];\n";
                                dataAppend += $"for (int j = 0; j < idata.Value.{pair.Key}Length; j++){{\n";
                                dataAppend += $"__{pair.Key.ToLower()}[j] = idata.Value.{pair.Key}(j);}}\n";
                                dataAppend += $"results[i, {j}] =  __{pair.Key.ToLower()};\n";
                            }
                            else
                            {
                                dataAppend += $"results[i, {j}] = idata.Value.{pair.Key};\n";
                            }

                            j++;
                        }

                        dataAppend += "\n}}}\n";
                    }

                    scriptViewer = scriptViewer.Replace("__namespace__", $"FlatBufferGenerated.{_namespaceSchema}").Replace("__name__", _masterName).Replace("__data_replace__", dataAppend);

                    EditorHelper.WriteToFile($"{scriptsPath}/{_masterName}Edit.cs".Replace("/", "\\"), scriptViewer);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    UnityEngine.Debug.Log($"<color=#25854B>Create view success!</color>");
                }

                void OnTaskParser()
                {
                    var byteBuffer = FlatHelper.Load(_pathBinaryFile);

                    var t = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).FirstOrDefault(type => type.Name == _masterName && type.Namespace != _namespaceSchema);

                    var data = t?.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == $"GetRootAs{_masterName}").FirstOrDefault(x => x.GetParameters().Length == 1)?.Invoke(null, new object[] {byteBuffer});

                    for (int i = 0; i < _itemInfo.Count; i++)
                    {
                        var results = (object[,]) _typeParser?.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "Execute").FirstOrDefault(x => x.GetParameters().Length == 2)?.Invoke(null, new object[] {data, _itemInfo[i]});
                    }
                }
            }

            EditorGUILayout.Space(EditorStyle.LAST_SPACE_SCROLL);
            EditorGUILayout.EndScrollView();
        }

        #region ItemView

        internal interface IItemGui
        {
            void Expand();
            void Collapse();
        }

        // internal struct ItemGui<T> : IItemGui
        // {
        //     internal struct Changes
        //     {
        //         public T currentValue;
        //         public T newValue;
        //         public bool valueChanged;
        //
        //         public Changes(T currentValue, T newValue, bool valueChanged)
        //         {
        //             this.currentValue = currentValue;
        //             this.newValue = newValue;
        //             this.valueChanged = valueChanged;
        //         }
        //     }
        //
        //     private AnimBool collapse;
        //     private readonly string _keyCollapse;
        //     private readonly Type type;
        //     private readonly string label;
        //     private readonly string valueName;
        //     private readonly int _index;
        //     private List<Changes> changes;
        //     private int foundItemsCount;
        //     private bool nothingFound;
        //     private bool[] showItem;
        //
        //     public ItemGui(int index, ref EditorWindow window, string customLabel = null, string customValueName = null)
        //     {
        //         _index = index;
        //         type = typeof(T);
        //         label = customLabel ?? type.Name;
        //         valueName = customValueName ?? label;
        //         _keyCollapse = $"{type.Name}_{index}_collapsed";
        //         collapse = new AnimBool(window.Repaint) {speed = 3.5f, target = EditorPrefs.GetBool(_keyCollapse)};
        //         changes = new List<Changes>();
        //         foundItemsCount = 0;
        //         nothingFound = false;
        //         showItem = new bool[0];
        //     }
        //
        //     public void Expand()
        //     {
        //         EditorPrefs.SetBool(_keyCollapse, true);
        //     }
        //
        //     public void Collapse()
        //     {
        //         EditorPrefs.SetBool(_keyCollapse, false);
        //     }
        //
        //     public T DoLayout(T itemData, Type itemType)
        //     {
        //         string filter = _searchFilter.ToLower();
        //         nothingFound = false;
        //         if (!_searching)
        //             foundItemsCount = itemType.GetProperties();
        //         else
        //         {
        //             // showItem = new bool[prefs.dictionary.Count];
        //             // foundItemsCount = 0;
        //             // int i = 0;
        //             // foreach (var value in prefs.dictionary)
        //             // {
        //             //     if (value.Key.ToLower().Contains(filter))
        //             //     {
        //             //         showItem[i] = true;
        //             //         foundItemsCount++;
        //             //     }
        //             //
        //             //     i++;
        //             // }
        //             //
        //             // nothingFound = foundItemsCount == 0;
        //         }
        //
        //         if (window == null || script == null || nothingFound)
        //             return prefs;
        //         shownTypesCount++;
        //         changes.Clear();
        //         EditorGUILayout.BeginVertical(regionBg, GUILayout.MaxHeight(18));
        //         EditorGUILayout.BeginHorizontal();
        //
        //         EditorGUI.BeginChangeCheck();
        //         collapse.target = GUILayout.Toggle(searching ? true : EditorLocalPrefs.GetBool(PN_collapse),
        //             label, foldout, GUILayout.ExpandWidth(true));
        //         if (EditorGUI.EndChangeCheck())
        //         {
        //             EditorLocalPrefs.SetBool(PN_collapse, collapse.target);
        //         }
        //
        //         GUILayout.FlexibleSpace();
        //         GUILayout.Button("[" + foundItemsCount + "]", EditorStyles.centeredGreyMiniLabel);
        //
        //         if (GUILayout.Button(new GUIContent("", !searching ? "Clear All" : "Remove Found Items"), "WinBtnCloseMac"))
        //         {
        //             if (!searching)
        //                 prefs.ClearAll();
        //             else
        //             {
        //                 int i = 0;
        //                 foreach (var value in prefs.dictionary)
        //                 {
        //                     if (!showItem[i])
        //                     {
        //                         i++;
        //                         continue;
        //                     }
        //                     else
        //                         changes.Add(new Changes(value.Key, "", default, false, false, true, 0));
        //
        //                     i++;
        //                 }
        //             }
        //
        //             EditorUtility.SetDirty(script);
        //         }
        //
        //         if (GUILayout.Button(new GUIContent("", "Add New Item"), "WinBtnMaxMac"))
        //         {
        //             int keysCount = prefs.Length;
        //             string newItemKey = "New " + valueName + " ";
        //             while (prefs.ContainsKey(newItemKey + keysCount))
        //                 keysCount++;
        //
        //             prefs.Add(newItemKey + keysCount, default);
        //             EditorUtility.SetDirty(script);
        //         }
        //
        //         EditorGUILayout.EndHorizontal();
        //         if (EditorGUILayout.BeginFadeGroup(collapse.faded))
        //         {
        //             if (prefs.Count > 0)
        //             {
        //                 int i = 0;
        //                 foreach (var value in prefs.dictionary)
        //                 {
        //                     if (searching && !showItem[i])
        //                     {
        //                         i++;
        //                         continue;
        //                     }
        //
        //                     EditorGUILayout.BeginHorizontal();
        //                     bool keyChanged = false;
        //                     bool keyChanged = false;
        //                     bool valueChanged = false;
        //                     EditorGUI.BeginChangeCheck();
        //                     var newKey = EditorGUILayout.DelayedTextField(value.Key);
        //                     if (EditorGUI.EndChangeCheck())
        //                         keyChanged = true;
        //
        //                     dynamic newValue = value.Value;
        //                     EditorGUI.BeginChangeCheck();
        //
        //                     if (type == typeof(bool))
        //                         newValue = GUILayout.Toggle(newValue, newValue ? "True" : "False", EditorStyles.miniButton);
        //
        //                     if (type == typeof(int))
        //                         newValue = EditorGUILayout.IntField(newValue);
        //
        //                     if (type == typeof(float))
        //                         newValue = EditorGUILayout.FloatField(newValue);
        //
        //                     if (type == typeof(Vector2))
        //                         newValue = EditorGUILayout.Vector2Field(GUIContent.none, newValue);
        //
        //                     if (type == typeof(Vector3))
        //                         newValue = EditorGUILayout.Vector3Field(GUIContent.none, newValue);
        //
        //                     if (type == typeof(Vector4))
        //                         newValue = EditorGUILayout.Vector4Field(GUIContent.none, newValue);
        //
        //                     if (type == typeof(string))
        //                         newValue = EditorGUILayout.TextField(newValue);
        //
        //                     if (EditorGUI.EndChangeCheck())
        //                         valueChanged = true;
        //
        //                     // Remove button
        //                     bool remove = GUILayout.Button("", macRemoveButton);
        //                     EditorGUILayout.EndHorizontal();
        //                     // Write changes
        //                     if (keyChanged || valueChanged || remove)
        //                         changes.Add(new Changes(value.Key, newKey, newValue, keyChanged, valueChanged, remove, i));
        //                     i++;
        //                 }
        //             }
        //             else
        //             {
        //                 EditorGUILayout.HelpBox("List is empty.", MessageType.Info);
        //             }
        //         }
        //
        //         // Apply changes
        //         for (int c = 0; c < changes.Count; c++)
        //         {
        //             var change = changes[c];
        //             if (change.remove)
        //             {
        //                 prefs.dictionary.Remove(change.currentKey);
        //                 continue;
        //             }
        //
        //             if (change.keyChanged)
        //             {
        //                 prefs.dictionary.Remove(change.currentKey);
        //                 prefs.dictionary.Add(change.newKey, change.value);
        //             }
        //
        //             if (change.valueChanged)
        //             {
        //                 prefs.dictionary[change.newKey] = change.value;
        //             }
        //         }
        //
        //         if (changes.Count > 0)
        //             EditorUtility.SetDirty(script);
        //         EditorGUILayout.EndFadeGroup();
        //         EditorGUILayout.EndVertical();
        //         return prefs;
        //     }
        // }

        #endregion
    }

    internal class CreatorWindow : EditorWindow
    {
        private string _nameSpace;
        private string _name;
        private int _typeIndex;
        private int _typeInheritanceIndex;
        private string _pathCreator;
        private bool _isFetchPath;
        private readonly string[] _types = {"struct", "class", "enum"};

        private readonly string[] _enumInheritanceType = {"none", "byte", "int", "long", "sbyte", "short", "uint", "ulong", "ushort"};

        //private readonly string[] _typeCollections = {"sbyte", "byte", "bool", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "enum", "string", "struct", "class"};
        private const string TEMPLATE_WIDTH_NAMESPACE = @"namespace __namespace__
{
    public __type__ __name__
    {

    }
}";

        public static void ShowWindow()
        {
            var window = GetWindow(typeof(CreatorWindow));
            window.titleContent = new GUIContent("Scripts Creator");
            window.minSize = new Vector2(420, 150);
        }

        public void OnGUI()
        {
            var style = EditorStyle.Get;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Namespace", style.widthLabel);
            _nameSpace = EditorGUILayout.TextField(_nameSpace);
            if (string.IsNullOrEmpty(_nameSpace))
            {
                GUI.color = Colors.Red;
                GUILayout.Label(new GUIContent("[*]", "Can not be null or empty!"), GUILayout.Width(20));
                GUI.color = Colors.White;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name", style.widthLabel);
            _name = EditorGUILayout.TextField(_name);
            if (string.IsNullOrEmpty(_name))
            {
                GUI.color = Colors.Red;
                GUILayout.Label(new GUIContent("[*]", "Can not be null or empty!"), GUILayout.Width(20));
                GUI.color = Colors.White;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Type", style.widthLabel);
            _typeIndex = EditorGUILayout.Popup(_typeIndex, _types, GUILayout.Width(80));

            if (_types[_typeIndex].Equals("enum"))
            {
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Inheritance", style.widthLabel);
                _typeInheritanceIndex = EditorGUILayout.Popup(_typeInheritanceIndex, _enumInheritanceType, GUILayout.Width(80));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Path", style.widthLabel);
            if (!_isFetchPath)
            {
                _isFetchPath = true;
                _pathCreator = EditorPrefs.HasKey(nameof(EditorHelper.DEFAULT_SCRIPTS_CREATOR_PATH)) ? EditorPrefs.GetString(nameof(EditorHelper.DEFAULT_SCRIPTS_CREATOR_PATH)) : EditorHelper.DEFAULT_SCRIPTS_CREATOR_PATH;
            }

            GUI.enabled = false;
            EditorGUILayout.TextField(_pathCreator);
            GUI.enabled = true;

            EditorHelper.PickFolderPath(ref _pathCreator, nameof(EditorHelper.DEFAULT_SCRIPTS_CREATOR_PATH));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorUtil.DrawUiLine(Colors.PaleGreen);
            if (!string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_nameSpace))
            {
                EditorHelper.Button("Create", style, Create, Colors.Lavender);
            }

            void Create()
            {
                var pathG = _pathCreator;
                if (!EditorHelper.IsValidPath(pathG))
                {
                    pathG = pathG.Insert(0, Application.dataPath);
                }

                var result = TEMPLATE_WIDTH_NAMESPACE;
                result = result.Replace("__type__", _types[_typeIndex]).Replace("__namespace__", _nameSpace);

                if (_types[_typeIndex].Equals("enum") && !_enumInheritanceType[_typeInheritanceIndex].Equals("none"))
                {
                    result = result.Replace("__name__", $"{_name} : {_enumInheritanceType[_typeInheritanceIndex]}");
                }
                else
                {
                    result = result.Replace("__name__", _name);
                }

                EditorHelper.WriteToFile($@"{pathG}\{_name}.cs", result);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _name = "";
                _typeInheritanceIndex = 0;
            }
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

    public struct ItemInfoCollapse
    {
        public string Name { get; set; }
        public KeyValuePair<string, Type>[] NameTypeField { get; set; }
        public string NameMasterField { get; set; } //Name field in master table is contain array data of that item type

        public ItemInfoCollapse(string name,
            KeyValuePair<string, Type>[] nameTypeField,
            string nameMasterField)
        {
            Name = name;
            NameTypeField = nameTypeField;
            NameMasterField = nameMasterField;
        }
    }
}

#endif