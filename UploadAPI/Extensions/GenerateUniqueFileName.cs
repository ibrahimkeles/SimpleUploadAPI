namespace UploadAPI.Extensions
{
    public static class GenerateUniqueFileName
    {
        public static string Generate(string fileName)
        {
            var fileDate = DateTime.Now.ToString()
                                .Replace(".","")
                                .Replace(" ","")
                                .Replace(":","");

            return string.Concat(fileDate
                                , "_"
                                , Guid.NewGuid().ToString().AsSpan(0, 4)
                                , Path.GetExtension(fileName));
        }
    }
}
