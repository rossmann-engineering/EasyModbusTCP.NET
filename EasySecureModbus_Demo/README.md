# EasyModbusSecure - Client Example

The Client performs the same functionality as in the original Modbus version. There are no no changes to the Modbus application protocol as a consequence of it being encapsulated by the secure transport.

The use of TLS provides confidential transport of the data, data integrity, anti-replay protection, endpoint authentication via certificates, and authorization via information embedded in the certificate 
such as user and device roles.

The additional functionality that is included concerns only the TLS operations as defined in the Modbus/TCP Security Protocol Specification. For example the client must provide a x.509v3 certificate as part
of the TLS Handshake.

```
ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802, "..\\..\\certs2\\client.pfx", args[0], true);
```

Such a certificate must be issued by signed by a Trusted Third Part (TTP) and validated during the TLS handshake in order for the server to verify it.

If the server does not require authenticate of the client, the communication can be initiated as before.

```
ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802);
```

The authorization functionality is implemented with the use of RoleIDs. Each certificate must provide one of these RoleOIDs in there extension section. After the session initiation and when the client and the
server communicate the provided RoleOID must be check against the actions that are allowed to be performed.  The RoleOID is defined in the Modbus.org PEM as OID 1.3.6.1.4.1.50316.802.1. The RoleOID can be user
or device specific.

## Regarding x.509v3 certificates

A common method to create x.509v3 certificates is by utilizing the OpenSSL library. 