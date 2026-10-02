#!/usr/bin/python3

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

#
#  This script reads in a number of files containing builtin declarations
#  (as created by the companion script extract-builtin-declarations) and
#  emits (on stdout), a set of initializations for tables that describe
#  the signatures and version information for all of the builtins.
#
#  Each file that is read must have a filename that encodes the compiler,
#  mode, version, and architecture from which the data was obtained:
#
#   builtins_${cc_string}_${version}_${arch}.txt
#
#  Within each file, there's an initial C-style comment that contains (only)
#  the builtin name, followed by the signature, e.g.:
#
#  /* __builtin_strrchr */ char* __builtin_strrchr(const char*, int);
#
#  A typical invocation is:
#
#   generate-builtin-table.py builtins_*.txt
#

import hashlib, re, sys

# Regular expression to match a vector attribute as used by Clang.
VECTOR_ATTR_RE = re.compile(R'__attribute__\(\(__vector_size__\(' +
                            R'(?P<size>\d+)\s*\*\s*sizeof\(' +
                            R'(?P<type>[^)]+)\)\)\)\)$')

# Regular expression to match a vector modifier as used by GCC.
VECTOR_MODIFIER_RE = re.compile(R'__vector\((?P<size>\d+)\)$')

# Regular expression for file names passed on the command line.
FILE_NAME_RE = re.compile(R'builtins_(?P<compiler>gx|Lx|mx)_(?P<version>\d+)' +
                          R'_(?P<arch>[amr])(?P<bits>32|64)\.txt')

# Mapping for compiler-specific types to corresponding
# configuration-independent types.
TYPE_MAPPING = {
    '__va_list' : '__builtin_va_list',
    '_Bool' : '__edg_bool_type__',
    'bool' : '__edg_bool_type__',
    'wchar_t' : '__edg_wchar_type__',
    '__size_t' : '__edg_size_type__',
    '__ptrdiff_t' : '__edg_ptrdiff_type__',
    '__int128' : '__int128_t',
    '<unnamed-unsigned:128>' : '__int128_t',
    # scalable vector types
    '__SVBool_t' : '__edg_scalable_vector_type__(__edg_bool_type__,1)',
    '__SVInt8_t' : '__edg_scalable_vector_type__(signed char,1)',
    '__SVUint8_t' : '__edg_scalable_vector_type__(unsigned char,1)',
    '__SVInt16_t' : '__edg_scalable_vector_type__(short,1)',
    '__SVUint16_t' : '__edg_scalable_vector_type__(unsigned short,1)',
    '__SVInt32_t' : '__edg_scalable_vector_type__(int,1)',
    '__SVUint32_t' : '__edg_scalable_vector_type__(unsigned int,1)',
    '__SVInt64_t' : '__edg_scalable_vector_type__(long,1)',
    '__SVUint64_t' : '__edg_scalable_vector_type__(unsigned long,1)',
    # Clang prior to 18.x used __SVBFloat16_t
    '__SVBFloat16_t' : '__edg_scalable_vector_type__(__bf16,1)',
    # GCC and Clang 18.x+ use __SVBFloat16_t
    '__SVBfloat16_t' : '__edg_scalable_vector_type__(__bf16,1)',
    '__SVFloat16_t' : '__edg_scalable_vector_type__(__fp16,1)',
    '__SVFloat32_t' : '__edg_scalable_vector_type__(float,1)',
    '__SVFloat64_t' : '__edg_scalable_vector_type__(double,1)',
    '__SVMfloat8_t' : '__edg_scalable_vector_type__(__mfp8,1)',
    # GCC/Clang ARM NEON vector types (aarch64)
    '__Bfloat16x4_t' : '__edg_neon_vector_type__(__bf16,4)',
    '__Bfloat16x8_t' : '__edg_neon_vector_type__(__bf16,8)',
    '__Float16x4_t' : '__edg_neon_vector_type__(__fp16,4)',
    '__Float16x8_t' : '__edg_neon_vector_type__(__fp16,8)',
    '__Float32x2_t' : '__edg_neon_vector_type__(float,2)',
    '__Float32x4_t' : '__edg_neon_vector_type__(float,4)',
    '__Float64x1_t' : '__edg_neon_vector_type__(double,1)',
    '__Float64x2_t' : '__edg_neon_vector_type__(double,2)',
    '__Int8x8_t' : '__edg_neon_vector_type__(signed char,8)',
    '__Int8x16_t' : '__edg_neon_vector_type__(signed char,16)',
    '__Int16x4_t' : '__edg_neon_vector_type__(short,4)',
    '__Int16x8_t' : '__edg_neon_vector_type__(short,8)',
    '__Int32x2_t' : '__edg_neon_vector_type__(int,2)',
    '__Int32x4_t' : '__edg_neon_vector_type__(int,4)',
    '__Int64x1_t' : '__edg_neon_vector_type__(long,1)',
    '__Int64x2_t' : '__edg_neon_vector_type__(long,2)',
    '__Uint8x8_t' : '__edg_neon_vector_type__(unsigned char,8)',
    '__Uint8x16_t' : '__edg_neon_vector_type__(unsigned char,16)',
    '__Uint16x4_t' : '__edg_neon_vector_type__(unsigned short,4)',
    '__Uint16x8_t' : '__edg_neon_vector_type__(unsigned short,8)',
    '__Uint32x2_t' : '__edg_neon_vector_type__(unsigned int,2)',
    '__Uint32x4_t' : '__edg_neon_vector_type__(unsigned int,4)',
    '__Uint64x1_t' : '__edg_neon_vector_type__(unsigned long,1)',
    '__Uint64x2_t' : '__edg_neon_vector_type__(unsigned long,2)',
    '__Poly8x8_t' : '__edg_neon_polyvector_type__(unsigned char,8)',
    '__Poly8x16_t' : '__edg_neon_polyvector_type__(unsigned char,16)',
    '__Poly16x4_t' : '__edg_neon_polyvector_type__(unsigned short,4)',
    '__Poly16x8_t' : '__edg_neon_polyvector_type__(unsigned short,8)',
    '__Poly64x1_t' : '__edg_neon_polyvector_type__(unsigned long,1)',
    '__Poly64x2_t' : '__edg_neon_polyvector_type__(unsigned long,2)',
    # GCC ARM NEON vector types (armv7)
    '__simd64_bfloat16_t' : '__edg_neon_vector_type__(__bf16,4)',
    '__simd128_bfloat16_t' : '__edg_neon_vector_type__(__bf16,8)',
    '__simd64_float16_t' : '__edg_neon_vector_type__(__fp16,4)',
    '__simd128_float16_t' : '__edg_neon_vector_type__(__fp16,8)',
    '__simd64_float32_t' : '__edg_neon_vector_type__(float,2)',
    '__simd128_float32_t' : '__edg_neon_vector_type__(float,4)',
    '__simd64_int8_t' : '__edg_neon_vector_type__(signed char,8)',
    '__simd128_int8_t' : '__edg_neon_vector_type__(signed char,16)',
    '__simd64_int16_t' : '__edg_neon_vector_type__(short,4)',
    '__simd128_int16_t' : '__edg_neon_vector_type__(short,8)',
    '__simd64_int32_t' : '__edg_neon_vector_type__(int,2)',
    '__simd128_int32_t' : '__edg_neon_vector_type__(int,4)',
    '__simd64_int64_t' : '__edg_neon_vector_type__(long long,1)',
    '__simd128_int64_t' : '__edg_neon_vector_type__(long long,2)',
    '__simd64_uint8_t' : '__edg_neon_vector_type__(unsigned char,8)',
    '__simd128_uint8_t' : '__edg_neon_vector_type__(unsigned char,16)',
    '__simd64_uint16_t' : '__edg_neon_vector_type__(unsigned short,4)',
    '__simd128_uint16_t' : '__edg_neon_vector_type__(unsigned short,8)',
    '__simd64_uint32_t' : '__edg_neon_vector_type__(unsigned int,2)',
    '__simd128_uint32_t' : '__edg_neon_vector_type__(unsigned int,4)',
    '__simd64_uint64_t' : '__edg_neon_vector_type__(unsigned long long,1)',
    '__simd128_uint64_t' : '__edg_neon_vector_type__(unsigned long long,2)',
    '__simd64_poly8_t' : '__edg_neon_polyvector_type__(signed char,8)',
    '__simd128_poly8_t' : '__edg_neon_polyvector_type__(signed char,16)',
    '__simd64_poly16_t' : '__edg_neon_polyvector_type__(short,4)',
    '__simd128_poly16_t' : '__edg_neon_polyvector_type__(short,8)',
    '__simd64_poly64_t' : '__edg_neon_polyvector_type__(long long,1)',
    '__simd128_poly64_t' : '__edg_neon_polyvector_type__(long long,2)',
}

# scalable vector types are represented with a '__clang_' prefix by Clang, but
# GCC uses the typedefs from the header file
SV_TYPE_MAPPING = {
    'svboolx2_t' : '__edg_scalable_vector_type__(__edg_bool_type__,2)',
    'svint8x2_t' : '__edg_scalable_vector_type__(signed char,2)',
    'svint16x2_t' : '__edg_scalable_vector_type__(short,2)',
    'svint32x2_t' : '__edg_scalable_vector_type__(int,2)',
    'svint64x2_t' : '__edg_scalable_vector_type__(long,2)',
    'svuint8x2_t' : '__edg_scalable_vector_type__(unsigned char,2)',
    'svuint16x2_t' : '__edg_scalable_vector_type__(unsigned short,2)',
    'svuint32x2_t' : '__edg_scalable_vector_type__(unsigned int,2)',
    'svuint64x2_t' : '__edg_scalable_vector_type__(unsigned long,2)',
    'svbfloat16x2_t' : '__edg_scalable_vector_type__(__bf16,2)',
    'svfloat16x2_t' : '__edg_scalable_vector_type__(__fp16,2)',
    'svfloat32x2_t' : '__edg_scalable_vector_type__(float,2)',
    'svfloat64x2_t' : '__edg_scalable_vector_type__(double,2)',
    'svmfloat8x2_t' : '__edg_scalable_vector_type__(__mfp8,2)',
    'svboolx3_t' : '__edg_scalable_vector_type__(__edg_bool_type__,3)',
    'svint8x3_t' : '__edg_scalable_vector_type__(signed char,3)',
    'svint16x3_t' : '__edg_scalable_vector_type__(short,3)',
    'svint32x3_t' : '__edg_scalable_vector_type__(int,3)',
    'svint64x3_t' : '__edg_scalable_vector_type__(long,3)',
    'svuint8x3_t' : '__edg_scalable_vector_type__(unsigned char,3)',
    'svuint16x3_t' : '__edg_scalable_vector_type__(unsigned short,3)',
    'svuint32x3_t' : '__edg_scalable_vector_type__(unsigned int,3)',
    'svuint64x3_t' : '__edg_scalable_vector_type__(unsigned long,3)',
    'svbfloat16x3_t' : '__edg_scalable_vector_type__(__bf16,3)',
    'svfloat16x3_t' : '__edg_scalable_vector_type__(__fp16,3)',
    'svfloat32x3_t' : '__edg_scalable_vector_type__(float,3)',
    'svfloat64x3_t' : '__edg_scalable_vector_type__(double,3)',
    'svmfloat8x3_t' : '__edg_scalable_vector_type__(__mfp8,3)',
    'svboolx4_t' : '__edg_scalable_vector_type__(__edg_bool_type__,4)',
    'svint8x4_t' : '__edg_scalable_vector_type__(signed char,4)',
    'svint16x4_t' : '__edg_scalable_vector_type__(short,4)',
    'svint32x4_t' : '__edg_scalable_vector_type__(int,4)',
    'svint64x4_t' : '__edg_scalable_vector_type__(long,4)',
    'svuint8x4_t' : '__edg_scalable_vector_type__(unsigned char,4)',
    'svuint16x4_t' : '__edg_scalable_vector_type__(unsigned short,4)',
    'svuint32x4_t' : '__edg_scalable_vector_type__(unsigned int,4)',
    'svuint64x4_t' : '__edg_scalable_vector_type__(unsigned long,4)',
    'svbfloat16x4_t' : '__edg_scalable_vector_type__(__bf16,4)',
    'svfloat16x4_t' : '__edg_scalable_vector_type__(__fp16,4)',
    'svfloat32x4_t' : '__edg_scalable_vector_type__(float,4)',
    'svfloat64x4_t' : '__edg_scalable_vector_type__(double,4)',
    'svmfloat8x4_t' : '__edg_scalable_vector_type__(__mfp8,4)',
}

for typename, decl in SV_TYPE_MAPPING.items():
    TYPE_MAPPING[typename] = decl
    TYPE_MAPPING[f'__clang_{typename}'] = decl


class Config:
    """
    Representation of a compiler configuration.
    """
    def __init__(self, compiler, bits, arch, version):
        self.compiler = compiler
        self.bits = bits
        self.arch = arch
        self.version = version

    def get_secondary(self):
        """
        Returns the representation of a secondary compiler configuration.
        """
        return Config('S' + self.compiler, self.bits, self.arch, self.version)

    def __members(self):
        return (self.compiler, self.bits, self.arch, self.version)

    def __eq__(self, o):
        return type(self) is type(o) and self.__members() == o.__members()

    def __hash__(self):
        return hash(self.__members())

    def __repr__(self):
        return f'{self.compiler}_{self.version}_{self.arch}_{self.bits}'


class Signature:
    """
    Representation of a function signature.
    """
    def __init__(self, types, qualifiers):
        self.types = types
        self.qualifiers = qualifiers

    def __members(self):
        return (self.types, self.qualifiers)

    def __lt__(self, o):
        return type(self) is type(o) and self.__members() < o.__members()

    def __eq__(self, o):
        return type(self) is type(o) and self.__members() == o.__members()

    def __hash__(self):
        return hash(self.__members())

    def __str__(self):
        ret = self.types[0]
        params = ','.join(self.types[1:])
        qualifiers = ' ' + self.qualifiers if self.qualifiers else ''

        return f'{ret} ({params}){qualifiers}'

    def __repr__(self):
        return self.__str__()


def normalize_qualifiers(s):
    """
    Normalize no-throw and no-return qualifiers.
    """
    s.replace(' (', '(')
    quals = []

    for tok in s.split():
        if tok == 'throw()' or tok == 'noexcept':
            quals.insert(0, '__edg_throw__()')
        elif tok == '__attribute__((noreturn))':
            quals.append('__attribute((noreturn))')
        elif tok == 'noexcept(false)':
            pass
        else:
            quals.append(tok)

    return ' '.join(quals)


def tokenize(ty):
    """
    Return a simple tokenized representation of the given type string.  "*"
    and "&" are always individual tokens, otherwise tokens are separated by
    spaces (except that parenthesized strings are considered as single tokens).
    For example:

        const int* -> const int *
    """
    tokens = []
    depth, curr = 0, ''

    for idx, c in enumerate(ty):
        if c == '(':
            depth += 1
        elif c == ')':
            depth -= 1

        if depth != 0:
            curr += c
        elif c in ' *&':
            if curr:
                tokens.append(curr)
                curr = ''
            if c != ' ':
                tokens.append(c)
        else:
            curr += c

    if curr:
        tokens.append(curr)

    return tokens


def normalize_type(ty):
    """
    Return a normalized representation of the given type.
    """
    tokens = tokenize(ty)

    vector_size, vector_start, vector_end = None, None, None
    for idx, tok in enumerate(tokens):
        if tok in TYPE_MAPPING:
            tok = TYPE_MAPPING[tok]
            tokens[idx] = tok

        # Check for a vector attribute or a vector modifier.
        mo = VECTOR_ATTR_RE.match(tok) or VECTOR_MODIFIER_RE.match(tok)
        if mo:
            vector_size = mo.group('size')
            vector_start = idx

        if (vector_start is not None and vector_end is None and
            tok in ('*', '&', 'const', 'volatile')):
            # Remember the end of the vector element type.
            vector_end = idx

    if vector_start is not None:
        if not vector_end:
            vector_end = len(tokens)
        element_type = normalize_type(' '.join(tokens[vector_start + 1:
                                                      vector_end]))
        # Replace the vector specification with __edg_vector_type__.
        del tokens[vector_start:vector_end]
        tokens.insert(vector_start,
                      f'__edg_vector_type__({element_type}, {vector_size})')

    # Reorder qualifiers and modifiers.
    prefix, postfix = [ None, None, None ], []
    ordered = { 'const' : 0,
                'volatile' : 1,
                'signed' : 2,
                'unsigned' : 2 }
    had_ptr = False

    for idx, tok in enumerate(tokens):
        if tok in '*&':
            had_ptr = True

        if tok in ('__complex__', '_Complex'):
            # __complex__ always needs to be appended at the end.
            postfix.append('__complex__')
            tokens[idx] = None

        if tok in ('restrict', '__restrict'):
            # Remove any restrict modifiers.
            tokens[idx] = None

        if not had_ptr:
            # Check for any qualifiers or modifiers we need to reorder.
            prefix_pos = ordered.get(tok, None)
            if prefix_pos is not None:
                prefix[prefix_pos] = tok
                tokens[idx] = None

    tokens = list(filter(lambda s: s is not None, prefix + tokens + postfix))

    for class_name in ('__va_list_tag', '_IO_FILE', '__NSConstantString'):
        # Add a struct if not already present.
        try:
            pos = tokens.index(class_name)
            if pos == 0 or tokens[pos - 1] != 'struct':
                tokens.insert(pos, 'struct')
        except ValueError:
            pass

    try:
        pos = tokens.index('__va_list_tag')
        if (pos >= 1 and pos < len(tokens) - 1 and
            tokens[pos - 1] == 'struct' and tokens[pos + 1] == '*'):
            # Replace struct __va_list_tag * with __builtin_va_list.
            tokens[pos - 1:pos + 2] = ['__builtin_va_list']
    except ValueError:
        pass

    try:
        pos = tokens.index('__int128_t')
        if pos >= 1 and tokens[pos - 1] == 'unsigned':
            # Replace unsigned __int128_t with __uint128_t.
            tokens[pos] = '__uint128_t'
            del tokens[pos - 1:pos]
    except ValueError:
        pass

    try:
        pos = tokens.index('int')
        if (pos >= 1 and
            (tokens[pos - 1] == 'unsigned' or tokens[pos - 1] == 'long' or
             tokens[pos - 1] == 'short')):
            # Remove redundant int after unsigned, long, or short.
            del tokens[pos:pos + 1]
    except ValueError:
        pass

    # Join the tokens together, removing any redundant spaces.
    return ' '.join(tokens).replace(' *', '*') \
                           .replace('* ', '*') \
                           .replace(' &', '&') \
                           .replace('& ', '&') \
                           .replace(', ', ',')


def split_params_and_qualifiers(s):
    """
    Split parameters-and-qualifiers into a tuple of parameters and the
    qualifiers.
    """
    assert s[0] == '('
    s = s[1:]
    start_pos, depth, params = 0, 1, []

    for pos, c in enumerate(s):
        if c == '(':
            depth += 1
        elif c == ')':
            depth -= 1
            if depth == 0:
                params.append(s[start_pos:pos].strip())
                start_pos = pos + 1
                break
        elif c == ',' and depth == 1:
            params.append(s[start_pos:pos].strip())
            start_pos = pos + 1

    # Normalize the parameter types
    params = tuple(map(normalize_type, params))
    qualifiers = normalize_qualifiers(s[start_pos:].strip())

    if params == ('',):
        params = ('void',)
    elif params == ('void&',):
        # Used by Clang to represent any type.
        params = ('...',)

    return params, qualifiers


def get_bitset_for_configs(cfgs):
    """
    Returns the set of bits for the given configuration set.
    """
    return set(map(lambda x: x.bits, cfgs))


def get_archset_for_configs(cfgs):
    """
    Returns the set of architectures for the given configuration set.  If
    the configuration set contains all possible architectures and bit-widths
    (with matching compiler versions), a set of { '*' } is returned instead
    (representing the "common" set).
    """
    archs = set(map(lambda x: x.arch, cfgs))
    if len(archs) == len(all_archs):
        mismatch = False
        compilers = set(map(lambda x: x.compiler, cfgs))
        for compiler in sorted(compilers):
            compiler_cfgs = set(filter_by_compiler(cfgs, compiler))
            for bits in sorted(all_bits):
                compiler_bits_cfgs = set(filter_by_bits(compiler_cfgs, bits))
                other_versions = (set(), set())

                for arch in sorted(archs):
                    all_versions = all_versions_by_compiler_arch_bits \
                        .get((compiler.lstrip('S'), arch, bits), set())
                    my_cfgs = set(filter_by_arch(compiler_bits_cfgs, arch))
                    my_versions = set(map(lambda x: x.version, my_cfgs))

                    # When comparing compiler version sets, only versions where
                    # an actual configuration is available need to match.
                    if my_versions.intersection(other_versions[1]) != \
                       other_versions[0].intersection(all_versions):
                        mismatch = True

                    other_versions = (my_versions, all_versions)

        if not mismatch:
            archs = { '*' }

    return archs


def classify_type(ty, bitset):
    """
    Return a classification for the given type and set of bits where that
    type was used.
    """
    cls = None

    if ((ty == 'unsigned' and bitset == {32}) or
        (ty == 'unsigned long' and bitset == {64})):
        cls = '__edg_size_type__'
    elif ((ty == 'const unsigned' and bitset == {32}) or
          (ty == 'const unsigned long' and bitset == {64})):
        cls = 'const __edg_size_type__'
    elif ((ty == 'int' and bitset == {32}) or
          (ty == 'long' and bitset == {64})):
        cls = '__edg_ptrdiff_type__'
    elif ((ty == '__int128_t' and bitset == {32}) or
          (ty == '__uint128_t' and bitset == {64})):
        cls = '__uint128_t'

    return cls


def get_restrictions(types):
    """
    Return a set of restrictions for the given list of types.
    """
    restrictions = set()

    for ty in types:
        if (ty.startswith('__edg_vector_type__(') or
            ty.startswith('__edg_neon_vector_type__(') or
            ty.startswith('__edg_neon_polyvector_type__(') or
            ty.startswith('__edg_scalable_vector_type__(')):
            restrictions.add('v')
        elif ty.startswith('__uint128_t') or ty.startswith('__int128_t'):
            restrictions.add('i')
        elif (ty.startswith('__Poly') or ty.startswith('__builtin_neon_') or
              ty.startswith('__builtin_aarch64_simd_')):
            restrictions.add('v')
            if (ty.startswith('__Poly128_t') or
                ty.startswith('__builtin_aarch64_simd_poly128') or
                ty.startswith('__builtin_aarch64_simd_ci') or
                ty.startswith('__builtin_aarch64_simd_oi') or
                ty.startswith('__builtin_aarch64_simd_ti') or
                ty.startswith('__builtin_aarch64_simd_xi') or
                ty.startswith('__builtin_neon_poly128') or
                ty.startswith('__builtin_neon_uti')):
                restrictions.add('i')
        elif ty.startswith('__float128') or ty.startswith('_Float128'):
            restrictions.add('f')
        elif ty.startswith('char8_t'):
            restrictions.add('c')

    return restrictions


def has_reference_type(types):
    """
    Return TRUE if the list of types contains a reference type.
    """
    return len(list(filter(lambda p: p.endswith('&'), types))) != 0


def filter_by_bits(cfgs, bits):
    """
    Return the configurations from a given set of configurations that match
    the specified number of bits.
    """
    return filter(lambda cfg: cfg.bits == bits, cfgs)


def filter_by_arch(cfgs, arch):
    """
    Return the configurations from a given set of configurations that match
    the specified architecture.
    """
    return filter(lambda cfg: cfg.arch == arch, cfgs)


def filter_by_compiler(cfgs, compiler):
    """
    Return the configurations from a given set of configurations that match
    the specified compiler.
    """
    return filter(lambda cfg: cfg.compiler == compiler, cfgs)


def map_to_versions(cfgs):
    """
    Return the set of versions for the given set of configurations.
    """
    return set(map(lambda cfg: cfg.version, cfgs))


def get_lower_bound(first_version, all_versions):
    """
    Return a lower bound for the given first version in a set of
    all_versions.  If we have 3-part version numbers (i.e., major.minor.patch),
    we adjust the version by stripping the minor and/or patch version
    components in cases where all_version doesn't contain a version between the
    original and the adjusted version (the minor version only gets adjusted if
    it was 1).
    """
    res = first_version

    if first_version >= 10000:
        prev_version = sorted(filter(lambda x: x < first_version,
                                     all_versions))[-1]
        version_no_minor = first_version - (first_version % 10000)
        version_no_patch = first_version - (first_version % 100)
        if ((first_version % 10000) // 100 == 1 and
            version_no_minor > prev_version):
            # Strip off any minor and patch version components.
            res = version_no_minor
        elif version_no_patch > prev_version:
            # Strip off any patch version component.
            res = version_no_patch

    return res


def get_upper_bound(last_version, all_versions):
    """
    Return an upper bound for the given last version in a set of
    all_versions.  If we have 3-part version numbers (i.e., major.minor.patch),
    we use the next version in all_versions, adjusted as in get_lower_bound,
    minus 1.
    """
    res = last_version

    if last_version >= 10000:
        next_version = sorted(filter(lambda x: x > last_version,
                                     all_versions))[0]
        next_no_minor = next_version - (next_version % 10000)
        next_no_patch = next_version - (next_version % 100)
        if ((next_version % 10000) // 100 == 1 and
            next_no_minor > last_version):
            res = next_no_minor - 1
        elif next_no_patch > last_version:
            res = next_no_patch - 1

    return res


def generate_version_condition(versions, all_versions, compiler_descr,
                               res_str):
    """
    Return a parenthesized version description for the given set of
    versions.  all_versions is the set of all possible versions, compiler_descr
    is the compiler descriptor, and res_str is the string representation for
    the type restrictions.
    """
    s = ''

    if versions == all_versions:
        s = compiler_descr + res_str
    else:
        version_list = sorted(all_versions)
        first, last, prev = None, None, None

        for v in version_list:
            if v in versions:
                if first is None:
                    first = v
            else:
                if first is not None:
                    # output range
                    if first == version_list[0]:
                        rng = '(-%d)' % (get_upper_bound(prev, all_versions),)
                    else:
                        rng = '(%d-%d)' % (get_lower_bound(first,
                                                           all_versions),
                                           get_upper_bound(prev, all_versions))
                    s += compiler_descr + rng + res_str

                    first, last = None, None

            prev = v

        if first is not None:
            rng = '(%d-)' % (get_lower_bound(first, all_versions),)
            s += compiler_descr + rng + res_str

    return s


def generate_condition(cfgs, arch, restrictions):
    """
    Return a condition string for the given compiler configurations.  arch
    is the single-letter architecture identifier and restrictions is a set of
    restrictions for the signature.
    """
    s = ''
    res_str = '[' + ''.join(sorted(restrictions)) + ']' if restrictions else ''

    for compiler_descr in ('Sgx', 'gx', 'SLx', 'Lx', 'Smx', 'mx'):
        compiler_cfgs = set(filter_by_compiler(cfgs, compiler_descr))
        if not compiler_cfgs:
            continue

        compiler = compiler_descr.lstrip('S')
        if arch != '*':
            all_versions = all_versions_by_compiler_arch[(compiler, arch)]
            all_versions_32bit = all_versions_by_compiler_arch_bits[(compiler,
                                                                     arch, 32)]
            all_versions_64bit = all_versions_by_compiler_arch_bits[(compiler,
                                                                     arch, 64)]
            diff_all_versions = \
                all_versions_32bit.symmetric_difference(all_versions_64bit)
        else:
            all_versions = all_versions_by_compiler[compiler]
            all_versions_32bit = all_versions_64bit = all_versions
            diff_all_versions = set()

        versions_32bit = map_to_versions(filter_by_bits(compiler_cfgs, 32))
        versions_64bit = map_to_versions(filter_by_bits(compiler_cfgs, 64))

        if versions_32bit.symmetric_difference(versions_64bit) == \
           diff_all_versions:
            # Adjust version data for cases where we only have data for either
            # 32 bits or 64 bits.
            versions_32bit.update(diff_all_versions)
            versions_64bit.update(diff_all_versions)

        versions_common = versions_32bit.intersection(versions_64bit)
        versions_32bit.difference_update(versions_common)
        versions_64bit.difference_update(versions_common)

        if versions_common:
            s += generate_version_condition(versions_common, all_versions,
                                            compiler_descr, res_str)
        if versions_32bit:
            s += generate_version_condition(versions_32bit, all_versions_32bit,
                                            compiler_descr + '4', res_str)
        if versions_64bit:
            s += generate_version_condition(versions_64bit, all_versions_64bit,
                                            compiler_descr + '8', res_str)

    return s


def get_bfk_name(name):
    """
    Return the name to be used for the builtin function kind for the given
    function name.
    """
    name = name.split(':')[-1]
    if name.startswith('__builtin_'):
        return name[10:]
    elif name.startswith('__atomic_'):
        return name[2:]
    elif name.startswith('__sync_'):
        return name[2:]

    return name


def generate_id(s):
    """
    Return an id for the given string.  This uses the first 10 hex digits
    of the MD5 hash of the string with a newline character added.
    """
    return hashlib.md5(s.encode('ascii') + b'\n').hexdigest()[:10]


def get_signature_partition(sig):
    """
    Return a partition index for a signature.  Currently, a signature
    containing a scalable vector type uses the partition id 1, and a signature
    containing a RISC-V vector type uses the partition id 2.  Any other
    signature gets a partition id of 0.
    """
    if sig.find('__rvv_') != -1:
        return 2
    elif sig.find('__edg_scalable_vector_type') != -1:
        return 1
    else:
        return 0


def split_riscv_builtin_name(name):
    """
    Return a tuple of the name (as a list of '_'-separated parts), the
    policy mode and a flag indicating wheter it includes a rounding mode.
    """
    assert name.startswith('__riscv_')
    parts = name[8:].split('_')

    if parts and parts[-1] in ('m', 'mu', 'tu', 'tum', 'tumu'):
        policy = parts[-1]
        del parts[-1:]
    else:
        policy = ''

    has_round_mode = parts and parts[-1] == 'rm'
    if has_round_mode:
        del parts[-1:]

    return parts, policy, has_round_mode


def encode_builtin_name(name, prev_name):
    """
    Encode the name in a way that can elide a common prefix.  If the name
    has a common prefix (for '_'-separated parts), use a '#' character,
    followed by the number of '_'-separated parts to strip from the end and the
    new suffix string.
    """
    if not prev_name:
        return name

    parts = name.split('_')
    prev_parts = prev_name.split('_')

    prefix_len = 0
    for s1, s2 in zip(parts, prev_parts):
        if s1 == s2:
            prefix_len += 1
        else:
            break

    prefix = '_'.join(parts[:prefix_len])
    if prefix == prefix_len*'_':
        s = name
    else:
        s = '#%x' % (len(prev_parts) - prefix_len,)
        del parts[:prefix_len]
        if parts:
            s += '_' + '_'.join(parts)

    return s


def make_at_encoding(beg, end=0):
    """
    Return the '@' overload encoding string for the given counts.  The '@'
    indicates that an additional function declaration should be added with
    parts of the name stripped off (from the end, go back beg '_'-separated
    parts and strip away the next beg - end parts, or until the end of the
    string).  The encoding is interpreted by
    load_overloadable_builtin_symbols.
    """
    # The '@' encoding format only supports single-digit counts.
    assert beg < 10
    enc = f'@{beg}'
    if end:
        enc += str(beg - end)
    return enc


def apply_at_encoding(name, enc_ovl):
    """
    Return the additional overload name implied by enc_ovl for name.
    enc_ovl has the form '@n' or '@nm' as described in
    load_overloadable_builtin_symbols.
    """
    assert enc_ovl and enc_ovl[0] == '@'
    n_skip = int(enc_ovl[1])
    parts = name.split('_')
    if len(enc_ovl) > 2:
        m_skip = int(enc_ovl[2])
        idx = len(parts) - n_skip
        ovl_parts = parts[:idx] + parts[idx + m_skip:]
    else:
        ovl_parts = parts[:-n_skip]
    return '_'.join(ovl_parts)


class Builtin_Overload:
    """
    Information for an overloaded builtin whose unadorned overload set is
    declared via the '@' encodings of its variants.  policies maps each
    RISC-V policy variant ('', 'tu', 'mu', 'tum', 'tumu', ...) to the set
    of compilers ('gx', 'Lx', 'mx') that provide it (unused for other
    architectures).
    """
    def __init__(self, name):
        self.name = name
        self.policies = { }

def is_overloaded(signatures, name):
    """
    Return True if the builtin is overloaded (i.e., the set of signatures
    contains an overload for at least one configuration, or it is one of the
    vlmul_ext or vlmul_trunc builtins that should be treated as overloaded).
    """
    if ((name.startswith('__riscv_vlmul_ext') and
         not name.startswith('__riscv_vlmul_ext_v_')) or
        (name.startswith('__riscv_vlmul_trunc') and
         not name.startswith('__riscv_vlmul_trunc_v_'))):
        # Some of these might only have a single signature, but they still need
        # to be treated as overloaded candidates
        return True

    nr_cfgs, cfg_union = 0, set()

    for sig, cfgs in signatures.items():
        nr_cfgs += len(cfgs)
        cfg_union.update(cfgs)

    return nr_cfgs != len(cfg_union)

class Overload_Table_Handler:
    """
    Handler for the '@' overload compression of a family of builtin tables
    (i.e., the tables entered by load_overloadable_builtin_symbols).  An
    overloaded builtin (e.g., svaba) is not emitted directly for
    configurations in which its signatures can be derived from variant
    builtins (e.g., svaba_s32); those variants carry an '@' encoding
    instead, so that the front end declares the unadorned overload set from
    the variant entries.  The base class implements the common machinery;
    subclasses provide the architecture-specific details.
    """
    def __init__(self, arch, headers, guard=None):
        # Single-letter architecture identifier of the table family.
        self.arch = arch
        # Table qualifiers (header names) of the table family.
        self.headers = headers
        # Preprocessor condition guarding the emitted tables, if any.
        self.guard = guard
        # Map of (table qualifier, '_'-separated name parts) to the
        # Builtin_Overload of the overloaded builtin with that name.
        self.overloaded = { }
        # Map of (table id, name, condition, signature) to a list of
        # (condition string, '@' encoding) pairs to emit for the entry.
        self.entry_enc = { }

    def matches(self, header):
        """
        Return True if the given table qualifier belongs to this table
        family.
        """
        return header in self.headers

    def is_candidate_name(self, name):
        """
        Return True if a builtin with the given name can be an overloaded
        builtin of this table family or a variant of one.
        """
        return True

    def add_overload(self, qual, name, signatures):
        """
        Record the overloaded builtin with the given name and signatures in
        the table qualified by qual.
        """
        self.overloaded.setdefault((qual, tuple(name.split('_'))),
                                   Builtin_Overload(name))

    def extract_overloads(self, builtins_map):
        """
        Populate the overloaded map from the given map of builtin qualnames
        to their signatures.
        """
        for qualname, signatures in sorted(builtins_map.items(),
                                           key=lambda s: (s[0].count('_'),
                                                          s[0])):
            if qualname.find(':') == -1:
                continue
            qual, name = qualname.split(':')
            if (self.matches(qual) and self.is_candidate_name(name) and
                is_overloaded(signatures, name)):
                self.add_overload(qual, name, signatures)

    def find_prefixed_overload(self, header, parts, suffix=()):
        """
        Return a tuple of the Builtin_Overload whose name parts are the
        longest '_'-separated prefix of parts (followed by the parts in
        suffix) and the length of that prefix, or (None, 0) if there is no
        such overloaded builtin.  header is the table qualifier.
        """
        ovl_info, prefix_len = None, 0
        for i in range(len(parts) - 1, 0, -1):
            key = (header, tuple(parts[:i]) + tuple(suffix))
            ovl_info = self.overloaded.get(key, None)
            if ovl_info:
                prefix_len = i
                break
        return ovl_info, prefix_len

    def analyze_variants(self, builtins_map):
        """
        Analyze the non-overloaded builtins of this table family to
        determine from which overloaded builtin their entries are derived
        (if any).
        """

    def adjust_bfk(self, qual, name, qualname, bfk_name):
        """
        Return a tuple of the builtin function kind name to use for the
        builtin with the given qualified name (that of the overloaded
        builtin for a variant) and a flag indicating whether the builtin
        should be skipped altogether (because it is an overloaded builtin
        whose entries are all derived from variants).
        """
        return bfk_name, False

    def get_sig_splits(self, qualname, sig, sig_cfgs):
        """
        Return the list of (configurations, '@' encoding) pairs into which
        the table entry for the given signature of the builtin with the
        given qualified name is split, or an empty list if no entry should
        be emitted for the signature.
        """
        return [(sig_cfgs, '')]

    def compute_entry_encodings(self):
        """
        Compute the '@' overload encodings of the table entries of this
        family (filling entry_enc).  Called after builtins_by_table has
        been populated and before the conditions enumeration is emitted
        (entries may be split into per-compiler entries with new condition
        strings).
        """


# One alternative in a builtin condition string (see a_builtin_user_descr).
COND_UNIT_RE = re.compile(
    r'S?(?P<comp>[Lgm])[c+x][ARX]?(?P<bits>[48])?'
    r'(?:\((?P<vmin>\d+)?-(?P<vmax>\d+)?\))?'
    r'(?:\[[vifc]+\])?'
)


def parse_condition_units(cond):
    """
    Return a list of (compiler, bits, vmin, vmax) tuples for cond, or None
    if the string cannot be parsed completely.  bits is 4, 8, or None (any).
    vmin/vmax are inclusive; None means unbounded.
    """
    units = []
    pos = 0
    while pos < len(cond):
        mo = COND_UNIT_RE.match(cond, pos)
        if not mo:
            return None
        bits = int(mo.group('bits')) if mo.group('bits') else None
        vmin = int(mo.group('vmin')) if mo.group('vmin') else None
        vmax = int(mo.group('vmax')) if mo.group('vmax') else None
        units.append((mo.group('comp'), bits, vmin, vmax))
        pos = mo.end()
    return units


def condition_units_overlap(u1, u2):
    """
    Return True if the two parsed condition units can both be true in some
    emulation.
    """
    comp1, bits1, vmin1, vmax1 = u1
    comp2, bits2, vmin2, vmax2 = u2
    if comp1 != comp2:
        return False
    if bits1 is not None and bits2 is not None and bits1 != bits2:
        return False
    lo1 = vmin1 if vmin1 is not None else 0
    lo2 = vmin2 if vmin2 is not None else 0
    hi1 = vmax1 if vmax1 is not None else 10 ** 9
    hi2 = vmax2 if vmax2 is not None else 10 ** 9
    return max(lo1, lo2) <= min(hi1, hi2)


# Map of condition compiler letters to compiler designators.
COND_COMPILERS = { 'g' : 'gx', 'L' : 'Lx', 'm' : 'mx' }


def condition_compilers(cond):
    """
    Return the set of compiler designators ('gx', 'Lx', 'mx') enabled by the
    given condition string, or None if the string cannot be parsed.
    """
    units = parse_condition_units(cond)
    if units is None:
        return None
    return set(COND_COMPILERS[u[0]] for u in units)


def split_condition_by_compiler(cond):
    """
    Return a dict mapping each compiler designator in cond to the
    concatenation of its raw condition units, or None if cond cannot be parsed
    completely.
    """
    units = { }
    pos = 0
    while pos < len(cond):
        mo = COND_UNIT_RE.match(cond, pos)
        if not mo:
            return None
        units.setdefault(COND_COMPILERS[mo.group('comp')], []).append(
            mo.group(0))
        pos = mo.end()
    return { comp : ''.join(raw) for comp, raw in units.items() }


def conditions_overlap_for_compiler(c1, c2, compiler):
    """
    Return True if there is some version/bit-width of the given compiler in
    which both condition strings are enabled.  If a string cannot be parsed,
    treat it as overlapping.
    """
    u1, u2 = parse_condition_units(c1), parse_condition_units(c2)
    if u1 is None or u2 is None:
        return True
    letter = compiler[0]
    for a in u1:
        for b in u2:
            if (a[0] == letter and b[0] == letter and
                condition_units_overlap(a, b)):
                return True
    return False


def intern_condition(cond):
    """
    Return the condition id for the given condition string, adding it to
    all_conditions/all_cond_ids if it is not already present.
    """
    if cond not in all_conditions:
        cond_id = generate_id(cond)
        all_cond_ids.append((cond_id, cond))
        all_conditions[cond] = cond_id
    return all_conditions[cond]


class RISCV_Vector_Handler(Overload_Table_Handler):
    """
    Overload compression handler for the RISC-V vector C API tables.
    """
    def is_candidate_name(self, name):
        return name.startswith('__riscv_')

    def add_overload(self, qual, name, signatures):
        parts, policy, _ = split_riscv_builtin_name(name)
        ovl_info = self.overloaded.setdefault(
            (qual, tuple(parts)), Builtin_Overload('_'.join(parts)))
        ovl_info.policies.setdefault(policy, set()).update(
            cfg.compiler.lstrip('S')
            for cfgs in signatures.values() for cfg in cfgs)

    def find_overload(self, header, parts):
        """
        Get overload information for a RISC-V builtin given by its name
        split into '_'-separated parts.  header is the table qualifier of
        the builtin.  Note that parts is modified in place: if no direct
        match is found, the last part is removed and a match with that part
        as a suffix is attempted instead (in which case suffix_len is 1).
        """
        ovl_info, _ = self.find_prefixed_overload(header, parts)
        suffix_len = 0
        if not ovl_info:
            suffix, suffix_len = parts[-1:], 1
            del parts[-1:]
            ovl_info, _ = self.find_prefixed_overload(header, parts, suffix)
        return ovl_info, suffix_len

    def get_at_encoding(self, header, name, compiler=None):
        """
        Return the '@' encoding used to declare an additional unadorned
        overload for the RISC-V vector builtin named name, or '' if none
        should be encoded.  header is the table qualifier of the builtin.
        If compiler is not None, consider only overload variants provided
        by that compiler ('gx', 'Lx', or 'mx'); otherwise the union of all
        compilers is considered.
        """
        parts, policy, has_round_mode = split_riscv_builtin_name(name)
        ovl_info, suffix_len = self.find_overload(header, parts)
        enc_ovl = ''
        if ovl_info:
            beg = (len(parts) - ovl_info.name.count('_') - 1 +
                   2 * suffix_len)
            end = suffix_len

            def available(p):
                compilers = ovl_info.policies.get(p, set())
                if compiler is None:
                    return len(compilers) != 0
                return compiler in compilers

            if ((ovl_info.name.startswith('nds_vln') or
                 ovl_info.name.startswith('th_vlb') or
                 ovl_info.name.startswith('th_vlh') or
                 ovl_info.name.startswith('th_vls') or
                 ovl_info.name.startswith('th_vlw') or
                 ovl_info.name.startswith('vle') or
                 ovl_info.name.startswith('vlse') or
                 ovl_info.name.startswith('vlsse') or
                 name.startswith('__riscv_vmv_v_x_')) and
                policy == ''):
                # Some name-mangled builtins are never added to the
                # overloaded builtin.  They usually don't have any vector
                # type in the signature, but for now, we keep a list of
                # those affected.
                ovl_policy = None
            elif available(policy):
                ovl_policy = policy
            elif available(''):
                ovl_policy = ''
            else:
                ovl_policy = None

            if ovl_policy is not None:
                if policy:
                    beg += 1
                    if available(policy):
                        end += 1
                if has_round_mode:
                    beg += 1
                enc_ovl = make_at_encoding(beg, end)

        return enc_ovl

    def adjust_bfk(self, qual, name, qualname, bfk_name):
        if name.startswith('__riscv_'):
            parts, _, _ = split_riscv_builtin_name(name)
            if (qual, tuple(parts)) in self.overloaded:
                # Ignore overloaded builtins, these will be added from the
                # name-mangled builtins.
                return bfk_name, True
            ovl_info, _ = self.find_overload(qual, parts)
            if ovl_info:
                bfk_name = get_bfk_name('__riscv_' + ovl_info.name)
        return bfk_name, False

    def return_type_ovl_collisions(self, table_list):
        """
        Return the set of (name, cond, sig, compiler) entries whose '@'
        abbreviation would declare, for the given compiler, a short name
        that is not a valid C++ overload of another simultaneously-enabled
        entry (same parameters, different return type).  table_list is a
        sequence of (header, builtins_by_table map) pairs for tables that
        can be loaded together.  Clang's unadorned RVV names include such
        overloads; attaching '@' to the corresponding mangled names then
        fails when the table is loaded.
        """
        # Group prospective abbreviations by (short name, compiler, param
        # types).
        groups = { }
        for header, table_builtins in table_list:
            for name, (_, builtin_sigs) in table_builtins.items():
                if not name.startswith('__riscv_'):
                    continue
                for cond, sig in builtin_sigs:
                    compilers = condition_compilers(cond)
                    if compilers is None:
                        # Unparseable condition: conservatively consider all
                        # compilers.
                        compilers = ('gx', 'Lx', 'mx')
                    for compiler in compilers:
                        enc_ovl = self.get_at_encoding(header, name,
                                                       compiler)
                        if not enc_ovl:
                            continue
                        abbr = apply_at_encoding(name, enc_ovl)
                        key = (abbr, compiler, sig.types[1:])
                        groups.setdefault(key, []).append(
                            (name, cond, sig, sig.types[0]))

        suppress = set()
        for (abbr, compiler, _), items in groups.items():
            rets = {item[3] for item in items}
            if len(rets) < 2:
                continue
            for i, a in enumerate(items):
                for b in items[i + 1:]:
                    if (a[3] != b[3] and
                        conditions_overlap_for_compiler(a[1], b[1],
                                                      compiler)):
                        suppress.add((a[0], a[1], a[2], compiler))
                        suppress.add((b[0], b[1], b[2], compiler))
        return suppress

    def compute_suppressed_overloads(self):
        """
        Return the set of (name, cond, sig, compiler) entries for which the
        '@' encoding must be omitted because of return-type collisions.
        The analysis is done separately for each load combination: the
        bit-width-independent tables are loaded together with either the
        32-bit or the 64-bit tables, but the 32-bit and 64-bit tables are
        never loaded together (and entries in the bit-width-specific tables
        do not necessarily carry a bit-width marker in their condition
        strings, so the conditions alone cannot be relied upon to keep them
        apart).
        """
        suppress = set()
        for bits in (32, 64):
            table_list = [(key[2], builtins_by_table[key])
                          for key in table_names
                          if key[0] == self.arch and self.matches(key[2])
                          and key[1] in (0, bits)]
            suppress.update(self.return_type_ovl_collisions(table_list))
        return suppress

    def compute_entry_encodings(self):
        """
        Compute the '@' overload encoding for each RISC-V vector table
        entry (filling entry_enc).

        The overload variants provided by GCC and Clang differ (e.g., Clang
        provides an unadorned __riscv_viota overload while GCC only
        provides the policy-suffixed variants), so the encoding is computed
        per compiler covered by the entry's condition.  If the encoding is
        valid for all covered compilers, a plain '@' encoding is emitted.
        If it is valid for exactly one of them, the encoding is flagged
        with the compiler letter (e.g., '@L2');
        load_overloadable_builtin_symbols only declares the additional
        overload when emulating that compiler.  If the encodings genuinely
        differ between compilers (which a flag cannot express), the entry
        is split into per-compiler entries with separate conditions.
        """
        suppress = self.compute_suppressed_overloads()
        if suppress:
            print(f'omitting @-overload on {len(suppress)} '
                  f'RISC-V vector entries (return-type collisions)',
                  file=sys.stderr)
        nr_flagged, nr_split = 0, 0
        for table_id in table_names:
            arch, bits, header = table_id
            if arch != self.arch or not self.matches(header):
                continue
            for name, (bfk_name, builtin_sigs) in \
                    builtins_by_table[table_id].items():
                if not bfk_name.startswith('__riscv_'):
                    continue
                for cond, sig in builtin_sigs:
                    compilers = split_condition_by_compiler(cond)
                    if compilers is None:
                        # Shouldn't happen; conservatively fall back to
                        # considering the union of all compilers.
                        enc = self.get_at_encoding(header, name)
                        if enc and any((name, cond, sig, c) in suppress
                                       for c in ('gx', 'Lx', 'mx')):
                            enc = ''
                        self.entry_enc[(table_id, name, cond, sig)] = \
                            [(cond, enc)]
                        print(f'warning: cannot parse condition "{cond}"',
                              file=sys.stderr)
                        continue
                    encs = { }
                    for compiler in compilers:
                        enc = self.get_at_encoding(header, name, compiler)
                        if (enc and
                            (name, cond, sig, compiler) in suppress):
                            enc = ''
                        encs[compiler] = enc
                    distinct = set(e for e in encs.values() if e)
                    key = (table_id, name, cond, sig)
                    need_split = False
                    if not distinct:
                        self.entry_enc[key] = [(cond, '')]
                    elif len(distinct) == 1:
                        enc = next(iter(distinct))
                        with_enc = [c for c, e in encs.items() if e]
                        if len(with_enc) == len(encs):
                            # Valid for all compilers covered by the
                            # condition.
                            self.entry_enc[key] = [(cond, enc)]
                        elif len(with_enc) == 1:
                            # Valid for a single compiler: flag the encoding
                            # with the compiler letter.
                            flagged = '@' + with_enc[0][0] + enc[1:]
                            self.entry_enc[key] = [(cond, flagged)]
                            nr_flagged += 1
                        else:
                            # Valid for a proper subset of multiple
                            # compilers: a single letter cannot express
                            # that, so split.
                            need_split = True
                    else:
                        # The encodings differ between compilers; split the
                        # entry into per-compiler entries.
                        need_split = True
                    if need_split:
                        pairs = []
                        for compiler in sorted(compilers):
                            sub_cond = compilers[compiler]
                            intern_condition(sub_cond)
                            pairs.append((sub_cond, encs[compiler]))
                        self.entry_enc[key] = pairs
                        nr_split += 1
        if nr_flagged:
            print(f'compiler-flagged @-overload on {nr_flagged} '
                  f'RISC-V vector entries', file=sys.stderr)
        if nr_split:
            print(f'split {nr_split} RISC-V vector entries into '
                  f'per-compiler entries (differing @-overloads)',
                  file=sys.stderr)


class ARM_Special_Handler(Overload_Table_Handler):
    """
    Overload compression handler for the ARM special header tables.
    """
    def __init__(self, arch, headers, guard=None):
        super().__init__(arch, headers, guard)
        # Map of a non-overloaded builtin to a tuple of the qualname of the
        # overloaded builtin from which it is derived (the longest
        # overloaded '_'-separated prefix of its name whose signatures
        # include those of the builtin) and the number of trailing
        # '_'-separated components that the overload encoding strips off.
        self.variant_base = { }
        # Map of (variant qualname, signature) to the set of configurations
        # in which the signature is also a signature of the overloaded base
        # builtin.
        self.ovl_covered = { }
        # Map of (base qualname, signature) to the set of configurations
        # for which the entry of the overloaded base builtin is dropped
        # (because it is derived from a variant entry).
        self.base_dropped = { }

    def analyze_variants(self, builtins_map):
        for qualname, signatures in builtins_map.items():
            if qualname.find(':') == -1:
                continue
            qual, name = qualname.split(':')
            parts = name.split('_')
            if (not self.matches(qual) or
                (qual, tuple(parts)) in self.overloaded):
                continue
            ovl_info, prefix_len = self.find_prefixed_overload(qual, parts)
            if not ovl_info:
                continue
            base_qualname = qual + ':' + ovl_info.name
            base_signatures = builtins_map[base_qualname]
            any_covered = False
            for sig, sig_cfgs in signatures.items():
                covered = sig_cfgs.intersection(
                    base_signatures.get(sig, set()))
                if covered:
                    self.ovl_covered[(qualname, sig)] = covered
                    self.base_dropped.setdefault((base_qualname, sig),
                                                 set()).update(covered)
                    any_covered = True
            if any_covered:
                self.variant_base[qualname] = (base_qualname,
                                               len(parts) - prefix_len)
        if self.variant_base:
            print(f'overload compression applied to '
                  f'{len(self.variant_base)} ARM special-table builtins '
                  f'({len(self.overloaded)} overload sets)',
                  file=sys.stderr)

    def adjust_bfk(self, qual, name, qualname, bfk_name):
        base = self.variant_base.get(qualname)
        if base:
            # Use the builtin function kind of the overloaded builtin whose
            # overload set this variant belongs to.
            bfk_name = get_bfk_name(base[0])
        return bfk_name, False

    def get_sig_splits(self, qualname, sig, sig_cfgs):
        sig_cfgs = sig_cfgs - self.base_dropped.get((qualname, sig), set())
        if not sig_cfgs:
            # All configurations of the entry are derived from variant
            # entries: no entry is emitted for the overloaded builtin.
            return []
        splits = [(sig_cfgs, '')]
        base = self.variant_base.get(qualname)
        if base:
            covered = self.ovl_covered.get((qualname, sig),
                                           set()).intersection(sig_cfgs)
            if covered:
                # A variant builtin is split in two if its '@' overload
                # encoding is not available in all of its configurations.
                splits = [(covered, make_at_encoding(base[1]))]
                if covered != sig_cfgs:
                    splits.append((sig_cfgs - covered, ''))
        return splits


# Map of all builtins.
builtins = { }

# RISC-V vector C API tables, keyed by the `#line` / comment prefix used
# by extract-builtin-declarations (the pragma name).
RISCV_VECTOR_HEADERS = ('vector', 'andes_vector', 'sifive_vector')

# ARM "special" header tables, keyed by the qualifier of the builtins
# extracted for them.
ARM_SPECIAL_HEADERS = ('arm_acle.h', 'arm_mve.h', 'arm_neon.h',
                       'arm_neon_sve_bridge.h', 'arm_sme.h', 'arm_sve.h')

# The overload compression handlers for the builtin tables that are entered
# by load_overloadable_builtin_symbols (and whose entries can therefore use
# the '@' overload encoding).
overload_handlers = (
    RISCV_Vector_Handler('r', RISCV_VECTOR_HEADERS,
                         guard='RISCV_VECTOR_BUILTINS_ENABLED'),
    ARM_Special_Handler('a', ARM_SPECIAL_HEADERS),
)


def get_overload_handler(header):
    """
    Return the overload compression handler for the given table qualifier,
    or None if the table has no '@' overload compression.
    """
    for handler in overload_handlers:
        if handler.matches(header):
            return handler
    return None


# Map of table names.
table_names = { ('*',  0, '')           : 'common',
                ('a',  0, '')           : 'arm',
                ('a', 32, '')           : 'arm_32',
                ('a', 32, 'arm_mve.h')  : 'arm_32_mve',
                ('a', 64, '')           : 'arm_64',
                ('a', 64, 'arm_acle.h') : 'arm_64_acle',
                ('a', 64, 'arm_neon.h') : 'arm_64_neon',
                ('a', 64, 'arm_neon_sve_bridge.h')
                                        : 'arm_64_neon_sve_bridge',
                ('a', 64, 'arm_sme.h')  : 'arm_64_sme',
                ('a', 64, 'arm_sve.h')  : 'arm_64_sve',
                ('m',  0, '')           : 'x86',
                ('m', 32, '')           : 'x86_32',
                ('m', 64, '')           : 'x86_64',
                ('r',  0, '')           : 'riscv',
                ('r',  0, 'vector')     : 'riscv_vector',
                ('r',  0, 'andes_vector')
                                        : 'riscv_andes_vector',
                ('r',  0, 'sifive_vector')
                                        : 'riscv_sifive_vector',
                ('r', 32, '')           : 'riscv_32',
                ('r', 32, 'vector')     : 'riscv_32_vector',
                ('r', 32, 'andes_vector')
                                        : 'riscv_32_andes_vector',
                ('r', 32, 'sifive_vector')
                                        : 'riscv_32_sifive_vector',
                ('r', 64, '')           : 'riscv_64',
                ('r', 64, 'vector')     : 'riscv_64_vector',
                ('r', 64, 'andes_vector')
                                        : 'riscv_64_andes_vector',
                ('r', 64, 'sifive_vector')
                                        : 'riscv_64_sifive_vector' }

# Map of builtins by table.
builtins_by_table = { }
for key in table_names.keys():
    builtins_by_table[key] = { }

# The generated builtin tables in the order of the corresponding
# a_builtin_function_category enumeration values; None represents bfc_none
# (no table).  This determines the contents of the generated builtin_tables
# array (and of the associated overload set arrays).  bfc_keyword and
# bfc_user, which follow bfc_x86_64 in the enumeration, have no generated
# table and are covered by the zero-initialization of the remaining array
# elements.
category_table_names = [
    None,
    'common',
    'arm',
    'arm_32',
    'arm_32_mve',
    'arm_64',
    'arm_64_acle',
    'arm_64_neon',
    'arm_64_neon_sve_bridge',
    'arm_64_sme',
    'arm_64_sve',
    'riscv',
    'riscv_vector',
    'riscv_andes_vector',
    'riscv_sifive_vector',
    'riscv_32',
    'riscv_32_vector',
    'riscv_32_andes_vector',
    'riscv_32_sifive_vector',
    'riscv_64',
    'riscv_64_vector',
    'riscv_64_andes_vector',
    'riscv_64_sifive_vector',
    'x86',
    'x86_32',
    'x86_64'
]

# Set of all bit-widths.
all_bits = set()

# Set of all architectures.
all_archs = set()

# Set of all (architecture, bits) tuples.
all_arch_bits = set()

# Map of compilers to a set of versions.
all_versions_by_compiler = { }

# Map of tuple of (compiler, arch) to a set of versions.
all_versions_by_compiler_arch = { }

# Map of tuple of (compiler, arch, bits) to a set of versions.
all_versions_by_compiler_arch_bits = { }

# Map of condition ids to condition strings.
all_conditions = { }

# List of all condition ids;
all_cond_ids = []

# Map of signature ids to signature strings.
all_signatures = { }

# List of all signature ids.
all_sig_ids = []

# Set of all builtin function kinds.
all_bfks = set()


for arg in sys.argv[1:]:
    mo = FILE_NAME_RE.match(arg)
    if mo is None:
        print(f'File name "{arg}" does not match expected format',
              file=sys.stderr)
        sys.exit(1)

    compiler, version_str, arch, bits_str = mo.group('compiler', 'version',
                                                     'arch', 'bits')
    version, bits = int(version_str), int(bits_str)

    cfg = Config(compiler, bits, arch, version)
    all_bits.add(cfg.bits)
    all_archs.add(cfg.arch)
    all_arch_bits.add((cfg.arch, cfg.bits))
    all_versions_by_compiler.setdefault(cfg.compiler, set()).add(cfg.version)
    all_versions_by_compiler_arch.setdefault((cfg.compiler, cfg.arch),
                                             set()).add(cfg.version)
    all_versions_by_compiler_arch_bits.setdefault((cfg.compiler, cfg.arch,
                                                   cfg.bits),
                                                  set()).add(cfg.version)

    print('reading', arg, file=sys.stderr)
    with open(arg) as fd:
        for l in fd:
            qualname, decl = l[3:-1].rstrip(';').split(' */ ', 1)
            name = qualname.split(':')[-1]
            namebeg = decl.index(name)
            nameend = namebeg + len(name)
            if decl[namebeg - 1] == '(':
                namebeg -= 1
            if decl[nameend] == ')':
                nameend += 1

            name, qualname = name.strip(), qualname.strip()
            if (name.startswith('operator ') or name == '__integer_pack' or
                name == '__builtin_unreachable trap' or
                (name == qualname and not name.startswith('_')) or
                decl.find('<unnamed-float:') != -1 or
                decl.find(' not supported ') != -1):
                # Ignore some entries that can't be processed.
                continue

            # Extract normalized return type, parameter list, and qualifiers.
            ret = normalize_type(decl[:namebeg].strip())
            params, qualifiers = split_params_and_qualifiers(decl[nameend:])

            if (name.startswith('__sync_') and
                ((name.endswith('_1') and params[0] == 'volatile char*') or
                 (name.endswith('_2') and params[0] == 'volatile short*') or
                 (name.endswith('_4') and params[0] == 'volatile int*') or
                 (name.endswith('_8') and
                  params[0] == 'volatile long long*') or
                 (name.endswith('_16') and
                  params[0] == 'volatile __int128_t*'))):
                # These builtins have an additional first parameter.
                params = tuple(['volatile void*'] + list(params[1:]))

            # Use explicit parameters where they are not provided by Clang.
            if (name == '__atomic_compare_exchange' and ret == 'void' and
                params == ('...',)):
                ret = '__edg_bool_type__'
                params = ('__edg_size_type__', 'volatile void*', 'void*',
                          'void*', '__edg_bool_type__', 'int', 'int')
            elif (name == '__atomic_exchange' and ret == 'void' and
                  params == ('...',)):
                ret = 'void'
                params = ('__edg_size_type__', 'volatile void*', 'void*',
                          'void*', 'int')
            elif (name == '__atomic_load' and ret == 'void' and
                  params == ('...',)):
                ret = 'void'
                params = ('__edg_size_type__', 'const volatile void*',
                          'void*', 'int')
            elif (name == '__atomic_store' and ret == 'void' and
                  params == ('...',)):
                ret = 'void'
                params = ('__edg_size_type__', 'volatile void*', 'void*',
                          'int')
            elif (name == '__atomic_compare_exchange' and
                  ret == '__edg_bool_type__' and len(params) == 6):
                params = tuple(list(params)[:4] + ['__edg_bool_type__'] +
                               list(params)[4:])
            elif (qualname.split(':')[0] in RISCV_VECTOR_HEADERS and
                  qualname.split(':')[-1].startswith('__riscv_') and
                  ((params[-1:] == ('unsigned',) and bits == 32) or
                   (params[-1:] == ('unsigned long',) and bits == 64))):
                # The last parameter (vl) is a size_t.  A size_t preceding it
                # is mapped during the signature consolidation below
                params = tuple(list(params)[:-1] + ['__edg_size_type__'])
            elif name in ('__divhc3', '__mulhc3'):
                if (compiler == 'gx' and version >= 160000 and
                    ret == '__fp16 __complex__'):
                    # Ignore __fp16 overload
                    continue

            sig = Signature(tuple([ret] + list(params)), qualifiers)
            builtins.setdefault(qualname, {}).setdefault(sig, set()).add(cfg)


# Attempt to consolidate signatures.
for name, signatures in builtins.items():
    if len(signatures) > 1:
        classification = []
        cls_bitset = set()
        new_signatures = {}

        for sig, cfgs in signatures.items():
            bitset = get_bitset_for_configs(cfgs)

            if len(bitset) == 2:
                new_signatures[sig] = cfgs
            else:
                cls_bitset.update(bitset)
                for idx, ty in enumerate(sig.types):
                    cls = classify_type(ty, bitset)
                    if len(classification) > idx:
                        classification[idx].add(cls)
                    else:
                        classification.append({cls})

        for sig, sig_cfgs in signatures.items():
            if sig not in new_signatures:
                new_types = list(sig.types)

                if len(cls_bitset) > 1:
                    for idx, cls in enumerate(classification):
                        if idx >= len(new_types):
                            break

                        if len(cls) == 1 and None not in cls:
                            new_types[idx] = list(cls)[0]

                new_sig = Signature(tuple(new_types), sig.qualifiers)
                new_signatures.setdefault(new_sig, set()).update(sig_cfgs)

        signatures = new_signatures
        builtins[name] = signatures


# Merge secondary signatures into primary declaration.
remove_builtins = []
for name, signatures in builtins.items():
    secondary_name = get_bfk_name(name)
    if name != secondary_name and secondary_name in builtins:
        secondary_signatures = builtins[secondary_name]

        for sig, sig_cfgs in signatures.items():
            sec_sig = sig
            if sec_sig in secondary_signatures:
                sec_cfgs = secondary_signatures[sec_sig]
                remove_cfgs = set()

                for cfg in sec_cfgs:
                    if cfg in sig_cfgs:
                        sig_cfgs.add(cfg.get_secondary())
                        remove_cfgs.add(cfg)

                sig_cfgs.difference_update(remove_cfgs)
                sec_cfgs.difference_update(remove_cfgs)
                if not sec_cfgs:
                    del secondary_signatures[sec_sig]

        if not secondary_signatures:
            # Delay removing of secondary name from builtins.
            remove_builtins.append(secondary_name)

for name in remove_builtins:
    del builtins[name]


# Extract overload information for the builtin tables that use '@'
# overload compression and analyze the variant builtins from which the
# overload sets are derived.
for handler in overload_handlers:
    handler.extract_overloads(builtins)
    handler.analyze_variants(builtins)


# Extract all conditions and declarations from the builtins.
for qualname, signatures in sorted(builtins.items()):
    if qualname.find(':') != -1:
        qual, name = qualname.split(':')
    else:
        qual, name = '', qualname
    bfk_name = get_bfk_name(name)
    handler = get_overload_handler(qual)

    if handler:
        bfk_name, skip = handler.adjust_bfk(qual, name, qualname, bfk_name)
        if skip:
            # Ignore overloaded builtins, these will be added from the
            # variant builtins.
            continue

    for sig, sig_cfgs in signatures.items():
        # Normally a signature is emitted as a single entry; it may be
        # split in two if its '@' overload encoding is not available in all
        # of its configurations.
        splits = (handler.get_sig_splits(qualname, sig, sig_cfgs)
                  if handler else [(sig_cfgs, '')])

        for split_cfgs, enc_ovl in splits:
            is_cpp = has_reference_type(sig.types)
            archset = get_archset_for_configs(split_cfgs)
            restrictions = get_restrictions(sig.types)

            for arch in archset:
                table_id = (arch, 0, qual)

                if arch != '*':
                    cfgs = set(filter_by_arch(split_cfgs, arch))
                    bitset = get_bitset_for_configs(split_cfgs)
                    if len(bitset) == 1:
                        table_id = (arch, bitset.pop(), qual)
                else:
                    cfgs = split_cfgs

                cond = generate_condition(cfgs, arch, restrictions)
                if is_cpp:
                    # A C++ specific signature needs to be marked as such in
                    # the condition, and a C-only signature needs to be added
                    # by replacing references with pointers.
                    cond = cond.replace('Lx', 'L+') \
                               .replace('gx', 'g+') \
                               .replace('mx', 'm+')
                    c_cond = cond.replace('+', 'c')
                    c_sig = Signature(tuple(map(lambda p: p.replace('&', '*'),
                                                sig.types)),
                                      sig.qualifiers)
                else:
                    c_cond, c_sig = None, None

                if cond not in all_conditions:
                    cond_id = generate_id(cond)
                    all_cond_ids.append((cond_id, cond))
                    all_conditions[cond] = cond_id

                if sig not in all_signatures:
                    sig_str = str(sig)
                    sig_id = generate_id(sig_str)
                    all_sig_ids.append((get_signature_partition(sig_str),
                                        sig_id, sig_str))
                    all_signatures[sig] = sig_id

                if c_cond:
                    if c_cond not in all_conditions:
                        cond_id = generate_id(c_cond)
                        all_cond_ids.append((cond_id, c_cond))
                        all_conditions[c_cond] = cond_id

                    if c_sig not in all_signatures:
                        c_sig_str = str(c_sig)
                        c_sig_id = generate_id(c_sig_str)
                        all_sig_ids.append(
                            (get_signature_partition(c_sig_str), c_sig_id,
                             c_sig_str))
                        all_signatures[c_sig] = c_sig_id

                _, builtin_sigs = \
                    builtins_by_table[table_id].setdefault(name,
                                                           (bfk_name, set()))
                builtin_sigs.add((cond, sig))
                if c_cond:
                    builtin_sigs.add((c_cond, c_sig))

                if enc_ovl:
                    handler.entry_enc[(table_id, name, cond, sig)] = \
                        [(cond, enc_ovl)]

    all_bfks.add(bfk_name)


# Compute the '@' overload encodings for the table entries.  This must be
# done before the conditions enumeration is emitted because entries may be
# split into per-compiler entries with new condition strings.
for handler in overload_handlers:
    handler.compute_entry_encodings()


proj_copyright = r'''
/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/
'''.lstrip()

defs_fd = open('builtin_defs.h', 'w')
print(proj_copyright, file=defs_fd)
print(R'''/*

builtin_defs.h -- Declarations related to builtin declarations

*/

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* Do not add builtins here.  See sys_predef.h to add user-defined builtins. */
/*lint -e641 */ /* Suppress lint messages about converting enums to int. */
/*------------------Beginning of automatically generated code---------------*/

/* An enumeration used to map a builtin to a particular type signature.
The first 10 digits of the md5sum of the type signature are used. */
enum a_builtin_function_type_index {''', file=defs_fd)

PARTITION_DESCR = [
    'General builtins.',
    'ARM scalable vector builtins.',
    'RISC-V vector builtins.',
]
PARTITION_CONDITION = [
    None, None, 'RISCV_VECTOR_BUILTINS_ENABLED'
]

all_sig_ids.sort()
prev_part, active_condition = None, None
for sig_part, sig_id, sig_str in all_sig_ids:
    if sig_part != prev_part:
        if active_condition:
            print(f'#endif /* {active_condition} */', file=defs_fd)
        active_condition = PARTITION_CONDITION[sig_part]
        if active_condition:
            print(f'#if {active_condition}', file=defs_fd)
        print(f'  /* {PARTITION_DESCR[sig_part]} */', file=defs_fd)
        prev_part = sig_part
    print(f'  bfti_{sig_id},', file=defs_fd)

if active_condition:
    print(f'#endif /* {active_condition} */', file=defs_fd)
print('''  bfti_last /* final entry */
};

static constexpr a_builtin_type_string
                builtin_type_strings[] = {''',
file=defs_fd)

prev_part, active_condition = None, None
for sig_part, sig_id, sig_str in all_sig_ids:
    if sig_part != prev_part:
        if active_condition:
            print(f'#endif /* {active_condition} */', file=defs_fd)
        active_condition = PARTITION_CONDITION[sig_part]
        if active_condition:
            print(f'#if {active_condition}', file=defs_fd)
        print(f'  /* {PARTITION_DESCR[sig_part]} */', file=defs_fd)
        prev_part = sig_part
    print(f'  /* bfti_{sig_id} */ "{sig_str}",', file=defs_fd)

if active_condition:
    print(f'#endif /* {active_condition} */', file=defs_fd)
print('''};

/* An enumeration used to map a builtin to a particular condition string.
The first 10 digits of the md5sum of the condition string are used. */
enum a_builtin_function_condition_index {''', file=defs_fd)

all_cond_ids.sort()
for cond_id, cond_str in all_cond_ids:
    print(f'  bfci_{cond_id},', file=defs_fd)

print('''  bfci_last /* final entry */
};

static constexpr a_builtin_condition_string
                builtin_condition_strings[] = {''',
file=defs_fd)

for cond_id, cond_str in all_cond_ids:
    print(f'  /* bfci_{cond_id} */ "{cond_str}",', file=defs_fd)

print('};\n', file=defs_fd)

for table_id, builtins in sorted(builtins_by_table.items()):
    table_name = f'builtin_{table_names[table_id]}_table'
    arch, bits, header = table_id
    handler = get_overload_handler(header)
    table_descr = { '*' : 'common', 'a' : 'ARM', 'm' : 'x86',
                    'r' : 'riscv' }[arch]
    if bits != 0:
        table_descr += f' {bits}-bit'
    if header:
        table_descr += f' ({header})'
    print(
f'''/* Entries for automatically-generated {table_descr} builtin functions. */
static constexpr a_builtin_descr
                {table_name}[] = {{''', file=defs_fd)

    if handler and handler.guard:
        print(f'#if {handler.guard}', file=defs_fd)

    # Build the rows of the table.
    rows = []
    prev_name = None
    for name, builtin_bfk_sigs in sorted(builtins.items()):
        enc_name = encode_builtin_name(name, prev_name) if header else name
        prev_name = name

        entries = []
        bfk_name, builtin_sigs = builtin_bfk_sigs

        for cond, sig in builtin_sigs:
            cond_id, sig_id = all_conditions[cond], all_signatures[sig]
            entries.append((cond_id, sig_id, cond, sig))

        emitted = set()
        for cond_id, sig_id, cond, sig in sorted(entries):
            out_entries = ([(cond, '')] if handler is None else
                           handler.entry_enc.get((table_id, name, cond, sig),
                                                 [(cond, '')]))

            for out_cond, enc_ovl in out_entries:
                out_cond_id = all_conditions[out_cond]
                if (out_cond_id, sig_id, enc_ovl) in emitted:
                    # Splitting can produce duplicates of other entries.
                    continue
                emitted.add((out_cond_id, sig_id, enc_ovl))
                rows.append((f'{enc_name}{enc_ovl}', out_cond_id, sig_id,
                             bfk_name))
                if header:
                    enc_name = '#0'

    for enc_name, cond_id, sig_id, bfk_name in rows:
        print(f'  {{ "{enc_name}", bfci_{cond_id}, ' +
              f'bfti_{sig_id}, bfk_{bfk_name} }},', file=defs_fd)

    if handler and handler.guard:
        print(f'#endif /* {handler.guard} */', file=defs_fd)
    print('''  { NULL, 0, 0, bfk_none }   /* end of table marker */
};
''', file=defs_fd)

table_map_lines = []
for name in category_table_names:
    if name is None:
        table_map_lines.append('  NULL')
    else:
        table_map_lines.append(f'  builtin_{name}_table')

print(R'''
/*
Array to map a builtin function category to the corresponding system builtin
table (NULL for non-system builtins).
*/
static constexpr const a_builtin_descr*
                builtin_tables[bfc_last] = {
''' + ',\n'.join(table_map_lines) + R'''
};

/*--------------------End of automatically generated code-----------------*/
/*lint +e641 */ /* Re-enable lint messages about converting enums to int. */

static constexpr size_t
                num_builtin_condition_entries =
                        /* Number of entries in builtin_condition_strings. */
        sizeof(builtin_condition_strings)/sizeof(builtin_condition_strings[0]);

static constexpr size_t
                num_builtin_type_entries =
                        /* Number of entries in builtin_type_strings. */
                  sizeof(builtin_type_strings)/sizeof(builtin_type_strings[0]);

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE
''', file=defs_fd)

defs_fd.close()

kinds_fd = open('builtin_kinds.h', 'w')
print(proj_copyright, file=kinds_fd)
print(R'''/*

builtin_kinds.h -- Enumeration of builtin function kinds

*/

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/* Enumeration for each automatically-generated builtin function. */
enum a_builtin_function_kind_tag {
  bfk_none = 0, /* flag meaning the routine is not a builtin */''',
  file=kinds_fd)

for name in sorted(all_bfks):
    print(f'  bfk_{name},', file=kinds_fd)

print('''  bfk_last      /* last entry */
};

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE
''', file=kinds_fd)

kinds_fd.close()
