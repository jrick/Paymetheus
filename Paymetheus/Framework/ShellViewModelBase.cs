using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paymetheus.Framework
{
    public class ShellViewModelBase : ViewModelBase
    {
        private string _windowTitle;
        public string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; RaisePropertyChanged(); }
        }

        private DialogViewModelBase _visibleDialogContent;
        public DialogViewModelBase VisibleDialogContent
        {
            get { return _visibleDialogContent; }
            set { _visibleDialogContent = value; RaisePropertyChanged(); }
        }

        protected virtual bool OnRoutedMessage(ViewModelBase sender, ViewModelMessageBase message)
        {
            var openDialog = message as OpenDialogMessage;
            if (openDialog != null)
            {
                VisibleDialogContent = openDialog.Dialog;
                return true;
            }

            var hideDialog = message as HideDialogMessage;
            if (hideDialog != null && sender == VisibleDialogContent)
            {
                VisibleDialogContent = null;
                return true;
            }

            return false;
        }
    }
}
