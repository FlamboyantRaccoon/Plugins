// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/10/10

using UnityEngine;
using UnityEditor;

//! @class lwPathUtil
//!
//!	@brief	Utility class to help using path (file and folder) inside Unity editor
public static class lwPathUtil
{
	//!	@enum PathType
	//!
	//!	@brief	Type of path
	public enum PathType
	{
		Any,
		Asset,
		Resource
	}

	//! Convert a relative path (regarding PathType enumeration) to an absolute path
	//!	Path type Any can not be converted to absolute since no indication is given to its context.
	//!
	//!	@param	ePathType		folder type according to the enum FolderType
	//!	@param	sSource			relative path source
	//!	@param	sDestination	absolute destination path
	//!
	//!	@return true if the source is correct, false otherwise
	public static bool ToAbsolute( PathType ePathType, string sSource, out string sDestination )
	{
		switch( ePathType )
		{
			case PathType.Any:
			{
				sDestination = sSource;
				return true;
			}
			case PathType.Asset:
			{
				if( string.IsNullOrEmpty( sSource ) || sSource.StartsWith( "Assets", System.StringComparison.Ordinal )==false )
				{
					sDestination = Application.dataPath;
					return false;
				}
				else
				{
					sDestination = Application.dataPath + sSource.Substring( "Assets".Length );
					return true;
				}
			}
			case PathType.Resource:
			{
				string[] sResourcesFolderGuids = AssetDatabase.FindAssets( "Resources t:DefaultAsset" );

				string sAbsoluteSourcePath = null;
				int nResourceFolderIndex = 0;
				while( sAbsoluteSourcePath==null && nResourceFolderIndex<sResourcesFolderGuids.Length )
				{
					string sResourceFolderPath = AssetDatabase.GUIDToAssetPath( sResourcesFolderGuids[nResourceFolderIndex] );
					string sAbsoluteResourceFolderPath;
					bool bIsSuccess = ToAbsolute( PathType.Asset, sResourceFolderPath, out sAbsoluteResourceFolderPath );
					lwTools.Assert( bIsSuccess );
					sAbsoluteSourcePath = System.IO.Path.Combine( sAbsoluteResourceFolderPath, sSource );
					if( System.IO.Directory.Exists( sAbsoluteSourcePath ) == false )
					{
						sAbsoluteSourcePath = null;
					}

					++nResourceFolderIndex;
				}

				sDestination = sAbsoluteSourcePath;
				return sAbsoluteSourcePath != null;
			}
			default:
			{
				lwTools.Assert( false, "Unhandled PathType '" + ePathType.ToString() + "'." );
				sDestination = sSource;
				return false;
			}
		}
	}

	//! Convert an absolute path to a relative path (regarding PathType enumeration)
	//!	Path type Any can not be converted to relative since no indication is given to its context.
	//!
	//!	@param	ePathType		folder type according to the enum FolderType
	//!	@param	sSource			absolute path source
	//!	@param	sDestination	relative destination path
	//!
	//!	@return true if the source is correct, false otherwise
	public static bool ToRelative( PathType ePathType, string sSource, out string sDestination )
	{
		switch( ePathType )
		{
			case PathType.Any:
			{
				sDestination = sSource;
				return true;
			}
			case PathType.Asset:
			{
				if( string.IsNullOrEmpty( sSource ) || sSource.StartsWith( Application.dataPath, System.StringComparison.Ordinal )==false )
				{
					sDestination = sSource;
					return false;
				}
				else
				{
					sDestination = sSource.Substring( Application.dataPath.Length - "Assets".Length );
					return true;
				}
			}
			case PathType.Resource:
			{
				if( string.IsNullOrEmpty( sSource ) || sSource.StartsWith( Application.dataPath, System.StringComparison.Ordinal )==false )
				{
					sDestination = sSource;
					return false;
				}
				else
				{
					string[] sPathElements = sSource.Split( '/', '\\' );
					int nLastResourceFolderIndex = System.Array.LastIndexOf( sPathElements, "Resources" );
					if( nLastResourceFolderIndex>=0 && nLastResourceFolderIndex<sPathElements.Length )
					{
						int nFolderCount = sPathElements.Length - 1 - nLastResourceFolderIndex;
						string[] sRelativePathElements = new string[nFolderCount];
						System.Array.Copy( sPathElements, sRelativePathElements, nFolderCount );
						sDestination = string.Join( "/", sRelativePathElements );
						return true;
					}
					else
					{
						sDestination = sSource;
						return false;
					}
				}
			}
			default:
			{
				lwTools.Assert( false, "Unhandled PathType '" + ePathType.ToString() + "'." );
				sDestination = sSource;
				return false;
			}
		}
	}
}