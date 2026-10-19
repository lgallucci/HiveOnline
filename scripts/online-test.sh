#!/usr/bin/env bash
# Starts the server in test mode and two multiplayer clients; Ctrl+C stops everything.
set -e
cd "$(dirname "$0")/.."
PORT="${1:-7777}"

dotnet build HiveServer -v q
dotnet build HiveOnline -v q

pids=()
trap 'kill "${pids[@]}" 2>/dev/null || true' EXIT INT TERM

dotnet run --no-build --project HiveServer -- --test "$PORT" &
pids+=($!)
sleep 2
dotnet run --no-build --project HiveOnline -- multiplayer localhost "$PORT" &
pids+=($!)
dotnet run --no-build --project HiveOnline -- multiplayer localhost "$PORT" &
pids+=($!)

wait
