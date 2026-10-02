# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# This script is execute by build-edg-env before creation of the Docker image
# to create the necessary softfloat.tar.gz file.

import os
import subprocess
import tarfile
import tempfile

from pathlib import Path

SOFTFLOAT_DIR = Path(__file__).resolve().parent
SOFTFLOAT_GIT_URL = 'https://github.com/ucb-bar/berkeley-softfloat-3.git'
SOFTFLOAT_COMMIT_FILE = SOFTFLOAT_DIR / 'SOFTFLOAT_COMMIT'
# Upstream Linux-386 Makefiles assume a native 32-bit host; on x86_64 we must
# pass -m32 so the archive installed as lib/x86 is actually i386.
SOFTFLOAT_PATCHES = (SOFTFLOAT_DIR / 'linux-386-m32.patch',)

def softfloat_commit() -> str:
  '''Return the pinned berkeley-softfloat-3 commit SHA.'''
  return SOFTFLOAT_COMMIT_FILE.read_text().strip()

def clone_softfloat(dest: Path) -> None:
  '''Shallow-clone berkeley-softfloat-3 at the pinned commit into ``dest``.'''
  commit = softfloat_commit()
  dest.mkdir(parents = True)
  subprocess.run(['git', 'init'], cwd = dest, check = True)
  subprocess.run(
    ['git', 'remote', 'add', 'origin', SOFTFLOAT_GIT_URL],
    cwd = dest,
    check = True
  )
  subprocess.run(
    ['git', 'fetch', '--depth', '1', 'origin', commit],
    cwd = dest,
    check = True
  )
  subprocess.run(
    ['git', 'checkout', 'FETCH_HEAD'],
    cwd = dest,
    check = True
  )

def apply_softfloat_patches(dest: Path) -> None:
  '''Apply local SoftFloat patches under ``dest``.'''
  for patch in SOFTFLOAT_PATCHES:
    subprocess.run(
      ['git', 'apply', '--verbose', str(patch)],
      cwd = dest,
      check = True
    )

def create_src_tar_file() -> None:
  '''Create softfloat.tar.gz for use by the Dockerfile.'''
  tar_file_path = Path(os.getcwd()).joinpath('softfloat.tar.gz')

  with tempfile.TemporaryDirectory() as tmp_dir_str:
    softfloat_dir = Path(tmp_dir_str, 'berkeley-softfloat-3')
    clone_softfloat(softfloat_dir)
    apply_softfloat_patches(softfloat_dir)
    with tarfile.open(tar_file_path, 'w:gz') as tar:
      for dir_entry in os.listdir(softfloat_dir):
        if dir_entry == '.git':
          continue
        tar.add(softfloat_dir / dir_entry, arcname = dir_entry)

def main() -> None:
  create_src_tar_file()

main()
