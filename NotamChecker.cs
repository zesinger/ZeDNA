using OpenQA.Selenium.DevTools.V130.Page;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;

public class NotamChecker
{
    private DateTime du;
    private DateTime au;
    private List<(DateTime fromDate, DateTime toDate, TimeSpan startTime, TimeSpan endTime)> dateRanges = 
        new List<(DateTime, DateTime , TimeSpan , TimeSpan)>();
    private string nname;

    public NotamChecker(string nzone, string notamresult)
    {
        string resultR18;
        nname = nzone;
        int offsNotam = notamresult.IndexOf(nzone.ToLower(), StringComparison.OrdinalIgnoreCase);
        string DuLine = default, DLine = default;
        if (offsNotam != -1)
        {
            // LastIndexOf cherche le dernier "DU:" en partant de offsNotam et en remontant
            int lastDuIndex = notamresult.LastIndexOf("DU:", offsNotam, StringComparison.OrdinalIgnoreCase);
            if (lastDuIndex != -1)
            {
                resultR18 = notamresult.Substring(lastDuIndex, offsNotam - lastDuIndex);
                string[] lines = resultR18.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                // cette ligne existe forcément puisqu'on a une chaine qui commence par "DU:"
                DuLine = lines.FirstOrDefault(line => line.StartsWith("DU:"));
                DLine = lines.FirstOrDefault(line => line.StartsWith("D)"));
            }
            else
            {
                au = du = new DateTime(1753, 1, 1);
                return;
            }
        }
        else
        {
            au = du = new DateTime(1753, 1, 1);
            return;
        }

        ParseDUAU(DuLine);
        if (DLine != default) ParseDLine(DLine);
        else
        {
            (DateTime fromDate, DateTime toDate, TimeSpan startTime, TimeSpan endTime) ndR = (du.Date, au.Date, du.TimeOfDay, au.TimeOfDay);
            dateRanges.Add(ndR);
        }
    }

    private void ParseDUAU(string line1)
    {
        // Example: DU: 20 01 2025 00:00 AU: 28 02 2025 22:00
        var duMatch = Regex.Match(line1, @"DU:\s*(\d{2}) (\d{2}) (\d{4}) (\d{2}):(\d{2})");
        var auMatch = Regex.Match(line1, @"AU:\s*(\d{2}) (\d{2}) (\d{4}) (\d{2}):(\d{2})");

        du = new DateTime(
            int.Parse(duMatch.Groups[3].Value),
            int.Parse(duMatch.Groups[2].Value),
            int.Parse(duMatch.Groups[1].Value),
            int.Parse(duMatch.Groups[4].Value),
            int.Parse(duMatch.Groups[5].Value),
            0
        );

        au = new DateTime(
            int.Parse(auMatch.Groups[3].Value),
            int.Parse(auMatch.Groups[2].Value),
            int.Parse(auMatch.Groups[1].Value),
            int.Parse(auMatch.Groups[4].Value),
            int.Parse(auMatch.Groups[5].Value),
            0
        );
    }

    private void ParseDLine(string line2)
    {
        // Remove leading 'D)'
        var content = line2.Substring(2).Trim();

        // Split parts by semicolon if multiple months/patterns
        var parts = content.Split(new char[]{ ';',','});

        foreach (var part in parts)
        {
            var trimmed = part.Trim();

            // Regex: Optional month
            var monthMatch = Regex.Match(trimmed, @"^(?<month>[A-Z]{3})?\s*(?<ranges>(?:\d{2}-\d{2}\s*)*)(?<time>\d{4}-\d{4})");

            string monthStr = monthMatch.Groups["month"].Success ? monthMatch.Groups["month"].Value : null;
            string rangesStr = monthMatch.Groups["ranges"].Value.Trim();
            string timeRangeStr = monthMatch.Groups["time"].Value;

            TimeSpan startTime = TimeSpan.ParseExact(timeRangeStr.Substring(0, 4), "hhmm", CultureInfo.InvariantCulture);
            TimeSpan endTime = TimeSpan.ParseExact(timeRangeStr.Substring(5, 4), "hhmm", CultureInfo.InvariantCulture);

            int assumedYear = du.Year;
            int assumedMonth = du.Month;

            if (monthStr != null)
            {
                assumedMonth = DateTime.ParseExact(monthStr, "MMM", CultureInfo.InvariantCulture).Month;

                // Adjust year: if DU month > parsed month → assume next year if AU year is after DU year
                if (assumedMonth < du.Month && au.Year > du.Year)
                    assumedYear = au.Year;
            }

            // Case: No day ranges, only time range → valid entire DU-AU period
            if (string.IsNullOrWhiteSpace(rangesStr))
            {
                dateRanges.Add((du.Date, au.Date, startTime, endTime));
                continue;
            }

            // Parse day ranges
            var rangeMatches = Regex.Matches(rangesStr, @"(\d{2})-(\d{2})");

            foreach (Match rm in rangeMatches)
            {
                int startDay = int.Parse(rm.Groups[1].Value);
                int endDay = int.Parse(rm.Groups[2].Value);

                DateTime fromDate = new DateTime(assumedYear, assumedMonth, startDay);
                DateTime toDate = new DateTime(assumedYear, assumedMonth, endDay);

                // Clip to DU/AU
                if (fromDate < du.Date) fromDate = du.Date;
                if (toDate > au.Date) toDate = au.Date;

                dateRanges.Add((fromDate, toDate, startTime, endTime));
            }
        }
    }

    /// <summary>
    /// Checks if the provided DateTime (date only) is within the NOTAM validity range and active days.
    /// Returns true and create a zone if active
    /// </summary>
    public bool IsActive(string nzone, DateTime dt, ref List<ZeDNA.Zone> zones)
    {
        // si la zone demandée n'est pas celle de ce notam, on quitte
        if (nzone != nname) return false;
        // si on est en dehors de la plage DU -> AU, on quitte
        if (dt < du || dt > au)
            return false;
        // on vérifie que le jour est précisément dans les dates d'activité
        foreach (var (fromDate, toDate, start, end) in dateRanges)
        {
            if (dt.Date >= fromDate && dt.Date <= toDate)
            {
                ZeDNA.Zone szone = new ZeDNA.Zone(nzone);
                DateTime duDateTime = new DateTime(fromDate.Date.Year, fromDate.Date.Month, fromDate.Date.Day, start.Hours, start.Minutes, start.Seconds);
                DateTime auDateTime = new DateTime(toDate.Date.Year, toDate.Date.Month, toDate.Date.Day, end.Hours, end.Minutes, end.Seconds);
                szone.SetTime(0, duDateTime, auDateTime);
                zones.Add(szone);
                return true;
            }
        }
        return false;
    }
}
