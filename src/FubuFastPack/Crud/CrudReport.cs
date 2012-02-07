using System;
using System.Collections.Generic;
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
            FlattenedValue = theFlattenedValue;
        }

        public object FlattenedValue { get; set; }
        public string EditUrl { get; set; }
        public Guid Id { get; set; }

        public override IDictionary<string, object> ToDictionary()
        {
            var returnValue = base.ToDictionary();
            returnValue.Add("flattenedValue", FlattenedValue);
            returnValue.Add("editUrl", EditUrl);
            returnValue.Add("Id", Id);
            return returnValue;
        }
    }
}