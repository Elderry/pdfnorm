using System;
using System.Collections.Generic;

namespace Pdfnorm.Common
{
    internal class IssueUtils
    {
        public static void Fix(
            bool assert,
            string issueMessage,
            string pdfName,
            bool dry = false,
            List<FixRecord> fixRecords = null,
            Action recoverAction = null,
            string recoverMessage = null)
        {
            if (!assert) { return; }

            PrintUtils.WriteLine($"[<DarkMagenta>{pdfName}</DarkMagenta>] {issueMessage}");

            if (recoverAction == null) { return; }
            PrintUtils.WriteLine($"[<DarkMagenta>{pdfName}</DarkMagenta>] {recoverMessage}");

            if (dry) { return; }
            fixRecords!.Add(new FixRecord(issueMessage));
            recoverAction.Invoke();
        }
    }
}
