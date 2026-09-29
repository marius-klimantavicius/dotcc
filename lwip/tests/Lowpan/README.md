# Finite independent receive vectors

`lowpan-vectors` runs independently encoded IPv6/UDP datagrams through actual
IEEE802.15.4 and RFC7668 inputs. Radio vectors cover raw IPv6 and IPHC with all
four short/extended source/destination combinations. BLE covers both dispatches,
public EUI64 conversion and invalid address lengths; a known CRC vector checks
802.15.4 CRC generation. Managed tests reject unknown dispatch, run two generated
owners, dispose the first and continue on the second, and require natural
pbuf/heap baselines.

The native control intentionally fails an assertion, then swaps only lowpan6.c
back to original source to reproduce valid short-source rejection. The repaired
fixture must deliver all ten inputs. No original algorithm is emulated by the
fixture; packet bytes/checksum are composed independently.

This suite is a subset: fragmentation/reassembly, contexts and every compressed
header form, output MTU behavior, ZEP transport/services and shared timer ownership
remain separate required work.
