// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using Paymetheus.Decred.Wallet;
using Paymetheus.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paymetheus.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        public AccountViewModel(Account account, AccountProperties properties, Balances balances) : base()
        {
            Account = account;
            _accountProperties = properties;
            _balances = balances;
        }

        public Account Account { get; }

        public uint AccountNumber => Account.AccountNumber;

        private AccountProperties _accountProperties;
        public AccountProperties AccountProperties
        {
            get { return _accountProperties; }
            internal set { _accountProperties = value; RaisePropertyChanged(); }
        }

        private Balances _balances;
        public Balances Balances
        {
            get { return _balances; }
            internal set { _balances = value; RaisePropertyChanged(); }
        }
    }
}
