#!/usr/bin/env bash
# Builds the HiveServer image with Cloud Build and runs it on a small Compute Engine VM
# with a static public IP and a firewall rule for the game port.
# Usage: PROJECT=my-gcp-project ./scripts/deploy-gcp.sh   (re-run to deploy a new version)
set -euo pipefail

PROJECT="${PROJECT:?Set PROJECT to your GCP project id}"
REGION="${REGION:-us-central1}"
ZONE="${ZONE:-us-central1-a}"
PORT="${PORT:-7777}"
VM="${VM:-hive-server}"
REPO="${REPO:-hive}"
IMAGE="$REGION-docker.pkg.dev/$PROJECT/$REPO/hiveserver:latest"

cd "$(dirname "$0")/.."
gcloud config set project "$PROJECT" >/dev/null

gcloud services enable compute.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com

gcloud artifacts repositories describe "$REPO" --location "$REGION" >/dev/null 2>&1 ||
  gcloud artifacts repositories create "$REPO" --repository-format=docker --location "$REGION"

gcloud builds submit --tag "$IMAGE" .

gcloud compute firewall-rules describe allow-hive >/dev/null 2>&1 ||
  gcloud compute firewall-rules create allow-hive --allow "tcp:$PORT" --target-tags hive-server

gcloud compute addresses describe "$VM-ip" --region "$REGION" >/dev/null 2>&1 ||
  gcloud compute addresses create "$VM-ip" --region "$REGION"
IP=$(gcloud compute addresses describe "$VM-ip" --region "$REGION" --format='value(address)')

if gcloud compute instances describe "$VM" --zone "$ZONE" >/dev/null 2>&1; then
  gcloud compute instances update-container "$VM" --zone "$ZONE" --container-image "$IMAGE"
else
  gcloud compute instances create-with-container "$VM" --zone "$ZONE" \
    --machine-type e2-small --address "$IP" --tags hive-server \
    --container-image "$IMAGE" --container-env "HIVE_PORT=$PORT" \
    --container-restart-policy always \
    --scopes cloud-platform
fi

echo "Server running at $IP:$PORT"
echo "Run the client with: HiveOnline $IP $PORT"
