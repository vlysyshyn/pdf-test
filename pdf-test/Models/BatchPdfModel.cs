namespace pdf_test.Models
{
    public class BatchPdfModel
    {
        public string Logo { get; set; }
        public List<TestBatchRecord> Records { get; set; } = new List<TestBatchRecord>();
    }

    public class TestBatchRecord
    {
        public string OriginatingAccountNumber { get; set; }
        public string OriginatingSortCode { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string PayeeAccountNumber { get; set; }
        public string PayeeSortCode { get; set; }
        public DateTime Date { get; set; }
    }
}
