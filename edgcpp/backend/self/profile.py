"""Explicit EDG self-host facts for its portable managed LP64 representation.

These describe the compiled EDG program and its original x86-64 output target,
not the operating system or CPU executing the translated compiler. Keep generic
portable libc/libc++ headers free of fabricated native platform macros.
"""


def portable_host_defines():
    return {
        'TARG_SUPPORTS_X86_64': '1',
        'HOST_POINTER_ALIGNMENT': '8',
        'HOST_IL_ENTRY_PREFIX_ALIGNMENT': '8',
        'HOST_HAS_FLOAT16_TYPE': '1',
        'HOST_COMPILER_SUPPORTS_BFLOAT16': '1',
        'MAX_MULTIBYTE_CHAR_LENGTH': '16',
    }


def portable_self_flags():
    return ['-D' + name + '=' + value for name, value in portable_host_defines().items()]
