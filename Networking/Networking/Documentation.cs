/*
{ Structure-diagram, Library-header }
The header for all packages (5 bytes).

 0               1               3
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|  Type |Channel|F|       SequenceNumber        |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|         PacketSize            |               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Type:           Message type                (4 bits)
The type of message being send/received.

Channel:        Sequence channel            (4 bits)
The channel that this message is being send through (zero for library messages)

F:              Fragment                    (1 bit)
Whether this message is a fragment of a larger message.

SequenceNumber: Sequence number             (15 bits)
The ID of this packet, this is to order messages.

PacketSize:     Packet size                 (16 bits)
The size of the data in bits (used to check for expansion or corruption)



{ Structure-diagram, Fragment-header }
This header is added if the fragment flag is true (4-16 bytes).

All values in this header are variable size meaning that the can be 8 to 32 bits long.
This is done by check if the value is greater or equal to 0x80.
Example:
24 ->       24 >= 128 = false ->    8 bits
128 ->      128 >= 128 = true ->    16 bits ->   128 >> 7 = 1 ->        1 >= 128 = false ->     16 bits
158 ->      158 >= 128 = true ->    16 bits ->   158 >> 7 = 1 ->        1 >= 128 = false ->     16 bits
32768 ->    32768 >= 128 = true ->  16 bits ->   32768 >> 7 = 256 ->    256 >= 128 = true ->    24 bits ->      256 >> 7 = 2 ->     2 >= 128 = false ->     24 bits

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                             Group                             |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           TotalBits                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                          FragmentSize                         |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                         FragmentNumber                        |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Group:          Fragment group      (8-32 bits)
The group that this fragment belongs to.

TotalBits:      Total bits                      (8-32 bits)
The total number of bits in this fragment group (used to check if all messages of this group are received).

FragmentSize:   Fragment size                   (8-32 bits)
The size in bytes of this chunk (used to check for expansion or corruption).

FragmentNumber: Fragment number                 (8-32 bits)
A number that represent the id of this fragment (ascending order).



+----------------------------------------------------+
|ALL OF THE FOLLOWING MESSAGES USE THE LIBRARY HEADER|
+----------------------------------------------------+


{ Structure-diagram, Ping }
This is an internal message used for round trip time calculation.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           PingNumber                          |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           TimeStamp                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

PingNumber:     Ping number                     (32 bits)
An Int32 that represent the id of the current ping.

TimeStamp:      Time stamp                      (32 bits)
A Single that represents the time of sending this message.

Additional information:
- Unreliable
- Expects Pong result



{ Structure-diagram, Pong }
This is an internal message used to respond to a ping message.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           PingNumber                          |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           TimeStamp                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

PingNumber:     Ping number                     (32 bits)
An Int32 that represent the id of the ping this pong is responding to.

TimeStamp:      Time stamp                      (32 bits)
A Single that represents the time of sending this message.

Additional information:
- Unreliable
- Send after receiving Ping message



{ Structure-diagram, Connect }
This is an internal message that is used by the client in order to conenct to the server.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                          ClientAppID                          |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                             HostID                            |
|                                                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           TimeStamp                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                            HailMsg                            |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

ClientAppID:    Client application indentifier  (>=24 bits)
A string that represent the application that is trying to connect to the server.

HostID:         Host computer indentifier       (64 bits)
A Int64 that represent the computer that is trying to connect to the server.

TimeStamp:      Time stamp                      (32 bits)
A Single that represents the time of sending this message.

HailMsg:        Hail message                    (Optional)
Additional user defined data that the user can use for connection security.

Additional information:
- Unreliable
- Expects ConnectionResonse or disconnect result
- Message is unhandled by serer after the time out period.



{ Structure-diagram, ConnectResponse }
This is an internal message that is used by the server as a response to the Connect message.
This indicates that the client is connected to the server.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                          ServerAppID                          |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                             HostID                            |
|                                                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                            HailMsg                            |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

ServerAppID:    Server application indentifier  (>= 24 bits)
A string that represent the server application that the client just connected to.

HostID:         Host computer indentifier       (64 bits)
A Int64 that represent the computer that is hosting the server application.

HailMsg:        Hail message                    (Optional)
Additional user defined data that the user can use for aditional information.

Additional information:
- Unreliable
- Possible send after receiving Connect message.


{ Structure-diagram, ConnectionEstablished }
This is an internal message that is used by the client as a response to the ConnectResponse message.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                           TimeStamp                           |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

TimeStamp:      Time stamp                      (32 bits)
A Single that represents the time of sending this message.

Additional information:
- UnReliable
- Send in order to check the connection.



{ Structure-diagram, Acknowledge }
This is an internal message that is used to indicate that a reliable message has received the remote host.

 0               1               3
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
| Type  |Channel|        SequenceNumber         |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Type:           Message type                (4 bits)
The type of message being received.

Channel:        Sequence channel            (4 bits)
The channel that this message is being send through

SequenceNumber: Sequence number             (16 bits)
The ID of the received packet.

Additional information:
- Unreliable
- Send in order to indicate that a data message has been received.



{ Structure-diagram, Disconnect }
This is an internal message that is used to indicate that a client has been disconnected from the server.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                             Reason                            |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Reason:         Reason                      (>= 16 bits)
The reason for the diconnect.

Additional information:
- Unreliable
- Can be send from server or client
- Possible send from server after receiving Connect message.



{ Structure-diagram, Discovery }
This is an internal message that is used to discover servers on a specified computer.
This message has no data.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                                                               |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Additional information:
- Unreliable
- Expects DiscoveryResponse result



{ Structure-diagram, DiscoveryResponse }
This is an internal message that is used as a response to the Discovery message.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                              Msg                              |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Msg:            Message                     (Optional)
A message that the user can use to send aditional information to the client.

Additional information:
- Unreliable
- Send after received Discovery message



{ Structure-diagram, MTUSet }
This is an internal message that is used to notify the remote host of the local mtu.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                             Value                             |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

Value:          New value                   (32 bits)
The remote mtu of the sender.

Additional information:
- Unreliable
- Expects MTUFinalized result
- Send after connection is established



{ Structure-diagram, MTUFinalized }
This is an internal message that is used to respond to a MTUSet message.

 0               1               3               4
 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7 0 1 2 3 4 5 6 7
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|S|                                                             |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

S:              Succeeded                   (1 bit)
Whether the value received was a valid value.

Additional information:
- Unreliable
- Send after received and processed MTUSet message



{ Structure-diagram, Connection }
This structure-diagram shows the handshake used to connect to the server.

+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|        Client         |       |        Server         |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+

+-+-+-+-+-+-+-+-+-+-+-+-+
|        Start          |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|       Discovery       |   =>  |     Timeout check     |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|    Check Message      |   <=  |   DiscoveryResponse   |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|        Connect        |   =>  |   Check Credentials   |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|         End           |   <=  |         Deny          |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|   Check Credentials   |   <=  |        Accept         |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
| ConnectionEstablished |   =>  |          End          |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+



{ Structure-diagram, Sending messages }
This structure-diagram shows the full process of sending a message and receiving it.

+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|        Client         |       |        Server         |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+

+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
|                  Establish connection                 |
+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|  User creates message |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|   Store in channel    |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|  Store in connection  |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|   Client heartbeat    |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
| Connection heartbeat  |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|   Channel heartbeat   |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|   Get library header  |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
|    Fragment message   |
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+
| Store sequencenumbers |                                       (Only when Reliable, Ordered or ReliableOrdered)
+-+-+-+-+-+-+-+-+-+-+-+-+
           ||
           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|     Send fragments    |   =>  |  Store all fragments  |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|      Receive acks     |   <=  |       Send acks       |       (Only when Reliable or Reliableordered)
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
           ||                              ||
           \/                              \/
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
|    Release sequence   |       |  Reconstruct message  |
+-+-+-+-+-+-+-+-+-+-+-+-+       +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                |   Drop/Order message  |       (Only when Ordered or ReliableOrdered)
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                |   Store in channel    |
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                |  Store in connection  |
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                |   Server heartbeat    |
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                | Connection heartbeat  |
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                |   Channel heartbeat   |
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                           ||
                                           \/
                                +-+-+-+-+-+-+-+-+-+-+-+-+
                                |    Release message    |
                                +-+-+-+-+-+-+-+-+-+-+-+-+
*/