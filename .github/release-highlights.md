<!-- SPDX-License-Identifier: Apache-2.0 OR MIT -->
<!-- SPDX-FileCopyrightText: 2026 Sebastien Rousseau -->
<!--
The hand-written Highlights of the next GitHub release: 2 to 4 bullets,
each "* **Headline**: one or two plain sentences.", no em dashes.
scripts/release-notes.sh puts them under "## Highlights ⭐️" and composes
the rest. Rewrite it in the release preparation of every version.
-->
* **First release on NuGet**: `CryptoService.Sdk` 0.0.40, a typed async client for every v2 operation of crypto-server, with no runtime dependency and no cryptography in the client.
* **The suite's resilience contract**: per-attempt timeouts, an optional deadline, opt-in retries that honour `Retry-After` and never resend a stateful call the server may have acted on, and redirects refused.
* **Proven against the server**: every vector of the conformance kit through the public methods, every example and every README code block run against crypto-server in CI, with 100 % line and branch coverage and two fuzz targets.
