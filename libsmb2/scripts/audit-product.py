#!/usr/bin/env python3
"""Inventory final source dependencies and publish whole-assembly-rooted NativeAOT.

Builds use private copies, leaving the generated projects and their obj/bin alone.
This qualifies compilation/dependencies, not SMB protocol correctness.
"""
import argparse
import json
from pathlib import Path
import re
import shutil
import sys as _production_sys
from pathlib import Path as _ProductionPath
_production_sys.path.insert(0, str(next(parent for parent in _ProductionPath(__file__).resolve().parents
    if (parent / "Scripts/production_support.py").is_file()) / "Scripts"))
from production_support import subprocess
import tempfile
import xml.etree.ElementTree as ET
from common import ROOT, run, sha

ALLOWED_IMPORTS = {
    'libc': {'link', 'chown', 'mkfifo', 'getrusage', 'getppid', 'kill'},
    'kernel32.dll': {'CreateHardLinkW', 'GetCurrentProcess', 'GetProcessTimes',
                     'CreateToolhelp32Snapshot', 'Process32FirstW', 'Process32NextW',
                     'CloseHandle', 'GetCurrentProcessId', 'OpenProcess', 'TerminateProcess'},
    'psapi.dll': {'GetProcessMemoryInfo'},
    'secur32.dll': {'AcquireCredentialsHandleW', 'InitializeSecurityContextW', 'QueryContextAttributesW',
                    'FreeContextBuffer', 'DeleteSecurityContext', 'FreeCredentialsHandle'},
}
# Exact current shared-runtime declarations. The sealed source/resource proof
# binds these additions; no other member, library or product role is approved.
RUNTIME_RESOURCE_PROOF = {'path': '/home/marius/p/dotcc/cpython/artifacts/owner-a-r32-20261018/v23/actual-resources.json',
 'sha256': 'baba4f18b3f892275f126a5be04a41477aff707cf772c69b1edf49118fe27f00',
 'compilerLibrarySha256': 'd7caacb16813ab9b1a39ff7745f9404e2f3d2863b674bfbedceab16b5fe17022',
 'runtimeResourceCount': 84,
 'headerResourceCount': 68}
RUNTIME_IMPORTS = {'PosixErf': {'runtimeModule': 'MathIeeeLib.cs',
              'library': 'libm.so.6',
              'symbol': 'erf',
              'declaration': '[DllImport("libm.so.6", EntryPoint = "erf", SetLastError = true)] private static '
                             'extern double PosixErf(double value);',
              'memberRole': 'private static extern member',
              'sourceSha256': 'd42bf73b2455e92c47b0468bedc7ce4a257911bae0923bc1f2ff2903a70c937e',
              'resourceName': 'DotCC.Runtime.MathIeeeLib.cs'},
 'PosixErfc': {'runtimeModule': 'MathIeeeLib.cs',
               'library': 'libm.so.6',
               'symbol': 'erfc',
               'declaration': '[DllImport("libm.so.6", EntryPoint = "erfc", SetLastError = true)] private static '
                              'extern double PosixErfc(double value);',
               'memberRole': 'private static extern member',
               'sourceSha256': 'd42bf73b2455e92c47b0468bedc7ce4a257911bae0923bc1f2ff2903a70c937e',
               'resourceName': 'DotCC.Runtime.MathIeeeLib.cs'},
 'PosixMmap': {'runtimeModule': 'MemoryMappingLib.cs',
               'library': 'libc',
               'symbol': 'mmap',
               'declaration': '[DllImport("libc", EntryPoint = "mmap", SetLastError = true)] private static '
                              'extern void* PosixMmap(void* address, ulong length, int protection, int flags, int '
                              'descriptor, long offset);',
               'memberRole': 'private static extern member',
               'sourceSha256': '230ada9ece2f7cb0f8c7edb4b8c16cab391397d93dbdfd262c4dc34a4f462ca0',
               'resourceName': 'DotCC.Runtime.MemoryMappingLib.cs'},
 'PosixMunmap': {'runtimeModule': 'MemoryMappingLib.cs',
                 'library': 'libc',
                 'symbol': 'munmap',
                 'declaration': '[DllImport("libc", EntryPoint = "munmap", SetLastError = true)] private static '
                                'extern int PosixMunmap(void* address, ulong length);',
                 'memberRole': 'private static extern member',
                 'sourceSha256': '230ada9ece2f7cb0f8c7edb4b8c16cab391397d93dbdfd262c4dc34a4f462ca0',
                 'resourceName': 'DotCC.Runtime.MemoryMappingLib.cs'},
 'PosixGetuid': {'runtimeModule': 'PasswdLib.cs',
                 'library': 'libc',
                 'symbol': 'getuid',
                 'declaration': '[DllImport("libc", EntryPoint = "getuid")] private static extern uint '
                                'PosixGetuid();',
                 'memberRole': 'private static extern member',
                 'sourceSha256': '8864c043aba5c426fe0ba3c87cf497a00d69e7b0be86d0eeabff31254ed46647',
                 'resourceName': 'DotCC.Runtime.PasswdLib.cs'},
 'PosixGeteuid': {'runtimeModule': 'PasswdLib.cs',
                  'library': 'libc',
                  'symbol': 'geteuid',
                  'declaration': '[DllImport("libc", EntryPoint = "geteuid")] private static extern uint '
                                 'PosixGeteuid();',
                  'memberRole': 'private static extern member',
                  'sourceSha256': '8864c043aba5c426fe0ba3c87cf497a00d69e7b0be86d0eeabff31254ed46647',
                  'resourceName': 'DotCC.Runtime.PasswdLib.cs'},
 'PosixGetgid': {'runtimeModule': 'PasswdLib.cs',
                 'library': 'libc',
                 'symbol': 'getgid',
                 'declaration': '[DllImport("libc", EntryPoint = "getgid")] private static extern uint '
                                'PosixGetgid();',
                 'memberRole': 'private static extern member',
                 'sourceSha256': '8864c043aba5c426fe0ba3c87cf497a00d69e7b0be86d0eeabff31254ed46647',
                 'resourceName': 'DotCC.Runtime.PasswdLib.cs'},
 'PosixGetegid': {'runtimeModule': 'PasswdLib.cs',
                  'library': 'libc',
                  'symbol': 'getegid',
                  'declaration': '[DllImport("libc", EntryPoint = "getegid")] private static extern uint '
                                 'PosixGetegid();',
                  'memberRole': 'private static extern member',
                  'sourceSha256': '8864c043aba5c426fe0ba3c87cf497a00d69e7b0be86d0eeabff31254ed46647',
                  'resourceName': 'DotCC.Runtime.PasswdLib.cs'},
 'PosixGetpwuidR': {'runtimeModule': 'PasswdLib.cs',
                    'library': 'libc',
                    'symbol': 'getpwuid_r',
                    'declaration': '[DllImport("libc", EntryPoint = "getpwuid_r")] private static extern int '
                                   'PosixGetpwuidR(uint uid, LinuxPasswd* record, byte* buffer, ulong length, '
                                   'LinuxPasswd** result);',
                    'memberRole': 'private static extern member',
                    'sourceSha256': '8864c043aba5c426fe0ba3c87cf497a00d69e7b0be86d0eeabff31254ed46647',
                    'resourceName': 'DotCC.Runtime.PasswdLib.cs'},
 'PosixGetpwnamR': {'runtimeModule': 'PasswdLib.cs',
                    'library': 'libc',
                    'symbol': 'getpwnam_r',
                    'declaration': '[DllImport("libc", EntryPoint = "getpwnam_r")] private static extern int '
                                   'PosixGetpwnamR(byte* name, LinuxPasswd* record, byte* buffer, ulong length, '
                                   'LinuxPasswd** result);',
                    'memberRole': 'private static extern member',
                    'sourceSha256': '8864c043aba5c426fe0ba3c87cf497a00d69e7b0be86d0eeabff31254ed46647',
                    'resourceName': 'DotCC.Runtime.PasswdLib.cs'},
 'HostOpenDirectory': {'runtimeModule': 'PosixFsLib.cs',
                       'library': 'libc',
                       'symbol': 'opendir',
                       'declaration': '[DllImport("libc", EntryPoint = "opendir", SetLastError = true)] private '
                                      'static extern nint HostOpenDirectory(byte* path);',
                       'memberRole': 'private static extern member',
                       'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                       'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'HostReadDirectory': {'runtimeModule': 'PosixFsLib.cs',
                       'library': 'libc',
                       'symbol': 'readdir',
                       'declaration': '[DllImport("libc", EntryPoint = "readdir", SetLastError = true)] private '
                                      'static extern byte* HostReadDirectory(nint directory);',
                       'memberRole': 'private static extern member',
                       'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                       'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'HostCloseDirectory': {'runtimeModule': 'PosixFsLib.cs',
                        'library': 'libc',
                        'symbol': 'closedir',
                        'declaration': '[DllImport("libc", EntryPoint = "closedir", SetLastError = true)] private '
                                       'static extern int HostCloseDirectory(nint directory);',
                        'memberRole': 'private static extern member',
                        'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                        'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'PosixOpenFile': {'runtimeModule': 'PosixFsLib.cs',
                   'library': 'libc',
                   'symbol': 'open',
                   'declaration': '[DllImport("libc", EntryPoint = "open", SetLastError = true)] private static '
                                  'extern int PosixOpenFile(byte* path, int flags, int mode);',
                   'memberRole': 'private static extern member',
                   'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                   'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'PosixSymbolicLink': {'runtimeModule': 'PosixFsLib.cs',
                       'library': 'libc',
                       'symbol': 'symlink',
                       'declaration': '[DllImport("libc", EntryPoint = "symlink", SetLastError = true)] private '
                                      'static extern int PosixSymbolicLink(byte* target, byte* linkpath);',
                       'memberRole': 'private static extern member',
                       'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                       'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'PosixReadLink': {'runtimeModule': 'PosixFsLib.cs',
                   'library': 'libc',
                   'symbol': 'readlink',
                   'declaration': '[DllImport("libc", EntryPoint = "readlink", SetLastError = true)] private '
                                  'static extern long PosixReadLink(byte* path, byte* buf, ulong bufsiz);',
                   'memberRole': 'private static extern member',
                   'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                   'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'ResizeFile': {'runtimeModule': 'PosixFsLib.cs',
                'library': 'libc',
                'symbol': 'ftruncate',
                'declaration': '[DllImport("libc", EntryPoint = "ftruncate", SetLastError = true)] static extern '
                               'int ResizeFile(SafeFileHandle handle, long size);',
                'memberRole': 'ftruncate-local static extern (compiler-lowered private static)',
                'sourceSha256': '4d9c60fa038c96667e3134cf845117758f5bf1b61907a6f707be816ca96d4e24',
                'resourceName': 'DotCC.Runtime.PosixFsLib.cs'},
 'HostStat': {'runtimeModule': 'PosixLib.cs',
              'library': 'libc',
              'symbol': 'stat',
              'declaration': '[DllImport("libc", EntryPoint = "stat", SetLastError = true)] private static extern '
                             'int HostStat(byte* path, LinuxX64Stat* metadata);',
              'memberRole': 'private static extern member',
              'sourceSha256': '474fa05fc1d921f1737e9d78bfaa7fc843929c2b6df140e2bc5d0e5f35ccde8e',
              'resourceName': 'DotCC.Runtime.PosixLib.cs'},
 'HostLStat': {'runtimeModule': 'PosixLib.cs',
               'library': 'libc',
               'symbol': 'lstat',
               'declaration': '[DllImport("libc", EntryPoint = "lstat", SetLastError = true)] private static '
                              'extern int HostLStat(byte* path, LinuxX64Stat* metadata);',
               'memberRole': 'private static extern member',
               'sourceSha256': '474fa05fc1d921f1737e9d78bfaa7fc843929c2b6df140e2bc5d0e5f35ccde8e',
               'resourceName': 'DotCC.Runtime.PosixLib.cs'},
 'HostFStat': {'runtimeModule': 'PosixLib.cs',
               'library': 'libc',
               'symbol': 'fstat',
               'declaration': '[DllImport("libc", EntryPoint = "fstat", SetLastError = true)] private static '
                              'extern int HostFStat(int descriptor, LinuxX64Stat* metadata);',
               'memberRole': 'private static extern member',
               'sourceSha256': '474fa05fc1d921f1737e9d78bfaa7fc843929c2b6df140e2bc5d0e5f35ccde8e',
               'resourceName': 'DotCC.Runtime.PosixLib.cs'},
 'PosixUtime': {'runtimeModule': 'PosixTimestampLib.cs',
                'library': 'libc',
                'symbol': 'utime',
                'declaration': '[DllImport("libc", EntryPoint = "utime", SetLastError = true)] private static '
                               'extern int PosixUtime(byte* path, void* times);',
                'memberRole': 'private static extern member',
                'sourceSha256': '3da2d24af882153d2ca9673a93dedcdef73dc24e869c336d1f1f4acf7d061307',
                'resourceName': 'DotCC.Runtime.PosixTimestampLib.cs'},
 'HostSignalAction': {'runtimeModule': 'ProcessSignalLib.cs',
                      'library': 'libc',
                      'symbol': 'sigaction',
                      'declaration': '[DllImport("libc", EntryPoint = "sigaction", SetLastError = true)] private '
                                     'static extern int HostSignalAction(int sig, LinuxSignalAction* action, '
                                     'LinuxSignalAction* previous);',
                      'memberRole': 'private static extern member',
                      'sourceSha256': 'd3c45e8358fa76ea62c7087b760b50b47302928fe4500b7dd4e6423587a186c7',
                      'resourceName': 'DotCC.Runtime.ProcessSignalLib.cs'},
 'PosixRaise': {'runtimeModule': 'ProcessSignalLib.cs',
                'library': 'libc',
                'symbol': 'raise',
                'declaration': '[DllImport("libc", EntryPoint = "raise", SetLastError = true)] private static '
                               'extern int PosixRaise(int sig);',
                'memberRole': 'private static extern member',
                'sourceSha256': 'd3c45e8358fa76ea62c7087b760b50b47302928fe4500b7dd4e6423587a186c7',
                'resourceName': 'DotCC.Runtime.ProcessSignalLib.cs'},
 'PosixProcessTimes': {'runtimeModule': 'ProcessTimesLib.cs',
                       'library': 'libc',
                       'symbol': 'times',
                       'declaration': '[DllImport("libc", EntryPoint = "times", SetLastError = true)] private '
                                      'static extern long PosixProcessTimes(void* buffer);',
                       'memberRole': 'private static extern member',
                       'sourceSha256': 'd7b12bd840f9ecb457aab2780142d93031022241c2680d42cd24730a46a392a4',
                       'resourceName': 'DotCC.Runtime.ProcessTimesLib.cs'},
 'PosixProcessClockSysconf': {'runtimeModule': 'ProcessTimesLib.cs',
                              'library': 'libc',
                              'symbol': 'sysconf',
                              'declaration': '[DllImport("libc", EntryPoint = "sysconf", SetLastError = true)] '
                                             'private static extern long PosixProcessClockSysconf(int name);',
                              'memberRole': 'private static extern member',
                              'sourceSha256': 'd7b12bd840f9ecb457aab2780142d93031022241c2680d42cd24730a46a392a4',
                              'resourceName': 'DotCC.Runtime.ProcessTimesLib.cs'},
 'PosixRecordLock': {'runtimeModule': 'RecordLockLib.cs',
                     'library': 'libc',
                     'symbol': 'fcntl',
                     'declaration': '[DllImport("libc", EntryPoint = "fcntl", SetLastError = true)] private '
                                    'static extern int PosixRecordLock(int fd, int cmd, void* arg);',
                     'memberRole': 'private static extern member',
                     'sourceSha256': '2841074dbff045fb3ad5a66856a8fd623ddab7d09a4b5c163fb065286e8f1655',
                     'resourceName': 'DotCC.Runtime.RecordLockLib.cs'},
 'PosixFlock': {'runtimeModule': 'RecordLockLib.cs',
                'library': 'libc',
                'symbol': 'flock',
                'declaration': '[DllImport("libc", EntryPoint = "flock", SetLastError = true)] private static '
                               'extern int PosixFlock(int fd, int operation);',
                'memberRole': 'private static extern member',
                'sourceSha256': '2841074dbff045fb3ad5a66856a8fd623ddab7d09a4b5c163fb065286e8f1655',
                'resourceName': 'DotCC.Runtime.RecordLockLib.cs'},
 'RecordLockSeek': {'runtimeModule': 'RecordLockLib.cs',
                    'library': 'libc',
                    'symbol': 'lseek',
                    'declaration': '[DllImport("libc", EntryPoint = "lseek", SetLastError = true)] private static '
                                   'extern long RecordLockSeek(int fd, long offset, int whence);',
                    'memberRole': 'private static extern member',
                    'sourceSha256': '2841074dbff045fb3ad5a66856a8fd623ddab7d09a4b5c163fb065286e8f1655',
                    'resourceName': 'DotCC.Runtime.RecordLockLib.cs'},
 'ResolverHostErrno': {'runtimeModule': 'ResolverLib.cs',
                       'library': 'libc',
                       'symbol': '__errno_location',
                       'declaration': '[DllImport("libc", EntryPoint = "__errno_location")] private static extern '
                                      'int* ResolverHostErrno();',
                       'memberRole': 'private static extern member',
                       'sourceSha256': '3dd78c66d7f2d4ad5577509f8dc779614946ebb11eb8d7c0f9ed730c52296f3f',
                       'resourceName': 'DotCC.Runtime.ResolverLib.cs'},
 'HostGetNameInfo': {'runtimeModule': 'ResolverLib.cs',
                     'library': 'libc',
                     'symbol': 'getnameinfo',
                     'declaration': '[DllImport("libc", EntryPoint = "getnameinfo")] private static extern int '
                                    'HostGetNameInfo(void* address, uint addressLength, byte* host, uint '
                                    'hostLength, byte* service, uint serviceLength, int flags);',
                     'memberRole': 'private static extern member',
                     'sourceSha256': '3dd78c66d7f2d4ad5577509f8dc779614946ebb11eb8d7c0f9ed730c52296f3f',
                     'resourceName': 'DotCC.Runtime.ResolverLib.cs'},
 'HostGetHostByName': {'runtimeModule': 'ResolverLib.cs',
                       'library': 'libc',
                       'symbol': 'gethostbyname_r',
                       'declaration': '[DllImport("libc", EntryPoint = "gethostbyname_r")] private static extern '
                                      'int HostGetHostByName(byte* name, void* record, byte* buffer, nuint '
                                      'capacity, void** result, int* hostError);',
                       'memberRole': 'private static extern member',
                       'sourceSha256': '3dd78c66d7f2d4ad5577509f8dc779614946ebb11eb8d7c0f9ed730c52296f3f',
                       'resourceName': 'DotCC.Runtime.ResolverLib.cs'},
 'HostGetAddrInfo': {'runtimeModule': 'ResolverLib.cs',
                     'library': 'libc',
                     'symbol': 'getaddrinfo',
                     'declaration': '[DllImport("libc", EntryPoint = "getaddrinfo")] private static extern int '
                                    'HostGetAddrInfo(byte* name, byte* service, ResolverInfo* hints, '
                                    'ResolverInfo** result);',
                     'memberRole': 'private static extern member',
                     'sourceSha256': '3dd78c66d7f2d4ad5577509f8dc779614946ebb11eb8d7c0f9ed730c52296f3f',
                     'resourceName': 'DotCC.Runtime.ResolverLib.cs'},
 'HostFreeAddrInfo': {'runtimeModule': 'ResolverLib.cs',
                      'library': 'libc',
                      'symbol': 'freeaddrinfo',
                      'declaration': '[DllImport("libc", EntryPoint = "freeaddrinfo")] private static extern void '
                                     'HostFreeAddrInfo(ResolverInfo* result);',
                      'memberRole': 'private static extern member',
                      'sourceSha256': '3dd78c66d7f2d4ad5577509f8dc779614946ebb11eb8d7c0f9ed730c52296f3f',
                      'resourceName': 'DotCC.Runtime.ResolverLib.cs'}}

DYNAMIC_CODE = re.compile(r'\b(?:Reflection\.Emit|DynamicMethod|AssemblyBuilder|TypeBuilder|'
                          r'Assembly\.Load(?:From|File)?\s*\(|Expression\s*\.\s*Compile\s*\(|'
                          r'Marshal\.GetDelegateForFunctionPointer|Marshal\.GetFunctionPointerForDelegate)')
IMPORT = re.compile(r'\[(?:[\w:.]+\.)?(DllImport|LibraryImport)(?:Attribute)?\(\s*"([^"\n]+)"([^]]*)\)\]'
                    r'\s*((?:public|private|internal|protected|static|extern|partial|unsafe|\w+[?*]?|\s)+)'
                    r'\s+(\w+)\s*\(', re.MULTILINE)
LEXICAL = re.compile(r'//[^\n]*|/\*.*?\*/|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'', re.DOTALL)
# User-approved exception for Kerberos.NET's assembly-level analysis warnings.
# Keep them in the receipt; do not suppress these warning codes for other assemblies.
IGNORED_AOT_WARNING = re.compile(
    r"^warning (?:IL2104: Assembly 'Kerberos\.NET' produced trim warnings\."
    r"|IL3053: Assembly 'Kerberos\.NET' produced AOT analysis warnings\.)")


def without_comments(text):
    return LEXICAL.sub(lambda match: re.sub(r'[^\n]', ' ', match.group())
                       if match.group().startswith(('//', '/*')) else match.group(), text)


def runtime_source_binding():
    """Reuse the sealed actual 84-resource proof and strict selected tool closure."""
    from production_support import selected_tool, validate_tool_closure
    proof = Path(RUNTIME_RESOURCE_PROOF['path'])
    if sha(proof) != RUNTIME_RESOURCE_PROOF['sha256']:
        raise RuntimeError('Shared runtime resource proof changed')
    resources = json.loads(proof.read_text())
    closure = validate_tool_closure(selected_tool())
    if (closure.get('DotCC.Lib.dll') != RUNTIME_RESOURCE_PROOF['compilerLibrarySha256']
            or resources['compilerLibrarySha256'] != closure['DotCC.Lib.dll']
            or resources['runtimeResourceCount'] != 84 or resources['headerResourceCount'] != 68):
        raise RuntimeError('Selected compiler differs from the exact runtime resource proof')
    for rule in RUNTIME_IMPORTS.values():
        path = ROOT.parent / 'DotCC.Libc' / rule['runtimeModule']
        source = next((row for row in resources['sources'] if row['resourceName'] == rule['resourceName']), None)
        if (source is None or source['source'] != str(path) or source['sha256'] != rule['sourceSha256']
                or sha(path) != rule['sourceSha256']):
            raise RuntimeError('Exact shared runtime source changed: ' + rule['runtimeModule'])
    return RUNTIME_RESOURCE_PROOF


def runtime_import_rule(assembly, relative, original, code, match, module):
    """Match exact declarations at their lexical nested type and local scope."""
    kind, library, options, declaration, method = match.groups()
    rule = RUNTIME_IMPORTS.get(method)
    if not rule or assembly != 'TranslatedLibsmb2' or relative != 'LibSmb2.cs' or module != rule['runtimeModule']:
        return None
    end = code.find(';', match.end())
    if end < 0 or ' '.join(code[match.start():end + 1].split()) != rule['declaration']:
        return None
    if kind != 'DllImport' or library != rule['library']:
        return None
    # Mask comments AND literals before balancing braces or identifying types.
    masked = LEXICAL.sub(lambda m: re.sub(r'[^\n]', ' ', m.group()), original)
    namespaces = list(re.finditer(r'\bnamespace\s+([\w.]+)\s*([;{])', masked))
    if len(namespaces) != 1 or namespaces[0].groups() != ('Managed.Smb', ';'):
        return None
    stack, pending = [], None
    for token in re.finditer(r'\b(?:class|struct|interface|enum)\s+(@?\w+)|[{};]', masked[:match.start()]):
        value = token.group()
        if token.group(1):
            pending = token.group(1)
        elif value == '{':
            stack.append((pending, token.start())); pending = None
        elif value == '}':
            if not stack: return None
            stack.pop(); pending = None
        else:
            pending = None
    if [name for name, _ in stack if name] != ['LibSmb2', 'Libc']:
        return None
    if method == 'ResizeFile':
        # This one source-local extern is lowered to a private static method.
        if len(stack) != 3 or stack[-1][0] is not None:
            return None
        opening = stack[-1][1]
        if not re.search(r'\bpublic\s+static\s+int\s+ftruncate\s*\(\s*int\s+fd\s*,\s*long\s+length\s*\)\s*$',
                         masked[max(0, opening - 250):opening]):
            return None
    elif len(stack) != 2:
        return None
    return rule


def project_inputs(project):
    # Evaluate imports, including Directory.Build.targets, without building.
    # Scanning only files beside the csproj misses authored transport sources.
    evaluated = json.loads(subprocess.check_output([
        'dotnet', 'msbuild', str(project),
        '-getItem:Compile,PackageReference,ProjectReference,NativeLibrary',
        '-getProperty:AssemblyName'], text=True))
    files = {project.resolve()}
    files.update(Path(item['FullPath']).resolve() for item in evaluated['Items']['Compile'])
    imports = {Path(item['DefiningProjectFullPath']).resolve()
               for item in evaluated['Items']['Compile']}
    # Include the authored import chain in the source receipt as well.
    for directory in [project.parent, *project.parent.parents]:
        if not directory.is_relative_to(ROOT.parent):
            continue
        for name in ('Directory.Build.props', 'Directory.Build.targets', 'Directory.Packages.props'):
            path = directory / name
            if path.is_file():
                imports.add(path.resolve())
    imports = {path for path in imports if path.is_relative_to(ROOT.parent)}
    return sorted(files), sorted(imports), evaluated


def inventory(project):
    files, imports, evaluated = project_inputs(project)
    assembly = evaluated['Properties']['AssemblyName'] or project.stem
    if assembly != 'TranslatedLibsmb2':
        raise RuntimeError('Rooted audit expects exact assembly name TranslatedLibsmb2: ' + assembly)
    runtime_binding = runtime_source_binding()
    result = dict(project=str(project), assembly=assembly, files={}, authored_files={},
                  build_inputs={str(path.relative_to(ROOT.parent)): sha(path) for path in imports}, imports=[],
                  native_loader_helpers=[], dynamic_code=[], unexpected_dependencies=[],
                  translated_native_loader_calls=[], unparsed_import_attributes=[], managed_dependencies=[])
    result['exact_shared_runtime_resource_proof'] = runtime_binding
    for kind in ('PackageReference', 'ProjectReference', 'NativeLibrary'):
        for item in evaluated['Items'][kind]:
            dependency = dict(kind=kind, attributes=item)
            if kind == 'PackageReference' and item['Identity'] == 'Kerberos.NET':
                result['managed_dependencies'].append(dependency)
            else:
                result['unexpected_dependencies'].append(dependency)
    for path in files:
        if path.is_relative_to(project.parent):
            relative = str(path.relative_to(project.parent))
            result['files'][relative] = sha(path)
        else:
            if not path.is_relative_to(ROOT / 'src'):
                raise RuntimeError('Unexpected external compilation source: ' + str(path))
            relative = str(path.relative_to(ROOT))
            result['authored_files'][relative] = sha(path)
        if path.suffix != '.cs':
            continue
        original = path.read_text()
        code = without_comments(original)
        modules = []
        for match in re.finditer(r'^\s*// ---- ([\w.]+\.cs) ----', original, re.MULTILINE):
            modules.append((match.start(), match.group(1)))

        def location(offset):
            module = None
            for start, name in modules:
                if start > offset:
                    break
                module = name
            return dict(file=relative, line=original.count('\n', 0, offset) + 1, runtime_module=module)

        parsed_imports = list(IMPORT.finditer(code))
        all_imports = list(re.finditer(r'\[\s*(?:[\w:.]+\.)?(?:DllImport|LibraryImport)(?:Attribute)?\s*\(', code))
        parsed_offsets = {match.start() for match in parsed_imports}
        result['unparsed_import_attributes'].extend(location(match.start()) for match in all_imports
                                                     if match.start() not in parsed_offsets)
        for match in parsed_imports:
            kind, library, options, declaration, method = match.groups()
            entry = re.search(r'EntryPoint\s*=\s*"([^"]+)"', options)
            symbol = entry.group(1) if entry else method
            allowed = library in ALLOWED_IMPORTS and symbol in ALLOWED_IMPORTS[library]
            loc = location(match.start())
            exact = runtime_import_rule(assembly, relative, original, code, match, loc['runtime_module'])
            if exact is not None:
                allowed = True
            result['imports'].append(dict(**location(match.start()), kind=kind, library=library,
                                           symbol=symbol, method=method, generic_host_allowlisted=allowed,
                                           exact_shared_runtime_record=exact))
        for match in DYNAMIC_CODE.finditer(code):
            result['dynamic_code'].append(dict(**location(match.start()), expression=match.group()))
        for match in re.finditer(r'\bNativeLibrary\.\w+\s*\(', code):
            loc = location(match.start())
            result['native_loader_helpers'].append(dict(**loc, expression=match.group()))
            if loc['runtime_module'] not in ('DlfcnLib.cs', 'NativeImports.cs'):
                result['translated_native_loader_calls'].append(dict(**loc, expression=match.group()))
        for match in re.finditer(r'\b(?:NativeImports\.(?:LoadLibrary|TryResolveExport)|DotCcImports\.\w+|dlopen|dlsym)\s*\(', code):
            loc = location(match.start())
            if loc['runtime_module'] not in ('DlfcnLib.cs', 'NativeImports.cs'):
                result['translated_native_loader_calls'].append(dict(**loc, expression=match.group()))
    result['static_passed'] = (all(item['generic_host_allowlisted'] for item in result['imports'])
                               and not result['dynamic_code'] and not result['unexpected_dependencies']
                               and not result['translated_native_loader_calls'] and not result['unparsed_import_attributes'])
    return result


def inspect_aot_artifacts(harness, audit):
    maps = list(harness.rglob('ProductAudit.map.xml'))
    responses = list(harness.rglob('ProductAudit.ilc.rsp'))
    if len(maps) != 1 or len(responses) != 1:
        raise RuntimeError('Expected one NativeAOT map and compiler response file')
    arguments = responses[0].read_text().splitlines()
    if '--root:TranslatedLibsmb2' not in arguments:
        raise RuntimeError('Native compiler did not receive the complete generated assembly root')
    method_names = [node.attrib['Name'] for node in ET.parse(maps[0]).iter('MethodCode')
                    if node.attrib.get('Name', '').startswith('TranslatedLibsmb2_')]
    if not any('__smb2_connect_share' in name for name in method_names):
        raise RuntimeError('Native map lacks uninvoked rooted SMB methods')
    audit['aot_map'] = str(maps[0].relative_to(ROOT))
    audit['aot_map_sha256'] = sha(maps[0])
    audit['aot_response'] = str(responses[0].relative_to(ROOT))
    audit['aot_response_sha256'] = sha(responses[0])
    audit['aot_root_argument'] = '--root:TranslatedLibsmb2'
    audit['compiled_generated_method_count'] = len(method_names)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--static-only', action='store_true', help='Inventory sources without any .NET build')
    parser.add_argument('--variant', choices=['raw', 'processed', 'both'], default='both')
    parser.add_argument('--rid', default='linux-x64')
    args = parser.parse_args()
    logs = ROOT / 'artifacts/product-audit'
    logs.mkdir(parents=True, exist_ok=True)
    build = ROOT / 'build/product-audit'
    build.mkdir(parents=True, exist_ok=True)
    stage = Path(tempfile.mkdtemp(prefix='run-', dir=build))
    receipt = dict(passed=False, static_only=args.static_only, rid=args.rid,
                   scope='source dependency inventory and complete generated assembly NativeAOT compilation',
                   limitations=['Not SMB behavioral qualification.',
                                'Source scanning is an inventory, not a complete call-graph proof.',
                                'Generic host P/Invokes and dormant native loader helpers remain in the runtime.',
                                'Kerberos.NET and its transitive packages require separate behavioral qualification.',
                                'Windows current-logon credentials use the explicitly inventoried secur32 SSPI adapter.'],
                   staging_directory=str(stage.relative_to(ROOT)), variants={})
    try:
        for variant, directory in [('raw', 'TranslatedLibsmb2.Raw'), ('processed', 'TranslatedLibsmb2')]:
            if args.variant not in ('both', variant):
                continue
            project = ROOT / 'generated' / directory / 'TranslatedLibsmb2.csproj'
            if not project.is_file():
                raise RuntimeError('Missing final product: ' + str(project))
            audit = inventory(project)
            receipt['variants'][variant] = audit
            if not audit['static_passed']:
                raise RuntimeError('Unexpected dependency or dynamic-code finding in ' + variant)
            print(f'{variant}: {len(audit["imports"])} host imports (including Windows SSPI), '
                  f'{len(audit["native_loader_helpers"])} dormant native loader call sites; static inventory PASS', flush=True)
            if args.static_only:
                continue
            destination = stage / variant
            library = destination / 'library'
            harness = destination / 'harness'
            host = destination / 'host'
            library.mkdir(parents=True)
            harness.mkdir()
            host.mkdir()
            for relative in audit['files']:
                target = library / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(project.parent / relative, target)
            if audit['files'] != {relative: sha(library / relative) for relative in audit['files']}:
                raise RuntimeError('Generated sources changed while snapshotting')
            for relative, expected in audit['authored_files'].items():
                target = host / Path(relative).relative_to('src')
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(ROOT / relative, target)
                if sha(target) != expected:
                    raise RuntimeError('Authored host sources changed while snapshotting')
            for path in (ROOT / 'tests/ProductAudit').iterdir():
                if path.suffix in ('.cs', '.csproj'):
                    shutil.copyfile(path, harness / path.name)
            isolated_project = harness / 'ProductAudit.csproj'
            option = '-p:Libsmb2GeneratedProject=' + str(library / 'TranslatedLibsmb2.csproj')
            host_option = '-p:Libsmb2HostSourceDirectory=' + str(host)
            evaluated = run(['dotnet', 'msbuild', isolated_project, option, host_option, '-getItem:TrimmerRootAssembly'],
                            logs / (variant + '-roots.log'), receipt)
            roots = json.loads(evaluated)['Items']['TrimmerRootAssembly']
            if not any(item['Identity'] == 'TranslatedLibsmb2' for item in roots):
                raise RuntimeError('Complete generated assembly is not an evaluated AOT root')
            audit['evaluated_roots'] = roots
            publish = destination / 'publish'
            run(['dotnet', 'publish', isolated_project, '-c', 'Release', '-r', args.rid,
                 '-p:PublishAot=true', '-p:IlcGenerateMapFile=true', option, host_option, '-o', publish, '--nologo'],
                logs / (variant + '-aot-build.log'), receipt, timeout=1800)
            binary = publish / 'ProductAudit'
            output = run([binary], logs / (variant + '-aot-run.log'), receipt, timeout=30).strip()
            if not re.fullmatch(r'ProductAudit: smb2_context=\d+', output):
                raise RuntimeError('Unexpected audit harness output: ' + output)
            audit['aot_output'] = output
            audit['aot_binary_sha256'] = sha(binary)
            inspect_aot_artifacts(harness, audit)
            audit['aot_analysis_warnings'] = re.findall(r'warning IL\d+[^\n]*',
                (logs / (variant + '-aot-build.log')).read_text())
            audit['ignored_aot_analysis_warnings'] = [warning for warning in audit['aot_analysis_warnings']
                                                     if IGNORED_AOT_WARNING.match(warning)]
            audit['blocking_aot_analysis_warnings'] = [warning for warning in audit['aot_analysis_warnings']
                                                      if not IGNORED_AOT_WARNING.match(warning)]
            if audit['ignored_aot_analysis_warnings']:
                print(variant + ': ignoring Kerberos.NET IL2104/IL3053 warnings (retained in receipt)', flush=True)
            if audit['blocking_aot_analysis_warnings']:
                raise RuntimeError('NativeAOT/trim analysis warnings require review')
            if args.rid.startswith('linux-'):
                dependencies = run(['readelf', '-d', binary], logs / (variant + '-elf-dynamic.log'), receipt)
                audit['elf_needed'] = re.findall(r'\(NEEDED\).*?\[([^]]+)\]', dependencies)
                forbidden = re.compile(r'smb|crypto|ssl|krb|gss|ws2_32|secur32', re.IGNORECASE)
                if any(forbidden.search(name) for name in audit['elf_needed']):
                    raise RuntimeError('Native SMB/crypto dependency needs origin review (BCL implementation dependencies may be legitimate)')
            current = inventory(project)
            if any(audit[key] != current[key] for key in ('files', 'authored_files', 'build_inputs')):
                raise RuntimeError('Generated/authored sources or build imports changed during audit')
            print(variant + ': whole-assembly-rooted NativeAOT publish and execution PASS', flush=True)
        if not args.static_only and len({item['aot_output'] for item in receipt['variants'].values()}) != 1:
            raise RuntimeError('Raw and processed audit ABI anchors differ')
        receipt['passed'] = True
    except BaseException as error:
        receipt['failure'] = str(error)
        raise
    finally:
        (logs / ('static-result.json' if args.static_only else 'result.json')).write_text(json.dumps(receipt, indent=2) + '\n')


if __name__ == '__main__':
    main()
