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
            var firstPendingOutput = new PendingOutput();
            firstPendingOutput.Changed += PendingOutput_Changed;
            PendingOutputs.Add(firstPendingOutput);

            RemovePendingOutput = new DelegateCommand<PendingOutput>(RemovePendingOutputAction);
            FinishCreateTransaction = new ButtonCommand("Send", FinishCreateTransactionAction);
            FinishCreateTransaction.Executable = false;
        }

        public class PendingOutput
        {
            bool DestinationValid = false; // No default destination
            bool OutputAmountValid = true; // Output amount defaults to zero, which is valid.

            /// <summary>
            /// Checks whether all user-set fields of the pending output are ready to be used
            /// to create a transaction output.
            /// </summary>
            /// <returns>Validity of the pending output</returns>
            public bool IsValid => DestinationValid && OutputAmountValid;

            public enum Kind
            {
                Address,
                Script,
            }

            private Kind _outputKind = Kind.Address;
            public Kind OutputKind
            {
                get { return _outputKind; }
                set { _outputKind = value; RaiseChanged(); }
            }

            private string _destination;
            private Address _destinationAddress;
            private byte[] _destinationScript;
            public string Destination
            {
                get { return _destination; }
                set
                {
                    _destination = value;

                    var valid = false;
                    switch (OutputKind)
                    {
                        case Kind.Address:
                            valid = Address.TryDecode(value, out _destinationAddress);
                            break;
                        case Kind.Script:
                            valid = Hexadecimal.TryDecode(value, out _destinationScript);
                            break;
                    }

                    DestinationValid = valid;
                    RaiseChanged();

                    if (!valid && value != "")
                    {
                        // TODO: this goes in a tooltip, needs a better message.
                        throw new ArgumentException("Invalid input");
                    }
                }
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

        public ObservableCollection<PendingOutput> PendingOutputs { get; } = new ObservableCollection<PendingOutput>();

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

        private bool _signChecked = true;
        public bool SignChecked
        {
            get { return _signChecked; }
            set { _signChecked = value; RaisePropertyChanged(); }
        }

        private bool _publishChecked = true;
        public bool PublishChecked
        {
            get { return _publishChecked; }
            set { _publishChecked = value; RaisePropertyChanged(); }
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
            if (PendingOutputs.Count > 0 && PendingOutputs.All(x => x.IsValid))
            {
                // TODO: calculate estimated fee
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
