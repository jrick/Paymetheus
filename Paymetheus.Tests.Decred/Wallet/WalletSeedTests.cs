// Copyright (c) 2016 The btcsuite developers
// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.using Paymetheus.Decred.Wallet;

using Paymetheus.Decred.Wallet;
using Xunit;

namespace Paymetheus.Tests.Decred.Wallet
{
    class WalletSeedTests
    {
        private static PgpWordList PgpWordList = new PgpWordList();

        [Fact]
        public static void DecodeAndValidateUserInputTest()
        {
            var decodedSeed = WalletSeed.DecodeAndValidateUserInput("moo", PgpWordList);
        }
    }
}