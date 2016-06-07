// Copyright (c) 2016 The btcsuite developers
// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using System;
using System.Windows.Input;

namespace Paymetheus.Framework
{
    public class DialogViewModelBase : ViewModelBase
    {
        public DialogViewModelBase() : base()
        {
            HideDialogCommand = new DelegateCommand(HideDialog);
        }

        public ICommand HideDialogCommand { get; }

        public void HideDialog()
        {
            throw new NotImplementedException();
        }
    }

    sealed class OpenDialogMessage : IViewModelMessage
    {
        public OpenDialogMessage(DialogViewModelBase dialog)
        {
            Dialog = dialog;
        }

        public DialogViewModelBase Dialog { get; }
    }

    sealed class HideDialogMessage : IViewModelMessage { }
}
