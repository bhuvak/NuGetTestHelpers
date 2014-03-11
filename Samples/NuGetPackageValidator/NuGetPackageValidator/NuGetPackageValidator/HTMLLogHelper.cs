using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.UI;

namespace NuGetPackageValidator
{
    public class HTMLLogger : IDisposable
    {
        /// <summary>
        /// Creates log file with the specified full path.
        /// </summary>
        /// <param name="fileName"></param>
        public HTMLLogger(string fileName)
        {
            stringwriter = new StringWriter();
            htmlWriter = new HtmlTextWriter(stringwriter);
            if (File.Exists(fileName))
                File.Delete(fileName);
            _streamWriter = new StreamWriter(fileName);

        }

        public void WriteSucessMessage(string message)
        {
            AddHtmlSucessMessage(message);
        }

        public void WriteSucessMessage(string messageFormat, params object[] args)
        {
            WriteSucessMessage(string.Format(messageFormat, args));
        }

        public void WriteLog(string message)
        {
            AddHtmlLog(message);
        }

        public void WriteLog(string messageFormat, params object[] args)
        {

            WriteLog(string.Format(messageFormat, args));
        }

        public void WriteError(string message)
        {
            AddHtmlError(message);
        }

        public void WriteError(string messageFormat, params object[] args)
        {
            WriteError(string.Format(messageFormat, args));
        }

        public void WriteHeader(string message)
        {
            AddHtmlHeader(message);
        }

        public void WriteHeader(string messageFormat, params object[] args)
        {
            WriteHeader(string.Format(messageFormat, args));
        }

        public void WriteSubHeader(string message)
        {
            AddHtmlSubHeader(message);
        }

        public void WriteSubHeader(string messageFormat, params object[] args)
        {
            WriteSubHeader(string.Format(messageFormat, args));
        }

        public void WriteTitle(string message)
        {
            AddHtmlTitle(message);
        }

        public void WriteLink(string linkText, string url)
        {
            AddHtmlLink(linkText, url);
        }
        public void WriteTitle(string messageFormat, params object[] args)
        {

            WriteTitle(string.Format(messageFormat, args));
        }

        public void WriteTestCaseResult(string scenario, string result,string details)
        {
            AddHtmlTableRowForTestCaseResult(scenario, result,details);
        }

        public void WriteTestCaseResultTableHeader(string[] headers)
        {
            BeginTestCaseResultTable(headers);
        }

        public void Write(string message)
        {
            htmlWriter.Write(message);
        }

        public void WriteEnd()
        {
            EndTag();
        }
        public void Dispose()
        {
            FileWriter.Write(stringwriter.ToString());
            FileWriter.Flush();
            FileWriter.Close();
        }

        #region PrivateMethods

        private void AddHtmlSucessMessage(string message)
        {

            WriteMessage(message, "color:green");
        }

        private void AddHtmlLog(string message)
        {

            WriteMessage(message, "color:black");
        }

        private void AddHtmlError(string message)
        {

            WriteMessage(message, "color:red");
        }

        private void WriteMessage(string message, string style)
        {
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Size, "2");
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Style, style);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
            htmlWriter.WriteEncodedText(message);
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }

        private void AddHtmlLink(string linkText, string url)
        {
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, url);
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
            htmlWriter.Write(linkText);
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }
        private void AddHtmlTitle(string message)
        {
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Style, "Color:blue");
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.H2);
            htmlWriter.Write(message);
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }

        private void AddHtmlHeader(string message)
        {
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Style, "Color:blue");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.H4);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.U);
            htmlWriter.Write(message);
            htmlWriter.RenderEndTag();
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }

        private void AddHtmlSubHeader(string message)
        {

            htmlWriter.RenderBeginTag(HtmlTextWriterTag.H5);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.U);
            htmlWriter.Write(message);
            htmlWriter.RenderEndTag();
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }

        private void AddHtmlHeader(string message, params object[] args)
        {

            AddHtmlHeader(string.Format(message, args));
        }

        private void AddHtmlTitle(string message, params object[] args)
        {

            AddHtmlTitle(string.Format(message, args));
        }

        private void AddHtmlLog(string message, params object[] args)
        {

            AddHtmlLog(string.Format(message, args));
        }

        private void AddHtmlError(string message, params object[] args)
        {

            AddHtmlError(string.Format(message, args));
        }

        internal void WriteScript(string script)
        {
            htmlWriter.Write(script);
        }       

        internal void WriteTable(Dictionary<string, string> tableValues)
        {
            AddHtmlTable(tableValues);
        }

        public void WriteSummary(int errorCount, int warningCount)
        {
            StringBuilder builder = new StringBuilder();
            htmlWriter.Write(@"<script type=""text/javascript"">
function Toggle(arg1,arg2) {
	var el = document.getElementById(arg1);
    var link = document.getElementById(arg2);
	if (el.style.display == ""block"") {
		el.style.display = ""none"";
        link.text = ""Show details""
	}
	else {
		el.style.display = ""block""
        link.text = ""Hide details"";
	}
}
</script>");
            builder.AppendLine(@"<div id=""result"" style=""visibility: hidden"">");
            builder.AppendLine(@"<span id=""totalerrors"">" + errorCount + @"</span> ");
            builder.AppendLine(@"<span id=""totalwarnings"">" + warningCount + @"</span>");
            builder.AppendLine(@"</div>");
            htmlWriter.Write(builder.ToString());
        }

        private void BeginTestCaseResultTable(string[] headers)
        {
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Table);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
            foreach (string header in headers)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Th);
                htmlWriter.Write(header);
                htmlWriter.RenderEndTag();
            }
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");

        }

        private void AddHtmlTableRowForTestCaseResult(string col1, string col2,string col3=null)
        {
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
            htmlWriter.Write(col1);
            htmlWriter.RenderEndTag();
            string bgcolor = "red";
            if (col2.Equals("Not Executed", StringComparison.OrdinalIgnoreCase))
                bgcolor = "yellow";
            if (col2.Equals("Passed", StringComparison.OrdinalIgnoreCase))
                bgcolor = "green";
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Bgcolor, bgcolor);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
            htmlWriter.Write(col2);
            htmlWriter.RenderEndTag();          
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.P);
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:Toggle('" + col1 + "div" + "','" + col1 + "link');");
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Id, col1 + "link");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
            htmlWriter.Write("Show details");
            htmlWriter.RenderEndTag();
            htmlWriter.RenderEndTag();
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Id, col1 +"div");
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Style, "display: none");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Div);
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Pre);
            htmlWriter.Write(col3);
            htmlWriter.RenderEndTag();
            htmlWriter.RenderEndTag();
            htmlWriter.RenderEndTag();
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }

        private void EndTag()
        {
            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");
        }

        private void AddHtmlTable(Dictionary<string, string> tableValues)
        {
            htmlWriter.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            htmlWriter.RenderBeginTag(HtmlTextWriterTag.Table);

            foreach (KeyValuePair<string, string> keyValuePair in tableValues)
            {
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                htmlWriter.Write(keyValuePair.Key);
                htmlWriter.RenderEndTag();
                htmlWriter.RenderBeginTag(HtmlTextWriterTag.Td);
                htmlWriter.Write(keyValuePair.Value);
                htmlWriter.RenderEndTag();
                htmlWriter.RenderEndTag();
                htmlWriter.WriteLine("");
            }

            htmlWriter.RenderEndTag();
            htmlWriter.WriteLine("");

        }
        #endregion PrivateMethods

        private StreamWriter _streamWriter;
        private StringWriter stringwriter;
        private HtmlTextWriter htmlWriter;
        private StreamWriter FileWriter
        {
            get
            {
                return _streamWriter;
            }
        }
    }
}


