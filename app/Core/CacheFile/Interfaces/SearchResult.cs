using System;
using System.Collections.Generic;
using System.Text;

namespace app.Core
{

    [Serializable]
    public class SearchResult
    {
        public int PageSize { set; get; }
        public int PageNumber { set; get; }

        public bool Status { set; get; }

        public int Total { set; get; }

        public int[] IDs { set; get; }

        public object Message { set; get; }

        public bool IsCache { set; get; }

        public string FieldSyncEdit { set; get; }


        public SearchResult()
        {
            //ID = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff"));
        }

        public SearchResult(bool _status, int _total, int[] _ids, string _message)
            : this()
        {
            Total = _total;
            Status = _status;
            IDs = _ids;
            Message = _message;
        }

        public override string ToString()
        {
            return Total.ToString();
        }

        public SearchResult Clone()
        {
            return new SearchResult()
            {
                IDs = this.IDs,
                IsCache = this.IsCache,
                Message = this.Message,
                Status = this.Status,
                Total = this.Total,
                PageSize = this.PageSize,
                PageNumber = this.PageNumber,
                FieldSyncEdit = this.FieldSyncEdit,
            };
        }
    }

}
