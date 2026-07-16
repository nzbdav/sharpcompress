#!/usr/bin/env bash
# Enforce sync/async partial-class pair drift policy (see docs/ARCHITECTURE.md).
set -euo pipefail

BASE_SHA="${BASE_SHA:-}"
if [[ -z "$BASE_SHA" ]]; then
    echo "::error::BASE_SHA environment variable is required"
    exit 1
fi

if [[ "${PR_BODY:-}" == *"[async-pair-skip]"* ]]; then
    echo "Skipping async pair check: [async-pair-skip] found in PR body"
    exit 0
fi

if git log --format=%B "${BASE_SHA}..HEAD" 2>/dev/null | grep -qF '[async-pair-skip]'; then
    echo "Skipping async pair check: [async-pair-skip] found in commit messages"
    exit 0
fi

changed_files=()
while IFS= read -r line; do
    changed_files+=("$line")
done < <(git diff --name-only --diff-filter=ACMR "${BASE_SHA}...HEAD" -- 'src/' | grep '\.cs$' || true)

if [[ ${#changed_files[@]} -eq 0 ]]; then
    exit 0
fi

is_changed() {
    local target="$1"
    local file
    for file in "${changed_files[@]}"; do
        if [[ "$file" == "$target" ]]; then
            return 0
        fi
    done
    return 1
}

# Intentionally async-only partials with no sync counterpart.
ALLOWLIST=(
    "src/SharpCompress/Compressors/LZMA/DecoderRegistry.Async.cs"
)

is_allowlisted() {
    local file="$1"
    for allowed in "${ALLOWLIST[@]}"; do
        if [[ "$file" == "$allowed" ]]; then
            return 0
        fi
    done
    return 1
}

# X.Async.cs pairs with X.cs in the same directory (not Foo.Bar.cs style).
is_pairable_sync() {
    local basename="${1##*/}"
    [[ "$basename" == *.cs ]] || return 1
    [[ "$basename" == *.Async.cs ]] && return 1
    local name="${basename%.cs}"
    [[ "$name" != *.* ]] || return 1
    return 0
}

is_pairable_async() {
    local basename="${1##*/}"
    [[ "$basename" == *.Async.cs ]] || return 1
    local name="${basename%.Async.cs}"
    [[ "$name" != *.* ]] || return 1
    return 0
}

get_async_sibling() {
    local sync_file="$1"
    local dir="${sync_file%/*}"
    local name="${sync_file##*/}"
    name="${name%.cs}"
    echo "${dir}/${name}.Async.cs"
}

get_sync_sibling() {
    local async_file="$1"
    local dir="${async_file%/*}"
    local name="${async_file##*/}"
    name="${name%.Async.cs}"
    echo "${dir}/${name}.cs"
}

violations=0

for file in "${changed_files[@]}"; do
    if is_allowlisted "$file"; then
        continue
    fi

    if is_pairable_async "$file"; then
        sync_sibling=$(get_sync_sibling "$file")
        if [[ -f "$sync_sibling" ]] && ! is_changed "$sync_sibling"; then
            echo "::error file=${file}::Async partial changed without paired sync file (${sync_sibling}). Touch both files or add [async-pair-skip] to the PR body."
            violations=$((violations + 1))
        fi
    elif is_pairable_sync "$file"; then
        async_sibling=$(get_async_sibling "$file")
        if [[ -f "$async_sibling" ]] && ! is_changed "$async_sibling"; then
            echo "::error file=${file}::Sync partial changed without paired async file (${async_sibling}). Touch both files or add [async-pair-skip] to the PR body."
            violations=$((violations + 1))
        fi
    fi
done

if [[ $violations -gt 0 ]]; then
    echo "Found ${violations} async pair drift violation(s)"
    exit 1
fi

exit 0
