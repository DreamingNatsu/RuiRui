﻿using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;

namespace RuiRuiBot.Botplugins.PersonalTools {
    public class WakeOnLan : IModule {
        public void Install(ModuleManager manager){
            manager.CreateCommands(bot => {
                bot.MinPermissions((int)Roles.Triumvirate);
                bot.CreateCommand("wakeonlan").Do(m => {
                    WakeFunction("D05099368DB5");
                    return "Sending wake-on-lan packets in local network.";
                });
                bot.CreateCommand("wakeonlan").Parameter("MAC").Do(m =>{
                    var mac = m.GetArg("MAC").Replace("_", "").Replace("-", "");
                    WakeFunction(mac);
                    return $"Sending wake-on-lan packets to {mac} local network.";
                });
            });
        }

        private static void WakeFunction(string macAddress){
            if(macAddress.Length != 12 || !macAddress.All(char.IsLetterOrDigit))
                throw new ArgumentOutOfRangeException(nameof(macAddress),"the value isn't a mac address");

            using (var client = new WolClass()) {
            client.Connect(new
                IPAddress(0xffffffff), //255.255.255.255  i.e broadcast
                0x2fff); // port=12287 let's use this one 
            client.SetClientToBrodcastMode();
            //set sending bites
            var counter = 0;
            //buffer to be send
            var bytes = new byte[1024]; // more than enough :-)
            //first 6 bytes should be 0xFF
            for (var y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;
            //now repeate MAC 16 times
            for (var y = 0; y < 16; y++) {
                var i = 0;
                for (var z = 0; z < 6; z++) {
                    bytes[counter++] =
                        byte.Parse(macAddress.Substring(i, 2),
                            NumberStyles.HexNumber);
                    i += 2;
                }
            }
            //now send wake up packet
            client.Send(bytes, 1024);
            }
        }
        private class WolClass : UdpClient
        {
            //this is needed to send broadcast packet
            public void SetClientToBrodcastMode()
            {
                if (Active)
                    Client.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Broadcast, 0);
            }
        }
    }


}