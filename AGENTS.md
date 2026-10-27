# AGENTS.md

Guidance for anyone (human or agent) working on this repository.

## What this project is

`winclean.bat` is a single-file Windows maintenance tool written in batch.
`README.md` is its documentation. There is no build step, no package manager,
no test suite and no dependencies — the deliverable is the batch file itself.

Upstream: https://github.com/adi20110908/winclean, default branch `main`.
Versions before v4.4 live on the `older-versions` branch.

## Release workflow — do this automatically, every time

The owner has explicitly asked that releases be handled **without being asked**.
After any change to the project, run the whole sequence in one go:

1. **Bump the version** everywhere it appears:
   * `winclean.bat` → `set "ver=vX.Y.Z"`
   * `README.md` → `Latest version: **vX.Y.Z**`
   * `README.md` → the `WINCLEAN vX.Y.Z` line inside the copied menu block
   * `README.md` → a new row at the top of the `## Version history` table,
     one sentence describing the change
2. **Commit** with a message that says what changed and ends with the version,
   e.g. `Return to the menu after the rating prompt (v4.4.1)`.
3. **Tag** the release commit with an annotated tag whose name is exactly the
   version string, including the leading `v`:

   ```
   git tag -a vX.Y.Z -m "winclean vX.Y.Z - <one-line summary>"
   ```

4. **Push** the commit and the tag together:

   ```
   git push origin main --follow-tags
   ```

Never leave a change uncommitted, untagged or unpushed, and never wait for the
owner to ask for a push — pushing is the agent's job, not the owner's.

### Version numbering

`v<major>.<minor>.<patch>`:

* **patch** — a bug fix
* **minor** — a new feature or a UI rework
* **major** — an incompatible change in behaviour

The current series is `v4.x.y`. Strings such as `v3.9.8.9` or `v4.3.1.1.1.1b`
are legacy — do not extend that style.

A version may be tagged **only once**. If that version string already exists
as a tag or a release, bump the version instead of moving or re-creating the
tag.

### Making a GitHub Release

A release is built from a tag: the tag is the code snapshot, the release notes
come from the matching row of the `## Version history` table in `README.md`.
Because tags are pushed automatically, the tag for every released version is
already on GitHub — creating the release is then just naming the tag and
pasting the history row.

## Repository rules

* Keep the working tree clean; a session should end with a clean `git status`.
* Before reporting success, check `git status -sb` and `git log --oneline -2`.
* If git reports a corrupt/missing object or an `unborn branch`, fix it rather
  than working around it: `git fsck --full` must exit 0, the default branch
  must point at the real branch (`main`), the clone must have a
  `remote.origin.fetch` refspec (`+refs/heads/*:refs/remotes/origin/*`) and
  the working branch must have an upstream. All four of these were broken
  once in this repo — verify, do not assume.
* Never commit credentials, `.env` files or tokens.

## This checkout (Freebuff worktree on Windows)

* The working branch is `freebuff/<chat-id>`, not `main`. Its upstream is
  `origin/main` and `push.default=upstream`, so a plain `git push` publishes
  to `main` — no `HEAD:main` dance needed.
* `push.followTags=true` is set, so **every push also publishes any new
  annotated tag**. Step 3 of the release workflow is therefore covered even
  if you only remember to run `git push`.
* `origin` is `https://github.com/adi20110908/winclean.git`. Authentication
  comes from the Windows Git Credential Manager; no token belongs in the
  repository, in a config file or in an environment variable.
* Tags already published: `v4.3.1.1.1.1b`, `v4.4.0`, `v4.4.1`.

## File format rules

* `winclean.bat`: **UTF-8 without BOM, CRLF line endings, no double CR**.
  The script runs `chcp 65001` and keeps Hungarian accented comments — do not
  strip them or rewrite them to ASCII.
* `README.md`: UTF-8, CRLF.
* After editing `winclean.bat`, run a static check before calling it done:
  every label defined exactly once, every `goto`/`call` target exists, no
  option-level `goto :EOF` (options hand control back with `goto :menu`), and
  the line endings are still intact.

## Known limitation of this workspace

There is no Windows `cmd` here, so the batch file cannot actually be executed.
Validate changes by static checks and code review, and say plainly in the
final answer that the change has not been run on Windows.

## Owner preferences (explicit)

* Version bump, commit, tag and push are all automatic — do not ask first.
* Keep it simple: one batch file organised around shared subroutines, no
  external tooling, no build system.
* Documentation must match the code: every claim in `README.md` should be
  something the batch file really does.
