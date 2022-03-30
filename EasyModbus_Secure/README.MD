# Requirements Listing and Implementation Description

|Requirement| Description |
| ------------- |:-------------:|
| R-01: The TLS Protocol v1.2 as defined in [RFC5246] or newer MUST be used as the secure transport protocol to an mbaps Device.|Supported
| R-02: Secure communications to an mbaps Device MUST use Mutual client/server authentication as provided by the TLS Handshake Protocol|Supported, see Demo Client README and [![link](link)](https://github.com/georgemakrakis/EasyModbusTCP.NET/blob/master/EasyModbus_Secure/Specs_vs_Packet_Capture.PNG)
| R-03: x.509v3 Certificates as defined in [RFC5280] MUST be used as mbaps device credentials for Identity/Authentication by the TLS protocol.|Supported
| R-04: If the Authorization function is enforced it MUST use the role transferred via x.509v3 certificate extensions|Supported
| R-05: There MUST be no change to the mbap protocol as a consequence of it being encapsulated by the secure transport|Supported
| R-06: mbaps end devices MUST provide mutual authentication when executing the TLS Handshake Protocol to create the TLS session.|Supported
| R-07: The TlsServer MUST send the CertificateRequest extension as part of its ServerHello message.|Supported, see [![link](link)](https://github.com/georgemakrakis/EasyModbusTCP.NET/blob/master/EasyModbus_Secure/Specs_vs_Packet_Capture.PNG)
| R-08: The TlsClient MUST send a ClientCertificate message upon receiving a request containing the Client Certificate Request.|Supported, see [![link](link)](https://github.com/georgemakrakis/EasyModbusTCP.NET/blob/master/EasyModbus_Secure/Specs_vs_Packet_Capture.PNG)
| R-09: If the TlsServer does not send a CertificateRequest message, then the TlsClient MUST send a ‘fatal alert’ message to the TlsServer and terminate the connection.|Supported
| R-10: If the TlsClient does not send a ClientCertificate message, then the TlsServer MUST send a ‘fatal alert’ message to TlsClient and terminate the connection.|Supported
| R-11: Per RFC5246-7.2.2, the TLS connection MUST NOT be resumed after a ‘fatal alert’.|Supported
| R-12: Cipher suites used with TLS for mbaps MUST be listed at the IANA Registry found @ http://www.iana.org/assignments/tls-parameters/tls-parameters.xhtml .|Supported, see https://docs.microsoft.com/en-us/dotnet/api/system.net.security.tlsciphersuite?view=net-6.0&viewFallbackFrom=net-4.5 
| R-13: The cipher allowed for TLS with mbaps MUST accommodate the use of x.509v3 certificates.|Supported
| R-14: mbaps Devices MUST provide at minimum the following TLS v1.2 cipher suites:  <ul><li> TLS_RSA_WITH_AES_128_CBC_SHA256, {0x00, 0x3C}</li> <li>TLS_RSA_WITH_NULL_SHA256, {0x00, 0x3B} </li></ul>|Can be set, see https://docs.microsoft.com/en-us/windows-server/security/tls/manage-tls#configuring-tls-cipher-suite-order-by-using-group-policy
| R-15: The default cipher suite for both mbaps client and server Devices SHOULD be TLS_RSA_WITH_AES_128_CBC_SHA256, {0x00, 0x3C}|Can be set, see https://docs.microsoft.com/en-us/windows-server/security/tls/manage-tls#configuring-tls-cipher-suite-order-by-using-group-policy
| R-66: Client devices with bulk transport encryption and NULL bulk encryption SHOULD always place NULL bulk transport cipher suites last in cipher suite priority|Can be set, see https://docs.microsoft.com/en-us/windows-server/security/tls/manage-tls#configuring-tls-cipher-suite-order-by-using-group-policy
| R-16: A mbaps Server Device SHOULD provide the role-based client AuthZ as described in this section.|Supported
| R-17: If a mbaps Server Device provides role-based client AuthZ, it MUST comply with the requirements identified in this section.|Supported
| R-18: To provide mbaps role-based client authorization capability the following elements are REQUIRED: <ul><li>x.509v3 client domain certificate ‘Role’ extension </li><li>mbaps server AuthZ algorithm</li><li> mbaps server Roles-to-Rights Rules Database </li></ul>|Supported
| R-19: The mbaps client device MUST be provisioned with its x.509v3 domain certificate.|Supported, see Demo Client README
| R-20: The x.509v3 client domain certificate MUST include the Role extension.is section.|Supported, see Demo Client README
| R-21: The Role in the X.509v3 certificate MUST use the Modbus.org PEM OID 1.3.6.1.4.1.50316.802.1|Supported, see Demo Client README
| R-22: The Role in the x.509v3 certificate MUST use ASN1:UTF8String encoding|Supported, see Demo Client README
| R-65: There MUST only be one role defined per certificate. The entire string will be treated as one role.|Supported
| R-23: If no Role is specified in the X.509v3 certificate, the mbaps server MUST provide a NULL role to the AuthZ algorithm.|Supported, see Demo Server README
| R-24: The mbaps AuthZ Algorithm MUST be defined and provided by the device vendor.|Supported
| R-25: The Roles-to-Rights Rules Database design, both syntax and semantics, MUST be defined by the device vendor.|Supported
| R-26: The Roles-to-Rights Rules Database for a particular application MUST be configured according to the device vendor’s design, and provisioned in the mbaps Server by the end user|Supported, see Demo Server README
| R-27: The Roles-to-Rights Rules Database for a particular application MUST be configurable by the end user.|Supported, see Demo Server README
| R-28: The Roles-to-Rights Rules Database for a particular application MUST NOT have hardcoded default roles that are unchangeable|Supported
| R-29: The Role values used in the x.509v3 client domain certificates MUST be consistent with the device vendor’s design of the Roles-to-Rights Rules Database.|Supported
| R-30: The mbaps server MUST extract the client Role from the received x.509v3 client domain certificate.|Supported
| R-31: If the MBAP protocol handler for authorization rejects a request it MUST use the exception code 01 – Illegal function code|Supported
| R-32: mbaps devices MUST provide TLS v1.2 or better|Supported
| R-33: mbaps Devices MUST conform to the requirements of [RFC5246]|Supported
| R-34: mbaps devices MUST NOT negotiate down to TLS v1.1, TLS v1.0, or SSL V3.0.|Supported by default, see https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls
| R-35: mbaps devices MUST NOT negotiate the use SSL v2.0 and SSL v1.0 in conformance with [RFC6176]|Supported by default, see https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls
| R-36: mbaps Devices SHOULD provide a counter mode cipher suite. Counter mode cipher suites include <ul><li>TLS_RSA_WITH_AES_128_GCM_SHA256, {0x00, 0x9C} </li><li> TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, {0xC0, 0x2B} </li></ul> |Can be set, see https://docs.microsoft.com/en-us/windows-server/security/tls/manage-tls#configuring-tls-cipher-suite-order-by-using-group-policy
| R-37: mbaps Devices MUST NOT negotiate the cipher suite, TLS_NULL_WITH_NULL_NULL |Supported
| R-38: Any cipher suite used by mbaps Devices and negotiated in a TLS Handshake Protocol exchange MUST be listed at IANA’s TLS Cipher Suite Registry in the [TLS-PARAMS].|Supported
| R-39: mbaps Devices MUST provide TLS Client-Server key exchange based-on RSA technology as specified by the mandatory cipher suite and described in [RFC5246]|Supported
| R-40 mbaps Devices SHOULD provide TLS Client-Server key exchange based on ECC technology.|Supported
| R-61 mbaps Devices using ECC technology MUST support at least P-256 NIST curve|Supported, see https://docs.microsoft.com/en-us/windows/win32/secauthn/tls-elliptic-curves-in-windows-10-1607-and-later
| R-62 mbaps Devices using ECC technology MUST support at least the minimum cipher suite of TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256|Supported, see https://docs.microsoft.com/en-us/windows/win32/secauthn/tls-cipher-suites-in-windows-10-v1809
| R-63 mbaps Devices using ECC technology MUST specify the curves used in their Client Hello using the Supported Elliptic Curves extension in [RFC4492]|Supported
| R-64 mbaps Devices using ECC technology MUST specify the point format used in their Client Hello using the Supported Point Format extension in [RFC4492]|Supported
| R-41: mbaps Devices MUST support the TLS Client-Server Mutual Authentication Handshake.|Supported
| R-42: mbaps Device SHOULD support the TLS Resumed Session Handshake on Client and Server.|Supported, see https://docs.microsoft.com/en-us/windows-server/security/tls/tls-registry-settings
| R-43: mbaps Device MAY support the TLS Session Ticket resumption on Client and Server|Partially supported, see https://docs.microsoft.com/en-us/windows-server/security/tls/tls-registry-settings
| R-44: mbaps Servers MUST reject a TLS Handshake where the Client has not responded to a Client Certificate request with certificate.|Supported
| R-45: mbaps Devices SHOULD provide x.509v3 Certificates signed by a Certificate Authority.|Supported
| R-46: mbaps Devices MUST send the entire certificate chain down to the root CA when sending their certificate|Supported
| R-47: x.509v3 Certificates provided by mbaps Devices MUST conform to the requirements of [RFC5280].|Supported
| R-48: If an mbaps Device is to be used in a scenario where encryption is required, then a cipher suite with the required encryption indicator MUST be chosen from the list at IANA’s TLS Cipher Suite Registry in the [TLS-PARAMS].|Supported
| R-49: If an mbaps Device is to be used in a scenario where encryption is not required, then a cipher suite with a NULL bulk encryption indicator MUST be chosen from the list at IANA’s TLS Cipher Suite Registry in the [TLS-PARAMS]|Supported
| R-50: mbaps Devices MUST NOT use the HMAC-MD5 hash algorithm.|Supported
| R-51: mbaps Devices MUST NOT use the HMAC-SHA-1 hash algorithm.|Supported
| R-52: mbaps Devices MUST provide the HMAC-SHA-256 hash algorithm.|Supported
| R-53: mbaps Device MUST NOT use a NULL HMAC hash algorithm.|Supported
| R-54: mbaps Devices MUST NOT provide the HMAC-SHA-1 hash algorithm for use in the PRF function to calculate the key block as defined in [RFC5246] sections 5, 6.3 and 8.1.|Supported
| R-55: mbaps Devices MUST provide the HMAC-SHA-256 hash algorithm for use in the PRF function to calculate the key block as defined in [RFC5246] sections 5, 6.3 and 8.1.|Supports HMAC-SHA-512, see  https://devblogs.microsoft.com/dotnet/cryptographic-improvements-in-asp-net-4-5-pt-2/
| R-56: As early as possible in their development cycle, mbaps devices MUST determine that they comply with the import/export conformance policies of their respective countries for the cryptography they provide.|
| R-57: mbaps devices MUST provide the Maximum Fragment Length Negotiation Extension as defined in [RFC6066].|Propably supported on Windows, see https://github.com/dotnet/runtime/issues/44241
| R-58: mbaps devices MUST provide the ability to negotiate a Maximum Fragment Length of 29 (512) bytes as defined in [RFC6066].|Propably supported on Windows, see https://github.com/dotnet/runtime/issues/44241
| R-59: mbaps devices MUST set the TLS CompressionMethod field of the ClientHello message to the value of NULL.|Supported
| R-60: mbaps devices MUST provide the TLS Renegotiation Indication Extension defined in [RFC5746] to provide the secure renegotiation of TLS sessions.|Supported
