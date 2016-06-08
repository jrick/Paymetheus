// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using Paymetheus.Decred.Wallet;
using Paymetheus.Framework;
using System;
using System.Windows;
using System.Windows.Input;

namespace Paymetheus.ViewModels
{
    public sealed class RequestViewModel : ViewModelBase
    {
        public RequestViewModel() : base()
        {
            _generateAddressCommand = new DelegateCommand(GenerateAddressAction);
        }

        private string _generatedAddress;
        public string GeneratedAddress
        {
            get { return _generatedAddress; }
            set { _generatedAddress = value; RaisePropertyChanged(); }
        }

        private DelegateCommand _generateAddressCommand;
        public ICommand GenerateAddressCommand => _generateAddressCommand;

        private async void GenerateAddressAction()
        {
            try
            {
                _generateAddressCommand.Executable = false;
                var account = new Account(0); // TODO: use selected account
                var address = await App.Current.Synchronizer.WalletRpcClient.NextExternalAddressAsync(account);
                GeneratedAddress = address;
            }
            catch (Exception ex)
            {
                // TODO: stop using messagebox for error reporting
                MessageBox.Show($"Error occurred when requesting address:\n\n{ex}", "Error");
            }
            finally
            {
                _generateAddressCommand.Executable = true;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
