// Differential oracle only. The reusable managed library never imports this.
import fs from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath, pathToFileURL } from 'node:url'
import assert from 'node:assert/strict'

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../..')
const pin = JSON.parse(await fs.readFile(path.join(root, 'config/api-source.json'), 'utf8'))
const fixture = JSON.parse(await fs.readFile(path.join(root, 'tests/ApiAcceptance/oracle-cases.json'), 'utf8'))
const { PGlite } = await import(pathToFileURL(path.join(root, 'ref', pin.oracle.directory, 'package/dist/index.js')))
const pg = await PGlite.create()
const value = p => p && typeof p === 'object' && !Array.isArray(p)
  ? ('$bytes' in p ? new Uint8Array(p.$bytes) : '$json' in p ? p.$json : p)
  : p
const results = []
try {
  for (const test of fixture.cases) {
    try {
      const result = await pg.query(test.sql, test.params.map(value), { rowMode: 'array' })
      assert.equal(test.sqlstate, undefined, `${test.name}: expected SQL error`)
      // Except NULL these fixture queries explicitly select server text, so
      // number/date/JSON differences between host codec choices cannot hide SQL errors.
      assert.deepEqual(result.rows, test.rows, test.name)
      results.push({ name: test.name, status: 'passed', rows: result.rows, oids: result.fields.map(f => f.dataTypeID) })
    } catch (error) {
      if (!test.sqlstate || error.code !== test.sqlstate) throw error
      results.push({ name: test.name, status: 'passed', sqlstate: error.code })
    }
    console.log(`PASS oracle/${test.name}`)
  }
} finally { await pg.close() }
const report = { package: pin.oracle.package, version: pin.oracle.version, sourceRevision: pin.revision,
  engine: 'upstream npm JS/WASM oracle only', passed: results.length, failed: 0, cases: results }
const output = process.argv[2]
if (output) await fs.writeFile(output, JSON.stringify(report, null, 2) + '\n')
console.log(`PASS ${results.length} pinned-upstream differential cases`)
