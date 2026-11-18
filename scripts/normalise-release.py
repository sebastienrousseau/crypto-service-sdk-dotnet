# SPDX-License-Identifier: Apache-2.0 OR MIT
# SPDX-FileCopyrightText: 2026 Sebastien Rousseau
"""Make the release assets in a directory reproducible, in place.

`dotnet pack` writes every byte of a package deterministically except
the zip entries' modification times, which are the time of the pack;
the CycloneDX tool writes a random serial number and the current time.
This script sets every zip entry time of each `.nupkg` and `.snupkg` to
the given instant (the released commit's time), leaving the compressed
data untouched, and gives each `.cdx.json` that instant as its
timestamp and a serial number derived from its content (a UUIDv5 of
its SHA-256). Run twice, it changes nothing the second time.

    python3 scripts/normalise-release.py DIST_DIR SOURCE_DATE_EPOCH

docs/REPRODUCING.md shows how a release is rebuilt and compared.
"""

from __future__ import annotations

import hashlib
import json
import struct
import sys
import time
import uuid
from pathlib import Path

LOCAL_HEADER = b"PK\x03\x04"
CENTRAL_HEADER = b"PK\x01\x02"
END_OF_CENTRAL = b"PK\x05\x06"


def dos_time(epoch: int) -> tuple[int, int]:
    """The (time, date) DOS fields of a UTC instant, 2-second resolution."""
    t = time.gmtime(max(epoch, 315532800))  # DOS dates start in 1980
    dos_t = (t.tm_hour << 11) | (t.tm_min << 5) | (t.tm_sec // 2)
    dos_d = ((t.tm_year - 1980) << 9) | (t.tm_mon << 5) | t.tm_mday
    return dos_t, dos_d


def stamp_zip(data: bytearray, epoch: int) -> None:
    """Set the time of every entry, in the central directory and in each local header."""
    end = data.rfind(END_OF_CENTRAL)
    if end < 0:
        raise ValueError("not a zip archive")
    count, _, offset = struct.unpack_from("<HII", data, end + 10)
    fields = struct.pack("<HH", *dos_time(epoch))
    for _ in range(count):
        if data[offset : offset + 4] != CENTRAL_HEADER:
            raise ValueError("malformed central directory")
        data[offset + 12 : offset + 16] = fields
        name_len, extra_len, comment_len = struct.unpack_from("<HHH", data, offset + 28)
        (local,) = struct.unpack_from("<I", data, offset + 42)
        if data[local : local + 4] != LOCAL_HEADER:
            raise ValueError("malformed local header")
        data[local + 10 : local + 14] = fields
        offset += 46 + name_len + extra_len + comment_len


def pin_sbom(bom: dict, epoch: int) -> dict:
    """The BOM with a fixed timestamp and a serial number derived from its content."""
    bom = dict(bom)
    bom.pop("serialNumber", None)
    bom["metadata"] = dict(bom.get("metadata", {}))
    bom["metadata"]["timestamp"] = time.strftime(
        "%Y-%m-%dT%H:%M:%SZ", time.gmtime(epoch)
    )
    canonical = json.dumps(
        bom, sort_keys=True, separators=(",", ":"), ensure_ascii=False
    )
    digest = hashlib.sha256(canonical.encode("utf-8")).hexdigest()
    serial = f"urn:uuid:{uuid.uuid5(uuid.NAMESPACE_URL, 'sha256:' + digest)}"
    ordered: dict = {}
    for key, value in bom.items():
        ordered[key] = value
        if key == "specVersion":
            ordered["serialNumber"] = serial
    ordered.setdefault("serialNumber", serial)
    return ordered


def main(argv: list[str]) -> int:
    if len(argv) != 3 or not argv[2].isdigit():
        print("usage: normalise-release.py DIST_DIR SOURCE_DATE_EPOCH", file=sys.stderr)
        return 2
    dist, epoch = Path(argv[1]), int(argv[2])
    for path in sorted(dist.glob("*.nupkg")) + sorted(dist.glob("*.snupkg")):
        data = bytearray(path.read_bytes())
        stamp_zip(data, epoch)
        path.write_bytes(bytes(data))
        print(f"normalise-release: {path.name}: entry times set")
    for path in sorted(dist.glob("*.cdx.json")):
        bom = pin_sbom(json.loads(path.read_text(encoding="utf-8")), epoch)
        path.write_text(
            json.dumps(bom, indent=2, ensure_ascii=False) + "\n", encoding="utf-8"
        )
        print(f"normalise-release: {path.name}: {bom['serialNumber']}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
