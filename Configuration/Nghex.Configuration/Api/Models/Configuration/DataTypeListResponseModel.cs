namespace Nghex.Configuration.Api.Models
{
    public class DataTypeResponseModel
    {
        /// <summary>
        /// Name of the data type
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Value of the data type
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    public class DataTypeListResponseModel
    {
        /// <summary>
        /// List of data types
        /// </summary>
        public List<DataTypeResponseModel> DataTypes { get; set; } = [];
        /// <summary>
        /// Total count of data types
        /// </summary>
        public int Count { get; set; }
    }
}
