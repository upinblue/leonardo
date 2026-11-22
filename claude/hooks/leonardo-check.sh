#!/usr/bin/env bash
# PostToolUse hook of the plugin leonardo.
#
# Claude Code passes the event as JSON on standard input. When the edited file is UI code, this script hands the event
# to the check tool (leonardo-check --hook). The tool prints its findings to standard error and exits with 2, which
# Claude Code shows to Claude. Without the tool, or for other files, the script exits with 0 and stays silent.
#
# Where the tool comes from, in this order:
#   1. LEONARDO_CHECK: a path to UpInBlue.Leonardo.Check.dll or to an executable.
#   2. leonardo-check on the PATH (installed as a .NET tool, see claude/README.md).
#   3. In a clone of upinblue/leonardo: the tool built from src/UpInBlue.Leonardo.Check.
# If none of them is found, the script prints a one-line warning to standard error and exits with 0: the hook never
# blocks Claude Code.
#
# Assumes a POSIX shell with grep. On Windows, Claude Code runs hooks with Git Bash from Git for Windows.

input="$(cat)"

# Only UI files. The check tool decides about generated and test folders.
if ! printf '%s' "$input" | grep -Eq '"file_path"[[:space:]]*:[[:space:]]*"[^"]*\.(xaml|cs|swift|css|scss|html|htm|tsx)"'; then
    exit 0
fi

run() {
    printf '%s' "$input" | "$@" --hook
    exit $?
}

if [ -n "${LEONARDO_CHECK:-}" ]; then
    case "$LEONARDO_CHECK" in
        *.dll) run dotnet "$LEONARDO_CHECK" ;;
        *) run "$LEONARDO_CHECK" ;;
    esac
fi

if command -v leonardo-check >/dev/null 2>&1; then
    run leonardo-check
fi

project="${CLAUDE_PROJECT_DIR:-$PWD}"
for configuration in Debug Release; do
    dll="$project/src/UpInBlue.Leonardo.Check/bin/$configuration/net10.0/UpInBlue.Leonardo.Check.dll"
    if [ -f "$dll" ]; then
        run dotnet "$dll"
    fi
done

echo "Leonardo check skipped: leonardo-check is not on the PATH (install it with dotnet tool install --global UpInBlue.Leonardo.Check, see the plugin README)." >&2
exit 0
