# Requirements Listing and Implementation Description

|Requirement| Description |
| ------------- |:-------------:|
| R-01: The TLS Protocol v1.2 as defined in [RFC5246] or newer MUST be used as the secure transport protocol to an mbaps Device.| right foo     ||
| R-02: Secure communications to an mbaps Device MUST use Mutual client/server authentication as provided by the TLS Handshake Protocol| right bar     ||
| R-03: x.509v3 Certificates as defined in [RFC5280] MUST be used as mbaps device credentials for Identity/Authentication by the TLS protocol.| right baz     ||
| R-04: If the Authorization function is enforced it MUST use the role transferred via x.509v3 certificate extensions| right baz     ||
| R-05: There MUST be no change to the mbap protocol as a consequence of it being encapsulated by the secure transport| right baz     ||
| R-06: mbaps end devices MUST provide mutual authentication when executing the TLS Handshake Protocol to create the TLS session.| right baz     ||
| R-07: The TlsServer MUST send the CertificateRequest extension as part of its ServerHello message.| right baz     ||
| R-08: The TlsClient MUST send a ClientCertificate message upon receiving a request containing the Client Certificate Request.| right baz     ||
| R-09: If the TlsServer does not send a CertificateRequest message, then the TlsClient MUST send a ‘fatal alert’ message to the TlsServer and terminate the connection.| right baz     ||
| R-10: If the TlsClient does not send a ClientCertificate message, then the TlsServer MUST send a ‘fatal alert’ message to TlsClient and terminate the connection.| right baz     ||
| R-11: Per RFC5246-7.2.2, the TLS connection MUST NOT be resumed after a ‘fatal alert’.| right baz     ||
| R-12: Cipher suites used with TLS for mbaps MUST be listed at the IANA Registry found @ http://www.iana.org/assignments/tls-parameters/tls-parameters.xhtml .| right baz     ||
| R-13: The cipher allowed for TLS with mbaps MUST accommodate the use of x.509v3 certificates.| right baz     ||
| R-14: mbaps Devices MUST provide at minimum the following TLS v1.2 cipher suites:  <ul><li> TLS_RSA_WITH_AES_128_CBC_SHA256, {0x00, 0x3C}</li> <li>TLS_RSA_WITH_NULL_SHA256, {0x00, 0x3B} </li></ul>| right baz     ||
| R-15: The default cipher suite for both mbaps client and server Devices SHOULD be TLS_RSA_WITH_AES_128_CBC_SHA256, {0x00, 0x3C}| right baz     ||
| R-66: Client devices with bulk transport encryption and NULL bulk encryption SHOULD always place NULL bulk transport cipher suites last in cipher suite priority| right baz     ||
| R-16: A mbaps Server Device SHOULD provide the role-based client AuthZ as described in this section.| right baz     ||
| R-17: If a mbaps Server Device provides role-based client AuthZ, it MUST comply with the requirements identified in this section.| right baz     ||
| R-18: To provide mbaps role-based client authorization capability the following elements are REQUIRED: <ul><li>x.509v3 client domain certificate ‘Role’ extension </li><li>mbaps server AuthZ algorithm</li><li> mbaps server Roles-to-Rights Rules Database </li></ul>| right baz     ||
| R-19: The mbaps client device MUST be provisioned with its x.509v3 domain certificate.| right baz     ||
| R-20: The x.509v3 client domain certificate MUST include the Role extension.is section.| right baz     ||
| R-21: The Role in the X.509v3 certificate MUST use the Modbus.org PEM OID 1.3.6.1.4.1.50316.802.1| right baz     ||
| R-22: The Role in the x.509v3 certificate MUST use ASN1:UTF8String encoding| right baz     ||
| R-65: There MUST only be one role defined per certificate. The entire string will be treated as one role.| right baz     ||
| R-23: If no Role is specified in the X.509v3 certificate, the mbaps server MUST provide a NULL role to the AuthZ algorithm.| right baz     ||
| R-24: The mbaps AuthZ Algorithm MUST be defined and provided by the device vendor.| right baz     ||
| R-25: The Roles-to-Rights Rules Database design, both syntax and semantics, MUST be defined by the device vendor.| right baz     ||
| R-26: The Roles-to-Rights Rules Database for a particular application MUST be configured according to the device vendor’s design, and provisioned in the mbaps Server by the end user| right baz     ||
| R-27: The Roles-to-Rights Rules Database for a particular application MUST be configurable by the end user.| right baz     ||
| R-28: The Roles-to-Rights Rules Database for a particular application MUST NOT have hardcoded default roles that are unchangeable| right baz     ||
| R-29: The Role values used in the x.509v3 client domain certificates MUST be consistent with the device vendor’s design of the Roles-to-Rights Rules Database.| right baz     ||
| R-30: The mbaps server MUST extract the client Role from the received x.509v3 client domain certificate.| right baz     ||
| R-31: If the MBAP protocol handler for authorization rejects a request it MUST use the exception code 01 – Illegal function code| right baz     ||
| R-32: mbaps devices MUST provide TLS v1.2 or better| right baz     ||
| R-33: mbaps Devices MUST conform to the requirements of [RFC5246]| right baz     ||
| R-34: mbaps devices MUST NOT negotiate down to TLS v1.1, TLS v1.0, or SSL V3.0.| right baz     ||
| R-35: mbaps devices MUST NOT negotiate the use SSL v2.0 and SSL v1.0 in conformance with [RFC6176]| right baz     ||
| R-36: mbaps Devices SHOULD provide a counter mode cipher suite. Counter mode cipher suites include <ul><li>TLS_RSA_WITH_AES_128_GCM_SHA256, {0x00, 0x9C} </li><li> TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256, {0xC0, 0x2B} </li></ul>| right baz     ||
| R-37: mbaps Devices MUST NOT negotiate the cipher suite, TLS_NULL_WITH_NULL_NULL| right baz     ||
| R-38: Any cipher suite used by mbaps Devices and negotiated in a TLS Handshake Protocol exchange MUST be listed at IANA’s TLS Cipher Suite Registry in the [TLS-PARAMS].| right baz     ||
| R-39: mbaps Devices MUST provide TLS Client-Server key exchange based-on RSA technology as specified by the mandatory cipher suite and described in [RFC5246]| right baz     ||
| R-40 mbaps Devices SHOULD provide TLS Client-Server key exchange based on ECC technology.| right baz     ||
| R-61 mbaps Devices using ECC technology MUST support at least P-256 NIST curve| right baz     ||
| R-62 mbaps Devices using ECC technology MUST support at least the minimum cipher suite of TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256| right baz     ||
| R-63 mbaps Devices using ECC technology MUST specify the curves used in their Client Hello using the Supported Elliptic Curves extension in [RFC4492]| right baz     ||
| R-64 mbaps Devices using ECC technology MUST specify the point format used in their Client Hello using the Supported Point Format extension in [RFC4492]| right baz     ||
| R-41: mbaps Devices MUST support the TLS Client-Server Mutual Authentication Handshake.| right baz     ||
| R-42: mbaps Device SHOULD support the TLS Resumed Session Handshake on Client and Server.| right baz     ||
| R-43: mbaps Device MAY support the TLS Session Ticket resumption on Client and Server| right baz     ||
| R-44: mbaps Servers MUST reject a TLS Handshake where the Client has not responded to a Client Certificate request with certificate.| right baz     ||
| R-45: mbaps Devices SHOULD provide x.509v3 Certificates signed by a Certificate Authority.| right baz     ||
| R-46: mbaps Devices MUST send the entire certificate chain down to the root CA when sending their certificate| right baz     ||
| R-47: x.509v3 Certificates provided by mbaps Devices MUST conform to the requirements of [RFC5280].| right baz     ||
| R-48: If an mbaps Device is to be used in a scenario where encryption is required, then a cipher suite with the required encryption indicator MUST be chosen from the list at IANA’s TLS Cipher Suite Registry in the [TLS-PARAMS].| right baz     ||
| R-49: If an mbaps Device is to be used in a scenario where encryption is not required, then a cipher suite with a NULL bulk encryption indicator MUST be chosen from the list at IANA’s TLS Cipher Suite Registry in the [TLS-PARAMS]| right baz     ||
| R-50: mbaps Devices MUST NOT use the HMAC-MD5 hash algorithm.| right baz     ||
| R-51: mbaps Devices MUST NOT use the HMAC-SHA-1 hash algorithm.| right baz     ||
| R-52: mbaps Devices MUST provide the HMAC-SHA-256 hash algorithm.| right baz     ||
| R-53: mbaps Device MUST NOT use a NULL HMAC hash algorithm.| right baz     ||
| R-54: mbaps Devices MUST NOT provide the HMAC-SHA-1 hash algorithm for use in the PRF function to calculate the key block as defined in [RFC5246] sections 5, 6.3 and 8.1.| right baz     ||
| R-55: mbaps Devices MUST provide the HMAC-SHA-256 hash algorithm for use in the PRF function to calculate the key block as defined in [RFC5246] sections 5, 6.3 and 8.1.| right baz     ||
| R-56: As early as possible in their development cycle, mbaps devices MUST determine that they comply with the import/export conformance policies of their respective countries for the cryptography they provide.| right baz     ||
| R-57: mbaps devices MUST provide the Maximum Fragment Length Negotiation Extension as defined in [RFC6066].| right baz     ||
| R-58: mbaps devices MUST provide the ability to negotiate a Maximum Fragment Length of 29 (512) bytes as defined in [RFC6066].| right baz     ||
| R-59: mbaps devices MUST set the TLS CompressionMethod field of the ClientHello message to the value of NULL.| right baz     ||
| R-60: mbaps devices MUST provide the TLS Renegotiation Indication Extension defined in [RFC5746] to provide the secure renegotiation of TLS sessions.| right baz     ||
