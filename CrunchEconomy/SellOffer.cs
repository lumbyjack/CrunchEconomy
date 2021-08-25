﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrunchEconomy
{
    public class SellOffer
    {
        public Boolean Enabled = false;
        public string typeId = "Ore";
        public string subtypeId = "Iron";
        public long minPrice = 1;
        public long maxPrice = 3;
        public double chance = 1;
        public Boolean SpawnItemsIfNeeded = false;
        public int SpawnIfCargoLessThan = 10;

        public Boolean BuyingGivesMiningContract = false;
        public string ContractName = "example";
        public Boolean BuyingGivesHaulingContract = false;
        public string IfGivesContractNPCTag = "GAIA";

        public Boolean IndividualRefreshTimer = false;
        public DateTime nextRefresh = DateTime.Now;
        public int SecondsBetweenRefresh = 600;
        public string path = "ignore me";
    }
}
