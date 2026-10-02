# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# This script is execute by build-edg-env before creation of the Docker image
# to create the necessary native-tools.tar.gz file.

import os
import tarfile

import edgtools
import edgutil

from pathlib import Path

def create_src_tar_file():
  '''Create native-tools.tar.gz for use by the Dockerfile.'''
  tools_dir = edgtools.get_tools_dir()
  tar_file_path = Path(os.getcwd()).joinpath('native-tools.tar.gz')

  with edgutil.CurrentWorkingDirectorySwap(tools_dir):
    with tarfile.open(tar_file_path, 'w:gz') as tar:
      tar.add('CMakeLists.txt')
      tar.add('cpp_tools')

def main() -> None:
  create_src_tar_file()

main()
