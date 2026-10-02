#!/bin/bash -e

# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# Install websockets for AcknowlEDG CLIs
pip install -U websockets

# Install sarif-tools
pip install -U sarif-tools

# For cvise
pip install -U chardet
