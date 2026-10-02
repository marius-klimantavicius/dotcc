/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

// Hash anchors for deep-linking into a review selection.
// Used on the corresponding /tests or /benchmarks route:
//
//   #encodeURIComponent("<diffHash>:<testName>")
//   #encodeURIComponent("<benchmarkId>")
//
// ':' separates components because it is not normally allowed in diff hashes
// or benchmark ids; the entire payload is URI-encoded as one string.

const COMPONENT_SEP = ':'

const stripHash = (hash) => {
  const raw = (hash ?? '').startsWith('#') ? hash.slice(1) : (hash ?? '')
  return raw.startsWith('/') ? raw.slice(1) : raw
}

export const formatTestAnchor = (diffHash, testName) => {
  return encodeURIComponent(`${diffHash}${COMPONENT_SEP}${testName}`)
}

export const formatBenchAnchor = (benchId) => {
  return encodeURIComponent(benchId)
}

export const parseTestAnchor = (hash) => {
  const raw = stripHash(hash)
  if (!raw) {
    return null
  }

  try {
    const decoded = decodeURIComponent(raw)
    const sep = decoded.indexOf(COMPONENT_SEP)
    if (sep === -1) {
      return null
    }
    return {
      diffHash: decoded.slice(0, sep),
      testName: decoded.slice(sep + 1)
    }
  } catch {
    return null
  }
}

export const parseBenchAnchor = (hash) => {
  const raw = stripHash(hash)
  if (!raw) {
    return null
  }

  try {
    return { id: decodeURIComponent(raw) }
  } catch {
    return null
  }
}
