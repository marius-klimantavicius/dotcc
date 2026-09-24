import hashlib
import json
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "scripts"))
from notices import write_notices


class NoticeTests(unittest.TestCase):
    def test_trailing_and_line_notices_are_preserved_without_c_code_or_string_literals(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            source = root / "input.c"
            source.write_text('const char *s = "/* Copyright fake literal */";\n'
                              'int code(void) { return 1; }\n'
                              '/* Copyright owner.\nPermission is hereby granted in full. */\n'
                              '// SPDX-License-Identifier: MIT\n// additional condition\n')
            (root / "sources.json").write_text(json.dumps({"commit": "pin", "sources": [{"path": source.name}]}))
            (root / "licenses.json").write_text(json.dumps({"commit": "pin", "files": []}))
            project = root / "Output.csproj"
            project.write_text('<Project/>')
            first = write_notices(root, root, project)
            notice = (root / "UPSTREAM-NOTICES.txt").read_text()
            self.assertIn("Permission is hereby granted in full.", notice)
            self.assertIn("// additional condition", notice)
            self.assertNotIn("fake literal", notice)
            self.assertNotIn("return 1", notice)
            self.assertEqual(first["files"][source.name], hashlib.sha256(source.read_bytes()).hexdigest())
            self.assertEqual(first["sha256"], write_notices(root, root, project)["sha256"])

    def test_changed_license_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            (root / "COPYING").write_text("changed license")
            (root / "sources.json").write_text(json.dumps({"commit": "pin", "sources": []}))
            (root / "licenses.json").write_text(json.dumps({"commit": "pin", "files": [{"path": "COPYING", "sha256": "0" * 64}]}))
            with self.assertRaisesRegex(RuntimeError, "hash mismatch"):
                write_notices(root, root, root / "Output.csproj")
