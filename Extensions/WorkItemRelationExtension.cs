using System.Linq;

namespace Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models
{
    public static class WorkItemRelationExtension
    {
        public static int GetItemId(this WorkItemRelation o)
        {
            int id = 0;
            if (!string.IsNullOrWhiteSpace(o.Url))
            {
                int.TryParse(o.Url.Split('/').Last(), out id);
            }
            return id;
        }
    }
}
