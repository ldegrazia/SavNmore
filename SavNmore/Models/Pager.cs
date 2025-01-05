using System;
using System.Configuration;
using savnmore;

namespace savnmore.Models
{
    /// <summary>
    /// Custom paging class
    /// </summary>
    public class Pager
    {
        private int _page;
        /// <summary>
        /// Current page number.
        /// </summary>
        public int Page
        {
            get { return _page < 1 ? 1 : _page; }
            set { _page = value; }
        }

        private int _perpage;
        /// <summary>
        /// Number of items per page to display
        /// </summary>
        public int Perpage
        {
            get { return _perpage < 1 ? Convert.ToInt32(ConfigurationManager.AppSettings[Constants.NumberOfItemsPerPageKey]) : _perpage; }
            set { _perpage = value; }
        }

        /// <summary>
        /// The lowest item in a page
        /// </summary>
        public int LowerBound
        {
            get { return RecordsPerPage - Perpage + 1; }
        }
        int _itemcount = 0;
        /// <summary>
        /// Total records. If the pagenumber is greater than the last page, the pagenumber is set to the last page number
        /// </summary>
        public int ItemCount
        {
            get { return _itemcount; }
            set
            {
                _itemcount = value;
                if (Page > Lastpage) { Page = Lastpage; }
            }
        }
        /// <summary>
        /// Number of records per page
        /// </summary>
        public int RecordsPerPage { get { return Page * Perpage; } }
        /// <summary>
        /// Max record, if the last page has less than a full page worth
        /// </summary>
        public int UpperBound
        {
            get
            {
                var upperBound = LowerBound + RecordsPerPage - 1;
                if (upperBound > ItemCount) //there are less items than the records per page
                {
                    return ItemCount;
                }
                return upperBound;
            }
        }


        /// <summary>
        /// Returns the current set from the total number of items and the  current page
        /// </summary>
        /// <returns></returns>
        public string CurrentSet()
        {

            return LowerBound + " to " + UpperBound + " of " + ItemCount;

        }
        /// <summary>
        /// If there are more records from the current page
        /// </summary>
        public bool HasMore
        {
            get { return Page < Lastpage; }
        }
        /// <summary>
        /// If there are more records from the current page
        /// </summary>
        public bool HasLess
        {
            get { return Page > 1; }
        }
        /// <summary>
        /// The previous page number
        /// </summary>
        public int PreviousPage
        {
            get { return Page - 1; }
        }
        /// <summary>
        /// The next page number or last page
        /// </summary>
        public int NextPage
        {
            get { return (Page == Lastpage) ? Page : Page + 1; }
        }
        /// <summary>
        /// The last page available
        /// </summary>
        public int Lastpage
        {
            get { return (int)Math.Ceiling((ItemCount / (double)Perpage)); }
        }

    }
}
