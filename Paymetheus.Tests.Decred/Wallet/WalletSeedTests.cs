// Copyright (c) 2016 The btcsuite developers
// Copyright (c) 2016 The Decred developers
// Licensed under the ISC license.  See LICENSE file in the project root for full license information.using Paymetheus.Decred.Wallet;

using Paymetheus.Decred.Util;
using Paymetheus.Decred.Wallet;
using System.Collections.Generic;
using Xunit;

namespace Paymetheus.Tests.Decred.Wallet
{
    public static class WalletSeedTests
    {
        private static PgpWordList PgpWordList = new PgpWordList();

        public static IEnumerable<object[]> PositiveTests()
        {
            return new[]
            {
                // with checksum
                new object[] {
                    "497497071bdbdf3fccdfddcf828dd18aac4493eda269253753f99897b84fd688",
                    "deckhand hydraulic preshrunk amusement beeswax suspicious talon customer spigot therapist swelter Saturday miser microscope stairway maverick ribcage designing playhouse unify rebirth guitarist bombast consensus dwelling Waterloo printer mosquito select document stockman maritime spearhead",
                },

                // without checksum
                new object[] {
                    "497497071bdbdf3fccdfddcf828dd18aac4493eda269253753f99897b84fd688",
                    "deckhand hydraulic preshrunk amusement beeswax suspicious talon customer spigot therapist swelter Saturday miser microscope stairway maverick ribcage designing playhouse unify rebirth guitarist bombast consensus dwelling Waterloo printer mosquito select document stockman maritime",
                },

                // identical hex
                new object[] {
                    "497497071bdbdf3fccdfddcf828dd18aac4493eda269253753f99897b84fd688",
                    "497497071bdbdf3fccdfddcf828dd18aac4493eda269253753f99897b84fd688",
                },

                // hex with checksum
                new object[] {
                    "497497071bdbdf3fccdfddcf828dd18aac4493eda269253753f99897b84fd688",
                    "497497071bdbdf3fccdfddcf828dd18aac4493eda269253753f99897b84fd68800",
                },
            };
        }

        [Theory]
        [MemberData(nameof(PositiveTests))]
        public static void DecodeAndValidateUserInputTest(string seed, string pgpSeed)
        {
            var decodedSeed = WalletSeed.DecodeAndValidateUserInput(seed, PgpWordList);
            var decodedSeedHex = Hexadecimal.Decode(seed);

            Assert.Equal(decodedSeedHex, decodedSeed);
        }
    }
}