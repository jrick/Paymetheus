// Copyright (c) 2016 The btcsuite developers
// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using Paymetheus.Decred;
using Paymetheus.Decred.Script;
using Paymetheus.Decred.Util;
using Paymetheus.Decred.Wallet;
using Paymetheus.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Paymetheus.ViewModels
{
    class CreateTransactionViewModel : ViewModelBase
    {
        public CreateTransactionViewModel() : base()
        {
            RemovePendingOutput = new DelegateCommand<PendingOutput>(RemovePendingOutputAction);
            FinishCreateTransaction = new ButtonCommand("Send", FinishCreateTransactionAction);
        }

        public class PendingOutput
        {
            public enum Kind
            {
                Address,
                Script,
            }

            private Kind _outputKind;
            public Kind OutputKind
            {
                get { return _outputKind; }
                set { _outputKind = value; RaiseChanged(); }
            }

            private string _destination;
            public string Destination
            {
                get { return _destination; }
                set { _destination = value; RaiseChanged(); }
            }

            public Amount _outputAmount;
            public Amount OutputAmount
            {
                get { return _outputAmount; }
                set
                {
                    if (_outputAmount != value)
                    {
                        _outputAmount = value;
                        RaiseChanged();
                    }
                }
            }

            /// <summary>
            /// Checks whether all user-set fields of the pending output are ready to be used
            /// to create a transaction output.
            /// </summary>
            /// <returns>Validity of the pending output</returns>
            public bool IsValid()
            {
                if (string.IsNullOrWhiteSpace(Destination))
                {
                    return false;
                }

                if (!TransactionRules.IsSaneOutputValue(OutputAmount))
                {
                    return false;
                }

                return true;
            }

            public event EventHandler Changed;

            private void RaiseChanged()
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }

            public OutputScript BuildOutputScript()
            {
                switch (OutputKind)
                {
                    case Kind.Address:
                        var address = Address.Decode(Destination);
                        return address.BuildScript();

                    case Kind.Script:
                        return new OutputScript.Unrecognized(Hexadecimal.Decode(Destination));

                    default:
                        throw new Exception($"Unknown pending output kind {OutputKind}");
                }
            }
        }

        public ObservableCollection<PendingOutput> PendingOutputs { get; } = new ObservableCollection<PendingOutput>
        {
            new PendingOutput(),
        };

        private Amount? _estimatedRemainingBalance;
        public Amount? EstimatedRemainingBalance
        {
            get { return _estimatedRemainingBalance; }
            set { _estimatedRemainingBalance = value; RaisePropertyChanged(); }
        }

        private Amount? _estimatedFee;
        public Amount? EstimatedFee
        {
            get { return _estimatedFee; }
            set { _estimatedFee = value; RaisePropertyChanged(); }
        }

        public ButtonCommand FinishCreateTransaction { get; }

        public ICommand RemovePendingOutput { get; }

        private void RemovePendingOutputAction(PendingOutput item)
        {
            if (PendingOutputs.Remove(item))
            {
                item.Changed -= PendingOutput_Changed;
                RecalculateTransaction();
            }
        }

        private void PendingOutput_Changed(object sender, EventArgs e)
        {
            RecalculateTransaction();
        }

        private void RecalculateTransaction()
        {
            if (PendingOutputs.Count > 0 && PendingOutputs.All(x => x.IsValid()))
            {
                EstimatedFee = 0;
                EstimatedRemainingBalance = 0;
                FinishCreateTransaction.Executable = true;
            }
            else
            {
                EstimatedFee = null;
                EstimatedRemainingBalance = null;
                FinishCreateTransaction.Executable = false;
            }
        }

        private void FinishCreateTransactionAction()
        {
            throw new NotImplementedException();
        }
    }
}
