using FubuLocalization;
using FubuMVC.Core.Ajax;
using FubuValidation;
using System.Collections.Generic;

namespace FubuFastPack
{
    public static class AjaxContinuationExtensions
    {
        public const string ShowPage = "showPage";
        public const string NavigatePage = "navigatePage";
        public const string Target = "Target";
        public const string ShowDialog = "showDialog";

        public static AjaxContinuation WithSubmission(this AjaxContinuation ajaxContinuation, Notification notification, object target)
        {
            ajaxContinuation[Target] = target;
            ajaxContinuation.Success = notification.IsValid();
            foreach (var validationError in notification.ToValidationErrors())
            {
                ajaxContinuation.Errors.Add(new AjaxError {field = validationError.field, message = validationError.message});
            }
            return ajaxContinuation;
        }

        public static AjaxContinuation ForDialog(string url, object target)
        {
            var returnValue = new AjaxContinuation();
            returnValue[ShowDialog] = url;
            returnValue[Target] = target;
            return returnValue;
        }

        public static AjaxContinuation ForMessage(StringToken key, object target)
        {
            var returnValue = AjaxContinuation.ForMessage(key);
            returnValue[Target] = target;
            return returnValue;
        }

        public static AjaxContinuation ForPage(string url, object target)
        {
            var returnValue = new AjaxContinuation();
            returnValue[ShowPage] = url;
            returnValue[Target] = target;
            return returnValue;
        }

        public static AjaxContinuation ForRefresh(object target)
        {
            var returnValue = new AjaxContinuation();
            returnValue[Target] = target;
            returnValue.Success = true;
            returnValue.ShouldRefresh = true;
            return returnValue;
        }

        public static AjaxContinuation ForSuccess()
        {
            return new AjaxContinuation(){Success = true};
        }

        public static AjaxContinuation ForError(Notification notification)
        {
            var ajaxContinuation = new AjaxContinuation
            {
                Success = notification.IsValid()
            };
            notification.ToValidationErrors().Each(
                x => ajaxContinuation.Errors.Add(new AjaxError() {field = x.field, message = x.message}));
            return ajaxContinuation;
        }

        public static AjaxContinuation ForNavigateWholePage(string url)
        {
            var forNavigateWholePage = new AjaxContinuation
            {
                Success = true
            };
            forNavigateWholePage[NavigatePage] = url;
            return forNavigateWholePage;
        }

        public static object TryGetData(this AjaxContinuation ajaxContinuation, string key)
        {           
            object returnValue = null;
            if(ajaxContinuation.HasData(key))
            {
                returnValue = ajaxContinuation[key];
            }
            
            return returnValue;
        }
    }
}