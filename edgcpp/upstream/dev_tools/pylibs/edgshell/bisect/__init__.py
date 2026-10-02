# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

# This module is built around bisecting the mono repo itself.
#
# It's not (currently) a general bisect utility.

import subprocess

import edgshell

from pathlib import Path
from typing import List

from edgshell import TOOL_PATH_GIT

class BisectInProgressException(Exception):
  pass

class NoBisectException(Exception):
  pass

class MonoRepoBisector:
  def __init__(self, mono_repo_dir: Path, *, backfill_allowed = True) -> None:
    self.mono_repo_dir = mono_repo_dir
    if backfill_allowed:
      self.enable_backfill = edgshell.is_git_version_at_least(2, 55)
    else:
      self.enable_backfill = False

  def _run_git_cmd(self, *cmd_args: List[str]) -> subprocess.CompletedProcess:
    return subprocess.run(
      [str(TOOL_PATH_GIT), *cmd_args],
      cwd = self.mono_repo_dir
    )

  def restore_bisect_files(self, target_commit: str = 'BISECT_HEAD') -> None:
    edgshell.run_git_restore_src_files(target_commit, cwd = self.mono_repo_dir)

  def is_active(self) -> None:
    return (self.mono_repo_dir / '.git' / 'BISECT_LOG').exists()

  def get_bisect_head(self) -> str:
    if not self.is_active():
      raise NoBisectException()

    return edgshell.get_git_commit_sha('BISECT_HEAD', cwd = self.mono_repo_dir)

  def start(self, bad_commit: str, good_commit: str) -> None:
    if self.is_active():
      raise BisectInProgressException()

    self._run_git_cmd(
      'bisect',
      'start',
      '--no-checkout',
      bad_commit,
      good_commit
    )
    if self.enable_backfill:
      # At the time of writing git-restore is very slow at downloading the
      # files it needs from a partial clone.  To work around this, the
      # experimental git-backfill is used to download any missing blobs,
      # related to the range of history relevant to the bisect, in full (if a
      # new enough git version is present on the system).
      self._run_git_cmd(
        'backfill',
        f"{good_commit}..{bad_commit}"
      )
    self.restore_bisect_files()

  def mark_good(self) -> None:
    if not self.is_active():
      raise NoBisectException()

    self._run_git_cmd(
      'bisect',
      'good'
    )
    self.restore_bisect_files()

  def mark_bad(self) -> None:
    if not self.is_active():
      raise NoBisectException()

    self._run_git_cmd(
      'bisect',
      'bad'
    )
    self.restore_bisect_files()

  def skip(self) -> None:
    if not self.is_active():
      raise NoBisectException()

    self._run_git_cmd(
      'bisect',
      'skip'
    )
    self.restore_bisect_files()

  def reset(self) -> None:
    if not self.is_active():
      raise NoBisectException()

    self._run_git_cmd(
      'bisect',
      'reset'
    )
    self.restore_bisect_files('HEAD')

  def view(self) -> None:
    if not self.is_active():
      raise NoBisectException()

    self._run_git_cmd(
      'bisect',
      'view'
    )

  def replay(self, log_file: Path) -> None:
    if self.is_active():
      raise BisectInProgressException()

    self._run_git_cmd(
      'bisect',
      'replay',
      str(log_file)
    )
    self.restore_bisect_files()

  def log(self) -> None:
    if not self.is_active():
      raise NoBisectException()

    self._run_git_cmd(
      'bisect',
      'log'
    )

