namespace CryptoPredictorWebApi
{
    public class DataProviderCsvService: IDataProviderService
    {
        private const string DIR_PATH = "CryptoCsvData";
        private const string CSV_SEPARATOR = ",";

        private static FileInfo[] GetFilesInfo()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), DIR_PATH);
            DirectoryInfo d = new DirectoryInfo(path);
            var filesList = d.GetFiles("*.csv");
            return filesList;
        }

        public List<CryptoPriceRow> GetData()
        {
            var filesList = GetFilesInfo();
            var cryptoPriceData = new List<CryptoPriceRow>();

            foreach (var file in filesList)
            {
                using (var reader = file.OpenText())
                {
                    reader.ReadLine();//First line is header, we don't need it.
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(CSV_SEPARATOR);
                        if (values.Length == 3)
                        {
                            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(values[0]));
                            cryptoPriceData.Add(
                                new CryptoPriceRow(
                                    values[1],
                                    Convert.ToDecimal(values[2]),
                                    dateTimeOffSet.DateTime
                                    )
                                );
                        }

                    }
                }
            }
            return cryptoPriceData;
        }
    }
}
