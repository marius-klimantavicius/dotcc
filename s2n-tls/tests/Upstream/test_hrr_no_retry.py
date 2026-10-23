"""Separate controls for the nine original same-curve HRR-inapplicable cases."""
import copy

import pytest

from common import Curves, Protocols, ProviderOptions, data_bytes
from configuration import MINIMAL_TEST_CERTS, TLS13_CIPHERS, available_ports
from fixtures import managed_process  # noqa: F401
from providers import OpenSSL, Provider, S2N
from utils import get_parameter_name, to_bytes


@pytest.mark.parametrize('cipher', TLS13_CIPHERS, ids=get_parameter_name)
@pytest.mark.parametrize('certificate', [cert for cert in MINIMAL_TEST_CERTS
    if cert.compatible_with_curve(Curves.X25519)], ids=get_parameter_name)
def test_default_curve_does_not_retry(managed_process, cipher, certificate):
    port = next(available_ports)
    payload = data_bytes(64)
    client_options = ProviderOptions(mode=Provider.ClientMode, port=port, cipher=cipher,
        data_to_send=payload, insecure=True, protocol=Protocols.TLS13)
    server_options = copy.copy(client_options)
    server_options.data_to_send = None
    server_options.mode = Provider.ServerMode
    server_options.key = certificate.key
    server_options.cert = certificate.cert
    server_options.extra_flags = None
    server_options.curve = Curves.X25519
    server_options.verbose = True
    server = managed_process(OpenSSL, server_options, timeout=5)
    client = managed_process(S2N, client_options, timeout=5)
    clients, servers = list(client.get_results()), list(server.get_results())
    assert len(clients) == len(servers) == 1
    client_result, server_result = clients[0], servers[0]
    client_result.assert_success()
    server_result.assert_success()
    assert b'Actual protocol version: 34' in client_result.stdout
    assert to_bytes('Cipher negotiated: ' + cipher.name) in client_result.stdout
    assert b'Curve: x25519' in client_result.stdout
    assert b'HELLO_RETRY_REQUEST' not in client_result.stdout
    assert b'cf 21 ad 74 e5' not in server_result.stdout
    assert b'9a 61 11 be 1d' not in server_result.stdout
    assert b'Shared Elliptic groups: X25519' in server_result.stdout
    assert payload in server_result.stdout
    import pytest_adapter
    pytest_adapter.record_no_hrr_control({
        'original_nodeid': 'test_hello_retry_requests.py::test_hrr_with_s2n_as_client['
            + certificate.name + '-TLS1.3-X25519-S2N-OpenSSL-' + cipher.name + ']',
        'certificate': certificate.name, 'cipher': cipher.name, 'curve': 'X25519',
        'payload_hex': payload.hex(), 'client_stdout_hex': client_result.stdout.hex(),
        'server_stdout_hex': server_result.stdout.hex(),
        'client_exit_code': client_result.exit_code, 'server_exit_code': server_result.exit_code})
