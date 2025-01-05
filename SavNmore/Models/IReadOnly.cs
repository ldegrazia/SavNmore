namespace savnmore.Models
{
    /// <summary>
    /// Interfaces to control object changes
    /// </summary>
    public interface IReadOnly
    {

        /// <summary>
        /// The object cannot be changed at all. Once created, the state stays
        /// </summary>
        bool IsReadOnly { get; set; }
    }
}
