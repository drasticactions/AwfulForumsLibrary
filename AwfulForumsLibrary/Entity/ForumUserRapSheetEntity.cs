using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Entity
{
    public enum PunishmentType
    {
        Probation,
        Ban,
        Permaban,
        Autoban,
        Other
    }

    public class ForumUserRapSheetEntity
    {
        public PunishmentType PunishmentType { get; private set; }

        public DateTime Date { get; private set; }

        public string HorribleJerk { get; private set; }

        public string PunishmentReason { get; private set; }

        public string RequestedBy { get; private set; }

        public string ApprovedBy { get; private set; }

        public long ThreadId { get; private set; }

        public long HorribleJerkId { get; private set; }

        public long ApprovedById { get; private set; }

        public long RequestedById { get; private set; }

        public string Link { get; private set; }

        private DateTime GetDateTime(string date)
        {
            Regex r = new Regex(@"(?!^)(?=[A-Z])");
            return DateTime.ParseExact(r.Replace(date, " "), "MM/dd/yy hh:mm tt", new CultureInfo("en-US"), DateTimeStyles.AllowWhiteSpaces);
        }

        private void GetPunishmentType(string pun)
        {
            switch (pun)
            {
                case "PROBATION":
                    PunishmentType = PunishmentType.Probation;
                    break;
                case "AUTOBAN":
                    PunishmentType = PunishmentType.Autoban;
                    break;
                case "BAN":
                    PunishmentType = PunishmentType.Ban;
                    break;
                case "PERMABAN":
                    PunishmentType = PunishmentType.Permaban;
                    break;
                default:
                    PunishmentType = PunishmentType.Other;
                    break;
            }
        }

        public void FromRapSheet(HtmlNode rapSheetNode)
        {
            List<HtmlNode> rapSheetData = rapSheetNode.Descendants("td").ToList();
            GetPunishmentType(rapSheetData[0].Descendants("b").FirstOrDefault().InnerText);
            Link =
                rapSheetData[0].Descendants("b")
                    .FirstOrDefault()
                    .Descendants("a")
                    .FirstOrDefault()
                    .GetAttributeValue("href", string.Empty);
            Date = GetDateTime(rapSheetData[1].InnerText);

            HorribleJerk = rapSheetData[2].Descendants("a").FirstOrDefault().InnerText;
            HorribleJerkId =
                Convert.ToInt64(
                    rapSheetData[2].Descendants("a").FirstOrDefault().GetAttributeValue("href", string.Empty).Split('=')
                        [3]);

            PunishmentReason = rapSheetData[3].InnerText;

            RequestedBy = rapSheetData[4].Descendants("a").FirstOrDefault().InnerText;
            RequestedById =
                Convert.ToInt64(
                    rapSheetData[4].Descendants("a").FirstOrDefault().GetAttributeValue("href", string.Empty).Split('=')
                        [3]);

            ApprovedBy = rapSheetData[5].Descendants("a").FirstOrDefault().InnerText;
            ApprovedById =
                Convert.ToInt64(
                    rapSheetData[5].Descendants("a").FirstOrDefault().GetAttributeValue("href", string.Empty).Split('=')
                        [3]);
        }

    }
}
