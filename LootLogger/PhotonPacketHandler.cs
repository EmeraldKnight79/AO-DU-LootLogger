using ExitGames.Client.Photon;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LootLogger
{
    internal class PhotonPacketHandler
    {
        private PacketHandler _eventHandler;

        public PhotonPacketHandler(PacketHandler p)
        {
            this._eventHandler = p;
        }

        public void PacketHandler(Packet packet)
        {
            Protocol16 protocol16 = new Protocol16();
            UdpDatagram udp = packet.Ethernet.IpV4.Udp;
            if (udp.SourcePort != 5056 && udp.DestinationPort != 5056)
            {
                return;
            }
            BinaryReader p = new BinaryReader(udp.Payload.ToMemoryStream());
            IPAddress.NetworkToHostOrder((int)p.ReadUInt16());
            p.ReadByte();
            byte commandCount = p.ReadByte();
            IPAddress.NetworkToHostOrder(p.ReadInt32());
            IPAddress.NetworkToHostOrder(p.ReadInt32());
            int commandHeaderLength = 12;
            int signifierByteLength = 1;
            for (int commandIdx = 0; commandIdx < (int)commandCount; commandIdx++)
            {
                try
                {
                    byte commandType = p.ReadByte();
                    p.ReadByte();
                    p.ReadByte();
                    p.ReadByte();
                    int commandLength = IPAddress.NetworkToHostOrder(p.ReadInt32());
                    IPAddress.NetworkToHostOrder(p.ReadInt32());
                    switch (commandType)
                    {
                        case 4:
                            goto IL_1E7;
                        case 5:
                            goto IL_1CF;
                        case 6:
                            break;
                        case 7:
                            p.BaseStream.Position += 4L;
                            commandLength -= 4;
                            break;
                        default:
                            goto IL_1CF;
                    }
                    p.BaseStream.Position += (long)signifierByteLength;
                    byte messageType = p.ReadByte();
                    int operationLength = commandLength - commandHeaderLength - 2;
                    StreamBuffer payload = new StreamBuffer(p.ReadBytes(operationLength));
                    switch (messageType)
                    {
                        case 2:
                            {
                                OperationRequest requestData = protocol16.DeserializeOperationRequest(payload);
                                this._eventHandler.OnRequest(requestData.OperationCode, requestData.Parameters);
                                goto IL_1E7;
                            }
                        case 3:
                            {
                                OperationResponse responseData = protocol16.DeserializeOperationResponse(payload);
                                this._eventHandler.OnResponse(responseData.OperationCode, responseData.ReturnCode, responseData.Parameters);
                                goto IL_1E7;
                            }
                        case 4:
                            {
                                EventData eventData = protocol16.DeserializeEventData(payload);
                                this._eventHandler.OnEvent(eventData.Code, eventData.Parameters);
                                goto IL_1E7;
                            }
                        default:
                            p.BaseStream.Position += (long)operationLength;
                            goto IL_1E7;
                    }
                    IL_1CF:
                    p.BaseStream.Position += (long)(commandLength - commandHeaderLength);
                    IL_1E7:;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                }
            }
        }
    }
}
