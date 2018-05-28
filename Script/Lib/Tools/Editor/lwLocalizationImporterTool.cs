// Copyright 2016 Bigpoint, Lyon
//
// Thanks: Sylvain Minjard <s.minjard@bigpoint.net>
//
// Date: 2017/03/20

using UnityEngine;
using UnityEditor;

using OfficeOpenXml;

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Localization importer tool.
/// Convert an excel file into one or many csv files
/// </summary>
public class lwLocalizationImporterTool : EditorWindow
{
	[MenuItem( "Tools/Rerolled/Localization Importer (Excel to CSV)", false, 1 )]
	public static lwLocalizationImporterTool CreateOrGetWindow()
	{
		lwLocalizationImporterTool window = EditorWindow.GetWindow<lwLocalizationImporterTool>();
		window.minSize = new Vector2(825.0f, 400.0f);
		window.ShowUtility();

		return window;
	}

#region Unity callbacks
	private void OnEnable()
	{
		titleContent = new GUIContent( "Localization Importer" );

		m_excelPackage = null;
		m_selectedWorksheets = new HashSet<ExcelWorksheet>();
		m_nFirstLanguageColumnIndexes = new List<int>();
		m_sLanguageCodes = new List<string>();
		m_sSelectedLanguageCodes = new HashSet<string>();

		LoadPrefs();

		ScanExcelFiles();
		if( m_excelPackage!= null )
		{
			ScanLanguages();
		}
	}

	private void OnDisable()
	{
		if( m_excelPackage!=null )
		{
			m_excelPackage.Dispose();
			m_excelPackage = null;
		}

		SavePrefs();
	}

	private void OnGUI()
	{		
		EditorGUILayout.Space();

		EditorGUI.BeginChangeCheck();
		m_sExcelFilePath = lwEditorGUILayout.FileSelection( "Excel source", m_sExcelFilePath, new string[]{ "Excel files", "xls,xlsx" }, lwPathUtil.PathType.Any, "Select source file" );
		if( EditorGUI.EndChangeCheck() )
		{
			OnExcelFileChanged();
			ScanExcelFiles();
		}

		if( GUILayout.Button( "Scan excel file", GUILayout.ExpandWidth( false ) ) )
		{
			ScanExcelFiles();
		}

		EditorGUILayout.Space();

		if( m_excelPackage!=null )
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical();
				{
					OnWorksheetSelectionGUI();

					EditorGUI.BeginDisabledGroup( m_selectedWorksheets.Count==0 );
					if( GUILayout.Button( "Scan languages", GUILayout.ExpandWidth( false ) ) )
					{
						ScanLanguages();
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				{
					// Display toggle for each language
					if( m_sLanguageCodes.Count>0 )
					{
						OnLanguageSelectionGUI();
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();

			if( m_selectedWorksheets.Count>0 && m_sSelectedLanguageCodes.Count>0 )
			{
				EditorGUILayout.Space();
				OnImportPanelGUI();
			}
		}

		EditorGUILayout.Space();
	}
#endregion

#region Private
	#region Declarations
	private static readonly string s_windowPreferencesPrefix = "LWS/LocalizationImporterTool/";

	private enum ExportDestinationFileOption
	{
		OneFilePerLanguage,
		OneFilePerLanguagePerSheet,
		OneFilePerSheetInLanguageFolder,
	}
	#endregion

	#region Methods
	private void LoadPrefs()
	{
		m_sExcelFilePath = EditorPrefs.GetString( s_windowPreferencesPrefix+"m_excelFilePath", "" );

		m_exportDestinationFileOption = lwParseTools.ParseEnumSafe<ExportDestinationFileOption>( EditorPrefs.GetString( s_windowPreferencesPrefix+"m_exportDestinationFileOption", ExportDestinationFileOption.OneFilePerLanguagePerSheet.ToString() ), ExportDestinationFileOption.OneFilePerLanguagePerSheet );
		m_sDestinationFolderPath = EditorPrefs.GetString( s_windowPreferencesPrefix+"m_destinationFolderPath", "" );
		if( System.IO.Directory.Exists( m_sDestinationFolderPath )==false )
		{
			m_sDestinationFolderPath = "";
		}
		m_sDestinationFileExtension = EditorPrefs.GetString( s_windowPreferencesPrefix+"m_destinationFileExtension", "csv" );

		m_sExoticCharacterFolderPath = EditorPrefs.GetString( s_windowPreferencesPrefix+"m_exoticCharacterFolderPath", "" );
		if( System.IO.Directory.Exists( m_sExoticCharacterFolderPath )==false )
		{
			m_sExoticCharacterFolderPath = "";
		}
	}

	private void SavePrefs()
	{
		EditorPrefs.SetString( s_windowPreferencesPrefix+"m_excelFilePath", m_sExcelFilePath );

		EditorPrefs.SetString( s_windowPreferencesPrefix+"m_exportDestinationFileOption", m_exportDestinationFileOption.ToString() );
		EditorPrefs.SetString( s_windowPreferencesPrefix+"m_destinationFolderPath", m_sDestinationFolderPath );
		EditorPrefs.SetString( s_windowPreferencesPrefix+"m_destinationFileExtension", m_sDestinationFileExtension );

		EditorPrefs.SetString( s_windowPreferencesPrefix+"m_exoticCharacterFolderPath", m_sExoticCharacterFolderPath );
	}

	private void OnWorksheetSelectionGUI()
	{
		EditorGUI.BeginChangeCheck();
		
		++EditorGUI.indentLevel;

		EditorGUILayout.BeginHorizontal( EditorStyles.toolbar, GUILayout.ExpandWidth( true ) );
		{
			GUILayout.Label( "Select sheets ", EditorStyles.miniLabel, GUILayout.ExpandWidth( false ) );
			if( GUILayout.Button( "All", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
			{
				IEnumerator<ExcelWorksheet> worksheetEnumerator = m_excelPackage.Workbook.Worksheets.GetEnumerator();
				while( worksheetEnumerator.MoveNext() )
				{
					m_selectedWorksheets.Add( worksheetEnumerator.Current );
				}
			}
			if( GUILayout.Button( "None", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
			{
				m_selectedWorksheets.Clear();
			}
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();

		// Display toggle for each worksheet
		{
			IEnumerator<ExcelWorksheet> worksheetEnumerator = m_excelPackage.Workbook.Worksheets.GetEnumerator();
			while( worksheetEnumerator.MoveNext() )
			{
				bool bWasSelected = m_selectedWorksheets.Contains( worksheetEnumerator.Current );
				bool bIsSelected = EditorGUILayout.Toggle( worksheetEnumerator.Current.Name, bWasSelected );

				if( bWasSelected!=bIsSelected )
				{
					if( bIsSelected )
					{
						m_selectedWorksheets.Add( worksheetEnumerator.Current );
					}
					else
					{
						m_selectedWorksheets.Remove( worksheetEnumerator.Current );
					}
				}
			}
		}

		--EditorGUI.indentLevel;

		if( EditorGUI.EndChangeCheck() )
		{
			ExcelWorksheet[] selectedWorksheets = new ExcelWorksheet[m_selectedWorksheets.Count];
			m_selectedWorksheets.CopyTo( selectedWorksheets );
			string[] sSelectedWorksheetNames = System.Array.ConvertAll( selectedWorksheets, item => item.Name );
			EditorPrefs.SetString( s_windowPreferencesPrefix+"selectedWorksheetNames", string.Join( ";", sSelectedWorksheetNames ) );

		}
	}

	private void OnLanguageSelectionGUI()
	{
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.BeginHorizontal( EditorStyles.toolbar, GUILayout.ExpandWidth( true ) );
		{
			GUILayout.Label( "Select languages ", EditorStyles.miniLabel, GUILayout.ExpandWidth( false ) );
			if( GUILayout.Button( "All", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
			{
				m_sSelectedLanguageCodes.UnionWith( m_sLanguageCodes );
			}
			if( GUILayout.Button( "None", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
			{
				m_sSelectedLanguageCodes.Clear();
			}
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();

		m_v2LanguageSelectionScrollPosition = EditorGUILayout.BeginScrollView( m_v2LanguageSelectionScrollPosition, false, false );
		{
			++EditorGUI.indentLevel;

			for( int nLanguageIndex = 0; nLanguageIndex<m_sLanguageCodes.Count; ++nLanguageIndex )
			{
				lwCountry countryStructure = lwCountryCode.GetlwCountry( m_sLanguageCodes[nLanguageIndex] );
				Color oldContentColor = GUI.contentColor;
				GUI.contentColor = countryStructure == null? Color.red : Color.white;
				
				bool bWasSelected = m_sSelectedLanguageCodes.Contains( m_sLanguageCodes[nLanguageIndex] );
				bool bIsSelected = EditorGUILayout.Toggle( m_sLanguageCodes[nLanguageIndex], bWasSelected );

				if( bWasSelected!=bIsSelected )
				{
					if( bIsSelected )
					{
						m_sSelectedLanguageCodes.Add( m_sLanguageCodes[nLanguageIndex] );
					}
					else
					{
						m_sSelectedLanguageCodes.Remove( m_sLanguageCodes[nLanguageIndex] );
					}
				}

				GUI.contentColor = oldContentColor;
			}

			--EditorGUI.indentLevel;
		}
		EditorGUILayout.EndScrollView();

		if( EditorGUI.EndChangeCheck() )
		{
			string[] sSelectedLanguages = new string[m_sSelectedLanguageCodes.Count];
			m_sSelectedLanguageCodes.CopyTo( sSelectedLanguages );			
			EditorPrefs.SetString( s_windowPreferencesPrefix+"selectedLanguages", string.Join( ";", sSelectedLanguages ) );
		}
	}

	private void OnImportPanelGUI()
	{
		EditorGUILayout.BeginHorizontal( EditorStyles.toolbar, GUILayout.ExpandWidth( true ) );
		{
			GUILayout.Label( "Import options", EditorStyles.miniLabel, GUILayout.ExpandWidth( false ) );
			GUILayout.FlexibleSpace();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUI.BeginChangeCheck();
		m_exportDestinationFileOption = ( ExportDestinationFileOption )EditorGUILayout.EnumPopup( "Destination", m_exportDestinationFileOption );
		m_sDestinationFolderPath = lwEditorGUILayout.FolderSelection( "Destination folder", m_sDestinationFolderPath, lwPathUtil.PathType.Asset );
		if( EditorGUI.EndChangeCheck() )
		{
			if( System.IO.Directory.Exists( m_sDestinationFolderPath )==false )
			{
				m_sDestinationFolderPath = "";
			}

			SavePrefs();
		}
		EditorGUI.BeginChangeCheck();
		m_sDestinationFileExtension = EditorGUILayout.TextField( "File extension", m_sDestinationFileExtension, GUILayout.ExpandWidth( false ) );
		if( EditorGUI.EndChangeCheck() )
		{
			Regex allowedCharacterRegExpression = new Regex( "[^a-zA-Z0-9]" );
			m_sDestinationFileExtension = allowedCharacterRegExpression.Replace( m_sDestinationFileExtension, "" );
			SavePrefs();
		}

		// check for exotic languages
		HashSet<string>.Enumerator selectedLanguageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
		bool bHasExoticLanguage = false;
		List<string> missingLanguageCodes = new List<string>();
		while( selectedLanguageEnumerator.MoveNext() )
		{
			lwCountry countryStructure = lwCountryCode.GetlwCountry( selectedLanguageEnumerator.Current );
			if( countryStructure==null )
			{
				missingLanguageCodes.Add( selectedLanguageEnumerator.Current );
			}
			else
			{
				bHasExoticLanguage |= countryStructure.m_bIsLatin==false;
			}
		}

		if( missingLanguageCodes.Count>0 )
		{
			EditorGUILayout.HelpBox( string.Format( "Some selected languages ({0}) do not exist in the language manager. Please notify a programmer to add it.", string.Join( ", ", missingLanguageCodes.ToArray() ) ), MessageType.Error );
		}
		else if( bHasExoticLanguage )
		{
			EditorGUILayout.HelpBox( "There are exotic languages among the selection. Therefore, the process will check for unique characters.", MessageType.Info );

			EditorGUI.BeginChangeCheck();
			m_sExoticCharacterFolderPath = lwEditorGUILayout.FolderSelection( "Exotic character folder", m_sExoticCharacterFolderPath, lwPathUtil.PathType.Any );
			if( EditorGUI.EndChangeCheck() )
			{
				if( System.IO.Directory.Exists( m_sExoticCharacterFolderPath )==false )
				{
					m_sExoticCharacterFolderPath = "";
				}

				SavePrefs();
			}
		}

		EditorGUILayout.Space();

		GUILayout.FlexibleSpace();
		{
			System.IO.FileInfo xlsFile = new System.IO.FileInfo( m_sExcelFilePath );
			System.DateTime excelFileLastWriteTime = xlsFile.LastWriteTime;
			bool hasSameLastWriteTime = excelFileLastWriteTime == m_excelFileLastWriteTime;
			if(hasSameLastWriteTime == false)
			{
				EditorGUILayout.HelpBox("The source excel file may have been modified since the last scan of the file.", MessageType.Warning);
			}

			Color oldBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = hasSameLastWriteTime? Color.green : Color.yellow;

			EditorGUI.BeginDisabledGroup( m_selectedWorksheets.Count==0 || m_sSelectedLanguageCodes.Count==0
				|| System.IO.Directory.Exists( m_sDestinationFolderPath )==false || ( bHasExoticLanguage && System.IO.Directory.Exists( m_sExoticCharacterFolderPath )==false )
				|| string.IsNullOrEmpty( m_sDestinationFileExtension ) );
			if( GUILayout.Button( "Import", GUILayout.ExpandWidth( true ) ) )
			{
				DoImportTexts();
			}
			EditorGUI.EndDisabledGroup();

			GUI.backgroundColor = oldBackgroundColor;
		}

		EditorGUILayout.Space();
	}

	private void OnExcelFileChanged()
	{
		EditorPrefs.DeleteKey( s_windowPreferencesPrefix+"selectedWorksheetNames" );
		EditorPrefs.DeleteKey( s_windowPreferencesPrefix+"selectedLanguages" );

		m_excelFileLastWriteTime = new System.DateTime();
	}

	private void ScanExcelFiles()
	{
		m_v2LanguageSelectionScrollPosition = Vector2.zero;

		m_sSelectedLanguageCodes.Clear();
		m_sLanguageCodes.Clear();
		m_selectedWorksheets.Clear();		
		m_nFirstLanguageColumnIndexes.Clear();
		if( m_excelPackage!=null )
		{
			m_excelPackage.Dispose();
			m_excelPackage = null;
		}

		if( string.IsNullOrEmpty( m_sExcelFilePath )==false && System.IO.File.Exists( m_sExcelFilePath ) )
		{
			try
			{
				EditorUtility.DisplayProgressBar( "Localization importer", "Scanning worksheets...", 0.0f );

				System.IO.FileInfo xlsFile = new System.IO.FileInfo( m_sExcelFilePath );
				m_excelPackage = new ExcelPackage( xlsFile );

				m_excelFileLastWriteTime = xlsFile.LastWriteTime;

				string[] sSelectedWorksheetNamesFromPreferences = EditorPrefs.GetString( s_windowPreferencesPrefix+"selectedWorksheetNames", "" ).Split( new char[]{ ';' }, System.StringSplitOptions.RemoveEmptyEntries );

				IEnumerator<ExcelWorksheet> worksheetEnumerator = m_excelPackage.Workbook.Worksheets.GetEnumerator();
				while( worksheetEnumerator.MoveNext() )
				{
					if( sSelectedWorksheetNamesFromPreferences.Length==0 || System.Array.IndexOf( sSelectedWorksheetNamesFromPreferences, worksheetEnumerator.Current.Name )>=0 )
					{
						m_selectedWorksheets.Add( worksheetEnumerator.Current );
					}
					m_nFirstLanguageColumnIndexes.Add( 0 );

					EditorUtility.DisplayProgressBar( "Localization importer", "Scanning worksheets...", m_selectedWorksheets.Count/( float )m_excelPackage.Workbook.Worksheets.Count );
				}

				SavePrefs();
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}
	}

	private void ScanLanguages()
	{
		try
		{
			ExcelWorksheet[] worksheets = new ExcelWorksheet[m_selectedWorksheets.Count];
			m_selectedWorksheets.CopyTo( worksheets );

			string sProgressBarTitle = "Scan languages";
			string sProgressBarMessage = "Scanning languages in '{0}'";
			bool bHasBeenCancelled = EditorUtility.DisplayCancelableProgressBar( sProgressBarTitle, string.Format( sProgressBarMessage, worksheets[0].Name ), 0.0f );

			Regex regExpression = new Regex( "[a-zA-Z][a-zA-Z]-[a-zA-Z][a-zA-Z]" );

			// Parse the first worksheet to get all languages
			m_sSelectedLanguageCodes.Clear();
			m_sLanguageCodes.Clear();
			List<string> languageCodesToAdd = new List<string>();

			int nColumnIndex = 1;
			while( nColumnIndex<=worksheets[0].Dimension.Columns && bHasBeenCancelled==false )
			{
				string sColumnHeaderText = worksheets[0].GetValue<string>( 1, nColumnIndex );
				if( string.IsNullOrEmpty( sColumnHeaderText )==false && regExpression.IsMatch( sColumnHeaderText ) )
				{
					if( m_sLanguageCodes.Contains( sColumnHeaderText ) )
					{
						bHasBeenCancelled = EditorUtility.DisplayDialog( "Scan languages", string.Format( "The language code '{0}' is defined in more than 1 column in worksheet '{1}'.", sColumnHeaderText, worksheets[0].Name ), "Ok", "Cancel" )==false;
					}
					else
					{
						if( m_nFirstLanguageColumnIndexes[0]==0 )
						{
							m_nFirstLanguageColumnIndexes[0] = nColumnIndex;
						}
						m_sLanguageCodes.Add( sColumnHeaderText );
					}
				}

				++nColumnIndex;
			}

			if( bHasBeenCancelled==false && m_sLanguageCodes.Count==0 )
			{
				EditorUtility.DisplayDialog( "Scan languages", string.Format( "No language code has been found in the worksheet '{0}'.", worksheets[0].Name ), "Ok" );
			}
			else
			{
				// Parse other worksheets to ensure they all have all the languages of the first worksheet but no more.
				int nWorksheetIndex = 1;
				while( nWorksheetIndex<worksheets.Length && bHasBeenCancelled==false )
				{
					bHasBeenCancelled = EditorUtility.DisplayCancelableProgressBar( sProgressBarTitle, string.Format( sProgressBarMessage, worksheets[nWorksheetIndex].Name ), nWorksheetIndex/( float )worksheets.Length );

					if( bHasBeenCancelled==false )
					{
						HashSet<string> languageUnicityChecker = new HashSet<string>();
						
						nColumnIndex = 1;
						while( nColumnIndex<=worksheets[nWorksheetIndex].Dimension.Columns && bHasBeenCancelled==false )
						{
							string sColumnHeaderText = worksheets[nWorksheetIndex].GetValue<string>( 1, nColumnIndex );
							if( string.IsNullOrEmpty( sColumnHeaderText )==false && regExpression.IsMatch( sColumnHeaderText ) )
							{
								if( languageUnicityChecker.Contains( sColumnHeaderText ) )
								{
									bHasBeenCancelled = EditorUtility.DisplayDialog( "Scan languages", string.Format( "The language code '{0}' is defined in more than 1 column in worksheet '{1}'.", sColumnHeaderText, worksheets[nWorksheetIndex].Name ), "Ok", "Cancel" )==false;
								}
								else
								{
									languageUnicityChecker.Add( sColumnHeaderText );
									if( m_nFirstLanguageColumnIndexes[nWorksheetIndex]==0 )
									{
										m_nFirstLanguageColumnIndexes[nWorksheetIndex] = nColumnIndex;
									}
								}

								if( bHasBeenCancelled==false && m_sLanguageCodes.Contains( sColumnHeaderText )==false )
								{
									if( languageCodesToAdd.Contains( sColumnHeaderText )==false )
									{
										languageCodesToAdd.Add( sColumnHeaderText );
									}
									bHasBeenCancelled = EditorUtility.DisplayDialog( "Scan languages", string.Format( "The language code '{0}' is defined in worksheet '{1}' but is not defined in worksheet '{2}'.", sColumnHeaderText, worksheets[nWorksheetIndex].Name, worksheets[0].Name ), "Ok", "Cancel" )==false;
								}
							}

							++nColumnIndex;
						}

						if( bHasBeenCancelled==false )
						{
							List<string> languageCodesNoDefined = m_sLanguageCodes.FindAll( item => languageUnicityChecker.Contains(item)==false );
							if( languageCodesNoDefined.Count>0 )
							{
								string sLanguageCodesNotDefinedEnumeration = string.Join( ", ", languageCodesNoDefined.ToArray() );
								bHasBeenCancelled = EditorUtility.DisplayDialog( "Scan languages", string.Format( "Some language codes ({0}) are not defined in worksheet '{1}' but is not defined in worksheet '{2}'.", sLanguageCodesNotDefinedEnumeration, worksheets[0].Name, worksheets[nWorksheetIndex].Name), "Ok", "Cancel" )==false;
							}
						}

						++nWorksheetIndex;
					}
				}

				if( bHasBeenCancelled==false )
				{
					m_sLanguageCodes.AddRange( languageCodesToAdd );
				}

				m_sSelectedLanguageCodes.UnionWith( m_sLanguageCodes );

				string[] sSelectedLanguagesFromPreferences = EditorPrefs.GetString( s_windowPreferencesPrefix+"selectedLanguages", "" ).Split(new char[]{ ';' }, System.StringSplitOptions.RemoveEmptyEntries );
				if( sSelectedLanguagesFromPreferences.Length > 0 )
				{
					m_sSelectedLanguageCodes.IntersectWith( sSelectedLanguagesFromPreferences );
				}
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}

	private void DoImportTexts()
	{
		try
		{
			OpenTargetFiles();

			string sProgressBarTitle = "Import localization texts";
			string sProgressBarMessage = "Importing worksheet '{0}'";
			bool bHasBeenCancelled = false;

			ExcelWorksheet[] worksheets = new ExcelWorksheet[m_selectedWorksheets.Count];
			m_selectedWorksheets.CopyTo( worksheets );

			HashSet<string> sLocalizationKeys = new HashSet<string>();

			float fPercentPerWorksheet = 1/( float )worksheets.Length;
			int nWorksheetIndex = 0;
			while( nWorksheetIndex<worksheets.Length )
			{
				int nWorksheetIndexInExcelFile = worksheets[nWorksheetIndex].Index-1;
				lwTools.Assert( nWorksheetIndexInExcelFile>=0 && nWorksheetIndexInExcelFile<m_excelPackage.Workbook.Worksheets.Count );
				
				string sCurrentProgressMessage = string.Format( sProgressBarMessage, worksheets[nWorksheetIndex].Name );
				float fMinPercent = fPercentPerWorksheet*nWorksheetIndex;
				float fMaxPercent = fPercentPerWorksheet*( nWorksheetIndex+1 );
				bHasBeenCancelled = EditorUtility.DisplayCancelableProgressBar( sProgressBarTitle, sCurrentProgressMessage, fMinPercent );

				if( bHasBeenCancelled==false )
				{
					string[] sLanguageCodes = new string[m_sSelectedLanguageCodes.Count];
					m_sSelectedLanguageCodes.CopyTo( sLanguageCodes );

					int[] nColumnIndexesForLanguages = new int[sLanguageCodes.Length];
					for( int nLanguageIndex = 0; nLanguageIndex<sLanguageCodes.Length; ++nLanguageIndex )
					{
						nColumnIndexesForLanguages[nLanguageIndex] = ExcelUtils.FindColumn( worksheets[nWorksheetIndex], 1, sLanguageCodes[nLanguageIndex] );
					}

					int nRowIndex = 2;
					while( bHasBeenCancelled==false && nRowIndex<=worksheets[nWorksheetIndex].Dimension.Rows )
					{
						float fWorksheetProgression = ( nRowIndex-2 )/( float )( worksheets[nWorksheetIndex].Dimension.Rows-2 );
						bHasBeenCancelled = EditorUtility.DisplayCancelableProgressBar( sProgressBarTitle, sCurrentProgressMessage, fMinPercent+( fMaxPercent-fMinPercent )*fWorksheetProgression );

						if( bHasBeenCancelled==false )
						{
							string sTextId = worksheets[nWorksheetIndex].GetValue<string>( nRowIndex, 1 );
							if( string.IsNullOrEmpty( sTextId )==false )
							{
								if( sLocalizationKeys.Contains( sTextId ) )
								{
									bHasBeenCancelled = EditorUtility.DisplayDialog( sProgressBarTitle, string.Format( "The localization key '{0}' is already defined somewhere in the source file.", sTextId ), "Continue and Ignore", "Cancel" )==false;
								}
								else
								{
									sLocalizationKeys.Add( sTextId );

									lwTools.Assert( m_nFirstLanguageColumnIndexes[nWorksheetIndexInExcelFile]>0 && m_nFirstLanguageColumnIndexes[nWorksheetIndexInExcelFile]<=worksheets[nWorksheetIndex].Dimension.Columns );
									string sFallbackLocalizedText = worksheets[nWorksheetIndex].GetValue<string>( nRowIndex, m_nFirstLanguageColumnIndexes[nWorksheetIndexInExcelFile] );
									
									WriteKeyInTargetFile( worksheets[nWorksheetIndex].Name, sTextId );
									for( int nLanguageIndex = 0; nLanguageIndex<sLanguageCodes.Length; ++nLanguageIndex )
									{
										string sLocalizedText = worksheets[nWorksheetIndex].GetValue<string>( nRowIndex, nColumnIndexesForLanguages[nLanguageIndex] );
										WriteTextInTargetFile( worksheets[nWorksheetIndex].Name, sLanguageCodes[nLanguageIndex], sLocalizedText, sFallbackLocalizedText );
									}
								}
							}
						}

						++nRowIndex;
					}

					if(bHasBeenCancelled == false)
					{
						AssetDatabase.Refresh();
					}
				}
				
				++nWorksheetIndex;
			}

			if( bHasBeenCancelled==false )
			{
				EditorUtility.DisplayDialog( sProgressBarTitle, "The texts have been successfully imported into the project.", "Thank you !" );
			}
		}
		finally
		{
			CloseTargetFiles();
			EditorUtility.ClearProgressBar();
		}
	}

	private void OpenTargetFiles()
	{
		lwTools.Assert( m_importStringBuilders==null );
		lwTools.Assert( m_importExoticCharacters==null );
		
		int nStringBuilderCount;
		switch( m_exportDestinationFileOption )
		{
			case ExportDestinationFileOption.OneFilePerLanguage: 				nStringBuilderCount = m_sSelectedLanguageCodes.Count; break;
			case ExportDestinationFileOption.OneFilePerLanguagePerSheet:
			case ExportDestinationFileOption.OneFilePerSheetInLanguageFolder:	nStringBuilderCount = m_selectedWorksheets.Count*m_sSelectedLanguageCodes.Count; break;
			default: lwTools.AssertFormat( false, "Invalid ExportDestinationFileOption '{0}'.", m_exportDestinationFileOption ); nStringBuilderCount = 0; break;
		}
		
		m_importStringBuilders = new StringBuilder[nStringBuilderCount];
		for( int nStringBuildIndex = 0; nStringBuildIndex<nStringBuilderCount; ++nStringBuildIndex )
		{
			m_importStringBuilders[nStringBuildIndex] = new StringBuilder();
			m_importStringBuilders[nStringBuildIndex].Append( "TEXTID" );
		}

		HashSet<ExcelWorksheet>.Enumerator worksheetEnumerator = m_selectedWorksheets.GetEnumerator();
		while( worksheetEnumerator.MoveNext() )
		{
			HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
			while( languageEnumerator.MoveNext() )
			{
				WriteTextInTargetFile( worksheetEnumerator.Current.Name, languageEnumerator.Current, languageEnumerator.Current, "" );
			}
		}

		{
			HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
			while( languageEnumerator.MoveNext() )
			{
				lwCountry langStructure = lwCountryCode.GetlwCountry( languageEnumerator.Current );
				if( langStructure.m_bIsLatin==false )
				{
					if( m_importExoticCharacters==null )
					{
						m_importExoticCharacters = new Dictionary<string, HashSet<char>>();
					}
					m_importExoticCharacters.Add( languageEnumerator.Current, new HashSet<char>() );
				}
			}
		}
	}

	private void WriteKeyInTargetFile( string sWorksheetName, string sLocalizedKey )
	{
		int nFirstStringBuilderIndex;
		int nLastStringBuilderIndex;
		GetStringBuilderIndexes( sWorksheetName, out nFirstStringBuilderIndex, out nLastStringBuilderIndex );

		for( int nStringBuilderIndex = nFirstStringBuilderIndex; nStringBuilderIndex<=nLastStringBuilderIndex; ++nStringBuilderIndex )
		{
			m_importStringBuilders[nStringBuilderIndex].Append( string.Format( "\n{0}", sLocalizedKey ) );
		}
	}

	private void WriteTextInTargetFile( string sWorksheetName, string sLanguageCode, string sLocalizedText, string sFallbackLocalizedText )
	{
		if( string.IsNullOrEmpty( sLocalizedText ) )
		{
			sLocalizedText = sFallbackLocalizedText;
		}
		sLocalizedText = ReplaceUnwantedCharacters( sLocalizedText );
		
		int nStringBuilderIndex = GetStringBuilderIndex( sWorksheetName, sLanguageCode );
		m_importStringBuilders[nStringBuilderIndex].Append( string.Format( ";{0}", sLocalizedText ) );

		lwCountry langStructure = lwCountryCode.GetlwCountry( sLanguageCode );
		if( m_importExoticCharacters!=null && langStructure.m_bIsLatin==false )
		{
			m_importExoticCharacters[sLanguageCode].UnionWith( sLocalizedText );
		}
	}

	private static string ReplaceUnwantedCharacters( string sText )
	{
		if( string.IsNullOrEmpty( sText ) )
		{
			return string.Empty;
		}

		sText = sText.Replace( "\u2019", "\'" );
		sText = sText.Replace( "\u00AB", "\"" );
		sText = sText.Replace( "\u00BB", "\"" );
		sText = sText.Replace( "\u2026", "..." );
		//		sText = sText.Replace( "\u00A0", " "); // Espace insécable
		sText = sText.Replace( "—", ":" );
		sText = sText.Replace( " ;", "," );
		sText = sText.Replace( ";", "," );
		sText = sText.Replace( "\r", "" );
		sText = sText.Replace( "\n", "" );

		return sText;
	}

	private void CloseTargetFiles()
	{
		// create and fill files
		switch( m_exportDestinationFileOption )
		{
			case ExportDestinationFileOption.OneFilePerLanguage:
			{
				int nLanguageIndex = 0;
				HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
				while( languageEnumerator.MoveNext() )
				{
					string sPath = System.IO.Path.Combine( m_sDestinationFolderPath, string.Format( "{0}.{1}", languageEnumerator.Current, m_sDestinationFileExtension ) );
					System.IO.File.WriteAllText( sPath, m_importStringBuilders[nLanguageIndex].ToString(), Encoding.UTF8 );

					++nLanguageIndex;
				}
			}
			break;
			case ExportDestinationFileOption.OneFilePerLanguagePerSheet:
			case ExportDestinationFileOption.OneFilePerSheetInLanguageFolder:
			{
				int nWorksheetIndex = 0;
				HashSet<ExcelWorksheet>.Enumerator worksheetEnumerator = m_selectedWorksheets.GetEnumerator();
				while( worksheetEnumerator.MoveNext() )
				{
					int nLanguageIndex = 0;
					HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
					while( languageEnumerator.MoveNext() )
					{
						string sPath;
						if( m_exportDestinationFileOption==ExportDestinationFileOption.OneFilePerLanguagePerSheet )
						{
							sPath = System.IO.Path.Combine( m_sDestinationFolderPath, string.Format( "{0}_{1}.{2}", worksheetEnumerator.Current.Name, languageEnumerator.Current, m_sDestinationFileExtension ) );
						}
						else
						{
							lwTools.AssertFormat( m_exportDestinationFileOption==ExportDestinationFileOption.OneFilePerSheetInLanguageFolder, "Invalid ExportDestinationFileOption '{0}'.", m_exportDestinationFileOption );
							sPath = System.IO.Path.Combine( m_sDestinationFolderPath, languageEnumerator.Current );
							if( System.IO.Directory.Exists( sPath )==false )
							{
								System.IO.Directory.CreateDirectory( sPath );
							}
							sPath = System.IO.Path.Combine( sPath, string.Format( "{0}.{1}", worksheetEnumerator.Current.Name, m_sDestinationFileExtension ) );
						}
						
						System.IO.File.WriteAllText( sPath, m_importStringBuilders[nWorksheetIndex*m_sSelectedLanguageCodes.Count+nLanguageIndex].ToString(), Encoding.UTF8 );

						++nLanguageIndex;
					}

					++nWorksheetIndex;
				}
			}
			break;
			default: lwTools.AssertFormat( false, "Invalid ExportDestinationFileOption '{0}'.", m_exportDestinationFileOption ); break;
		}
		m_importStringBuilders = null;

		// create and fill exotic characters
		{
			HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
			while( languageEnumerator.MoveNext() )
			{
				lwCountry langStructure = lwCountryCode.GetlwCountry( languageEnumerator.Current );
				if( langStructure.m_bIsLatin==false )
				{
					char[] allCharactersUsedByExoticLanguage = new char[m_importExoticCharacters[languageEnumerator.Current].Count];
					m_importExoticCharacters[languageEnumerator.Current].CopyTo( allCharactersUsedByExoticLanguage );

					string sPath = System.IO.Path.Combine( m_sExoticCharacterFolderPath, string.Format( "ExoticCharacters_{0}.txt", languageEnumerator.Current ) );
					System.IO.File.WriteAllText( sPath, new string( allCharactersUsedByExoticLanguage ), Encoding.UTF8 );
				}
			}
		}
		m_importExoticCharacters = null;
	}

	private void GetStringBuilderIndexes( string sWorksheetName, out int nFirstIndex, out int nLastIndex )
	{
		lwTools.Assert( m_importStringBuilders!=null );

		switch( m_exportDestinationFileOption )
		{
			case ExportDestinationFileOption.OneFilePerLanguage:
			{
				nFirstIndex = 0;
				nLastIndex = m_importStringBuilders.Length-1;
			}
			break;
			case ExportDestinationFileOption.OneFilePerLanguagePerSheet:
			case ExportDestinationFileOption.OneFilePerSheetInLanguageFolder:
			{
				int nWorksheetIndex = 0;
				HashSet<ExcelWorksheet>.Enumerator worksheetEnumerator = m_selectedWorksheets.GetEnumerator();
				while( worksheetEnumerator.MoveNext() && worksheetEnumerator.Current.Name!=sWorksheetName )
				{
					++nWorksheetIndex;
				}

				nFirstIndex = nWorksheetIndex*m_sSelectedLanguageCodes.Count;
				nLastIndex = ( nWorksheetIndex+1 )*m_sSelectedLanguageCodes.Count-1;
			}
			break;
			default:
			{
				lwTools.AssertFormat( false, "Invalid ExportDestinationFileOption '{0}'.", m_exportDestinationFileOption );
				nFirstIndex = -1;
				nLastIndex = -1;
			}
			break;
		}
	}

	private int GetStringBuilderIndex( string sWorksheetName, string sLanguageCode )
	{
		lwTools.Assert( m_importStringBuilders!=null );

		int nStringBuilderIndex;
		switch( m_exportDestinationFileOption )
		{
			case ExportDestinationFileOption.OneFilePerLanguage:
			{
				int nLanguageIndex = 0;
				HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
				while( languageEnumerator.MoveNext() && languageEnumerator.Current!=sLanguageCode )
				{
					++nLanguageIndex;
				}
				nStringBuilderIndex = nLanguageIndex;
			}
			break;
			case ExportDestinationFileOption.OneFilePerLanguagePerSheet:
			case ExportDestinationFileOption.OneFilePerSheetInLanguageFolder:
			{
				int nWorksheetIndex = 0;
				HashSet<ExcelWorksheet>.Enumerator worksheetEnumerator = m_selectedWorksheets.GetEnumerator();
				while( worksheetEnumerator.MoveNext() && worksheetEnumerator.Current.Name!=sWorksheetName )
				{
					++nWorksheetIndex;
				}

				int nLanguageIndex = 0;
				HashSet<string>.Enumerator languageEnumerator = m_sSelectedLanguageCodes.GetEnumerator();
				while( languageEnumerator.MoveNext() && languageEnumerator.Current!=sLanguageCode )
				{
					++nLanguageIndex;
				}

				nStringBuilderIndex = nWorksheetIndex*m_sSelectedLanguageCodes.Count+nLanguageIndex;
			}
			break;
			default: lwTools.AssertFormat( false, "Invalid ExportDestinationFileOption '{0}'.", m_exportDestinationFileOption ); nStringBuilderIndex = -1; break;
		}

		return nStringBuilderIndex;
	}
	#endregion

	#region Attributes
	private Vector2 m_v2LanguageSelectionScrollPosition;

	private string m_sExcelFilePath;
	private System.DateTime m_excelFileLastWriteTime;

	private ExcelPackage m_excelPackage;
	private HashSet<ExcelWorksheet> m_selectedWorksheets;
	private List<int> m_nFirstLanguageColumnIndexes;
	private List<string> m_sLanguageCodes;
	private HashSet<string> m_sSelectedLanguageCodes;

	private ExportDestinationFileOption m_exportDestinationFileOption;
	private string m_sDestinationFolderPath;
	private string m_sDestinationFileExtension;

	private string m_sExoticCharacterFolderPath;

	private StringBuilder[] m_importStringBuilders;
	private Dictionary<string, HashSet<char>> m_importExoticCharacters;
	#endregion
#endregion
}
