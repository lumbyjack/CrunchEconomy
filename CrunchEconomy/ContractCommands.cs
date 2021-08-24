﻿using CrunchEconomy.Contracts;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using VRage.Game.ModAPI;

namespace CrunchEconomy
{
    [Category("contract")]
    public class ContractCommands : CommandModule
    {
        public static Dictionary<ulong, Dictionary<int, Guid>> ids = new Dictionary<ulong, Dictionary<int, Guid>>();
        public static Dictionary<ulong, int> playerMax = new Dictionary<ulong, int>();

        [Command("quit", "quit current contracts")]
        [Permission(MyPromoteLevel.None)]
        public void DoContractDetails(int contractnum)
        {
            if (CrunchEconCore.playerData.TryGetValue(Context.Player.SteamUserId, out PlayerData data))
            {
                ids.TryGetValue(Context.Player.SteamUserId, out Dictionary<int, Guid> derp);
                if (derp == null)
                {
                    Context.Respond("I dont know the ids! Use !contract info");
                    return;
                }

                if (derp.ContainsKey(contractnum))
                {
                    Contract cancel = data.getMiningContracts()[derp[contractnum]];
                    data.MiningReputation -= cancel.reputation * 2;

                    data.getMiningContracts().Remove(derp[contractnum]);
                    data.MiningContracts.Remove(derp[contractnum]);
                    derp.Remove(contractnum);
                    ids[Context.Player.SteamUserId] = derp;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Cancelled contract");
                    sb.AppendLine("Mine " + cancel.SubType + " Ore " + String.Format("{0:n0}", cancel.minedAmount) + " / " + String.Format("{0:n0}", cancel.amountToMineOrDeliver));

                    sb.AppendLine("Reputation lowered by " + cancel.reputation * 2);
                    sb.AppendLine();
                    sb.AppendLine("Remaining Contracts");
                    sb.AppendLine();
                    foreach (Contract c in data.getMiningContracts().Values)
                    {

                        if (c.minedAmount >= c.amountToMineOrDeliver)
                        {
                            sb.AppendLine("Deliver " + c.SubType + " Ore " + String.Format("{0:n0}", c.amountToMineOrDeliver));
                            sb.AppendLine("Reward : " + String.Format("{0:n0}", c.contractPrice) + " SC. and " + c.reputation + " reputation gain.");
                        }
                        else
                        {
                            sb.AppendLine("Mine " + c.SubType + " Ore " + String.Format("{0:n0}", c.minedAmount) + " / " + String.Format("{0:n0}", c.amountToMineOrDeliver));
                            sb.AppendLine("Reward : " + String.Format("{0:n0}", c.contractPrice) + " SC. and " + c.reputation + " reputation gain.");

                        }
                        sb.AppendLine("");
                    }
                    DialogMessage m = new DialogMessage("Contract", "Cancel", sb.ToString());
                    ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                    cancel.status = ContractStatus.Failed;
                    File.Delete(CrunchEconCore.path + "//PlayerData//Mining//InProgress//" + cancel.ContractId + ".xml");
                    CrunchEconCore.ContractSave.Remove(cancel.ContractId);
                    CrunchEconCore.ContractSave.Add(cancel.ContractId, cancel);
                    CrunchEconCore.utils.WriteToJsonFile<PlayerData>(CrunchEconCore.path + "//PlayerData//Data//" + data.steamId + ".json", data);
                }else
                {
                    Context.Respond("Cannot find that contract.");
                }
            }
        }
        [Command("info", "view current contracts")]
        [Permission(MyPromoteLevel.None)]
        public void DoContractDetails()
        {
            if (CrunchEconCore.playerData.TryGetValue(Context.Player.SteamUserId, out PlayerData data))
            {
                int num = 0;
                ids.TryGetValue(Context.Player.SteamUserId, out Dictionary<int, Guid> derp);
                if (derp == null)
                {
                  //  Context.Respond("ids didnt contain");
                    derp = new Dictionary<int, Guid>();
                    num = 0;
                }
                else
                {
                    num = playerMax[Context.Player.SteamUserId];
                }
                Dictionary<Guid, int> temp = new Dictionary<Guid, int>();

                foreach (KeyValuePair<int, Guid> pair in derp)
                {
                    if (!temp.ContainsKey(pair.Value))
                    {
                        temp.Add(pair.Value, pair.Key);
                    }
                }
                List<IMyGps> playerList = new List<IMyGps>();
                MySession.Static.Gpss.GetGpsList(Context.Player.IdentityId, playerList);
                foreach (IMyGps gps in playerList)
                {
                    if (gps.Description.Contains("Contract Delivery Location."))
                    {
                        MyAPIGateway.Session?.GPS.RemoveGps(Context.Player.Identity.IdentityId, gps);
                    }
                }

                StringBuilder contractDetails = new StringBuilder();
                contractDetails.AppendLine("Current Mining Reputation " + data.MiningReputation);
                contractDetails.AppendLine("");
                contractDetails.AppendLine("Current Hauling Reputation " + data.HaulingReputation);
                contractDetails.AppendLine("");
                foreach (Contract c in data.getMiningContracts().Values)
                {
               
                    if (c.minedAmount >= c.amountToMineOrDeliver)
                    {
                        c.DoPlayerGps(Context.Player.Identity.IdentityId);
                        contractDetails.AppendLine("Deliver " + c.SubType + " Ore " + String.Format("{0:n0}", c.amountToMineOrDeliver));
                        contractDetails.AppendLine("Reward : " + String.Format("{0:n0}", c.contractPrice) + " SC. and " + c.reputation + " reputation gain.");
                        
                    }
                    else
                    {
                        contractDetails.AppendLine("Mine " + c.SubType + " Ore " + String.Format("{0:n0}", c.minedAmount) + " / " + String.Format("{0:n0}", c.amountToMineOrDeliver));
                        contractDetails.AppendLine("Reward : " + String.Format("{0:n0}", c.contractPrice) + " SC. and " + c.reputation + " reputation gain.");
                    }
                    if (derp.ContainsValue(c.ContractId))
                    {
                        contractDetails.AppendLine("To quit use !contract quit " + temp[c.ContractId]);
                        //Context.Respond("1");
                    }
                    else
                    {
                        //  Context.Respond("2");
                        num++;
                        derp.Add(num, c.ContractId);
                        contractDetails.AppendLine("To quit use !contract quit " + num);
                     
                    }
                    contractDetails.AppendLine("");
                }
                ids.Remove(Context.Player.SteamUserId);
                ids.Add(Context.Player.SteamUserId, derp);
                playerMax.Remove(Context.Player.SteamUserId);
                playerMax.Add(Context.Player.SteamUserId, num);
                DialogMessage m2 = new DialogMessage("Contract Details", "Instructions", contractDetails.ToString());
                ModCommunication.SendMessageTo(m2, Context.Player.SteamUserId);
            }
            else
            {
                Context.Respond("You dont currently have any contracts.");
            }
        }
    }
}
