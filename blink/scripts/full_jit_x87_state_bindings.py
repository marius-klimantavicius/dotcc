"""R133 copy-only canonical-x87 producer adapter. ProducerProtocol1.

Run AFTER full_jit_copy_bindings and BEFORE full_jit_bindings (clones).
Never apply to already cloned bound units. This module does no I/O/emission.
Ordinary base builds have no callback references outside preprocessor guards.
All guest accesses stay in authored CopyFromUserRead/CopyToUserWrite boundaries.
"""
from __future__ import annotations
import hashlib
import re
from collections import Counter

INPUTS = ('scripts/full_jit_x87_state_bindings.py',)
MARKER = '/* R133 canonical x87 state transport v1. */'
CLONES = '/* Full-JIT operation-specific semantic clones. */'
DECLARATIONS = r'''
/* R133 canonical x87 state transport v1. */
#include "host-x87-state.h"
'''

BOUNDARY_SIGNATURES = (
    ('blink_host_guest_x87_active', 'int', ['struct Machine *', 'unsigned int']),
    ('blink_host_guest_x87_state', 'int', ['struct Machine *', 'unsigned int', 'int', 'void *', 'unsigned int']),
    ('blink_host_guest_x87_reject_legacy', 'void', ['struct Machine *', 'unsigned int']),
)


def once(source, before, after):
    if source.count(before) != 1:
        raise ValueError('R133 anchor differs: ' + before[:100])
    return source.replace(before, after, 1)


def function(source, name):
    """Exact lexical body, without matching calls, comments, strings or clones."""
    # Mask comments/string contents without changing offsets; selected SSE
    # wrappers also have one-line definitions. Never count their quoted braces.
    tokens = re.compile(r'/\*.*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.S)
    clean = tokens.sub(lambda m: ''.join('\n' if c == '\n' else ' ' for c in m[0]), source)
    pattern = re.compile(r'^[\w *]+\b' + re.escape(name) + r'\([^;{}]*\)\s*\{', re.M)
    matches = list(pattern.finditer(clean))
    if len(matches) != 1:
        raise ValueError('R133 function inventory differs: ' + name)
    a = matches[0].start()
    depth = 1
    for end in range(matches[0].end(), len(clean)):
        if clean[end] == '{': depth += 1
        elif clean[end] == '}': depth -= 1
        if depth == 0: return source[a:end + 1]
    raise ValueError('R133 function boundary missing: ' + name)


# Filled from exact native9 stage20261029-082902-2fa6b918 preclone originals.
# Hashes are immutable source identities, not a guessed shape whitelist.
PINS = {'blink/signal.c': {'DeliverSignal': 'c91a39b3691ef60677751af296ed62377e47eceb03d9aab8155de6a8b5bd53e8',
                    'SigRestore': '2bd42410c51c4a4f8a88c3aa01fd0a3dc274b877005e61bedec112ea99d8b4c5'},
 'blink/machine.c': {'OpFxsave': 'dc9e67e924829c267927ad34839a3dce6885c4f959c4500a47c66fd4112d4f9b',
                     'OpFxrstor': 'bd680b155ad766bd793ff85b125fae63b3d330ed11187aa691f7d81ade2766c9',
                     'OpEmms': 'dd17f3e5e17c6b29a6d6e1b1e4ce06c784022b446057130302d80373b258a8d5'},
 'blink/fpu.c': {'SetFpuEnv': '487474d0858867a16a4d0b8a87ccb5d0bd20b6c927d188be3ceebcb91cc27ed9',
                 'GetFpuEnv': 'b9ba7e8f2160dba314ef2d161d3d21c6fba354a908e8e02df0868c9dcf8d282c',
                 'OpFstenv': 'eacb9f6bb9b738042b6efb76a69d3a94daabc7bdead752253b4c55c94cda9ff7',
                 'OpFldenv': '0a20902d4680a79c10a21dcf4b22de5ffce1cb327270b63322d92c13917789d7',
                 'OpFsave': '512559442a1eb285d25255dfbfd005aae748b7805b432a19a1323c241021eb19',
                 'OpFrstor': 'fc24676f93650663d806b1b6d9b29bde3064520af09f369ebaa1cc185b645fc0',
                 'OpFinit': 'ac4597f0fb0548721d3762a47426a35e41d86b7f26300d30616755a20318b73f',
                 'FpuPush': '35d1ca995d4eac5a1dc9f0355718e4558942badcfe2cabbe79811baea5aef0ee',
                 'FpuPop': '9df2584754d40f090dfa65828e2ea340389e9cc8ea6a5d008b1a05d3f25b3bf6',
                 'OpFpu': '07d387245d8c306b656cac3092e20294052695344d1cf6cb47fb71cb321e4487'},
 'blink/reset.c': {'ResetFpu': '9cb321c9c1e768b17f3bb19cb5da2da41ec0f9c20ebd3fb90a4bb66056bb74d0'},
 'blink/cvt.c': {'OpVpsQpiCvtpi2ps': '67c132ff92beb3c483a292471ec25cb2578d6ec0e287c74904b9bb0cd83497db',
                 'OpVpdQpiCvtpi2pd': '9bea397f980a7ef1ae79b8bf21dbff27303b2ffe75a05367801f4ec134eeb3c0',
                 'OpPpiWpsqCvtps2pi': '709aa43ee9e07fe03114e72bf8db64682b1fa4ca3fddfa5e6d0fb4fc4cc6d75d',
                 'OpPpiWpsqCvttps2pi': 'dc3cc9d4102e774a0b39e186b30b137efd24bff87f7d02b29757a60beb20c9db',
                 'OpPpiWpdCvtpd2pi': 'd7d6ddc1d739c4087d737a90b0ac9628cccfa772ebb0149afd745c15ac96eca7',
                 'OpPpiWpdCvttpd2pi': '14485c3adeb8c386e3d197aad3b674595072a0cbd79247082fd85cfb56445b54'},
 'blink/ssemov.c': {'MovqPqQq': '29d6411eb5a76da17d0c722d824c19df119692334925ae098d70ee48fa7e31d6',
                    'MovqQqPq': '3f0b1c5500126b134752d8b70dc9e6170797ff32cbe95fe584d7f1bbcaee23e6',
                    'MovqPqEqp': '0f5659d969fd399915370f037b6f854b9fb9914880096ded8622f99060463d61',
                    'MovdPqEd': '66a4cd576e840e84e7a0159833ce64e8ca3c6e7d3f7d2307fc666792ca108bcf',
                    'MovdEdPq': '11986772acd1cf8cdc8e52d0b3f8b6b9003f7a55b760235adf175e6d7ecc4bc4',
                    'MovqEqpPq': '36b32d370e9f333f3d65cf863c4553df620bc2151fea69f28c14877e91825c21',
                    'MovntqMqPq': '2388c2563f364783ad632c644790cdce70453254291e2343a6b6660a53284de0',
                    'Movq2dqVdqNq': '62366d7cbbabc42de93c6cba4ea1da014c514f3e6240bc1a7db9c5430c4d7333',
                    'Movdq2qPqUq': 'e3b68d0f92d9c1246e6e714aea8e2b9e79fc044bda12e718ab218fc7629c7f73'},
 'blink/sse.c': {'OpPsb': 'a157b2eac9fbfefe870985863fb88d920f2fec1386f656f9438c1a5290dfe9da',
                 'Op171': 'e2705b600aedc341f6757d6b97306d8b90557b7e199e7b18e6241537a3d6b4c5',
                 'Op172': '01c02177f4fe1ad4c6291d5785a9d4e6a0df5f8417a3074a646f59565e128177',
                 'Op173': '3f6783de3d2205e5c726eda3e2f0f61d5395bb8488e1d94179ae674509bd89b8',
                 'OpSsePalignr': '63230bf852c69766b076900306acba7d8cd7934e0cdea56e379245c35dac28a0',
                 'OpSse': '428a0522d4b972cb368b4b9fd3894c4813359b8edeb8924577703d5c387b1589',
                 'OpSsePunpcklbw': '2c174c74831560074cf2003d6bae310e213b8ed95cba52e7d65ab3e790c45511',
                 'OpSsePunpcklwd': '54b37f0a08930e19c91aca4e173eeca1ed21335cf6535c2a6cff7541ab225af0',
                 'OpSsePunpckldq': 'cf67d89d04cbf5d8ab87120c3eb68941e1ee1c4f5b45d3b0da8ffb4616e6a0e6',
                 'OpSsePacksswb': 'efeb78800d5c09322b4bc8d430c950aca1ea08ae6436094ae44e2dfb260acf95',
                 'OpSsePcmpgtd': '23148365fb05c238e3ec9e80c16adae0cfd6a92fd130b561052963d22c94cfbe',
                 'OpSsePackuswb': '06e038d5cc1a7ca4f495940d8b7d40741821319e785a322366a3f8cee3d3a1b4',
                 'OpSsePunpckhbw': '04fa43ed495e3b9ef9da2682c24a388ec254599c975e568a8f058361a8dd6bd4',
                 'OpSsePunpckhwd': '50251c4ec4ae89679879c84c3d207ebcdd1f2fbbb2620e7846e298310b02b17a',
                 'OpSsePunpckhdq': '69b56fbbcba084c7b79846b88966c1d9b1d58ba68413bafda2cd09b268e927a9',
                 'OpSsePackssdw': '16bbee98639c3ea4dbb593e5d5d5dbd06a6ce9660c39021d6be514e3356c0406',
                 'OpSsePunpcklqdq': '50b84e3cb6241660eff0a21abe9950b5e8ff2cb6f0316e5d1872345c85da657f',
                 'OpSsePunpckhqdq': '1b495d5b802614ea3ee0d9f488585046480be98bedec55fb2484223ce09c1580',
                 'OpSsePcmpeqd': 'e110ac884e6cdbea0c7fe2e52a3f53e9d9e3bf2998a56e202b1a9cb7a2d43afe',
                 'OpSsePsrlwv': '73a27a63b3784a34c32dfbdce96195ad10edbf2c26d867aa0fbeb30c13458609',
                 'OpSsePsrldv': '921cf720e2f993e8f2b72e3180e04d455ad67661bf2b16643c06854228d0ed6c',
                 'OpSsePsrlqv': '94b4313a5d402cc591997ccd82bb80d01cc864ddaae5840828f945279b204888',
                 'OpSsePaddq': 'fda4331882558e6ecc6df386e1a2b0e792fc3decc235200d7995d9f70a3c2203',
                 'OpSsePsubusb': 'e188f7ae1166f2f76badf14be6cb390ad4ef800672c1f5cf47c57a816cf94670',
                 'OpSsePsubusw': '14aeee80a53d13826a62ab4b86d3840c287791c1c5c5f665bfdeab2979f99d29',
                 'OpSsePaddusb': 'f1038bec42b18cd61a20c0303a83df060e4a23749ab0f58ef6ecb7547eb9aacc',
                 'OpSsePsrawv': 'c264848a8273557c88ad4276004d60b93c2413e2aa1399cf015c6c767be37425',
                 'OpSsePsradv': 'cf3748ef174c569a5b39505932a3b7fd371689c66cadd2d1b7ad4e276e4b2025',
                 'OpSsePmulhuw': '3c0e62d8c8addc1b2d217a8512e23220f20029a8e9caf52151a1711648101503',
                 'OpSsePsubsb': '8f7c0c1a6c9e85c38a2774145b259888566c870d0c38991c1db719b09f172166',
                 'OpSsePminsw': '2f0884ecc420583afab9cfc874b681424b49fd1812723b8cce3ee54df9824860',
                 'OpSsePaddsb': 'dbe189cdee26b5755d8d974b6426d6ed243fda1f57eba3f1bdcf3fac219be6df',
                 'OpSsePmaxsw': '838ff19df6e5295bd7e77fa9577b80551f46121fed2cb7b8650ea6f6ff8bdacb',
                 'OpSsePsllwv': 'd22c3ba6701b764ba27503263f0000a9b628c62a9cfd52261b78192bd35bcbdf',
                 'OpSsePslldv': 'e9754f2a780f4587f8000a962f551487595cb3b1ad0605a13054fe4c7f7e1e29',
                 'OpSsePsllqv': '81361d1b3db954ed6773a903676eca6f91ed8f1439da06170dc3ced31f3b2904',
                 'OpSsePmuludq': '26978c076511fe2b15b24303fa37faa530ecba653ca1796775780c7eae0d82ea',
                 'OpSsePmaddwd': '806065171928ec9e2e2e1d19123a665070f1066329c04b9ddd5ea3bb7c7e63cd',
                 'OpSsePsadbw': 'e82b17ee3cbc6c6d54b9093975440967b32da339a18e3297986b58186f9c895e',
                 'OpSsePsubd': 'ecf4f7adb13c4915363a946b9a33cd233775d87a8725a7908bba30cfd8bed8fc',
                 'OpSsePsubq': '7808b1f6787fc7ad8a26a9db4682f634cdd1deff304ef0df6cb4f6aebfa23036',
                 'OpSsePaddd': 'ba396eb249ea045d83434e435a305eb723bb0a1a78441e79d6399b738963fb59',
                 'OpSsePshufb': 'd3d475ff53ae29cd3199ab485e005e2b0a68f1cde9b5c76b39491983145976eb',
                 'OpSsePhaddw': '919a2368a3475b0bccb82f685466c89e5c8c4642310fe94c45e0472e49ed45cc',
                 'OpSsePhaddd': 'd331897faf3887fc4bc24f5886df7c6424c084d3e63388ebb3de6be6dd61c6b4',
                 'OpSsePmaddubsw': '774d63bf014fd37a26858559c62435801f45874819f35b8e1676d5690ade4906',
                 'OpSsePhsubw': '80d82463fd2fb890ccd5d5eddbeeea22c4a3c988928538ef07e00697fce61640',
                 'OpSsePhsubd': 'a68d4493a8fc1c1182bb0adf8d3bb7061f205fb73a619892ed7aaa53d7068974',
                 'OpSsePsignb': 'a34d9bfebb8727fa2f5bbb7960dbe868e8561c9f5a3fb84fdfb5527b23989f53',
                 'OpSsePsignw': '2ae5de2bbd7b4ca7ebd3b81aafa8e1c79045177b5c5cbdb862c3e3416553ceb7',
                 'OpSsePsignd': '17e070a5bdf2a08f14b9c7f74a089cec0ea9ec27e3085058445c217b562261cb',
                 'OpSsePmulhrsw': '060ad0424c2f80c8c29c0063d0d51be1e9f952dcb13a1970970bc55dd108ac5a',
                 'OpSsePabsw': 'fa2d29693f8f275881b63a3910abd61639641afb4dab31efdc2b4d86541f3947',
                 'OpSsePabsd': '1467822238c701cb6290646c267134b643fe2f238ca85986038b336824550edd',
                 'OpSsePmulld': '998e6c1a99ca3963141cc0811aa340b2059d8f9cf16d9b2ff6a731e1397fa1f4'},
 'blink/modrm.c': {'GetModrmRegisterMmPointerRead': 'c1b33a4b5ebcc6daaa1afd537dccf9f0dab3a9dda29895145781612a01a7f28e',
                   'GetModrmRegisterMmPointerRead8': 'e07ab35a08fc293370b6b8e46ca45dc00f0cae78c2b42f35cffb5e44c708a966',
                   'GetModrmRegisterMmPointerWrite': '271c0dcf2b2dc33028650b74bbd843ec6a79b81758336967124e3591acbe8ce7',
                   'GetModrmRegisterMmPointerWrite8': '17949edaa98be3c3ab870665959df44827f1a883625213c0bee5d7d5e9b3a427',
                   'GetMmxAddress': 'b452107f2a04172eef44f32db730e22a67ebd71624cd79b5ccebe5b2acd97907'}}


LEGACY_GUARDS = {'blink/cvt.c': ['OpVpsQpiCvtpi2ps',
                 'OpVpdQpiCvtpi2pd',
                 'OpPpiWpsqCvtps2pi',
                 'OpPpiWpsqCvttps2pi',
                 'OpPpiWpdCvtpd2pi',
                 'OpPpiWpdCvttpd2pi'],
 'blink/ssemov.c': ['MovqPqQq',
                    'MovqQqPq',
                    'MovqPqEqp',
                    'MovdPqEd',
                    'MovdEdPq',
                    'MovqEqpPq',
                    'MovntqMqPq',
                    'Movq2dqVdqNq',
                    'Movdq2qPqUq'],
 'blink/sse.c': ['OpPsb',
                 'Op171',
                 'Op172',
                 'Op173',
                 'OpSsePalignr',
                 'OpSse',
                 'OpSsePunpcklbw',
                 'OpSsePunpcklwd',
                 'OpSsePunpckldq',
                 'OpSsePacksswb',
                 'OpSsePcmpgtd',
                 'OpSsePackuswb',
                 'OpSsePunpckhbw',
                 'OpSsePunpckhwd',
                 'OpSsePunpckhdq',
                 'OpSsePackssdw',
                 'OpSsePunpcklqdq',
                 'OpSsePunpckhqdq',
                 'OpSsePcmpeqd',
                 'OpSsePsrlwv',
                 'OpSsePsrldv',
                 'OpSsePsrlqv',
                 'OpSsePaddq',
                 'OpSsePsubusb',
                 'OpSsePsubusw',
                 'OpSsePaddusb',
                 'OpSsePsrawv',
                 'OpSsePsradv',
                 'OpSsePmulhuw',
                 'OpSsePsubsb',
                 'OpSsePminsw',
                 'OpSsePaddsb',
                 'OpSsePmaxsw',
                 'OpSsePsllwv',
                 'OpSsePslldv',
                 'OpSsePsllqv',
                 'OpSsePmuludq',
                 'OpSsePmaddwd',
                 'OpSsePsadbw',
                 'OpSsePsubd',
                 'OpSsePsubq',
                 'OpSsePaddd',
                 'OpSsePshufb',
                 'OpSsePhaddw',
                 'OpSsePhaddd',
                 'OpSsePmaddubsw',
                 'OpSsePhsubw',
                 'OpSsePhsubd',
                 'OpSsePsignb',
                 'OpSsePsignw',
                 'OpSsePsignd',
                 'OpSsePmulhrsw',
                 'OpSsePabsw',
                 'OpSsePabsd',
                 'OpSsePmulld'],
 'blink/modrm.c': ['GetModrmRegisterMmPointerRead',
                   'GetModrmRegisterMmPointerRead8',
                   'GetModrmRegisterMmPointerWrite',
                   'GetModrmRegisterMmPointerWrite8',
                   'GetMmxAddress']}

LEGACY_CALLS = frozenset(('FpuSt', 'FpuPush', 'FpuPop', 'MmReg', 'MmRm',
                          'GetMmxAddress'))
# These address independent XMM storage. Count them in the exact reviewed-unit
# inventory as before, but do not reject unrelated pure-XMM units for using them.
REVIEWED_XMM_CALLS = frozenset(('XmmRexrReg', 'XmmRexbRm'))


def legacy_accesses(source):
    """Shared conservative lexical inventory, not a C alias/preprocessor proof.

    C line splicing precedes comment removal. Replace comments and literals
    with whitespace, so quoted/commented tokens cannot hide real accesses or
    merge neighboring identifiers. Compare token spellings, not formatting.
    Pinned function hashes still authenticate the exact original source bytes.
    """
    source = re.sub(r'\\\r?\n', '', source)
    noncode = re.compile(r'/\*.*?\*/|//[^\n]*|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.S)
    clean = noncode.sub(' ', source)
    tokens = re.findall(r'[A-Za-z_][A-Za-z_0-9]*|->|[^\s]', clean)
    accesses = Counter()
    for i, token in enumerate(tokens):
        if (token == 'fpu' and tokens[i + 1:i + 2] in (['.'], ['->'])
                and tokens[i + 2:i + 3] == ['st']):
            accesses['fpu.st'] += 1
        elif token in LEGACY_CALLS | REVIEWED_XMM_CALLS and tokens[i + 1:i + 2] == ['(']:
            accesses[token + '('] += 1
    return dict(accesses)


LEGACY_ACCESS_COUNTS = {'blink/signal.c': {'fpu.st': 2},
 'blink/machine.c': {'fpu.st': 2},
 'blink/fpu.c': {'FpuSt(': 9, 'FpuPop(': 20, 'FpuPush(': 12},
 'blink/reset.c': {'fpu.st': 1},
 'blink/cvt.c': {'XmmRexrReg(': 32, 'MmReg(': 8},
 'blink/ssemov.c': {'XmmRexrReg(': 42, 'MmReg(': 10, 'XmmRexbRm(': 9, 'MmRm(': 1},
 'blink/sse.c': {'XmmRexbRm(': 2, 'XmmRexrReg(': 4, 'GetMmxAddress(': 1},
 'blink/modrm.c': {'MmRm(': 2, 'XmmRexbRm(': 3, 'GetMmxAddress(': 1}}

def checked(source, path):
    # Extra legacy accesses appended outside the named function anchors must
    # not silently become new active consumers. Exact reviewed token inventory
    # supplements (and does not replace) function-body authentication.
    if legacy_accesses(source) != LEGACY_ACCESS_COUNTS[path]:
        raise ValueError('R133 legacy state access inventory differs: ' + path)
    for name, expected in PINS[path].items():
        if hashlib.sha256(function(source, name).encode()).hexdigest() != expected:
            raise ValueError('R133 function fingerprint differs: ' + path + ':' + name)


def guarded_original(original, replacement):
    return '#ifdef BLINK_MANAGED_GUEST_THREADS\n' + replacement + '\n#else\n' + original + '\n#endif'


def signal(source):
    source = once(source, '  struct SignalFrame sf;\n  // when the guest returns',
                  '  struct SignalFrame sf;\n'
                  '#ifdef BLINK_MANAGED_GUEST_THREADS\n'
                  '  int r133_active = blink_host_guest_x87_active(m, 1);\n'
                  '#endif\n  // when the guest returns')
    begin = '  Write32(sf.fp.mxcsr, m->mxcsr);'
    end = '  memcpy(sf.fp.xmm, m->xmm, sizeof(sf.fp.xmm));'
    a = source.index(begin); b = source.index(end, a) + len(end)
    original = source[a:b]
    source = source[:a] + guarded_original(original, '''  if (!blink_host_guest_x87_state(m, 1, 1, &sf.fp, sizeof(sf.fp))) {
''' + original + '\n  }') + source[b:]
    # Inactive SigRestore retains its original frame-copy behavior exactly.
    # Active restoration must read the actual uc.fpstate pointer, not assume
    # the adjacent inline fp image. Validate before ANY architectural mutation.
    anchor = '  unassert(!CopyFromUserRead(m, &sf, Read64(m->sp) - 8, sizeof(sf)));'
    source = once(source, anchor, guarded_original(anchor, '''  if (r133_active) {
    u64 fp;
    /* R139: only the context/header prefix is required here. The actual
     * fpstate pointer may be null or external to this frame's inline image. */
    _Static_assert(offsetof(struct SignalFrame, uc) == 136, "signal uc offset");
    _Static_assert(sizeof(sf.uc) == 304, "signal uc size");
    _Static_assert(offsetof(struct SignalFrame, fp) == 440, "signal fp offset");
    _Static_assert(sizeof(sf.fp) == 512, "signal fp size");
    _Static_assert(sizeof(sf) == 952, "signal frame size");
    _Static_assert(offsetof(struct SignalFrame, uc) + sizeof(sf.uc) ==
                   offsetof(struct SignalFrame, fp), "signal context prefix");
    memset(&sf, 0, sizeof(sf));
    if (CopyFromUserRead(m, &sf, Read64(m->sp) - 8,
                         offsetof(struct SignalFrame, fp)) == -1)
      TerminateSignal(m, SIGSEGV_LINUX, m->segvcode);
    fp = Read64(sf.uc.fpstate);
    if (fp) {
      if (CopyFromUserRead(m, &sf.fp, fp, sizeof(sf.fp)) == -1 ||
          blink_host_guest_x87_state(m, 1, 2, &sf.fp, sizeof(sf.fp)) != 1)
        TerminateSignal(m, SIGSEGV_LINUX, SI_KERNEL_LINUX);
    }
  } else {
''' + anchor + '\n  }'))
    # Restore SP stays unconditional, outside the legacy-FP block.
    begin = '  m->mxcsr = Read32(sf.fp.mxcsr);'
    end = '  memcpy(m->xmm, sf.fp.xmm, sizeof(sf.fp.xmm));'
    a = source.index(begin); b = source.index(end, a) + len(end)
    original = source[a:b]
    replacement = '''  if (r133_active) {
    if (Read64(sf.uc.fpstate)) {
      if (blink_host_guest_x87_state(m, 1, 3, &sf.fp, sizeof(sf.fp)) != 1)
        TerminateSignal(m, SIGSEGV_LINUX, SI_KERNEL_LINUX);
    } else {
      blink_host_guest_x87_state(m, 1, 4, 0, 0);
    }
    memcpy(m->sp, sf.uc.rsp, 8);
  } else {
''' + original + '\n  }'
    source = source[:a] + guarded_original(original, replacement) + source[b:]
    source = once(source, '  // finally, call the signal handler using the sigaction arguments', '''#ifdef BLINK_MANAGED_GUEST_THREADS
  /* Linux handle_signal: successful frame installation precedes clearing
   * user FP/SSE state for handler entry. Same Machine and physical allocation. */
  blink_host_guest_x87_state(m, 1, 22, 0, 0);
#endif
  // finally, call the signal handler using the sigaction arguments''')
    return source


def fx(source):
    for name, save in [('OpFxsave', True), ('OpFxrstor', False)]:
        op = '(Rexw(rde) ? 5 : 7)' if save else '(Rexw(rde) ? 6 : 8)'
        lines = '''
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (blink_host_guest_x87_active(m, 1)) {
    i64 address;
    u8 image[512];
    if (m->mode.omode != XED_MODE_LONG ||
        m->mode.genmode != XED_GEN_MODE_PROTECTED)
      blink_host_guest_x87_reject_legacy(m, 1); /* other modes MISSING */
    address = ComputeAddress(A);
    if (address & 15) ThrowProtectionFault(m);
'''
        if save:
            lines += '''    blink_host_guest_x87_state(m, 1, ''' + op + ''', image, sizeof(image));
    /* 416-byte defined prefix; preserve reserved/software-owned tail. */
    SetWriteAddr(m, address, 416);
    if (CopyToUserWrite(m, address, image, 416) == -1)
      ThrowSegmentationFault(m, address);
'''
        else:
            lines += '''    SetReadAddr(m, address, 416);
    memset(image, 0, sizeof(image));
    if (CopyFromUserRead(m, image, address, 416) == -1)
      ThrowSegmentationFault(m, address);
    if (blink_host_guest_x87_state(m, 1, ''' + op + ''', image, sizeof(image)) != 1)
      ThrowProtectionFault(m);
'''
        lines += '    return;\n  }\n#endif\n'
        anchor = 'static void ' + name + '(P) {'
        source = once(source, anchor, anchor + lines)
    # No arithmetic or old MMX helper gets active state through EMMS.
    source = once(source, 'static void OpEmms(P) {', '''static void OpEmms(P) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_x87_reject_legacy(m, 1);
#endif''')
    return source


def fpu(source):
    for name, smallop, largeop, save, registers in [
        ('OpFstenv', 11, 13, True, False), ('OpFldenv', 12, 14, False, False),
        ('OpFsave', 15, 17, True, True), ('OpFrstor', 16, 18, False, True),
    ]:
        anchor = 'static void ' + name + '(struct Machine *m) {'
        prefix = 'static void ' + name + '(struct Machine *m, u64 rde, i64 disp, u64 uimm0) {'
        lines = '''
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (blink_host_guest_x87_active(m, 1)) {
    i64 address;
    unsigned bytes;
    u8 image[108];
    /* Long/protected only; operand66 selects14/94, REX.W ignored here. */
    if (m->mode.omode != XED_MODE_LONG ||
        m->mode.genmode != XED_GEN_MODE_PROTECTED)
      blink_host_guest_x87_reject_legacy(m, 1);
    address = ComputeAddress(A);
    bytes = (Osz(rde) ? 14 : 28)''' + (' + 80' if registers else '') + ';\n'
        # Protected16 restore has no FOP field. Do not invent a load/retain/clear
        # policy from a layout diagram: reject before address evaluation until
        # a primary rule and parent fixture qualify that missing form.
        if not save:
            lines = lines.replace('    address = ComputeAddress(A);',
                '    if (Osz(rde)) blink_host_guest_x87_reject_legacy(m, 1); /* MISSING protected16 restore FOP */\n'
                '    address = ComputeAddress(A);')
        op = '(Osz(rde) ? %d : %d)' % (smallop, largeop)
        if save:
            lines += '    blink_host_guest_x87_state(m, 1, ' + op + ', image, bytes);\n'
            lines += '''    SetWriteAddr(m, address, bytes);
    if (CopyToUserWrite(m, address, image, bytes) == -1)
      ThrowSegmentationFault(m, address);
'''
            lines += '    blink_host_guest_x87_state(m, 1, %d, 0, 0);\n' % (20 if registers else 21)
        else:
            lines += '''    SetReadAddr(m, address, bytes);
    if (CopyFromUserRead(m, image, address, bytes) == -1)
      ThrowSegmentationFault(m, address);
'''
            lines += '    blink_host_guest_x87_state(m, 1, ' + op + ', image, bytes);\n'
        lines += '    return;\n  }\n#endif\n'
        source = once(source, anchor, prefix + lines)
        source = once(source, name + '(m));', name + '(A));')
    source = once(source, 'void OpFinit(struct Machine *m) {', '''void OpFinit(struct Machine *m) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  if (blink_host_guest_x87_state(m, 1, 20, 0, 0)) return;
#endif''')
    metadata = '''  m->fpu.ip = MaskAddress(m->mode.omode, m->ip - Oplength(rde));
  m->fpu.op = op << 8 | ModrmMod(rde) << 6 | ModrmReg(rde) << 3 | ModrmRm(rde);
  m->fpu.dp = ismemory ? ComputeAddress(A) : 0;'''
    source = once(source, metadata, guarded_original(metadata, '''  if (blink_host_guest_x87_active(m, 1)) {
    /* Only the four explicit copy-only environment operations above may use
     * this compatibility boundary. Preserve previous FIP/FDP/FOP on entry. */
    if (!(ismemory && (op == 1 || op == 5) &&
          (ModrmReg(rde) == 4 || ModrmReg(rde) == 6)))
      blink_host_guest_x87_reject_legacy(m, 1);
  } else {
''' + metadata + '\n  }'))
    for anchor in ['void FpuPush(struct Machine *m, double x) {', 'double FpuPop(struct Machine *m) {']:
        source = once(source, anchor, anchor + '''
#ifdef BLINK_MANAGED_GUEST_THREADS
  blink_host_guest_x87_reject_legacy(m, 1);
#endif''')
    # Strict startup handshake exists only in the adapted producer.
    source += '\nint BlinkGuestX87StateProtocol(void) { return 1; }\n'
    return source


def adapt(path, content):
    if path not in PINS:
        if (path.startswith('blink/') and path.endswith('.c') and any(
                access not in {name + '(' for name in REVIEWED_XMM_CALLS}
                for access in legacy_accesses(content.decode('utf-8', errors='replace')))):
            raise ValueError('R133 unreviewed legacy state consumer: ' + path)
        return content
    source = content.decode('utf-8')
    if CLONES in source or MARKER in source or 'BlinkGuestX87StateProtocol' in source:
        raise ValueError('R133 requires fresh preclone source: ' + path)
    checked(source, path)
    if path == 'blink/signal.c': source = signal(source)
    elif path == 'blink/machine.c': source = fx(source)
    elif path == 'blink/fpu.c': source = fpu(source)
    elif path in LEGACY_GUARDS:
        for name in LEGACY_GUARDS[path]:
            body = function(source, name)
            at = body.index('{') + 1
            guarded = body[:at] + '\n#ifdef BLINK_MANAGED_GUEST_THREADS\n  blink_host_guest_x87_reject_legacy(m, 1);\n#endif\n' + body[at:]
            source = once(source, body, guarded)
    elif path == 'blink/reset.c':
        source = once(source, 'static void ResetFpu(struct Machine *m) {', '''static void ResetFpu(struct Machine *m) {
#ifdef BLINK_MANAGED_GUEST_THREADS
  /* NewMachine(NULL parent) resets fresh, unpublished storage before TLS
   * installation. Only the current Machine has a canonical sidecar. */
  if (m == g_machine && blink_host_guest_x87_state(m, 1, 19, 0, 0)) return;
#endif''')
    else: raise ValueError('R133 adapter path inventory differs')
    return (DECLARATIONS + source).encode('utf-8')
