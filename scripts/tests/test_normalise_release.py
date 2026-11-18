# SPDX-License-Identifier: Apache-2.0 OR MIT
# SPDX-FileCopyrightText: 2026 Sebastien Rousseau
"""Tests for scripts/normalise-release.py (standard library only).

    python -m unittest discover -s scripts/tests -v
"""

from __future__ import annotations

import importlib.util
import io
import json
import re
import tempfile
import unittest
import zipfile
from pathlib import Path

SCRIPT = Path(__file__).resolve().parents[1] / "normalise-release.py"
SPEC = importlib.util.spec_from_file_location("normalise_release", SCRIPT)
assert SPEC is not None and SPEC.loader is not None
normalise = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(normalise)

EPOCH = 1_760_000_000
UUID_URN = re.compile(
    r"^urn:uuid:[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"
)


def make_zip(date_time: tuple[int, int, int, int, int, int]) -> bytes:
    """A small zip whose entries all carry the given time."""
    buffer = io.BytesIO()
    with zipfile.ZipFile(buffer, "w", zipfile.ZIP_DEFLATED) as archive:
        for name, body in (("a.txt", b"alpha" * 50), ("dir/b.txt", b"beta" * 50)):
            info = zipfile.ZipInfo(name, date_time=date_time)
            info.compress_type = zipfile.ZIP_DEFLATED
            archive.writestr(info, body)
    return buffer.getvalue()


def stamped(raw: bytes, epoch: int = EPOCH) -> bytes:
    data = bytearray(raw)
    normalise.stamp_zip(data, epoch)
    return bytes(data)


class StampZipTests(unittest.TestCase):
    def test_zips_built_at_different_times_become_identical(self) -> None:
        first = make_zip((2020, 1, 2, 3, 4, 6))
        second = make_zip((2024, 11, 30, 23, 59, 58))
        self.assertNotEqual(first, second)
        self.assertEqual(stamped(first), stamped(second))

    def test_result_is_still_a_valid_archive(self) -> None:
        result = stamped(make_zip((2020, 1, 2, 3, 4, 6)))
        with zipfile.ZipFile(io.BytesIO(result)) as archive:
            self.assertIsNone(archive.testzip())
            self.assertEqual(archive.read("a.txt"), b"alpha" * 50)
            self.assertEqual(archive.read("dir/b.txt"), b"beta" * 50)
            expected = (2025, 10, 9, 8, 53, 20)
            self.assertEqual(
                [info.date_time for info in archive.infolist()], [expected] * 2
            )

    def test_stamping_twice_changes_nothing(self) -> None:
        once = stamped(make_zip((2020, 1, 2, 3, 4, 6)))
        self.assertEqual(stamped(once), once)

    def test_not_a_zip_is_rejected(self) -> None:
        with self.assertRaisesRegex(ValueError, "not a zip archive"):
            normalise.stamp_zip(bytearray(b"plain text"), EPOCH)

    def test_dos_time_clamps_to_1980(self) -> None:
        dos_t, dos_d = normalise.dos_time(0)
        self.assertEqual(dos_t, 0)
        self.assertEqual(dos_d, (0 << 9) | (1 << 5) | 1)


class PinSbomTests(unittest.TestCase):
    BOM = {
        "bomFormat": "CycloneDX",
        "specVersion": "1.6",
        "serialNumber": "urn:uuid:00000000-0000-0000-0000-000000000000",
        "metadata": {"timestamp": "2020-01-01T00:00:00Z"},
        "components": [{"name": "x"}],
    }

    def test_same_content_gives_the_same_serial(self) -> None:
        other = dict(self.BOM, serialNumber="urn:uuid:11111111-1111-1111-1111-111111111111")
        other["metadata"] = {"timestamp": "2031-05-05T05:05:05Z"}
        self.assertEqual(
            normalise.pin_sbom(self.BOM, EPOCH), normalise.pin_sbom(other, EPOCH)
        )

    def test_different_content_gives_a_different_serial(self) -> None:
        changed = dict(self.BOM, components=[{"name": "y"}])
        self.assertNotEqual(
            normalise.pin_sbom(self.BOM, EPOCH)["serialNumber"],
            normalise.pin_sbom(changed, EPOCH)["serialNumber"],
        )

    def test_serial_matches_the_cyclonedx_pattern(self) -> None:
        serial = normalise.pin_sbom(self.BOM, EPOCH)["serialNumber"]
        self.assertRegex(serial, UUID_URN)

    def test_timestamp_is_the_given_instant(self) -> None:
        pinned = normalise.pin_sbom(self.BOM, EPOCH)
        self.assertEqual(pinned["metadata"]["timestamp"], "2025-10-09T08:53:20Z")

    def test_is_idempotent_and_keeps_serial_after_spec_version(self) -> None:
        once = normalise.pin_sbom(self.BOM, EPOCH)
        self.assertEqual(normalise.pin_sbom(once, EPOCH), once)
        keys = list(once)
        self.assertEqual(keys.index("serialNumber"), keys.index("specVersion") + 1)

    def test_serial_added_when_there_is_no_spec_version(self) -> None:
        pinned = normalise.pin_sbom({"metadata": {}}, EPOCH)
        self.assertRegex(pinned["serialNumber"], UUID_URN)

    def test_input_is_not_mutated(self) -> None:
        before = json.dumps(self.BOM, sort_keys=True)
        normalise.pin_sbom(self.BOM, EPOCH)
        self.assertEqual(json.dumps(self.BOM, sort_keys=True), before)


class MainTests(unittest.TestCase):
    def run_main(self, *args: str) -> int:
        return normalise.main(["normalise-release.py", *args])

    def test_bad_usage_returns_2(self) -> None:
        self.assertEqual(self.run_main("only-one"), 2)
        self.assertEqual(self.run_main("dir", "not-a-number"), 2)

    def test_directory_is_normalised_in_place_and_stable(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            (root / "pkg.nupkg").write_bytes(make_zip((2020, 1, 2, 3, 4, 6)))
            (root / "pkg.snupkg").write_bytes(make_zip((2024, 6, 7, 8, 9, 10)))
            (root / "sbom.cdx.json").write_text(
                json.dumps(PinSbomTests.BOM), encoding="utf-8"
            )
            self.assertEqual(self.run_main(str(root), str(EPOCH)), 0)
            snapshot = {p.name: p.read_bytes() for p in root.iterdir()}
            self.assertEqual(snapshot["pkg.nupkg"], snapshot["pkg.snupkg"])
            self.assertEqual(self.run_main(str(root), str(EPOCH)), 0)
            self.assertEqual(snapshot, {p.name: p.read_bytes() for p in root.iterdir()})


if __name__ == "__main__":
    unittest.main()
