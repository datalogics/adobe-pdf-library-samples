using System;
using Datalogics.PDFL;

namespace AddElements
{
    class AEReportProc : ReportProc
    {
        public override void Call(ReportType reportType, string message, string replacementText)
        {
            if (reportType == ReportType.Error)
            {
                Console.Write("Error: ");
            }
            else if (reportType == ReportType.Note)
            {
                Console.Write("Note: ");
            }
            else if (reportType == ReportType.Warning)
            {
                Console.Write("Warning:");
            }

            if (message != null)
            {
                if (replacementText != null)
                {
                    message = message.Replace("%s", replacementText);
                }

                Console.WriteLine(message);
            }
            else if (replacementText != null)
            {
                Console.WriteLine(replacementText);
            }
            else
            {
                Console.WriteLine("Unknown");
            }
        }
    }
}
