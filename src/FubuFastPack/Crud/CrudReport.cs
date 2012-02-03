using System;
using FubuMVC.Core.Ajax;
using FubuValidation;

namespace FubuFastPack.Crud
{
    public class CrudReport : AjaxContinuation
    {
        public CrudReport()
        {
        }

        public CrudReport(Notification notification, object target, object theFlattenedValue)
        {
            this.WithSubmission(notification, target);
            flattenedValue = theFlattenedValue;
        }

        public object flattenedValue { get; set; }
        public string editUrl { get; set; }
        public Guid Id { get; set; }
    }
}