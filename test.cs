using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

//connection
Console.Title = "EggCraft 06212022";
Console.WriteLine("Type in server address (no port): ");
String server = Console.ReadLine();
Console.WriteLine("Type in port (default is 25565): ");
Int32 port = Int32.Parse(Console.ReadLine());
Console.WriteLine("Connecting to: " + server + ":" + port);
TcpClient tcp = new TcpClient(server, port);
NetworkStream stream = tcp.GetStream();
BinaryReader reader = new BinaryReader(stream);
BinaryWriter writer = new BinaryWriter(stream);

int SEGMENT_BITS = 0x7F;
int CONTINUE_BIT = 0x80;
int ReadVarInt(NetworkStream stream)
{
    int value = 0;
    int length = 0;
    int currentByte;

    while (true)
    {
        currentByte = stream.ReadByte();
        value |= (currentByte & 0x7F) << (length++ * 7);
        if (length > 5) throw new IOException("VarInt too big");
        if ((currentByte & 0x80) != 0x80) break;
    }
    return value;
}
string ReadString(NetworkStream stream, int length)
{
    byte[] data = new byte[length];
    stream.Read(data);
    return Encoding.UTF8.GetString(data);
}
void writeVarInt(int value)
{
    while (true)
    {
        if ((value & ~SEGMENT_BITS) == 0)
        {
            stream.WriteByte(Convert.ToByte(value));
            return;
        }

        stream.WriteByte(Convert.ToByte((value & SEGMENT_BITS) | CONTINUE_BIT));

        value >>= 7;
    }
}
void writeVarLong(long value)
{
    while (true)
    {
        if ((value & ~((long)SEGMENT_BITS)) == 0)
        {
            stream.WriteByte(Convert.ToByte(value));
            return;
        }

        stream.WriteByte(Convert.ToByte((value & SEGMENT_BITS) | CONTINUE_BIT));

        value >>= 7;
    }
}

//Handshake
byte[] handshakeProtocol = BitConverter.GetBytes(754);
byte[] handshakeAddress = Encoding.ASCII.GetBytes(server);
byte[] handshakePort = BitConverter.GetBytes(port);
byte[] handshakeNextState = BitConverter.GetBytes(2);
int packetId = 0x00;
int packetLength = packetId.ToString().Length + handshakeAddress.Length + handshakeProtocol.Length + handshakePort.Length + handshakeNextState.Length;
writeVarInt(packetLength);
writeVarInt(packetId);
stream.Write(handshakeProtocol);
stream.Write(handshakeAddress);
stream.Write(handshakePort);
stream.Write(handshakeNextState);
stream.Flush();

//Login
byte[] loginName = Encoding.ASCII.GetBytes("BotTest");
byte[] loginHasSigData = BitConverter.GetBytes(false);
packetId = 0x00;
packetLength = packetId.ToString().Length + loginName.Length + loginHasSigData.Length;
writeVarInt(packetLength);
writeVarInt(packetId);
stream.Write(loginName);
stream.Write(loginHasSigData);
stream.Flush();

//Login success
packetLength = ReadVarInt(stream);
packetId = ReadVarInt(stream);
int uuidLength = ReadVarInt(stream);
string uuid = ReadString(stream, 16);
int nameLength = ReadVarInt(stream);
string name = ReadString(stream, 16);
Console.WriteLine(uuid);
Console.WriteLine(name);
Console.WriteLine(packetLength);
Console.WriteLine(packetId);
