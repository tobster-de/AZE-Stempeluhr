using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace AZE.Impl
{
    class ExcelFileAccess : IDisposable
    {
        private Excel.Application excelApp;

        private Excel.Workbook excelWorkbook;

        private Excel.Worksheet excelWorksheet;

        private ExcelFileAccess(string file, bool readOnly = true)
        {
            try
            {
                this.excelApp = new Excel.Application();
                this.excelWorkbook = this.excelApp.Workbooks.Open(file, 0, readOnly, 5, "", "", false, Excel.XlPlatform.xlWindows, "", !readOnly, false, 0, true, false, false);
                this.excelWorksheet = (Excel.Worksheet)this.excelWorkbook.Worksheets.Item[1];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                this.excelWorkbook = null;
                this.excelWorksheet = null;
            }
        }

        public static ExcelFileAccess OpenReadOnly(string file)
        {
            return new ExcelFileAccess(file);
        }

        public static ExcelFileAccess OpenFullAccess(string file)
        {
            return new ExcelFileAccess(file, false);
        }

        public AzeData FindData(DateTime date, int? knownRow = null)
        {
            if (this.excelWorkbook == null)
            {
                return null;
            }

            try
            {
                int rows = this.excelWorksheet.UsedRange.Rows.Count;
                Excel.Range cells = (Excel.Range)this.excelWorksheet.Columns["C"];

                int rowNumber = knownRow.GetValueOrDefault();
                if (!knownRow.HasValue)
                {
                    for (int i = 1; i <= rows; i++)
                    {
                        dynamic x = cells.Cells[i].Value;
                        if (x != null && x.Equals(date))
                        {
                            rowNumber = i;
                            break;
                        }
                    }
                }
                else if (rowNumber > rows)
                {
                    return null;
                }

                Excel.Range rowData = (Excel.Range)this.excelWorksheet.Rows[rowNumber];

                var colH = rowData.Cells[8].Value;
                var colI = rowData.Cells[9].Value;
                var colJ = rowData.Cells[10].Value;
                var colN = rowData.Cells[14].Value;

                var begin = colH != null ? DateTime.FromOADate(colH) : null;
                var end = colI != null ? DateTime.FromOADate(colI) : null;
                var pause = colJ != null ? DateTime.FromOADate(colJ) : null;
                var work = colN != null ? DateTime.FromOADate(colN) : null;

                return new AzeData(rowNumber, begin, end, pause, work);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return null;
            }
        }

        public bool SetTime(int rowNumber, AzeTimeValueEnum timeValue, DateTime time)
        {
            if (this.excelWorkbook == null || this.excelWorkbook.ReadOnly)
            {
                return false;
            }

            try
            {
                int rows = this.excelWorksheet.UsedRange.Rows.Count;
                if (rowNumber > rows)
                {
                    return false;
                }

                Excel.Range rowData = (Excel.Range)this.excelWorksheet.Rows[rowNumber];

                // remove everything except hour / minute
                double targetTime = time.AddSeconds(-time.Second).AddMilliseconds(-time.Millisecond).ToOADate();
                // removes the date part
                targetTime = targetTime - Math.Truncate(targetTime);
                rowData.Cells[8 + timeValue].Value = targetTime;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return false;
            }
        }

        public bool AddPause(int rowNumber, int minutes)
        {
            if (this.excelWorkbook == null || this.excelWorkbook.ReadOnly)
            {
                return false;
            }

            try
            {
                int rows = this.excelWorksheet.UsedRange.Rows.Count;
                if (rowNumber > rows)
                {
                    return false;
                }

                Excel.Range rowData = (Excel.Range)this.excelWorksheet.Rows[rowNumber];
                
                var colJ = rowData.Cells[10].Value;
                DateTime pause = colJ != null ? DateTime.FromOADate(colJ) : new DateTime();
                
                rowData.Cells[10].Value = pause.AddMinutes(minutes).ToOADate();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return false;
            }
        }

        public void SaveFile()
        {
            if (this.excelWorkbook != null && !this.excelWorkbook.ReadOnly)
            {
                this.excelWorkbook.Save();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.excelWorksheet != null)
            {
                Marshal.FinalReleaseComObject(this.excelWorksheet);
                this.excelWorksheet = null;
            }

            if (this.excelWorkbook != null)
            {
                this.excelWorkbook.Close(!this.excelWorkbook.ReadOnly);
                Marshal.FinalReleaseComObject(this.excelWorkbook);
                this.excelWorkbook = null;
            }

            if (this.excelApp != null)
            {
                this.excelApp.Quit();
                Marshal.FinalReleaseComObject(this.excelApp);
                this.excelApp = null;
            }
        }
    }
}
