// Copyright 2016 Bigpoint, Lyon
//
// Maintainer: Sylvain MINJARD <sminjard@bigpoint.net>
//
// Date: 2016/10/25

using OfficeOpenXml;

//! @class	ExcelUtils
//!
//! @brief	Utility class for excel parsing
public static class ExcelUtils
{
	//! Find the row index containing the given value at the given column
	//!
	//!	@param	worksheet		worksheet to search in
	//!	@param	columnIndex		index of the column to search in
	//!	@param	value			value to search for
	//!
	//!	@return the index of the row found, -1 otherwise
	public static int FindRow(ExcelWorksheet worksheet, int columnIndex, object value)
	{
		return FindRow(worksheet, columnIndex, value, 1, worksheet.Dimension.Rows);
	}

	//! Find the row index containing the given value at the given column
	//!
	//!	@param	worksheet		worksheet to search in
	//!	@param	columnIndex		index of the column to search in
	//!	@param	value			value to search for
	//!	@param	firstRowIndex	first row in the colmun to check
	//!
	//!	@return the index of the row found, -1 otherwise
	public static int FindRow(ExcelWorksheet worksheet, int columnIndex, object value, int firstRowIndex)
	{
		return FindRow(worksheet, columnIndex, value, firstRowIndex, worksheet.Dimension.Rows);
	}

	//! Find the row index containing the given value at the given column
	//!
	//!	@param	worksheet		worksheet to search in
	//!	@param	columnIndex		index of the column to search in
	//!	@param	value			value to search for
	//!	@param	startRowIndex	first row in the colmun to check
	//!	@param	lastRowIndex	last row in the colmun to check
	//!
	//!	@return the index of the row found, -1 otherwise
	public static int FindRow(ExcelWorksheet worksheet, int columnIndex, object value, int firstRowIndex, int lastRowIndex)
	{
		lwTools.Assert(worksheet != null);
		lwTools.Assert(columnIndex >= 1  &&  columnIndex <= worksheet.Dimension.Columns, "Invalid columnIndex : " + columnIndex);
		lwTools.Assert(firstRowIndex >= 1  &&  lastRowIndex <= worksheet.Dimension.Rows, "Invalid firstRowIndex : " + firstRowIndex);
		lwTools.Assert(lastRowIndex >= 1  &&  lastRowIndex <= worksheet.Dimension.Rows, "Invalid lastRowIndex : " + lastRowIndex);

		int rowIndex = firstRowIndex;
		bool hasSearchEnded = false;
		while(hasSearchEnded == false  &&  object.Equals(worksheet.GetValue(rowIndex, columnIndex), value) == false)
		{
			hasSearchEnded = rowIndex == lastRowIndex;
			if(firstRowIndex <= lastRowIndex)
			{
				++rowIndex;
			}
			else
			{
				--rowIndex;
			}
		}

		if(hasSearchEnded)
		{
			return -1;
		}
		else
		{
			return rowIndex;
		}
	}

	//! Find the column index containing the given value at the given row
	//!
	//!	@param	worksheet		worksheet to search in
	//!	@param	rowIndex		index of the row to search in
	//!	@param	value			value to search for
	//!
	//!	@return the index of the column found, -1 otherwise
	public static int FindColumn(ExcelWorksheet worksheet, int rowIndex, object value)
	{
		return FindColumn(worksheet, rowIndex, value, 1, worksheet.Dimension.Columns);
	}

	//! Find the column index containing the given value at the given row
	//!
	//!	@param	worksheet			worksheet to search in
	//!	@param	rowIndex			index of the column to search in
	//!	@param	value				value to search for
	//!	@param	firstColumnIndex	first column in the row to check
	//!
	//!	@return the index of the column found, -1 otherwise
	public static int FindColumn(ExcelWorksheet worksheet, int rowIndex, object value, int firstColumnIndex)
	{
		return FindColumn(worksheet, rowIndex, value, firstColumnIndex, worksheet.Dimension.Columns);
	}

	//! Find the column index containing the given value at the given row
	//!
	//!	@param	worksheet			worksheet to search in
	//!	@param	rowIndex			index of the row to search in
	//!	@param	value				value to search for
	//!	@param	firstColumnIndex	first column in the row to check
	//!	@param	lastColumnIndex		last column in the row to check
	//!
	//!	@return the index of the column found, -1 otherwise
	public static int FindColumn(ExcelWorksheet worksheet, int rowIndex, object value, int firstColumnIndex, int lastColumnIndex)
	{
		lwTools.Assert(worksheet != null);
		lwTools.Assert(rowIndex >= 1  &&  rowIndex <= worksheet.Dimension.Rows, "Invalid rowIndex : " + rowIndex);
		lwTools.Assert(firstColumnIndex >= 1  &&  firstColumnIndex <= worksheet.Dimension.Columns, "Invalid firstColumnIndex : " + firstColumnIndex);
		lwTools.Assert(lastColumnIndex >= 1  &&  lastColumnIndex <= worksheet.Dimension.Columns, "Invalid lastColumnIndex : " + lastColumnIndex);

		int columnIndex = firstColumnIndex;
		bool hasSearchEnded = false;
		while(hasSearchEnded == false  &&  object.Equals(worksheet.GetValue(rowIndex, columnIndex), value) == false)
		{
			hasSearchEnded = columnIndex == lastColumnIndex;
			if(firstColumnIndex <= lastColumnIndex)
			{
				++columnIndex;
			}
			else
			{
				--columnIndex;
			}
		}

		if(hasSearchEnded)
		{
			return -1;
		}
		else
		{
			return columnIndex;
		}
	}
}
