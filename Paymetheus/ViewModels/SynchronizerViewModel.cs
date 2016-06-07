﻿// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.

using Paymetheus.Decred;
using Paymetheus.Decred.Wallet;
using Paymetheus.Framework;
using Paymetheus.Rpc;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Paymetheus.ViewModels
{
    public sealed class SynchronizerViewModel : ViewModelBase
    {
        private SynchronizerViewModel(Process walletProcess, WalletClient client)
        {
            WalletRpcProcess = walletProcess;
            WalletRpcClient = client;

            Messenger.RegisterSingleton<SynchronizerViewModel>(OnMessageReceived);
        }

        public Process WalletRpcProcess { get; }
        public WalletClient WalletRpcClient { get; }
        public Wallet Wallet { get; private set; }

        public static async Task<SynchronizerViewModel> Startup(BlockChainIdentity activeNetwork, string walletAppDataDir)
        {
            // Begin the asynchronous reading of the certificate before starting the wallet
            // process.  This uses filesystem events to know when to begin reading the certificate,
            // and if there is too much delay between wallet writing the cert and this process
            // beginning to observe the change, the event may never fire and the cert won't be read.
            var rootCertificateTask = TransportSecurity.ReadModifiedCertificateAsync(walletAppDataDir);

            var walletProcess = WalletProcess.Start(activeNetwork, walletAppDataDir);

            WalletClient walletClient;
            try
            {
                var listenAddress = WalletProcess.RpcListenAddress("localhost", activeNetwork);
                var rootCertificate = await rootCertificateTask;
                walletClient = await WalletClient.ConnectAsync(listenAddress, rootCertificate);
            }
            catch (Exception)
            {
                if (walletProcess.HasExited)
                {
                    throw new Exception("Wallet process closed unexpectedly");
                }
                walletProcess.KillIfExecuting();
                throw;
            }

            return new SynchronizerViewModel(walletProcess, walletClient);
        }

        public Amount TotalBalance => Wallet?.TotalBalance ?? 0;

        private int _syncedBlockHeight;
        public int SyncedBlockHeight
        {
            get { return _syncedBlockHeight; }
            set { _syncedBlockHeight = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<AccountViewModel> Accounts { get; } = new ObservableCollection<AccountViewModel>();

        private async void OnMessageReceived(IViewModelMessage message)
        {
            var startupWizardFinishedMessage = message as StartupWizardFinishedMessage;
            if (startupWizardFinishedMessage != null)
            {
                await OnWalletProcessOpenedWallet();
            }
        }

        private async Task OnWalletProcessOpenedWallet()
        {
            try
            {
                var syncingWallet = await WalletRpcClient.Synchronize(_wallet_ChangesProcessed);
                Wallet = syncingWallet.Item1;
                OnSyncedWallet();

                var syncTask = syncingWallet.Item2;
                await syncTask;
            }
            catch (ConnectTimeoutException)
            {
                MessageBox.Show("Unable to connect to wallet");
            }
            catch (Exception ex)
            {
                var ae = ex as AggregateException;
                if (ae != null)
                {
                    Exception inner;
                    if (ae.TryUnwrap(out inner))
                        ex = inner;
                }

                HandleSyncFault(ex);
            }
            finally
            {
                var wallet = Wallet;
                if (wallet != null)
                    wallet.ChangesProcessed -= _wallet_ChangesProcessed;
                var shell = (ShellViewModel)ViewModelLocator.ShellViewModel;
                shell.StartupWizardVisible = true;
            }
        }

        private static void HandleSyncFault(Exception ex)
        {
            string message;
            var shutdown = false;

            // Sync task ended.  Decide whether to restart the task and sync a
            // fresh wallet, or error out with an explanation.
            if (ErrorHandling.IsTransient(ex))
            {
                // This includes network issues reaching the wallet, like disconnects.
                message = $"A temporary error occurred, but reconnecting is not implemented.\n\n{ex}";
                shutdown = true; // TODO: implement reconnect logic.
            }
            else if (ErrorHandling.IsServerError(ex))
            {
                message = $"The wallet failed to service a request.\n\n{ex}";
            }
            else if (ErrorHandling.IsClientError(ex))
            {
                message = $"A client request could not be serviced.\n\n{ex}";
            }
            else
            {
                message = $"An unexpected error occurred:\n\n{ex}";
                shutdown = true;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, "Error");
                if (shutdown)
                    App.Current.Shutdown();
            });
        }

        private void _wallet_ChangesProcessed(object sender, Wallet.ChangesProcessedEventArgs e)
        {
            // TODO: The OverviewViewModel should probably connect to this event.  This could be
            // done after the wallet is synced.
            var overviewViewModel = ViewModelLocator.OverviewViewModel as OverviewViewModel;
            if (overviewViewModel != null)
            {
                var currentHeight = e.NewChainTip?.Height ?? SyncedBlockHeight;

                var movedTxViewModels = overviewViewModel.RecentTransactions
                    .Where(txvm => e.MovedTransactions.ContainsKey(txvm.TxHash))
                    .Select(txvm => Tuple.Create(txvm, e.MovedTransactions[txvm.TxHash]));

                var newTxViewModels = e.AddedTransactions.Select(tx => new TransactionViewModel(Wallet, tx.Item1, tx.Item2)).ToList();

                foreach (var movedTx in movedTxViewModels)
                {
                    var txvm = movedTx.Item1;
                    var location = movedTx.Item2;

                    txvm.Location = location;
                    txvm.Confirmations = BlockChain.Confirmations(currentHeight, location);
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var txvm in newTxViewModels)
                    {
                        overviewViewModel.RecentTransactions.Insert(0, txvm);
                    }
                });
            }

#if false
            if (VisibleContent is AccountViewModel)
            {
                var accountViewModel = (AccountViewModel)VisibleContent;
                AccountProperties accountProperties;
                if (e.ModifiedAccountProperties.TryGetValue(accountViewModel.Account, out accountProperties))
                {
                    accountViewModel.UpdateAccountProperties(1, accountProperties);
                }
            }
#endif
        }

        private void OnSyncedWallet()
        {
            var txSet = Wallet.RecentTransactions;
            var recentTx = txSet.UnminedTransactions
                .Select(x => new TransactionViewModel(Wallet, x.Value, BlockIdentity.Unmined))
                .Concat(txSet.MinedTransactions.ReverseList().SelectMany(b => b.Transactions.Select(tx => new TransactionViewModel(Wallet, tx, b.Identity))))
                .Take(10);
            var overviewViewModel = (OverviewViewModel)SingletonViewModelLocator.Resolve("Overview");
            overviewViewModel.AccountsCount = Wallet.EnumerateAccounts().Count();
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var tx in recentTx)
                    overviewViewModel.RecentTransactions.Add(tx);
            });
            SyncedBlockHeight = Wallet.ChainTip.Height;
            NotifyRecalculatedBalances();
            var shell = (ShellViewModel)ViewModelLocator.ShellViewModel;
            shell.StartupWizardVisible = false;
        }

        private void NotifyRecalculatedBalances()
        {
            RaisePropertyChanged(nameof(TotalBalance));

#if false
            // If account visible, calculate spendable balance
            if (_visibleContent is AccountViewModel)
            {
                var accountViewModel = (AccountViewModel)_visibleContent;
                var accountProperties = _wallet.LookupAccountProperties(accountViewModel.Account);
                accountViewModel.UpdateAccountProperties(1, accountProperties); // TODO: don't hardcode confs
                Application.Current.Dispatcher.Invoke(() =>
                {
                    accountViewModel.PopulateTransactionHistory();
                });
            }
#endif
        }
    }
}
