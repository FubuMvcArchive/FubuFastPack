using FubuMVC.Core.Ajax;
using FubuValidation;

namespace FubuFastPack.Crud.Properties
{
    public class UpdatePropertyResultViewModel : AjaxContinuation
    {
        public UpdatePropertyResultViewModel()
        {
        }

        public UpdatePropertyResultViewModel(Notification notification, string valueToDisplay)
        {
            this.WithSubmission(notification);
            this[AjaxContinuationExtensions.NewValueToDisplay] = valueToDisplay;
        }        
    }
}