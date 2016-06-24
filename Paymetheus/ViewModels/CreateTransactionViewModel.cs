﻿// Copyright (c) 2016 The btcsuite developers
// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using Paymetheus.Decred;
using Paymetheus.Decred.Script;
using Paymetheus.Decred.Util;
using Paymetheus.Decred.Wallet;
using Paymetheus.Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Paymetheus.ViewModels
{
    class CreateTransactionViewModel : ViewModelBase
    {
        public CreateTransactionViewModel() : base()
        {
            var synchronizer = ViewModelLocator.SynchronizerViewModel as SynchronizerViewModel;
            if (synchronizer != null)
            {
                SelectedAccount = synchronizer.Accounts[0];
            }

            AddPendingOutputCommand = new DelegateCommand(AddPendingOutput);
            RemovePendingOutputCommand = new DelegateCommand<PendingOutput>(RemovePendingOutput);
            FinishCreateTransaction = new ButtonCommand("Send", FinishCreateTransactionAction);
            FinishCreateTransaction.Executable = false;

            AddPendingOutput();
        }

        private AccountViewModel _selectedAccount;
        public AccountViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set { _selectedAccount = value; RaisePropertyChanged(); }
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
                    try
                    {
                        switch (OutputKind)
                        {
                            case Kind.Address:
                                _destinationAddress = Address.Decode(value);
                                if (_destinationAddress.IntendedBlockChain != App.Current.ActiveNetwork)
                                {
                                    throw new ArgumentException("Address is intended for use on another block chain");
                                }
                                break;
                            case Kind.Script:
                                _destinationScript = Hexadecimal.Decode(value);
                                break;
                        }
                        _destination = value;
                        DestinationValid = true;
                    }
                    catch
                    {
                        DestinationValid = false;
                        if (value != "")
                            throw;
                    }
                    finally
                    {
                        RaiseChanged();
                    }
                }
            }

            private Amount _outputAmount;
            public string OutputAmount
            {
                get { return _outputAmount.ToString(); }
                set
                {
                    try
                    {
                        var outputAmount = Denomination.Decred.AmountFromString(value);
                        if (_outputAmount < 0)
                        {
                            throw new ArgumentException("Output amount may not be negative");
                        }
                        _outputAmount = outputAmount;
                        OutputAmountValid = true;
                    }
                    catch
                    {
                        OutputAmountValid = false;
                        throw;
                    }
                    finally
                    {
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
                if (!DestinationValid)
                {
                    throw new Exception("Unable to build output script from invalid destination");
                }

                switch (OutputKind)
                {
                    case Kind.Address:
                        return _destinationAddress.BuildScript();

                    case Kind.Script:
                        return new OutputScript.Unrecognized(_destinationScript);

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

        public ICommand AddPendingOutputCommand { get; }
        public ICommand RemovePendingOutputCommand { get; }

        private void AddPendingOutput()
        {
            var pendingOutput = new PendingOutput();
            pendingOutput.Changed += PendingOutput_Changed;
            PendingOutputs.Add(pendingOutput);
            RecalculateTransaction();
        }

        private void RemovePendingOutput(PendingOutput item)
        {
            if (PendingOutputs.Remove(item))
            {
                item.Changed -= PendingOutput_Changed;

                if (PendingOutputs.Count == 0)
                {
                    AddPendingOutput();
                }

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

                // TODO: only make executable if we know the transaction can be created
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
