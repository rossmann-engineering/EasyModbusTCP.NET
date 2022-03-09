# EasyModbusSecure - Client Example

The Client performs the same functionality as in the original Modbus version. There are no changes to the Modbus application protocol as a consequence of it being encapsulated by a secure transport.

The use of TLS provides confidential transport of the data, data integrity, anti-replay protection, endpoint authentication via certificates, and authorization via information embedded in the certificate 
such as user and device roles.

The additional functionality that is included concerns only the TLS operations as defined in the Modbus/TCP Security Protocol Specification. For example, the client must provide an x.509v3 certificate as part
of the TLS Handshake.

```
ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802, "..\\..\\certs2\\client.pfx", args[0], true);
```

Such a certificate must be issued by signed by a Trusted Third Party (TTP) and validated during the TLS handshake for the server to verify it.

If the server does not require to authenticate the client, the communication can be initiated as before.

```
ModbusSecureClient modbusClient = new ModbusSecureClient("127.0.0.1", 802);
```

The authorization functionality is implemented with the use of RoleIDs. Each certificate must provide one of these RoleOIDs in their extension section. After the session initiation and when the client and the
server communicate the provided RoleOID must be checked against the actions that are allowed to be performed.  The RoleOID is defined in the Modbus.org PEM as OID 1.3.6.1.4.1.50316.802.1. The RoleOID can be user
or device-specific.

## Regarding x.509v3 certificates

A common method to create x.509v3 certificates is by utilizing the OpenSSL library. These certificates can then be converted into other formats that suit the operating system and the libraries that it uses.

The process used to create the certificates for the .NET 4.5 version of the library and test its functionality, is described as follows. However, this is not an exhaustive tutorial on how to perform this operation.
The x.509v3 certificate creation process should be performed based on the needs of the organization and the devices in use.

### Root CA Creation

This first command in OpenSSL, generates a Private Key for the Root CA:

```
openssl genrsa -out ca.key 4096
```

Then you can generate the Root CA certificate using:

```
openssl req -new -x509 -days 1826 -key ca.key -out ca.crt
```

The .p12 file must generated and certified:

```
openssl pkcs12 -export -out ca.p12 -inkey ca.key -in ca.crt
```

Now you have to generate a Certificate Revocation List (CRL) to be used with the CA. First we perform the configuaration:

```
openssl ca -config ca.conf -gencrl -keyfile ca.key -cert ca.crt -out root.crl.pem
```

Then create the CRL:

```
openssl crl -inform PEM -in root.crl.pem -outform DER -out root.crl
```

### Client certificate using the Root CA

The Client certificate creation can be summarized in one bash script to speed up the setup:

```

#!/bin/bash

set -e

openssl genrsa -out client.key 4096

openssl req -sha256 -new -key client.key -out client.csr -config openssl_client.cnf

openssl x509 -req -days 1000 -in client.csr -CA ca.crt -CAkey ca.key -CAcreateserial -CAserial certserial -out client.crt -extensions v3_req -extfile client_ext.cnf

openssl x509 -in client.crt -text -noout

openssl pkcs12 -export -out client.pfx -inkey client.key -in client.crt

```

The required **openssl_client.cnf** should include the RoleOID reference and can be structured as follows:

```
#
# openssl_client.cnf
#

[ req ]
prompt = no
distinguished_name = server_distinguished_name
req_extensions = v3_req

[ server_distinguished_name ]
commonName = ClientCert
stateOrProvinceName = NY
countryName = US
emailAddress = my@email.com
organizationName = Here
organizationalUnitName = Here here

[ v3_req ]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names
1.3.6.1.4.1.50316.802.1 = ASN1:UTF8String:Operator

[ alt_names ]
DNS.0 = localhost

```

Acks: To @ZacharyHale for his assistance to the creation process of the certificates

## Notes regarding the Modbus/TCP Security Protocol Specification

