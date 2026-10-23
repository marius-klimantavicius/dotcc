"""Bounded observation of unchanged upstream readiness markers across pipe reads."""
import ast
import hashlib
import inspect
import os
import textwrap

MAX_MARKER_BYTES = 4096
MAX_FRAGMENT_BYTES = 32768  # The original communicator's os.read limit.
MAX_PROOF_BYTES = 1024 * 1024
MAX_FRAGMENTS = 256
MAX_CONTROL_MARKER_BYTES = 1024 * 1024


def control_match(communicator, marker, kind, report):
    """Search the original output chunks without joining different channels."""
    if isinstance(marker, bytes):
        searches = [('raw-bytes', marker)]
        marker_size = len(marker)
    elif isinstance(marker, str):
        # The original send/close predicates search str(data), not decoded
        # text. In particular record_padding supplies repr(payload)[2:-1].
        # Preserve that comparison without interpreting plaintext escapes.
        searches = [('original-bytes-repr', marker)]
        marker_size = len(marker.encode('utf-8'))
        try:
            literal = ast.literal_eval(marker)
        except (SyntaxError, ValueError):
            literal = None
        # An explicit canonical bytes repr also identifies the whole payload
        # when surrounding process output shares its pipe read. Do not apply
        # this conversion to a stripped repr or an arbitrary text marker.
        if isinstance(literal, bytes) and repr(literal) == marker:
            searches.append(('explicit-bytes-repr', literal))
    else:
        raise RuntimeError('Original control marker must be bytes or text')
    if not marker_size or marker_size > MAX_CONTROL_MARKER_BYTES:
        raise RuntimeError('Original control marker exceeds the bounded transport adapter')
    for stream, chunks in communicator._fileobj2output.items():
        tail, size = [], 0
        required = (len(chunks[-1]) if chunks else 0) + marker_size - 1
        for fragment in reversed(chunks):
            if len(fragment) > MAX_FRAGMENT_BYTES:
                raise RuntimeError('Original pipe fragment exceeds os.read limit')
            tail.append(fragment)
            size += len(fragment)
            if size >= required:
                break
        accumulated = b''.join(reversed(tail))[-required:] if required else b''
        for domain, expected in searches:
            searched = repr(accumulated) if domain == 'original-bytes-repr' else accumulated
            offset = searched.find(expected)
            if offset < 0:
                continue
            observation = {'kind': kind, 'name': communicator.name,
                'nodeid': os.environ.get('PYTEST_CURRENT_TEST'), 'command': communicator.proc.args,
                'stream': communicator.get_fd_name(communicator.proc, stream),
                'matching_domain': domain,
                'marker_hex': expected.encode('utf-8').hex() if isinstance(expected, str) else expected.hex(),
                'fragment_lengths': [len(fragment) for fragment in reversed(tail)],
                'match_offset': offset, 'spanning_fragments': not any(
                    expected in (repr(fragment) if domain == 'original-bytes-repr' else fragment) for fragment in tail)}
            if domain == 'original-bytes-repr':
                observation.update(marker_text=expected,
                    matched_repr_text=searched[max(0, offset - 64):offset + len(expected) + 64],
                    searched_raw_tail_sha256=hashlib.sha256(accumulated).hexdigest())
            else:
                observation['matched_tail_hex'] = accumulated[max(0, offset - 64):offset + len(expected) + 64].hex()
            report['control_markers'].append(observation)
            return True
    return False


def install(report):
    from processes import _processCommunicator
    original = _processCommunicator._communicate
    source = textwrap.dedent(inspect.getsource(original))
    tree = ast.parse(source)
    target = ast.dump(ast.parse('self.wait_for_marker in data_str', mode='eval').body, include_attributes=False)
    read_size = int(os.environ.get('DOTCC_S2N_PIPE_READ_LIMIT', MAX_FRAGMENT_BYTES))
    if not 1 <= read_size <= MAX_FRAGMENT_BYTES:
        raise RuntimeError('Diagnostic pipe read limit must be within the original os.read bound')

    class ReplaceReadiness(ast.NodeTransformer):
        replacements = 0
        controls = {'kill': 0, 'close': 0, 'send': 0}
        reads = 0

        def visit_Call(self, node):
            expected = ast.parse('os.read(key.fd, 32768)', mode='eval').body
            if ast.dump(node, include_attributes=False) == ast.dump(expected, include_attributes=False):
                self.reads += 1
                node.args[1] = ast.Constant(read_size)
                return node
            return self.generic_visit(node)

        def visit_Compare(self, node):
            for kind, comparison in (('kill', 'kill_marker in data'), ('close', 'close_marker in data_str'), ('send', 'send_marker in data_str')):
                if ast.dump(node, include_attributes=False) == ast.dump(ast.parse(comparison, mode='eval').body, include_attributes=False):
                    self.controls[kind] += 1
                    return ast.copy_location(ast.Call(func=ast.Name(id='_dotcc_control_match', ctx=ast.Load()),
                        args=[ast.Name(id='self', ctx=ast.Load()), ast.Name(id=kind + '_marker', ctx=ast.Load()),
                              ast.Constant(kind)], keywords=[]), node)
            if ast.dump(node, include_attributes=False) != target: return self.generic_visit(node)
            self.replacements += 1
            return ast.copy_location(ast.Call(func=ast.Name(id='_dotcc_readiness_match', ctx=ast.Load()),
                args=[ast.Name(id='self', ctx=ast.Load()), ast.Attribute(value=ast.Name(id='key', ctx=ast.Load()), attr='fileobj', ctx=ast.Load()),
                      ast.Name(id='data', ctx=ast.Load())], keywords=[]), node)

    transform = ReplaceReadiness(); tree = transform.visit(tree)
    if transform.replacements != 1:
        raise RuntimeError('Expected exactly one original readiness comparison; refusing a changed upstream helper')
    if transform.controls != {'kill': 1, 'close': 1, 'send': 1} or transform.reads != 1:
        raise RuntimeError('Expected exactly one original kill/close/send comparison and pipe read; refusing changed upstream control flow')
    ast.fix_missing_locations(tree)
    report['readiness_adapter'] = {'original_method': original.__qualname__, 'original_file': inspect.getsourcefile(original),
        'original_method_sha256': hashlib.sha256(source.encode()).hexdigest(), 'replaced_comparisons': 1,
        'original_comparison': 'self.wait_for_marker in data_str', 'max_marker_bytes': MAX_MARKER_BYTES,
        'max_fragment_bytes': MAX_FRAGMENT_BYTES, 'max_accumulated_bytes': MAX_MARKER_BYTES - 1 + MAX_FRAGMENT_BYTES,
        'max_proof_bytes_per_marker': MAX_PROOF_BYTES, 'max_fragments_per_marker': MAX_FRAGMENTS}
    report['readiness_adapter'].update(original_pipe_read_limit=MAX_FRAGMENT_BYTES, actual_pipe_read_limit=read_size)
    report['readiness'] = []
    report['control_marker_adapter'] = {'original_method_sha256': hashlib.sha256(source.encode()).hexdigest(),
        'replaced_comparisons': transform.controls, 'max_marker_bytes': MAX_CONTROL_MARKER_BYTES,
        'max_search_tail_bytes': MAX_CONTROL_MARKER_BYTES + MAX_FRAGMENT_BYTES,
        'scope': 'Original close/kill/send markers across original same-channel chunks only; original assertions/timeouts unchanged',
        'text_marker_semantics': 'Original marker in repr(bytes); explicit canonical full bytes repr additionally identifies its payload. No arbitrary text/stripped-repr escape decoding.'}
    report['control_markers'] = []

    def match(communicator, stream, fragment):
        marker = communicator.wait_for_marker
        if not isinstance(marker, str): raise RuntimeError('Original readiness marker must be text')
        expected = marker.encode('utf-8')
        if not expected or len(expected) > MAX_MARKER_BYTES or len(fragment) > MAX_FRAGMENT_BYTES:
            raise RuntimeError('Readiness marker/read exceeds the explicit adapter bound')
        state = getattr(communicator, '_dotcc_readiness_observation', None)
        if state is None or state['marker'] != expected:
            observation = {'name': communicator.name, 'command': communicator.proc.args,
                           'nodeid': os.environ.get('PYTEST_CURRENT_TEST'), 'marker_hex': expected.hex(),
                           'fragments': [], 'matched': False, 'observed_bytes': 0}
            state = {'marker': expected, 'streams': {}, 'proof': observation}
            communicator._dotcc_readiness_observation = state
            report['readiness'].append(observation)
        proof = state['proof']
        if len(proof['fragments']) >= MAX_FRAGMENTS or proof['observed_bytes'] + len(fragment) > MAX_PROOF_BYTES:
            raise RuntimeError('Readiness observation exceeded its explicit proof bound')
        channel = communicator.get_fd_name(communicator.proc, stream)
        tail, observed = state['streams'].get(channel, (b'', 0))
        accumulated = tail + fragment
        offset = accumulated.find(expected)
        proof['fragments'].append({'stream': channel, 'offset': observed, 'fragment_hex': fragment.hex(),
            'prefix_hex': tail.hex(), 'accumulated_bytes': len(accumulated), 'matched': offset >= 0,
            'match_stream_offset': observed - len(tail) + offset if offset >= 0 else None})
        proof['observed_bytes'] += len(fragment)
        state['streams'][channel] = (accumulated[-(len(expected) - 1):] if len(expected) > 1 else b'', observed + len(fragment))
        if offset >= 0: proof['matched'] = True
        return offset >= 0

    namespace = dict(original.__globals__, _dotcc_readiness_match=match,
                     _dotcc_control_match=lambda communicator, marker, kind: control_match(communicator, marker, kind, report))
    ast.increment_lineno(tree, original.__code__.co_firstlineno - 1)
    exec(compile(tree, inspect.getsourcefile(original), 'exec'), namespace)
    _processCommunicator._communicate = namespace[original.__name__]
