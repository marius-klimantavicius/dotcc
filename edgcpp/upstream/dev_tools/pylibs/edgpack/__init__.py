# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

import io
import os
import platform
import stat
import sys
import tarfile
import time

import edgshell

from datetime import date
from pathlib import Path, PurePath

from edgutil import eprint

DEFAULT_PREDEF_MACROS="""
# EDG C/C++ predefined macro definition file.
#
# The format of the entries in this file is:
#
# mode,!mode,mode   cannot_redefine   macro_name   macro_value
#
# - "mode" is a label from the predefined macro modes table.  The macro is
#   defined if the mode is set, or if the mode is not set when "!mode" is
#   used.  The macro is defined if any of the mode tests is TRUE.  The
#   mode table can be customized, but the current set of supported modes is:
#
#     gnu:        gcc or g++ mode
#     gcc:        gcc mode
#     gpp:        g++ mode
#     microsoft:  Microsoft mode
#     strict:     strict C or C++ mode
#     cpp:        any C++ mode
#     all:        all modes
#
# - cannot_redefine indicates whether the predefined macro may later be
#   redefined.  The value must be "yes" or "no".
#
# - macro_name is the name of the macro to be defined.
#
# - macro_value is the value to which the macro should be defined.  All of
#   the characters until the end of the line are used as the macro value.
#
# See also the make_predef_macro_table script in the "misc" directory,
# which can create this file for a given version of the gcc compiler.
#
"""

def user_info_stripper(info: tarfile.TarInfo) -> tarfile.TarInfo:
  info.uid = info.gid = 0
  info.uname = info.gname = ''
  info.mtime = int(time.time())
  return info

def _require_file(path: Path) -> None:
  if path.exists():
    return
  eprint(f"{path} is required but does not exist")
  sys.exit(1)

def _require_multi_arch(path: Path) -> None:
  if edgshell.has_architectures(path, 'arm64', 'x86_64'):
    return
  eprint(f"{path} is not a universal binary")
  eprint('all executables must be universal binaries')
  sys.exit(1)

def _require_static_linking(path: Path) -> None:
  if not edgshell.is_dynamic_executable(path):
    return
  eprint(f"{path} is a dynamically linked executable")
  eprint('all executables must be statically linked')
  sys.exit(1)

def check_readme(source_path: Path) -> None:
  '''Perform checks on the README file at the given source path that is
  intended for packaging.
  '''
  _require_file(source_path)

def check_bash_script(source_path: Path) -> None:
  '''Perform checks on the bash script at the given source path that is
  intended for packaging.
  '''
  _require_file(source_path)

def check_python_script(source_path: Path) -> None:
  '''Perform checks on the python script at the given source path that is
  intended for packaging.
  '''
  _require_file(source_path)

def check_binary(source_path: Path) -> None:
  '''Perform checks on the executable binary at the given source path that is
  intended for packaging.
  '''
  _require_file(source_path)
  if platform.system() == 'Darwin':
    # On MacOS static linking is discouraged and largely unsupported (so that
    # is not checked here).
    #
    # We also need to consider multiple architectures on the MacOS platform.
    _require_multi_arch(source_path)
  else:
    _require_static_linking(source_path)

def check_lib(source_path: Path) -> None:
  '''Perform checks on the library at the given source path that is intended
  for packaging.
  '''
  _require_file(source_path)
  if platform.system() == 'Darwin':
    # We need to consider multiple architectures on the MacOS platform.
    _require_multi_arch(source_path)

def _is_edg_std_header_file(filename: str) -> bool:
  if filename.endswith('.h'):
    return True
  if filename.endswith('.stdh'):
    return True
  return False

def add_header_files_to_tar(tar: tarfile.TarFile, header_dir: Path,
                            directory: PurePath) -> None:
  '''Add the header files in the directory header_dir to the given directory
  inside of the tar file.
  '''
  for root, dirs, filenames in os.walk(header_dir):
    # Skip any experimental subdirectories.
    #
    # Note this function can still be used for adding experimental headers by
    # using the experimental directory as the root (not a subdirectory).
    dirs[:] = [d for d in dirs if d != 'experimental']
    # Add the header files.
    for filename in filenames:
      if _is_edg_std_header_file(filename):
        file_path = Path(root, filename)
        tar.add(
          file_path,
          str(directory / file_path.relative_to(header_dir)),
          filter = user_info_stripper
        )

def write_file_to_tar(tar: tarfile.TarFile, source_path: Path,
                      dest_path: PurePath) -> None:
  '''Write the given file to the given tar file at the given destination.

  All user information will be stripped from the file with the user info
  stripper.
  '''
  tar.add(source_path, str(dest_path), filter = user_info_stripper)

def _tar_info_for_str_file(arcname: str,
                           bytes_io: io.BytesIO) -> tarfile.TarInfo:
  info = tarfile.TarInfo(arcname)
  info.size = len(bytes_io.getbuffer())
  # Apply the user info stripper to get consistent results with
  # write_file_to_tar.
  info = user_info_stripper(info)
  return info

def write_str_to_tar(tar: tarfile.TarFile, dest_path: PurePath, text: str, *,
                     encoding: str = 'utf-8') -> None:
  '''Write the given text to the given tar file (using the specified encoding)
  with the file path described by dest_path.
  '''
  bytes_io = io.BytesIO(text.encode(encoding))
  info = _tar_info_for_str_file(str(dest_path), bytes_io)

  tar.addfile(info, bytes_io)


def write_script_str_to_tar(tar: tarfile.TarFile, dest_path: PurePath,
                            text: str, *,
                            encoding: str = 'utf-8') -> None:
  '''Write the given text to the given tar file (using the specified encoding)
  with the file path described by dest_path.
  '''
  bytes_io = io.BytesIO(text.encode(encoding))
  info = _tar_info_for_str_file(str(dest_path), bytes_io)
  # Make the script an executabe file
  info.mode = (
    # User - read, write, execute
    stat.S_IRUSR | stat.S_IWUSR | stat.S_IXUSR |
    # Group - read, execute
    stat.S_IRGRP | stat.S_IXGRP |
    # Other - read, execute
    stat.S_IROTH | stat.S_IXOTH
  )

  tar.addfile(info, bytes_io)
