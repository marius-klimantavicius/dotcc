"""Bridge EDG's concrete GNU atomic builtin ABI to shared AtomicOps.

The size-leading generic ABI and scalar suffixes follow dot_gen_atomics.h,
which in turn follows c_gen_be.c. Synchronization algorithms remain exclusively
in backend/runtime/AtomicOps.cs, including unaligned and wide-value fallbacks.
"""
import re


def expression(method):
    name = method['symbol']
    if not name.startswith(('__atomic_', '__sync_')):
        return None
    result = method['result']
    parameters = [kind for kind, _ in method['arguments']]
    arguments = [value for _, value in method['arguments']]
    boolean = result if result in ('byte', 'sbyte') else 'byte'
    prefix = 'global::Edg.Runtime.AtomicOps.'

    def call(target, expected_result, expected_parameters, values=None, returns_boolean=False):
        if method['variadic'] or result != expected_result or parameters != expected_parameters:
            raise ValueError('Atomic builtin ABI does not match EDG lowering: ' + name)
        code = prefix + target + '(' + ', '.join(arguments if values is None else values) + ')'
        return '(' + result + ')(' + code + ' ? 1 : 0)' if returns_boolean else code

    if name == '__sync_synchronize':
        return call('ThreadFence', 'void', [], ['5'])
    if name in ('__atomic_thread_fence', '__atomic_signal_fence'):
        return call('ThreadFence' if name == '__atomic_thread_fence' else 'SignalFence', 'void', ['int'])
    if name in ('__atomic_is_lock_free', '__atomic_always_lock_free'):
        return call('IsLockFree', boolean, ['ulong', 'void*'], ['(nuint)' + arguments[0], arguments[1]], True)
    if name in ('__atomic_test_and_set', '__atomic_clear'):
        clear = name == '__atomic_clear'
        return call('Clear' if clear else 'TestAndSet', 'void' if clear else boolean, ['void*', 'int'],
                    ['ref *(byte*)' + arguments[0], arguments[1]], not clear)
    if name in ('__atomic_load', '__atomic_store'):
        return call('LoadBytes' if name == '__atomic_load' else 'StoreBytes', 'void', ['ulong', 'void*', 'void*', 'int'])
    if name == '__atomic_exchange':
        return call('ExchangeBytes', 'void', ['ulong', 'void*', 'void*', 'void*', 'int'])
    if name == '__atomic_compare_exchange':
        return call('CompareExchangeBytes', boolean, ['ulong', 'void*', 'void*', 'void*', boolean, 'int', 'int'],
                    [*arguments[:4], arguments[4] + ' != 0', *arguments[5:]], True)
    match = re.fullmatch(r'__(atomic|sync)_(\w+)_(1|2|4|8|16)', name)
    if not match:
        return None
    family, operation, width = match.groups()
    kind = {'1': 'byte', '2': 'ushort', '4': 'uint', '8': 'ulong', '16': 'global::System.UInt128'}[width]
    address = 'ref *(' + kind + '*)' + arguments[0]
    if family == 'sync':
        if operation in ('val_compare_and_swap', 'bool_compare_and_swap'):
            returns_boolean = operation.startswith('bool_')
            return call('CompareExchangeMatches' if returns_boolean else 'CompareExchangeValue', boolean if returns_boolean else kind,
                        ['void*', kind, kind], [address, *arguments[1:]], returns_boolean)
        if operation == 'lock_test_and_set':
            return call('Exchange', kind, ['void*', kind], [address, arguments[1], '5'])
        if operation == 'lock_release':
            return call('Store', 'void', ['void*'], [address, '(' + kind + ')0', '5'])
        operations = {part.replace('_', '_and_'): ''.join(word.title() for word in part.split('_'))
                      for op in ('add', 'sub', 'and', 'or', 'xor', 'nand') for part in ('fetch_' + op, op + '_fetch')}
        if operation in operations:
            return call(operations[operation], kind, ['void*', kind], [address, arguments[1], '5'])
        return None
    if operation == 'load':
        return call('Load', kind, ['void*', 'int'], [address, arguments[1]])
    if operation in ('store', 'exchange'):
        return call(operation.title(), 'void' if operation == 'store' else kind, ['void*', kind, 'int'], [address, *arguments[1:]])
    if operation == 'compare_exchange':
        return call('CompareExchange', boolean, ['void*', 'void*', kind, boolean, 'int', 'int'],
                    [address, 'ref *(' + kind + '*)' + arguments[1], arguments[2], arguments[3] + ' != 0', *arguments[4:]], True)
    operations = {part: ''.join(word.title() for word in part.split('_'))
                  for op in ('add', 'sub', 'and', 'or', 'xor', 'nand') for part in ('fetch_' + op, op + '_fetch')}
    if operation in operations:
        return call(operations[operation], kind, ['void*', kind, 'int'], [address, *arguments[1:]])
    return None
