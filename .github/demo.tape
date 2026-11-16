# SPDX-License-Identifier: Apache-2.0 OR MIT
#
# Rendered by `make demo` (vhs, https://github.com/charmbracelet/vhs) from
# the repository root, after `make build`, with CRYPTO_SERVICE_URL and
# CRYPTO_SERVICE_API_KEY naming a running crypto-server on loopback (see
# DEVELOPMENT.md). Hidden: copy the built Hash and SignVerify examples
# into a throwaway directory. On screen: the Hash example checking a
# SHA-256 digest against its test vector, then SignVerify generating an
# Ed25519 key on the server, signing by keyId, verifying, and deleting
# the key. Nothing leaves the loopback interface, and no credential is
# shown.
Output .github/demo.gif

Set Shell "bash"
Set FontSize 14
Set Width 1200
Set Height 520
Set Padding 20
Set Theme "Catppuccin Mocha"

Hide
Type `R="$PWD" && cd "$(mktemp -d /tmp/crypto-demo.XXXX)"`
Enter
Type `cp -R "$R/examples/Hash/bin/Release/net10.0" hash && cp -R "$R/examples/SignVerify/bin/Release/net10.0" sign`
Enter
Type `export PS1='$ ' && clear`
Enter
Sleep 1s
Show

Type "hash/Hash"
Sleep 500ms
Enter
Sleep 4s

Type "sign/SignVerify"
Sleep 500ms
Enter
Sleep 6s
