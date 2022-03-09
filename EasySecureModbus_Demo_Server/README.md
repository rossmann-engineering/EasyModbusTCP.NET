# EasyModbusSecure - Server Example

The Server performs the same functionality as in the original Modbus version. There are no no changes to the Modbus application protocol as a consequence of it being encapsulated by the secure transport.
The only changes concern the requirements of the Modbus/TCP Security Protocol Specification.

The use of TLS provides confidentialllity to the transported data, data integrity, anti-replay protection, endpoint authentication via certificates, and authorization via information embedded in the certificate 
such as user and device roles.

Similarly to HTTPS, the server must provide to each client a x.509v3 certificate as part of the TLS Handshake.

```
ModbusSecureServerAuthZ modbusServer = new ModbusSecureServerAuthZ("..\\..\\certs2\\server.pfx", certPass, true, roles);
```

Such a certificate must be issued by signed by a Trusted Third Part (TTP) and validated during the TLS handshake. The client should validate the provided server certificate path to a trusted root certificate.

If the server does not require authenticate of the client, the communication can be initiated by using the **false** option. Hovever the standard specifies the is **REQUIRED** for both end devices to provide 
mutual authentication when executing the TLS Handshake to create the TLS session (R-06). Therefore it is highly recommended that the mutual authentication option to be activated all the time.

The authorization functionality is implemented with the use of RoleIDs. Each client certificate must provide one of these RoleOIDs in there extension section. That role should match the database of the server
in order for the client to perform specific actions Roles-to-Rights Rules Database. The functionality of extracting the ASN1:UTF8String encoded RoleOID from the client certificate is performed via the use of 
the BouncyCastle library. An example of a Roles-to-Rights Rules Database can be as follows

```
List<ValueTuple<string, List<byte>>> roles = new List<ValueTuple<string, List<byte>>>
{
    ValueTuple.Create("Operator", new List<byte> { (byte)0x01, (byte)0x0F, (byte)0x06} ),
    ValueTuple.Create("Engineer", new List<byte> { (byte)0x01 })
};
```

Therefore the Operator can perform "Read Coils, Write Single Holding Register, Write Multiple Holding Registers",  while the Engineer can only "Read Coils". If the provided RoleOID is not matched to the 
Modbus Function Codes an exception is raised and an "exception code 01 – Illegal Function Code" is returned to the client.

## Regarding x.509v3 certificates

A common method to create x.509v3 certificates is by utilizing the OpenSSL library. These certificates can then be converted into other formats that suit the operating system and the libraries that it uses.

The same process described in [Client Example README](../EasySecureModbus_Demo/README.md), is used to create a RootCA. Below the similar process for the Server certificates is presented.

### Server certificate using the Root CA

The Server certificate creation can once again be summarized in one bash script to speed up the setup:

```

#!/bin/bash

set -e

openssl genrsa -out server.key 4096

openssl req -sha256 -new -key server.key -out server.csr -config openssl_server.cnf

openssl x509 -req -days 1000 -in server.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out server.crt -extensions v3_req -extfile openssl_server.cnf

openssl x509 -in server.crt -text -noout

openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt

```

The required **openssl_server.cnf** should include the RoleOID reference and can be structured as follows:

```
#
# openssl_server.cnf
#

[ req ]
prompt = no
distinguished_name = server_distinguished_name

[ server_distinguished_name ]
# IP address or Domain of the machine
commonName = 127.0.0.1
stateOrProvinceName = NY
countryName = US
emailAddress = you@email.com
organizationName = Here
organizationalUnitName = Here here

[ v3_req ]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[ alt_names ]
DNS.0 = localhost

```