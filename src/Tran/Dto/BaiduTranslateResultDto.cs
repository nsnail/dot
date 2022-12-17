// ReSharper disable  InconsistentNaming
// ReSharper disable  ClassNeverInstantiated.Global

#pragma warning disable IDE1006,SA1300
namespace Dot.Tran.Dto;

internal sealed record BaiduTranslateResultDto
{
    public sealed record DataItem
    {
        public string dst { get; set; }

        public int prefixWrap { get; set; }

        public string result { get; set; }

        public string src { get; set; }
    }

    public sealed record PhoneticItem
    {
        public string src_str { get; set; }

        public string trg_str { get; set; }
    }

    public sealed record Root
    {
        public string errmsg { get; set; }

        public int errno { get; set; }

        public int error { get; set; }

        public string errShowMsg { get; set; }

        public string from { get; set; }

        public long logid { get; set; }

        public string query { get; set; }

        public string to { get; set; }

        public Trans_result trans_result { get; set; }
    }

    public sealed record Trans_result
    {
        public List<DataItem> data { get; set; }

        public string from { get; set; }

        public List<PhoneticItem> phonetic { get; set; }

        public int status { get; set; }

        public string to { get; set; }

        public int type { get; set; }
    }
}