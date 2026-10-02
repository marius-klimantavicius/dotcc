/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

// Client-side equivalents of edg-bench-run-delta's change metrics.

export const computePercentChange = (baselineOps, runOps) => {
  return ((baselineOps - runOps) / Math.max(1, baselineOps)) * 100
}

export const formPercentChangeComment = (percentChange) => {
  if (percentChange >= 0) {
    return `${percentChange.toFixed(2)}% faster`
  }
  return `${Math.abs(percentChange).toFixed(2)}% slower`
}

export const formChangeComment = (baselineOps, runOps) => {
  return formPercentChangeComment(computePercentChange(baselineOps, runOps))
}

export const opDelta = (baselineOps, runOps) => {
  return runOps - baselineOps
}

export const isFaster = (baselineOps, runOps) => {
  return runOps <= baselineOps
}

export const formatSignedOps = (value) => {
  const formatted = Math.abs(value).toLocaleString()
  return value >= 0 ? `+${formatted}` : `-${formatted}`
}

export const mean = (values) => {
  if (values.length === 0) {
    return 0
  }
  let total = 0
  for (const value of values) {
    total += value
  }
  return total / values.length
}

export const median = (values) => {
  if (values.length === 0) {
    return 0
  }
  const sorted = values.slice().sort((left, right) => left - right)
  const middle = Math.floor(sorted.length / 2)
  if (sorted.length % 2 === 0) {
    return (sorted[middle - 1] + sorted[middle]) / 2
  }
  return sorted[middle]
}

// Population standard deviation (statistics.pstdev): divide by N.
export const populationStdev = (values) => {
  if (values.length === 0) {
    return 0
  }
  const average = mean(values)
  let sumSquares = 0
  for (const value of values) {
    const delta = value - average
    sumSquares += delta * delta
  }
  return Math.sqrt(sumSquares / values.length)
}
