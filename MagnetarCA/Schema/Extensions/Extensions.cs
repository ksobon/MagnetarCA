using System.IO;

namespace MagnetarCA.Schema.Extensions
{
    public static class Extensions
    {
        public static string GetProjectFolder(this Project p)
        {
            return Path.Combine(p.Root, $"{p.Name} {p.Number}");
        }

        public static string GetRfiFolder(this Rfi r)
        {
            return Path.Combine(r.Root, $"RFI_{r.Number}");
        }

        public static string GetProjectDetailPath(this Project p)
        {
            return Path.Combine(p.GetProjectFolder(), $"Project\\ProjectInfo_Sync\\project_detail_{p.Number}.json");
        }

        public static string GetCompanyDetailPath(this Company c)
        {
            return Path.Combine(c.Root, $"company_{c.Name}.json");
        }

        public static string GetCompanyFolder(this Project p)
        {
            return Path.Combine(p.GetProjectFolder(), "Project\\Company_Sync");
        }

        public static string GetRfiFolder(this Project p)
        {
            return Path.Combine(p.GetProjectFolder(), "CA\\RFI\\RFI_Sync");
        }

        public static string GetResponsesFolder(this Rfi r)
        {
            return Path.Combine(r.GetRfiFolder(), "Responses");
        }

        public static string GetRfiDetailPath(this Rfi r)
        {
            return Path.Combine(r.GetRfiFolder(), $"rfi_detail_{r.Number}.json");
        }

        public static string GetRfiResponseDetailPath(this Response r)
        {
            return Path.Combine(r.Root, $"rfi_response_{r.Number:D3}.json");
        }

        public static string GetRfiAttachmentPath(this Rfi r, string aPath)
        {
            return Path.Combine(r.GetRfiFolder(), $"Attachments\\{Path.GetFileName(aPath)}");
        }

        public static string GetRfiResponseAttachmentPath(this Response r, string aPath)
        {
            return Path.Combine(r.Root, $"..\\Attachments\\{r.GetRfiResponseAttachmentName(aPath)}");
        }

        #region Utilities

        private static string GetRfiResponseAttachmentName(this Response r, string aPath)
        {
            return $"{r.Number:D3}_{Path.GetFileName(aPath)}";
        }

        #endregion
    }
}
