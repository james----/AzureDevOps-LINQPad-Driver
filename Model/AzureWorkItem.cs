using Humanizer;
using LINQPad;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace AzureDevOpsDataContextDriver
{
    public class AzureWorkItem
    {
        private AzureDevOpsConnectionInfo ConnectionInfo;

        public AzureWorkItem(AzureDevOpsConnectionInfo conn)
        {
            ConnectionInfo = conn;
            Children = new List<AzureWorkItem>();
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public string ItemType { get; set; }

        public string State { get; set; }

        public string IterationPath { get; set; }

        public string AssignedTo { get; set; }

        public string Blocked { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime AssignedOn { get; set; }

        public DateTime ClosedDate { get; set; }

        public TimeSpan Elapsed
        {
            get
            {
                if (ClosedDate.Year > 1900)
                {
                    return ClosedDate - CreatedDate;
                }
                else
                {
                    return TimeSpan.MinValue;
                }
            }
        }

        public double BacklogPriority { get; set; }

        public AzureWorkItem Parent { get; set; }

        public IList<AzureWorkItem> Children { get; set; }

        public object ToDump()
        {
            return InternalDump(this);
        }

        static IDictionary<string, object> InternalDump(AzureWorkItem item)
        {
            IDictionary<string, object> custom = new ExpandoObject();
            foreach (var p in item.GetType().GetProperties())
            {
                if (p.Name == "BacklogPriority") continue;
                if (p.Name == "CreatedDate")
                {
                    if (item.CreatedDate.Year > 1900)
                    {
                        custom[p.Name] = item.CreatedDate.ToString("dd-MMM-yyyy HH:mm:dd");
                    }
                    else
                    {
                        custom[p.Name] = string.Empty;
                    }
                    continue;
                }
                else if (p.Name == "ClosedDate")
                {
                    if (item.ClosedDate.Year > 1900)
                    {
                        custom[p.Name] = item.ClosedDate.ToString("dd-MMM-yyyy HH:mm:dd");
                    }
                    else
                    {
                        custom[p.Name] = string.Empty;
                    }
                    continue;
                }
                else if (p.Name == "AssignedToDate")
                {
                    if (item.AssignedOn.Year > 1900)
                    {
                        custom[p.Name] = item.AssignedOn.ToString("dd-MMM-yyyy HH:mm:dd");
                    }
                    else
                    {
                        custom[p.Name] = string.Empty;
                    }
                    continue;
                }
                else if (p.Name == "Id")
                {
                    custom[p.Name] = new Hyperlinq($"{item.ConnectionInfo.Url}/_workitems/edit/{item.Id}", item.Id.ToString());
                }
                else if (p.Name == "Elapsed")
                {
                    custom[p.Name] = item.Elapsed.Humanize();
                    continue;
                }
                else if (p.Name == "Children")
                {
                    if (item.Children.Count > 0)
                    {
                        custom[p.Name] = item.Children.Select(c => InternalDump(c));
                    }
                    else
                    {
                        custom[p.Name] = string.Empty;
                    }
                    continue;
                }
                else if (p.Name == "Parent")
                {
                    if (item.Parent != null)
                    {
                        custom[p.Name] = item.Parent.ToDump();
                    }
                    else
                    {
                        custom[p.Name] = string.Empty;
                    }
                    continue;
                }
                custom[p.Name] = p.GetValue(item);
            }
            return custom;
        }
    }
}
