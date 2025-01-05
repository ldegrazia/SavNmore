using System;
using System.Configuration;
using System.Text;
using System.Web;
using System.IO;
using savnmore;

namespace savnmore
{
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }
    /// <summary>
    /// Logging function which writes to te log in the location specified in the web config
    /// </summary>
    public static class Logger
    {

        /// <summary>
        /// Writes a log entry with the message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="theType"></param>
        public static void WriteLine(MessageType theType, string message)
        {
            try
            {

                using (var w = File.AppendText(HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[Constants.LogFileKey])))
                {
                    Log(theType, message, w);
                    // Close the writer and underlying file.
                    w.Close();
                }
            }
            catch (Exception ex)
            {   //check here why it failed and ask user to retry if the file is in use. 
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Writes to the log
        /// </summary>
        /// <param name="type"></param>
        /// <param name="logMessage"></param>
        /// <param name="w"></param>
        public static void Log(MessageType type, string logMessage, TextWriter w)
        {
            w.WriteLine("{0},{1},{2},{3}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), type, logMessage);
            w.Flush();// Update the underlying file.
        }
        /// <summary>
        /// Returns the log
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string DumpLog()
        {
            try
            {
                using (
                    var reader =
                        File.OpenText(
                            HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[Constants.LogFileKey])))
                {
                    // While not at the end of the file, read and write lines.
                    var sb = new StringBuilder();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        sb.AppendLine("<br/>");
                        sb.AppendLine(line);
                    }
                    reader.Close();
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {   //check here why it failed and ask user to retry if the file is in use. 
                return ex.Message;
            }
        }
        public static string DeleteLog()
        {
            try
            {

                File.Delete(
                    HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[Constants.LogFileKey]));
                return "Deleted log";
            }
            catch (Exception ex)
            {   //check here why it failed and ask user to retry if the file is in use. 
                return ex.Message;
            }
        }
    }
}