import json
import boto3

# s3_client = boto3.client("s3")
ddb_client = boto3.client("dynamodb")

def lambda_handler(event, context):
    print("event:", event)
    scan = ddb_client.scan(TableName="S3FilesMetadata")
    print("scan result:", scan)
    return {
        "statusCode": 200,
        "body": json.dumps(scan)
    }
