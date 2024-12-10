#!/bin/bash
# took it from here:
# https://stackoverflow.com/questions/69813206/how-to-empty-s3-bucket-using-aws-cli

bucket_name="$1"

if [ "$bucket_name" = "" ]; then
    echo "please specify a bucket name"
    exit 1
fi

output=$(aws s3api list-object-versions \
  --bucket "${bucket_name}" \
  --output=json \
  --query='{Objects: Versions[].{Key:Key,VersionId:VersionId}}')

aws s3api delete-objects \
  --bucket ${bucket_name} \
  --delete "$output"
