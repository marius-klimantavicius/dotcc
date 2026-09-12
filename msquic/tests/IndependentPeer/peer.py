#!/usr/bin/env python3
"""Separate, pinned aioquic reference peer. Never a product backend."""
import argparse
import asyncio
import json
from pathlib import Path
import ssl

from aioquic.asyncio import QuicConnectionProtocol, connect, serve
from aioquic.quic.configuration import QuicConfiguration
from aioquic.quic.events import HandshakeCompleted, StreamDataReceived, ConnectionTerminated, StreamReset
from aioquic.tls import CipherSuite, Group

SIZE = 65537
ALPN = 'dotcc-probe'


def payload(sender):
    return bytes((index * 31 + 17 + sender * 29) & 255 for index in range(SIZE))


class Peer(QuicConnectionProtocol):
    def __init__(self, quic, *, args, done, receipt, **kwargs):
        super().__init__(quic, **kwargs)
        self.args, self.done, self.receipt = args, done, receipt
        self.server = args.role == 'server'
        self.received = bytearray()
        self.finished = False
        self.stream_id = None
        self.rotation_task = None
        self.rotation_finished = False
        self.response_sent = False
        self.received_packet = asyncio.Event()
        # This release exposes ciphers in configuration but groups only on TLS.
        # Apply the group before any handshake messages; preserve upstream code.
        # These private seams are audited against the immutable source pin.
        initialize = quic._initialize

        def initialize_profile(peer_cid):
            initialize(peer_cid)
            quic.tls._supported_groups = [Group.SECP256R1]

        quic._initialize = initialize_profile

    def datagram_received(self, data, addr):
        super().datagram_received(data, addr)
        # NEW_CONNECTION_ID has no public peer-CID-available event in this pin.
        # Wake the observer after upstream has handled the complete UDP packet.
        self.received_packet.set()

    def begin_rotation(self):
        if self.rotation_task is None:
            self.rotation_task = asyncio.create_task(self.rotate_and_continue())

    async def rotate_and_continue(self):
        try:
            async with asyncio.timeout(8):
                # The pinned method is a no-op without a spare peer CID. Read
                # upstream state only; all issuance/retirement/packets remain
                # owned by the unmodified aioquic implementation.
                while True:
                    self.received_packet.clear()
                    if self._quic._handshake_confirmed and self._quic._peer_cid_available:
                        break
                    await self.received_packet.wait()
                await self.ping()
                before = self._quic._peer_cid
                self.change_connection_id()  # Public protocol API also transmits.
                after = self._quic._peer_cid
                assert after.cid != before.cid and after.sequence_number > before.sequence_number, 'CID rotation was a no-op'
                self.receipt['cid_rotation'] = dict(
                    calls=1, api='QuicConnectionProtocol.change_connection_id',
                    before_sequence=before.sequence_number, after_sequence=after.sequence_number,
                    before_cid=before.cid.hex(), after_cid=after.cid.hex(),
                    sent_bytes_before=self.receipt['sent_bytes'], received_bytes_before=len(self.received),
                    post_rotation_ping_acknowledged=False)
                # An actual acknowledged packet sent with the new CID proves
                # acceptance before the rest of the payload / FIN is submitted.
                await self.ping()
                self.receipt['cid_rotation']['post_rotation_ping_acknowledged'] = True
                self.rotation_finished = True
                if self.server:
                    if self.finished:
                        self.send_server_response()
                else:
                    self._quic.send_stream_data(self.stream_id, payload(0)[SIZE // 2:], end_stream=True)
                    self.receipt['sent_bytes'] = SIZE
                    self.transmit()
        except asyncio.CancelledError:
            raise
        except Exception as error:
            self.fail(error)

    def send_server_response(self):
        assert self.finished and not self.response_sent
        self.response_sent = True
        self._quic.send_stream_data(self.stream_id, payload(1), end_stream=True)
        self.receipt['sent_bytes'] = SIZE
        self.transmit()

    async def drain_background(self):
        if self.rotation_task is not None:
            if not self.rotation_task.done():
                self.rotation_task.cancel()
            await asyncio.gather(self.rotation_task, return_exceptions=True)

    def fail(self, error):
        self.receipt['error'] = str(error)
        if not self.done.done():
            self.done.set_exception(RuntimeError(str(error)))
        self.close(error_code=1, reason_phrase='reference validation failed')

    def quic_event_received(self, event):
        try:
            if isinstance(event, HandshakeCompleted):
                context = self._quic.tls
                cipher = int(context.key_schedule.cipher_suite)
                expected = 0x1301 if self.args.cipher == '128' else 0x1302
                # Both roles must have used P256, not merely advertised it.
                curves = [key.curve.name for key in context._ec_private_keys]
                assert curves == ['secp256r1'], curves
                assert context._x25519_private_key is None and context._x448_private_key is None
                assert cipher == expected and event.alpn_protocol == self.args.alpn
                assert not event.early_data_accepted and not event.session_resumed
                assert self._quic._version == 1
                self.receipt.update(handshake=True, cipher=cipher, group=23, alpn=event.alpn_protocol,
                                    quic_version=1, early_data=False, resumed=False)
                if not self.server:
                    self.stream_id = self._quic.get_next_available_stream_id()
                    length = SIZE // 2 if self.args.rotate_cid else SIZE
                    self._quic.send_stream_data(self.stream_id, payload(0)[:length], end_stream=not self.args.rotate_cid)
                    self.receipt['sent_bytes'] = length
                    self.transmit()
                    if self.args.rotate_cid:
                        self.begin_rotation()
            elif isinstance(event, StreamDataReceived):
                assert self.receipt.get('handshake'), 'application data before verified handshake'
                if self.stream_id is None:
                    assert self.server and event.stream_id == 0
                    self.stream_id = event.stream_id
                assert event.stream_id == self.stream_id and not self.finished
                self.received.extend(event.data)
                assert len(self.received) <= SIZE
                self.receipt['received_bytes'] = len(self.received)
                if self.server and self.args.rotate_cid and event.data:
                    self.begin_rotation()
                if event.end_stream:
                    assert self.received == payload(int(not self.server)), 'payload mismatch'
                    self.finished = True
                    self.receipt['verified_fin'] = True
                    if self.server:
                        if not self.args.rotate_cid or self.rotation_finished:
                            self.send_server_response()
                    elif not self.done.done():
                        self.done.set_result(None)
            elif isinstance(event, StreamReset):
                raise RuntimeError('stream reset before complete exchange')
            elif isinstance(event, ConnectionTerminated):
                self.receipt['close_code'] = event.error_code
                self.receipt['close_reason'] = event.reason_phrase
                assert event.error_code == 0 and self.finished, event
                if self.server and not self.done.done():
                    self.done.set_result(None)
        except Exception as error:
            self.fail(error)


async def run(args, receipt):
    done = asyncio.get_running_loop().create_future()
    # connect() can fail before control reaches await done. Retrieve that
    # exception too; await still propagates it on the normal connected path.
    done.add_done_callback(lambda future: None if future.cancelled() else future.exception())
    cipher = CipherSuite.AES_128_GCM_SHA256 if args.cipher == '128' else CipherSuite.AES_256_GCM_SHA384
    configuration = QuicConfiguration(is_client=args.role == 'client', alpn_protocols=[args.alpn],
        cipher_suites=[cipher], supported_versions=[1], server_name=args.server_name,
        idle_timeout=10.0, verify_mode=ssl.CERT_REQUIRED if args.role == 'client' else None)
    if args.role == 'server':
        configuration.load_cert_chain(args.certificate, args.key)
    else:
        configuration.load_verify_locations(cafile=args.trust)
    protocols = []

    def factory(quic, **kwargs):
        protocol = Peer(quic, args=args, done=done, receipt=receipt, **kwargs)
        protocols.append(protocol)
        return protocol
    host = '127.0.0.1' if args.family == 'ipv4' else '::1'
    async with asyncio.timeout(20):
        if args.role == 'client':
            async with connect(host, args.port, configuration=configuration, create_protocol=factory) as protocol:
                try:
                    await done
                finally:
                    await protocol.drain_background()
        else:
            server = await serve(host, args.port, configuration=configuration, create_protocol=factory)
            try:
                port = server._transport.get_extra_info('sockname')[1]
                Path(args.ready).write_text(str(port) + '\n')
                await done
            finally:
                for protocol in protocols:
                    await protocol.drain_background()
                server.close()
    assert receipt.get('handshake') and receipt.get('verified_fin')
    assert receipt['received_bytes'] == SIZE and receipt['sent_bytes'] == SIZE
    if args.rotate_cid:
        assert receipt['cid_rotation']['post_rotation_ping_acknowledged']
        assert receipt['cid_rotation']['calls'] == 1


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('role', choices=['client', 'server'])
    parser.add_argument('--certificate', required=True)
    parser.add_argument('--key', required=True)
    parser.add_argument('--trust', required=True)
    parser.add_argument('--cipher', choices=['128', '256'], required=True)
    parser.add_argument('--family', choices=['ipv4', 'ipv6'], required=True)
    parser.add_argument('--port', type=int, default=0)
    parser.add_argument('--server-name', default='localhost')
    parser.add_argument('--alpn', default=ALPN)
    parser.add_argument('--rotate-cid', action='store_true', help='Rotate a real peer CID during the stream exchange')
    parser.add_argument('--ready')
    parser.add_argument('--receipt', required=True)
    args = parser.parse_args()
    receipt = dict(passed=False, role=args.role, family=args.family, received_bytes=0, sent_bytes=0,
                   certificate_validation=args.role == 'client', product_dependency=False,
                   rotate_cid=args.rotate_cid)
    try:
        asyncio.run(run(args, receipt))
        receipt['passed'] = True
    except Exception as error:
        receipt.setdefault('error', str(error) or type(error).__name__)
    finally:
        Path(args.receipt).write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(receipt))
    raise SystemExit(0 if receipt['passed'] else 1)
